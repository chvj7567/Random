using ChvjUnityInfra;
using UnityEngine;

//# 순서 정하기 결과 셀 1개 (기획서 §9.1). CHPoolingScrollView 풀링 대상.
//# 내부 위젯은 모두 [SerializeField] private — 외부엔 Bind/Reset 의도 API 만 노출 (Rule 02 §6.1).
public class OrderCell : MonoBehaviour
{
    [SerializeField] private CHText _text;

    //# 순번 포맷 stringID — "{0}. {1}" (기획서 §6.3).
    private const int StringIdRankFormat = 2081;

    //# 풀 재사용 시 이전 상태 초기화 (Rule 03 §4).
    private void OnEnable()
    {
        ResetCell();
    }

    private void ResetCell()
    {
        if (_text != null)
        {
            _text.SetText(string.Empty);
        }
    }

    //# 순번 + 이름을 포맷 문자열로 표시 (기획서 §4).
    public void Bind(OrderCellData data)
    {
        if (_text == null)
            return;

        if (data == null)
            return;

        _text.SetText(string.Format(JsonManager.Instance.GetStringData(StringIdRankFormat), data.Rank, data.Name));
    }
}
