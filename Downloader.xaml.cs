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
            this.ThumbnailImages = new List<PinterestImage> { };
            this.Dialog = new System.Windows.Forms.FolderBrowserDialog();
            
            // Init control attributes
            this.DownloadFolder.Text= Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            // Init events
            this.WebClient_.OpenReadCompleted += DownloadThumbnailCompleted;
            this.WebClient_.DownloadFileCompleted += DownloadOriginalCompleted;
            this.WebClient_.DownloadProgressChanged += DownloadSourceCodeChanged;
            this.WebClient_.DownloadStringCompleted += DownloadSourceCodeCompleted;
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

            // Declare forbiden images names list
            var forbiden = new List<string> { "avatars", "default", "api", "upload" };

            var itemList = doc.DocumentNode.SelectNodes("//img").ToList();
            foreach (HtmlAgilityPack.HtmlNode x in itemList)
                if (!forbiden.Any(f => x.Attributes["src"].Value.Contains(f)))
                    this.ThumbnailImages.Add(new PinterestImage(x.Attributes["src"].Value));

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

        private void ScanButtonClicked(object sender, RoutedEventArgs e)
        {
            // Get source code from URL
            if (this.BoardSource.Text.Length > 0 && this.BoardSource.Text.Contains("pinterest.com"))
            {
                // Disable scan button
                this.ScanButton.IsEnabled = false;
                this.DownloadButton.IsEnabled = false;

                // Clear previous results
                this.Thumbnails.Children.Clear();
                this.ThumbnailImages.Clear();

                // Download source code
                if (this.WebClient_.IsBusy)
                    this.WebClient_.CancelAsync();
          
                this.WebClient_.DownloadStringAsync(new Uri(this.BoardSource.Text));

                // Hide result and show informations text
                this.ThumbnailsViewer.Visibility = Visibility.Hidden;
                this.InformationsText.Visibility = Visibility.Visible;
                this.InformationsText.Content = "Scan in progress...";
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
        }

        void DownloadSourceCodeCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // Get source code
            this.BoardSourceCode.Text = e.Result;

            // Re-enable scan button
            this.ScanButton.IsEnabled = true;

            // Hide Informations Text
            this.InformationsText.Visibility = Visibility.Hidden;

            // Launch source code analyse
            this.Success = this.AnalyseSourceCode();

            // Download thumbnail images
            this.DownloadThumbnailImages();

            // Show result
            this.ThumbnailsViewer.Visibility = Visibility.Visible;
            int count = this.ThumbnailImages.Count;

            // Update Status Text
            this.ThumbnailsViewer.ToolTip = String.Format("({0}{1} image{2} found)", (count == 1 ? "only " : null), count, (count > 1 ? "s" : null));

            // Enable Download button
            this.DownloadButton.IsEnabled = (count > 0 ? true : false);
        }

        void DownloadSourceCodeChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.InformationsText.Content = String.Format("Scan in progress ({0} / 100 %)", e.ProgressPercentage);
        }

        private void DownloadThumbnailImages()
        {
            if (this.ThumbnailImages.Count > 0)
            {
                PinterestImage img = this.ThumbnailImages.First();
                this.WebClient_.OpenReadAsync(new Uri(img.thumbnailurl));
            }
        }

        void DownloadThumbnailCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            // Confirm thumbnail download
            var thumbnail = this.ThumbnailImages.Find(x => x.thumbnaildownlaoded.Equals(false));
            if (thumbnail != null)
            {
                thumbnail.thumbnaildownlaoded = true;
                if (!this.WebClient_.IsBusy && !e.Cancelled)
                {
                    thumbnail.LoadImageFromStream(e.Result);
                    this.Thumbnails.Children.Add(thumbnail.image);
                }
            }

            int count = this.ThumbnailImages.Count;
            int done = this.ThumbnailImages.Where(x => x.thumbnaildownlaoded.Equals(true)).ToList().Count;

            // Download the next thumbnails
            thumbnail = this.ThumbnailImages.Find(x => x.thumbnaildownlaoded.Equals(false));
            if (thumbnail != null && !WebClient_.IsBusy)
                this.WebClient_.OpenReadAsync(new Uri(thumbnail.thumbnailurl));
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

            // Check if download is alreday in progress, cancel it if it does
            if (this.WebClient_.IsBusy)
            {
                this.WebClient_.CancelAsync();
                this.HideDownloadProgressionStats();
                return;
            }

            // Show download stats
            this.DownloadProgressionBar.Maximum = this.ThumbnailImages.Count;
            this.DownloadProgressionBar.Value = 0;
            this.ShowDownloadProgressionStats();

            // Download all the originals images
            this.ThumbnailImages.ForEach(x => x.originaldownlaoded = false);
            this.DownloadOriginalImages();
        }

        private void DownloadOriginalImages()
        {
            if (this.ThumbnailImages.Count > 0)
            {
                PinterestImage img = this.ThumbnailImages.First();
                this.WebClient_.DownloadFileAsync(new Uri(img.originalurl), String.Format("{0}\\{1}", this.DownloadFolder.Text, img.filename));
            }
        }

        void DownloadOriginalCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            // Confirm original imge download
            var img = this.ThumbnailImages.Find(x => x.originaldownlaoded.Equals(false));
            if (img != null)
                img.originaldownlaoded = true;

            // Download the next original image
            img = this.ThumbnailImages.Find(x => x.originaldownlaoded.Equals(false));
            if (img != null)
                this.WebClient_.DownloadFileAsync(new Uri(img.originalurl), String.Format("{0}\\{1}", this.DownloadFolder.Text, img.filename));

            int count = this.ThumbnailImages.Count;
            int done = this.ThumbnailImages.Where(x => x.originaldownlaoded.Equals(true)).ToList().Count;

            // Update Download Progression Stats
            this.DownloadProgressionText.Content = String.Format("({0}{1} / {2} image{3})", (count == 1 ? "only " : null), done, count, (count > 1 ? "s" : null));
            this.DownloadProgressionBar.Value = done;

            // Hide Download Progression Stats
            if (done == count)
                this.HideDownloadProgressionStats();
        }

        private void ShowDownloadProgressionStats()
        {
            var i = new Image();

            // Hide "change download folder" feature
            this.DownloadFolder.Visibility = Visibility.Hidden;
            this.ChangeDownloadFolder.Visibility = Visibility.Hidden;

            // Change download button text
            this.DownloadButton.Content = "Cancel";
            this.DownloadButton.ToolTip = "Cancel current download.";

            // Show download progression
            this.DownloadProgressionBar.Visibility = Visibility.Visible;
            this.DownloadProgressionText.Visibility = Visibility.Visible;

            // Disable scan button
            this.ScanButton.IsEnabled = false;
        }

        private void HideDownloadProgressionStats()
        {
            // Show "change download folder" feature
            this.DownloadFolder.Visibility = Visibility.Visible;
            this.ChangeDownloadFolder.Visibility = Visibility.Visible;

            // Change download button text
            this.DownloadButton.Content = "Download!";
            this.DownloadButton.ToolTip = "Download originals images here.";

            // Hide download progression
            this.DownloadProgressionBar.Visibility = Visibility.Hidden;
            this.DownloadProgressionText.Visibility = Visibility.Hidden;

            // Enable scan button
            this.ScanButton.IsEnabled = true;
        }
        #endregion

        #region Attributes
        private bool Success { get; set; }
        private WebClient WebClient_ { get; set; }
        private bool SaveThumbnails { get; set; }
        private bool CreatedSpecificFolder { get; set; }
        private List<PinterestImage> ThumbnailImages { get; set; }
        private System.Windows.Forms.FolderBrowserDialog Dialog { get; set; }
        #endregion
    }
}
