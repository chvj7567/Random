using ChvjUnityInfra;
using TMPro;
using UnityEngine;

//# CHText 의 stringID 모드 폰트/머티리얼을 게임 GameManagement.FontAsset 에 위임하는 어댑터.
public class GameFontProvider : IFontProvider
{
    public TMP_FontAsset GetFont()
    {
        return GameManagement.Instance.FontAsset;
    }

    public Material GetFontMaterial()
    {
        TMP_FontAsset font = GameManagement.Instance.FontAsset;
        if (font == null)
            return null;

        return font.material;
    }
}
