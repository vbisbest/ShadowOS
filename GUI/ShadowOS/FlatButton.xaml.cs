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
    /// Interaction logic for FlatButton.xaml
    /// </summary>
    public partial class FlatButton : UserControl
    {
        public FlatButton()
        {
            InitializeComponent();
        }

        #region HoverImage
        public static DependencyProperty HoverImageProperty = DependencyProperty.Register("HoverImage", typeof(string), typeof(FlatButton), new PropertyMetadata(string.Empty, (s, ea) =>
        {
        }));

        public string HoverImage
        {
            get
            {
                return (string)this.GetValue(HoverImageProperty);
            }

            set
            {
                this.SetValue(HoverImageProperty, value);
            }
        }
        #endregion

        #region Image
        public static DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(string), typeof(FlatButton), new PropertyMetadata(string.Empty, (s, ea) =>
        {
        }));

        public string Image
        {
            get
            {
                return (string)this.GetValue(ImageProperty);
            }

            set
            {
                this.SetValue(ImageProperty, value);
            }
        }
        #endregion

        #region HoverColor
        public static DependencyProperty HoverColorProperty = DependencyProperty.Register("HoverColor", typeof(Brush), typeof(FlatButton), new PropertyMetadata(new SolidColorBrush(Colors.Transparent), (s, ea) =>
        {
        }));

        public Brush HoverColor
        {
            get
            {
                return (Brush)this.GetValue(HoverColorProperty);
            }

            set
            {
                this.SetValue(HoverColorProperty, value);
            }
        }
        #endregion

        #region Color
        public static DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Brush), typeof(FlatButton), new PropertyMetadata(new SolidColorBrush(Colors.Transparent), (s, ea) =>
        {
        }));

        public Brush Color
        {
            get
            {
                return (Brush)this.GetValue(ColorProperty);
            }

            set
            {
                this.SetValue(ColorProperty, value);
            }
        }
        #endregion

        #region Command
        public static DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(FlatButton), new PropertyMetadata(null, (s, ea) =>
        {
        }));

        public ICommand Command
        {
            get
            {
                return (ICommand)this.GetValue(CommandProperty);
            }

            set
            {
                this.SetValue(CommandProperty, value);
            }
        }
        #endregion
    }
}
