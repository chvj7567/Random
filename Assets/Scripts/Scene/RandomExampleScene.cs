using System.Collections.Generic;
using UnityEngine;

public class RandomExampleScene : MonoBehaviour
{
    [SerializeField] private ButtonEx _menuButton;

    [SerializeField] private ButtonEx _randomNumberButton;
    [SerializeField] private ButtonEx _randomYesNoButton;
    [SerializeField] private ButtonEx _randomMonthButton;
    [SerializeField] private ButtonEx _randomDayButton;
    [SerializeField] private ButtonEx _randomFoodButton;
    [SerializeField] private ButtonEx _randomCountryButton;

    private IRouletteSceneAccess _rouletteSceneAccess;

    private void Start()
    {
        _menuButton.OnClick(() =>
        {
            Close();
        });

        _randomNumberButton.OnClick(() =>
        {
            List<string> liNumber = new List<string>();
            for (int i = 0; i < 10; i ++)
            {
                liNumber.Add($"{i}");
            }

            UIManager.Instance.ShowUI(CommonEnum.EUI.UIRoulette, new UIRouletteArg
            {
                liText = liNumber
            });
        });

        _randomYesNoButton.OnClick(() =>
        {
            List<string> liYesNo = new List<string>();

            bool yes = true;
            for (int i = 0; i < 8; i++)
            {
                if (yes)
                {
                    yes = false;
                    liYesNo.Add("Yes");
                }
                else
                {
                    yes = true;
                    liYesNo.Add("NO");
                }
            }

            UIManager.Instance.ShowUI(CommonEnum.EUI.UIRoulette, new UIRouletteArg
            {
                liText = liYesNo
            });
        });

        _randomMonthButton.OnClick(() =>
        {
            List<string> liMonth = new List<string>();
            for (int i = 1; i <= 12; i++)
            {
                liMonth.Add($"{i}");
            }

            UIManager.Instance.ShowUI(CommonEnum.EUI.UIRoulette, new UIRouletteArg
            {
                liText = liMonth
            });
        });

        _randomDayButton.OnClick(() =>
        {
            List<string> liDay = new List<string>();
            for (int i = 1; i <= 31; i++)
            {
                liDay.Add($"{i}");
            }

            UIManager.Instance.ShowUI(CommonEnum.EUI.UIRoulette, new UIRouletteArg
            {
                liText = liDay
            });
        });

        _randomFoodButton.OnClick(() =>
        {
            var liJsonData = JsonManager.Instance.GetFoodDataList();

            List<string> liFood = new List<string>();
            foreach (var data in liJsonData)
            {
                liFood.Add(data.food);
            }

            UIManager.Instance.ShowUI(CommonEnum.EUI.UIRoulette, new UIRouletteArg
            {
                liText = liFood
            });
        });

        _randomCountryButton.OnClick(() =>
        {
            var liJsonData = JsonManager.Instance.GetCountryDataList();

            List<string> liCountry = new List<string>();
            foreach (var data in liJsonData)
            {
                liCountry.Add(data.name);
            }

            UIManager.Instance.ShowUI(CommonEnum.EUI.UIRoulette, new UIRouletteArg
            {
                liText = liCountry
            });
        });
    }

    public void SetRouletteSceneAccess(IRouletteSceneAccess rouletteSceneAccess)
    {
        _rouletteSceneAccess = rouletteSceneAccess;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _rouletteSceneAccess.ShowScene(CommonEnum.EMenu.Menu);
    }
}
