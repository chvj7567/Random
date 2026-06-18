using ChvjUnityInfra;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

//# 동전 면 — 단일 시스템 내부 enum 이라 CommonEnum 이 아닌 이 파일에 둔다 (Rule 02 §8).
public enum ECoinFace
{
    Head,
    Tail,
}

public class CoinFlipScene : MonoBehaviour, IRouletteBackButton
{
    [SerializeField] private CHButton _menuButton;
    [SerializeField] private CHButton _throwButton;
    [SerializeField] private Image _coinImage;
    [SerializeField] private CHText _resultText;

    //# 토글에서 고른 횟수 상시 표시 (토글 위). 토글 변경 시 Update 에서 실시간 갱신.
    [SerializeField] private CHText _selectedCountText;

    //# 다회 던지기 진행 표시 (코인 아래, "현재 / 전체"). 단일 던지기·연출 종료 시 비움.
    [SerializeField] private CHText _progressText;

    //# 연속 횟수 토글 그룹 (1 / 3 / 5 / 10). 인덱스 = _countOptions 와 1:1.
    [SerializeField] private List<CHToggle> _countToggles = new List<CHToggle>();

    //# 회전 연출 기준 트랜스폼 (보통 _coinImage 의 RectTransform). _tossFrames 비었을 때 DOTween 폴백용.
    [SerializeField] private RectTransform _coinTransform;

    //# 토스 시퀀스 프레임 (CoinToss_00~23). 와이어링되면 주 연출 경로, 비어있으면 DOTween 회전 폴백.
    [SerializeField] private Sprite[] _tossFrames = new Sprite[0];

    //# 앞면 골드 착지 이미지 — 토스 애니메이션 종료 후 결과 스프라이트로 교체 (틴트 아님).
    [SerializeField] private Sprite _headResultSprite;

    //# 뒷면 실버 착지 이미지.
    [SerializeField] private Sprite _tailResultSprite;

    //# 던지기 연출 파라미터 (기획서 §3).
    private const float FlipDuration = 0.9f;
    private const float SequentialInterval = 0.25f;

    //# 표시 문자열 stringID (String.json)
    private const int StringIdHead = 142;
    private const int StringIdTail = 143;
    private const int StringIdHeadCount = 144;
    private const int StringIdTailCount = 145;
    private const int StringIdSelectedCount = 148;
    private const int StringIdProgress = 149;

    //# _countToggles 인덱스 → 실제 연속 횟수.
    private static readonly int[] _countOptions = { 1, 3, 5, 10 };

    //# 면별 틴트 색 — 단일 Coin 스프라이트(흰색 실루엣)에 색을 입혀 앞/뒷면 구분.
    private static readonly Color HeadColor = new Color(1f, 0.84f, 0.30f);
    private static readonly Color TailColor = new Color(0.78f, 0.80f, 0.85f);

    private IRouletteSceneAccess _rouletteSceneAccess;

    private Sprite _coinSprite;

    private Tween _flipTween;
    private Coroutine _throwRoutine;
    private bool _spriteLoaded;

    //# 선택 횟수 표시 캐시 — 직전 표시값과 다를 때만 SetText (불필요한 갱신 회피).
    private int _shownSelectedCount = -1;

    private async void Start()
    {
        _menuButton.OnClick(() =>
        {
            Close();
        });

        _throwButton.OnClick(() =>
        {
            OnClickThrow();
        });

        await LoadCoinSprite();
    }

    private void OnEnable()
    {
        //# 재진입 시 이전 결과/연출 잔존 방지 (기획서 §5.2).
        //# CHToggle.toggle 은 자식 Start 에서 채워지므로 OnEnable 시점엔 접근 금지 — visual 리셋은 null 가드.
        ResetToggles();

        _resultText.SetText(string.Empty);
        _throwButton.Interactable = true;

        //# 진행 표시 비우기 + 선택 횟수 캐시 리셋 (다음 Update 에서 재반영).
        if (_progressText != null)
        {
            _progressText.SetText(string.Empty);
        }

        _shownSelectedCount = -1;

        //# 기본면 표시 — 결과 라벨은 비운 상태 유지 (기획서 §5.2).
        if (_tossFrames.Length > 0)
        {
            ShowDefaultResult();
        }
        else if (_spriteLoaded)
        {
            SetCoinSprite(ECoinFace.Head);
        }
    }

    private void Update()
    {
        //# 토글 선택 횟수 실시간 갱신 — 변경됐을 때만 SetText (GetSelectedCount 는 GetComponent 미사용).
        if (_selectedCountText == null)
            return;

        int current = GetSelectedCount();
        if (current == _shownSelectedCount)
            return;

        _shownSelectedCount = current;
        _selectedCountText.SetText(string.Format(JsonManager.Instance.GetStringData(StringIdSelectedCount), current));
    }

    //# 진입 기본 표시 — 앞면 골드 착지 이미지 + 자연색. 미와이어링이면 첫 토스 프레임 폴백.
    private void ShowDefaultResult()
    {
        if (_coinImage == null)
            return;

        _coinImage.color = Color.white;

        Sprite defaultSprite = _headResultSprite != null ? _headResultSprite : _tossFrames[0];
        if (defaultSprite != null)
        {
            _coinImage.sprite = defaultSprite;
        }
    }

    private void OnDisable()
    {
        //# 진행 중 이탈 시 Tween/Coroutine 누수 방지 (기획서 §8).
        if (_flipTween != null)
        {
            _flipTween.Kill();
        }

        StopAllCoroutines();
        _throwRoutine = null;
    }

    private void OnClickThrow()
    {
        //# 연출 진행 중 재탭 무시 (입력 잠금).
        if (_throwRoutine != null)
            return;

        int count = GetSelectedCount();
        List<ECoinFace> results = FlipMany(count);

        _throwRoutine = StartCoroutine(PlayThrowSequence(results));
    }

    private IEnumerator PlayThrowSequence(List<ECoinFace> results)
    {
        _throwButton.Interactable = false;

        int total = results.Count;
        bool multi = total > 1;

        //# 다회 던지기는 결과 영역에 회차 라벨을 안 띄우므로 직전 요약 잔존을 시작 시 제거 (요구 #3).
        if (multi == true)
        {
            _resultText.SetText(string.Empty);
        }

        for (int i = 0; i < total; i++)
        {
            //# 다회일 때만 "현재 / 전체" 진행 표시.
            if (multi == true)
            {
                if (_progressText != null)
                {
                    _progressText.SetText(string.Format(JsonManager.Instance.GetStringData(StringIdProgress), i + 1, total));
                }
            }

            //# 단일 던지기일 때만 회차 결과 라벨을 표시.
            yield return PlayFlipAnimation(results[i], multi == false);

            if (i < total - 1)
            {
                yield return new WaitForSeconds(SequentialInterval);
            }
        }

        //# 연출 종료 — 진행 표시 비우고 요약(앞/뒤 카운트)으로 전환.
        if (_progressText != null)
        {
            _progressText.SetText(string.Empty);
        }

        ShowSummary(results);

        _throwButton.Interactable = true;
        _throwRoutine = null;
    }

    private IEnumerator PlayFlipAnimation(ECoinFace face, bool showLabel)
    {
        if (_tossFrames.Length > 0)
        {
            yield return PlayTossFrames();
        }
        else
        {
            yield return PlayFlipTween();
        }

        ShowFace(face, showLabel);
    }

    //# 24프레임 토스 시퀀스 재생 — 프레임 아트에 상하 이동·회전이 들어있어 트랜스폼 조작 불필요.
    private IEnumerator PlayTossFrames()
    {
        if (_coinImage != null)
        {
            //# 프레임 자연색 그대로 표시.
            _coinImage.color = Color.white;
        }

        float elapsed = 0f;
        while (elapsed < FlipDuration)
        {
            int idx = Mathf.Clamp((int)(elapsed / FlipDuration * _tossFrames.Length), 0, _tossFrames.Length - 1);
            if (_tossFrames[idx] != null && _coinImage != null)
            {
                _coinImage.sprite = _tossFrames[idx];
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        //# 마지막 프레임으로 고정 (착지 자세).
        Sprite lastFrame = _tossFrames[_tossFrames.Length - 1];
        if (lastFrame != null && _coinImage != null)
        {
            _coinImage.sprite = lastFrame;
        }
    }

    //# DOTween Y축 회전 폴백 — _tossFrames 미와이어링 시 (하위호환, 기획서 §3-4).
    private IEnumerator PlayFlipTween()
    {
        if (_coinTransform != null)
        {
            if (_flipTween != null)
            {
                _flipTween.Kill();
            }

            //# 동전 플립 — Y축 다회전 후 감속. 결과면은 이미 확정, 연출은 표현만.
            _coinTransform.localEulerAngles = Vector3.zero;
            _flipTween = _coinTransform
                .DOLocalRotate(new Vector3(0f, 360f * 3f, 0f), FlipDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic);

            yield return _flipTween.WaitForCompletion();

            _coinTransform.localEulerAngles = Vector3.zero;
        }
        else
        {
            yield return new WaitForSeconds(FlipDuration);
        }
    }

    //# 단일 스프라이트 고정 + 면별 색 틴트로 앞/뒷면 구분 (결과 라벨은 건드리지 않음). 진입 시 기본면 표시용.
    private void SetCoinSprite(ECoinFace face)
    {
        if (_coinImage == null)
            return;

        _coinImage.sprite = _coinSprite;
        _coinImage.color = face == ECoinFace.Head ? HeadColor : TailColor;
    }

    //# 착지 결과 표시 — 프레임 경로는 자연색 + 결과 스프라이트 교체(앞=골드/뒤=실버), 폴백은 정적 스프라이트 + 틴트.
    private void ShowFace(ECoinFace face, bool showLabel)
    {
        if (_tossFrames.Length > 0)
        {
            if (_coinImage != null)
            {
                _coinImage.color = Color.white;

                Sprite result = face == ECoinFace.Head ? _headResultSprite : _tailResultSprite;
                if (result != null)
                {
                    _coinImage.sprite = result;
                }
            }
        }
        else
        {
            SetCoinSprite(face);
        }

        //# 결과 라벨은 단일 던지기(showLabel)일 때만 — 다회는 코인 면 + 진행 표시로 충분 (요구 #3).
        if (showLabel == true)
        {
            int labelId = face == ECoinFace.Head ? StringIdHead : StringIdTail;
            _resultText.SetText(JsonManager.Instance.GetStringData(labelId));
        }
    }

    private void ShowSummary(List<ECoinFace> results)
    {
        if (results.Count <= 1)
            return;

        (int head, int tail) = CountFaces(results);

        string headText = string.Format(JsonManager.Instance.GetStringData(StringIdHeadCount), head);
        string tailText = string.Format(JsonManager.Instance.GetStringData(StringIdTailCount), tail);

        //# TODO(P0-coin): 10회 결과 리스트는 CHPoolingScrollView (BuildModalPopup 패턴) 로 확장 예정.
        //# 현재는 앞/뒤 카운트 요약 텍스트로 동작 (1/3/5/10 모두 동작, 리스트 셀 풀링은 미구현).
        StringBuilder sb = new StringBuilder();
        sb.Append(headText);
        sb.Append(" / ");
        sb.Append(tailText);

        _resultText.SetText(sb.ToString());
    }

    private void ResetToggles()
    {
        //# 첫 옵션(1회) 선택 상태로 리셋. CHToggle.toggle 이 아직 null 일 수 있어 가드.
        for (int i = 0; i < _countToggles.Count; i++)
        {
            CHToggle chToggle = _countToggles[i];
            if (chToggle == null)
                continue;

            if (chToggle.toggle == null)
                continue;

            chToggle.toggle.isOn = i == 0;
        }
    }

    private int GetSelectedCount()
    {
        for (int i = 0; i < _countToggles.Count && i < _countOptions.Length; i++)
        {
            CHToggle chToggle = _countToggles[i];
            if (chToggle == null)
                continue;

            if (chToggle.toggle != null && chToggle.toggle.isOn)
                return _countOptions[i];
        }

        return 1;
    }

    private async System.Threading.Tasks.Task LoadCoinSprite()
    {
        _coinSprite = await CHMResource.Instance.LoadAsync<Sprite>(CommonEnum.ECoin.Coin);

        _spriteLoaded = true;

        //# 로드 완료 시점이 OnEnable 보다 늦을 수 있어 기본면만 다시 반영 (라벨은 비움 유지).
        //# OnEnable 과 동일 분기 — 프레임 경로면 골드 착지 이미지 유지, 폴백만 정적 스프라이트.
        if (gameObject.activeInHierarchy == false)
            return;

        if (_tossFrames.Length > 0)
        {
            ShowDefaultResult();
        }
        else
        {
            SetCoinSprite(ECoinFace.Head);
        }
    }

    public void SetRouletteSceneAccess(IRouletteSceneAccess rouletteSceneAccess)
    {
        _rouletteSceneAccess = rouletteSceneAccess;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _rouletteSceneAccess.ShowScene(CommonEnum.ERouletteMenu.Menu);
    }

    //# === 결과 산출 (연출과 분리한 순수 로직 — test-engineer 진입점) ===

    //# 단일 던지기 결과 산출. 균등 50/50.
    public static ECoinFace Flip()
    {
        return UnityEngine.Random.Range(0, 2) == 0 ? ECoinFace.Head : ECoinFace.Tail;
    }

    //# 연속 N회 결과 산출. 시작 시점에 모두 확정 (기획서 §3-4).
    public static List<ECoinFace> FlipMany(int count)
    {
        List<ECoinFace> results = new List<ECoinFace>();

        if (count <= 0)
            return results;

        for (int i = 0; i < count; i++)
        {
            results.Add(Flip());
        }

        return results;
    }

    //# 결과 리스트의 앞/뒤 카운트 요약.
    public static (int head, int tail) CountFaces(List<ECoinFace> results)
    {
        int head = 0;
        int tail = 0;

        if (results == null)
            return (head, tail);

        foreach (ECoinFace face in results)
        {
            if (face == ECoinFace.Head)
            {
                ++head;
            }
            else
            {
                ++tail;
            }
        }

        return (head, tail);
    }
}
