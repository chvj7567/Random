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
        CoinFlip,
    }

    //# 동전 스프라이트 에셋 키 — 값명 = 에셋 파일명 (Coin.png). 앞/뒷면은 단일 스프라이트 + 색 틴트로 구분.
    public enum ECoin
    {
        Coin,
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
