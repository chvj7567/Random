using System.Collections.Generic;
using UnityEngine;

public class RandomFoodScene : MonoBehaviour
{
    [SerializeField] private ButtonEx _menuButton;
    [SerializeField] private ButtonEx _rouletteButton;

    private IRouletteSceneAccess _rouletteSceneAccess;

    private void Start()
    {
        _menuButton.OnClick(() =>
        {
            Close();
        });

        _rouletteButton.OnClick(() =>
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
