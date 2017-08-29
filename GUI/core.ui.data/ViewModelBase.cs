using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace core.ui.data
{
    public abstract class ViewModelBase : BindableBase, IDisposable
    {
        #region GotoState
        public static DependencyProperty GotoStateProperty = DependencyProperty.RegisterAttached("GotoState", typeof(string), typeof(ViewModelBase), new PropertyMetadata(string.Empty, (s, ea) =>
        {
            Control c = s as Control;

            if (c != null)
            {
                if (!c.IsInDesignMode())
                {
                    string state = (string) ea.NewValue;
                    VisualStateManager.GoToState(c, state, false);
                }
            }
        }));

        public static string GetGotoState(DependencyObject d)
        {
            return (string)d.GetValue(GotoStateProperty);
        }

        public static void SetGotoState(DependencyObject d, string o)
        {
            d.SetValue(GotoStateProperty, o);
        }
        #endregion

        #region Glue
        public static DependencyProperty GlueProperty = DependencyProperty.RegisterAttached("Glue", typeof(bool), typeof(ViewModelBase), new PropertyMetadata(false, (s, ea) =>
        {
            FrameworkElement fe = s as FrameworkElement;

            if (fe != null)
            {
                if (!fe.IsInDesignMode())
                {
                    bool glue = (bool) ea.NewValue;

                    if (glue)
                    {
                        fe.DataContextChanged += glue_DataContextChanged;

                        fe.Publish(new Garbage(() => { fe.DataContextChanged -= glue_DataContextChanged; }, "ViewModelBase.GlueProperty"));

                        GlueView(fe, fe.DataContext as ViewModelBase);
                    }
                }
            }
        }));

        public static void GlueView(FrameworkElement fe, ViewModelBase vmb)
        {
            if (vmb != null)
            {
                vmb.View = fe;
            }
        }

        static void glue_DataContextChanged(object sender, DependencyPropertyChangedEventArgs ea)
        {
            FrameworkElement fe = sender as FrameworkElement;

            if (fe != null)
            {
                GlueView(fe, fe.DataContext as ViewModelBase);
            }
        }

        public static bool GetGlue(DependencyObject d)
        {
            return (bool)d.GetValue(GlueProperty);
        }

        public static void SetGlue(DependencyObject d, bool glued)
        {
            d.SetValue(GlueProperty, glued);
        }
        #endregion

        #region ViewModel
        public static DependencyProperty ViewModelProperty = DependencyProperty.RegisterAttached("ViewModel", typeof(string), typeof(ViewModelBase), new PropertyMetadata(string.Empty, (s, ea) =>
        {
            FrameworkElement fe = s as FrameworkElement;

            if (fe != null)
            {
                if (!fe.IsInDesignMode())
                {
                    InjectViewModel(fe, ea.NewValue as string);
                    SetGlue(fe, true);
                }
            }
        }));

        static void InjectViewModel(FrameworkElement fe, string name)
        {
            if (fe != null)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    ViewModelBase vm = name.ToViewModel();

                    fe.DataContext = vm;
                }
            }
        }

        public static string GetViewModel(DependencyObject d)
        {
            return (string) d.GetValue(ViewModelProperty);
        }

        public static void SetViewModel(DependencyObject d, string name)
        {
            d.SetValue(ViewModelProperty, name);
        }
        #endregion

        public ViewModelBase()
        {
            ViewChanged += ViewModelBase_ViewChangedInternal;
            ViewChanged += OnViewChanged;

            Publish(new Garbage(this, string.Format("{0}:ViewModelBase.Ctor", this.GetType().Name)));
        }

        void ViewModelBase_ViewChangedInternal(FrameworkElement oldValue, FrameworkElement newValue)
        {
            if (newValue != null)
            {
                if (ImplementsMessageSink)
                {
                    newValue.Subscribe(this);
                }

                _PublishQueue.Process();
            }
        }

        private ActionQueue _PublishQueue = new ActionQueue();

        public void Publish<T>(T message) where T : Message
        {
            Publish(message, 0);
        }

        public void Publish<T>(T message, long index) where T : Message
        {
            if (View != null)
            {
                View.Publish(message);
            }
            else
            {
                long q = index;

                q = _PublishQueue.Add(() => { 
                    Publish(message, q); 
                }, index);
            }
        }

        public virtual void OnViewChanged(FrameworkElement oldValue, FrameworkElement newValue)
        {
        }

        public event Action<FrameworkElement, FrameworkElement> ViewChanged;

        public FrameworkElement View
        {
            get
            {
                return _View;
            }

            private set
            {
                FrameworkElement previous = _View;
                _View = value;

                if (previous != _View)
                {
                    ViewChanged(previous, value);
                }
            }
        }
        private FrameworkElement _View = null;

        public bool ImplementsMessageSink
        {
            get
            {
                return GetType().ImplementsMessageSink();
            }
        }

        public void Dispose()
        {
            Disposing();
            _View = null;
        }

        public abstract void Disposing();
    }
}
