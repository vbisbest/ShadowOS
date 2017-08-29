using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ShadowOS
{
    public class GrayscaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            int i = (int) (double) value;

            return new SolidColorBrush(string.Format("{0}{0}{0}", i.ToString("X2")).ToColor());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return value;
        }
    }

    public class WindowStateHoverConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            string rc = string.Empty;

            WindowState state = (WindowState)value;

            switch (state)
            {
                case WindowState.Maximized:
                    rc = @"Images\26\custom\restoreselected.png";
                    break;

                case WindowState.Minimized:
                    rc = @"Images\26\custom\restoreselected.png";
                    break;

                case WindowState.Normal:
                    rc = @"Images\26\custom\maximizeselected.png";
                    break;
            }

            return rc;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return value;
        }
    }

    public class WindowStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            string rc = string.Empty;

            WindowState state = (WindowState)value;

            switch (state)
            {
                case WindowState.Maximized:
                    rc = @"Images\26\custom\restore.png";
                    break;

                case WindowState.Minimized:
                    rc = @"Images\26\custom\restore.png";
                    break;

                case WindowState.Normal:
                    rc = @"Images\26\custom\maximize.png";
                    break;
            }

            return rc;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return value;
        }
    }
}
