using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using core.ui.data;

namespace core.ui
{
    [ContentProperty("Stops")]
    public partial class ProgressBar : UserControl
    {
        public ProgressBar()
        {
            InitializeComponent();
            Stops = _Stops;
        }

        void Recalculate()
        {
            empty.Total = empty.Size;

            _Total = 0;

            foreach (ProgressStop stop in Stops)
            {
                _Total += stop.Size;
            }

            foreach (ProgressStop stop in Stops)
            {
                stop.Total = _Total;
            }
        }

        int _Total;

        ProgressStop empty = new ProgressStop() { Size = 1, Name = string.Empty, Fill = new SolidColorBrush("f0f0f0".ToColor()) };

        void Refresh()
        {
            Recalculate();

            sections.Children.Clear();
            sections.ColumnDefinitions.Clear();

            List<ProgressStop> nz = NonZeroStops;

            if (nz.Count > 1)
            {
                foreach (ProgressStop stop in nz)
                {
                    sections.ColumnDefinitions.Add(stop.ColumDefinition);
                }

                int index = 0;
                foreach (ProgressStop stop in nz)
                {
                    stop.BuildColor(sections, stop.ColumDefinition, index == 0, index == nz.Count - 1, ProgressBorderThickness, ProgressCornerRadius);

                    index++;
                }

                index = 0;
                foreach (ProgressStop stop in nz)
                {
                    stop.BuildHighlight(sections, stop.ColumDefinition, index == 0, index == nz.Count - 1, ProgressBorderThickness, ProgressCornerRadius, Highlight);

                    index++;
                }

                index = 0;
                foreach (ProgressStop stop in nz)
                {
                    stop.BuildBorder(sections, stop.ColumDefinition, index == 0, index == nz.Count - 1, ProgressBorderThickness, ProgressCornerRadius, ProgressBorderBrush);

                    index++;
                }
            }
            else if (nz.Count == 1)
            {
                ProgressStop stop = nz[0];

                sections.ColumnDefinitions.Add(stop.ColumDefinition);
                stop.BuildColor(sections, stop.ColumDefinition, true, true, ProgressBorderThickness, ProgressCornerRadius);
                stop.BuildHighlight(sections, stop.ColumDefinition, true, true, ProgressBorderThickness, ProgressCornerRadius, Highlight);
                stop.BuildBorder(sections, stop.ColumDefinition, true, true, ProgressBorderThickness, ProgressCornerRadius, ProgressBorderBrush);
            }
            else if (nz.Count == 0)
            {
                sections.ColumnDefinitions.Add(empty.ColumDefinition);
                empty.BuildColor(sections, empty.ColumDefinition, true, true, ProgressBorderThickness, ProgressCornerRadius);
                empty.BuildHighlight(sections, empty.ColumDefinition, true, true, ProgressBorderThickness, ProgressCornerRadius, Highlight);
                empty.BuildBorder(sections, empty.ColumDefinition, true, true, ProgressBorderThickness, ProgressCornerRadius, ProgressBorderBrush);
            }

            int size = 0;

            foreach (ProgressStop stop in nz)
            {
                if (stop.IncludeInCounts)
                {
                    size += stop.Size;
                }
            }
        }

        #region ProgressBorderBrush
        public static DependencyProperty ProgressBorderBrushProperty = DependencyProperty.Register("ProgressBorderBrush", typeof(Brush), typeof(ProgressBar), new PropertyMetadata(new SolidColorBrush(Colors.Black), (s, ea) =>
        {
            ProgressBar pb = s as ProgressBar;

            if (pb != null)
            {
                pb.Refresh();
            }
        }));

        public Brush ProgressBorderBrush
        {
            get
            {
                return (Brush)GetValue(ProgressBorderBrushProperty);
            }

            set
            {
                SetValue(ProgressBorderBrushProperty, value);
            }
        }
        #endregion

        #region ProgressBorderThickness
        public static DependencyProperty ProgressBorderThicknessProperty = DependencyProperty.Register("ProgressBorderThickness", typeof(Thickness), typeof(ProgressBar), new PropertyMetadata(new Thickness(1), (s, ea) =>
        {
            ProgressBar pb = s as ProgressBar;

            if (pb != null)
            {
                pb.Refresh();
            }
        }));

        public Thickness ProgressBorderThickness
        {
            get
            {
                return (Thickness)GetValue(ProgressBorderThicknessProperty);
            }

            set
            {
                SetValue(ProgressBorderThicknessProperty, value);
            }
        }
        #endregion

        #region ProgressCornerRadius
        public static DependencyProperty ProgressCornerRadiusProperty = DependencyProperty.Register("ProgressCornerRadius", typeof(CornerRadius), typeof(ProgressBar), new PropertyMetadata(new CornerRadius(5), (s, ea) =>
        {
            ProgressBar pb = s as ProgressBar;

            if (pb != null)
            {
                pb.Refresh();
            }
        }));

        public CornerRadius ProgressCornerRadius
        {
            get
            {
                return (CornerRadius)GetValue(ProgressCornerRadiusProperty);
            }

            set
            {
                SetValue(ProgressCornerRadiusProperty, value);
            }
        }
        #endregion

        #region Stops
        public static DependencyProperty StopsProperty = DependencyProperty.Register("Stops", typeof(ObservableCollection<ProgressStop>), typeof(ProgressBar), new PropertyMetadata(null, (s, ea) =>
        {
            ProgressBar bar = s as ProgressBar;

            ObservableCollection<ProgressStop> newValue = ea.NewValue as ObservableCollection<ProgressStop>;
            ObservableCollection<ProgressStop> oldValue = ea.OldValue as ObservableCollection<ProgressStop>;

            if (newValue != null)
            {
                newValue.CollectionChanged += (collection, cea) =>
                {
                    if (cea.NewItems != null && cea.NewItems.Count > 0)
                    {
                        foreach (ProgressStop stop in cea.NewItems)
                        {
                            stop.PropertyChanged += (ps, psea) =>
                            {
                                if (psea.PropertyName == "Size")
                                {
                                    bar.Refresh();
                                }
                            };
                        }
                    }

                    bar.Refresh();
                };

                if (newValue.Count > 0)
                {
                    foreach (ProgressStop stop in newValue)
                    {
                        stop.PropertyChanged += (ps, psea) =>
                        {
                            if (psea.PropertyName == "Size")
                            {
                                bar.Refresh();
                            }
                        };
                    }
                }

                bar.Refresh();
            }
        }));

        static void stop_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<ProgressStop> Stops
        {
            get
            {
                return (ObservableCollection<ProgressStop>)this.GetValue(StopsProperty);
            }
            set
            {
                this.SetValue(StopsProperty, value);
            }
        }
        ObservableCollection<ProgressStop> _Stops = new ObservableCollection<ProgressStop>();

        public List<ProgressStop> NonZeroStops
        {
            get
            {
                _NonZeroStops.Clear();

                foreach (ProgressStop ps in Stops)
                {
                    if (ps.Size > 0)
                    {
                        _NonZeroStops.Add(ps);
                    }
                }

                return _NonZeroStops;
            }
        }
        List<ProgressStop> _NonZeroStops = new List<ProgressStop>();
        #endregion

        private Brush Highlight
        {
            get
            {
                return this.Resources["glow"] as Brush;
            }
        }
    }

    public class ProgressStop : BindableBase
    {
        public string ToolTip
        {
            get
            {
                return _ToolTip;
            }
            set
            {
                _ToolTip = value;
                NotifyPropertyChanged(() => ToolTip);
            }
        }
        string _ToolTip = string.Empty;

        public bool IncludeInCounts
        {
            get
            {
                return _IncludeInCounts;
            }
            set
            {
                _IncludeInCounts = value;
                NotifyPropertyChanged(() => IncludeInCounts);
            }
        }
        bool _IncludeInCounts = true;

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                NotifyPropertyChanged(() => Name);
            }
        }
        private string _Name = string.Empty;
        public Brush Fill
        {
            get
            {
                return _Fill;
            }
            set
            {
                _Fill = value;
                NotifyPropertyChanged(() => Fill);
            }
        }
        private Brush _Fill = new SolidColorBrush(Colors.Transparent);

        public int Size
        {
            get
            {
                return _Size;
            }
            set
            {
                _Size = value;
                NotifyPropertyChanged(() => Size);
            }
        }
        private int _Size = 0;

        public int Total
        {
            get
            {
                return _Total;
            }
            set
            {
                _Total = value;

                if (!double.IsNaN(Percentage))
                {
                    _ColumnDefinition = new ColumnDefinition() { Width = new GridLength(Percentage, GridUnitType.Star) };
                }

                NotifyPropertyChanged(() => Total);
                NotifyPropertyChanged(() => Percentage);
            }
        }
        private int _Total = 0;

        public double Percentage
        {
            get
            {
                return ((double)Size / (double)Total) * 100.0;
            }
        }

        public ColumnDefinition ColumDefinition
        {
            get
            {
                return _ColumnDefinition;
            }
        }
        private ColumnDefinition _ColumnDefinition;

        public void BuildHighlight(Grid g, ColumnDefinition cd, bool first, bool last, Thickness thickness, CornerRadius radius, Brush brush)
        {
            CornerRadius left = new CornerRadius(radius.TopLeft, 0, 0, radius.BottomLeft);
            CornerRadius right = new CornerRadius(0, radius.TopRight, radius.BottomRight, 0);

            Thickness lBorder = new Thickness(thickness.Left, thickness.Top, 0, thickness.Bottom);
            Thickness rBorder = new Thickness(0, thickness.Top, thickness.Right, thickness.Bottom);
            Thickness cBorder = new Thickness(0, thickness.Top, 0, thickness.Bottom);

            CornerRadius both = new CornerRadius(radius.TopLeft, radius.TopRight, radius.BottomRight, radius.BottomLeft);
            Thickness bBorder = new Thickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);

            Border b = new Border();
            b.BorderBrush = new SolidColorBrush(Colors.Transparent);

            if (first && !last)
            {
                b.CornerRadius = left;
                b.BorderThickness = lBorder;
            }

            if (last && !first)
            {
                b.CornerRadius = right;
                b.BorderThickness = rBorder;
            }

            if (first && last)
            {
                b.CornerRadius = both;
                b.BorderThickness = bBorder;
            }

            if (!first && !last)
            {
                b.BorderThickness = cBorder;
            }

            b.Background = brush;

            g.Children.Add(b);
            Grid.SetColumn(b, g.IndexOf(cd));

        }
        public void BuildBorder(Grid g, ColumnDefinition cd, bool first, bool last, Thickness thickness, CornerRadius radius, Brush brush)
        {
            CornerRadius left = new CornerRadius(radius.TopLeft, 0, 0, radius.BottomLeft);
            CornerRadius right = new CornerRadius(0, radius.TopRight, radius.BottomRight, 0);

            Thickness lBorder = new Thickness(thickness.Left, thickness.Top, 0, thickness.Bottom);
            Thickness rBorder = new Thickness(0, thickness.Top, thickness.Right, thickness.Bottom);
            Thickness cBorder = new Thickness(0, thickness.Top, 0, thickness.Bottom);

            CornerRadius both = new CornerRadius(radius.TopLeft, radius.TopRight, radius.BottomRight, radius.BottomLeft);
            Thickness bBorder = new Thickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);

            Border b = new Border();
            b.BorderBrush = brush;

            if (first && !last)
            {
                b.CornerRadius = left;
                b.BorderThickness = lBorder;
            }

            if (last && !first)
            {
                b.CornerRadius = right;
                b.BorderThickness = rBorder;
            }

            if (first && last)
            {
                b.CornerRadius = both;
                b.BorderThickness = bBorder;
            }

            if (!first && !last)
            {
                b.BorderThickness = cBorder;
            }

            b.Background = new SolidColorBrush(Colors.Transparent);

            if (!string.IsNullOrEmpty(ToolTip))
            {
                b.ToolTip = string.Format(ToolTip, Size);
            }

            g.Children.Add(b);
            Grid.SetColumn(b, g.IndexOf(cd));
        }

        public void BuildColor(Grid g, ColumnDefinition cd, bool first, bool last, Thickness thickness, CornerRadius radius)
        {
            CornerRadius left = new CornerRadius(radius.TopLeft, 0, 0, radius.BottomLeft);
            CornerRadius right = new CornerRadius(0, radius.TopRight, radius.BottomRight, 0);

            Thickness lBorder = new Thickness(thickness.Left, thickness.Top, 0, thickness.Bottom);
            Thickness rBorder = new Thickness(0, thickness.Top, thickness.Right, thickness.Bottom);
            Thickness cBorder = new Thickness(0, thickness.Top, 0, thickness.Bottom);

            CornerRadius both = new CornerRadius(radius.TopLeft, radius.TopRight, radius.BottomRight, radius.BottomLeft);
            Thickness bBorder = new Thickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);

            Border b = new Border();
            b.BorderBrush = new SolidColorBrush(Colors.Transparent);

            if (first && !last)
            {
                b.CornerRadius = left;
                b.BorderThickness = lBorder;
            }

            if (last && !first)
            {
                b.CornerRadius = right;
                b.BorderThickness = rBorder;
            }

            if (first && last)
            {
                b.CornerRadius = both;
                b.BorderThickness = bBorder;
            }

            if (!first && !last)
            {
                b.BorderThickness = cBorder;
            }

            b.Background = Fill;

            g.Children.Add(b);
            Grid.SetColumn(b, g.IndexOf(cd));
        }
    }
}
