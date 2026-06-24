using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Random.Tests
{
    //# order_shuffle — OrderShuffleScene.Shuffle(IList<string>, System.Random) 본격 테스트 스위트.
    //# 대상: static, MonoBehaviour 불필요. 기획서 §8 Fisher-Yates, 원본 비파괴.
    //# 불변식: 반환 길이 == names.Count · 멤버 multiset 보존 · 결정성(동일 seed → 동일 순열) · rng==null → 원순서 반환.
    //# 결정성은 "동일 seed → 동일 결과"(상대 비교)로만 단언 — seed→배열 하드코딩 금지(Mono/CoreCLR 난수열 상이).
    public class OrderShuffleTests
    {
        //# 입력 범위 (기획서 §2 MinCount=2, MaxCount=12).
        private const int MinCount = 2;
        private const int MaxCount = 12;

        //# === 헬퍼 ===

        //# 0..n-1 의 distinct 이름 리스트("p0".."p{n-1}"). 구조·스윕 테스트용.
        private static List<string> MakeNames(int n)
        {
            List<string> names = new List<string>();
            for (int i = 0; i < n; i++)
            {
                names.Add("p" + i);
            }
            return names;
        }

        //# 시드 고정 rng — 결정적 재현용. 네임스페이스 충돌 방지를 위해 System.Random 완전 한정.
        private static System.Random Rng(int seed)
        {
            return new System.Random(seed);
        }

        //# === §1 정상 동작 — 기본 불변식 ===

        //# 반환 길이가 입력 수와 정확히 같아야 한다. N=2..12 스윕.
        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(7)]
        [TestCase(10)]
        [TestCase(12)]
        public void Shuffle_반환_길이가_입력과_같다(int n)
        {
            List<string> names = MakeNames(n);

            for (int seed = 0; seed < 10; seed++)
            {
                List<string> result = OrderShuffleScene.Shuffle(names, Rng(seed));

                Assert.AreEqual(n, result.Count,
                    "반환 길이가 입력 수와 달라짐. n=" + n + " seed=" + seed);
            }
        }

        //# 반환 리스트의 멤버(multiset)가 입력과 정확히 일치해야 한다 — 누락/창작/중복 없음.
        [Test]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(7)]
        [TestCase(12)]
        public void Shuffle_전체멤버가_입력과_정확히_일치한다_multiset(int n)
        {
            List<string> names = MakeNames(n);
            List<string> sortedInput = names.OrderBy(s => s, System.StringComparer.Ordinal).ToList();

            for (int seed = 0; seed < 20; seed++)
            {
                List<string> result = OrderShuffleScene.Shuffle(names, Rng(seed));
                List<string> sortedResult = result.OrderBy(s => s, System.StringComparer.Ordinal).ToList();

                CollectionAssert.AreEqual(sortedInput, sortedResult,
                    "멤버 multiset 이 입력과 다름(누락/창작/중복). n=" + n + " seed=" + seed);
            }
        }

        //# 동명이인 개수가 그대로 보존되어야 한다 — "김"3 "이"2 "박"1 = 6명, 30개 seed.
        [Test]
        public void Shuffle_동명이인이_개수그대로_보존된다()
        {
            List<string> names = new List<string> { "김", "김", "김", "이", "이", "박" };

            for (int seed = 0; seed < 30; seed++)
            {
                List<string> result = OrderShuffleScene.Shuffle(names, Rng(seed));

                Assert.AreEqual(6, result.Count, "전체 인원 6명 보존. seed=" + seed);
                Assert.AreEqual(3, result.Count(s => s == "김"), "동명이인 '김' 3개 보존. seed=" + seed);
                Assert.AreEqual(2, result.Count(s => s == "이"), "동명이인 '이' 2개 보존. seed=" + seed);
                Assert.AreEqual(1, result.Count(s => s == "박"), "'박' 1개 보존. seed=" + seed);
            }
        }

        //# === §2 원본 비파괴 ===

        //# Shuffle 호출 후 입력 리스트의 순서/내용이 그대로여야 한다 (복사본 셔플).
        [Test]
        public void Shuffle_입력리스트를_변형하지_않는다()
        {
            List<string> names = MakeNames(8);
            List<string> snapshot = new List<string>(names);

            OrderShuffleScene.Shuffle(names, Rng(42));

            CollectionAssert.AreEqual(snapshot, names,
                "Shuffle 호출 후 입력 리스트의 순서/내용이 바뀌었다 (원본 비파괴 위반).");
        }

        //# === §3 경계값 ===

        //# names.Count==1 — 1명이면 그대로 반환 (길이 1, 동일 이름).
        //# production MinCount=2 이지만 Shuffle 자체는 1명을 금지하지 않음 — 계약 범위.
        [Test]
        public void Shuffle_1명이면_그대로_반환한다()
        {
            List<string> names = new List<string> { "Alice" };

            List<string> result = OrderShuffleScene.Shuffle(names, Rng(0));

            Assert.AreEqual(1, result.Count, "반환 길이는 1이어야 한다.");
            Assert.AreEqual("Alice", result[0], "유일한 이름이 그대로 반환되어야 한다.");
        }

        //# MinCount 경계 — 2명이면 반환 길이 2, 멤버 보존.
        [Test]
        public void Shuffle_2명이면_반환_길이_2_멤버_보존()
        {
            List<string> names = MakeNames(2);
            List<string> sortedInput = names.OrderBy(s => s, System.StringComparer.Ordinal).ToList();

            for (int seed = 0; seed < 10; seed++)
            {
                List<string> result = OrderShuffleScene.Shuffle(names, Rng(seed));

                Assert.AreEqual(2, result.Count, "2명 반환 길이는 2여야 한다. seed=" + seed);

                List<string> sortedResult = result.OrderBy(s => s, System.StringComparer.Ordinal).ToList();
                CollectionAssert.AreEqual(sortedInput, sortedResult,
                    "2명 멤버 보존 실패. seed=" + seed);
            }
        }

        //# MaxCount 경계 — 12명이면 반환 길이 12, 멤버 보존.
        [Test]
        public void Shuffle_12명이면_반환_길이_12_멤버_보존()
        {
            List<string> names = MakeNames(12);
            List<string> sortedInput = names.OrderBy(s => s, System.StringComparer.Ordinal).ToList();

            for (int seed = 0; seed < 10; seed++)
            {
                List<string> result = OrderShuffleScene.Shuffle(names, Rng(seed));

                Assert.AreEqual(12, result.Count, "12명 반환 길이는 12여야 한다. seed=" + seed);

                List<string> sortedResult = result.OrderBy(s => s, System.StringComparer.Ordinal).ToList();
                CollectionAssert.AreEqual(sortedInput, sortedResult,
                    "12명 멤버 보존 실패. seed=" + seed);
            }
        }

        //# === §4 null/빈 입력 ===

        //# null 입력이면 빈 리스트를 반환한다 (예외 없음).
        [Test]
        public void Shuffle_null입력이면_빈리스트_반환()
        {
            List<string> result = OrderShuffleScene.Shuffle(null, Rng(0));

            Assert.IsNotNull(result, "null 입력이라도 null 을 반환하면 안 된다.");
            Assert.AreEqual(0, result.Count, "null 입력이면 빈 리스트를 반환해야 한다.");
        }

        //# 빈 리스트면 빈 리스트를 반환한다 (예외 없음).
        [Test]
        public void Shuffle_빈리스트면_빈리스트_반환()
        {
            List<string> result = OrderShuffleScene.Shuffle(new List<string>(), Rng(0));

            Assert.IsNotNull(result, "빈 리스트 입력이라도 null 을 반환하면 안 된다.");
            Assert.AreEqual(0, result.Count, "빈 리스트 입력이면 빈 리스트를 반환해야 한다.");
        }

        //# === §5 rng == null ===

        //# rng 가 null 이면 예외 없이 원본과 동일 순서를 반환한다.
        [Test]
        public void Shuffle_rng가null이면_예외없이_원순서_반환()
        {
            List<string> names = MakeNames(7);

            List<string> result = OrderShuffleScene.Shuffle(names, null);

            Assert.IsNotNull(result, "rng==null 이어도 null 반환 안 됨.");
            Assert.AreEqual(names.Count, result.Count, "rng==null 이어도 반환 길이 == 입력 수.");

            //# rng==null 은 셔플 생략 → 원순서 그대로 반환 (계약: §2).
            CollectionAssert.AreEqual(names, result,
                "rng==null 이면 셔플 없이 원순서 그대로여야 한다.");
        }

        //# rng==null 멤버 multiset 보존 — 원순서 반환이므로 multiset 도 당연히 보존되나 명시.
        [Test]
        public void Shuffle_rng가null이면_멤버_multiset_보존()
        {
            List<string> names = new List<string> { "A", "B", "B", "C" };
            List<string> sortedInput = names.OrderBy(s => s, System.StringComparer.Ordinal).ToList();

            List<string> result = OrderShuffleScene.Shuffle(names, null);

            List<string> sortedResult = result.OrderBy(s => s, System.StringComparer.Ordinal).ToList();
            CollectionAssert.AreEqual(sortedInput, sortedResult,
                "rng==null 이어도 멤버 multiset 이 보존되어야 한다.");
        }

        //# === §6 결정성 ===

        //# 같은 seed 면 동일한 순서를 만든다 (상대 비교 — 절대값 하드코딩 금지).
        [Test]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(8)]
        [TestCase(12)]
        public void Shuffle_같은시드면_동일한_순서를_만든다(int n)
        {
            List<string> names = MakeNames(n);

            for (int seed = 0; seed < 10; seed++)
            {
                //# 두 개의 독립된 Rng 인스턴스로 — 하나를 재사용하면 내부 상태가 달라짐.
                List<string> a = OrderShuffleScene.Shuffle(names, Rng(seed));
                List<string> b = OrderShuffleScene.Shuffle(names, Rng(seed));

                CollectionAssert.AreEqual(a, b,
                    "같은 seed 인데 결과 순서가 다름. n=" + n + " seed=" + seed);
            }
        }

        //# === §7 무작위성 (약하게) ===

        //# 다른 seed 면 대체로 다른 순서를 만든다 — N=8, 20개 seed 중 최소 1개 다름 (sanity).
        [Test]
        public void Shuffle_다른시드면_대체로_다른_순서를_만든다()
        {
            List<string> names = MakeNames(8);
            List<string> baseline = OrderShuffleScene.Shuffle(names, Rng(0));
            string baseKey = string.Join(",", baseline);

            bool anyDifference = false;
            for (int seed = 1; seed <= 20 && anyDifference == false; seed++)
            {
                List<string> other = OrderShuffleScene.Shuffle(names, Rng(seed));
                if (string.Join(",", other) != baseKey)
                {
                    anyDifference = true;
                }
            }

            Assert.IsTrue(anyDifference,
                "20개 시드 모두 seed=0 과 동일한 순서를 만들었다 — 셔플이 시드를 반영하지 못함.");
        }

        //# === §8 Fisher-Yates 편향 없음 (통계) ===

        //# N=3 (6가지 순열), 6000번 반복, 각 순열 출현 횟수 ∈ [600, 1400].
        //# 실제 편향 셔플과 Fisher-Yates 의 차이를 잡는 용도. 기대값 1000 ± 40%.
        //# 6가지 순열을 명시적으로 정의 → count==0 (아예 나타나지 않는 순열)을 반드시 잡는다.
        [Test]
        public void Shuffle_모든_순열이_고르게_생성된다_N3()
        {
            List<string> names = new List<string> { "A", "B", "C" };

            //# 6가지 순열 모두 명시 — 관찰된 key 만 검사하면 count=0 순열이 누락됨.
            List<string> perm0 = new List<string> { "A", "B", "C" };
            List<string> perm1 = new List<string> { "A", "C", "B" };
            List<string> perm2 = new List<string> { "B", "A", "C" };
            List<string> perm3 = new List<string> { "B", "C", "A" };
            List<string> perm4 = new List<string> { "C", "A", "B" };
            List<string> perm5 = new List<string> { "C", "B", "A" };

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { PermKey(perm0), 0 },
                { PermKey(perm1), 0 },
                { PermKey(perm2), 0 },
                { PermKey(perm3), 0 },
                { PermKey(perm4), 0 },
                { PermKey(perm5), 0 },
            };

            //# 6000번 — 1개 System.Random 인스턴스로 순차 호출 (seed 연속성 상관 문제 없음, 교과서 방식).
            System.Random rng = new System.Random(777);
            for (int i = 0; i < 6000; i++)
            {
                List<string> result = OrderShuffleScene.Shuffle(names, rng);
                string key = PermKey(result);

                //# 정의된 6가지 순열에 없으면 테스트 자체 오류 (이름이 달라질 수 없음).
                if (counts.ContainsKey(key))
                {
                    counts[key]++;
                }
                else
                {
                    Assert.Fail("6000번 반복 중 예상치 못한 순열 등장: " + key);
                }
            }

            //# 각 순열 출현 횟수가 [600, 1400] 안에 있어야 한다 (기대값 1000, ±40%).
            foreach (System.Collections.Generic.KeyValuePair<string, int> kv in counts)
            {
                Assert.GreaterOrEqual(kv.Value, 600,
                    "순열 '" + kv.Key + "' 출현 횟수 " + kv.Value + " 가 하한 600 미만 — 편향 또는 미등장 의심.");
                Assert.LessOrEqual(kv.Value, 1400,
                    "순열 '" + kv.Key + "' 출현 횟수 " + kv.Value + " 가 상한 1400 초과 — 편향 의심.");
            }
        }

        //# PermKey: 리스트를 "A,B,C" 형태 단일 문자열로 직렬화 (순열 식별용).
        private static string PermKey(List<string> list)
        {
            return string.Join(",", list);
        }

        //# === §9 스윕 — N=2..12, 5개 seed, 반환 길이 + multiset 보존 불변식 ===

        [Test]
        public void Shuffle_전조합_스윕_길이와보존_불변식_유지()
        {
            for (int n = MinCount; n <= MaxCount; n++)
            {
                List<string> names = MakeNames(n);
                List<string> sortedInput = names.OrderBy(s => s, System.StringComparer.Ordinal).ToList();

                for (int seed = 0; seed < 5; seed++)
                {
                    List<string> result = OrderShuffleScene.Shuffle(names, Rng(seed));

                    //# 반환 길이.
                    Assert.AreEqual(n, result.Count,
                        "스윕 반환 길이 위반. n=" + n + " seed=" + seed);

                    //# multiset 보존.
                    List<string> sortedResult = result.OrderBy(s => s, System.StringComparer.Ordinal).ToList();
                    CollectionAssert.AreEqual(sortedInput, sortedResult,
                        "스윕 멤버 보존 위반. n=" + n + " seed=" + seed);
                }
            }
        }
    }
}
