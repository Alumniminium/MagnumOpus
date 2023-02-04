using System.Collections.Concurrent;

namespace MagnumOpus.ECS
{
    public class PixelTaskScheduler : TaskScheduler
    {
        private readonly int _threadCount;
        private readonly BlockingCollection<Task> _tasks = new BlockingCollection<Task>();

        public PixelTaskScheduler(int threadCount)
        {
            _threadCount = threadCount;

            for (int i = 0; i < _threadCount; i++)
            {
                var thread = new Thread(() =>
                {
                    foreach (var task in _tasks.GetConsumingEnumerable())
                    {
                        TryExecuteTask(task);
                    }
                });

                thread.IsBackground = true;
                thread.Start();
            }
        }

        protected override void QueueTask(Task task) => _tasks.Add(task);

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;

        protected override IEnumerable<Task> GetScheduledTasks() => _tasks.ToArray();
    }
}