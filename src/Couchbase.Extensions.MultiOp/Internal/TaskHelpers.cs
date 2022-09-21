using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Couchbase.Extensions.MultiOp.Internal
{
    internal static class TaskHelpers
    {
        public static IList<Task>
            RunFairForEach(this IEnumerable<object?> states, Func<object?, Task> taskFactory,
                CancellationToken cancellationToken = default)
        {
            var list = new List<Task>(states is IList stateList ? stateList.Count : 0);
            foreach (var state in states)
            {
                list.Add(RunFair(taskFactory, state, cancellationToken));
            }

            return list;
        }

        public static Task RunFair(Func<object?, Task> taskFactory, object? state, CancellationToken cancellationToken = default) =>
            Task<Task>.Factory.StartNew(taskFactory, state, cancellationToken,
                    TaskCreationOptions.PreferFairness | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default)
                .Unwrap();
    }
}
