using ChvjUnityInfra;
using UnityEngine;

public class Lotto2Scene : MonoBehaviour, IRouletteBackButton
{
    private const string Image_Path = "Lotto2";

    [SerializeField] private CHText _jo1Text;
    [SerializeField] private CHText _jo2Text;
    [SerializeField] private CHText _jo3Text;
    [SerializeField] private CHText _jo4Text;
    [SerializeField] private CHText _jo5Text;
    [SerializeField] private NumberInfo _lotto1Info;
    [SerializeField] private NumberInfo _lotto2Info;
    [SerializeField] private NumberInfo _lotto3Info;
    [SerializeField] private NumberInfo _lotto4Info;
    [SerializeField] private NumberInfo _lotto5Info;
    [SerializeField] private CHButton _rouletteButton;
    [SerializeField] private CHButton _menuButton;

    private IRandomSceneAccess _randomSceneAccess;

    private void Start()
    {
        //# _menuButton 미와이어링 시 등록 건너뜀 (크래시 방지)
        if (_menuButton != null)
        {
            _menuButton.OnClick(() =>
            {
                Close();
            });
        }

        _rouletteButton.OnClick(() =>
        {
            StartRoulette();
        });

        StartRoulette();
    }

    public void SetRandomSceneAccess(IRandomSceneAccess randomSceneAccess)
    {
        _randomSceneAccess = randomSceneAccess;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _randomSceneAccess.ShowScene(CommonEnum.ERouletteMenu.Menu);
    }

    private void StartRoulette()
    {
        StartRoulette(_jo1Text, _lotto1Info);
        StartRoulette(_jo2Text, _lotto2Info);
        StartRoulette(_jo3Text, _lotto3Info);
        StartRoulette(_jo4Text, _lotto4Info);
        StartRoulette(_jo5Text, _lotto5Info);
    }

    private void StartRoulette(CHText joText, NumberInfo lottoInfo)
    {
        joText.SetText(Random.Range(1, 6));

        foreach (CHButton button in lottoInfo.liNumberButton)
        {
            button.SetText($"{Random.Range(0, 10)}");
        }
    }
}
