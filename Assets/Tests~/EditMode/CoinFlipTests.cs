using System.Collections.Generic;
using NUnit.Framework;

namespace Random.Tests
{
    //# coin_flip(동전 던지기) P0 — 연출과 분리된 순수 결과 산출 로직 테스트.
    //# 대상: CoinFlipScene.Flip / FlipMany / CountFaces (모두 static, GameObject 불필요).
    //# 결정적 단위(CountFaces)와 확률적 단위(Flip/FlipMany)를 구분 — 확률 테스트는 불변식만 단언(flaky 방지).
    public class CoinFlipTests
    {
        //# === Flip — 단일 결과 (확률적, 범위 불변식만 단언) ===

        [Test]
        public void Flip_호출시_앞면또는뒷면을_반환한다()
        {
            //# 여러 번 호출해도 항상 Head|Tail 범위를 벗어나지 않는다 (enum 값 2개뿐).
            for (int i = 0; i < 200; i++)
            {
                ECoinFace face = CoinFlipScene.Flip();
                Assert.IsTrue(face == ECoinFace.Head || face == ECoinFace.Tail,
                    "Flip 결과는 Head 또는 Tail 이어야 한다. 실제: " + face);
            }
        }

        [Test]
        public void Flip_분포_sanity_양면이_모두_출현한다()
        {
            //# 대량 호출 시 양면 모두 1회 이상 출현하면 통과. 정확한 50/50 비율은 flaky 하므로 단언하지 않는다.
            bool headSeen = false;
            bool tailSeen = false;

            for (int i = 0; i < 1000; i++)
            {
                ECoinFace face = CoinFlipScene.Flip();
                if (face == ECoinFace.Head)
                    headSeen = true;
                else
                    tailSeen = true;

                if (headSeen && tailSeen)
                    break;
            }

            Assert.IsTrue(headSeen, "1000회 던지기에서 앞면(Head)이 한 번도 나오지 않았다.");
            Assert.IsTrue(tailSeen, "1000회 던지기에서 뒷면(Tail)이 한 번도 나오지 않았다.");
        }

        //# === FlipMany — 연속 N회 (개수/합 불변식) ===

        [Test]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(10)]
        public void FlipMany_N회_요청시_정확히_N개_반환(int count)
        {
            List<ECoinFace> results = CoinFlipScene.FlipMany(count);

            Assert.IsNotNull(results, "FlipMany 는 null 을 반환하지 않는다.");
            Assert.AreEqual(count, results.Count, count + "회 요청 시 정확히 " + count + "개를 반환해야 한다.");
        }

        [Test]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(10)]
        public void FlipMany_앞뒤_카운트합이_N과_같다(int count)
        {
            //# CountFaces 연계 — 모든 결과는 앞 또는 뒤 둘 중 하나이므로 head+tail == count 가 항상 성립.
            List<ECoinFace> results = CoinFlipScene.FlipMany(count);

            (int head, int tail) = CoinFlipScene.CountFaces(results);

            Assert.AreEqual(count, head + tail, "앞면 수 + 뒷면 수 는 전체 던지기 수(" + count + ")와 같아야 한다.");
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-100)]
        public void FlipMany_0이하면_빈리스트(int count)
        {
            List<ECoinFace> results = CoinFlipScene.FlipMany(count);

            Assert.IsNotNull(results, "0 이하 입력에도 null 이 아닌 빈 리스트를 반환해야 한다.");
            Assert.AreEqual(0, results.Count, count + " 입력 시 빈 리스트(Count 0)여야 한다.");
        }

        //# === CountFaces — 결정적 단위 (난수 무관, 회귀 박제) ===

        [Test]
        public void CountFaces_null이면_0_0반환()
        {
            (int head, int tail) = CoinFlipScene.CountFaces(null);

            Assert.AreEqual(0, head, "null 입력 시 head 는 0 이어야 한다.");
            Assert.AreEqual(0, tail, "null 입력 시 tail 은 0 이어야 한다.");
        }

        [Test]
        public void CountFaces_빈리스트면_0_0반환()
        {
            List<ECoinFace> empty = new List<ECoinFace>();

            (int head, int tail) = CoinFlipScene.CountFaces(empty);

            Assert.AreEqual(0, head, "빈 리스트의 head 는 0.");
            Assert.AreEqual(0, tail, "빈 리스트의 tail 은 0.");
        }

        [Test]
        public void CountFaces_혼합리스트_정확히_센다()
        {
            //# 직접 구성한 결정적 리스트 — 난수 무관. 앞 3 / 뒤 2.
            List<ECoinFace> results = new List<ECoinFace>
            {
                ECoinFace.Head,
                ECoinFace.Tail,
                ECoinFace.Head,
                ECoinFace.Tail,
                ECoinFace.Head,
            };

            (int head, int tail) = CoinFlipScene.CountFaces(results);

            Assert.AreEqual(3, head, "앞면(Head) 은 정확히 3개여야 한다.");
            Assert.AreEqual(2, tail, "뒷면(Tail) 은 정확히 2개여야 한다.");
        }

        [Test]
        public void CountFaces_모두앞면이면_tail은_0()
        {
            //# 한쪽 면만 있는 경계 — head 만 세고 tail 은 0.
            List<ECoinFace> results = new List<ECoinFace>
            {
                ECoinFace.Head,
                ECoinFace.Head,
                ECoinFace.Head,
                ECoinFace.Head,
            };

            (int head, int tail) = CoinFlipScene.CountFaces(results);

            Assert.AreEqual(4, head, "전부 앞면이면 head 는 4.");
            Assert.AreEqual(0, tail, "전부 앞면이면 tail 은 0.");
        }

        [Test]
        public void CountFaces_모두뒷면이면_head는_0()
        {
            List<ECoinFace> results = new List<ECoinFace>
            {
                ECoinFace.Tail,
                ECoinFace.Tail,
                ECoinFace.Tail,
            };

            (int head, int tail) = CoinFlipScene.CountFaces(results);

            Assert.AreEqual(0, head, "전부 뒷면이면 head 는 0.");
            Assert.AreEqual(3, tail, "전부 뒷면이면 tail 은 3.");
        }
    }
}
