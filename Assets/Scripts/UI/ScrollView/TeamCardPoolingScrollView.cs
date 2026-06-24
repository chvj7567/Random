using ChvjUnityInfra;
using System.Collections.Generic;
using UnityEngine;

//# 팀 나누기 팀 카드 1장의 데이터 (기획서 §4.1).
public class TeamSplitTeamData
{
    public int TeamIndex;
    public IList<string> Members;
    public Color Color;
}

//# CHPoolingScrollView 구현체 — InitItem 에서 카드 바인딩, 2열 고정 높이 (Rule 03 §3).
public class TeamCardPoolingScrollView : CHPoolingScrollView<TeamCard, TeamSplitTeamData>
{
    public override void InitItem(TeamCard item, TeamSplitTeamData data, int index)
    {
        item.Bind(data.TeamIndex, data.Members, data.Color);
    }

    public override void InitPoolingObject(TeamCard item) { }
}
