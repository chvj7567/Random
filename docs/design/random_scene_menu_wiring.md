# RandomScene 메뉴 UI — Unity 수동 와이어링 체크리스트

> 코드(.cs)·String.json 은 완료·리뷰 통과됨. 이 환경엔 Unity 에디터가 없어 아래 prefab/씬/아이콘/meta 작업은 **사용자가 Unity 에서 직접** 해야 메뉴가 실제로 그려진다. 기획서: `random_scene_menu.md`.
>
> 확정 사항: 세로 스크롤 / RandomFood 카드 제목="예시 룰렛"(stringID 154, 설명 155) / 아이콘 미실존분은 실존 에셋 폴백.

## A. 신규 .cs 4개의 .meta 생성
Unity 에디터를 열면 아래 4개 신규 스크립트의 `.meta` 가 자동 생성된다. 생성 후 `git add` 로 함께 스테이징(Rule 01 — 신규 파일 .meta 동행).
- `Assets/Scripts/UI/ScrollView/MenuPanel.cs`
- `Assets/Scripts/UI/ScrollView/MenuCardPoolingScrollView.cs`
- `Assets/Scripts/UI/ScrollView/MenuCardCell.cs`
- `Assets/Scripts/UI/ScrollView/MenuCardCatalog.cs`

## B. 아이콘 스프라이트 등록 (`Assets/AddressableResource/Sprite/`)
1. Dark UI 원본을 식별자 안전 이름으로 **복사**(.meta 동행, GUID 보존):
   - `Assets/Dark UI/New Icons/White A1.png` → `Sprite/MenuNumber.png`
   - `Assets/Dark UI/New Icons/White Apple.png` → `Sprite/MenuFood.png`
   - `Assets/Dark UI/New Icons/White Cycle.png` → `Sprite/MenuShuffle.png`  (Shuffle 미실존 폴백 — Cycle=섞기/순환 의미)
   - `Coin.png` 은 이미 `Sprite/` 에 존재 — 그대로 사용
2. 각 PNG 의 Texture Type = `Sprite (2D and UI)` 확인
3. 4개 스프라이트를 Addressables 에 등록 — **주소 = 파일명**(MenuNumber/MenuFood/MenuShuffle/Coin), 라벨 = `Resource`(CHMResource 기본 라벨). Rule 03 §2 (EMenuIcon 값명 = 파일명).

## C. MenuCardCell.prefab 생성 (`Assets/AddressableResource/Prefab/MenuCardCell.prefab`)
- 루트 `MenuCardCell`: RectTransform(1180×168) + Image(카드 면 `#34343c`, radius 24) + Button + **CHButton** + **MenuCardCell**(스크립트)
- 자식 `Icon`: RectTransform(96×96, 좌패딩 36) + Image → 셀 `_icon`
- 자식 `Title`: RectTransform + TextMeshProUGUI(44px, 흰색, 좌정렬) + **CHText**(`_stringID = -1`) → 셀 `_title`
- 자식 `Desc`: RectTransform + TextMeshProUGUI(26px, `#b8b8c0`, 좌정렬) + **CHText**(`_stringID = -1`) → 셀 `_desc`
- 자식 `Chevron`(선택): TextMeshProUGUI "›" 32px `#6f6f78`
- 셀 `[SerializeField]`: `_icon`/`_title`/`_desc`/`_button`(루트 CHButton) 4개 모두 인스펙터 연결
- CHButton 은 같은 GameObject 의 Button 을 요구 → 루트에 Button+CHButton 동반

## D. RandomScene.unity 씬 구성
1. 기존 절대배치 메뉴 버튼 4개(`CoinMenuButton` 등) + `MenuGroup` 흩뿌린 배치 **제거**
2. 타이틀 영역(높이 220) GameObject 2개:
   - `TitleLabel`: TextMeshProUGUI(56px 흰색) + **CHText** `_stringID = 150`
   - `SubtitleLabel`: TextMeshProUGUI(26px `#b8b8c0`) + **CHText** `_stringID = 151`
3. 리스트 영역 계층:
   ```
   MenuPanel (GameObject)            ← MenuPanel 스크립트
   └─ ScrollView                     ← ScrollRect + MenuCardPoolingScrollView 스크립트
      ├─ Viewport                    ← Image(mask) + RectMask2D
      │  └─ Content                  ← LayoutGroup 붙이지 말 것 (아래 주의)
      │     └─ MenuCardCell (origin)  ← C 의 prefab 인스턴스 1개(prototype)
   ```
4. `MenuCardPoolingScrollView` 인스펙터:
   - `_origin` → Content 자식의 origin MenuCardCell 인스턴스
   - `_itemGap` = (0, **24**)
   - `_padding` = left **50** / right **50** / top **0** / bottom **120**
   - `_scrollDirection` = **Vertical**
   - `_align` = **LeftOrTop** (Center 면 카드가 우측으로 ~100px 밀려 여백 깨짐)
   - `_columnCount` = **1**
   - `_poolItemCount` = 0(자동) 또는 12 / `_refresh` = false
5. `ScrollView` RectTransform: 타이틀(220) 아래 ~ 하단 배너 위까지
6. `MenuPanel._scrollView` ← 자식 ScrollView 드래그
7. `RandomScene._menuPanel` ← MenuPanel GameObject 드래그
8. `liMainSceneObj` 에 메뉴 화면 구성요소(타이틀 라벨들 + MenuPanel) 등록 (메뉴↔서브씬 토글 대상)

> **주의 — 의도적 deviation**: 기획서 §7.5 는 "Content 에 VerticalLayoutGroup" 을 명시하나, `CHPoolingScrollView` 가 `InitItemTransform` 에서 `anchoredPosition` 을 직접 계산·배치하므로 **활성 LayoutGroup 을 붙이면 충돌**한다. 간격/패딩은 LayoutGroup 이 아니라 ScrollView 컴포넌트 필드(`_itemGap`/`_padding`)로 준다. (code-reviewer 가 인프라 동작과 일치하는 정당한 deviation 으로 검증함.)
>
> origin MenuCardCell 인스턴스에서 컴포넌트 제거 금지(`m_RemovedComponents`) — `_icon`/`_title` 참조 null → 시각 깨짐 (Rule 03 §3).

## 완료 후
- 컴파일 에러 0 확인 → 신규 .cs 4개 `.meta` + 아이콘 PNG·meta + MenuCardCell.prefab + RandomScene.unity 변경을 `git add`
- 메뉴 화면 진입 → 카드 4종(랜덤 숫자 / 예시 룰렛 / 커스텀 랜덤 / 동전 던지기) 렌더 + 탭 시 해당 모드 전환 확인
