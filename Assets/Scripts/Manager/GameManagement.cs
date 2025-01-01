using GoogleMobileAds.Api;
using JetBrains.Annotations;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GameManagement : SingletoneStatic<GameManagement>
{
    public SystemLanguage Language { get; set; }
    public bool Initialize { get; private set; } = false;
    public TMP_FontAsset FontAsset { get; private set; }

    public async Task InitManager()
    {
        if (Initialize)
            return;

        Initialize = true;

        await ResourceManager.Instance.Init();
        await JsonManager.Instance.Init();
        await UIManager.Instance.Init();
        AudioManager.Instance.Init();
        AdmobManager.Instance.Init();

        TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

        ResourceManager.Instance.LoadFont(CommonEnum.EFont.Jua, (font) =>
        {
            FontAsset = font;
            taskCompletionSource.SetResult(true);
        });

        await taskCompletionSource.Task;
    }

    public void ShowBanner()
    {
        if (Initialize == false)
            return;

        AdmobManager.Instance.ShowBanner(AdPosition.Top);
        AdmobManager.Instance.ShowBanner(AdPosition.Bottom);
    }
}
