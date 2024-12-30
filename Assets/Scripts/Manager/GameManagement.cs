using GoogleMobileAds.Api;
using System.Threading.Tasks;
using UnityEngine;

public class GameManagement : StaticSingletone<GameManagement>
{
    public SystemLanguage Language { get; set; }
    public bool Initialize { get; private set; } = false;

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
    }

    public void ShowBanner()
    {
        if (Initialize == false)
            return;

        AdmobManager.Instance.ShowBanner(AdPosition.Top);
        AdmobManager.Instance.ShowBanner(AdPosition.Bottom);
    }
}
