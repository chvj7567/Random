using ChvjUnityInfra;

//# CHText 의 stringID 조회를 게임 JsonManager 다국어 테이블에 위임하는 어댑터.
public class GameStringProvider : IStringProvider
{
    public string GetString(int stringID)
    {
        return JsonManager.Instance.GetStringData(stringID);
    }
}
