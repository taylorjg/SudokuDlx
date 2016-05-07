using System;
using System.Threading;

namespace SudokuDlxWpf.Extensions
{
    public static class SynchronizationContextExtensions
    {
        public static void Post(this SynchronizationContext synchronizationContext, Action action)
        {
            synchronizationContext.Post(_ => action(), null);
        }

        public static void Post<T1>(this SynchronizationContext synchronizationContext, Action<T1> action, T1 state1)
        {
            synchronizationContext.Post(
                obj =>
                {
                    var s1 = (T1)obj;
                    action(s1);
                },
                state1);
        }

        public static void Post<T1, T2>(this SynchronizationContext synchronizationContext, Action<T1, T2> action, T1 state1, T2 state2)
        {
            synchronizationContext.Post(
                obj =>
                {
                    var tuple = (Tuple<T1, T2>)obj;
                    var s1 = tuple.Item1;
                    var s2 = tuple.Item2;
                    action(s1, s2);
                },
                Tuple.Create(state1, state2));
        }

        public static void Post<T1, T2, T3>(this SynchronizationContext synchronizationContext, Action<T1, T2, T3> action, T1 state1, T2 state2, T3 state3)
        {
            synchronizationContext.Post(
                obj =>
                {
                    var tuple = (Tuple<T1, T2, T3>)obj;
                    var s1 = tuple.Item1;
                    var s2 = tuple.Item2;
                    var s3 = tuple.Item3;
                    action(s1, s2, s3);
                },
                Tuple.Create(state1, state2, state3));
        }
    }
}
