# 기획서 — menu_picker (메뉴 추천)

> 결정 도우미 도구 풀 / **우선순위 P2**
> 작성: game-designer 단계 (구현 전). 코드 없음 — 본 문서는 gameplay-programmer 의 구현 입력.

---

## 1. 개요

- **한 줄 컨셉**: 카테고리(한식/중식/일식/양식 등)를 고르면 해당 카테고리 메뉴 리스트에서 하나를 무작위로 추천하는 도구.
- **결정 단위**: 카테고리 내 1개 추천.

> ⚠️ **기존 기능과 중복 주의**: `RandomExampleScene` 이 이미 음식/동물/나라 리스트를 `UIRoulette` 룰렛으로 뽑는다(`GetAnimalNameList`/`GetCountryNameList`, `EJson.Animal`/`Country`). menu_picker 는 **카테고리 분기 + 메뉴별 설명** 을 추가한 확장형.
> **사용자 결정 필요(택1)**: (A) 별도 독립 도구로 신설(카테고리 토글 + 설명 표시) / (B) 기존 `RandomExampleScene` 음식 항목을 카테고리·설명으로 보강. → 아래 §5~7 은 (A) 기준. (B) 선택 시 §5 배치/와이어링 불필요(기존 씬 데이터·표시만 확장).

---

## 2. 입력 (Input)

| 항목 | 형태 | 비고 |
|---|---|---|
| 카테고리 | `CHToggle` 그룹 또는 `CHButton` ×N (한식/중식/일식/양식/…) | 라디오 선택 |
| 추천 | `CHButton` (추천) | 단일 액션 |

- 카테고리 목록은 데이터(Json)에서 구성 — 코드 하드코딩 금지.

---

## 3. 동작 (Behavior)

1. 카테고리 선택 → 추천 탭.
2. 클릭 사운드(`EAudio.Click`).
3. 해당 카테고리 메뉴 리스트에서 `UnityEngine.Random.Range(0, count)` 1개 선택.
4. 메뉴명 + 간단 설명 표시.

> **단일 추천**이라 기존 앱 패턴(`CHMUI.ShowUI(EUI.UIRoulette)`) 으로 룰렛 스핀 재사용 가능 — 현재 UI 풍과 일치. 설명까지 보여주려면 룰렛 결과 후 설명 패널 추가 또는 자체 결과 영역.

---

## 4. 결과 (Output)

- 추천 메뉴명(`CHText`) + 설명(`CHText`).
- 룰렛 경로 사용 시 스핀 후 메뉴 확정 → 설명 표기.
- 표시 문자열·메뉴/설명 데이터는 Json·String.json 경유(§6).

---

## 5. UI / 화면 흐름  *(옵션 A — 독립 도구 기준)*

### 5.0 현재 UI 풍 준수 (필수)

- **테마**: `Assets/Dark UI/` 다크 패널·버튼.
- **폰트**: `EFont.Jua` — 모든 텍스트 `CHText`.
- **컴포넌트**: 카테고리 `CHToggle`/`CHButton`, 추천 `CHButton`, 라벨 `CHText`.
- **레이아웃 관례**: 상단 메뉴(뒤로) 버튼 · 중앙 카테고리+결과 · 하단 추천 버튼.

### 5.1 배치 / RouletteScene 와이어링

- `RandomExampleScene` 와 동일하게 **`RouletteScene` 의 자식 GameObject**(active 토글).
- 연결(`RouletteScene.cs`): ① `ERouletteMenu.MenuPicker` 추가 → ② `[SerializeField] MenuPickerScene _menuPickerScene` → ③ `SetManagement()` 에서 `SetRouletteSceneAccess(this)` → ④ `ShowScene()` switch `case MenuPicker` + `liMainSceneObj` 토글 → ⑤ 메뉴 진입 버튼 `_liMenu` 등록.
- 복귀: `Close()` → `SetActive(false)` + `ShowScene(ERouletteMenu.Menu)`.

### 5.2 구성

- 상단: 메뉴(뒤로) 버튼 `CHButton`
- 중앙: 카테고리 선택(토글/버튼) + 추천 결과(메뉴명 + 설명, Dark UI 패널)
- 하단: 추천 `CHButton`
- **상태 리셋**: `OnEnable` 에서 카테고리 선택·결과 초기화.

---

## 6. Enum · 에셋 키 · 문자열 매핑

| 종류 | 키 | 비고 |
|---|---|---|
| 메뉴 항목 | `CommonEnum.ERouletteMenu.MenuPicker` (신규) | 호스트 메뉴 진입 키 |
| 메뉴 데이터 Json | `CommonEnum.EJson.Food`(신규) 또는 기존 Json 구조 확장 | 카테고리별 메뉴+설명. `JsonManager` 로드 |
| 룰렛 UI(선택) | `CommonEnum.EUI.UIRoulette` (재사용) | 단일 추천 스핀 경로 |
| 클릭 사운드 | `CommonEnum.EAudio.Click` (재사용) | — |
| 폰트 | `CommonEnum.EFont.Jua` (재사용) | 현재 UI 풍 |
| 시각 테마 | `Assets/Dark UI/` (재사용) | — |
| 표시 문자열 | String.json 신규 stringID — "추천" / 카테고리명 등 | 다음 빈 ID 순차 할당 |

> 메뉴/설명은 표시 텍스트라 가능한 String.json 또는 전용 Json 테이블로 관리(코드 리터럴 금지 — 표시 텍스트 단일화 정책).

---

## 7. ChvjPackage 연동 포인트 (Rule 03)

- **UI 컴포넌트**: `CHToggle`/`CHButton`/`CHText`.
- **데이터**: 메뉴 리스트는 `JsonManager`(기존 `GetAnimalNameList`/`GetCountryNameList` 선례) 로드. `EJson` 신규 키.
- **UI 호출**: 룰렛 경로 시 `CHMUI.ShowUI(EUI.UIRoulette, ...)` 재사용.
- **사운드**: `CHMSound` Click hook.
- **문자열**: `JsonManager.GetStringData` 경유.

---

## 8. 엣지 케이스 / 구현 메모

- 카테고리 미선택 → 추천 차단 또는 기본 카테고리.
- 빈 카테고리(메뉴 0개) → 안내.
- 같은 메뉴 연속 추천 허용(복원). 직전 제외 옵션은 추후.
- 추천 로직(`Range`)을 데이터 로드와 분리해 테스트.

---

## 9. 추후 확장 (P2 범위 밖)

- 직전 추천 제외.
- 사용자 정의 카테고리·메뉴 추가.
- 가격대/거리 등 필터.
