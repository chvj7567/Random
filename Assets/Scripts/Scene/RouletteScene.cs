using System;
using System.Collections.Generic;
using UnityEngine;

public interface IRouletteSceneAccess
{
    public void ShowScene(CommonEnum.ERouletteMenu sceneType);
}

public class RouletteScene : MonoBehaviour, IRouletteSceneAccess
{
    [Serializable]
    private class Menu
    {
        public CommonEnum.ERouletteMenu menu;
        public ButtonEx buttonEx;
    }

    [SerializeField] private List<GameObject> liMainSceneObj = new List<GameObject>();
    [SerializeField] private List<Menu> _liMenu = new List<Menu>();
    [SerializeField] private RandomNumberScene _randomNumberScene;
    [SerializeField] private RandomExampleScene _randomFoodScene;
    [SerializeField] private CustomRandomScene _customRandomScene;

    private CommonEnum.ERouletteMenu _curScene = CommonEnum.ERouletteMenu.Menu;

    private async void Start()
    {
        //# 광고 On
        GameManagement.Instance.ShowBanner();

        //# 메뉴 관리 기능 넘겨줌
        SetManagement();

        //# 메뉴 버튼 기능 세팅
        SetMenuButton();

        //# 메뉴 씬 보여줌
        ShowScene(CommonEnum.ERouletteMenu.Menu);
    }

    private void SetMenuButton()
    {
        foreach (var menuInfo in _liMenu)
        {
            menuInfo.buttonEx.OnClick(() =>
            {
                _curScene = menuInfo.menu;
                ShowScene(menuInfo.menu);
            });
        }
    }

    private void SetManagement()
    {
        _randomNumberScene.SetRouletteSceneAccess(this);
        _randomFoodScene.SetRouletteSceneAccess(this);
        _customRandomScene.SetRouletteSceneAccess(this);
    }

    public void ShowScene(CommonEnum.ERouletteMenu sceneType)
    {
        _randomNumberScene.gameObject.SetActive(false);
        _randomFoodScene.gameObject.SetActive(false);
        _customRandomScene.gameObject.SetActive(false);

        switch (sceneType)
        {
            case CommonEnum.ERouletteMenu.Menu:
                {
                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(true);
                    }
                }
                break;
            case CommonEnum.ERouletteMenu.RandomNumber:
                {
                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _randomNumberScene.gameObject.SetActive(true);
                }
                break;
            case CommonEnum.ERouletteMenu.RandomFood:
                {
                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _randomFoodScene.gameObject.SetActive(true);
                }
                break;
            case CommonEnum.ERouletteMenu.CustomRandom:
                {
                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _customRandomScene.gameObject.SetActive(true);
                }
                break;
        }
    }
}
