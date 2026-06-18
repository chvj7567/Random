using ChvjUnityInfra;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IRouletteBackButton
{
    public void Close();
}
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
        public CHButton buttonEx;
    }

    [SerializeField] private List<GameObject> liMainSceneObj = new List<GameObject>();
    [SerializeField] private List<Menu> _liMenu = new List<Menu>();
    [SerializeField] private RandomNumberScene _randomNumberScene;
    [SerializeField] private RandomExampleScene _randomFoodScene;
    [SerializeField] private CustomRandomScene _customRandomScene;
    [SerializeField] private CoinFlipScene _coinFlipScene;

    private CommonEnum.ERouletteMenu _curScene = CommonEnum.ERouletteMenu.Menu;
    private IRouletteBackButton _mainRouletteUI;

    private void Update()
    {
        //# CHMUI 가 떠 있으면 ESC 는 패키지가 최상위 UI 를 닫음 → 여기선 처리하지 않음.
        //# 떠 있는 UI 가 없을 때만 현재 서브 화면을 메뉴로 되돌린다(안드로이드 뒤로가기 대체).
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CHMUI.Instance.CheckUI)
                return;

            _mainRouletteUI?.Close();
        }
    }

    private async void Start()
    {
        //# 배너 On
        GameManagement.Instance.ShowBanner();

        //# 메뉴 매니저 접근 넘기기
        SetManagement();

        //# 메뉴 버튼 바인딩
        SetMenuButton();

        //# 메뉴 화면 보여주기
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
        _coinFlipScene.SetRouletteSceneAccess(this);
    }

    public void ShowScene(CommonEnum.ERouletteMenu sceneType)
    {
        _randomNumberScene.gameObject.SetActive(false);
        _randomFoodScene.gameObject.SetActive(false);
        _customRandomScene.gameObject.SetActive(false);
        _coinFlipScene.gameObject.SetActive(false);

        switch (sceneType)
        {
            case CommonEnum.ERouletteMenu.Menu:
                {
                    _mainRouletteUI = null;

                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(true);
                    }
                }
                break;
            case CommonEnum.ERouletteMenu.RandomNumber:
                {
                    _mainRouletteUI = _randomNumberScene;

                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _randomNumberScene.gameObject.SetActive(true);
                }
                break;
            case CommonEnum.ERouletteMenu.RandomFood:
                {
                    _mainRouletteUI = _randomFoodScene;

                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _randomFoodScene.gameObject.SetActive(true);
                }
                break;
            case CommonEnum.ERouletteMenu.CustomRandom:
                {
                    _mainRouletteUI = _customRandomScene;

                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _customRandomScene.gameObject.SetActive(true);
                }
                break;
            case CommonEnum.ERouletteMenu.CoinFlip:
                {
                    _mainRouletteUI = _coinFlipScene;

                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _coinFlipScene.gameObject.SetActive(true);
                }
                break;
        }
    }
}
