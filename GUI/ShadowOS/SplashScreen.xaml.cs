using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ShadowOS
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
            Loaded += SplashScreen_Loaded;
            PreviewKeyDown += SplashScreen_PreviewKeyDown;
        }

        void SplashScreen_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Close();
        }

        async void SplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(new TimeSpan(0,0,6));

            //Close();
        }
    }
}
