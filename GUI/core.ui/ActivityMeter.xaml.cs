using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace core.ui
{
    public partial class ActivityMeter : UserControl
    {
        public ActivityMeter()
        {
            InitializeComponent();

            InitializeChart();
            InitializePointer();
        }        

        void InitializePointer()
        {
            pointer.BarBorderBrush = PointerBrush;
            pointer.BarBorderThickness = new Thickness(0,PointerThickness,0,0);
        }

        void InitializeChart()
        {
            chartBorder.BorderBrush = ChartBorderBrush;
            chartBorder.BorderThickness = new Thickness(0, 0, ChartBorderThickness.Right, ChartBorderThickness.Bottom);

            chart.ColumnDefinitions.Clear();
            chart.RowDefinitions.Clear();
            chart.Children.Clear();

            for (int row = 0; row < ChartRows; row++)
            {
                chart.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }

            for (int column = 0; column < ChartColumns; column++)
            {
                chart.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }

            foreach(RowDefinition rd in chart.RowDefinitions)
            {
                foreach (ColumnDefinition cd in chart.ColumnDefinitions)
                {
                    Border b = new Border() { Background = new SolidColorBrush(Colors.Transparent), BorderBrush = ChartBorderBrush, BorderThickness = new Thickness (ChartBorderThickness.Left, ChartBorderThickness.Top, 0, 0) };
                    chart.Add(cd, rd, b);
                }
            }
        }

        void InitializeGraph()
        {
            graph.ColumnDefinitions.Clear();

            for (int i = 0; i < ProgressSize; i++)
            {
                graph.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }

            StartPathTimer();

            for (int i = 0; i < ProgressSize; i++)
            {
                Add(0);
            }
        }

        void Refresh()
        {
            for (int i = _Bars.Count - 1; i >= 0; i--)
            {
                Grid.SetColumn(_Bars[i], i);
            }
        }

        void Recalculate(double scale)
        {
            if(!double.IsInfinity(scale) && !double.IsNaN(scale))
            {
                foreach(VerticalBar b in _Bars)
                {
                    int height = (int) ((double) b.BarHeight * scale);

                    if(height <= 100)
                    {
                        b.BarHeight = height;
                    }
                }
            }
        }

        void ProcessTick()
        {
            lock (this)
            {
                pointer.Text = Tick.Value.ToString();

                UpdateMax((double)Tick.Value);

                if (Max != 0)
                {
                    Add((int)(((double)Tick.Value / Max) * 100.0));
                }
                else
                {
                    Add(0);
                }
            }
        }


        double Max
        {
            get
            {
                double rc = 0;

                foreach (double max in _Max)
                {
                    if (max > rc)
                    {
                        rc = max;
                    }
                }

                return rc;
            }
        }

        void UpdateMax(double max)
        {
            int index = _Max.Count;
            if (index < ProgressSize)
            {
                if (Max != 0 && Max < max)
                {
                    Recalculate(Max / max);
                }

                _Max.Add(max);
            }
            else
            {
                double old = Max;
                _Max.RemoveAt(0);

                if (old != Max)
                {
                    Recalculate(old / Max);
                }

                UpdateMax(max);
            }
        }
        List<double> _Max = new List<double>();

        List<VerticalBar> _Bars = new List<VerticalBar>();
        void Add(int percentage)
        {
            int index = _Bars.Count;

            if (index < ProgressSize)
            {
                VerticalBar bar = new VerticalBar() { BarBrush = GraphBrush , BarHeight = percentage };
                _Bars.Add(bar);

                Grid.SetColumn(bar, index);
                graph.Children.Add(bar);
            }
            else
            {
                graph.Children.Remove(_Bars[0]);
                _Bars.RemoveAt(0);

                Refresh();

                Add(percentage);
            }

            UpdateCurrentHeight(percentage);
        }


        void UpdateCurrentHeight(int percentage)
        {
            lock (_PathTimer)
            {
                _CurrentHeight = percentage;
            }
        }

        int _CurrentHeight = 0;
        DispatcherTimer _PathTimer;
        void StartPathTimer()
        {
            _PathTimer = new System.Windows.Threading.DispatcherTimer();
            _PathTimer.Tick += (s, ea) =>
            {
                lock (_PathTimer)
                {
                    if(_CurrentHeight > pointer.BarHeight)
                    {
                        pointer.BarHeight ++;
                    }
                    else if(_CurrentHeight < pointer.BarHeight)
                    {
                        pointer.BarHeight --;
                    }
                }
            };
            _PathTimer.Interval = new TimeSpan(0, 0, 0, 0, 30);
            _PathTimer.Start();
        }

        #region Tick
        public static DependencyProperty TickProperty = DependencyProperty.Register("Tick", typeof(ProgressTick), typeof(ActivityMeter), new PropertyMetadata(null, (s, ea) =>
        {
            ActivityMeter tb = s as ActivityMeter;

            if (tb != null)
            {
                tb.ProcessTick();
            }
        }));

        public ProgressTick Tick
        {
            get
            {
                return (ProgressTick) GetValue(TickProperty);
            }

            set
            {
                SetValue(TickProperty, value);
            }
        }
        #endregion

        #region ProgressSize
        public static DependencyProperty ProgressSizeProperty = DependencyProperty.Register("ProgressSize", typeof(int), typeof(ActivityMeter), new PropertyMetadata(0, (s, ea) =>
        {
            ActivityMeter tb = s as ActivityMeter;

            if (tb != null)
            {
                tb.InitializeGraph();
            }
        }));

        public int ProgressSize
        {
            get
            {
                return (int)GetValue(ProgressSizeProperty);
            }

            set
            {
                SetValue(ProgressSizeProperty, value);
            }
        }
        #endregion

        #region GraphBrush
        public static DependencyProperty GraphBrushProperty = DependencyProperty.Register("GraphBrush", typeof(Brush), typeof(ActivityMeter), new PropertyMetadata(new SolidColorBrush(Colors.Green), (s, ea) =>
        {
            ActivityMeter tb = s as ActivityMeter;

            if (tb != null)
            {
                tb.InitializeGraph();
            }
        }));

        public Brush GraphBrush
        {
            get
            {
                return (Brush)GetValue(GraphBrushProperty);
            }

            set
            {
                SetValue(GraphBrushProperty, value);
            }
        }
        #endregion

        #region ChartRows
        public static DependencyProperty ChartRowsProperty = DependencyProperty.Register("ChartRows", typeof(int), typeof(ActivityMeter), new PropertyMetadata(2, (s, ea) =>
        {
            ActivityMeter tb = s as ActivityMeter;

            if (tb != null)
            {
                tb.InitializeChart();
            }
        }));

        public int ChartRows
        {
            get
            {
                return (int)GetValue(ChartRowsProperty);
            }

            set
            {
                SetValue(ChartRowsProperty, value);
            }
        }
        #endregion

        #region ChartColumns
        public static DependencyProperty ChartColumnsProperty = DependencyProperty.Register("ChartColumns", typeof(int), typeof(ActivityMeter), new PropertyMetadata(8, (s, ea) =>
        {
            ActivityMeter tb = s as ActivityMeter;

            if (tb != null)
            {
                tb.InitializeChart();
            }
        }));

        public int ChartColumns
        {
            get
            {
                return (int)GetValue(ChartColumnsProperty);
            }

            set
            {
                SetValue(ChartColumnsProperty, value);
            }
        }
        #endregion

        #region ChartBorderBrush
        public static DependencyProperty ChartBorderBrushProperty = DependencyProperty.Register("ChartBorderBrush", typeof(Brush), typeof(ActivityMeter), new PropertyMetadata(new SolidColorBrush(Colors.Gray), (s, ea) =>
        {
            ActivityMeter tb = s as ActivityMeter;

            if (tb != null)
            {
                tb.InitializeChart();
            }
        }));

        public Brush ChartBorderBrush
        {
            get
            {
                return (Brush)GetValue(ChartBorderBrushProperty);
            }

            set
            {
                SetValue(ChartBorderBrushProperty, value);
            }
        }
        #endregion

        #region ChartBorderThickness
        public static DependencyProperty ChartBorderThicknessProperty = DependencyProperty.Register("ChartBorderThickness", typeof(Thickness), typeof(ActivityMeter), new PropertyMetadata(new Thickness(1), (s, ea) =>
        {
            ActivityMeter tb = s as ActivityMeter;

            if (tb != null)
            {
                tb.InitializeChart();
            }
        }));

        public Thickness ChartBorderThickness
        {
            get
            {
                return (Thickness)GetValue(ChartBorderThicknessProperty);
            }

            set
            {
                SetValue(ChartBorderThicknessProperty, value);
            }
        }
        #endregion

        #region PointerBrush
        public static DependencyProperty PointerBrushProperty = DependencyProperty.Register("PointerBrush", typeof(Brush), typeof(ActivityMeter), new PropertyMetadata(new SolidColorBrush(Colors.Black) { Opacity = .5 }, (s, ea) =>
        {
            ActivityMeter tb = s as ActivityMeter;

            if (tb != null)
            {
                tb.InitializePointer();
            }
        }));

        public Brush PointerBrush
        {
            get
            {
                return (Brush)GetValue(PointerBrushProperty);
            }

            set
            {
                SetValue(PointerBrushProperty, value);
            }
        }
        #endregion

        #region PointerThickness
        public static DependencyProperty PointerThicknessProperty = DependencyProperty.Register("PointerThickness", typeof(int), typeof(ActivityMeter), new PropertyMetadata(2, (s, ea) =>
        {
            ActivityMeter tb = s as ActivityMeter;

            if (tb != null)
            {
                tb.InitializePointer();
            }
        }));

        public int PointerThickness
        {
            get
            {
                return (int)GetValue(PointerThicknessProperty);
            }

            set
            {
                SetValue(PointerThicknessProperty, value);
            }
        }
        #endregion
    }

    internal class VerticalBar : Border
    {
        public VerticalBar()
        {
            Initialize();
        }

        Grid Grid
        {
            get;
            set;
        }

        RowDefinition Top
        {
            get;
            set;
        }

        RowDefinition Bottom
        {
            get;
            set;
        }

        #region Text
        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(VerticalBar), new PropertyMetadata(string.Empty, (s, ea) =>
        {
            VerticalBar cb = s as VerticalBar;

            if (cb != null)
            {
                cb.UpdateText();
            }
        }));

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }

            set
            {
                SetValue(TextProperty, value);
            }
        }
        #endregion 

        TextBlock TextBlock
        {
            get;
            set;
        }

        #region BarBrush
        public static DependencyProperty BarBrushProperty = DependencyProperty.Register("BarBrush", typeof(Brush), typeof(VerticalBar), new PropertyMetadata(new SolidColorBrush(Colors.Green), (s, ea) =>
        {
            VerticalBar cb = s as VerticalBar;

            if (cb != null)
            {
                cb.Update();
            }
        }));

        public Brush BarBrush
        {
            get
            {
                return (Brush) GetValue(BarBrushProperty);
            }

            set
            {
                SetValue(BarBrushProperty, value);
            }
        }
        #endregion

        #region BarBorderBrush
        public static DependencyProperty BarBorderBrushProperty = DependencyProperty.Register("BarBorderBrush", typeof(Brush), typeof(VerticalBar), new PropertyMetadata(new SolidColorBrush(Colors.Transparent), (s, ea) =>
        {
            VerticalBar cb = s as VerticalBar;

            if (cb != null)
            {
                cb.Update();
            }
        }));

        public Brush BarBorderBrush
        {
            get
            {
                return (Brush)GetValue(BarBorderBrushProperty);
            }

            set
            {
                SetValue(BarBorderBrushProperty, value);
            }
        }
        #endregion

        #region BarBorderThickness
        public static DependencyProperty BarBorderThicknessProperty = DependencyProperty.Register("BarBorderThickness", typeof(Thickness), typeof(VerticalBar), new PropertyMetadata(new Thickness(0), (s, ea) =>
        {
            VerticalBar cb = s as VerticalBar;

            if (cb != null)
            {
                cb.Update();
            }
        }));

        public Thickness BarBorderThickness
        {
            get
            {
                return (Thickness)GetValue(BarBorderThicknessProperty);
            }

            set
            {
                SetValue(BarBorderThicknessProperty, value);
            }
        }
        #endregion

        #region BarHeight
        public static DependencyProperty BarHeightProperty = DependencyProperty.Register("BarHeight", typeof(int), typeof(VerticalBar), new PropertyMetadata(0, (s, ea) =>
        {
            VerticalBar cb = s as VerticalBar;

            if (cb != null)
            {
                cb.Update();
            }
        }));

        public int BarHeight
        {
            get
            {
                return (int)GetValue(BarHeightProperty);
            }

            set
            {
                SetValue(BarHeightProperty, value);
            }
        }
        #endregion


        void UpdateText()
        {
            TextBlock.Text = Text;
        }

        void Update()
        {
            _Content.Background = BarBrush;
            _Content.BorderBrush = BarBorderBrush;
            _Content.BorderThickness = BarBorderThickness;

            if (BarHeight > 100 || BarHeight < 0)
            {
                throw new ArgumentException("BarHeight must be within the reange [0 - 100]");
            }

            Top.Height = new GridLength(1, GridUnitType.Star);
            if (BarHeight == 0)
            {
                Top.Height = new GridLength(1, GridUnitType.Star);
                Bottom.Height = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                Top.Height = new GridLength(100 - BarHeight, GridUnitType.Star);
                Bottom.Height = new GridLength(BarHeight, GridUnitType.Star);
            }

            if (BarHeight >= 20)
            {
                Grid.Add(null, Bottom, TextBlock);
                TextBlock.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            }
            else
            {
                Grid.Add(null, Top, TextBlock);
                TextBlock.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            }
        }

        Border _Content = new Border();
        Path _Path = new Path() { Data = Geometry.Parse("M 0,0 L 1, 0"), Stretch = Stretch.Fill, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(5) };

        void Initialize()
        {
            Grid = new Grid();
            Top = new RowDefinition();
            Bottom = new RowDefinition();

            Grid.RowDefinitions.Add(Top);
            Grid.RowDefinitions.Add(Bottom);

            Grid.Add(null, Bottom, _Content);

            this.Child = Grid;

            TextBlock = new TextBlock() { FontSize = 8, Foreground = new SolidColorBrush(Colors.Black) };
            TextBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            TextBlock.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            Grid.Add(null, Bottom, TextBlock);

            Update();
            UpdateText();
        }
    }

    public class ProgressTick
    {
        public int Value
        {
            get;
            set;
        }
    }
}
