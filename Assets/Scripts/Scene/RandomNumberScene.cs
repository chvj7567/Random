using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class RandomNumberScene : MonoBehaviour
{
    [SerializeField] private ButtonEx _menuButton;
    [SerializeField] private TMP_InputField _startNumberInput;
    [SerializeField] private TMP_InputField _endNumberInput;
    [SerializeField] private ButtonEx _randomButton;

    private IMainSceneManagement _mainSceneManagement;

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
            var startNumber = CheckInteger(_startNumberInput);
            var endNubmer = CheckInteger(_endNumberInput);

            if (startNumber.Item1 == false || endNubmer.Item1 == false)
            {
                _randomButton.SetText("숫자 입력 확인");
            }
            else if (startNumber.Item2 > endNubmer.Item2)
            {
                _randomButton.SetText("숫자 범위 확인");
            }
            else
            {
                //_randomButton.SetText($"{Random.Range(startNumber.Item2, endNubmer.Item2 + 1)}");

                List<string> liText = new List<string>();

                for (int i = startNumber.Item2; i <= endNubmer.Item2; i++)
                {
                    liText.Add($"{i}");
                }

                UIManager.Instance.ShowUI(CommonEnum.EUI.UIRoulette, new UIRouletteArg
                {
                    liText = liText
                });
            }
        });
    }

    public void SetManagement(IMainSceneManagement manager)
    {
        _mainSceneManagement = manager;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _mainSceneManagement.ShowScene(CommonEnum.EMenu.Menu);
    }

    private (bool, int) CheckInteger(TMP_InputField input)
    {
        if (input == null)
            return (false, 0);

        if (int.TryParse(input.text, out int result) == false)
        {
            if (input.placeholder is TextMeshProUGUI placeholder)
            {
                placeholder.text = "정수 입력...";
            }

            return (false, 0);
        }

        return (true, result);
    }
}
