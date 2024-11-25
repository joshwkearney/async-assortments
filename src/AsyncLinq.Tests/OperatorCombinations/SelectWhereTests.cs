using AsyncCollections.Linq;

namespace AsyncCollections.Linq.Tests.OperatorCombinations {
    public class SelectWhereTests {
        [Fact]
        public async Task TestSequential() {
            var list = TestHelper.CreateRandomList(100);
            var expected = list.Where(x => x > 35).Select(x => x / 2 - 1).ToArray();

            var test = await new TestEnumerable<int>(list)
                .Where(x => x > 35)
                .Select(x => x / 2 - 1)
                .ToListAsync();

            Assert.Equal(expected, test);
        }

        [Fact]
        public async Task TestAsync1() {
            var list = TestHelper.CreateRandomList(100);
            var expected = list.Where(x => x > 35).Select(x => x / 2 - 1).ToArray();

            var test = await new TestEnumerable<int>(list)
                .AsyncWhere(Filter)
                .Select(x => x / 2 - 1)
                .ToListAsync();

            Assert.Equal(expected, test);

            async ValueTask<bool> Filter(int x) {
                await Task.Delay(10);
                return x > 35;
            }
        }

        [Fact]
        public async Task TestAsync2() {
            var list = TestHelper.CreateRandomList(100);
            var expected = list.Where(x => x > 35).Select(x => x / 2 - 1).ToArray();

            var test = await new TestEnumerable<int>(list)
                .Where(x => x > 35)
                .AsyncSelect(Selector)
                .ToListAsync();

            Assert.Equal(expected, test);

            async ValueTask<int> Selector(int x) {
                await Task.Delay(10);
                return x / 2 - 1;
            }
        }

        [Fact]
        public async Task TestAsync3() {
            var list = TestHelper.CreateRandomList(100);
            var expected = list.Where(x => x > 35).Select(x => x / 2 - 1).ToArray();

            var test = await new TestEnumerable<int>(list)
                .AsyncWhere(Filter)
                .AsyncSelect(Selector)
                .ToListAsync();

            Assert.Equal(expected, test);

            async ValueTask<bool> Filter(int x) {
                await Task.Delay(10);
                return x > 35;
            }

            async ValueTask<int> Selector(int x) {
                await Task.Delay(10);
                return x / 2 - 1;
            }
        }

        [Fact]
        public async Task TestAsync4() {
            var list = TestHelper.CreateRandomList(100);
            var expected = list.Select(x => x / 2 - 1).Where(x => x > 35).ToArray();

            var test = await new TestEnumerable<int>(list)
                .AsyncSelect(Selector)
                .AsyncWhere(Filter)
                .ToListAsync();

            Assert.Equal(expected, test);

            async ValueTask<bool> Filter(int x) {
                await Task.Delay(10);
                return x > 35;
            }

            async ValueTask<int> Selector(int x) {
                await Task.Delay(10);
                return x / 2 - 1;
            }
        }
    }
}
