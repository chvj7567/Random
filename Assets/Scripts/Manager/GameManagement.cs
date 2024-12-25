using GoogleMobileAds.Api;
using System.Threading.Tasks;

public static class GameManagement
{
    public static bool Initialize = false;

    public static async Task InitManager()
    {
        if (Initialize)
            return;

        Initialize = true;

        await ResourceManager.Instance.Init();
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
