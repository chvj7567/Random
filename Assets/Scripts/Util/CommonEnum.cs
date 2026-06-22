using UnityEngine;

public class CommonEnum
{
    public enum EScene
    {
        Start,
        Random
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
        Lotto1,
        Lotto2,
    }

    //# 동전 스프라이트 에셋 키 — 값명 = 에셋 파일명 (Coin.png). 앞/뒷면은 단일 스프라이트 + 색 틴트로 구분.
    public enum ECoin
    {
        Coin,
    }

    //# 메뉴 카드 아이콘 에셋 키 — 값명 = Sprite 폴더 파일명 (Rule 03 §2). Dark UI 원본을 식별자 안전 이름으로 복사.
    //# MenuNumber←White A1 / MenuFood←White Apple / MenuShuffle←White Cycle / Coin←기존 Coin.png.
    public enum EMenuIcon
    {
        MenuNumber,
        MenuFood,
        MenuShuffle,
        Coin,
        MenuLotto1,
        MenuLotto2,
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
