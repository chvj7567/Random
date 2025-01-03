using System;
using System.Collections.Generic;
using UnityEngine;

public interface ILottoMenuSceneAccess
{
    public void ShowScene(CommonEnum.ELottoMenu sceneType);
}

public class LottoMenuScene : MonoBehaviour, ILottoMenuSceneAccess
{
    [Serializable]
    private class Menu
    {
        public CommonEnum.ELottoMenu menu;
        public ButtonEx buttonEx;
    }

    [SerializeField] private List<GameObject> liMainSceneObj = new List<GameObject>();
    [SerializeField] private List<Menu> _liMenu = new List<Menu>();
    [SerializeField] private LottoScene _lottoScene;
    [SerializeField] private Lotto2Scene _lotto2Scene;

    private CommonEnum.ELottoMenu _curScene = CommonEnum.ELottoMenu.Menu;

    private void Start()
    {
        //# 광고 On
        GameManagement.Instance.ShowBanner();

        //# 메뉴 관리 기능 넘겨줌
        SetManagement();

        //# 메뉴 버튼 기능 세팅
        SetMenuButton();

        //# 메뉴 씬 보여줌
        ShowScene(CommonEnum.ELottoMenu.Menu);
    }

    private void SetManagement()
    {
        _lottoScene.SetLottoMenuSceneAccess(this);
        _lotto2Scene.SetLottoMenuSceneAccess(this);
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

    public void ShowScene(CommonEnum.ELottoMenu sceneType)
    {
        _lottoScene.gameObject.SetActive(false);
        _lotto2Scene.gameObject.SetActive(false);

        switch (sceneType)
        {
            case CommonEnum.ELottoMenu.Menu:
                {
                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(true);
                    }
                }
                break;
            case CommonEnum.ELottoMenu.Lotto:
                {
                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _lottoScene.gameObject.SetActive(true);
                }
                break;
            case CommonEnum.ELottoMenu.Lotto2:
                {
                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _lotto2Scene.gameObject.SetActive(true);
                }
                break;
        }
    }
}
