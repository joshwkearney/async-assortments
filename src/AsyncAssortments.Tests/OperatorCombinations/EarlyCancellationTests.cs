using System.Runtime.CompilerServices;
using AsyncAssortments;
using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.OperatorCombinations {
    public class EarlyCancellationTests {
        [Fact]
        public async Task TestAsyncSelectCancellation() {
            var count = 0;

            var first = await new int[] { 1, 2, 3, 4 }
                .ToAsyncEnumerable()
                .AsConcurrent()
                .AsyncSelect(async (x, c) => {
                    if (x > 1) {
                        await Task.Delay(100, c);
                    }
                    
                    Interlocked.Increment(ref count);
                    return x;
                })
                .FirstAsync();

            await Task.Delay(200);
            
            Assert.Equal(1, count);
        }
        
        [Fact]
        public async Task TestConcatCancellation() {
            var count = 0;

            var first = await new int[] { 1 }
                .ToAsyncEnumerable()
                .Where(x => {
                    Interlocked.Increment(ref count);
                    return true;
                })
                .AsConcurrent()
                .Concat(SecondSequence())
                .FirstAsync();

            await Task.Delay(200);
            
            Assert.Equal(1, count);
            
            async IAsyncEnumerable<int> SecondSequence([EnumeratorCancellation] CancellationToken token = default) {
                await Task.Delay(100, token);
                
                Interlocked.Increment(ref count);
                yield return 2;
                
                Interlocked.Increment(ref count);
                yield return 3;
            }
        }
        
        [Fact]
        public async Task TestJoinCancellation() {
            var count = 0;

            var first = await new int[] { 1, 2, 3 }
                .ToAsyncEnumerable()
                .AsConcurrent()
                .Join(SecondSequence(), x => x, x => x)
                .FirstAsync();

            await Task.Delay(200);
            
            Assert.Equal(1, count);
            
            async IAsyncEnumerable<int> SecondSequence([EnumeratorCancellation] CancellationToken token = default) {
                Interlocked.Increment(ref count);
                yield return 1;
                
                await Task.Delay(100, token);
                
                Interlocked.Increment(ref count);
                yield return 2;
                
                Interlocked.Increment(ref count);
                yield return 3;
            }
        }
    }
}