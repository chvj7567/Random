using TMPro;
using UnityEngine;

public class Lotto2Scene : MonoBehaviour
{
    private const string Image_Path = "Lotto2";

    [SerializeField] private TMP_Text _jo1Text;
    [SerializeField] private TMP_Text _jo2Text;
    [SerializeField] private TMP_Text _jo3Text;
    [SerializeField] private TMP_Text _jo4Text;
    [SerializeField] private TMP_Text _jo5Text;
    [SerializeField] private NumberInfo _lotto1Info;
    [SerializeField] private NumberInfo _lotto2Info;
    [SerializeField] private NumberInfo _lotto3Info;
    [SerializeField] private NumberInfo _lotto4Info;
    [SerializeField] private NumberInfo _lotto5Info;
    [SerializeField] private ButtonEx _rouletteButton;

    private ILottoMenuSceneAccess _lottoMenuSceneAccess;

    private void Start()
    {
        _rouletteButton.OnClick(() =>
        {
            StartRoulette(_jo1Text, _lotto1Info);
            StartRoulette(_jo2Text, _lotto2Info);
            StartRoulette(_jo3Text, _lotto3Info);
            StartRoulette(_jo4Text, _lotto4Info);
            StartRoulette(_jo5Text, _lotto5Info);
        });
    }

    public void SetLottoMenuSceneAccess(ILottoMenuSceneAccess lottoMenuSceneAccess)
    {
        _lottoMenuSceneAccess = lottoMenuSceneAccess;
    }

    private void StartRoulette(TMP_Text joText, NumberInfo lottoInfo)
    {
        joText.text = $"{Random.Range(1, 6)}Á¶";

        foreach (var buttonEx in lottoInfo.liNumberButton)
        {
            buttonEx.SetText($"{Random.Range(0, 10)}");
        }
    }
}
