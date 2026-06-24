# 기획서 — team_split (팀 나누기)

> 결정 도우미 도구 풀 / **우선순위 P1**
> 작성: game-designer 단계 (구현 전). 코드 없음 — 본 문서는 gameplay-programmer 의 구현 입력.
> **상태: implementation-ready** — 구현자가 추가 판단 없이 그대로 만들 수 있도록 모든 결정 확정.

---

## 0. § 헤더 (한눈 요약)

- **목표**: N명을 T개 팀으로 무작위·균등 분배하는 단일 액션 도구를 `RandomScene` 의 서브 화면으로 추가.
- **검증 가설**: 사다리타기와 다른 "그룹핑" 결과(팀별 멤버 목록)가 기존 UI 풍·조작 흐름 안에서 자연스럽게 동작하는가.
- **현재 단계 범위 적합성**: 범위 내 — 운영/유지보수 단계의 신규 무작위 도구 추가(룰렛/로또/사다리와 동급). 메타·서버·사운드·아트 신규 없음.
- **핵심 메커니즘**: 참가자 이름 리스트 + 팀 수 스테퍼(2~N) 입력 → "나누기" **1회 클릭** → Fisher-Yates 셔플 후 floor(N/T) 균등 배분(나머지 앞 팀부터 1명) → 팀 카드(CHPoolingScrollView) **2열 고정 높이** 그리드 표시. 2단계 토글 없음.

**구현 기반(base)**: `Assets/Scripts/Scene/LadderScene.cs` 를 클론·개조. 단 사다리의 **2단계 토글(`_ladderDrawn`)·선 렌더(`RenderLadder`/`AppendSegment`)·DOTween 트레이스는 제거**하고, 결과 표시를 **팀 카드 풀링**으로 대체한다. 입력 패턴·`OnEnable` 리셋·풀 워밍·`IRandomSceneAccess` 와이어링·순수 로직 분리 선례는 그대로 따른다.

---

## 1. 개요

- **한 줄 컨셉**: N명을 T개 팀으로 균등하게 무작위 분배하는 도구.
- **결정 단위**: N명 → T팀 균등 배분(나머지는 앞 팀부터 1명씩).
- **차별점**: lottery·ladder 와 달리 **그룹핑** 결과. 팀별 멤버 목록이 산출물.

---

## 2. 입력 (Input)

> 다중 항목 입력은 기존 `LadderScene` / `CustomRandomScene` 의 참가자 입력 패턴 그대로 (단일 컬럼).

| 항목 | 형태 | 범위 / 기본 | 비고 |
|---|---|---|---|
| 참가자 이름 | `TMP_InputField` + 추가(`+`)/삭제(`−`) `CHButton` → `CustomScrollView` | 0 ~ **12** | 분배 대상. 빈/공백 무시, 동명이인 허용 |
| 팀 개수 T | 스테퍼 `CHButton` `−` / `CHText` 값 / `CHButton` `+` | **2 ~ N**, 기본 **2** | N = 현재 참가자 수 |
| 나누기 | `CHButton` (나누기) | — | **단일 액션** (§3.0) |

- **참가자 상한 `MaxCount = 12`** (확정) — 목업 `0 / 12` 표기와 일치. 사다리(`MaxCount=8`)보다 큼: 팀 나누기는 선 렌더 없이 카드만 그려 화면 부담이 적고, 다인원 그룹핑 수요가 크다. `+` 버튼은 12 도달 시 `Interactable=false`.
- **팀 수 상한·하한 클램프** (확정):
  - 하한 = 2 고정 (`−` 버튼은 T≤2 일 때 `Interactable=false`).
  - 상한 = `Max(2, 참가자 수 N)` (`+` 버튼은 T≥N 일 때 `Interactable=false`).
  - 참가자 **추가/삭제 시마다** 상한을 재계산하고 현재 T 를 `Clamp(2, Max(2, N))` 로 보정한다 → 빈 팀 방지(§8).
  - N<2 인 동안에도 스테퍼 자체는 값 2 를 유지(나누기만 차단).

---

## 3. 동작 (Behavior)

### 3.0 단일 액션 (확정 — 사다리 2단계 토글과의 차이)

팀 나누기는 **단일 액션**이다. "나누기" 버튼을 **1회 클릭하면 그 클릭 안에서 셔플 + 균등 배분 + 팀 카드 렌더가 모두 완료**된다.

- 사다리의 `_ladderDrawn` 같은 **1단계(그리기)→2단계(결과 보기) 토글을 두지 않는다.**
- 버튼 텍스트는 항상 **"나누기"** (stringID 2070) — 클릭 후에도 텍스트가 "결과 보기" 등으로 바뀌지 않는다.
- 같은 입력으로 다시 클릭하면 **새 셔플로 재배분**(`_cardScrollView.SetItemList()` 재호출 — CHPoolingScrollView 가 내부 풀을 자동 관리). 즉 매 클릭이 독립적인 재추첨.
- 연출 길이가 짧으므로(카드 팝업 애니메이션 한 번) 클릭 중 입력 잠금은 두지 않아도 무방하나, 카드 팝 트윈을 쓸 경우 트윈 진행 중 재클릭은 `KillCardTween()` 후 새로 시작한다.

### 3.1 클릭 처리 순서

1. 참가자 수 N = 현재 리스트 수, 팀 수 T = 스테퍼 값.
2. **검증** (§8):
   - N < 2 → 안내(stringID 2065 재사용) 표시 후 **중단**.
   - (T 는 입력 단계에서 이미 `2 ~ N` 로 클램프되어 있으므로 별도 검증 불필요. 방어적으로 `T = Clamp(T, 2, N)` 한 번 더 적용.)
3. 클릭 사운드(`EAudio.Click`) — `CHButton` hook 으로 자동(별도 호출 불필요).
4. 순수 로직 `Split(names, T, rng)` 호출 (§10) → `List<List<string>>` 팀별 멤버.
5. `_cardScrollView.SetItemList(dataList)` 호출 — `TeamSplitTeamData` 빌드 후 전달. 카드 풀·배치·바인딩은 CHPoolingScrollView 가 일괄 처리(§4).
6. 안내 라벨을 결과 요약(§3.2)로 갱신.

> 균등 규칙 명시: 팀 간 인원 차이는 최대 1명. 예) 7명 3팀 → 3·2·2.

### 3.2 실시간 배분 미리보기 (확정 — 목업 `guide` 라벨)

하단 안내 `CHText` 한 칸(`_guideText`)에 현재 입력 상태를 항상 표시한다. 나누기 전/후 동일 라벨을 재사용한다.

- **N < 2** 일 때: stringID 2065 ("최소 2명이 필요해요") 그대로. 색은 경고(예: `#ff9b9b` — 목업 톤). 나누기 버튼 `Interactable=false`.
- **N ≥ 2** 일 때: 배분 형태 미리보기. 나누기 버튼 `Interactable=true`. 색은 보조 텍스트 톤(`#b8b8c0`).
  - 나머지 `rem = N % T` 가 **0** 이면: stringID 2071 포맷 `"{0}명 → {1}팀 × {2}명"` → 예 `6명 → 2팀 × 3명`.
  - `rem > 0` 이면: stringID 2072 포맷 `"{0}명 → {1}팀 {2}명 · {3}팀 {4}명"`
    - `{0}`=N, `{1}`=`rem`(큰 팀 수), `{2}`=`floor(N/T)+1`, `{3}`=`T-rem`(작은 팀 수), `{4}`=`floor(N/T)` → 예 `7명 → 1팀 3명 · 2팀 2명`.
  - 미리보기는 **참가자 추가/삭제·팀수 변경 시마다** 갱신(나누기를 누르지 않아도 형태가 보임).

---

## 4. 결과 (Output)

### 4.1 표시 방식 — CHPoolingScrollView 2열 고정 높이 (Rule 03 §3)

결과는 **`CardScrollView` 에 `TeamCardPoolingScrollView : CHPoolingScrollView<TeamCard, TeamSplitTeamData>` 를 두고 `SetItemList(dataList)` 한 번 호출**하여 팀 카드를 2열 그리드로 배치한다. CHPoolingScrollView 가 origin 기반 풀·배치·바인딩을 일괄 처리. `CHMPool` 수동 Pop/Push 미사용.

**카드 안 멤버 목록 표시 방식 (확정): 단일 `CHText` 줄바꿈** — 멤버를 멤버 셀 풀로 또 그리지 않고, 카드 내부 `CHText` 한 칸에 멤버 이름을 `\n` 으로 줄바꿈 결합해 표시한다.

- 근거: LadderScene `FormatMappingRows` 선례(단일 CHText + `\n`/`\t` 결합). 풀링 중첩(카드 풀 × 멤버 셀 풀)을 피해 구현 복잡도·반환 누수 위험을 낮춘다.
- 멤버 셀 풀(카드 안에 멤버를 셀 단위로 풀링) 방식은 **P2 확장**으로 보류(§9).

### 4.2 TeamCard 프리팹 구성

`TeamCard` 한 장은 Dark UI 톤 패널(배경 `#34343c`)에 다음을 담는다 — 모두 `[SerializeField] private`, 외부엔 `Bind(...)` API 만 노출(Rule 02 §6.1):

| 요소 | 컴포넌트 | 내용 |
|---|---|---|
| 팀 색 뱃지 | `Image` | 팀 인덱스별 색(§4.3) 원형 점 |
| 팀 헤더 | `CHText` | stringID 2073 포맷 `"{0}팀"` → `1팀`/`2팀`… |
| 인원 뱃지 | `CHText` | stringID 2074 포맷 `"{0}명"` → `3명` |
| 멤버 목록 | `CHText` | 멤버 이름 `\n` 줄바꿈 결합 |

- 팀 카드 컴포넌트(예: `TeamCard : MonoBehaviour`)는 `Bind(int teamIndex, IList<string> members, Color color)` 하나로 위 4요소를 채운다.
- 카드 GameObject 는 `CHPoolingScrollView` 의 origin 셀로 동작한다(`CHMPool.Pop/Push` 미사용).
- `OnEnable` 에서 풀 재사용 리셋(이전 멤버 텍스트/색 잔존 제거).

### 4.3 팀 색 팔레트

팀 인덱스별 색을 순환 적용(LadderScene `_pathColors` 선례와 동형, 목업 `COLORS` 와 동일 톤). 카드 색 뱃지에 사용:

```
[0]#ff6b6b [1]#ffd64d [2]#5ad1c4 [3]#7aa2ff [4]#c08bff [5]#ff9f5a
[6]#6bd47f [7]#ff7bbd [8]#f06595 [9]#74c0fc [10]#ffa94d [11]#63e6be
```

(12색 — 참가자 상한 12와 동수. T 는 최대 N(=12)까지 가능하므로 12색이면 색 충돌 없음. `index % 12` 로 순환.)

### 4.4 배치

- 중앙 결과 영역에 **2열 그리드** 로 카드를 배치, 세로 스크롤. `TeamCardPoolingScrollView(_columnCount=2)` 가 CardScrollView 위에 있고, origin(TeamCard) 의 **고정 크기(550×320)** 를 읽어 2열 배치·풀 관리. `VerticalLayoutGroup`/`ContentSizeFitter`/`LayoutElement` 미사용.
- 카드 내부 자식은 **TOP 앵커** 로 배치: 헤더행(ColorBadge·HeaderText·CountText) y=-12, MemberText y=-50 · 높이 250(최대 6명 @40px/라인 240px 허용).
- 결과 전(초기/리셋) 상태: 빈 안내 문구 표시 — stringID 2075 `"참가자를 입력하고\n나누기를 누르세요"` (목업 `.empty`). 첫 나누기 후 빈 안내는 숨김.

---

## 5. UI / 화면 흐름

### 5.0 현재 UI 풍 준수 (필수)

> 그룹핑 결과는 룰렛으로 표현 불가 — 자체 결과. 외형·레이아웃·컴포넌트만 현재 앱 풍에 맞춘다. 승인된 목업 `.mockups/team_split.html` 이 시각 기준.

- **테마**: `Assets/Dark UI/` 다크 패널·버튼(배경 `#262626`·패널 `#2f2f37`·카드 `#34343c`·골드 `#ffd64d`). 팀 카드 동일 톤.
- **폰트**: `EFont.Jua` — 모든 텍스트 `CHText`.
- **컴포넌트**: 추가/삭제·스테퍼·나누기 `CHButton`, 라벨 `CHText`, 입력 `TMP_InputField`, 리스트 `CustomScrollView`.
- **레이아웃 관례**: 상단 메뉴(뒤로) 버튼 · 중앙 입력+팀 결과 · 하단 나누기 버튼.

### 5.1 배치 / RandomScene 와이어링

> 명칭 정합: 호스트는 현행 **`RandomScene`** (구 RouletteScene), 접근 인터페이스 **`IRandomSceneAccess`**, 와이어링 메서드 **`SetRandomSceneAccess(this)`**, 뒤로가기 인터페이스 **`IRouletteBackButton`**(현행 그대로 — 이름 변경 없음).

- `LadderScene` 와 동일하게 **`RandomScene` 의 자식 GameObject**(active 토글). 사용자가 Unity 에디터에서 Ladder GameObject 를 복사해 `TeamSplit` GameObject 로 만들어 둠 — `TeamSplitScene.cs` 는 거기 부착.
- 연결(`RandomScene.cs` 수정):
  1. `ERouletteMenu.TeamSplit` 추가(§6) — 호스트 `switch` 진입 키.
  2. `[SerializeField] private TeamSplitScene _teamSplitScene;` 필드 추가.
  3. `SetManagement()` 에 `_teamSplitScene.SetRandomSceneAccess(this);` 추가.
  4. `ShowScene()` 의 모든 화면 끄기 블록에 `_teamSplitScene.gameObject.SetActive(false);` 추가 + `case ERouletteMenu.TeamSplit:` 추가(`liMainSceneObj` 전부 `SetActive(false)` + `_teamSplitScene.gameObject.SetActive(true)` + `_mainRouletteUI = _teamSplitScene`).
- 복귀: `TeamSplitScene.Close()` → `gameObject.SetActive(false)` + `_randomSceneAccess.ShowScene(ERouletteMenu.Menu)` (LadderScene.Close 와 동일).
- `TeamSplitScene` 은 `IRouletteBackButton` 구현(`Close()`) + `SetRandomSceneAccess(IRandomSceneAccess)` 보유 (LadderScene 시그니처 그대로).

### 5.2 구성

- 상단: 메뉴(뒤로) 버튼 `CHButton` (`_menuButton`) — 제목 "팀 나누기"(stringID 2076) `CHText`.
- 입력존: 좌측 참가자 컬럼(`_playerInput` + `_playerPlusButton`/`_playerMinusButton` + `_playerScrollView` + 인원 카운트 `CHText`), 우측 팀 수 스테퍼(`_teamMinusButton`/`_teamValueText`/`_teamPlusButton`).
- 중앙: 팀 결과 영역(`TeamCardPoolingScrollView` 2열 고정 높이(550×320), Dark UI 패널) + 빈 안내 `CHText`.
- 하단: 배분 미리보기/안내 `CHText`(`_guideText`) + 나누기 `CHButton`(`_drawButton`).
- **상태 리셋**: `OnEnable` 에서 참가자 리스트·팀수(2)·결과 카드·미리보기 라벨 초기화 (LadderScene `ResetState` 패턴).

### 5.3 참가자 인원 카운트 라벨

참가자 컬럼 하단에 `CHText` 로 `"{0} / {1}"` 형태 인원 표기(목업 `0 / 12`). 포맷 stringID 2077 `"{0} / {1}"` → `5 / 12`. `{1}` = `MaxCount`(12).

---

## 6. Enum · 에셋 키 · 문자열 매핑

### 6.1 Enum / 에셋 키

| 종류 | 키 | 비고 |
|---|---|---|
| 메뉴 항목 | `CommonEnum.ERouletteMenu.TeamSplit` (**신규** — `Ladder` 다음에 추가) | 호스트 메뉴 진입 키 |
| 메뉴 아이콘 | `CommonEnum.EMenuIcon.MenuTeamSplit` (**신규** — `MenuLadder` 다음) | 아이콘 에셋 키 |
| 팀 카드 셀 | `TeamCard` (씬 내 origin 오브젝트) | CHPoolingScrollView origin 셀. `CHPoolable` 미사용 |
| 클릭 사운드 | `CommonEnum.EAudio.Click` (재사용) | `CHButton` hook 자동 |
| 폰트 | `CommonEnum.EFont.Jua` (재사용) | 현재 UI 풍 |
| 시각 테마 | `Assets/Dark UI/` (재사용) | — |

- **아이콘 에셋 준비**: `Assets/Dark UI/New Icons/White Friends.png` 를 식별자 안전 이름으로 `Assets/AddressableResource/Sprite/MenuTeamSplit.png` 로 복사(기존 `MenuLadder` 등과 동일 절차 — `EMenuIcon` 주석 참조). 의미 = 사람 그룹(팀 편성). `.meta` GUID 보존 불필요(신규 복사본).
- **명명 일치 강제**(Rule 03 §2): `EMenuIcon.MenuTeamSplit` ↔ `MenuTeamSplit.png`, `TeamCard` 프리팹 파일명 = 풀 prototype 참조명.

### 6.2 메뉴 카드 데이터 (MenuCardCatalog 1행 추가)

`MenuCardCatalog.BuildMenuCards()` 의 `Ladder` 행 다음에 1행 추가:

```
menu          = ERouletteMenu.TeamSplit
iconKey       = EMenuIcon.MenuTeamSplit
titleStringID = 2078   // "팀 나누기"
descStringID  = 2079   // "무작위로 팀 편성"
```

- 제목 문구: **"팀 나누기"** / "Team Split" (stringID 2078).
- 설명 문구: **"무작위로 팀 편성"** / "Random team split" (stringID 2079).

> 참고: 화면 상단 제목(2076)과 메뉴 카드 제목(2078)은 동일 문구지만 **별도 ID** 로 둔다(메뉴 카드 catalog 는 다른 도구도 제목/상단을 분리 운용 — Lotto1: 카드 2057 / 화면 별도). 단일 진실 충돌 아님(서로 다른 표시 위치).

### 6.3 String.json 신규 stringID (2070부터 순차)

> **할당 기준점 확정**: 사다리가 2061~2067 사용. **2068·2069 는 이미 존재**("참가자"/"Player", "결과"/"Result" — 사다리 입력 컬럼 헤더, prefab CHText `_stringID` 경유라 코드 grep 에 안 잡힘). 따라서 팀 나누기 신규 ID 는 **2070 부터** 시작한다.

**재사용(REUSE) — 새로 추가하지 않음**:

| stringID | 한글 | 영문 | 용도 |
|---|---|---|---|
| 2065 (기존) | 최소 2명이 필요해요 | At least 2 players are required | 참가자 N<2 안내(§3.2) |
| 2068 (기존) | 참가자 | Player | 참가자 컬럼 헤더 |

**신규 추가 (2070~2079, 연속 10개)**:

| stringID | 한글 | 영문 | 용도 |
|---|---|---|---|
| 2070 | 나누기 | Split | 나누기 버튼(§3.0) |
| 2071 | {0}명 → {1}팀 × {2}명 | {0} → {1} teams × {2} | 미리보기(나머지 0, §3.2) |
| 2072 | {0}명 → {1}팀 {2}명 · {3}팀 {4}명 | {0}: {1} teams of {2}, {3} teams of {4} | 미리보기(나머지>0, §3.2) |
| 2073 | {0}팀 | Team {0} | 팀 카드 헤더(§4.2) |
| 2074 | {0}명 | {0} | 팀 카드 인원 뱃지(§4.2) |
| 2075 | 참가자를 입력하고\n나누기를 누르세요 | Enter players and\ntap Split | 결과 영역 빈 안내(§4.4) |
| 2076 | 팀 나누기 | Team Split | 화면 상단 제목(§5.2) |
| 2077 | {0} / {1} | {0} / {1} | 참가자 인원 카운트(§5.3) |
| 2078 | 팀 나누기 | Team Split | 메뉴 카드 제목(§6.2) |
| 2079 | 무작위로 팀 편성 | Random team split | 메뉴 카드 설명(§6.2) |

> 구현자 주의: 각 ID 를 추가하기 전 String.json 에 해당 번호가 비어 있는지 재확인(2068·2069 처럼 선점된 ID 가 있을 수 있음). 본 표 작성 시점 2070~ 는 비어 있음을 확인했다.

---

## 7. ChvjPackage 연동 포인트 (Rule 03)

- **UI 컴포넌트**: `CHButton`/`CHText`/`TMP_InputField`/`CustomScrollView` (Rule 03 §3).
- **풀링**: 팀 카드는 `TeamCardPoolingScrollView(CHPoolingScrollView)` 가 관리 — `SetItemList()` 한 번 호출로 Pop/배치/Push 일괄 처리. `CHMPool` 수동 Pop/Push 미사용(Rule 03 §3).
- **사운드**: `CHMSound` Click hook(부팅 시 등록된 hook 사용 — 코드에서 별도 호출 불필요).
- **문자열**: `JsonManager.Instance.GetStringData(id)` 경유. 포맷 문자열은 `string.Format(GetStringData(id), ...)`.
- **순수 로직 분리**: 배분 로직은 MonoBehaviour 비의존 `static` 메서드로 분리(§10) — LadderScene 의 `BuildRungs`/`BuildPath`/`Trace` 선례. test-engineer 진입점.

---

## 8. 엣지 케이스 / 구현 메모

| 케이스 | 처리 |
|---|---|
| 참가자 0~1명 | 나누기 차단 — 버튼 `Interactable=false` + 안내 2065. `Split` 호출 안 함 |
| T > N | 입력 단계에서 `+` 버튼 비활성 + 추가/삭제 시 `Clamp(2, Max(2,N))` 재클램프 → T≤N 보장(빈 팀 방지) |
| N=2, T=2 | 1·1 배분(정상) |
| 균등 규칙 | `floor(N/T)` + 나머지 `N%T` 앞 팀부터 1명 → 인원 차 ≤ 1. 셔플과 배분 분리해 테스트(§10) |
| 동명이인 | 허용 — 이름 문자열로만 구분 안 함. 셔플·배분은 원소 단위라 중복 이름도 보존 |
| 빈/공백 이름 | 추가 시 `IsNullOrWhiteSpace` 무시(LadderScene `AddItem` 동일) |
| 재진입 | `OnEnable` 에서 리스트·팀수(2)·결과 카드 전부 리셋 |
| 화면 이탈 | `OnDisable` 에서 `_cardScrollView.Clear()` 호출 — CHPoolingScrollView 내부 풀 반환 |
| 재클릭 | 매 클릭 새 셔플 — `SetItemList()` 재호출로 CHPoolingScrollView 가 자동 갱신 (§3.0) |

---

## 9. 추후 확장 (P1 범위 밖)

- 팀별 인원 직접 지정(불균등 허용).
- 고정 멤버(특정 인원 같은 팀 강제) 제약.
- 팀명 직접 입력.
- **멤버 셀 풀링** — 카드 안 멤버를 단일 CHText 줄바꿈 대신 멤버 셀 단위 풀(아바타/삭제 버튼 등 멤버별 인터랙션 필요 시). 현재는 §4.1 단일 CHText 로 충분.

---

## 10. 균등 배분 순수 로직 명세 (test-engineer 진입점)

LadderScene 의 `BuildRungs(int n, System.Random rng)` 와 동형 — MonoBehaviour 비의존 `public static`, 렌더와 분리.

### 시그니처

```
public static List<List<string>> Split(IList<string> names, int teamCount, System.Random rng)
```

- `names`: 참가자 이름(순서 무관, 동명이인 허용). null/빈 → 빈 결과 정책은 아래 불변식 참조.
- `teamCount`: T (호출 전 `2 ~ N` 클램프 보장. 방어적으로 내부에서 `Clamp(1, names.Count)` 가능하나 정상 입력 가정).
- `rng`: `System.Random` 주입(테스트에서 고정 seed → 결정적). null 이면 셔플 생략(원순서 배분)하거나 호출부에서 `new System.Random()` 보장.

### 동작

1. `names` 를 **Fisher-Yates 셔플**(rng 사용) — 원본 비파괴(복사본 셔플 권장).
2. `base = floor(N / T)`, `rem = N % T`.
3. 팀 t(0-based)의 size = `base + (t < rem ? 1 : 0)` — 나머지를 **앞 팀부터 1명씩**.
4. 셔플된 순서대로 앞에서부터 각 팀 size 만큼 잘라 담는다.

### 불변식 (테스트 대상)

- 팀 수 == `teamCount` (= T).
- 각 팀 size ∈ { `floor(N/T)`, `floor(N/T)+1` }.
- size 가 `floor(N/T)+1` 인 팀 개수 == `N % T` (앞 팀부터).
- 팀 간 인원 차 ≤ 1.
- **전체 멤버 보존** — 모든 팀 멤버 합집합(multiset) == 입력 `names` (원소 누락·중복 추가 없음, 동명이인 카운트 보존).
- **결정성** — 동일 `names`·`teamCount`·동일 seed 의 `System.Random` → 동일 결과(셔플 재현 가능).
- 예시: N=7, T=3 → size 3·2·2. N=6, T=2 → 3·3. N=5, T=3 → 2·2·1.

---

## 11. Self-Review 체크 (작성자)

- Placeholder 잔존 0 — TBD/또는/적절히/§참조-본문비움 없음. 미정 수치 없음(전부 확정값).
- 스펙 커버리지 — 스펙 없음(uses_superpowers:false), 사용자 요구·기존 P1 기획서 직접 매핑. 목업의 모든 라벨(제목·참가자/팀수 헤더·인원카운트·스테퍼·미리보기·카드 헤더/인원/멤버·빈안내·나누기) → stringID 매핑 완료.
- 내부 일관성 — MaxCount=12 (목업·§2·§7 동일). stringID 2070~2079 + 재사용 2065/2068 충돌 없음. 색 12개 = 상한 12.
- 명명 일관성 — `TeamSplitScene`/`ERouletteMenu.TeamSplit`/`EMenuIcon.MenuTeamSplit`/`TeamCard`/`IRandomSceneAccess`/`SetRandomSceneAccess`/`IRouletteBackButton`/`Split(...)` 본문 전체 동일 표기.
- 스코프 — 단일 구현 단위(도구 1종 추가). 분할 불요.
- 구현 요청사항 완전성 — Enum 2종·Interface(현행 재사용)·에셋 키·프리팹 스키마·순수 로직 시그니처 명세.
- UI 목업 — `.mockups/team_split.html` 존재(사용자 승인 톤)와 본 기획서 수치 일치.
