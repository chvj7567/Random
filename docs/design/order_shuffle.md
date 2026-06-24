# 기획서 — order_shuffle (순서 정하기)

> 결정 도우미 도구 풀 / **우선순위 P2**
> 작성: game-designer 단계 (구현 전). 코드 없음 — 본 문서는 gameplay-programmer 의 구현 입력.
> 최종 업데이트: 2026-06-24 (구조 오류 수정 + stringID 확정 + 구현 요청사항 보강)

---

## § 헤더

- **목표**: N개 이름을 무작위 순서로 섞어 1번~N번 순위를 즉시 표시하는 단일 액션 도구를 추가한다.
- **검증 가설**: 발표 순서·차례 등 전체 순열이 필요한 상황에서 단일 버튼 탭으로 결과가 나오는가.
- **현재 단계 범위 적합성**: 범위 내 — 운영/유지보수 단계, 기존 RandomScene 서브 화면 추가 패턴과 동일.
- **핵심 메커니즘**: 이름 목록 입력 → 섞기 버튼 탭 → Fisher-Yates 셔플 → 1번부터 순서대로 리스트 표시. 재탭하면 즉시 재섞기.

---

## 1. 개요

- **한 줄 컨셉**: N개 이름을 무작위로 섞어 순번(1번~N번)을 정해주는 도구.
- **결정 단위**: N개 전체 순열(발표 순서·차례 정하기).
- **차별점**: lottery(일부 추출)·team_split(그룹핑) 과 달리 **전체를 한 줄로 순서화**.

---

## 2. 입력 (Input)

| 항목 | 형태 | 범위 / 기본값 | 비고 |
|---|---|---|---|
| 이름 목록 | `TMP_InputField` + 추가/삭제 `CHButton` → `CustomScrollView` | 2 ~ 12 | 순서 대상 |
| 섞기 | `CHButton` (섞기) | — | 단일 액션, 재탭 = 재섞기 |

**MaxCount = 12, MinCount = 2** (TeamSplit 과 동일 상한 — 입력 UI 일관성 및 결과 영역 스크롤 없이 한 화면에 보이는 최대 목표치).

---

## 3. 동작 (Behavior)

1. 이름 2개 이상 입력 → 섞기 버튼 탭.
2. 클릭 사운드(`EAudio.Click`).
3. Fisher-Yates 셔플로 전체 순열 생성 (결과 산출을 연출과 분리 — 로직은 `OrderShuffleScene` 내 순수 메서드).
4. 1번~N번 순서로 결과 리스트 즉시 표시.
5. **재탭(재섞기)**: 섞기 버튼을 다시 탭하면 동일 이름 목록으로 재셔플하여 결과를 덮어쓴다.

> 균등 셔플(모든 순열 동일 확률). `§9`의 "재섞기 전용 버튼"은 현재 범위 밖이며, 현 시점에서 메인 버튼 재탭으로 동일 기능을 제공한다.

---

## 4. 결과 (Output)

- 순번 리스트 — `CHPoolingScrollView<OrderCell, OrderCellData>` 패턴.
- 셀 표시 형식: stringID 2081 `"{0}. {1}"` 포맷 적용 (`0` = 순번, `1` = 이름).
- Dark UI 톤.
- 이름 입력 전 또는 섞기 미실행 상태: 빈 결과 안내 텍스트(stringID 2082) + `OrderCell` 영역 숨김.
- 첫 섞기 실행 후: 빈 결과 안내 숨김 + `OrderCell` 리스트 표시.

---

## 5. UI / 화면 흐름

### 5.0 현재 UI 풍 준수 (필수)

- **테마**: `Assets/Dark UI/` 다크 패널·버튼.
- **폰트**: `EFont.Jua` — 모든 텍스트 `CHText`.
- **컴포넌트**: 추가/삭제·섞기 `CHButton`, 라벨 `CHText`, 입력 `TMP_InputField`, 리스트 `CustomScrollView` / `CHPoolingScrollView`.
- **레이아웃 관례**: 상단 뒤로 버튼 · 중앙 입력 영역 + 순서 결과 리스트 · 하단 섞기 버튼.

### 5.1 배치 / RandomScene 와이어링

`TeamSplitScene` 추가 선례와 정확히 동일한 6-터치포인트 패턴을 따른다.

| # | 파일 | 변경 내용 |
|---|---|---|
| 1 | `Assets/Scripts/Util/CommonEnum.cs` | `ERouletteMenu` 에 `OrderShuffle` 값 추가 |
| 2 | `Assets/Scripts/Util/CommonEnum.cs` | `EMenuIcon` 에 `MenuOrderShuffle` 값 추가 |
| 3 | `Assets/Scripts/Scene/RandomScene.cs` | `[SerializeField] private OrderShuffleScene _orderShuffleScene;` 필드 추가 |
| 4 | `Assets/Scripts/Scene/RandomScene.cs` | `SetManagement()` 에 `_orderShuffleScene.SetRandomSceneAccess(this);` 추가 |
| 5 | `Assets/Scripts/Scene/RandomScene.cs` | `ShowScene()` 상단 전체 비활성 블록(line 72~79 패턴)에 `_orderShuffleScene.gameObject.SetActive(false);` 추가 |
| 6 | `Assets/Scripts/Scene/RandomScene.cs` | `ShowScene()` switch 에 `case CommonEnum.ERouletteMenu.OrderShuffle:` 블록 추가 (`_mainRouletteUI = _orderShuffleScene` + liMainSceneObj off + `_orderShuffleScene.gameObject.SetActive(true)`) |
| 7 | `Assets/Scripts/UI/ScrollView/MenuCardCatalog.cs` | `BuildMenuCards()` 반환 리스트에 OrderShuffle 행 추가 (`menu`, `iconKey`, `titleStringID 2083`, `descStringID 2084`) |

> **`SetManagement()`는 기존 구조 그대로 유지** — `RandomScene.Start()`가 호출하는 현재 패턴을 변경하지 않는다.

복귀: `OrderShuffleScene.Close()` → `gameObject.SetActive(false)` + `_randomSceneAccess.ShowScene(CommonEnum.ERouletteMenu.Menu)`.

### 5.2 구성

- **상단**: 뒤로 버튼 `CHButton` (`_menuButton`)
- **입력 컬럼**: 이름 입력칸(`TMP_InputField`) + 추가 버튼 + 삭제 버튼 + 이름 리스트 `CustomScrollView` + 인원 카운트 `CHText`
- **결과 영역**: 빈 안내 `CHText`(stringID 2082) + `OrderCellPoolingScrollView` (결과 전 숨김)
- **하단**: 섞기 `CHButton` (`_shuffleButton`)
- **상태 리셋**: `OnEnable` 에서 이름 리스트·순서 결과 초기화, 빈 안내 표시 복원.

---

## 6. Enum · 에셋 키 · 문자열 매핑

### 6.1 Enum

| Enum | 값 | 추가 위치 | 비고 |
|---|---|---|---|
| `CommonEnum.ERouletteMenu` | `OrderShuffle` (신규) | `CommonEnum.cs` | `TeamSplit` 다음 순서 |
| `CommonEnum.EMenuIcon` | `MenuOrderShuffle` (신규) | `CommonEnum.cs` | 기존 `MenuShuffle`은 CustomRandom 전용이라 별도 키 필요 |

### 6.2 에셋 키

| 에셋 | 키 / 파일명 | 위치 | 비고 |
|---|---|---|---|
| 순서 셀 프리팹 | `OrderCell.prefab` | `Assets/AddressableResource/Prefab/` | CHPoolingScrollView 풀링 prototype |
| 메뉴 아이콘 | `MenuOrderShuffle.png` | `Assets/AddressableResource/Sprite/` | `Assets/Dark UI/New Icons/White Layers Round.png` 복사·리네임 — `MenuNumber←White A1`, `MenuFood←White Apple` 동일 패턴 |
| 클릭 사운드 | `CommonEnum.EAudio.Click` (재사용) | — | 기존 |
| 폰트 | `CommonEnum.EFont.Jua` (재사용) | — | 기존 |
| 시각 테마 | `Assets/Dark UI/` (재사용) | — | 기존 |

### 6.3 문자열 stringID

마지막 기존 ID: 2079 (`"무작위로 팀 편성"`). 신규 ID는 2080부터 순차 할당.

| stringID | 용도 | korean | english |
|---|---|---|---|
| 2077 | 인원 카운트 포맷 (재사용) | `{0} / {1}` | `{0} / {1}` |
| 2080 | 섞기 버튼 텍스트 | `섞기` | `Shuffle` |
| 2081 | 순번 포맷 | `{0}. {1}` | `{0}. {1}` |
| 2082 | 빈 결과 안내 | `이름을 입력하고\n섞기를 누르세요` | `Enter names and\ntap Shuffle` |
| 2083 | 메뉴 카드 제목 | `순서 정하기` | `Order Shuffle` |
| 2084 | 메뉴 카드 설명 | `무작위로 순서 배정` | `Random order assignment` |

String.json에 2080~2084 항목 추가 필요.

---

## 7. ChvjPackage 연동 포인트 (Rule 03)

- **UI 컴포넌트**: 모든 버튼 `CHButton`, 모든 텍스트 `CHText`(`TMP_Text` 동반), 입력 `TMP_InputField`, 이름 리스트 `CustomScrollView`, 결과 리스트 `CHPoolingScrollView`.
- **풀링**: 결과 셀 `OrderCellPoolingScrollView` → `OrderCell` — BuildModalPopup 패턴 준수. `Object.Instantiate` 금지.
- **사운드**: `CHMSound.Instance?.Play(EAudio.Click)` (기존 `CHButton.ClickSoundHook` 경유).
- **문자열**: `JsonManager.Instance.GetStringData(stringID)` 경유 (하드코딩 금지).

---

## 8. 엣지 케이스 / 구현 메모

- 이름 0~1개 → 섞기 버튼 비활성(`Interactable = false`). 결과 리스트 비표시.
- 동명이인 허용 — 중복 이름 입력 차단 없음.
- Fisher-Yates 정확 구현(편향 셔플 금지) — 셔플 로직은 `OrderShuffleScene` 내 순수 정적 메서드로 분리(test-engineer 진입점).
- 입력 추가 후 `TMP_InputField` 자동 리셋(기존 TeamSplitScene 선례 — `input.text = string.Empty`).
- `OnDisable` 에서 `OrderCellPoolingScrollView.Clear()` 호출 — CHPoolingScrollView 내부 풀 관리.
- `OnEnable`은 `Start()`보다 먼저 실행될 수 있으므로 버튼 구독에 의존하지 않고 `ResetState()`로 상태 직접 세팅(TeamSplitScene 선례).

---

## 9. 구현 요청사항 (gameplay-programmer 용)

### 9.1 신규 파일

| 파일 | 역할 |
|---|---|
| `Assets/Scripts/Scene/OrderShuffleScene.cs` | 메인 Scene 컨트롤러 (`MonoBehaviour`, `IRouletteBackButton` 구현, `SetRandomSceneAccess(IRandomSceneAccess)` 포함) |
| `Assets/Scripts/UI/ScrollView/OrderCellPoolingScrollView.cs` | `CHPoolingScrollView<OrderCell, OrderCellData>` 상속 — `InitItem` / `InitPoolingObject` 오버라이드 |
| `Assets/Scripts/UI/OrderCell.cs` | 결과 셀 컴포넌트 — `[SerializeField] private CHText _text;` + `Bind(OrderCellData data)` |

### 9.2 Enum 추가

**`Assets/Scripts/Util/CommonEnum.cs`**

```
ERouletteMenu: TeamSplit 다음에 OrderShuffle 추가
EMenuIcon: MenuTeamSplit 다음에 MenuOrderShuffle 추가
```

### 9.3 기존 파일 수정

**`Assets/Scripts/Scene/RandomScene.cs`**

```
[SerializeField] 필드: private OrderShuffleScene _orderShuffleScene;
SetManagement(): _orderShuffleScene.SetRandomSceneAccess(this); 추가
ShowScene() 전체 비활성 블록: _orderShuffleScene.gameObject.SetActive(false); 추가
ShowScene() switch: case CommonEnum.ERouletteMenu.OrderShuffle: 블록 추가
```

**`Assets/Scripts/UI/ScrollView/MenuCardCatalog.cs`**

```
BuildMenuCards() 반환 리스트에 추가:
  menu = CommonEnum.ERouletteMenu.OrderShuffle
  iconKey = CommonEnum.EMenuIcon.MenuOrderShuffle
  titleStringID = 2083
  descStringID = 2084
```

**`Assets/AddressableResource/Json/String.json`**

```
stringID 2080~2084 항목 추가 (§6.3 표 참조)
```

### 9.4 SO 스키마 / 데이터 구조

```csharp
// OrderCellData — 순수 데이터 구조체 (MonoBehaviour 비의존)
public struct OrderCellData
{
    public int Rank;    // 1-based 순번
    public string Name; // 이름
}
```

`OrderCell.Bind(OrderCellData data)` 내부:
- `_text.SetText(string.Format(JsonManager.Instance.GetStringData(2081), data.Rank, data.Name))`

### 9.5 프리팹 / 에셋 등록

| 에셋 | 경로 | Addressables 주소 | 라벨 |
|---|---|---|---|
| `OrderCell.prefab` | `Assets/AddressableResource/Prefab/OrderCell.prefab` | `OrderCell` | `Resource` |
| `MenuOrderShuffle.png` | `Assets/AddressableResource/Sprite/MenuOrderShuffle.png` | `MenuOrderShuffle` | `Resource` |

### 9.6 배치 관계 (프리팹 하이어라키)

`OrderShuffleScene` 은 `RandomScene` 의 자식 GameObject(active=false 초기값). 이름 리스트 `CustomScrollView`, 결과 스크롤 `OrderCellPoolingScrollView`, `_origin` → `OrderCell` 인스턴스는 인스펙터에서 연결. `OrderCellPoolingScrollView._origin` = 씬 내 OrderCell 인스턴스 드래그.

---

## 10. 추후 확장 (현재 범위 밖 — P2)

- 재섞기 전용 빠른 버튼(현재는 메인 섞기 버튼 재탭으로 동일 기능 제공).
- 순서 결과 캡처/공유(기존 `Capture` 활용).
- 가중치(특정 인원 앞 순서 편향).
