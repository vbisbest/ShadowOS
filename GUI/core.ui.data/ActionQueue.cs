using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace core.ui.data
{
    public class ActionQueue
    {
        public ActionQueue()
        {
        }

        public long Add(Action a, long index)
        {
            long rc = 0;

            lock (_Queue)
            {
                rc = (index == 0) ? ++_QueueId : index;

                _Queue.Add(new Item() { Action = a, Index = rc });
            }

            return rc;
        }
        long _QueueId = 0;

        public void Process()
        {
            lock (this)
            {
                List<Item> queue = new List<Item>();

                lock (_Queue)
                {
                    queue.AddRange(_Queue);
                    _Queue.Clear();
                }

                queue = queue.OrderBy(i => i.Index).ToList();

                foreach (Item i in queue)
                {
                    i.Action();
                }
            }
        }

        private List<Item> _Queue = new List<Item>();

        private class Item
        {
            public Action Action
            {
                get;
                set;
            }

            public long Index
            {
                get;
                set;
            }
        }
    }
}
