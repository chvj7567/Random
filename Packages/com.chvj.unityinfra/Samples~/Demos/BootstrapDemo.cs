using System.Threading.Tasks;
using ChvjUnityInfra;
using TMPro;
using UnityEngine;

namespace ChvjUnityInfraDemos
{
    /// <summary>
    /// 통합 초기화 데모 — ChvjUnityInfraSDK.Initialize 한 번 호출로 모든 매니저 셋업.
    /// 실제 게임에서 게임 부팅 코드(예: GameManagement.InitManager)에 쓰는 패턴.
    /// </summary>
    public class BootstrapDemo : MonoBehaviour
    {
        public enum EAudio { None, BGM, Click }
        public enum EFont { Main }

        private TMP_FontAsset _fontAsset;
        private Material _fontMaterial;

        private async void Start()
        {
            // 1. 오디오 + UI hook/Provider 설정 — SDK에 묶이지 않으므로 필요한 것만 직접 등록

            // BGM 채널로 자동 loop 처리할 enum 키 명시 (params, 0~N개)
            CHMSound.Instance.Init<EAudio>(EAudio.BGM);

            // 클릭/토글 시 자동 재생할 사운드
            CHButton.ClickSoundHook = () => CHMSound.Instance.Play(EAudio.Click);
            CHToggle.ChangeSoundHook = CHButton.ClickSoundHook;

            // CHText의 stringID → 문자열 변환 (게임 측 구현)
            CHText.StringProvider = new DemoStringProvider();

            // CHText의 stringID 모드일 때 폰트/머티리얼
            CHText.FontProvider = new DemoFontProvider(this);

            // 2. 코어 초기화 — CHMResource.Init 직후 / CHMUI.Init 이전에
            //    실행할 추가 작업(폰트 로드, 게임 데이터 로드)을 인자로 전달
            await ChvjUnityInfraSDK.Initialize(LoadGameDataAsync);

            Debug.Log("[BootstrapDemo] 초기화 완료. 모든 매니저 사용 가능.");
            // 이 시점부터:
            // - CHMResource / CHMUI / CHMSound 사용 가능
            // - CHButton / CHText hook + provider 등록됨
            // - 활성화된 옵트인 모듈(Ads/IAP)도 자동 Init 완료
        }

        private async Task LoadGameDataAsync()
        {
            // 폰트/머티리얼 로드 (FontProvider가 사용)
            var fontTask = new TaskCompletionSource<bool>();
            CHMResource.Instance.Load<TMP_FontAsset>(EFont.Main, font =>
            {
                _fontAsset = font;
                fontTask.SetResult(true);
            });
            await fontTask.Task;

            var matTask = new TaskCompletionSource<bool>();
            CHMResource.Instance.Load<Material>($"{EFont.Main}Material", mat =>
            {
                _fontMaterial = mat;
                matTask.SetResult(true);
            });
            await matTask.Task;

            // 게임 데이터 JSON 로드 등 추가 작업도 여기에
            // await MyJsonLoader.LoadAllAsync();
        }

        private class DemoStringProvider : IStringProvider
        {
            // 실제 게임에선 JsonManager.GetString(id) 등으로 i18n 처리
            public string GetString(int stringID) => $"[String#{stringID}]";
        }

        private class DemoFontProvider : IFontProvider
        {
            private readonly BootstrapDemo _owner;
            public DemoFontProvider(BootstrapDemo owner) => _owner = owner;
            public TMP_FontAsset GetFont() => _owner._fontAsset;
            public Material GetFontMaterial() => _owner._fontMaterial;
        }
    }
}
