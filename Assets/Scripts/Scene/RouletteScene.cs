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
    [SerializeField] private RandomExampleScene _randomFoodScene;
    [SerializeField] private CustomRandomScene _customRandomScene;

    private CommonEnum.EMenu _curScene = CommonEnum.EMenu.Menu;

    private async void Start()
    {
        //# ���� On
        GameManagement.Instance.ShowBanner();

        //# �޴� ���� ��� �Ѱ���
        SetManagement();

        //# �޴� ��ư ��� ����
        SetMenuButton();

        //# �޴� �� ������
        ShowScene(CommonEnum.EMenu.Menu);
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

    public void ShowScene(CommonEnum.EMenu sceneType)
    {
        _randomNumberScene.gameObject.SetActive(false);
        _randomFoodScene.gameObject.SetActive(false);
        _customRandomScene.gameObject.SetActive(false);

        switch (sceneType)
        {
            case CommonEnum.EMenu.Menu:
                {
                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(true);
                    }
                }
                break;
            case CommonEnum.EMenu.RandomNumber:
                {
                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _randomNumberScene.gameObject.SetActive(true);
                }
                break;
            case CommonEnum.EMenu.RandomFood:
                {
                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _randomFoodScene.gameObject.SetActive(true);
                }
                break;
            case CommonEnum.EMenu.CustomRandom:
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
