using System.Collections.Generic;

//# 메인 메뉴 카드 정의 (Unity 비의존 순수 데이터). 모드 추가 = 여기 1행 추가 (데이터 주도, 기획서 §6).
//# MonoBehaviour 와 분리해 테스트 가능하게 둠 — 표시 책임은 MenuPanel, 정의 책임은 본 클래스.
public static class MenuCardCatalog
{
    //# 이번 빌드 5종 (4종=기획서 §5.3 확정값 + 로또=사용자 요구 직접 추가). 순서 = 화면 노출 순서.
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
            new MenuCardData
            {
                menu = CommonEnum.ERouletteMenu.Lotto1,
                iconKey = CommonEnum.EMenuIcon.MenuLotto1,
                titleStringID = 2057,
                descStringID = 2058,
            },
            new MenuCardData
            {
                menu = CommonEnum.ERouletteMenu.Lotto2,
                iconKey = CommonEnum.EMenuIcon.MenuLotto2,
                titleStringID = 2059,
                descStringID = 2060,
            },
            new MenuCardData
            {
                menu = CommonEnum.ERouletteMenu.Ladder,
                iconKey = CommonEnum.EMenuIcon.MenuLadder,
                titleStringID = 2061,
                descStringID = 2062,
            },
            new MenuCardData
            {
                menu = CommonEnum.ERouletteMenu.TeamSplit,
                iconKey = CommonEnum.EMenuIcon.MenuTeamSplit,
                titleStringID = 2078,
                descStringID = 2079,
            },
        };
    }
}
