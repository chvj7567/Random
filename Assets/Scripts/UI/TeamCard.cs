using ChvjUnityInfra;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//# 팀 나누기 결과 카드 1장 (기획서 §4.2). CHPoolingScrollView 풀링 대상.
//# 내부 위젯은 모두 [SerializeField] private — 외부엔 Bind/Reset 의도 API 만 노출 (Rule 02 §6.1).
public class TeamCard : MonoBehaviour
{
    [SerializeField] private Image _colorBadge;
    [SerializeField] private CHText _headerText;
    [SerializeField] private CHText _countText;
    [SerializeField] private CHText _memberText;
    [SerializeField] private CHText _memberText1;

    private const int StringIdTeamHeader = 2073;
    private const int FirstGroupSize = 3;

    //# 풀 재사용 시 이전 상태 초기화 (Rule 03 §4).
    private void OnEnable()
    {
        ResetCard();
    }

    private void ResetCard()
    {
        if (_headerText != null)
        {
            _headerText.SetText(string.Empty);
        }

        if (_countText != null)
        {
            _countText.SetText(string.Empty);
        }

        if (_memberText != null)
        {
            _memberText.SetText(string.Empty);
        }

        if (_memberText1 != null)
        {
            _memberText1.SetText(string.Empty);
        }

        if (_colorBadge != null)
        {
            _colorBadge.color = Color.white;
        }
    }

    //# 팀 1장의 4요소(색 뱃지·헤더·인원·멤버)를 한 번에 채운다.
    //# teamIndex 는 0-based, 표시 헤더는 +1 ("1팀"부터).
    public void Bind(int teamIndex, IList<string> members, Color color)
    {
        if (_colorBadge != null)
        {
            _colorBadge.color = color;
        }

        if (_headerText != null)
        {
            _headerText.SetText(string.Format(JsonManager.Instance.GetStringData(StringIdTeamHeader), teamIndex + 1));
        }

        int memberCount = members != null ? members.Count : 0;
        if (_countText != null)
        {
            _countText.SetText(memberCount);
        }

        if (_memberText != null)
        {
            _memberText.SetText(JoinMembers(members, 0, FirstGroupSize));
        }

        if (_memberText1 != null)
        {
            _memberText1.SetText(JoinMembers(members, FirstGroupSize, int.MaxValue));
        }
    }

    //# start 이상 end 미만 인덱스 범위의 멤버 이름을 \n 으로 결합. 범위 밖이면 빈 문자열.
    private static string JoinMembers(IList<string> members, int start, int end)
    {
        if (members == null || members.Count == 0)
            return string.Empty;

        int actualEnd = Mathf.Min(end, members.Count);
        if (start >= actualEnd)
            return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = start; i < actualEnd; i++)
        {
            if (i > start)
            {
                sb.Append('\n');
            }

            sb.Append(members[i]);
        }

        return sb.ToString();
    }
}
