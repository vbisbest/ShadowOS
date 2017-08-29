using core.ui.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace core.ui
{
    public class Touch
    {
        #region Click
        public static DependencyProperty ClickProperty = DependencyProperty.RegisterAttached("Click", typeof(RelayCommand), typeof(Touch), new PropertyMetadata(null, (s, ea) =>
        {
            FrameworkElement fe = s as FrameworkElement;

            ListViewItem lvi = s as ListViewItem;
            GridViewColumnHeader gvh = s as GridViewColumnHeader;

            if (lvi != null)
            {
                if (!lvi.IsInDesignMode())
                {
                    lvi.PreviewMouseLeftButtonUp -= lvi_SingleClick;
                    lvi.PreviewMouseLeftButtonUp += lvi_SingleClick;

                    fe.Publish(new Garbage(() => { lvi.MouseDown -= lvi_SingleClick; }, "Touch.ListViewItem.SingleClick"));
                }
            }
            else if (gvh != null)
            {
                if (!gvh.IsInDesignMode())
                {
                    gvh.PreviewMouseLeftButtonUp -= gvh_SingleClick;
                    gvh.PreviewMouseLeftButtonUp += gvh_SingleClick;

                    fe.Publish(new Garbage(() => { gvh.MouseDown -= gvh_SingleClick; }, "Touch.GridViewItem.SingleClick"));
                }
            }
            else if (fe != null)
            {
                if (!fe.IsInDesignMode())
                {
                    fe.PreviewMouseLeftButtonUp -= fe_SingleClick;
                    fe.PreviewMouseLeftButtonUp += fe_SingleClick;

                    fe.Publish(new Garbage(() => { fe.MouseDown -= fe_SingleClick; }, "Touch.SingleClick"));
                }
            }
        }));

        static void gvh_SingleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RelayCommand rc = GetClick(sender as DependencyObject);
            if (rc != null)
            {
                var element = Mouse.DirectlyOver;

                if (!(element is System.Windows.Controls.Primitives.Thumb))
                {
                    rc.Execute(sender);
                }
            }
        }

        static void lvi_SingleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;

            if (fe != null)
            {
                RelayCommand dc = GetClick(fe);

                if (dc != null)
                {
                    if (dc.CanExecute(null))
                    {
                        Action relay = () =>
                        {
                            dc.Execute(null);
                        };

                        relay.CleanInvoke(DispatcherHelper.UIDispatcher);
                    }
                }
            }
        }

        static void fe_SingleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RelayCommand dc = GetClick(sender as DependencyObject);

            if (dc != null)
            {
                if (dc.CanExecute(null))
                {
                    Action relay = () =>
                    {
                        dc.Execute(null);
                    };

                    relay.CleanInvoke(DispatcherHelper.UIDispatcher);
                }
            }
        }

        public static RelayCommand GetClick(DependencyObject d)
        {
            return (RelayCommand)d.GetValue(ClickProperty);
        }

        public static void SetClick(DependencyObject d, RelayCommand o)
        {
            d.SetValue(ClickProperty, o);
        }
        #endregion

        #region DoubleClick
        public static DependencyProperty DoubleClickProperty = DependencyProperty.RegisterAttached("DoubleClick", typeof(RelayCommand), typeof(Touch), new PropertyMetadata(null, (s, ea) =>
        {
            FrameworkElement fe = s as FrameworkElement;

            ListViewItem lvi = s as ListViewItem;
            GridViewColumnHeader gvh = s as GridViewColumnHeader;

            if (lvi != null)
            {
                if (!lvi.IsInDesignMode())
                {
                    lvi.MouseDoubleClick -= lvi_MouseDoubleClick;
                    lvi.MouseDoubleClick += lvi_MouseDoubleClick;

                    fe.Publish(new Garbage(() => { lvi.MouseDoubleClick -= lvi_MouseDoubleClick; }, "Touch.ListViewItem.DoubleClick"));
                }
            }
            else if (gvh != null)
            {
                if (!gvh.IsInDesignMode())
                {
                    gvh.MouseDoubleClick -= gvh_MouseDoubleClick;
                    gvh.MouseDoubleClick += gvh_MouseDoubleClick;

                    fe.Publish(new Garbage(() => { gvh.MouseDoubleClick -= gvh_MouseDoubleClick; }, "Touch.GridViewItem.DoubleClick"));
                }
            }
            else if (fe != null)
            {
                if (!fe.IsInDesignMode())
                {
                    fe.MouseDown -= fe_MouseDoubleClick;
                    fe.MouseDown += fe_MouseDoubleClick;

                    fe.Publish(new Garbage(() => { fe.MouseDown -= fe_MouseDoubleClick; }, "Touch.DoubleClick"));
                }
            }
        }));

        static void gvh_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RelayCommand rc = GetDoubleClick(sender as DependencyObject);
            if (rc != null)
            {
                rc.Execute(sender);
            }
        }

        static void lvi_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RelayCommand dc = GetDoubleClick(sender as DependencyObject);

            if (dc != null)
            {
                if (dc.CanExecute(null))
                {
                    Action relay = () =>
                    {
                        dc.Execute(null);
                    };

                    relay.CleanInvoke(DispatcherHelper.UIDispatcher);
                }
            }
        }

        static void fe_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                RelayCommand dc = GetDoubleClick(sender as DependencyObject);

                if (dc != null)
                {
                    if (dc.CanExecute(null))
                    {
                        Action relay = () =>
                        {
                            dc.Execute(null);
                        };

                        relay.CleanInvoke(DispatcherHelper.UIDispatcher);
                    }
                }
            }
        }

        public static RelayCommand GetDoubleClick(DependencyObject d)
        {
            return (RelayCommand)d.GetValue(DoubleClickProperty);
        }

        public static void SetDoubleClick(DependencyObject d, RelayCommand o)
        {
            d.SetValue(DoubleClickProperty, o);
        }
        #endregion
    }
}
