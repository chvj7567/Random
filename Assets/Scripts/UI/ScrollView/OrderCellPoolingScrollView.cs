using ChvjUnityInfra;

//# 순서 정하기 결과 셀 1개의 데이터 (기획서 §9.4). class 로 정의 — struct 이면 ElementAtOrDefault null 체크가 항상 true.
public class OrderCellData
{
    public int Rank;
    public string Name;
}

//# CHPoolingScrollView 구현체 — InitItem 에서 셀 바인딩, 1열 세로 리스트 (Rule 03 §3).
public class OrderCellPoolingScrollView : CHPoolingScrollView<OrderCell, OrderCellData>
{
    public override void InitItem(OrderCell item, OrderCellData data, int index)
    {
        item.Bind(data);
    }

    public override void InitPoolingObject(OrderCell item) { }
}
