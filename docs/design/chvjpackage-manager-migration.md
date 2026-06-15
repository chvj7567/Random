# ChvjPackage 매니저 인프라 마이그레이션 계획서

> 본 문서는 **순수 인프라 리팩터의 마이그레이션 계획서**다. 기획/밸런스 결정이 아니라, Random 의 자체 중복 인프라를 임베드 패키지 `com.chvj.unityinfra`(namespace `ChvjUnityInfra`, v2.0.5) 의 매니저로 교체하고 자체 사본을 제거하기 위한 단계별 계획이다.
> 코드(`.cs`)는 한 줄도 포함하지 않는다 — 변환은 모두 개념/의사코드 수준이며, 실제 구현은 gameplay-programmer 가 담당한다.

## § 헤더

- **목표** — Random 의 전역 네임스페이스 자체 인프라(ResourceManager·UIManager·AudioManager·AdmobManager·UIBase·UIArg·CHScrollView·ButtonEx·TextEx·Singletone* )를 `ChvjUnityInfra` 패키지 매니저로 교체하고, 교체 완료 후 자체 사본을 삭제한다.
- **검증 가설** — (밸런스가 아닌 인프라 리팩터이므로) "각 단계 후 컴파일이 항상 그린으로 유지되고, 기존 게임 동작(룰렛 회전·로또 조회·광고 노출·다국어 텍스트·사운드)이 회귀 없이 보존되는가"가 검증 대상.
- **현재 단계 범위 적합성** — `project.md` `stage: 운영/유지보수`. 컨셉서 없음(유틸 앱). 본 작업은 사용자 요구(프롬프트)를 직접 입력으로 삼은 인프라 리팩터로 운영/유지보수 단계 범위 내.
- **핵심 메커니즘** — 매니저 1개씩 교체(Resource → Audio → Pool → UI → Ads), 각 단계마다 컴파일 그린 유지. 패키지 타입과 자체 타입의 이름 충돌(`UIBase`/`UIArg`/`PoolingScrollView*` enum)은 단계적으로 `using ChvjUnityInfra;` 도입 + 자체 타입 제거 순으로 해소.

---

## 0. 사전 조사 결과 (현 상태 팩트)

| 항목 | 사실 | 출처 |
|---|---|---|
| 게임 코드 어셈블리 | `Assembly-CSharp` (게임 코드에 asmdef 없음) | `Assets/` 하위 asmdef 미존재 확인 |
| 패키지 Core asmdef | `com.chvj.unityinfra`, `autoReferenced: true`, `rootNamespace: ChvjUnityInfra` | `Runtime/com.chvj.unityinfra.asmdef` |
| Core 사용 가능 여부 | `autoReferenced: true` → Assembly-CSharp 에서 `ChvjUnityInfra` 타입 **즉시 사용 가능** (manifest/asmdef 추가 불필요) | 동일 |
| Ads asmdef | `com.chvj.unityinfra.ads`, `defineConstraints: ["UNITY_INFRA_ADS"]`, references `GoogleMobileAds.Api` | `Runtime/Ads/com.chvj.unityinfra.ads.asmdef` |
| **UNITY_INFRA_ADS 정의 여부** | **정의 안 됨** (ProjectSettings scriptingDefineSymbols 에 `ENABLE_JSON_CATALOG;DOTWEEN` 만 존재) | `ProjectSettings/ProjectSettings.asset` L827~ |
| ⇒ `CHMAdmob` 현 상태 | **현재 컴파일되지 않음/존재하지 않음** — Ads 단계 전에 define 활성화 필요 | 위 두 항목 결합 |
| 설치된 패키지 모듈 | Core·Resource·Pool·Audio·UI·Ads (Iap·Social 은 미설치 SDK 로 제거됨) | `project.md` / Glob 결과 |
| Addressables 라벨 | 자체 ResourceManager 와 CHMResource 모두 기본 라벨 `"Resource"` 사용 — **일치** | `ResourceManager.cs` L13 / `CHMResource.cs` L14 |

---

## 1. 매핑표 — Random 자체 타입 → 패키지 대응

### 1-A. 매니저 (싱글톤)

| Random 자체 | 패키지 대응 | 대응 종류 | 비고 |
|---|---|---|---|
| `ResourceManager : SingletoneStatic` | `CHMResource : CHSingletonStatic` | 1:1 (API 형태 다름) | 라벨 `"Resource"` 동일. 캐싱/Unload 는 패키지가 추가 제공 |
| `UIManager : SingletoneMonoBehaviour` | `CHMUI : CHSingleton` | 1:1 (API 형태 다름) | UI 캐싱·ESC 닫기·캔버스 자동 확보. 단, ESC 는 `ENABLE_LEGACY_INPUT_MANAGER` 일 때만(§6 리스크) |
| `AudioManager : SingletoneMonoBehaviour` | `CHMSound : CHSingleton` | 1:1 (초기화/재생 모델 다름) | 효과음이 `PlayOneShot` 으로 변경됨(동작 변화, §2-C) |
| `AdmobManager : SingletoneStatic` | `CHMAdmob : CHSingletonStatic` | 1:1 (단, **define 게이트**) | `UNITY_INFRA_ADS` 필요. `AcquireReward`/`CloseAD` 가 `Action` 필드 → `event Action` 으로 변경 |
| `GameManagement : SingletoneStatic` | **대응 없음** | 유지 | 게임 부팅 오케스트레이션 + 언어/폰트/룰렛 카운트 상태. 패키지 `ChvjUnityInfraSDK.Initialize` 로 내부 호출 재배선만 |
| `JsonManager : SingletoneStatic` | **대응 없음** | 유지 | 게임 도메인 데이터(String/Country/Animal). 단 `IStringProvider` 어댑터로 CHText 에 연결(§2-D) |

### 1-B. UI / 유틸 기반 타입

| Random 자체 | 패키지 대응 | 대응 종류 | 비고 |
|---|---|---|---|
| `UIBase`(전역) | `ChvjUnityInfra.UIBase` | 1:1 (시그니처 다름) | `InitUI(EUI,UIArg)` → `Init(Enum)`+`InitUI(UIArg)` 분리. `UIType` 필드→프로퍼티(Enum). **abstract** 가 됨 |
| `UIArg`(전역) | `ChvjUnityInfra.UIArg` | 1:1 | 빈 베이스. 파생 클래스(UIAlarmArg/UILottoArg/UIRouletteArg)는 패키지 UIArg 상속으로 변경 |
| `ButtonEx`(`Button` 요구) | `CHButton`(`Button` 요구) | 1:1 | `Interatable`(오타) → `Interactable`. `OnClick` 내부 사운드 호출이 `ClickSoundHook` 위임으로 변경(§2-E). `[MovedFrom("ButtonEx")]` 보유 |
| `TextEx`(`TMP_Text` 요구) | `CHText`(`TMP_Text` 요구) | 1:1 | stringID 조회를 `IStringProvider`/`IFontProvider` 로 위임. 기본 `_stringID` 가 `1`→`-1` 로 다름(§2-D 주의). `[MovedFrom("TextEx")]` 보유 |
| `CHScrollView<TItem,TData>`(전역, 풀링 스크롤) | `CHPoolingScrollView<TItem,TData>`(`ChvjUnityInfra`) | 1:1 (거의 동일 구현) | **이름이 다름**(`CHScrollView`→`CHPoolingScrollView`). 내부 보호 필드 `_liPoolItem`→`liPoolItem`, `_liData`→`liData` |
| `SingletoneMonoBehaviour<T>` | `CHSingleton<T>` | 1:1 | 동일 패턴. `OnDestroy` 봉인 → `OnApplicationQuit` 봉인으로 변경 |
| `SingletoneStatic<T>` | `CHSingletonStatic<T>` | 1:1 | 동일 패턴 |
| `Extension`(`RotateZRoation`/`RotateXYPosition`/`Spin` 등) | **대응 없음** | 유지 | 게임 도메인 확장 메서드(룰렛 회전 등). 패키지 무관 |
| `CommonEnum`(EUI/EScene/EAudio/EJson/EFont/...) | **대응 없음**(게임 소유) | 유지 | 패키지는 `Enum` 베이스로 받음 → 게임 enum 그대로 사용. Rule 02 §8 위치 정책만 점검 |

### 1-C. enum 충돌 주의 (이름 동일, 네임스페이스 다름)

| 전역(자체 `CHScrollView.cs` 정의) | 패키지(`ChvjUnityInfra`) | 처리 |
|---|---|---|
| `PoolingScrollViewDirection` | `ChvjUnityInfra.PoolingScrollViewDirection` | 자체 `CHScrollView.cs` 삭제 시 자연 해소. 삭제 전 `using ChvjUnityInfra;` 도입하면 **CS0104 모호 참조** 발생 → §4 순서로 회피 |
| `PoolingScrollViewAlign` | `ChvjUnityInfra.PoolingScrollViewAlign` | 동일 |
| `PoolingScrollViewScrollingDirection` | `ChvjUnityInfra.PoolingScrollViewScrollingDirection` | 동일 |
| `PoolingScrollViewItem<TItem>` | `ChvjUnityInfra.PoolingScrollViewItem<TItem>` | 동일 |

---

## 2. API 차이 (delta) — 자체 → 패키지 호출 변환 (개념/의사코드)

### 2-A. Resource

| 자체 API | 패키지 API |
|---|---|
| `ResourceManager.Instance.Init()` → `Task<bool>` | `CHMResource.Instance.Init(label="Resource")` → `Task<bool>` (동시호출 race 방지 내장) |
| `LoadUI(EUI, Action<GameObject>)` (내부 Instantiate) | `Instantiate<GameObject>(Enum, Action<T>)` 또는 `InstantiateAsync<GameObject>(Enum)` |
| `LoadGameObject(name, cb)` (내부 Instantiate) | `Instantiate<GameObject>(string, cb)` |
| `LoadAudio(EAudio, Action<AudioClip>)` (로드만) | `Load<AudioClip>(Enum, cb)` 또는 `LoadAsync<AudioClip>(Enum)` |
| `LoadJson(EJson, Action<TextAsset>)` | `Load<TextAsset>(Enum, cb)` / `LoadAsync<TextAsset>(Enum)` |
| `LoadFont(EFont, Action<TMP_FontAsset>)` | `Load<TMP_FontAsset>(Enum, cb)` / `LoadAsync<TMP_FontAsset>(Enum)` |

주의: 자체 `LoadUI`/`LoadGameObject` 는 **로드 후 Instantiate 까지** 수행한다. 패키지에서는 로드만 하는 `Load<T>` 와 Instantiate 까지 하는 `Instantiate<T>` 가 분리되어 있으므로, "프리팹을 실제로 생성하던" 호출은 `Instantiate<T>` 로, "에셋만 가져오던"(AudioClip/TextAsset/Font) 호출은 `Load<T>` 로 매핑한다.

변환 예(의사코드):
```
// (자체) UI 프리팹 인스턴스 생성
ResourceManager.Instance.LoadUI(EUI.UIAlarm, go => { ... });
// (패키지)
CHMResource.Instance.Instantiate<GameObject>(EUI.UIAlarm, go => { ... });

// (자체) 폰트 에셋 로드
ResourceManager.Instance.LoadFont(EFont.Jua, font => { ... });
// (패키지)
CHMResource.Instance.Load<TMP_FontAsset>(EFont.Jua, font => { ... });
```

### 2-B. UI 매니저 + UIBase

자체 `UIBase.InitUI(EUI uiType, UIArg arg)` 한 메서드가 ① 타입 세팅 ② 배경/뒤로 버튼 바인딩 ③ 파생 초기화를 모두 한다.
패키지는 **① ②를 `internal void Init(Enum)`** 가, **③을 `public abstract void InitUI(UIArg)`** 가 담당하도록 분리했다. `CHMUI.ActivateUI` 가 `Init(uiType)` → `InitUI(arg)` 순으로 호출한다.

| 자체 | 패키지 |
|---|---|
| `UIManager.Instance.ShowUI(EUI, UIArg, Action<UIBase>)` | `CHMUI.Instance.ShowUI(Enum, UIArg, Action<UIBase>)` (또는 `ShowUIAsync`) |
| `UIManager.Instance.CloseUI(uiBase, reuse)` | `CHMUI.Instance.CloseUI(uiBase, reuse=false)` (기본 reuse 값 다름 주의) |
| `UIManager.Instance.RemoveCashingUI(EUI)` | `CHMUI` 내부 private — 외부 호출 불필요(`CloseUI(reuse:false)` 가 캐시 제거) |
| `UIManager.Instance.SetMainUI(IRouletteBackButton)` / `ResetMainUI()` | **대응 없음** — 게임 도메인 동작(§6 리스크: 룰렛 뒤로가기) |
| `_mainRouletteUI?.Close()` (ESC 시) | `CHMUI` 의 ESC 는 최상위 UI 만 닫음 → 메인 룰렛 뒤로가기 로직은 게임 측 보존 필요 |
| `closeDisposable`(UniRx `CompositeDisposable`) | 패키지 `ChvjUnityInfra.CompositeDisposable`(자체 경량 구현, UniRx 비의존) |

파생 UI 변환 예(의사코드 — UIAlarm):
```
// (자체)
public override void InitUI(CommonEnum.EUI uiType, UIArg arg) {
    base.InitUI(uiType, arg);
    _arg = arg as UIAlarmArg;
    _alarmText.text = _arg.alarmText;
}
// (패키지) — uiType 인자 사라짐. UIType 은 프로퍼티로 조회. abstract override.
public override void InitUI(UIArg arg) {
    _arg = arg as UIAlarmArg;
    _alarmText.text = _arg.alarmText;  // base 호출 불필요(배경/뒤로 버튼은 Init 이 처리)
}
```
> UIAlarm 의 `_alarmText` 는 현재 `TMP_Text` 직접 참조다. Rule 03 §3 상 `CHText` 로 교체 권장이나, **본 마이그레이션 범위는 매니저 인프라 교체**이므로 위젯 래퍼 교체는 본 계획서 범위 외(후속 작업으로 분리). 본 단계에서는 `TMP_Text` 직접 세팅을 유지해 동작 보존을 우선한다.

### 2-C. Audio (동작 변화 있음)

자체 `AudioManager` 는 `Init()` 에서 enum 길이만큼 `AudioSource` 를 만들고, 효과음도 `Play()`(단일 source) 로 재생한다. 패키지 `CHMSound` 는:
- `Init<TAudio>(params bgmKeys)` 제네릭으로 **enum 타입 주입** 필요. `EAudio.BGM` 을 bgmKey 로 명시해야 loop 채널이 됨.
- `None`/`Max` 항목은 AudioSource 를 만들지 않음 → 현 `EAudio.None=0` 과 정합(자체는 index 0 에 빈 AudioSource 를 넣던 차이가 흡수됨).
- **효과음은 `PlayOneShot`** 으로 재생(동시 재생 허용) — 자체는 `audioSource.Play()`(겹침 시 끊김). **동작 변화**: 빠른 연속 클릭음이 겹쳐 들릴 수 있음. 룰렛/로또 앱 특성상 허용 가능하나 §6 에 회귀 항목으로 기록.
- 볼륨이 `PlayerPrefs` 영구 저장으로 변경(자체는 메모리 기본 1f/0.5f). 기존 사용자에게 0.5 default 동일하므로 첫 실행 동작 동일.

| 자체 | 패키지 |
|---|---|
| `AudioManager.Instance.Init()` | `CHMSound.Instance.Init<CommonEnum.EAudio>(CommonEnum.EAudio.BGM)` |
| `AudioManager.Instance.Play(EAudio, pitch)` | `CHMSound.Instance.Play(Enum, pitch)` |
| `SetBGMVolume(v)` / `SetEffectVolume(v)` | 동명 메서드 존재 (추가로 `SetMasterVolume`) |
| `BgmVolume`/`EffectVolume`/`Ratio` | 동명 프로퍼티 존재 (`Ratio` = `MasterVolume` 별칭) |

### 2-D. 다국어 텍스트 (TextEx → CHText) + JsonManager 어댑터

`TextEx` 는 `JsonManager.Instance.GetStringData(id)` 와 `GameManagement.Instance.FontAsset` 를 직접 호출한다. `CHText` 는 이를 정적 `IStringProvider`/`IFontProvider` 로 추상화했다. 따라서 **JsonManager/GameManagement 를 감싸는 어댑터 2개를 게임 측에 두고 부팅 시 1회 주입**한다.

```
// 부팅 1회 (의사코드)
CHText.StringProvider = <JsonManager 를 감싼 IStringProvider 구현>;
CHText.FontProvider   = <GameManagement.FontAsset 를 감싼 IFontProvider 구현>;
```
주의(default stringID 차이): 자체 `TextEx._stringID` 기본값 `1`, 패키지 `CHText._stringID` 기본값 `-1`. `-1` 은 "stringID 미사용(직접 SetText)" 의미. **프리팹에 박힌 `_stringID` 값이 그대로 직렬화되어 넘어오므로** (`[MovedFrom("TextEx")]` 가 컴포넌트 매핑) 기존에 의도적으로 `1` 을 쓰던 라벨은 보존된다. 단, 컴포넌트 신규 추가 시 기본값이 `-1` 임을 인지.

### 2-E. 버튼 (ButtonEx → CHButton)

자체 `ButtonEx.OnClick` 내부에서 `AudioManager.Instance.Play(EAudio.Click)` 을 직접 호출한다. `CHButton` 은 이를 정적 `ClickSoundHook` 으로 위임 → 부팅 시 1회 등록 필요.
```
// 부팅 1회 (의사코드)
CHButton.ClickSoundHook  = () => CHMSound.Instance.Play(CommonEnum.EAudio.Click);
CHToggle.ChangeSoundHook = CHButton.ClickSoundHook;   // 토글 사용 시
```
`Interatable`(자체 오타) → `Interactable`(패키지) 로 프로퍼티명이 바뀌므로 호출부 점검(현재 호출부에 `Interatable` 사용 0건 — Grep 으로 재확인 필요).

### 2-F. Ads (define 게이트 + event 변화)

| 자체 | 패키지 |
|---|---|
| `AdmobManager.Instance.Init()` | `CHMAdmob.Instance.Init()` (UMP 동의/ATT/RequestConfiguration 추가). **단 `UNITY_INFRA_ADS` 정의 + GoogleMobileAds.Api 어셈블리 해결 필요** |
| 광고 ID 코드 하드코딩 | `AdConfig` ScriptableObject(`Assets/Resources/ChvjUnityInfra/AdConfig.asset`) 로 이동. Tools 메뉴로 생성 |
| `ShowBanner(AdPosition)` | 동일 (추가 `HideBanner`) |
| `ShowInterstitialAd()` / `ShowRewardedAd()` | 동일 (추가 `IsInterstitialReady`/`IsRewardedReady`) |
| `public Action AcquireReward;` `public Action CloseAD;`(필드 대입) | `public event Action AcquireReward;` `public event Action CloseAD;` — **`=` 대입 불가, `+=`/`-=` 만 가능**. 기존 대입 호출부 변환 필요 |
| `MobileAds.Initialize` 즉시 | UMP→Init 비동기 체인. `CHMAdmobBootstrap` 이 `RuntimeInitializeOnLoadMethod` 로 Init 자동 등록(SDK.Initialize 경유) |

> 부팅 라인 `RouletteScene` 의 `GameManagement.Instance.ShowBanner()` → 내부적으로 `CHMAdmob.Instance.ShowBanner(...)` 로 재배선. 광고 ID 는 코드에서 `AdConfig.asset` 으로 이전 — 기존 프로덕션 배너/전면 ID 2종을 AdConfig 에 입력해야 동작 보존(rewarded 는 기존 빈 문자열).

### 2-G. 통합 부팅 (GameManagement.InitManager 재배선)

자체 `GameManagement.InitManager` 순서:
```
ResourceManager.Init → JsonManager.Init → UIManager.Init → AudioManager.Init → AdmobManager.Init → Font 로드
```
패키지 권장 순서(`ChvjUnityInfraSDK.Initialize` 활용):
```
CHMSound.Init<EAudio>(EAudio.BGM)          // 동기, 사전
CHButton.ClickSoundHook / CHToggle.ChangeSoundHook 등록
CHText.StringProvider / FontProvider 등록  // (JsonManager·FontAsset 로드 이후여야 의미 있음 → afterResourceInit 안에서)
await ChvjUnityInfraSDK.Initialize(afterResourceInit: async () => {
    await JsonManager.Init();              // CHMResource 의존 게임 데이터
    await <FontAsset 로드>;
});                                         // 내부: CHMResource.Init → afterResourceInit → CHMUI.Init → AdsInitHook
```
`GameManagement` 자체는 존속하되, 그 안의 매니저 호출 5줄을 위 패키지 호출로 치환한다.

---

## 3. 영향 범위 — 교체 대상 호출부

### 3-A. 매니저 참조 호출부 (`*.Instance` 사용 건수, Grep 기준)

| 파일 | 매니저 참조 건수 | 주된 사용 |
|---|---|---|
| `Scene/RandomExampleScene.cs` | 13 | Resource/UI/Audio/Json/GameManagement 다수 |
| `Manager/GameManagement.cs` | 7 | InitManager 오케스트레이션(§2-G) |
| `Util/UIRoulette.cs` | 6 | UI/Json/Admob/GameManagement |
| `Scene/RouletteScene.cs` | 5 | GameManagement.ShowBanner / UIManager.Set·ResetMainUI |
| `Manager/JsonManager.cs` | 4 | ResourceManager.LoadJson (자체→패키지 시 §2-A) |
| `Util/TextEx.cs` | 4 | Json/GameManagement (→ CHText 가 Provider 로 대체) |
| `Scene/RandomNumberScene.cs` | 3 | UI/Audio |
| `Scene/StartScene.cs` | 3 | GameManagement.InitManager / Language |
| `Manager/UIManager.cs` | 2 | (자체 삭제 대상) |
| `Scene/LottoScene.cs` | 2 | UI/Resource |
| `Manager/AudioManager.cs` 외 1 | 각 1~2 | 자체 정의/내부 |
| `Util/ButtonEx.cs` | 1 | AudioManager.Play (→ ClickSoundHook) |
| `Prefab/Capture.cs` | 1 | UIManager.ShowUI(UIAlarm) |
| `Scene/CustomRandomScene.cs` · `Scene/LottoMenuScene.cs` | 각 1 | UI |

매니저 참조 총 **56건 / 16파일**.

### 3-B. 기반 타입 상속/사용 호출부 (`UIBase`/`UIArg`/`CHScrollView`/`ButtonEx`/`TextEx`/`Singletone*` 등 59건 / 27파일)

| 분류 | 파일 |
|---|---|
| `UIBase` 파생 | `UI/UIAlarm.cs`, `UI/UILoading.cs`, `UI/UILotto.cs`, `Util/UIRoulette.cs` |
| `UIArg` 파생 | `UIAlarmArg`(UIAlarm.cs), `UILottoArg`(UILotto.cs), `UIRouletteArg`(UIRoulette.cs) |
| `CHScrollView<,>` 파생 | `UI/ScrollView/CustomScrollView.cs`, `LottoScrollView.cs`, `RouletteScrollView.cs` (+ 각 Item: `*ScrollViewItem.cs` 는 TItem) |
| `ButtonEx` 참조 | `Scene/RouletteScene.cs`(Menu.buttonEx), `UI/UILotto.cs`(_topButton/_bottomButton), `Prefab/Capture.cs`, `Scene/RandomExampleScene.cs` 등 |
| `TextEx` 참조 | 다수 Scene/UI (텍스트 라벨) |
| `Singletone*` 상속 | 자체 6개 매니저 — 패키지 매니저로 교체되며 함께 사라짐 |

> `*ScrollViewItem.cs`(Cell) 들은 `MonoBehaviour` 파생일 뿐 기반 타입 교체 영향이 적다. 영향은 주로 ScrollView(부모) 의 베이스 클래스명 변경(`CHScrollView`→`CHPoolingScrollView`).

### 3-C. 씬/프리팹 자산 영향 (코드 외)

- 자체 `UIBase`/`ButtonEx`/`TextEx`/`CHScrollView` 파생 컴포넌트가 **프리팹/씬에 직렬화로 부착**되어 있다. 클래스 교체 시 컴포넌트 참조(GUID/스크립트) 가 깨질 수 있음 → `[MovedFrom]` 가 있는 `CHButton`(ButtonEx)·`CHText`(TextEx) 는 Unity 가 자동 재매핑하지만, `UIBase`/`CHScrollView` 는 `[MovedFrom]` 이 없어 **수동 재할당 또는 스크립트 GUID 매핑 필요**(§6 리스크).

---

## 4. 마이그레이션 순서 (각 단계 후 컴파일 그린 유지)

> 핵심 원칙: ① 자체 타입과 패키지 타입이 **공존하는 중간 상태에서 `using ChvjUnityInfra;` 를 광역 도입하지 않는다**(이름 충돌 회피). ② 매니저 1개 교체 → 컴파일 확인 → 다음. ③ 자체 파일 삭제는 그 타입을 참조하는 호출부가 0건이 된 직후에 한다.

### 단계 0 — 준비 (코드 동작 불변)
- `CommonEnum` 위치/네이밍이 Rule 02 §8 / Rule 03 §2 정합인지 점검(에셋 파일명 = enum 값명). **변경 없음 권장** — 현 enum 값명과 Addressable 주소 일치 여부만 확인.
- `IStringProvider`/`IFontProvider` 게임 측 어댑터 2개를 **신규 추가**(자체 인프라는 아직 그대로). 컴파일 그린.

### 단계 1 — Resource (`ResourceManager` → `CHMResource`)
- 변환 대상: 모든 `ResourceManager.Instance.Load*` 호출(§2-A 매핑). Instantiate 계열은 `Instantiate<T>`, 에셋 로드 계열은 `Load<T>`.
- `JsonManager` 내부의 `ResourceManager.Instance.LoadJson` 도 이 단계에서 `CHMResource.Instance.Load<TextAsset>` 로 치환.
- `GameManagement.InitManager` 의 `ResourceManager.Init` → `CHMResource.Init`(또는 SDK.Initialize 부분 도입).
- 호출부 0건 확인 후 **`ResourceManager.cs` 삭제**. 컴파일 그린.

### 단계 2 — Audio (`AudioManager` → `CHMSound`)
- 부팅에 `CHMSound.Init<EAudio>(EAudio.BGM)` 추가, 모든 `AudioManager.Instance.Play/SetVolume` → `CHMSound`.
- `ButtonEx` 내부의 `AudioManager.Play(Click)` 호출은 단계 5(UI 위젯)로 미루되, 임시로 `CHMSound` 직접 호출로 바꿔 컴파일만 유지(또는 ClickSoundHook 선등록). 권장: 이 단계에서 `ClickSoundHook` 등록을 먼저 넣고 ButtonEx 는 단계 5 에서 CHButton 으로 교체.
- 호출부 0건 확인 후 **`AudioManager.cs` 삭제**. 컴파일 그린.

### 단계 3 — Pool (`CHMPool` 신규 도입 — 자체 대체 없음)
- 현재 Random 은 풀 매니저가 없고 `Instantiate`/`Destroy` 를 직접 쓴다(`UIRoulette` 의 룰렛 셀/라인 복제, `ResourceManager.LoadGameObject` 의 Instantiate, `CHScrollView.CreatePoolingObject`). Rule 03 §4 상 런타임 스폰은 `CHMPool` 권장.
- **범위 판단**: 룰렛 셀 복제(`UIRoulette`)와 스크롤뷰 풀은 동작이 안정적이고 교체 위험이 크다. 본 마이그레이션은 "매니저 인프라 교체"가 목표이므로, **CHMPool 전면 적용은 후속 작업으로 분리**하고 본 계획서에서는 (a) `CHMPool.Init()` 부팅 추가만 하거나 (b) 생략한다. 단계 3 은 **선택적**으로 표기(컴파일 영향 없음).
- 결정: 본 계획서는 **단계 3 을 "CHMPool 부팅 초기화만(Init) 추가, 스폰 코드 전면 교체는 후속"** 으로 좁힌다. 사유 — 룰렛 회전 시각 회귀 위험 최소화.

### 단계 4 — UI (`UIManager`/`UIBase`/`UIArg` → `CHMUI`/`ChvjUnityInfra.UIBase`/`UIArg`)
> 이름 충돌이 가장 큰 단계. 자체 `UIBase`/`UIArg` 와 패키지 동명 타입 공존 불가 → **한 커밋 안에서 일괄 전환**해야 함(파생 UI 4개 + Arg 3개 + 호출부).
- 부팅에 `CHMUI.Init()`(또는 SDK.Initialize 에 포함) 배선.
- 파생 UI(`UIAlarm`/`UILoading`/`UILotto`/`UIRoulette`): 베이스를 패키지 `UIBase` 로, `InitUI(EUI,UIArg)` → `InitUI(UIArg)` 시그니처 변경(§2-B). `UIType` 사용처는 프로퍼티로.
- Arg 파생: 패키지 `UIArg` 상속으로. (Rule 03 §5 — Arg 는 해당 UIBase 파일 상단 유지)
- 호출부: `UIManager.Instance.ShowUI/CloseUI` → `CHMUI.Instance.*`. `SetMainUI/ResetMainUI`(룰렛 뒤로가기)는 **게임 측 로직으로 분리 보존**(§6).
- `closeDisposable` 타입을 패키지 `CompositeDisposable` 로(UniRx 의존 제거 또는 병존 — §6).
- 호출부 0건 확인 후 **`UIManager.cs` + 자체 `UIBase.cs`(UIArg 포함) 삭제**. 컴파일 그린.
- 같은 커밋 또는 직후: `CHScrollView<,>` → `CHPoolingScrollView<,>` 베이스 교체(3 ScrollView), 그 후 **`Util/CHScrollView.cs` 삭제**(전역 PoolingScrollView* enum 충돌 동시 해소).

### 단계 5 — UI 위젯 (`ButtonEx`/`TextEx` → `CHButton`/`CHText`)
- `[MovedFrom]` 덕에 프리팹 컴포넌트는 자동 재매핑되지만, **코드 참조 타입명**은 수동 변경(`ButtonEx`→`CHButton`, `TextEx`→`CHText`).
- `ClickSoundHook`/`StringProvider`/`FontProvider` 가 이미 단계 0/2 에서 등록되어 있어야 사운드/다국어 동작 보존.
- 호출부 0건 확인 후 **`Util/ButtonEx.cs` + `Util/TextEx.cs` 삭제**. 컴파일 그린.

### 단계 6 — Singletone 베이스 정리
- 자체 매니저가 모두 사라졌으므로 `Util/SingletoneMonoBehaviour.cs` / `Util/SingletoneStatic.cs` 참조 0건 확인.
- 게임 측에 남은 자체 싱글톤(`GameManagement`/`JsonManager`)은 `CHSingletonStatic<T>` 로 베이스 교체 후 **`Singletone*.cs` 삭제**. 컴파일 그린.

### 단계 7 — Ads (`AdmobManager` → `CHMAdmob`) — define 선행
> **선행 필수**: Tools/ChvjUnityInfra/Settings 에서 "Use Admob" 체크 → `UNITY_INFRA_ADS` 정의 → Ads 어셈블리 컴파일. `GoogleMobileAds.Api` 어셈블리 해결 확인(§6 리스크).
- `AdConfig.asset` 생성 + 기존 배너/전면 프로덕션 ID 2종 입력.
- `AdmobManager.Instance.*` → `CHMAdmob.Instance.*`. `AcquireReward`/`CloseAD` 대입(`=`) 호출부를 `+=` 로 변경(§2-F).
- `GameManagement.ShowBanner` 내부 → `CHMAdmob.Instance.ShowBanner`.
- `UIRoulette.Close` 의 `AdmobManager.Instance.ShowInterstitialAd()` → `CHMAdmob`.
- 호출부 0건 확인 후 **`AdmobManager.cs` 삭제**. 컴파일 그린.

### 단계 8 — 최종 정리
- `GameManagement.InitManager` 가 SDK.Initialize 기반으로 일원화됐는지 확인.
- 남은 자체 인프라 잔재 0건 Grep 확인. 씬/프리팹 미싱 스크립트 0건 확인.

---

## 5. 삭제 대상 (교체 완료 후)

| 삭제 파일 | 삭제 시점(단계) | 선행 조건 |
|---|---|---|
| `Assets/Scripts/Manager/ResourceManager.cs` | 1 | 호출부 0건 |
| `Assets/Scripts/Manager/AudioManager.cs` | 2 | 호출부 0건 |
| `Assets/Scripts/Manager/UIManager.cs` | 4 | 호출부 0건 |
| `Assets/Scripts/Util/UIBase.cs` (전역 `UIBase`+`UIArg`) | 4 | 파생/호출부 0건 + 패키지 UIBase 로 전환 완료 |
| `Assets/Scripts/Util/CHScrollView.cs` (전역 PoolingScrollView* enum 포함) | 4 | ScrollView 3개 베이스 교체 완료 |
| `Assets/Scripts/Util/ButtonEx.cs` | 5 | 참조 0건 |
| `Assets/Scripts/Util/TextEx.cs` | 5 | 참조 0건 |
| `Assets/Scripts/Util/SingletoneMonoBehaviour.cs` | 6 | 상속 0건 |
| `Assets/Scripts/Util/SingletoneStatic.cs` | 6 | 상속 0건 |
| `Assets/Scripts/Manager/AdmobManager.cs` | 7 | 호출부 0건 + UNITY_INFRA_ADS 활성 |

각 파일 삭제 시 동행 `.meta` 도 함께 제거(Rule 01 — 삭제 파일의 meta 스테이징).

**비삭제(유지)**: `GameManagement.cs`, `JsonManager.cs`, `Extension.cs`, `CommonEnum.cs`, 신규 추가 Provider 어댑터 2개, 모든 Scene/UI/Cell 게임 로직.

### 이름 모호성 충돌 주의 (`using ChvjUnityInfra;` 도입)
- `using ChvjUnityInfra;` 를 자체 `UIBase`/`UIArg`/`PoolingScrollView*` enum 이 **아직 존재하는 상태**에서 도입하면 **CS0104(모호 참조)** 발생.
- 회피: 단계 4 에서 (a) 자체 `UIBase.cs`/`CHScrollView.cs` 삭제와 (b) `using ChvjUnityInfra;` 도입을 **같은 커밋**에 묶거나, (c) 삭제 전까지는 패키지 타입을 `ChvjUnityInfra.UIBase` 처럼 **완전 한정명**으로 참조한다.
- `CHText`/`CHButton` 은 자체에 동명 타입이 없으므로(`TextEx`/`ButtonEx`) 충돌 없음.
- `CHScrollView`(자체) vs `CHPoolingScrollView`(패키지) 는 **이름이 달라** 직접 충돌은 없으나, 같은 파일이 정의하는 `PoolingScrollViewDirection` 등 enum 이 충돌.

---

## 6. 리스크 / 주의

| # | 리스크 | 영향 | 완화 |
|---|---|---|---|
| R1 | **`UNITY_INFRA_ADS` 미정의 → CHMAdmob 부재** | Ads 단계에서 컴파일 실패 | 단계 7 선행으로 Tools 설정에서 define 활성 + `GoogleMobileAds.Api` 어셈블리 존재 확인. (현재 GoogleMobileAds 는 Editor asmdef 만 확인됨 — 런타임 어셈블리 해결 여부 빌드 전 검증) |
| R2 | **`AcquireReward`/`CloseAD` event 화** | `= 대입` 호출부 컴파일 실패 | 모든 대입을 `+=`/`-=` 로 변경(현 호출부 점검) |
| R3 | **자체 UIBase/CHScrollView 에 `[MovedFrom]` 없음** | 프리팹/씬의 컴포넌트가 미싱 스크립트로 깨짐 → 룰렛/로또/팝업 시각 깨짐 | 교체 시 동일 GUID 유지 불가하므로, (a) `[MovedFrom]` 추가 후 교체 또는 (b) 프리팹별 스크립트 수동 재할당 + 인스펙터 참조 재확인. 단계 4 직후 Play 검증 |
| R4 | **CHMSound 효과음 PlayOneShot** | 빠른 연속 클릭/회전음 겹침(동작 변화) | 유틸 앱 특성상 허용. 거슬리면 게임 측에서 채널 분리 또는 직전 Stop. 회귀 항목으로 기록 |
| R5 | **ESC/뒤로가기 메인 룰렛 닫기** | `UIManager.SetMainUI/ResetMainUI` 가 CHMUI 에 없음 → Android 뒤로가기 시 메인 화면 종료 동작 손실 | `IRouletteBackButton` 흐름을 게임 측(RouletteScene)에 보존. CHMUI 는 최상위 UI 만 닫고, 스택이 비면 게임 측 핸들러가 메인 룰렛 Close 호출 |
| R6 | **CHMUI ESC 가 LEGACY_INPUT_MANAGER 게이트** | Input System Only 프로젝트면 ESC 자동닫기 미동작 | 현 프로젝트 입력 모드 확인. 모바일 뒤로가기는 어차피 게임 측 처리이므로 영향 제한적 |
| R7 | **MonoBehaviour 싱글톤 초기화 순서** | CHMUI/CHMSound 가 첫 접근 시 자동 생성 — 부팅 순서가 어긋나면 캔버스/오디오 늦생성 | `ChvjUnityInfraSDK.Initialize` 순서(Resource→afterResourceInit→UI→Ads) 준수. CHMSound.Init 는 SDK 밖이므로 부팅에서 명시 호출 |
| R8 | **CompositeDisposable 타입 이원화** | UniRx `CompositeDisposable` vs 패키지 동명 타입 혼용 시 모호성 | UIBase 의 `closeDisposable` 는 패키지 타입을 따름. 게임 로직의 UniRx 사용처는 `using UniRx;` 한정으로 분리 |
| R9 | **default `_stringID` 1 → -1 차이** | 신규 CHText 추가 시 라벨이 비어 보임 | 기존 프리팹 값은 `[MovedFrom]` 으로 보존됨. 신규 추가 시 stringID 명시 |
| R10 | **`SetMainUI(IRouletteBackButton)` 의 IRouletteBackButton** | 게임 인터페이스 — 패키지 무관, 유지 | 변경 없음. CHMUI 와 무관하게 RouletteScene 이 소유 |

---

## 7. 범위 밖 / 유지 (패키지 대응 없음)

| 유지 대상 | 사유 |
|---|---|
| `GameManagement` | 게임 부팅 오케스트레이션 + 언어/폰트/룰렛 카운트 도메인 상태. 내부 매니저 호출만 재배선 |
| `JsonManager` (+ StringData/CountryData/AnimalData) | 게임 도메인 다국어/데이터 테이블. `IStringProvider` 어댑터로 CHText 에 연결 |
| `Extension` (`RotateZRoation`/`RotateXYPosition`/`Spin` 등) | 룰렛 회전 도메인 확장 메서드 |
| `CommonEnum` | 게임 소유 enum(에셋 키). 패키지는 `Enum` 베이스로 수용 |
| 모든 Scene/Cell/게임 로직 | 게임플레이 — 인프라 교체 무관 |
| **CHMPool 전면 적용** | 본 계획서는 "매니저 인프라 교체"로 한정. 풀링 스폰 전환은 회귀 위험 큼 → 후속 작업으로 분리(단계 3 은 Init 부팅만) |
| **UI 위젯(Slider 등) / TMP_Text 직접 참조의 CHText 전환** | 위젯 단위 래퍼 정합(Rule 03 §3)은 매니저 교체와 별개 후속. 본 단계는 동작 보존 우선 |
| Iap / Social 모듈 | SDK 미설치로 패키지에서 제거됨. 범위 밖 |

---

## § 구현 요청사항 (gameplay-programmer 용)

> 본 작업은 인프라 리팩터로, 신규 게임 콘텐츠 Enum/SO 추가가 아니다. 아래는 "유지/추가/삭제" 명세.

### 신규 추가 (게임 측, `.cs` — gameplay-programmer 작성)
- **IStringProvider 구현 어댑터** — `ChvjUnityInfra.IStringProvider.GetString(int)` → `JsonManager.Instance.GetStringData(id)` 위임. (Rule 02 §9 — 공용 인터페이스 구현부는 적절 위치)
- **IFontProvider 구현 어댑터** — `GetFont()` → `GameManagement.Instance.FontAsset`, `GetFontMaterial()` → 폰트 머티리얼(없으면 `font.material`).
- **부팅 배선**(GameManagement.InitManager 내부): CHMSound.Init / ClickSoundHook / StringProvider / FontProvider 등록 + ChvjUnityInfraSDK.Initialize 호출(§2-G).

### Enum
- **신규 Enum 없음.** 기존 `CommonEnum.EUI/EScene/EAudio/EJson/EFont/...` 유지. (정합 점검만: enum 값명 = Addressable 주소 = 에셋 파일명, Rule 03 §2)
- 삭제: 전역 `PoolingScrollViewDirection/Align/ScrollingDirection`(자체 `CHScrollView.cs` 정의) — `CHScrollView.cs` 삭제로 함께 제거, 패키지 동명 enum 사용.

### Interface
- 신규 구현 대상: `ChvjUnityInfra.IStringProvider`, `ChvjUnityInfra.IFontProvider` (위 어댑터 2종).
- 유지: 게임 `IRouletteBackButton` / `IRouletteSceneAccess`(RouletteScene.cs) — 패키지 무관.

### 에셋 키 / SO 스키마
- **AdConfig.asset** (단계 7) — `Assets/Resources/ChvjUnityInfra/AdConfig.asset`. 스키마는 패키지 `AdConfig`(BannerAdUnitId/InterstitialAdUnitId/RewardedAdUnitId/UseTestAds/...). 기존 프로덕션 배너 `ca-app-pub-7085378387310828/7475395879`, 전면 `ca-app-pub-7085378387310828/3699678689` 입력. rewarded 는 빈 값(미사용).
- **Scripting Define** — `UNITY_INFRA_ADS` (Tools/ChvjUnityInfra/Settings → Use Admob) 활성화. 단계 7 선행.
- 프리팹 컴포넌트 재매핑: `ButtonEx→CHButton`, `TextEx→CHText` 는 `[MovedFrom]` 자동. `UIBase`(자체→패키지), `CHScrollView→CHPoolingScrollView` 는 수동 재할당 + 인스펙터 참조 재검증(R3).

---

## Self-Review

- **스펙 커버리지**: 스펙 없음 — 사용자 요구(프롬프트 7개 항목) 직접 매핑. ①매핑표 §1, ②API delta §2, ③영향 범위 §3, ④마이그레이션 순서 §4, ⑤삭제 대상 §5, ⑥리스크/주의 §6, ⑦범위 밖/유지 §7 — 7/7 매핑. 갭 0.
- **Placeholder 잔존**: TBD/추후/적절히/또는(위임) 0건. "후속 작업으로 분리"는 범위 결정으로 명시(애매 위임 아님).
- **내부 일관성**: 삭제 단계(§5)와 순서(§4)의 단계 번호 일치. define 사실(§0/R1/§4단계7/구현요청) 동일. 라벨 "Resource" 일치 명시.
- **시그니처/명명 일관성**: `CHMResource/CHMUI/CHMSound/CHMPool/CHMAdmob`, `CHSingleton/CHSingletonStatic`, `IStringProvider/IFontProvider`, `ClickSoundHook/ChangeSoundHook`, `UNITY_INFRA_ADS`, `CHScrollView→CHPoolingScrollView` 전 문서 동일 표기 확인.
- **스코프**: 매니저 인프라 교체로 한정. CHMPool 전면화/위젯 정합은 후속 분리 명시.

**Self-Review: 통과 (스펙 없음 — 사용자 요구 7항목 직접 매핑, 갭 0 / placeholder 0).**
