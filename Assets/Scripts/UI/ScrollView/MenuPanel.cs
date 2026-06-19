using UnityEngine;

//# 메인 메뉴 카드 리스트 컨테이너 (Rule 03 §3 3-class 중 Panel).
//# 카드 정의는 MenuCardCatalog(순수 데이터)에 위임 — 본 클래스는 표시·주입만.
public class MenuPanel : MonoBehaviour
{
    [SerializeField] private MenuCardPoolingScrollView _scrollView;

    private bool _built;

    //# 진입 경로 주입 + 최초 데이터 구성. RandomScene.Start 에서 1회 호출.
    public void Setup(IRandomSceneAccess access)
    {
        if (_scrollView == null)
            return;

        _scrollView.SetAccess(access);
        _scrollView.SetItemList(MenuCardCatalog.BuildMenuCards());
        _built = true;
    }

    //# 메뉴 화면 복귀 시 호출 — 스크롤 위치 상단 리셋 (기획서 §8).
    public void ResetScroll()
    {
        if (_built == false)
            return;

        _scrollView.SetScrollPosition(0);
    }
}
