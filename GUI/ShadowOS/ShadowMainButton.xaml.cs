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
    /// Interaction logic for ShadowMainButton.xaml
    /// </summary>
    public partial class ShadowMainButton : UserControl
    {
        public ShadowMainButton()
        {
            InitializeComponent();
        }

        #region TitleText
        public static DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleText", typeof(string), typeof(ShadowMainButton), new PropertyMetadata(string.Empty, (s, ea) =>
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
        public static DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(RelayCommand), typeof(ShadowMainButton), new PropertyMetadata(null, (s, ea) =>
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
