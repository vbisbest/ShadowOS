using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace core.ui.data
{
    public class GarbageCollector : IMessageSink<Garbage>, IDisposable
    {
        public GarbageCollector(FrameworkElement fe)
        {
            if (fe != null)
            {
                var mediators = Mediator.GetResources(fe);
                MediatorInfo mi = new MediatorInfo() { ReferenceName = string.Empty };
                mi.Messages.Add(new TypeWrapper() { TypeName = "Garbage" });
                mediators.Add(mi);

                fe.Subscribe(this);
            }
        }

        public void HandleMessage(Garbage message)
        {
            _Garbage.Push(message);
            message.Handled = true;
        }

        public bool CanHandleMessage(Garbage message)
        {
            return true;
        }

        private Stack<Garbage> _Garbage = new Stack<Garbage>();

        public void Dispose()
        {
            while (_Garbage.Count > 0)
            {
                Garbage message = _Garbage.Pop();

                if (message != null)
                {
                    message.Dispose();
                }
            }
        }
    }

    [DebuggerDisplay("{Id}:{Message}")]
    public class Garbage : Message, IDisposable
    {
        public Garbage(IDisposable disposeable, string message) : this (() => { disposeable.Dispose(); }, message)
        {
        }

        public Garbage(Action dispose, string message)
        {
            _Dispose = dispose;
            Message = message;
        }

        public string Message
        {
            get;
            set;
        }

        private Action _Dispose = null;

        public void Dispose()
        {
            if (_Dispose != null)
            {
                _Dispose();
                _Dispose = null;
            }
        }
    }
}
