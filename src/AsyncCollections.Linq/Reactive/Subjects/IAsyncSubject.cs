//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace AsyncCollections.Reactive {
//    public interface IAsyncSubject<T> : IAsyncEnumerable<T>,  IDisposable {
//        public void AsyncNext(ValueTask<T> item);        
//    }

//    public static class SubjectExtensions {
//        public static void Next<T>(this IAsyncSubject<T> subject, T item) => subject.AsyncNext(ValueTask.FromResult(item));

//        public static void AsyncNext<T>(this IAsyncSubject<T> subject, Func<ValueTask<T>> item) => subject.AsyncNext(item());

//        public static void AsyncNext<T>(this IAsyncSubject<T> subject, Task<T> item) => subject.AsyncNext(new ValueTask<T>(item));

//        public static void NextRange<T>(this IAsyncSubject<T> subject, IEnumerable<T> items) {
//            foreach (var item in items) {
//                subject.Next(item);
//            }
//        }

//        public static void AsyncNextRange<T>(this IAsyncSubject<T> subject, IEnumerable<T> items) {
//            foreach (var item in items) {
//                subject.Next(item);
//            }
//        }
//    }
//}
