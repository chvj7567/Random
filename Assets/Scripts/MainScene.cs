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

    private async void Start()
    {
        await ResourceManager.Instance.Init();
        await UIManager.Instance.Init();

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
                            foreach (var obj in liMainSceneObj)
                            {
                                obj.SetActive(false);
                            }

                            _randomNumberScene.gameObject.SetActive(true);
                        }).AddTo(this);
                    }
                    break;
                case CommonEnum.EMenu.Lotto:
                    {
                        menuInfo.button.OnClickAsObservable().Subscribe(_ =>
                        {
                            SceneManager.LoadScene(1);
                        }).AddTo(this);
                    }
                    break;
                case CommonEnum.EMenu.CustomRandom:
                    {
                        menuInfo.button.OnClickAsObservable().Subscribe(_ =>
                        {
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
}
