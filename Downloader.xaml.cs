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
using System.Windows.Shapes;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Windows.Navigation;

namespace Piny
{
    /// <summary>
    /// Interaction logic for Downloader.xaml
    /// </summary>
    public partial class Downloader : Window
    {
        #region Constructors
        public Downloader()
        {
            // Default calls
            InitializeComponent();

            // Init attributes
            this.Success = false;
            this.SaveThumbnails = false;
            this.CreatedSpecificFolder = false;
            this.WebClient_ = new WebClient();
            this.OriginalImagesURLS = new List<string> { };
            this.ThumbnailsImagesURLS = new List<string> { };
            this.Dialog = new System.Windows.Forms.FolderBrowserDialog();
            
            // Init control attributes
            this.DownloadFolder.Text= Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            // Init events
            /*this.DeveloperLink.RequestNavigate += new RequestNavigateEventHandler(RequestNavigateHandler);
            this.ApplicationLink.RequestNavigate += new RequestNavigateEventHandler(RequestNavigateHandler);
             * */
        }
        #endregion

        #region Methods
        private void RequestNavigateHandler(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private bool AnalyseSourceCode()
        {
            // Find images URls in page source code by selecting nodes (thx to HtmlAgilityPack)
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(this.BoardSourceCode.Text);

            var itemList = doc.DocumentNode.SelectNodes("//img").ToList();
            foreach (HtmlAgilityPack.HtmlNode x in itemList)
                if (!x.Attributes["src"].Value.Contains("avatars") && !x.Attributes["src"].Value.Contains("api"))
                    this.ThumbnailsImagesURLS.Add(x.Attributes["src"].Value);

           // Find images URls in page source code with regulary expression

            /*Regex regx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);
            MatchCollection matches = regx.Matches(this.BoardSourceCode.Text);

            foreach (Match match in matches)
            {
                if (!exist(this.Images, match.Value) && match.Value.EndsWith(".jpg"))
                {
                    this.Images.Add(match.Value.Replace("236x", "originals"));
                    this.ImagesListView.Items.Add(match.Value.Replace("236x", "originals"));
                }
            }*/
            return (true);
        }

        private void ScanSourceCodeButtonClicked(object sender, RoutedEventArgs e)
        {
            // Check if source code field is empty
           /* if (this.BoardSourceCode.Text.Length == 0)
            {
                MessageBox.Show("Source code field is empty :(.", "Error");
                this.Success = false;
                return;
            }*/

            // Clear previous results
            this.ThumbnailsImagesURLS.Clear();
            this.Thumbnails.Children.Clear();

            // Launch source code analyse
            this.Success = this.AnalyseSourceCode();

            // Add and show results in ListView
            //this.ThumbnailsImagesURLS.ForEach(x => this.ImagesListView.Items.Add(x));

            // Update Status Bar
            // coming soon
        }

        private void ScanButtonClicked(object sender, RoutedEventArgs e)
        {
            // Get source code from URL
            if (this.BoardSource.Text.Length > 0 && this.BoardSource.Text.Contains("pinterest.com"))
            {
                // to do : handle exceptions
                this.BoardSourceCode.Text = this.WebClient_.DownloadString(this.BoardSource.Text);
            }
            else if (this.BoardSource.Text.Length == 0)
            {
                MessageBox.Show("URL field is empty.", "Error");
                return;
            }
            else
            {
                MessageBox.Show("Problem with the URL.", "Error");
                return;
            }

            // Clear previous results
            this.ThumbnailsImagesURLS.Clear();
            this.Thumbnails.Children.Clear();

            // Launch source code analyse
            this.Success = this.AnalyseSourceCode();

            // Show result
            int count = this.ThumbnailsImagesURLS.Count;
            this.ThumbnailsImagesURLS.ForEach(delegate(string url)
            {
                var l = new Image();
                var s = this.WebClient_.OpenRead(url);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = s;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                l.Source = bitmap;
                l.MaxWidth = 200;
                this.Thumbnails.Children.Add(l);
            });

            // Update Status Text
            this.ThumbnailsViewer.ToolTip = String.Format("({0}{1} image{2} found)", (count == 1 ? "only " : null), count, (count > 1 ? "s" : null));   

            // Enable Download button
            this.DownloadButton.IsEnabled = (count > 0 ? true : false);
        }

        private void DownloadFolderButtonClicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult result = this.Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
                this.DownloadFolder.Text = Dialog.SelectedPath; 
        }

        private void DownloadButtonClicked(object sender, RoutedEventArgs e)
        {
            // Check if download folder exist
            if (!Directory.Exists(this.DownloadFolder.Text))
            {
                MessageBox.Show("The download folder doesn't exist.", "Error");
                return;
            }

            // Get originals images
            this.ThumbnailsImagesURLS.ForEach(img => this.OriginalImagesURLS.Add(img.Replace("236x", "originals")));

            // Download all the originals images
            this.OriginalImagesURLS.ForEach(delegate(string fileurl)
            {
                Uri uri = new Uri(fileurl);
                string filename = System.IO.Path.GetFileName(uri.LocalPath);
                // to do : handle exceptions
                this.WebClient_.DownloadFile(fileurl, String.Format("{0}\\{1}", this.DownloadFolder.Text, filename));
            });
        }
        #endregion

        #region Attributes
        private bool Success { get; set; }
        private WebClient WebClient_ { get; set; }
        private bool SaveThumbnails { get; set; }
        private bool CreatedSpecificFolder { get; set; }
        private List<string> OriginalImagesURLS { get; set; }
        private List<string> ThumbnailsImagesURLS { get; set; }
        private System.Windows.Forms.FolderBrowserDialog Dialog { get; set; }
        #endregion
    }
}
