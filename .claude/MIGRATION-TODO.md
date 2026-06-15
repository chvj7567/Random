# `.claude/` 마이그레이션 갱신 리스트

> `.claude/` 디렉터리는 **Project Lair**(5분 역방향 보스전 로그라이크)에서 그대로 가져온 것이며, 현재 저장소인 **Random**(룰렛/로또/랜덤 유틸 + 광고 앱)과 맞지 않는다. 아래는 실제 코드를 조사해 뽑은, 갱신이 필요한 항목 전체 리스트다.
>
> 작성일: 2026-06-15 / 조사 기준: `D:\Random` 실제 파일

---

## 0. 실제 프로젝트 ↔ `.claude` 가정 대조표

| 항목 | `.claude` 가 가정 (Lair) | **실제 (Random)** | 상태 |
|---|---|---|---|
| 프로젝트명 | Project Lair | **Random** (github.com/chvj7567/Random) | ❌ 불일치 |
| 컨셉 | 역방향 보스전 로그라이크 | **룰렛/로또/랜덤 숫자 + 커스텀 랜덤** 유틸 앱 (광고 포함) | ❌ 불일치 |
| Unity 버전 | 6000.0.68f1 | **6000.0.68f1** (버전업 후) | ✅ 일치 |
| URP | 17.0.4 | **17.0.4** (버전업 후) | ✅ 일치 |
| 코드 루트 | `Assets/_Lair/` | **`Assets/Scripts/`** | ❌ 불일치 |
| 네임스페이스 | `Lair` | **없음 (global namespace)** | ❌ 불일치 |
| 인프라 패키지 | `com.chvj.unityinfra` (ChvjPackage) | **없음** — 자체 Manager 들로 구현 | ❌ 불일치 |
| UI 래퍼 | `CHText`/`CHButton`/`CHToggle`/`CHMUI`/`UIBase`/`UIArg` | **자체 `UIBase`/`ButtonEx`/`CHScrollView`** (패키지 아님), CHText/CHButton **미사용** | ⚠️ 부분 |
| 에셋 로드 | `CHMResource` / Addressables Enum 키 | **자체 `ResourceManager`** + Addressables 사용 | ⚠️ 부분 |
| 풀링 | `CHMPool` | (확인 필요 — 자체 구현 여부) | ⚠️ 확인 |
| 테스트 폴더 | `Assets/_Lair/Tests/EditMode`·`PlayMode` | **없음** (`com.unity.test-framework`는 설치돼 있음) | ❌ 불일치 |
| 테스트 asmdef | `Lair`, `Lair.Tests.EditMode/PlayMode` | **asmdef 자체 없음 (Assembly-CSharp)** | ❌ 불일치 |
| docs 폴더 | `docs/design`·`qa-reports`·`superpowers/*` | **없음** | ❌ 불일치 |
| 도메인 데이터 | `BalanceConfig.asset`, `Art/Cards/` | **없음** | ❌ 불일치 |
| 컨셉서 | `docs/design/project_lair_concept.md` | **없음** | ❌ 불일치 |
| CLAUDE.md | (자유 양식 전제) | **없음** | ⚠️ 신규 작성 권장 |

### 실제 Random 프로젝트 구조 (참고)
```
Assets/Scripts/
  Manager/   AdmobManager · AudioManager · GameManagement · JsonManager · ResourceManager · UIManager
  Scene/     StartScene · LottoScene · Lotto2Scene · LottoMenuScene · RouletteScene
             RandomNumberScene · RandomExampleScene · CustomRandomScene
  UI/        RouletteItem · UIAlarm · UILoading · UILotto · ScrollView/
  Prefab/    Capture
  Util/      UIBase · UIRoulette · ButtonEx · CHScrollView · CommonEnum · Extension · SingletoneMonoBehaviour
```
주요 외부 의존: Addressables 2.2.2, GoogleMobileAds(Admob), Input System, URP 17.0.3.

---

## 1. `project.md` — 전면 재작성 (최우선) ✅ 완료 (2026-06-15)

> 메타 키 섹션 재작성 완료. 인프라는 **ChvjPackage(`com.chvj.unityinfra` v2.0.5, 소스: `D:/ChvjPackage/Packages/com.chvj.unityinfra/`)** 로 설정.
> ✅ 2026-06-15 버전업 반영: Unity 6000.0.68f1 / URP 17.0.4 / Addressables 2.8.1 (ChvjPackage 요구치와 정합 일치).
> ✅ 2026-06-15 패키지 임베드: `Packages/com.chvj.unityinfra/` 로 복사 완료 (Unity 자동 인식, manifest 등록 불필요).
> ✅ 2026-06-15 컨셉 키 정리: 유틸 앱이라 `concept_doc`·`stage_goal`·`concept_sections` 제거. `stage`=운영/유지보수만 유지.
> 협업 흐름·스킬 후보 표는 #6 결정(에이전트 존폐 등) 이후 정리 예정이라 이번엔 보류.

`.claude/project.md` 의 **거의 모든 필수 키**가 틀림. 새 값으로 교체:

- [ ] `name`: Project Lair → **Random**
- [ ] `one_liner`: 로그라이크 설명 → **룰렛/로또/랜덤 유틸 앱** 한 줄로
- [ ] `concept_doc`: `docs/design/project_lair_concept.md` → 실제 컨셉서 경로 (없으면 신규 작성 or 키 제거)
- [ ] `stage` / `stage_goal`: Lair v0.3 가설 → Random 의 실제 단계/목표로
- [ ] `concept_sections.*`: Lair 컨셉서 § 번호 — **전부 무효**, 제거 또는 재정의
- [ ] `engine`: 6000.0.68f1 / URP 17.0.4 → **6000.0.60f1 / URP 17.0.3**
- [ ] `namespace`: `Lair` → **없음**(global) 명시 or 새 네임스페이스 도입 결정
- [ ] `architecture`: MVVM → 실제 구조(Manager + Scene + UIBase) 반영 여부 결정
- [ ] `code_root`: `Assets/_Lair/` → **`Assets/Scripts/`**
- [ ] `test_paths.*`: `Assets/_Lair/Tests/*` → 실제 테스트 폴더 신설 시 그 경로 (현재 없음)
- [ ] `test_asmdef.*`: `Lair*` → 실제 asmdef (현재 Assembly-CSharp, asmdef 없음)
- [ ] `infrastructure.*`: `com.chvj.unityinfra` → **제거** 또는 "자체 Manager 사용" 으로 대체
- [ ] `docs.*`: `docs/...` 경로 — 폴더 생성하거나 경로 조정
- [ ] `balance_config_asset` / `card_data_folder`: **Lair 전용 — 제거**
- [ ] 후보 스킬 표(88~99행): 게임 개발 파이프라인 전제 → Random 작업 성격에 맞게 재검토

---

## 2. `rules/` — 갱신 필요 ✅ 완료 (2026-06-15)

> Rule 00: ChvjPackage 예시가 실제 인프라와 일치 → 변경 없음. Rule 01: 커밋 예시를 Random(룰렛/로또/광고) 도메인으로 교체. Rule 02: 코드 예시·CommonEnum/Interface 를 `namespace Random` + Random 타입으로 교체. Rule 03: ChvjPackage 유지(채택), Lair 예시(Slime/EMonster/CardSelection)를 Random(UILotto/EUI/AlarmPopup)으로 교체. Rule 04: 폴더 구조를 `Assets/AddressableResource/`(UI·Prefab·Audio·Font·Json) 실제 구조로 교체.


| 파일 | 문제 | 조치 |
|---|---|---|
| `00-project-meta-file.md` | 예시에 Lair/ChvjPackage 다수, 필수 키 표가 Lair 전제 | 예시 일반화 (Lair → Random) |
| `01-no-auto-commit.md` | 예시 커밋 메시지가 "영웅 HP 단계별 스킬" 등 Lair 게임 도메인 | 예시를 Random 도메인(룰렛/광고)으로 교체. **규칙 자체는 유효** |
| `02-csharp-style.md` | C# 스타일 규칙 자체는 범용 → **대체로 유효**. 단 예시·MVVM 강제·`Lair.Data` 네임스페이스 예시는 Random 에 맞게 | 네임스페이스/MVVM 적용 범위 결정, 예시 정리 |
| `03-chvjpackage.md` | **전체가 ChvjPackage 전제** — Random 엔 패키지 없음. `CHMResource`/`CHMUI`/`CHMPool`/`CHText`/`UIArg` 전부 미존재 | Random 의 자체 Manager(`ResourceManager`/`UIManager`/`AudioManager`/`UIBase`) 규칙으로 **재작성하거나 룰 폐기** |
| `04-unity-asset.md` | 폴더 구조가 `Assets/_Lair/Art/...` 전제, Enum=파일명 정책 | 실제 `Assets/` 구조(`AddressableResource`, `Resources`, `Scenes`)에 맞게 재작성 |

> ⚠️ Rule 03 이 가장 큰 작업. Random 은 ChvjPackage 미사용이므로 **"있는 룰을 Random 자체 매니저 규약으로 갈아끼울지 / 03 룰을 폐기할지"** 결정 필요.

---

## 3. `agents/` — Lair 도메인 의존 (6개 전부) ✅ 완료 (2026-06-15)

> 결정대로 6종 모두 유지, 용어만 일반화. 하드코딩 메뉴 경로 `Lair/Tests`·`Lair/Sim`·`lair-test-result.json` → `Random/...`·`random-test-result.json`. 제거된 `concept_sections`·없는 `concept_doc` 참조(game-designer·qa-simulator)는 "있을 때만"으로 graceful 처리. ChvjPackage API 참조는 패키지 채택으로 그대로 유효.


모든 에이전트가 `.claude/project.md` 를 진입점으로 읽으므로, **project.md 만 고치면 자동으로 따라오는 부분**이 많다. 단 본문에 Lair 고유 표현이 박힌 곳은 개별 수정 필요.

- [ ] `game-designer.md` — 카드·밸런스·시너지 등 로그라이크 전제. Random 에 "게임 디자이너" 역할이 필요한지부터 결정
- [ ] `qa-simulator.md` — "N판 헤드리스 시뮬레이션/밸런스" — Random(유틸 앱)엔 부적합 가능성. 폐기/재정의 검토
- [ ] `code-reviewer.md` — Rule 00~04 준수 검토. Rule 03 갱신에 연동
- [ ] `test-engineer.md` — EditMode/PlayMode 테스트. 테스트 인프라 신설 여부에 연동
- [ ] `gameplay-programmer.md` — `_Lair`/`Lair` 네임스페이스/ChvjPackage 언급 정리
- [ ] `design-reviewer.md` — 기획서 검토. game-designer 존속 여부에 연동

> 핵심 결정: **Random 은 "게임"이 아니라 유틸 앱**이다. game-designer / design-reviewer / qa-simulator 3개 에이전트가 이 프로젝트에 의미가 있는지부터 판단해야 함.

---

## 4. `skills/` — 파이프라인 스킬 4종 ✅ 완료 (2026-06-15)

> 4종 모두 "Project Lair" → "Random" 으로 교체. Random 에 없는 `CLAUDE.md §5/§6/§7/§8/§9` 참조를 실제 소스(`.claude/rules/` Rule 00~04, `project.md` 「협업 흐름」, Rule 00 메인 오케스트레이터 행동 규칙)로 교체.


- [ ] `start-develop` / `-auto` / `-simple` / `-quick` 의 SKILL.md 본문에 Lair/ChvjPackage 언급 정리
- [ ] 위 에이전트 존폐 결정에 따라 파이프라인 단계 재구성 (예: game-designer 제거 시 흐름 단축)

---

## 5. 신규 작성 권장

- [ ] `CLAUDE.md` (현재 없음) — Random 프로젝트용 사람-읽기 문서
- [ ] `docs/` 폴더 — project.md 가 참조하는 design/qa-reports 경로 (사용할 경우)
- [ ] 컨셉서 — 유틸 앱이라 불필요할 수 있음. project.md `concept_doc` 키 처리와 함께 결정

---

## 6. 사용자 결정 필요 항목 (작업 전 확정)

1. ~~ChvjPackage(Rule 03) 처리~~ → **결정됨: ChvjPackage 채택**. Rule 03 은 유지하되 Random 실제 매니저(`ResourceManager`/`UIManager` 등)와 패키지 API 간 정합만 점검. (남은 작업: manifest.json 물리 등록 + 기존 자체 매니저 → 패키지 API 마이그레이션 범위 결정)
2. ~~네임스페이스~~ → **결정됨: `Random` 네임스페이스 도입** (신규 코드부터, 기존 전역 코드 점진 이관). 룰 예시 반영 완료.
3. ~~게임 전용 에이전트 3종~~ → **결정됨: 6종 모두 유지, 용어만 일반화**. 반영 완료.
4. ~~테스트 인프라~~ → **결정: 나중에 신설** (지금은 project.md 권장 경로만 유지, 첫 테스트 시 test-engineer 가 셋업)
5. ~~superpowers 흐름~~ → **결정·완료: `uses_superpowers: false`** (brainstorming/writing-plans 생략, 간이 흐름 — game-designer 부터 시작)
6. ~~ChvjPackage 물리 등록~~ → **결정·완료: 임베드 복사** (`Packages/com.chvj.unityinfra/`)
   - ✅ **모듈 구성(정정)**: 8개 모듈 전부 임베드 (Ads·Audio·Core·Iap·Pool·Resource·Social·UI). Ads/Iap/Social 은 각자 자체 asmdef + `defineConstraints`(`UNITY_INFRA_ADS`/`_IAP`/`_SOCIAL`)로 게이트 → define 미설정 시 컴파일 대상에서 제외되어 **빌드를 깨지 않음**(휴면). 필요 시 해당 SDK 설치 + Tools/ChvjUnityInfra/Settings 에서 define 활성화로 켠다.
     - ⚠️ 정정 경위: 처음엔 "단일 asmdef"로 오판해 Iap/Social 을 삭제했으나, 실제로는 모듈별 asmdef define 게이트라 삭제가 불필요했음 → 소스에서 복원 완료.
   - ⚠️ **컴파일 미검증**: Unity 에디터로 임포트/컴파일 실제 확인 필요(이 환경에서 에디터 실행 불가).
   - ⚠️ **코드 중복**: Random 은 이미 자체 infra(전역 `UIBase`·`UIArg`·`ResourceManager`·`AudioManager`·`CHScrollView`·`PoolingScrollViewItem` 등)를 들고 있고, 패키지는 `ChvjUnityInfra.*` 로 격리됨 → 컴파일 충돌은 없으나 **기능 중복**. 실제로 패키지 API(`CHMResource`/`CHMUI`/`CHMPool` 등)로 갈아끼우고 자체 사본을 제거하는 **코드 마이그레이션은 별도 큰 작업**(미착수). `using ChvjUnityInfra;` 추가 시 `UIBase`/`UIArg` 등 이름 모호성 주의.
7. ~~stage/stage_goal/concept_doc~~ → **결정·완료: 안 맞는 키 삭제** (concept_doc·stage_goal·concept_sections 제거)

---

## 우선순위 요약

1. **project.md 전면 재작성** (1번) — 모든 에이전트의 진입점이라 효과가 가장 큼
2. **Rule 03 (ChvjPackage) 결정 및 처리** (2번) — 실제와 가장 크게 어긋남
3. Rule 01/02/04 예시 정리 (2번)
4. 에이전트/스킬은 project.md + 결정사항 확정 후 일괄 정리 (3·4번)
