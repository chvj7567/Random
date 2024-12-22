using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public interface IMainSceneManager
{
    public void ShowMainScene();

    public void HideMainScene();
}

public class MainScene : MonoBehaviour, IMainSceneManager
{
    [Serializable]
    class Menu
    {
        public CommonEnum.EMenu menu;
        public ButtonEx buttonEx;
    }

    [SerializeField] List<GameObject> liMainSceneObj = new List<GameObject>();
    [SerializeField] List<Menu> _liMenu = new List<Menu>();
    [SerializeField] RandomNumberScene _randomNumberScene;
    [SerializeField] CustomRandomScene _customRandomScene;
    [SerializeField] GameObject _loadingObject;
    CommonEnum.EMenu _curScene = CommonEnum.EMenu.None;

    private async void Start()
    {
        _loadingObject.SetActive(true);

        HideMainScene();
        _randomNumberScene.gameObject.SetActive(false);
        _customRandomScene.gameObject.SetActive(false);

        await ResourceManager.Instance.Init();
        await UIManager.Instance.Init();
        AudioManager.Instance.Init();
        AdmobManager.Instance.Init();

        AdmobManager.Instance.ShowBanner(AdPosition.Top);
        AdmobManager.Instance.ShowBanner(AdPosition.Bottom);

        UIManager.Instance.ShowUI(CommonEnum.EUI.UILoading, null, (uiBase) =>
        {
            UIManager.Instance.CloseUI(uiBase);
        });

        ShowMainScene();
        SetMenuButton();

        _randomNumberScene.SetManager(this);
        _customRandomScene.SetManager(this);

        _loadingObject.SetActive(false);
    }

    private void SetMenuButton()
    {
        foreach (var menuInfo in _liMenu)
        {
            switch (menuInfo.menu)
            {
                case CommonEnum.EMenu.RandomNumber:
                    {
                        menuInfo.buttonEx.OnClick(() =>
                        {
                            _curScene = CommonEnum.EMenu.RandomNumber;
                            HideMainScene();

                            _randomNumberScene.gameObject.SetActive(true);
                        });
                    }
                    break;
                case CommonEnum.EMenu.Lotto:
                    {
                        menuInfo.buttonEx.OnClick(() =>
                        {
                            _curScene = CommonEnum.EMenu.Lotto;
                            SceneManager.LoadScene(1);
                        });
                    }
                    break;
                case CommonEnum.EMenu.CustomRandom:
                    {
                        menuInfo.buttonEx.OnClick(() =>
                        {
                            _curScene = CommonEnum.EMenu.CustomRandom;
                            foreach (var obj in liMainSceneObj)
                            {
                                obj.SetActive(false);
                            }

                            _customRandomScene.gameObject.SetActive(true);
                        });
                    }
                    break;
            }
        }
    }

    public void ShowMainScene()
    {
        foreach (var obj in liMainSceneObj)
        {
            obj.SetActive(true);
        }
    }

    public void HideMainScene()
    {
        foreach (var obj in liMainSceneObj)
        {
            obj.SetActive(false);
        }
    }
}
