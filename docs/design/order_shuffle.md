# 기획서 — order_shuffle (순서 정하기)

> 결정 도우미 도구 풀 / **우선순위 P2**
> 작성: game-designer 단계 (구현 전). 코드 없음 — 본 문서는 gameplay-programmer 의 구현 입력.

---

## 1. 개요

- **한 줄 컨셉**: N개 이름을 무작위로 섞어 순번(1번~N번)을 정해주는 도구.
- **결정 단위**: N개 전체 순열(발표 순서·차례 정하기).
- **차별점**: lottery(일부 추출)·team_split(그룹핑) 과 달리 **전체를 한 줄로 순서화**.

---

## 2. 입력 (Input)

> 다중 항목 입력은 기존 `CustomRandomScene` 패턴 그대로.

| 항목 | 형태 | 범위 / 기본 | 비고 |
|---|---|---|---|
| 이름 목록 | `TMP_InputField` + 추가/삭제 `CHButton` → `CustomScrollView` | 2 ~ N | 순서 대상 |
| 섞기 | `CHButton` (섞기) | — | 단일 액션 |

---

## 3. 동작 (Behavior)

1. 이름 입력 → 섞기 탭.
2. 클릭 사운드(`EAudio.Click`).
3. Fisher-Yates 셔플로 전체 순열 생성.
4. 1번~N번 순서로 표시(셔플 연출 후 정지).

> 균등 셔플(모든 순열 동일 확률) — 결과 산출을 연출과 분리.

---

## 4. 결과 (Output)

- 순번 리스트(`1. 이름` … `N. 이름`), `CHPoolingScrollView`.
- 순번 라벨 포맷은 String.json stringID 경유(§6).
- Dark UI 톤.

---

## 5. UI / 화면 흐름

### 5.0 현재 UI 풍 준수 (필수)

> 순서화 결과는 룰렛으로 표현 불가 — 자체 리스트 결과. 외형·레이아웃·컴포넌트만 현재 앱 풍에 맞춘다.

- **테마**: `Assets/Dark UI/` 다크 패널·버튼.
- **폰트**: `EFont.Jua` — 모든 텍스트 `CHText`.
- **컴포넌트**: 추가/삭제·섞기 `CHButton`, 라벨 `CHText`, 입력 `TMP_InputField`, 리스트 `CustomScrollView`.
- **레이아웃 관례**: 상단 메뉴(뒤로) 버튼 · 중앙 입력+순서 결과 · 하단 섞기 버튼.

### 5.1 배치 / RouletteScene 와이어링

- `CustomRandomScene` 와 동일하게 **`RouletteScene` 의 자식 GameObject**(active 토글).
- 연결(`RouletteScene.cs`): ① `ERouletteMenu.OrderShuffle` 추가 → ② `[SerializeField] OrderShuffleScene _orderShuffleScene` → ③ `SetManagement()` 에서 `SetRouletteSceneAccess(this)` → ④ `ShowScene()` switch `case OrderShuffle` + `liMainSceneObj` 토글 → ⑤ 메뉴 진입 버튼 `_liMenu` 등록.
- 복귀: `Close()` → `SetActive(false)` + `ShowScene(ERouletteMenu.Menu)`.

### 5.2 구성

- 상단: 메뉴(뒤로) 버튼 `CHButton`
- 입력: 이름 입력칸 + 추가·삭제 버튼 + `CustomScrollView`
- 중앙: 순서 결과 영역(번호 매겨진 리스트, Dark UI 패널)
- 하단: 섞기 `CHButton`
- **상태 리셋**: `OnEnable` 에서 이름 리스트·순서 결과 초기화.

---

## 6. Enum · 에셋 키 · 문자열 매핑

| 종류 | 키 | 비고 |
|---|---|---|
| 메뉴 항목 | `CommonEnum.ERouletteMenu.OrderShuffle` (신규) | 호스트 메뉴 진입 키 |
| 순서 셀 프리팹 | `Assets/AddressableResource/Prefab/OrderCell` | 결과 풀링 prototype |
| 클릭 사운드 | `CommonEnum.EAudio.Click` (재사용) | — |
| 폰트 | `CommonEnum.EFont.Jua` (재사용) | 현재 UI 풍 |
| 시각 테마 | `Assets/Dark UI/` (재사용) | — |
| 표시 문자열 | String.json 신규 stringID — "섞기" / "{0}. {1}"(순번 포맷) 등 | 다음 빈 ID 순차 할당 |

---

## 7. ChvjPackage 연동 포인트 (Rule 03)

- **UI 컴포넌트**: `CHButton`/`CHText`/`TMP_InputField`/`CustomScrollView`.
- **풀링**: 순서 결과 셀 `CHPoolingScrollView`(BuildModalPopup 패턴). `Object.Instantiate` 금지.
- **사운드**: `CHMSound` Click hook.
- **문자열**: `JsonManager.GetStringData` 경유.

---

## 8. 엣지 케이스 / 구현 메모

- 이름 0~1개 → 섞기 차단(순서 의미 없음).
- 동명이인 허용.
- Fisher-Yates 정확 구현(편향 셔플 금지) — 분포 테스트.
- 셔플 로직을 연출과 분리.

---

## 9. 추후 확장 (P2 범위 밖)

- 역순/재섞기 빠른 버튼.
- 순서 결과 캡처/공유(기존 `Capture` 활용).
- 가중치(특정 인원 앞 순서 편향).
