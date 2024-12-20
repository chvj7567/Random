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
        public Button button;
    }

    [SerializeField] List<GameObject> liMainSceneObj = new List<GameObject>();
    [SerializeField] List<Menu> _liMenu = new List<Menu>();
    [SerializeField] RandomNumberScene _randomNumberScene;
    [SerializeField] CustomRandomScene _customRandomScene;

    CommonEnum.EMenu _curScene = CommonEnum.EMenu.None;

    private async void Start()
    {
        HideMainScene();
        _randomNumberScene.gameObject.SetActive(false);
        _customRandomScene.gameObject.SetActive(false);

        await ResourceManager.Instance.Init();
        await UIManager.Instance.Init();

        ShowMainScene();

        _randomNumberScene.SetManager(this);
        _customRandomScene.SetManager(this);

        foreach (var menuInfo in _liMenu)
        {
            switch (menuInfo.menu)
            {
                case CommonEnum.EMenu.RandomNumber:
                    {
                        menuInfo.button.OnClickAsObservable().Subscribe(_ =>
                        {
                            _curScene = CommonEnum.EMenu.RandomNumber;
                            HideMainScene();

                            _randomNumberScene.gameObject.SetActive(true);
                        }).AddTo(this);
                    }
                    break;
                case CommonEnum.EMenu.Lotto:
                    {
                        menuInfo.button.OnClickAsObservable().Subscribe(_ =>
                        {
                            _curScene = CommonEnum.EMenu.Lotto;
                            SceneManager.LoadScene(1);
                        }).AddTo(this);
                    }
                    break;
                case CommonEnum.EMenu.CustomRandom:
                    {
                        menuInfo.button.OnClickAsObservable().Subscribe(_ =>
                        {
                            _curScene = CommonEnum.EMenu.CustomRandom;
                            foreach (var obj in liMainSceneObj)
                            {
                                obj.SetActive(false);
                            }

                            _customRandomScene.gameObject.SetActive(true);
                        }).AddTo(this);
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
