using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

[Serializable]
public class LottoJson
{
    public List<LottoResponse> liLottoInfo = new List<LottoResponse>();
}

[Serializable]
public class LottoResponse
{
    public long totSellamnt;
    public long firstWinamnt;
    public string returnValue;
    public string drwNoDate;
    public int drwtNo1;
    public int drwtNo2;
    public int drwtNo3;
    public int drwtNo4;
    public int drwtNo5;
    public int drwtNo6;
    public int bnusNo;
    public int drwNo;
}

[Serializable]
public class NumberInfo
{
    public List<ButtonEx> liNumberButton;
}

public class Lotto : MonoBehaviour
{
    const string Image_Path = "Roulette";
    const string Local_LottoDataFile = "lotto.json";
    const int Fail_Rank = 6;

    [SerializeField] private UILotto _uiLotto;

    [Header("로딩")]
    [SerializeField] private GameObject _loadingPageObject;
    [SerializeField] private Image _loadingImage;
    [SerializeField] private TMP_Text _loadingText;

    [Header("로또 정보")]
    [SerializeField] private Image _rouletteImage;
    [SerializeField] private Button _rouletteButton;
    [SerializeField] private Button _customImageButton;
    [SerializeField] private TMP_Text _saveLottoRoundText;
    [SerializeField] private Button _lottoInfoUpdateButton;

    [Header("당첨 정보")]
    [SerializeField] private NumberInfo _lotto1Info;
    [SerializeField] private NumberInfo _lotto2Info;
    [SerializeField] private NumberInfo _lotto3Info;
    [SerializeField] private NumberInfo _lotto4Info;
    [SerializeField] private NumberInfo _lotto5Info;

    string _lottoURL = "www.dhlottery.co.kr/common.do?method=getLottoNumber&drwNo=";
    List<LottoResponse> _lilottoResponse = new List<LottoResponse>();

    public List<LottoResponse> LottoResponseList => _lilottoResponse;

    private void Start()
    {
        Loading(false);

        //# 버튼 바인딩
        _rouletteButton.onClick.AddListener(() =>
        {
            StartRoulette(_lotto1Info);
            StartRoulette(_lotto2Info);
            StartRoulette(_lotto3Info);
            StartRoulette(_lotto4Info);
            StartRoulette(_lotto5Info);
            _uiLotto.View();
        });
        _customImageButton.onClick.AddListener(PickImage);
        _lottoInfoUpdateButton.onClick.AddListener(() => Loading(true));

        //# 룰렛 이미지 설정
        //# 사용자가 커스텀한 룰렛 이미지가 있으면 바로 설정
        var path = PlayerPrefs.GetString(Image_Path, string.Empty);
        if (path != string.Empty)
        {
            StartCoroutine(LoadImage(path));
        }
    }

    private async void Loading(bool update)
    {
        _loadingPageObject.SetActive(true);
        var loadingTween = _loadingImage.rectTransform
            .DORotate(new Vector3(0, 0, 360), .2f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1);

        await GetLottoInfo(update);

        loadingTween.Kill();
        _loadingPageObject.SetActive(false);
    }

    #region Gallery
    private void PickImage()
    {
#if PLATFORM_ANDROID
        if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) == false)
        {
            PermissionCallbacks callback = new PermissionCallbacks();
            callback.PermissionGranted += msg => {
                Debug.Log($"{msg} 승인");

                NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) => {
                    if (path != null)
                    {
                        PlayerPrefs.SetString(Image_Path, path);
                        StartCoroutine(LoadImage(path));
                    }
                }, "Select an image", "image/*");
            };

            callback.PermissionDenied += msg => {
                Debug.Log($"{msg} 거절");
            };

            Permission.RequestUserPermission(Permission.ExternalStorageRead, callback);
        }
        else
        {
            NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) => {
                if (path != null)
                {
                    PlayerPrefs.SetString(Image_Path, path);
                    StartCoroutine(LoadImage(path));
                }
            }, "Select an image", "image/*");
        }
#endif
    }

    private IEnumerator LoadImage(string path)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                _loadingImage.sprite = sprite;
                _rouletteImage.sprite = sprite;
            }
        }
    }
    #endregion Gallery

    #region Lotto
    private async Task GetLottoInfo(bool update = false)
    {
        string localLottoPath = $"{Application.persistentDataPath}/{Local_LottoDataFile}";

        Debug.Log(localLottoPath);

        if (update == false && File.Exists(localLottoPath))
        {
            LottoJson json = JsonUtility.FromJson<LottoJson>(File.ReadAllText(localLottoPath));
            _lilottoResponse = json.liLottoInfo;
            _saveLottoRoundText.text = $"최근 회차 : {_lilottoResponse.Count}회차";
        }
        else
        {
            for (int i = _lilottoResponse.Count + 1; i < 9999; i++)
            {
                if (await GetWebLottoNumbers(i) == false)
                {
                    _saveLottoRoundText.text = $"최근 회차 : {_lilottoResponse.Count}회차";
                    break;
                }
            }

            LottoJson lottoJson = new LottoJson();
            lottoJson.liLottoInfo = _lilottoResponse;

            var json = JsonUtility.ToJson(lottoJson, true);
            File.WriteAllText(localLottoPath, json);
        }
    }

    private void StartRoulette(NumberInfo lottoInfo)
    {
        _rouletteButton.interactable = false;

        List<int> liMyNumber = new List<int>(6);

        int num1 = RandomLottoNumber(liMyNumber);
        liMyNumber.Add(num1);

        int num2 = RandomLottoNumber(liMyNumber);
        liMyNumber.Add(num2);

        int num3 = RandomLottoNumber(liMyNumber);
        liMyNumber.Add(num3);

        int num4 = RandomLottoNumber(liMyNumber);
        liMyNumber.Add(num4);

        int num5 = RandomLottoNumber(liMyNumber);
        liMyNumber.Add(num5);

        int num6 = RandomLottoNumber(liMyNumber);
        liMyNumber.Add(num6);

        liMyNumber.Sort();

        int numBounus = RandomLottoNumber(liMyNumber);
        liMyNumber.Add(numBounus);

        _rouletteImage.rectTransform
            .DORotate(new Vector3(0, 0, 360), .2f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(5)
            .OnComplete(() =>
            {
                _rouletteButton.interactable = true;

                for (int i = 0; i < liMyNumber.Count; i++)
                {
                    lottoInfo.liNumberButton[i].SetText($"{liMyNumber[i]}");
                }
            });
    }

    private int RandomLottoNumber(List<int> liExcludeNumber)
    {
        int num = 0;

        do
        {
            num = UnityEngine.Random.Range(1, 46);
        } while (liExcludeNumber.Contains(num));

        return num;
    }

    private async Task<bool> GetWebLottoNumbers(int round)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get($"{_lottoURL}{round}"))
        {
            await webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
                return false;
            }
            else
            {
                string json = webRequest.downloadHandler.text;
                LottoResponse lottoResponse = JsonUtility.FromJson<LottoResponse>(json);
                if (lottoResponse == null ||
                    lottoResponse.returnValue != "success")
                    return false;

                _lilottoResponse.Add(lottoResponse);
                _loadingText.text = $"{round}회차 정보 저장...";
                return true;
            }
        }
    }

    private (int, LottoResponse) CheckFirstResult(List<int> liMyNumber)
    {
        if (liMyNumber == null || liMyNumber.Count == 0)
            return (Fail_Rank, null);

        LottoResponse result = new LottoResponse();
        int maxRank = Fail_Rank;
        int matchCount = 0;

        foreach (var lotto in _lilottoResponse)
        {
            matchCount = 0;

            if (liMyNumber.Contains(lotto.drwtNo1))
                ++matchCount;

            if (liMyNumber.Contains(lotto.drwtNo2))
                ++matchCount;

            if (liMyNumber.Contains(lotto.drwtNo3))
                ++matchCount;

            if (liMyNumber.Contains(lotto.drwtNo4))
                ++matchCount;

            if (liMyNumber.Contains(lotto.drwtNo5))
                ++matchCount;

            if (liMyNumber.Contains(lotto.drwtNo6))
                ++matchCount;

            switch (matchCount)
            {
                case 6:
                    {
                        maxRank = 1;
                        result = lotto;
                    }
                    break;
            }
        }

        return (maxRank, result);
    }
    #endregion Lotto
}

