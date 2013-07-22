using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Piny
{
    /// <summary>
    /// Interaction logic for Core.xaml
    /// </summary>
    public partial class Core : Application
    {
        // Core here
    }

    public class UriToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (File.Exists(value.ToString()) == false)
                return (null);
            BitmapFrame frame = BitmapFrame.Create(new Uri(value.ToString()), BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);

            if (frame.Thumbnail != null)
                return (frame.Thumbnail);

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelWidth = 10;
            bi.DecodePixelHeight = 10;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.CreateOptions = BitmapCreateOptions.DelayCreation;
            bi.UriSource = new Uri(value.ToString());
            bi.EndInit();
            return (bi);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
