using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace core.ui.data
{
    public class Mediator : IDisposable
    {
        #region Reference
        public static DependencyProperty ReferenceProperty = DependencyProperty.RegisterAttached("Reference", typeof(string), typeof(Mediator), new PropertyMetadata(string.Empty, (s, ea) =>
        {
            FrameworkElement fe = s as FrameworkElement;

            if (fe != null)
            {
                if (!fe.IsInDesignMode())
                {
                    ProcessQueues();
                }
            }
        }));

        public static string GetReference(DependencyObject d)
        {
            return (string)d.GetValue(ReferenceProperty);
        }

        public static void SetReference(DependencyObject d, string o)
        {
            d.SetValue(ReferenceProperty, o);
        }
        #endregion

        #region Resources
        public static DependencyProperty ResourcesProperty = DependencyProperty.RegisterAttached("ResourcesInternal", typeof(ObservableCollection<MediatorInfo>), typeof(Mediator), new PropertyMetadata(null));

        public static ObservableCollection<MediatorInfo> GetResources(DependencyObject d)
        {
            ObservableCollection<MediatorInfo> rc = null;

            if (d != null)
            {
                rc = d.GetValue(ResourcesProperty) as ObservableCollection<MediatorInfo>;

                if (rc == null)
                {
                    rc = new ObservableCollection<MediatorInfo>();
                    SetResources(d, rc);
                }
            }

            return rc;
        }

        public static void SetResources(DependencyObject d, ObservableCollection<MediatorInfo> mediators)
        {
            FrameworkElement fe = d as FrameworkElement;

            if (fe != null)
            {
                if (!fe.IsInDesignMode())
                {
                    if (mediators != null)
                    {
                        mediators.CollectionChanged += Resources_CollectionChanged;
                        d.SetValue(ResourcesProperty, mediators);

                        fe.Publish(new Garbage(() =>
                        {
                            mediators.CollectionChanged -= Resources_CollectionChanged;

                            foreach (MediatorInfo mi in mediators)
                            {
                                mi.Dispose();
                            }

                            mediators.Clear();
                        }, "Mediator.SetResources"));
                    }
                }
            }
        }

        static void Resources_CollectionChanged(object s, System.Collections.Specialized.NotifyCollectionChangedEventArgs ea)
        {
            ProcessQueues();
        }

        #endregion

        #region Register Global
        public static DependencyProperty RegisterGlobalProperty = DependencyProperty.RegisterAttached("RegisterGlobalInternal", typeof(ObservableCollection<TypeWrapper>), typeof(Mediator), new PropertyMetadata(null));

        public static ObservableCollection<TypeWrapper> GetRegisterGlobal(DependencyObject d)
        {
            ObservableCollection<TypeWrapper> rc = d.GetValue(RegisterGlobalProperty) as ObservableCollection<TypeWrapper>;

            if (rc == null)
            {
                rc = new ObservableCollection<TypeWrapper>();
                SetRegisterGlobal(d, rc);
            }

            return rc;
        }

        public static void SetRegisterGlobal(DependencyObject d, ObservableCollection<TypeWrapper> messages)
        {
            FrameworkElement fe = d as FrameworkElement;

            if (fe != null)
            {
                if (!fe.IsInDesignMode())
                {
                    messages.CollectionChanged += RegisterGlobal_CollectionChanged;

                    fe.Publish(new Garbage(() =>
                    {
                        messages.CollectionChanged -= RegisterGlobal_CollectionChanged;
                    }, "Mediator.SetRegisterGlobal"));
                }
            }
            d.SetValue(RegisterGlobalProperty, messages);
        }

        static void RegisterGlobal_CollectionChanged(object s, System.Collections.Specialized.NotifyCollectionChangedEventArgs ea)
        {
            foreach (TypeWrapper type in ea.NewItems)
            {
                _GlobalMediator.Register(type);
            }

            ProcessQueues();
        }

        private static Mediator _GlobalMediator = new Mediator();
        #endregion

        #region Publish
        public bool Publish<T>(T m) where T : Message
        {
            MessageInfo<T> mi = new MessageInfo<T>(m);

            return Retry<T>(mi);
        }

        private bool Retry<T>(MessageInfo<T> mi) where T : Message
        {
            return Retry(mi, 0);
        }

        private bool Retry<T>(MessageInfo<T> mi, long index) where T : Message
        {
            bool rc = false;
            if (mi != null)
            {
                List<object> sinks = null;
                _Sinks.TryGetValue(mi.Key, out sinks);

                if (sinks != null)
                {
                    rc = true;

                    foreach (IMessageSink<T> sink in sinks)
                    {
                        if (!mi.Recipients.Contains(sink))
                        {
                            if (sink.CanHandleMessage(mi.Message))
                            {
                                sink.HandleMessage(mi.Message);
                            }

                            mi.Recipients.Add(sink);

                            if (mi.Message.Handled)
                            {
                                mi.HandledCount++;

                                if (mi.Message.Multicast)
                                {
                                    mi.Message.Handled = false;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                if (mi.HandledCount == 0 || (mi.Message.Multicast && (mi.Message.MaxRetryAttempts == -1 || mi.RetryCount < mi.Message.MaxRetryAttempts)))
                {
                    ActionQueue mq = null;
                    _RetryQueue.TryGetValue(mi.Key, out mq);

                    if (mq == null)
                    {
                        mq = new ActionQueue();
                        _RetryQueue.Add(mi.Key, mq);
                    }

                    long q = index;

                    q = mq.Add(() =>
                    {
                        Retry<T>(mi, q);
                        mi.RetryCount++;
                    }, index);
                }
            }

            return rc;
        }

        private Dictionary<string, ActionQueue> _RetryQueue = new Dictionary<string, ActionQueue>();
        #endregion

        public void Register(TypeWrapper type)
        {
            List<object> sinks = null;
            _Sinks.TryGetValue(type.FullName, out sinks);

            if (sinks == null)
            {
                sinks = new List<object>();
                _Sinks.Add(type.FullName, sinks);
            }
        }

        private bool AddSink(string fullName, object sink)
        {
            bool rc = false;

            List<object> sinks = null;
            _Sinks.TryGetValue(fullName, out sinks);

            if (sinks != null)
            {
                if (sinks.AddDistinct(sink))
                {
                    ActionQueue mq = null;
                    _RetryQueue.TryGetValue(fullName, out mq);

                    if (mq != null)
                    {
                        mq.Process();
                    }
                }

                rc = true;
            }

            return rc;
        }

        private bool RemoveSink(string fullName, object sink)
        {
            bool rc = false;

            List<object> sinks = null;
            _Sinks.TryGetValue(fullName, out sinks);

            if (sinks != null)
            {
                sinks.Remove(sink);
                rc = true;
            }

            return rc;
        }

        public void Subscribe(object sink)
        {
            bool fail = true;

            foreach (Type t in sink.GetType().GetMessageSinks())
            {
                Type mt = t.GetMessageType();
                AddSink(mt.FullName, sink);

                fail = false;
            }

            if (fail)
            {
                throw new ArgumentException("Only IMessageSink<T> may subscribe to Mediator(s).", "sink");
            }
        }

        private Dictionary<string, List<object>> _Sinks = new Dictionary<string, List<object>>();

        #region Utility
        public static List<string> GetAllMessages(object sink)
        {
            List<string> rc = new List<string>();
            bool fail = true;

            foreach (Type t in sink.GetType().GetMessageSinks())
            {
                Type mt = t.GetMessageType();
                rc.Add(mt.FullName);

                fail = false;
            }

            if (fail)
            {
                throw new ArgumentException("Only IMessageSink<T> may subscribe to Mediator(s).", "sink");
            }

            return rc;
        }

        public static List<MediatorInfo> GetAllMediatorInfo(FrameworkElement fe)
        {
            List<MediatorInfo> rc = new List<MediatorInfo>();

            DependencyObject root = fe;

            while (root != null)
            {
                ObservableCollection<MediatorInfo> mediators = root.GetValue(Mediator.ResourcesProperty) as ObservableCollection<MediatorInfo>;

                if (mediators != null)
                {
                    foreach (MediatorInfo mi in mediators)
                    {
                        rc.Add(mi);
                    }
                }

                root = VisualTreeHelper.GetParent(root);
            }

            return rc;
        }

        public static List<string> GetAllReferences(FrameworkElement fe)
        {
            List<string> rc = new List<string>();

            DependencyObject root = fe;

            while (root != null)
            {
                string reference = Mediator.GetReference(root) as string;
                if (!string.IsNullOrEmpty(reference))
                {
                    rc.Add(reference);
                }

                root = VisualTreeHelper.GetParent(root);
            }

            return rc;
        }

        public static List<Mediator> GetAllMediators(FrameworkElement fe)
        {
            List<Mediator> rc = new List<Mediator>();
            List<MediatorInfo> mediators = GetAllMediatorInfo(fe);
            List<string> references = GetAllReferences(fe);


            foreach (MediatorInfo mi in mediators)
            {
                if (mi.ReferenceName == string.Empty)
                {
                    rc.Add(mi.Mediator);
                    continue;
                }

                foreach (string reference in references)
                {
                    if (mi.ReferenceName == reference)
                    {
                        rc.Add(mi.Mediator);
                    }
                }
            }

            rc.Add(_GlobalMediator);

            return rc;
        }

        private static void _Subscribe_Loaded(object sender, RoutedEventArgs e)
        {
            _SubscribeQueue.Process();
        }

        private static void Subscribe(FrameworkElement fe, object sink, string message)
        {
            Subscribe(fe, sink, message, 0);
        }

        private static void Subscribe(FrameworkElement fe, object sink, string message, long index)
        {
            if (fe != null)
            {
                if (!fe.IsInDesignMode())
                {
                    bool found = false;

                    foreach (Mediator mediator in GetAllMediators(fe))
                    {
                        if (mediator.AddSink(message, sink))
                        {
                            fe.Publish(new Garbage(() => { mediator.RemoveSink(message, sink); }, string.Format("{2}:Mediator.Subscribe({0}, {1})", sink.GetType().Name, message, fe.GetType().Name)));
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        fe.Loaded -= _Subscribe_Loaded;
                        fe.Loaded += _Subscribe_Loaded;

                        long q = index;

                        q = _SubscribeQueue.Add(() =>
                        {
                            fe.Loaded -= _Subscribe_Loaded;
                            Subscribe(fe, sink, message, q);
                        }, index);
                    }
                }
            }
        }

        public static void Subscribe(FrameworkElement fe, object sink)
        {
            if (fe != null)
            {
                if (!fe.IsInDesignMode())
                {
                    foreach (string message in GetAllMessages(sink))
                    {
                        Subscribe(fe, sink, message);
                    }
                }
            }
        }

        private static ActionQueue _SubscribeQueue = new ActionQueue();

        private static void _Publish_Loaded(object sender, RoutedEventArgs e)
        {
            _PublishQueue.Process();
        }

        public static void Publish<T>(FrameworkElement view, T message) where T : Message
        {
            Publish(view, message, 0);
        }

        public static void Publish<T>(FrameworkElement view, T message, long index) where T : Message
        {
            if (view != null)
            {
                if (!view.IsInDesignMode())
                {
                    if (view.IsVisible())
                    {
                        bool found = false;

                        foreach (Mediator mediator in GetAllMediators(view))
                        {
                            if (mediator.Publish<T>(message))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            view.Loaded -= _Publish_Loaded;
                            view.Loaded += _Publish_Loaded;

                            long q = index;

                            q = _PublishQueue.Add(() => {
                                view.Loaded -= _Publish_Loaded; 
                                Publish<T>(view, message, q); 
                            }, index);
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentNullException("view");
            }
        }

        private static ActionQueue _PublishQueue = new ActionQueue();

        internal static void ProcessQueues()
        {
            _SubscribeQueue.Process();
            _PublishQueue.Process();
        }
        #endregion

        public void Dispose()
        {
            if (_Sinks != null)
            {
                foreach (var bucket in _Sinks.Values)
                {
                    bucket.Clear();
                }

                _Sinks = null;
            }
        }
    }

    public interface IMessageSink<T> where T : Message
    {
        void HandleMessage(T message);
        bool CanHandleMessage(T message);
    }

    public class MessageSinkInfo
    {
        private MessageSinkInfo(Type messageSink, List<Type> interfaces)
        {
            _MessageSink = messageSink;
            _Interfaces = new ReadOnlyCollection<Type>(interfaces);
        }

        public Type MessageSink
        {
            get
            {
                return _MessageSink;
            }
        }
        private Type _MessageSink = null;

        public ReadOnlyCollection<Type> Interfaces
        {
            get
            {
                return _Interfaces;
            }
        }
        private ReadOnlyCollection<Type> _Interfaces = null;

        #region Extensions
        public static MessageSinkInfo GetMessageSinkInfo(Type t)
        {
            MessageSinkInfo msi = null;
            _MessageSinkInfo.TryGetValue(t.FullName, out msi);

            if (msi == null)
            {
                List<Type> interfaces = GetMessageSinks(t);

                msi = new MessageSinkInfo(t, interfaces);
                _MessageSinkInfo.Add(t.FullName, msi);
            }

            return msi;
        }

        private static Dictionary<string, MessageSinkInfo> _MessageSinkInfo = new Dictionary<string, MessageSinkInfo>();

        private static List<Type> GetMessageSinks(Type t)
        {
            List<Type> rc = new List<Type>();

            foreach (Type i in t.GetTypeInfo().ImplementedInterfaces)
            {
                if (i.IsMessageSink())
                {
                    rc.Add(i);
                }
            }

            return rc;
        }
        #endregion
    }

    [ContentProperty("Messages")]
    public class MediatorInfo : IDisposable
    {
        public MediatorInfo()
        {
            ReferenceName = string.Empty;
            Messages.CollectionChanged += Messages_CollectionChanged;
        }

        void Messages_CollectionChanged(object s, System.Collections.Specialized.NotifyCollectionChangedEventArgs ea)
        {
            if (ea != null)
            {
                if (ea.NewItems != null)
                {
                    foreach (TypeWrapper wrapper in ea.NewItems)
                    {
                        Mediator.Register(wrapper);
                    }

                    Mediator.ProcessQueues();
                }
            }
        }

        public string ReferenceName
        {
            get;
            set;
        }

        public ObservableCollection<TypeWrapper> Messages
        {
            get
            {
                return _Messages;
            }
        }
        private ObservableCollection<TypeWrapper> _Messages = new ObservableCollection<TypeWrapper>();

        public Mediator Mediator
        {
            get
            {
                return _Mediator;
            }
        }
        private Mediator _Mediator = new Mediator();

        public void Dispose()
        {
            if (_Mediator != null)
            {
                _Mediator.Dispose();
                _Mediator = null;
            }

            if (_Messages != null)
            {
                _Messages.CollectionChanged -= Messages_CollectionChanged;

                foreach (TypeWrapper tw in _Messages)
                {
                    tw.Dispose();
                }

                _Messages.Clear();

                _Messages = null;
            }
        }
    }

    public class MessageInfo<T> where T : Message
    {
        public MessageInfo(T message)
        {
            _Message = message;

            HandledCount = 0;
            RetryCount = 0;
        }

        public int HandledCount
        {
            get;
            set;
        }

        public int RetryCount
        {
            get;
            set;
        }

        public List<IMessageSink<T>> Recipients
        {
            get
            {
                return _Recipients;
            }
        }
        private List<IMessageSink<T>> _Recipients = new List<IMessageSink<T>>();

        public T Message
        {
            get
            {
                return _Message;
            }
        }
        private T _Message;

        public string Key
        {
            get
            {
                return typeof(T).FullName;
            }
        }
    }

    [DebuggerDisplay("{Id}")]
    public abstract class Message
    {
        public Message()
        {
            Handled = false;
            Cancel = false;
            Multicast = false;

            MaxRetryAttempts = 0;

            Id = _Id;

            _Id ++;
        }

        public bool Handled
        {
            get;
            set;
        }

        public bool Cancel
        {
            get;
            set;
        }

        public bool Multicast
        {
            get;
            set;
        }

        public int MaxRetryAttempts
        {
            get;
            set;
        }

        public long Id
        {
            get;
            set;
        }

        static long _Id = 0;
    }
}
