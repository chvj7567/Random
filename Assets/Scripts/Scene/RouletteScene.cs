using System;
using System.Collections.Generic;
using UnityEngine;

public interface IRouletteSceneAccess
{
    public void ShowScene(CommonEnum.EMenu sceneType);
}

public class RouletteScene : MonoBehaviour, IRouletteSceneAccess
{
    [Serializable]
    private class Menu
    {
        public CommonEnum.EMenu menu;
        public ButtonEx buttonEx;
    }

    [SerializeField] private List<GameObject> liMainSceneObj = new List<GameObject>();
    [SerializeField] private List<Menu> _liMenu = new List<Menu>();
    [SerializeField] private RandomNumberScene _randomNumberScene;
    [SerializeField] private CustomRandomScene _customRandomScene;
    [SerializeField] private GameObject _loadingObject;

    private CommonEnum.EMenu _curScene = CommonEnum.EMenu.Menu;

    private async void Start()
    {
        _loadingObject.SetActive(true);

        //# 매니저들 초기화
        await GameManagement.InitManager();

        //# 광고 On
        GameManagement.ShowBanner();

        //# 메뉴 관리 기능 넘겨줌
        SetManagement();

        //# 메뉴 버튼 기능 세팅
        SetMenuButton();

        //# 메뉴 씬 보여줌
        ShowScene(CommonEnum.EMenu.Menu);

        _loadingObject.SetActive(false);
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
        _customRandomScene.SetRouletteSceneAccess(this);
    }

    public void ShowScene(CommonEnum.EMenu sceneType)
    {
        switch (sceneType)
        {
            case CommonEnum.EMenu.Menu:
                {
                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(true);
                    }

                    _randomNumberScene.gameObject.SetActive(false);
                    _customRandomScene.gameObject.SetActive(false);
                }
                break;
            case CommonEnum.EMenu.RandomNumber:
                {
                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _randomNumberScene.gameObject.SetActive(true);
                    _customRandomScene.gameObject.SetActive(false);
                }
                break;
            case CommonEnum.EMenu.CustomRandom:
                {
                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _randomNumberScene.gameObject.SetActive(false);
                    _customRandomScene.gameObject.SetActive(true);
                }
                break;
        }
    }
}
