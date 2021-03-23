using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PresenceLight
{
    public static class Extensions
    {
        public static System.Windows.Media.Color MapColor(this string hexColor)
        {
            return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexColor);
        }

        public static BitmapImage? LoadImage(this Stream imageData)
        {

            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();

            imageData.Position = 0;
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = null;
            image.StreamSource = imageData;
            image.EndInit();

            image.Freeze();
            return image;

        }
    }
}
