using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Piny
{
    public class PinterestImage
    {
        #region Construtor(s)
        public PinterestImage(string thumbnailurl_)
        {
            // Init attributes
            this.image = new Image() { MaxWidth = 200 };
            this.thumbnaildownlaoded = false;
            this.originaldownlaoded = false;
            this.thumbnailurl = thumbnailurl_;
            this.originalurl = thumbnailurl_.Replace("236x", "originals");
            filename = System.IO.Path.GetFileName(new Uri(thumbnailurl_).LocalPath);
        }
        #endregion

        #region Methods
        public void LoadImageFromStream(Stream s)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = s;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            this.image.Source = bitmap;
        }
        #endregion

        #region Attributes
        public string thumbnailurl { get; set; }
        public string originalurl { get; set; }
        public string filename { get; set; }
        public bool thumbnaildownlaoded { get; set; }
        public bool originaldownlaoded { get; set; }
        public Image image { get; set; }
        #endregion
    }
}
