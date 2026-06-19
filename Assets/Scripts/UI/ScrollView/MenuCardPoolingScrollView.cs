using ChvjUnityInfra;

//# 메뉴 카드 셀 데이터 (기획서 §7.4). 표시 문자열은 String.json stringID 경유.
public class MenuCardData
{
    public CommonEnum.ERouletteMenu menu;
    public CommonEnum.EMenuIcon iconKey;
    public int titleStringID;
    public int descStringID;
}

//# 메인 메뉴 세로 카드 리스트 (Rule 03 §3 3-class 중 ScrollView). 위치/간격/패딩은 인스펙터 필드로 설정.
public class MenuCardPoolingScrollView : CHPoolingScrollView<MenuCardCell, MenuCardData>
{
    private IRandomSceneAccess _access;

    //# Panel 이 SetItemList(=풀 생성) 전에 진입 경로를 주입.
    public void SetAccess(IRandomSceneAccess access)
    {
        _access = access;
    }

    //# 풀 재사용 시마다 호출 — 표시 갱신만. 클릭 바인딩은 InitPoolingObject 1회 (리스너 누적 방지).
    public override void InitItem(MenuCardCell item, MenuCardData data, int index)
    {
        item.Bind(data);
    }

    //# 풀링 오브젝트 생성 시 1회 — 클릭 핸들러를 여기서 등록(셀의 현재 data 를 읽음).
    public override void InitPoolingObject(MenuCardCell item)
    {
        item.SetupClick(_access);
    }
}
