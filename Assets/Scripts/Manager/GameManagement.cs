using GoogleMobileAds.Api;
using System.Threading.Tasks;
using UnityEngine;

public static class GameManagement
{
    public static SystemLanguage Language { get; set; }
    public static bool Initialize { get; private set; } = false;

    public static async Task InitManager()
    {
        if (Initialize)
            return;

        Initialize = true;

        await ResourceManager.Instance.Init();
        await JsonManager.Instance.Init();
        await UIManager.Instance.Init();
        AudioManager.Instance.Init();
        AdmobManager.Instance.Init();
    }

    public static void ShowBanner()
    {
        if (Initialize == false)
            return;

        AdmobManager.Instance.ShowBanner(AdPosition.Top);
        AdmobManager.Instance.ShowBanner(AdPosition.Bottom);
    }
}
