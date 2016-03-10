using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MigrationTest
{
    public static class TaskExtensions
    {
        public static async Task WaitForComplete(this IEnumerable<Task> tasks, bool parallel)
        {
            if (parallel)
            {
                await Task.WhenAll(tasks);
            }
            else
            {
                foreach (var task in tasks)
                    await task;
            }
        }
    }
}
