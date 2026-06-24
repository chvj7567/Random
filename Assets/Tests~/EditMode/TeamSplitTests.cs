using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Random.Tests
{
    //# team_split(팀 나누기) P1 — 연출과 분리된 균등 배분 순수 로직 테스트 (기획서 §10).
    //# 대상: TeamSplitScene.Split(IList<string>, int, System.Random) — static, GameObject 불필요.
    //# 불변식: 팀수 보존 · 각 팀 size ∈ {floor,floor+1} · 큰 팀(앞쪽) 개수==N%T · 인원차≤1 · 멤버 multiset 보존 · 결정성.
    //# 결정성은 LadderTests 선례대로 "동일 seed → 동일 결과"(상대 비교)로 단언 — seed→배열 하드코딩 금지(Mono/CoreCLR 난수열 상이).
    public class TeamSplitTests
    {
        //# 본 시스템 입력 범위 (TeamSplitScene MinCount=2, MaxCount=12 / T ∈ 2..N).
        private const int MinN = 2;
        private const int MaxN = 12;

        //# === 헬퍼 ===

        //# 0..n-1 의 distinct 이름 리스트 ("p0".."p{n-1}"). 스윕/구조 테스트용.
        private static List<string> MakeNames(int n)
        {
            List<string> names = new List<string>();
            for (int i = 0; i < n; i++)
            {
                names.Add("p" + i);
            }
            return names;
        }

        //# 시드 고정 rng — 결정적 재현용.
        private static System.Random Rng(int seed)
        {
            return new System.Random(seed);
        }

        //# === 정상 배분 — 명시적 케이스 (기획서 §10 예시) ===

        //# 큰 팀이 앞. 7/3→3·2·2, 6/2→3·3, 5/3→2·2·1 등. size 패턴은 셔플과 무관한 결정값이라 정확 단언.
        //# expected 는 기획서 §10 예시를 그대로 박은 하드코딩 오라클(CSV) — production 공식을 재계산하지 않는다.
        //# CSV(string) 로 전달: int[] TestCase 인자는 NUnit 디스커버리에서 splat 될 수 있어 회피.
        [Test]
        [TestCase(7, 3, "3,2,2")]
        [TestCase(6, 2, "3,3")]
        [TestCase(5, 3, "2,2,1")]
        [TestCase(2, 2, "1,1")]
        [TestCase(12, 5, "3,3,2,2,2")]
        public void Split_팀크기패턴이_큰팀앞으로_정확하다(int n, int t, string expectedCsv)
        {
            int[] expected = expectedCsv.Split(',').Select(int.Parse).ToArray();

            List<List<string>> teams = TeamSplitScene.Split(MakeNames(n), t, Rng(123));

            Assert.AreEqual(t, teams.Count, "팀 수는 teamCount 와 같아야 한다.");
            for (int i = 0; i < t; i++)
            {
                Assert.AreEqual(expected[i], teams[i].Count,
                    "팀 " + i + " 의 인원이 기대값과 다름. n=" + n + " t=" + t);
            }
        }

        //# 7/3 의 구체값(3·2·2)을 기획서 예시 그대로 한 번 더 직접 박제 (회귀 고정).
        [Test]
        public void Split_7명3팀이면_3_2_2다()
        {
            List<List<string>> teams = TeamSplitScene.Split(MakeNames(7), 3, Rng(0));

            Assert.AreEqual(3, teams.Count, "3팀이어야 한다.");
            Assert.AreEqual(3, teams[0].Count, "1팀(앞)은 3명.");
            Assert.AreEqual(2, teams[1].Count, "2팀은 2명.");
            Assert.AreEqual(2, teams[2].Count, "3팀은 2명.");
        }

        //# === 팀 수 보존 ===

        [Test]
        [TestCase(2, 2)]
        [TestCase(7, 3)]
        [TestCase(12, 4)]
        [TestCase(12, 12)]
        public void Split_반환_팀수가_teamCount와_같다(int n, int t)
        {
            List<List<string>> teams = TeamSplitScene.Split(MakeNames(n), t, Rng(7));

            Assert.AreEqual(t, teams.Count, "반환 리스트 길이는 teamCount 여야 한다. n=" + n + " t=" + t);
        }

        //# === 균등 규칙 — size ∈ {base, base+1}, 큰 팀 개수 == N%T, 위치는 앞 팀부터, 인원차 ≤ 1 ===

        [Test]
        [TestCase(7, 3)]
        [TestCase(6, 2)]
        [TestCase(5, 3)]
        [TestCase(11, 4)]
        [TestCase(12, 5)]
        public void Split_각팀크기가_base_또는_base플러스1이다(int n, int t)
        {
            int baseSize = n / t;

            for (int seed = 0; seed < 20; seed++)
            {
                List<List<string>> teams = TeamSplitScene.Split(MakeNames(n), t, Rng(seed));

                foreach (List<string> team in teams)
                {
                    Assert.IsTrue(team.Count == baseSize || team.Count == baseSize + 1,
                        "팀 size 는 floor(N/T) 또는 +1 이어야 한다. n=" + n + " t=" + t + " seed=" + seed + " size=" + team.Count);
                }
            }
        }

        [Test]
        [TestCase(7, 3)]
        [TestCase(11, 4)]
        [TestCase(12, 5)]
        [TestCase(10, 3)]
        public void Split_큰팀개수가_나머지N퍼센트T와_같다(int n, int t)
        {
            int baseSize = n / t;
            int rem = n % t;

            List<List<string>> teams = TeamSplitScene.Split(MakeNames(n), t, Rng(99));

            int bigTeams = teams.Count(team => team.Count == baseSize + 1);
            Assert.AreEqual(rem, bigTeams, "큰 팀(size+1) 개수는 N%T 여야 한다. n=" + n + " t=" + t);
        }

        //# 큰 팀이 "앞쪽"에 모인다 — size 가 비증가(non-increasing) 수열인지 확인 (count 만으론 못 잡음).
        [Test]
        [TestCase(7, 3)]
        [TestCase(11, 4)]
        [TestCase(12, 5)]
        [TestCase(10, 3)]
        [TestCase(8, 3)]
        public void Split_큰팀이_앞쪽에_배치된다(int n, int t)
        {
            int baseSize = n / t;
            int rem = n % t;

            List<List<string>> teams = TeamSplitScene.Split(MakeNames(n), t, Rng(55));

            for (int i = 0; i < teams.Count; i++)
            {
                int expected = baseSize + (i < rem ? 1 : 0);
                Assert.AreEqual(expected, teams[i].Count,
                    "팀 " + i + " 크기가 앞-큰-팀 규칙과 다름 (큰 팀은 앞쪽이어야 함). n=" + n + " t=" + t);
            }

            //# 비증가 수열인지 한 번 더 (앞 큰 팀 규칙의 결과).
            for (int i = 1; i < teams.Count; i++)
            {
                Assert.GreaterOrEqual(teams[i - 1].Count, teams[i].Count,
                    "팀 크기는 앞에서 뒤로 비증가여야 한다. n=" + n + " t=" + t + " i=" + i);
            }
        }

        [Test]
        [TestCase(7, 3)]
        [TestCase(12, 5)]
        [TestCase(11, 4)]
        [TestCase(10, 4)]
        public void Split_팀간_인원차가_1이하다(int n, int t)
        {
            for (int seed = 0; seed < 20; seed++)
            {
                List<List<string>> teams = TeamSplitScene.Split(MakeNames(n), t, Rng(seed));

                int min = teams.Min(team => team.Count);
                int max = teams.Max(team => team.Count);

                Assert.LessOrEqual(max - min, 1,
                    "팀 간 인원 차는 1 이하여야 한다. n=" + n + " t=" + t + " seed=" + seed + " (max=" + max + " min=" + min + ")");
            }
        }

        //# === 멤버 보존 (multiset) — 누락·중복추가·창작 없음 ===

        //# distinct 이름 스윕은 set 기준으로도 통과하므로, 여기선 multiset(정렬 후 element-wise) 으로 비교한다.
        [Test]
        [TestCase(7, 3)]
        [TestCase(12, 4)]
        [TestCase(5, 5)]
        public void Split_전체멤버가_입력과_정확히_일치한다_multiset(int n, int t)
        {
            List<string> names = MakeNames(n);

            for (int seed = 0; seed < 20; seed++)
            {
                List<List<string>> teams = TeamSplitScene.Split(names, t, Rng(seed));

                List<string> flat = teams.SelectMany(team => team).ToList();

                Assert.AreEqual(names.Count, flat.Count,
                    "전체 배분 인원이 입력 수와 달라짐. n=" + n + " t=" + t + " seed=" + seed);

                List<string> sortedInput = names.OrderBy(s => s, System.StringComparer.Ordinal).ToList();
                List<string> sortedOutput = flat.OrderBy(s => s, System.StringComparer.Ordinal).ToList();

                CollectionAssert.AreEqual(sortedInput, sortedOutput,
                    "배분 멤버 multiset 이 입력과 다름 (누락/창작/중복). n=" + n + " t=" + t + " seed=" + seed);
            }
        }

        //# 동명이인 — 같은 이름 2개 입력 시 2개 모두 보존 (set 으로 dedup 되지 않음). 스윕이 구조적으로 못 잡는 케이스.
        [Test]
        public void Split_동명이인이_개수그대로_보존된다()
        {
            //# "김"3 "이"2 "박"1 — 총 6명, 3팀.
            List<string> names = new List<string> { "김", "김", "김", "이", "이", "박" };

            for (int seed = 0; seed < 30; seed++)
            {
                List<List<string>> teams = TeamSplitScene.Split(names, 3, Rng(seed));

                List<string> flat = teams.SelectMany(team => team).ToList();

                Assert.AreEqual(6, flat.Count, "전체 인원 6명 보존. seed=" + seed);
                Assert.AreEqual(3, flat.Count(s => s == "김"), "동명이인 '김' 3개가 그대로 보존되어야 한다. seed=" + seed);
                Assert.AreEqual(2, flat.Count(s => s == "이"), "동명이인 '이' 2개가 그대로 보존되어야 한다. seed=" + seed);
                Assert.AreEqual(1, flat.Count(s => s == "박"), "'박' 1개 보존. seed=" + seed);
            }
        }

        //# 같은 사람이 두 팀에 동시 배정되지 않음 — 전체 배분 수 == 입력 수 면 중복 배정 0 (이미 multiset 으로 보장되나 명시).
        [Test]
        public void Split_한명이_두팀에_중복배정되지_않는다()
        {
            List<string> names = MakeNames(9);

            for (int seed = 0; seed < 20; seed++)
            {
                List<List<string>> teams = TeamSplitScene.Split(names, 4, Rng(seed));

                int total = teams.Sum(team => team.Count);
                Assert.AreEqual(names.Count, total,
                    "배분 총합이 입력 수와 같아야 중복 배정/누락이 없다. seed=" + seed);
            }
        }

        //# === 원본 비파괴 (기획서 §10 "원본 비파괴") ===

        [Test]
        public void Split_입력리스트를_변형하지_않는다()
        {
            List<string> names = MakeNames(8);
            List<string> snapshot = new List<string>(names);

            TeamSplitScene.Split(names, 3, Rng(42));

            CollectionAssert.AreEqual(snapshot, names,
                "Split 호출 후 입력 리스트의 순서/내용이 그대로여야 한다 (복사본 셔플).");
        }

        //# === 결정성 — 같은 seed → 같은 결과 (상대 비교, 절대값 하드코딩 금지) ===

        [Test]
        [TestCase(7, 3)]
        [TestCase(12, 4)]
        [TestCase(9, 5)]
        [TestCase(2, 2)]
        public void Split_같은시드면_동일한_분배를_만든다(int n, int t)
        {
            List<string> names = MakeNames(n);

            List<List<string>> a = TeamSplitScene.Split(names, t, Rng(2024));
            List<List<string>> b = TeamSplitScene.Split(names, t, Rng(2024));

            Assert.AreEqual(a.Count, b.Count, "팀 수가 같아야 한다.");
            for (int i = 0; i < a.Count; i++)
            {
                CollectionAssert.AreEqual(a[i], b[i],
                    "같은 시드인데 팀 " + i + " 의 멤버/순서가 다름. n=" + n + " t=" + t);
            }
        }

        //# === 무작위성 (약하게) — 다른 seed 다수 → 분배가 고정되지 않음 (sanity, N≥6) ===
        //# N=2/3 은 배치 경우의 수가 적어 "전부 동일"이 우연히 성립할 수 있으므로 N≥6 에서만 검사 (flaky 방지).

        [Test]
        public void Split_다른시드면_대체로_다른_분배를_만든다()
        {
            List<string> names = MakeNames(8);

            //# baseline(seed 0) 의 팀0 멤버 순서를 직렬화. 20개 시드 중 최소 하나는 달라야 한다.
            List<List<string>> baseline = TeamSplitScene.Split(names, 3, Rng(0));
            string baseKey = Serialize(baseline);

            bool anyDifference = false;
            for (int seed = 1; seed <= 20 && anyDifference == false; seed++)
            {
                List<List<string>> other = TeamSplitScene.Split(names, 3, Rng(seed));
                if (Serialize(other) != baseKey)
                {
                    anyDifference = true;
                }
            }

            Assert.IsTrue(anyDifference,
                "20개 시드 모두 seed=0 과 동일한 분배를 만들었다 — 셔플이 시드를 반영하지 못함.");
        }

        private static string Serialize(List<List<string>> teams)
        {
            return string.Join("|", teams.Select(team => string.Join(",", team)));
        }

        //# === 스윕 — N=2..12 × T=2..N 전 조합에서 크기·보존 불변식 유지 ===

        [Test]
        public void Split_전조합_스윕_크기와보존_불변식_유지()
        {
            for (int n = MinN; n <= MaxN; n++)
            {
                List<string> names = MakeNames(n);
                List<string> sortedInput = names.OrderBy(s => s, System.StringComparer.Ordinal).ToList();

                for (int t = 2; t <= n; t++)
                {
                    int baseSize = n / t;
                    int rem = n % t;

                    //# 시드 2개로 — 크기 불변식은 셔플 무관이라 한 시드면 충분하나 보존은 여러 시드로.
                    for (int seed = 0; seed < 5; seed++)
                    {
                        List<List<string>> teams = TeamSplitScene.Split(names, t, Rng(seed));

                        //# 팀 수.
                        Assert.AreEqual(t, teams.Count,
                            "스윕 팀 수 위반. n=" + n + " t=" + t + " seed=" + seed);

                        //# 앞 큰 팀 규칙 — 위치별 정확 크기.
                        for (int i = 0; i < t; i++)
                        {
                            int expected = baseSize + (i < rem ? 1 : 0);
                            Assert.AreEqual(expected, teams[i].Count,
                                "스윕 크기 위반. n=" + n + " t=" + t + " team=" + i + " seed=" + seed);
                        }

                        //# 인원차 ≤ 1.
                        int min = teams.Min(team => team.Count);
                        int max = teams.Max(team => team.Count);
                        Assert.LessOrEqual(max - min, 1,
                            "스윕 인원차 위반. n=" + n + " t=" + t + " seed=" + seed);

                        //# multiset 보존.
                        List<string> flat = teams.SelectMany(team => team).ToList();
                        List<string> sortedOutput = flat.OrderBy(s => s, System.StringComparer.Ordinal).ToList();
                        CollectionAssert.AreEqual(sortedInput, sortedOutput,
                            "스윕 멤버 보존 위반. n=" + n + " t=" + t + " seed=" + seed);
                    }
                }
            }
        }

        //# === 경계 / 계약 ===

        //# T==N — 각 팀 정확히 1명.
        [Test]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(12)]
        public void Split_팀수가_참가자수와_같으면_각팀_1명이다(int n)
        {
            List<List<string>> teams = TeamSplitScene.Split(MakeNames(n), n, Rng(11));

            Assert.AreEqual(n, teams.Count, "팀 수는 N 이어야 한다.");
            foreach (List<string> team in teams)
            {
                Assert.AreEqual(1, team.Count, "T==N 이면 각 팀 1명이어야 한다. n=" + n);
            }
        }

        //# N==2, T==2 — 1·1 최소 케이스.
        [Test]
        public void Split_2명2팀이면_각팀_1명이다()
        {
            List<List<string>> teams = TeamSplitScene.Split(MakeNames(2), 2, Rng(0));

            Assert.AreEqual(2, teams.Count, "2팀이어야 한다.");
            Assert.AreEqual(1, teams[0].Count, "1팀 1명.");
            Assert.AreEqual(1, teams[1].Count, "2팀 1명.");
        }

        //# 빈 입력 / null — teamCount 개의 빈 팀 반환 (기획서 §10 정책, 코드가 명시 처리).
        [Test]
        public void Split_빈리스트면_teamCount개의_빈팀을_반환한다()
        {
            List<List<string>> teams = TeamSplitScene.Split(new List<string>(), 3, Rng(0));

            Assert.AreEqual(3, teams.Count, "빈 입력에도 팀 수는 teamCount.");
            foreach (List<string> team in teams)
            {
                Assert.AreEqual(0, team.Count, "빈 입력이면 모든 팀이 비어 있어야 한다.");
            }
        }

        [Test]
        public void Split_null입력이면_teamCount개의_빈팀을_반환한다()
        {
            List<List<string>> teams = TeamSplitScene.Split(null, 4, Rng(0));

            Assert.AreEqual(4, teams.Count, "null 입력에도 팀 수는 teamCount.");
            foreach (List<string> team in teams)
            {
                Assert.AreEqual(0, team.Count, "null 입력이면 모든 팀이 비어 있어야 한다.");
            }
        }

        //# 계약 위반 영역(미정의)은 최소만 — rng == null 이면 예외 없이 팀수 불변식 + 멤버 보존만 확인 (코드: 셔플 생략 원순서 배분).
        [Test]
        public void Split_rng가null이면_예외없이_팀수와_멤버를_보존한다()
        {
            List<string> names = MakeNames(7);

            List<List<string>> teams = TeamSplitScene.Split(names, 3, null);

            Assert.AreEqual(3, teams.Count, "rng null 이어도 팀 수는 teamCount.");

            List<string> flat = teams.SelectMany(team => team).ToList();
            Assert.AreEqual(names.Count, flat.Count, "rng null 이어도 멤버 누락/창작 없음.");

            List<string> sortedInput = names.OrderBy(s => s, System.StringComparer.Ordinal).ToList();
            List<string> sortedOutput = flat.OrderBy(s => s, System.StringComparer.Ordinal).ToList();
            CollectionAssert.AreEqual(sortedInput, sortedOutput, "rng null 멤버 multiset 보존.");
        }
    }
}
