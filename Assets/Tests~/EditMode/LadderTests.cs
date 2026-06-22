using System.Collections.Generic;
using NUnit.Framework;

namespace Random.Tests
{
    //# 사다리타기(ladder) — 연출과 분리된 순수 순열 산출 로직 테스트 (기획서 §3·§8).
    //# 대상: LadderScene.LevelCount / BuildRungs / BuildPath / Trace (모두 static, GameObject 불필요).
    //# 핵심 불변식: 전단사(순열) · 인접충돌 0 · |Δcol|≤1 · 경로길이 R+1 · BuildPath↔Trace 일관 · 결정성.
    //# rng 는 항상 new System.Random(seed) 로 시드 고정 — 사다리는 시드 하 hard invariant 라 정확히 단언(flaky 아님).
    public class LadderTests
    {
        //# 본 시스템 입력 상한/하한 (LadderScene MinCount=2, MaxCount=8).
        private const int MinN = 2;
        private const int MaxN = 8;

        //# === LevelCount — 결정적 단위 (기획서 §3.1 R = N*3) ===

        [Test]
        [TestCase(2, 6)]
        [TestCase(3, 9)]
        [TestCase(4, 12)]
        [TestCase(8, 24)]
        public void LevelCount_N에_대해_3배를_반환한다(int n, int expected)
        {
            Assert.AreEqual(expected, LadderScene.LevelCount(n),
                "레벨 수는 N*3 이어야 한다 (기획서 §3.1).");
        }

        //# === BuildRungs 차원 — 결정적 단위 ===

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(8)]
        public void BuildRungs_배열차원이_레벨수x컬럼수_N에서1뺀값이다(int n)
        {
            bool[,] rungs = LadderScene.BuildRungs(n, new System.Random(0));

            Assert.AreEqual(LadderScene.LevelCount(n), rungs.GetLength(0),
                "row(레벨) 수는 LevelCount(n) 이어야 한다.");
            Assert.AreEqual(n - 1, rungs.GetLength(1),
                "column 수는 n-1(인접쌍 수) 이어야 한다.");
        }

        //# === 불변식 2: 같은 레벨 인접 가로줄 없음 (기획서 §8 같은높이 인접충돌 금지) ===
        //# 한 컬럼 c 가 좌(rungs[h,c-1])·우(rungs[h,c]) 동시에 rung 을 가지면 충돌.
        //# BuildRungs 의 skip-by-2 가 구조적으로 차단해야 한다.

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void BuildRungs_한레벨에_인접충돌이_없다(int n)
        {
            for (int seed = 0; seed < 100; seed++)
            {
                bool[,] rungs = LadderScene.BuildRungs(n, new System.Random(seed));
                int levelCount = rungs.GetLength(0);

                for (int h = 0; h < levelCount; h++)
                {
                    for (int c = 0; c < n - 1; c++)
                    {
                        if (rungs[h, c] == false)
                            continue;

                        //# c 에 rung 이 있으면 우측(c+1) 에 동시 rung 이 있으면 안 됨 (컬럼 c+1 의 좌/우 충돌).
                        if (c + 1 < n - 1)
                        {
                            Assert.IsFalse(rungs[h, c + 1],
                                "인접 충돌: seed=" + seed + " n=" + n + " level=" + h + " column=" + c + " 와 " + (c + 1) + " 가 동시 rung.");
                        }
                    }
                }
            }
        }

        //# === 불변식 1: 전단사(순열) — N명 시작 0..N-1 의 Trace 결과가 {0..N-1} 순열 ===
        //# rungs 는 (n, seed) 당 한 번만 생성하고 모든 start 를 같은 사다리에 흘린다 (advisor 지적).

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void Trace_모든시작컬럼이_도착컬럼의_순열을_이룬다(int n)
        {
            for (int seed = 0; seed < 100; seed++)
            {
                bool[,] rungs = LadderScene.BuildRungs(n, new System.Random(seed));

                bool[] seen = new bool[n];
                for (int start = 0; start < n; start++)
                {
                    int end = LadderScene.Trace(start, rungs, n);

                    Assert.IsTrue(end >= 0 && end < n,
                        "도착 컬럼이 범위를 벗어남: seed=" + seed + " n=" + n + " start=" + start + " end=" + end);
                    Assert.IsFalse(seen[end],
                        "중복 도착(전단사 위반): seed=" + seed + " n=" + n + " start=" + start + " 가 이미 점유된 end=" + end + " 로 매핑.");

                    seen[end] = true;
                }

                //# 누락 0 — n 개 도착이 모두 점유되면 {0..n-1} 순열 성립.
                for (int col = 0; col < n; col++)
                {
                    Assert.IsTrue(seen[col],
                        "누락 도착(전단사 위반): seed=" + seed + " n=" + n + " 결과 컬럼 " + col + " 로 도착하는 시작이 없음.");
                }
            }
        }

        //# === 불변식 3: |Δcolumn| ≤ 1 — BuildPath 인접 노드 간 컬럼 변화 ≤ 1 ===

        [Test]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(6)]
        [TestCase(8)]
        public void BuildPath_인접노드_컬럼변화가_1이하다(int n)
        {
            for (int seed = 0; seed < 50; seed++)
            {
                bool[,] rungs = LadderScene.BuildRungs(n, new System.Random(seed));

                for (int start = 0; start < n; start++)
                {
                    List<(int level, int column)> path = LadderScene.BuildPath(start, rungs, n);

                    for (int i = 1; i < path.Count; i++)
                    {
                        int delta = path[i].column - path[i - 1].column;
                        int abs = delta < 0 ? -delta : delta;

                        Assert.LessOrEqual(abs, 1,
                            "컬럼 점프 위반: seed=" + seed + " n=" + n + " start=" + start + " node=" + i + " Δ=" + delta);
                    }
                }
            }
        }

        //# === 불변식 4: 경로 길이 == LevelCount(n)+1 (BuildRungs 산출 사다리 한정) ===

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(8)]
        public void BuildPath_길이가_레벨수_더하기1이다(int n)
        {
            bool[,] rungs = LadderScene.BuildRungs(n, new System.Random(7));
            int expectedLen = LadderScene.LevelCount(n) + 1;

            for (int start = 0; start < n; start++)
            {
                List<(int level, int column)> path = LadderScene.BuildPath(start, rungs, n);

                Assert.AreEqual(expectedLen, path.Count,
                    "경로 길이는 LevelCount(n)+1 이어야 한다. n=" + n + " start=" + start);
            }
        }

        [Test]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        public void BuildPath_첫노드는_시작컬럼_레벨0이다(int n)
        {
            bool[,] rungs = LadderScene.BuildRungs(n, new System.Random(3));

            for (int start = 0; start < n; start++)
            {
                List<(int level, int column)> path = LadderScene.BuildPath(start, rungs, n);

                Assert.AreEqual(0, path[0].level, "첫 노드의 레벨은 0.");
                Assert.AreEqual(start, path[0].column, "첫 노드의 컬럼은 시작 컬럼.");
            }
        }

        //# === 불변식 5: BuildPath 마지막 노드 컬럼 == Trace (두 함수 일관성) ===

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(8)]
        public void Trace_와_BuildPath마지막노드가_일치한다(int n)
        {
            for (int seed = 0; seed < 50; seed++)
            {
                bool[,] rungs = LadderScene.BuildRungs(n, new System.Random(seed));

                for (int start = 0; start < n; start++)
                {
                    List<(int level, int column)> path = LadderScene.BuildPath(start, rungs, n);
                    int pathEnd = path[path.Count - 1].column;
                    int traceEnd = LadderScene.Trace(start, rungs, n);

                    Assert.AreEqual(pathEnd, traceEnd,
                        "Trace 와 BuildPath 마지막 컬럼이 달라짐: seed=" + seed + " n=" + n + " start=" + start);
                }
            }
        }

        //# === 불변식 6: 결정성 — 같은 시드 → 같은 BuildRungs 결과 ===

        [Test]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(6)]
        [TestCase(8)]
        public void BuildRungs_같은시드면_동일한_사다리를_만든다(int n)
        {
            //# 동일 시드의 독립 System.Random 인스턴스 2개를 element-wise 비교.
            bool[,] a = LadderScene.BuildRungs(n, new System.Random(12345));
            bool[,] b = LadderScene.BuildRungs(n, new System.Random(12345));

            Assert.AreEqual(a.GetLength(0), b.GetLength(0), "레벨 수가 같아야 한다.");
            Assert.AreEqual(a.GetLength(1), b.GetLength(1), "컬럼 수가 같아야 한다.");

            for (int h = 0; h < a.GetLength(0); h++)
            {
                for (int c = 0; c < a.GetLength(1); c++)
                {
                    Assert.AreEqual(a[h, c], b[h, c],
                        "같은 시드인데 rung 배치가 다름: n=" + n + " level=" + h + " column=" + c);
                }
            }
        }

        [Test]
        public void BuildRungs_다른시드면_대체로_다른_사다리를_만든다()
        {
            //# 결정성의 대우 sanity — 여러 시드쌍 중 최소 하나는 달라야 한다 (난수가 시드를 실제 반영).
            //# 정확한 차이율은 단언하지 않는다(밀도 의존). "전부 동일"이 아님만 확인.
            bool anyDifference = false;
            bool[,] baseline = LadderScene.BuildRungs(6, new System.Random(0));

            for (int seed = 1; seed <= 20 && anyDifference == false; seed++)
            {
                bool[,] other = LadderScene.BuildRungs(6, new System.Random(seed));

                for (int h = 0; h < baseline.GetLength(0) && anyDifference == false; h++)
                {
                    for (int c = 0; c < baseline.GetLength(1); c++)
                    {
                        if (baseline[h, c] != other[h, c])
                        {
                            anyDifference = true;
                            break;
                        }
                    }
                }
            }

            Assert.IsTrue(anyDifference,
                "20개 시드 모두 seed=0 과 동일한 사다리를 만들었다 — 난수가 시드를 반영하지 못함.");
        }

        //# === 엣지: 가로줄 0개 → 항등 매핑 (start == end) ===
        //# 손수 만든 all-false 배열은 차원을 [LevelCount(n), n-1] 로 맞춰야 BuildPath 인덱싱과 일치 (advisor 지적).

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(8)]
        public void Trace_가로줄없으면_시작이_그대로_도착이다(int n)
        {
            bool[,] noRungs = new bool[LadderScene.LevelCount(n), n - 1];

            for (int start = 0; start < n; start++)
            {
                Assert.AreEqual(start, LadderScene.Trace(start, noRungs, n),
                    "가로줄 0개면 항등 매핑이어야 한다. n=" + n + " start=" + start);
            }
        }

        //# === 엣지: 손수 만든 단일 swap — 결정적 동작 박제 ===
        //# 레벨 0 컬럼 0 에만 rung → 0↔1 교환, 나머지는 항등. (n=3, 컬럼 차원=2)

        [Test]
        public void Trace_단일가로줄이면_해당쌍만_교환한다()
        {
            int n = 3;
            bool[,] rungs = new bool[LadderScene.LevelCount(n), n - 1];
            rungs[0, 0] = true;

            Assert.AreEqual(1, LadderScene.Trace(0, rungs, n), "0 은 1 로 교환되어야 한다.");
            Assert.AreEqual(0, LadderScene.Trace(1, rungs, n), "1 은 0 으로 교환되어야 한다.");
            Assert.AreEqual(2, LadderScene.Trace(2, rungs, n), "2 는 rung 영향 없이 그대로.");
        }

        //# === 엣지: N=2 최소 — 단일 컬럼쌍의 전단사 ===

        [Test]
        public void Trace_N2_최소에서도_전단사다()
        {
            int n = MinN;

            for (int seed = 0; seed < 50; seed++)
            {
                bool[,] rungs = LadderScene.BuildRungs(n, new System.Random(seed));

                int end0 = LadderScene.Trace(0, rungs, n);
                int end1 = LadderScene.Trace(1, rungs, n);

                Assert.AreNotEqual(end0, end1,
                    "N=2 두 시작이 같은 도착으로 가면 안 됨: seed=" + seed + " end0=" + end0 + " end1=" + end1);
                Assert.IsTrue((end0 == 0 && end1 == 1) || (end0 == 1 && end1 == 0),
                    "N=2 결과는 항등 또는 교환 둘 중 하나여야 한다: seed=" + seed + " (" + end0 + "," + end1 + ")");
            }
        }

        //# === 엣지: N=8 상한 — MaxCount 경계에서 전단사 유지 ===

        [Test]
        public void Trace_N8_상한에서도_전단사다()
        {
            int n = MaxN;

            for (int seed = 0; seed < 100; seed++)
            {
                bool[,] rungs = LadderScene.BuildRungs(n, new System.Random(seed));

                bool[] seen = new bool[n];
                for (int start = 0; start < n; start++)
                {
                    int end = LadderScene.Trace(start, rungs, n);
                    Assert.IsFalse(seen[end],
                        "N=8 중복 도착: seed=" + seed + " start=" + start + " end=" + end);
                    seen[end] = true;
                }

                for (int col = 0; col < n; col++)
                {
                    Assert.IsTrue(seen[col],
                        "N=8 누락 도착: seed=" + seed + " 결과 컬럼 " + col);
                }
            }
        }

        //# === 방어: rng == null 이면 빈(all-false) 사다리 → 항등 ===

        [Test]
        public void BuildRungs_rng가null이면_가로줄없는_사다리다()
        {
            int n = 4;
            bool[,] rungs = LadderScene.BuildRungs(n, null);

            Assert.AreEqual(LadderScene.LevelCount(n), rungs.GetLength(0), "차원은 유지되어야 한다.");

            for (int h = 0; h < rungs.GetLength(0); h++)
            {
                for (int c = 0; c < rungs.GetLength(1); c++)
                {
                    Assert.IsFalse(rungs[h, c], "rng null 이면 모든 rung 이 false 여야 한다.");
                }
            }

            for (int start = 0; start < n; start++)
            {
                Assert.AreEqual(start, LadderScene.Trace(start, rungs, n),
                    "rng null → 가로줄 없음 → 항등 매핑.");
            }
        }
    }
}
