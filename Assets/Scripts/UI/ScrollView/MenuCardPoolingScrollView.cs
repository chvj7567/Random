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

    //# 풀 재사용 시마다 호출 — 표시 갱신 + 클릭 등록(셀이 가드로 1회만 실제 등록).
    //# origin 셀은 InitPoolingObject 를 받지 않으므로 클릭 등록을 여기서도 해야 누락이 없다.
    public override void InitItem(MenuCardCell item, MenuCardData data, int index)
    {
        item.SetupClick(_access);
        item.Bind(data);
    }

    //# 풀링 오브젝트(클론) 생성 시 1회 — 클릭 핸들러 선등록. (origin 은 여기로 안 옴 → InitItem 이 보완)
    public override void InitPoolingObject(MenuCardCell item)
    {
        item.SetupClick(_access);
    }
}
