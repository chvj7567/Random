using System.Collections.Generic;

//# 메인 메뉴 카드 정의 (Unity 비의존 순수 데이터). 모드 추가 = 여기 1행 추가 (데이터 주도, 기획서 §6).
//# MonoBehaviour 와 분리해 테스트 가능하게 둠 — 표시 책임은 MenuPanel, 정의 책임은 본 클래스.
public static class MenuCardCatalog
{
    //# 이번 빌드 4종 (기획서 §5.3 확정값). 순서 = 화면 노출 순서.
    public static List<MenuCardData> BuildMenuCards()
    {
        return new List<MenuCardData>
        {
            new MenuCardData
            {
                menu = CommonEnum.ERouletteMenu.RandomNumber,
                iconKey = CommonEnum.EMenuIcon.MenuNumber,
                titleStringID = 152,
                descStringID = 153,
            },
            new MenuCardData
            {
                menu = CommonEnum.ERouletteMenu.RandomFood,
                iconKey = CommonEnum.EMenuIcon.MenuFood,
                titleStringID = 154,
                descStringID = 155,
            },
            new MenuCardData
            {
                menu = CommonEnum.ERouletteMenu.CustomRandom,
                iconKey = CommonEnum.EMenuIcon.MenuShuffle,
                titleStringID = 156,
                descStringID = 157,
            },
            new MenuCardData
            {
                menu = CommonEnum.ERouletteMenu.CoinFlip,
                iconKey = CommonEnum.EMenuIcon.Coin,
                titleStringID = 158,
                descStringID = 159,
            },
        };
    }
}
