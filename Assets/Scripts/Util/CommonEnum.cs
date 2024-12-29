using UnityEngine;

public class CommonEnum
{
    public enum EScene
    {
        Roulette,
        Lotto
    }

    public enum EUI
    {
        UILotto,
        UILoading,
        UIAlarm,
        UIRoulette,
    }

    public enum EMenu
    {
        Menu = 0,
        RandomNumber,
        RandomFood,
        CustomRandom,
    }

    public enum EAudio
    {
        None = 0,
        BGM,
        Click,
    }

    public enum ERoulette
    {
        Circle,
        Scroll,
    }

    public enum EJson
    {
        Food,
        String
    }
}
