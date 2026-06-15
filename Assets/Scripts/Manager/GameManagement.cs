using ChvjUnityInfra;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GameManagement : CHSingletonStatic<GameManagement>
{
    public SystemLanguage Language { get; set; }
    public bool Initialize { get; private set; } = false;
    public TMP_FontAsset FontAsset { get; private set; }
    public int RouletteCount { get; set; }

    public async Task InitManager()
    {
        if (Initialize)
            return;

        Initialize = true;

        //# 오디오 채널 구성 (BGM 만 loop) + 클릭 사운드 hook 등록
        CHMSound.Instance.Init<CommonEnum.EAudio>(CommonEnum.EAudio.BGM);
        CHButton.ClickSoundHook = () => CHMSound.Instance.Play(CommonEnum.EAudio.Click);

        //# 다국어/폰트 Provider 등록 (실제 조회는 JsonManager·FontAsset 로드 이후 발생)
        CHText.StringProvider = new GameStringProvider();
        CHText.FontProvider = new GameFontProvider();

        //# 리소스 초기화 → (게임 데이터/폰트 로드 + 광고 SDK 초기화) → UI 초기화 순서로 일원화
        await ChvjUnityInfraSDK.Initialize(async () =>
        {
            await JsonManager.Instance.Init();
            //# Admob 은 패키지 전환 보류 — 기존 자체 AdmobManager 유지. SDK 초기화(MobileAds.Initialize) 복구
            AdmobManager.Instance.Init();
            await LoadFontAsset();
        });
    }

    private async Task LoadFontAsset()
    {
        TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

        CHMResource.Instance.Load<TMP_FontAsset>(CommonEnum.EFont.Jua, (font) =>
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

        AdmobManager.Instance.ShowBanner(GoogleMobileAds.Api.AdPosition.Bottom);
    }
}
