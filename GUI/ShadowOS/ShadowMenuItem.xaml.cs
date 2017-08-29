using core.ui.data;
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
    /// Interaction logic for ShadowMenuItem.xaml
    /// </summary>
    public partial class ShadowMenuItem : UserControl
    {
        public ShadowMenuItem()
        {
            InitializeComponent();
        }

        #region TitleText
        public static DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleText", typeof(string), typeof(ShadowMenuItem), new PropertyMetadata(string.Empty, (s, ea) =>
        {
        }));

        public string TitleText
        {
            get
            {
                return (string)this.GetValue(TitleTextProperty);
            }

            set
            {
                this.SetValue(TitleTextProperty, value);
            }
        }
        #endregion

        #region Command
        public static DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(RelayCommand), typeof(ShadowMenuItem), new PropertyMetadata(null, (s, ea) =>
        {
        }));

        public RelayCommand Command
        {
            get
            {
                return (RelayCommand)this.GetValue(CommandProperty);
            }

            set
            {
                this.SetValue(CommandProperty, value);
            }
        }
        #endregion
    }
}
