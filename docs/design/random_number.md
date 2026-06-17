# 기획서 — random_number (숫자 뽑기)

> 결정 도우미 도구 풀 / **우선순위 P2**
> 작성: game-designer 단계 (구현 전). 코드 없음 — 본 문서는 gameplay-programmer 의 구현 입력.

---

## 1. 개요

- **한 줄 컨셉**: 최소~최대 범위에서 숫자를 K개 무작위로 뽑는 도구(중복 허용/금지 선택).
- **결정 단위**: 범위 내 K개 추출.

> ⚠️ **기존 기능과 강한 중복**: `RandomNumberScene` 이 이미 존재한다 — 시작/끝 숫자를 입력받아 `[시작..끝]` 리스트를 만들어 `CHMUI.ShowUI(EUI.UIRoulette)` 룰렛으로 **1개** 뽑는다(stringID 140·141 입력/범위 에러 게이트 포함). 본 도구는 **뽑을 개수 K + 중복 허용/금지 옵션** 을 추가한 확장형이다.
> **사용자 결정 필요(택1)**: (A) 별도 독립 도구로 신설 / (B) **기존 `RandomNumberScene` 을 K·중복옵션으로 확장**(권장 — 신규 메뉴 항목·씬 추가 없이 기존 진입점 보강). → 아래 §5~7 은 (A) 기준으로 작성. **(B) 선택 시 §5.1 배치/와이어링은 불필요**(기존 씬에 스테퍼·토글·다중 결과만 추가).

---

## 2. 입력 (Input)

| 항목 | 형태 | 범위 / 기본 | 비고 |
|---|---|---|---|
| 최소값 | `TMP_InputField` | 정수 | 기존 `RandomNumberScene` 의 `_startNumberInput` 선례 |
| 최대값 | `TMP_InputField` | 정수, 최소값 이상 | 기존 `_endNumberInput` 선례 |
| 뽑을 개수 K | 스테퍼 `CHButton` − / + + `CHText` | 1 ~ 범위 크기 | 비복원 시 상한 = (max−min+1) |
| 중복 허용 | `CHToggle` | 기본 끔(비복원) | 켜면 복원 추출 |
| 뽑기 | `CHButton` (뽑기) | — | 단일 액션 |

- 입력 검증(정수 파싱·min ≤ max)은 기존 `RandomNumberScene.CheckInteger` + stringID 140·141 에러 표기 패턴 그대로.
- 비복원인데 K > 범위 크기면 K 클램프 또는 뽑기 차단.

---

## 3. 동작 (Behavior)

1. 범위·K·중복옵션 입력 → 뽑기 탭.
2. 입력 검증 실패 시 에러 문자열(기존 140·141 재사용).
3. 클릭 사운드(`EAudio.Click`).
4. 추출:
   - **비복원**: `[min..max]` 셔플 후 앞 K개.
   - **복원**: 매번 `UnityEngine.Random.Range(min, max+1)` K회(중복 가능).
5. 결과 표시.

> **K=1** 이면 기존 패턴(`UIRoulette` 스핀) 그대로 — 현재 UI 풍 일치. **K≥2** 는 자체 다중 결과 영역.

---

## 4. 결과 (Output)

- 뽑힌 K개 숫자 리스트.
- K=1 룰렛 경로면 `UIRoulette` 결과 그대로.
- 표시 문자열은 String.json stringID 경유(§6).

---

## 5. UI / 화면 흐름  *(옵션 A — 독립 도구 기준)*

### 5.0 현재 UI 풍 준수 (필수)

> 다중 추출·중복옵션은 유지하되, 외형·레이아웃·컴포넌트는 현재 앱 풍에 맞춘다. **K=1 은 기존 UIRoulette 결과 재사용**.

- **테마**: `Assets/Dark UI/` 다크 패널·버튼.
- **폰트**: `EFont.Jua` — 모든 텍스트 `CHText`.
- **컴포넌트**: 범위 입력 `TMP_InputField`(기존 선례), K 스테퍼·뽑기 `CHButton`, 중복 `CHToggle`, 라벨 `CHText`.
- **레이아웃 관례**: 상단 메뉴(뒤로) 버튼 · 중앙 입력+결과 · 하단 뽑기 버튼. (기존 `RandomNumberScene` 레이아웃 연장)

### 5.1 배치 / RouletteScene 와이어링

- `RandomNumberScene` 와 동일하게 **`RouletteScene` 의 자식 GameObject**(active 토글).
- 연결(`RouletteScene.cs`): ① `ERouletteMenu.RandomNumberMulti` 추가 → ② `[SerializeField] ...Scene` → ③ `SetManagement()` 에서 `SetRouletteSceneAccess(this)` → ④ `ShowScene()` switch case + `liMainSceneObj` 토글 → ⑤ 메뉴 진입 버튼 `_liMenu` 등록.
- 복귀: `Close()` → `SetActive(false)` + `ShowScene(ERouletteMenu.Menu)`.

> (B) 선택 시 이 절은 불필요 — 기존 `RandomNumberScene` 의 `ERouletteMenu.RandomNumber` 진입점·씬을 그대로 쓰고 입력/결과만 확장.

### 5.2 구성

- 상단: 메뉴(뒤로) 버튼 `CHButton`
- 입력: 최소/최대 `TMP_InputField` · K 스테퍼 · 중복 `CHToggle`
- 중앙: 결과 영역(K개 숫자, Dark UI 패널)
- 하단: 뽑기 `CHButton` (에러 시 라벨에 안내)
- **상태 리셋**: `OnEnable` 에서 입력·K(1)·중복(끔)·결과 초기화 (기존 `RandomNumberScene.OnEnable` 패턴).

---

## 6. Enum · 에셋 키 · 문자열 매핑

| 종류 | 키 | 비고 |
|---|---|---|
| 메뉴 항목 | (A) `CommonEnum.ERouletteMenu.RandomNumberMulti` 신규 / (B) 기존 `RandomNumber` 재사용 | 택1에 따라 |
| 결과 셀 프리팹 | `Assets/AddressableResource/Prefab/NumberResultCell` | K≥2 결과 풀링 prototype |
| 룰렛 UI(K=1) | `CommonEnum.EUI.UIRoulette` (재사용) | 단일 뽑기 경로 |
| 클릭 사운드 | `CommonEnum.EAudio.Click` (재사용) | — |
| 폰트 | `CommonEnum.EFont.Jua` (재사용) | 현재 UI 풍 |
| 시각 테마 | `Assets/Dark UI/` (재사용) | — |
| 표시 문자열 | 입력/범위 에러는 기존 stringID **140·141 재사용** + "뽑기"/"결과" 신규 | 신규는 다음 빈 ID 순차 할당 |

---

## 7. ChvjPackage 연동 포인트 (Rule 03)

- **UI 컴포넌트**: `TMP_InputField`(범위)·`CHButton`(스테퍼·뽑기)·`CHToggle`(중복)·`CHText`(결과).
- **풀링**: K≥2 결과 셀 `CHPoolingScrollView`(BuildModalPopup 패턴). `Object.Instantiate` 금지.
- **UI 호출**: K=1 은 `CHMUI.ShowUI(EUI.UIRoulette, new UIRouletteArg{liText})` 재사용.
- **사운드**: `CHMSound` Click hook.
- **문자열**: `JsonManager.GetStringData` 경유(에러 140·141 포함).

---

## 8. 엣지 케이스 / 구현 메모

- 정수 파싱 실패·min > max → 기존 140·141 에러 표기.
- 비복원 K > 범위 크기 → K 클램프 또는 차단.
- 음수·큰 범위 허용(int 범위). 범위 폭 과대 시 비복원 셔플 비용 주의(필요 시 부분 셔플/샘플링).
- 추출 로직(복원/비복원)을 연출과 분리해 분포·중복 규칙 테스트.

---

## 9. 추후 확장 (P2 범위 밖)

- 정렬 옵션(오름/내림차순).
- 구간 가중치.
- 결과 합계/평균 표기.
