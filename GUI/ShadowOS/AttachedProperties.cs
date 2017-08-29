using core.ui.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ShadowOS
{
    public static class WindowHelper
    {
        #region ToggleMaximize
        public static DependencyProperty ToggleMaximizeProperty = DependencyProperty.RegisterAttached("ToggleMaximize", typeof(bool), typeof(WindowHelper), new PropertyMetadata(false, (s, ea) =>
        {
            FrameworkElement fe = s as FrameworkElement;

            if (fe != null)
            {
                bool enabled = (bool)ea.NewValue;

                if (enabled)
                {
                    fe.MouseLeftButtonDown -= fe_DoubleButtonDown;
                    fe.MouseLeftButtonDown += fe_DoubleButtonDown;
                }
                else
                {
                    fe.MouseLeftButtonDown -= fe_DoubleButtonDown;
                }
            }
        }));

        static void fe_DoubleButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                FrameworkElement fe = sender as FrameworkElement;

                if (fe != null)
                {
                    Window w = fe.Find(typeof(Window)) as Window;

                    if (w != null)
                    {
                        w.WindowState = w.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                    }
                }
            }
        }

        public static bool GetToggleMaximize(DependencyObject d)
        {
            return (bool)d.GetValue(ToggleMaximizeProperty);
        }

        public static void SetToggleMaximize(DependencyObject d, bool o)
        {
            d.SetValue(ToggleMaximizeProperty, o);
        }
        #endregion

        #region Drag
        public static DependencyProperty DragProperty = DependencyProperty.RegisterAttached("Drag", typeof(bool), typeof(WindowHelper), new PropertyMetadata(false, (s, ea) =>
        {
            FrameworkElement fe = s as FrameworkElement;

            if (fe != null)
            {
                bool enabled = (bool)ea.NewValue;

                if (enabled)
                {
                    fe.MouseLeftButtonDown -= fe_MouseLeftButtonDown;
                    fe.MouseLeftButtonDown += fe_MouseLeftButtonDown;
                }
                else
                {
                    fe.MouseLeftButtonDown -= fe_MouseLeftButtonDown;
                }
            }
        }));

        static void fe_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;

            if (fe != null)
            {
                Window w = fe.Find(typeof(Window)) as Window;

                if (w != null)
                {
                    w.DragMove();
                }
            }
        }

        public static bool GetDrag(DependencyObject d)
        {
            return (bool)d.GetValue(DragProperty);
        }

        public static void SetDrag(DependencyObject d, bool o)
        {
            d.SetValue(DragProperty, o);
        }
        #endregion
    }

    public static class ThumbHelper
    {
        #region Resize
        public static DependencyProperty ResizeProperty = DependencyProperty.RegisterAttached("Resize", typeof(bool), typeof(ThumbHelper), new PropertyMetadata(false, (s, ea) =>
        {
            Thumb t = s as Thumb;

            if (t != null)
            {
                bool enabled = (bool)ea.NewValue;

                if (enabled)
                {
                    t.DragDelta += t_DragDelta;
                }
            }
        }));

        static void t_DragDelta(object sender, DragDeltaEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;

            if (fe != null)
            {
                Window w = fe.Find(typeof(Window)) as Window;

                if (w != null)
                {
                    try
                    {
                        w.Height += e.VerticalChange;
                        w.Width += e.HorizontalChange;
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static bool GetResize(DependencyObject d)
        {
            return (bool)d.GetValue(ResizeProperty);
        }

        public static void SetResize(DependencyObject d, bool o)
        {
            d.SetValue(ResizeProperty, o);
        }
        #endregion
    }

    public static class PopupHelper
    {
        #region Close
        public static DependencyProperty CloseProperty = DependencyProperty.RegisterAttached("Close", typeof(RelayCommand), typeof(PopupHelper), new PropertyMetadata(null, (s, ea) =>
        {
            Popup p = s as Popup;

            if (p != null)
            {
                p.LostFocus += p_Closed;
            }
        }));

        static void p_Closed(object sender, EventArgs e)
        {
            RelayCommand rc = GetClose(sender as DependencyObject);

            if (rc != null)
            {
                if (rc.CanExecute(null))
                {
                    rc.Execute(null);
                }
            }
        }

        public static RelayCommand GetClose(DependencyObject d)
        {
            return (RelayCommand)d.GetValue(CloseProperty);
        }

        public static void SetClose(DependencyObject d, RelayCommand o)
        {
            d.SetValue(CloseProperty, o);
        }
        #endregion
    }

    public static class GridViewHelper
    {
        #region Manage
        public static DependencyProperty ManageProperty = DependencyProperty.RegisterAttached("Manage", typeof(bool), typeof(GridViewHelper), new PropertyMetadata(false, (s, ea) =>
        {
            ListView lv = s as ListView;

            if (lv != null)
            {
                if (!lv.IsInDesignMode())
                {
                    lv.SizeChanged -= lv_SizeChanged;
                    lv.SizeChanged += lv_SizeChanged;
                }
            }
        }));

        static void lv_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView lv = sender as ListView;

            if (lv != null)
            {
                GridView gv = lv.View as GridView;

                if (gv != null)
                {
                    bool scrollviewer = ScrollViewer.GetVerticalScrollBarVisibility(gv) == ScrollBarVisibility.Visible;

                    double width = 0;

                    foreach (var c in gv.Columns)
                    {
                        width += c.Width;
                    }

                    width -= gv.Columns[gv.Columns.Count - 1].Width;

                    width += 6;

                    width += scrollviewer == true ? SystemParameters.VerticalScrollBarWidth : 0;

                    gv.Columns[gv.Columns.Count - 1].Width = lv.ActualWidth - width;
                }
            }
        }

        public static bool GetManage(DependencyObject d)
        {
            return (bool)d.GetValue(ManageProperty);
        }

        public static void SetManage(DependencyObject d, bool o)
        {
            d.SetValue(ManageProperty, o);
        }
        #endregion

        #region Maximize
        public static DependencyProperty MaximizeProperty = DependencyProperty.RegisterAttached("Maximize", typeof(bool), typeof(GridViewHelper), new PropertyMetadata(false, (s, ea) =>
        {
            GridViewColumn c = s as GridViewColumn;
        }));

        public static bool GetMaximize(DependencyObject d)
        {
            return (bool)d.GetValue(MaximizeProperty);
        }

        public static void SetMaximize(DependencyObject d, bool o)
        {
            d.SetValue(MaximizeProperty, o);
        }
        #endregion
    }

    public static class CanvasHelper
    {
        #region Position
        public static DependencyProperty PositionProperty = DependencyProperty.RegisterAttached("Position", typeof(FrameworkElement), typeof(CanvasHelper), new PropertyMetadata(null, (s, ea) =>
        {
            FrameworkElement fe = s as FrameworkElement;

            if (fe != null)
            {
                if (!fe.IsInDesignMode())
                {
                    fe.IsVisibleChanged -= fe_IsVisibleChanged;
                    fe.IsVisibleChanged += fe_IsVisibleChanged;
                }
            }
        }));

        static void fe_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;

            if (fe != null)
            {
                Window w = MoreVisualTreeHelper.Find(fe, typeof(Window)) as Window;

                if (w != null)
                {
                    FrameworkElement position = GetPosition(fe);

                    Point point = position.TransformToAncestor(w).Transform(new Point(0, 0));

                    Canvas.SetLeft(fe, point.X - 10);
                    Canvas.SetTop(fe, point.Y - 10);
                }
            }
        }

        public static FrameworkElement GetPosition(DependencyObject d)
        {
            return (FrameworkElement)d.GetValue(PositionProperty);
        }

        public static void SetPosition(DependencyObject d, FrameworkElement o)
        {
            d.SetValue(PositionProperty, o);
        }
        #endregion
    }
}
