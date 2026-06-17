# 기획서 — lottery (제비뽑기)

> 결정 도우미 도구 풀 / **우선순위 P1**
> 작성: game-designer 단계 (구현 전). 코드 없음 — 본 문서는 gameplay-programmer 의 구현 입력.

---

## 1. 개요

- **한 줄 컨셉**: N개 항목을 상자에 넣고 그 중 K개를 무작위로 뽑는 제비뽑기.
- **결정 단위**: N개 중 K개 **비복원** 추출(중복 없음).
- **차별점**: roulette(1개) 와 달리 **여러 개 동시 추출**. 상자에서 제비를 뽑는 연출.

---

## 2. 입력 (Input)

> 다중 항목 입력은 기존 `CustomRandomScene` 패턴 그대로.

| 항목 | 형태 | 범위 / 기본 | 비고 |
|---|---|---|---|
| 항목 목록 | `TMP_InputField` + 추가/삭제 `CHButton` → `CustomScrollView` | 2 ~ N | 뽑기 후보 |
| 뽑을 개수 K | 스테퍼 `CHButton` − / + + `CHText` | 1 ~ N, 기본 1 | 항목 수에 따라 상한 동적 |
| 뽑기 | `CHButton` (뽑기) | — | 단일 액션 |

- K 상한은 현재 항목 수 N. 항목 추가/삭제 시 K 상한 재계산(초과 시 K 클램프).

---

## 3. 동작 (Behavior)

1. 항목 입력 + K 설정 → 뽑기 탭.
2. 클릭 사운드(`EAudio.Click`), 버튼 입력 잠금.
3. 상자/제비 뽑기 연출(DOTween) — K개 순차 등장.
4. **비복원 추출**: 후보를 셔플(Fisher-Yates) 후 앞 K개. 같은 항목 중복 없음.
5. 뽑힌 K개 결과 표시 + 버튼 잠금 해제.

> **K=1** 이면 기존 앱 패턴(`CHMUI.ShowUI(EUI.UIRoulette)`)으로 룰렛 스핀 재사용 가능 — 현재 UI 풍과 일치. **K≥2** 는 룰렛(단일 스핀) 부적합 → 자체 다중 결과 영역.

---

## 4. 결과 (Output)

- 뽑힌 K개 리스트(`CHPoolingScrollView` 또는 고정 슬롯).
- K=1 룰렛 경로면 `UIRoulette` 결과 그대로.
- 표시 문자열은 String.json stringID 경유(§6).

---

## 5. UI / 화면 흐름

### 5.0 현재 UI 풍 준수 (필수)

> 다중 추출 메커니즘은 유지하되, 외형·레이아웃·컴포넌트는 현재 앱 풍에 맞춘다. **K=1 은 기존 UIRoulette 결과 재사용**.

- **테마**: `Assets/Dark UI/` 다크 패널·버튼.
- **폰트**: `EFont.Jua` — 모든 텍스트 `CHText`.
- **컴포넌트**: 추가/삭제·스테퍼·뽑기 `CHButton`, 라벨 `CHText`, 입력 `TMP_InputField`, 리스트 `CustomScrollView`.
- **레이아웃 관례**: 상단 메뉴(뒤로) 버튼 · 중앙 입력+결과 · 하단 뽑기 버튼.

### 5.1 배치 / RouletteScene 와이어링

- `CustomRandomScene` 와 동일하게 **`RouletteScene` 의 자식 GameObject**(active 토글).
- 연결(`RouletteScene.cs`): ① `ERouletteMenu.Lottery` 추가 → ② `[SerializeField] LotteryScene _lotteryScene` → ③ `SetManagement()` 에서 `SetRouletteSceneAccess(this)` → ④ `ShowScene()` switch `case Lottery` + `liMainSceneObj` 토글 → ⑤ 메뉴 진입 버튼 `_liMenu` 등록.
- 복귀: `Close()` → `SetActive(false)` + `ShowScene(ERouletteMenu.Menu)`.

### 5.2 구성

- 상단: 메뉴(뒤로) 버튼 `CHButton`
- 입력: 항목 입력칸 + 추가·삭제 버튼 + `CustomScrollView` / K 스테퍼
- 중앙: 상자/뽑기 연출 영역(Dark UI 패널)
- 하단: 뽑기 `CHButton` + 결과 영역 `CHText`/리스트
- **상태 리셋**: `OnEnable` 에서 항목 리스트·K(1)·결과 초기화.

---

## 6. Enum · 에셋 키 · 문자열 매핑

| 종류 | 키 | 비고 |
|---|---|---|
| 메뉴 항목 | `CommonEnum.ERouletteMenu.Lottery` (신규) | 호스트 메뉴 진입 키 |
| 결과 셀 프리팹 | `Assets/AddressableResource/Prefab/LotteryResultCell` | K≥2 결과 풀링 prototype |
| 룰렛 UI(K=1) | `CommonEnum.EUI.UIRoulette` (재사용) | 단일 뽑기 경로 |
| 클릭 사운드 | `CommonEnum.EAudio.Click` (재사용) | — |
| 폰트 | `CommonEnum.EFont.Jua` (재사용) | 현재 UI 풍 |
| 시각 테마 | `Assets/Dark UI/` (재사용) | — |
| 표시 문자열 | String.json 신규 stringID — "뽑기" / "결과" 등 | 다음 빈 ID 순차 할당 |

---

## 7. ChvjPackage 연동 포인트 (Rule 03)

- **UI 컴포넌트**: `CHButton`/`CHText`/`TMP_InputField`/`CustomScrollView`.
- **풀링**: K≥2 결과 셀 `CHPoolingScrollView`(BuildModalPopup 패턴, Rule 03 §3). 동적 제비 오브젝트는 `CHMPool`.
- **UI 호출**: K=1 은 `CHMUI.ShowUI(EUI.UIRoulette, new UIRouletteArg{liText})` 재사용.
- **사운드**: `CHMSound` Click hook.
- **문자열**: `JsonManager.GetStringData` 경유.

---

## 8. 엣지 케이스 / 구현 메모

- K > 현재 항목 수 → K 자동 클램프(또는 뽑기 차단).
- 항목 0~1개 → 뽑기 차단.
- 비복원 보장(같은 항목 두 번 금지) — 셔플 후 절단 방식 권장.
- 추출 로직을 연출과 분리(비복원·균등 분포 테스트).

---

## 9. 추후 확장 (P1 범위 밖)

- 복원 추출(중복 허용) 토글.
- 가중치 항목.
- 뽑은 항목 자동 제외 후 연속 뽑기.
