using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace TaskCollections
{
    public class TaskQueue
    {
        private Queue<Task> _queue;

        public TaskQueue()
        {
            _queue = new Queue<Task>();
        }

        public void AddTask(Task task)
        {
            _queue.Enqueue(task);
        }

        public void RemoveLast()
        {
            _queue = new Queue<Task>(_queue.Take(_queue.Count - 1));
        }

        public Task StartQueue()
        {
            Queue<Task> q = new Queue<Task>(_queue);

            Task prev = q.Dequeue();

            for (int _ = 1; _ < q.Count; ++_)
            {
                prev = prev.ContinueWith(async (t) => 
                {
                    var d = q.Dequeue();
                    d.Start();
                    await d;
                });
            }

            prev.Start();
            return prev;
        }
    }
}
