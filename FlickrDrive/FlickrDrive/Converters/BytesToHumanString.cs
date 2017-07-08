using System;
using System.Windows.Data;

namespace FlickrDrive.Converters
{
    class BytesToHumanString : IValueConverter
    {
        string BytesToString(long byteCount)
        {
            string[] suffix = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
            {
                return "0" + " " + suffix[0];
            }
            long bytes = Math.Abs(byteCount);
            int place = (int)(Math.Floor(Math.Log(bytes, 1024)));
            double resultValue = Math.Round(bytes / Math.Pow(1024, place), 1);
            return resultValue + " " + suffix[place];
        }

        public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture)
        {
            return BytesToString((long)value);
        }


        public object ConvertBack(object value, Type targetType, object parameter,
                        System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
