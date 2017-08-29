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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        public MainWindow()
        {
            InitializeComponent();

            this.UseLayoutRounding = true;

            this.Loaded += (s, ea) =>
            {
                _GarbageCollector = new GarbageCollector(this);
            };

            this.Closing += (s, ea) =>
            {
                Dispose();
            };
        }

        public GarbageCollector GarbageCollector
        {
            get
            {
                return _GarbageCollector;
            }
        }

        private GarbageCollector _GarbageCollector = null;

        public void Dispose()
        {
            _GarbageCollector.Dispose();
            _GarbageCollector = null;
        }
    }
}
