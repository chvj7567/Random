# Tests~ (휴면 테스트 보관소)

이 폴더는 이름이 `~` 로 끝나 **Unity 가 컴파일·임포트에서 무시**한다. 리포지토리에는 남지만 에디터 빌드에는 영향이 없다.

## 왜 여기 있나
결정 도우미 `coin_flip` 의 EditMode 테스트(`CoinFlipTests.cs`), `ladder`(사다리타기) 순열 로직 테스트(`LadderTests.cs`), `team_split`(팀 나누기) 균등 배분 로직 테스트(`TeamSplitTests.cs`)를 작성했으나, 현재 프로젝트는 게임 코드가 asmdef 없는 단일 `Assembly-CSharp` 다. 테스트 어셈블리는 `Assembly-CSharp` 를 참조할 수 없어(Unity 컴파일 규칙) 테스트를 컴파일하려면 production asmdef 신설이 필요하다 — 이는 모든 의존성(ChvjUnityInfra·DOTween·Admob·UniRx·Addressables·InputSystem·TMP)을 명시 참조로 옮기는 광범위 변경이라, 우선 **테스트를 휴면 보관**하기로 결정했다(coin_flip 2026-06-17, ladder 2026-06-22, team_split 2026-06-23).

세 테스트 파일 모두 같은 `Random.Tests.EditMode` 어셈블리(`Random.Tests.EditMode.asmdef.template`) 하에 들어간다 — 도구별 별도 asmdef 신설 불필요. template 이 이미 production `"Random"` 을 참조하므로 부활 시 대상 Scene 들도 함께 컴파일된다.

> **휴면 검증 보강(team_split)**: `TeamSplitScene.Split` 은 Unity 의존이 전혀 없는 순수 메서드(`List`/`IList`/`System.Random`)라, 휴면 상태에서도 `Split` 을 throwaway dotnet 콘솔에 verbatim 복사해 본 테스트의 단언을 런타임 검증했다 — 3519 체크 PASS / 0 FAIL (.NET 9). Unity Test Runner 부활 전 단언값 오류·multiset 비교 실수를 사전 차단. (결정성 테스트는 seed→배열 절대값을 박지 않고 동일 seed 상대 비교만 — Mono/CoreCLR 난수열 차이에 무관하게 부활 후에도 유효.)

## 부활 절차 (전용 테스트 인프라 작업 시)
1. `Assets/Scripts/` 에 production asmdef 신설 — 이름 `Random`. 위 의존성을 모두 명시 참조로 추가(누락 시 production 컴파일 깨짐).
2. 이 폴더를 `Assets/Tests~` → `Assets/Tests` 로 되돌린다(폴더명에서 `~` 제거).
3. `Random.Tests.EditMode.asmdef.template` → `Random.Tests.EditMode.asmdef` 로 이름 변경. `references` 에 `"Random"` 이 들어 있는지 확인(이미 박혀 있음).
4. Unity 에서 `.meta` 자동 생성 후 Test Runner(EditMode)로 실행.

## 대상 테스트 진입점 (production)
- `CoinFlipTests.cs` → `CoinFlipScene.Flip()` / `FlipMany(int)` / `CountFaces(List<ECoinFace>)`, `enum ECoinFace { Head, Tail }`
- `LadderTests.cs` → `LadderScene.LevelCount(int)` / `BuildRungs(int, System.Random)` / `BuildPath(int, bool[,], int)` / `Trace(int, bool[,], int)` (모두 public static, GameObject 불필요)
- `TeamSplitTests.cs` → `TeamSplitScene.Split(IList<string>, int, System.Random)` (public static, GameObject 불필요)
