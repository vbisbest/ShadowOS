using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace System
{
    public static class MoreGrid
    {
        public static void Add(this Grid source, ColumnDefinition cd, RowDefinition rd, UIElement uie)
        {
            if (source != null)
            {
                if (uie != null)
                {
                    if (!source.Children.Contains(uie))
                    {
                        source.Children.Add(uie);
                    }

                    if (cd != null)
                    {
                        Grid.SetColumn(uie, source.IndexOf(cd));
                    }

                    if (rd != null)
                    {
                        Grid.SetRow(uie, source.IndexOf(rd));
                    }
                }
            }
        }

        public static int IndexOf(this Grid source, ColumnDefinition cd)
        {
            int rc = -1;

            if (source != null)
            {
                if (cd != null)
                {
                    int index = -1;
                    foreach (ColumnDefinition column in source.ColumnDefinitions)
                    {
                        index++;

                        if (column == cd)
                        {
                            rc = index;
                            break;
                        }
                    }
                }
            }

            return rc;
        }

        public static int IndexOf(this Grid source, RowDefinition rd)
        {
            int rc = -1;

            if (source != null)
            {
                if (rd != null)
                {
                    int index = -1;
                    foreach (RowDefinition row in source.RowDefinitions)
                    {
                        index++;

                        if (row == rd)
                        {
                            rc = index;
                            break;
                        }
                    }
                }
            }

            return rc;
        }
    }

    public static class MoreString
    {
        public static Color ToColor(this string source)
        {
            Color rc = Colors.Transparent;


            try
            {
                rc = Color.FromRgb((byte)int.Parse(source.Substring(0, 2), NumberStyles.HexNumber), (byte)int.Parse(source.Substring(2, 2), NumberStyles.HexNumber), (byte)int.Parse(source.Substring(4, 2), NumberStyles.HexNumber));
            }
            catch
            {
            }

            return rc;
        }
    }
}
