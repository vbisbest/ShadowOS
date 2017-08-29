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

namespace ShadowOS
{
    /// <summary>
    /// Interaction logic for ShadowTab.xaml
    /// </summary>
    public partial class ShadowTab : UserControl
    {
        public ShadowTab()
        {
            InitializeComponent();
        }

        #region TitleText
        public static DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleText", typeof(string), typeof(ShadowTab), new PropertyMetadata(string.Empty, (s, ea) =>
        {
            ShadowTab t = s as ShadowTab;
        }));

        public string TitleText
        {
            get
            {
                return (string) this.GetValue(TitleTextProperty);
            }

            set
            {
                this.SetValue(TitleTextProperty, value);
            }
        }
        #endregion

        #region IsSelected
        public static DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(ShadowTab), new PropertyMetadata(false, (s, ea) =>
        {
            ShadowTab t = s as ShadowTab;
        }));

        public bool IsSelected
        {
            get
            {
                return (bool) this.GetValue(IsSelectedProperty);
            }

            set
            {
                this.SetValue(IsSelectedProperty, value);
            }
        }
        #endregion

        #region SelectedImage
        public static DependencyProperty SelectedImageProperty = DependencyProperty.Register("SelectedImage", typeof(string), typeof(ShadowTab), new PropertyMetadata(string.Empty, (s, ea) =>
        {
            ShadowTab t = s as ShadowTab;
        }));

        public string SelectedImage
        {
            get
            {
                return (string)this.GetValue(SelectedImageProperty);
            }

            set
            {
                this.SetValue(SelectedImageProperty, value);
            }
        }
        #endregion

        #region UnselectedImage
        public static DependencyProperty UnselectedImageProperty = DependencyProperty.Register("UnselectedImage", typeof(string), typeof(ShadowTab), new PropertyMetadata(string.Empty, (s, ea) =>
        {
            ShadowTab t = s as ShadowTab;
        }));

        public string UnselectedImage
        {
            get
            {
                return (string)this.GetValue(UnselectedImageProperty);
            }

            set
            {
                this.SetValue(UnselectedImageProperty, value);
            }
        }
        #endregion

        #region EventCount
        public static DependencyProperty EventCountProperty = DependencyProperty.Register("EventCount", typeof(int), typeof(ShadowTab), new PropertyMetadata(0, (s, ea) =>
        {
            ShadowTab t = s as ShadowTab;
        }));

        public int EventCount
        {
            get
            {
                return (int)this.GetValue(EventCountProperty);
            }

            set
            {
                this.SetValue(EventCountProperty, value);
            }
        }
        #endregion
    }
}
