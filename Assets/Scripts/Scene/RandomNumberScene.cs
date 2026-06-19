using ChvjUnityInfra;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RandomNumberScene : MonoBehaviour, IRouletteBackButton
{
    [SerializeField] private CHButton _menuButton;
    [SerializeField] private TMP_InputField _startNumberInput;
    [SerializeField] private TMP_InputField _endNumberInput;
    [SerializeField] private CHButton _randomButton;

    //# 표시 문자열 stringID (String.json)
    private const int StringIdInputError = 140;
    private const int StringIdRangeError = 141;

    private IRandomSceneAccess _randomSceneAccess;

    private void OnEnable()
    {
        _startNumberInput.text = string.Empty;
        _endNumberInput.text = string.Empty;
        _randomButton.SetText("Random");
    }

    private void Start()
    {
        _menuButton.OnClick(() =>
        {
            Close();
        });

        _randomButton.OnClick(() =>
        {
            (bool, int) startNumber = CheckInteger(_startNumberInput);
            (bool, int) endNubmer = CheckInteger(_endNumberInput);

            if (startNumber.Item1 == false || endNubmer.Item1 == false)
            {
                _randomButton.SetText(JsonManager.Instance.GetStringData(StringIdInputError));
            }
            else if (startNumber.Item2 > endNubmer.Item2)
            {
                _randomButton.SetText(JsonManager.Instance.GetStringData(StringIdRangeError));
            }
            else
            {
                //_randomButton.SetText($"{Random.Range(startNumber.Item2, endNubmer.Item2 + 1)}");

                List<string> liText = new List<string>();

                for (int i = startNumber.Item2; i <= endNubmer.Item2; i++)
                {
                    liText.Add($"{i}");
                }

                CHMUI.Instance.ShowUI(CommonEnum.EUI.UIRoulette, new UIRouletteArg
                {
                    liText = liText
                });
            }
        });
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

    private (bool, int) CheckInteger(TMP_InputField input)
    {
        if (input == null)
            return (false, 0);

        if (int.TryParse(input.text, out int result) == false)
        {
            return (false, 0);
        }

        return (true, result);
    }
}
