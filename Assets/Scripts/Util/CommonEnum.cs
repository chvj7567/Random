using UnityEngine;

public class CommonEnum
{
    public enum EScene
    {
        Start,
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

    public enum ERouletteMenu
    {
        Menu = 0,
        RandomNumber,
        RandomFood,
        CustomRandom,
    }

    public enum ELottoMenu
    {
        Menu = 0,
        Lotto,
        Lotto2,
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
        String,
        Country,
        Animal,
    }

    public enum EFont
    {
        Jua
    }
}
