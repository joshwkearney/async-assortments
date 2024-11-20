using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncLinq.Tests.OperatorCombinations {
    public class SkipTakeTests {
        [Fact]
        public async Task TestSkipTake1() {
            var list = TestHelper.CreateRandomList(50);

            var expected = list
                .Skip(16)
                .Take(25)
                .Skip(3)
                .Take(10)
                .ToArray();

            var test = await new TestEnumerable<int>(list)
                .Skip(16)
                .Take(25)
                .Skip(3)
                .Take(10)
                .ToListAsync();

            Assert.Equal(expected, test);
            Assert.True(expected.Length > 0);
        }

        [Fact]
        public async Task TestSkipTake2() {
            var list = TestHelper.CreateRandomList(50);

            var expected = list
                .Skip(16)
                .Take(25)
                .Skip(3)
                .Take(100)
                .ToArray();

            var test = await new TestEnumerable<int>(list)
                .Skip(16)
                .Take(25)
                .Skip(3)
                .Take(100)
                .ToListAsync();

            Assert.Equal(expected, test);
            Assert.True(expected.Length > 0);
        }

        [Fact]
        public async Task TestSkipTake3() {
            var list = new[] { 1, 2, 3, 4, 5 };

            var expected = list
                .Skip(1)
                .Take(3)
                .Skip(1)
                .Take(100)
                .ToArray();

            var test = await new TestEnumerable<int>(list)
                .Skip(1)
                .Take(3)
                .Skip(1)
                .Take(100)
                .ToListAsync();

            Assert.Equal(expected, test);
            Assert.True(expected.Length > 0);
        }
    }
}
