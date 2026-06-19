using ChvjUnityInfra;
using System.Collections.Generic;
using UnityEngine;

public class RandomExampleScene : MonoBehaviour, IRouletteBackButton
{
    [SerializeField] private CHButton _menuButton;

    [SerializeField] private CHButton _randomNumberButton;
    [SerializeField] private CHButton _randomYesNoButton;
    [SerializeField] private CHButton _randomMonthButton;
    [SerializeField] private CHButton _randomDayButton;
    [SerializeField] private CHButton _randomAnimalButton;
    [SerializeField] private CHButton _randomCountryButton;

    //# 표시 문자열 stringID (String.json)
    private const int StringIdYes = 100;
    private const int StringIdNo = 101;
    private const int StringIdMonthStart = 110;
    private const int StringIdDayFormat = 130;

    private IRandomSceneAccess _randomSceneAccess;

    private void Start()
    {
        _menuButton.OnClick(() =>
        {
            Close();
        });

        _randomNumberButton.OnClick(() =>
        {
            List<string> liNumber = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                liNumber.Add($"{i}");
            }

            ShowRoulette(liNumber);
        });

        _randomYesNoButton.OnClick(() =>
        {
            List<string> liYesNo = new List<string>();

            string yesText = JsonManager.Instance.GetStringData(StringIdYes);
            string noText = JsonManager.Instance.GetStringData(StringIdNo);

            bool yes = true;
            for (int i = 0; i < 8; i++)
            {
                if (yes)
                {
                    yes = false;
                    liYesNo.Add(yesText);
                }
                else
                {
                    yes = true;
                    liYesNo.Add(noText);
                }
            }

            ShowRoulette(liYesNo);
        });

        _randomMonthButton.OnClick(() =>
        {
            List<string> liMonth = new List<string>();
            for (int i = 0; i < 12; i++)
            {
                liMonth.Add(JsonManager.Instance.GetStringData(StringIdMonthStart + i));
            }

            ShowRoulette(liMonth);
        });

        _randomDayButton.OnClick(() =>
        {
            List<string> liDay = new List<string>();

            string dayFormat = JsonManager.Instance.GetStringData(StringIdDayFormat);
            for (int i = 1; i <= 31; i++)
            {
                liDay.Add(string.Format(dayFormat, i));
            }

            ShowRoulette(liDay);
        });

        _randomAnimalButton.OnClick(() =>
        {
            ShowRoulette(JsonManager.Instance.GetAnimalNameList());
        });

        _randomCountryButton.OnClick(() =>
        {
            ShowRoulette(JsonManager.Instance.GetCountryNameList());
        });
    }

    private void ShowRoulette(List<string> liText)
    {
        CHMUI.Instance.ShowUI(CommonEnum.EUI.UIRoulette, new UIRouletteArg
        {
            liText = liText,
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
}
