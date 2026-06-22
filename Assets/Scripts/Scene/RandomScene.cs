using ChvjUnityInfra;
using System.Collections.Generic;
using UnityEngine;

public interface IRouletteBackButton
{
    public void Close();
}
public interface IRandomSceneAccess
{
    public void ShowScene(CommonEnum.ERouletteMenu sceneType);
}

public class RandomScene : MonoBehaviour, IRandomSceneAccess
{
    [SerializeField] private List<GameObject> liMainSceneObj = new List<GameObject>();
    [SerializeField] private MenuPanel _menuPanel;
    [SerializeField] private RandomNumberScene _randomNumberScene;
    [SerializeField] private RandomExampleScene _randomFoodScene;
    [SerializeField] private CustomRandomScene _customRandomScene;
    [SerializeField] private CoinFlipScene _coinFlipScene;
    [SerializeField] private LottoScene _lottoScene;
    [SerializeField] private Lotto2Scene _lotto2Scene;
    [SerializeField] private LadderScene _ladderScene;

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

        //# 메뉴 카드 리스트 구성 (데이터 주도) — 진입 경로 주입 + 카드 데이터 채우기.
        if (_menuPanel != null)
            _menuPanel.Setup(this);

        //# 메뉴 화면 보여주기
        ShowScene(CommonEnum.ERouletteMenu.Menu);
    }

    private void SetManagement()
    {
        _randomNumberScene.SetRandomSceneAccess(this);
        _randomFoodScene.SetRandomSceneAccess(this);
        _customRandomScene.SetRandomSceneAccess(this);
        _coinFlipScene.SetRandomSceneAccess(this);
        _lottoScene.SetRandomSceneAccess(this);
        _lotto2Scene.SetRandomSceneAccess(this);
        _ladderScene.SetRandomSceneAccess(this);
    }

    public void ShowScene(CommonEnum.ERouletteMenu sceneType)
    {
        _randomNumberScene.gameObject.SetActive(false);
        _randomFoodScene.gameObject.SetActive(false);
        _customRandomScene.gameObject.SetActive(false);
        _coinFlipScene.gameObject.SetActive(false);
        _lottoScene.gameObject.SetActive(false);
        _lotto2Scene.gameObject.SetActive(false);
        _ladderScene.gameObject.SetActive(false);

        switch (sceneType)
        {
            case CommonEnum.ERouletteMenu.Menu:
                {
                    _mainRouletteUI = null;

                    foreach (var obj in liMainSceneObj)
                    {
                        obj.SetActive(true);
                    }

                    //# 메뉴 복귀 시 스크롤 상단 리셋 (기획서 §8).
                    if (_menuPanel != null)
                        _menuPanel.ResetScroll();
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
            case CommonEnum.ERouletteMenu.Lotto1:
                {
                    _mainRouletteUI = _lottoScene;

                    foreach (GameObject obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _lottoScene.gameObject.SetActive(true);
                }
                break;
            case CommonEnum.ERouletteMenu.Lotto2:
                {
                    _mainRouletteUI = _lotto2Scene;

                    foreach (GameObject obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _lotto2Scene.gameObject.SetActive(true);
                }
                break;
            case CommonEnum.ERouletteMenu.Ladder:
                {
                    _mainRouletteUI = _ladderScene;

                    foreach (GameObject obj in liMainSceneObj)
                    {
                        obj.SetActive(false);
                    }

                    _ladderScene.gameObject.SetActive(true);
                }
                break;
        }
    }
}
