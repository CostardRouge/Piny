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

namespace Piny
{
    /// <summary>
    /// Interaction logic for Downloader.xaml
    /// </summary>
    public partial class Downloader : Window
    {
        public Downloader()
        {
            // Default call
            InitializeComponent();

            // Init attributes
            this.Images = new List<string> { };
            this.DownloadFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }

        private void ScanSourceCodeButtonClicked(object sender, RoutedEventArgs e)
        {
            // Scan URL if source code filed is empty
            if (this.BoardSourceCode.Text.Length == 0)
            {
                MessageBox.Show("BoardSourceCode empty.");
                this.ScanURLButtonClicked(null, null);
            }

            // Clear previous results
            this.Images.Clear();
            this.ImagesListView.Items.Clear();

            // Find URls in page source code with regulary expression
            Regex regx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);
            MatchCollection matches = regx.Matches(this.BoardSourceCode.Text);

            foreach (Match match in matches)
            {
                if (!exist(this.Images, match.Value) && match.Value.EndsWith(".jpg"))
                {
                    this.Images.Add(match.Value.Replace("236x", "originals"));
                    this.ImagesListView.Items.Add(match.Value.Replace("236x", "originals"));
                }
            }

            /*
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(this.BoardSourceCode.Text);

            var itemList = doc.DocumentNode.SelectNodes("//img").ToList();

            foreach (HtmlAgilityPack.HtmlNode x in itemList)
            {
               
                    this.Images.Add(x.Attributes["src"].Value);
                    this.ImagesListView.Items.Add(x.Attributes["src"].Value);
                
            }*/

           // Debug : Add total image count 
            this.ImagesListView.Items.Add(this.Images.Count + " image(s)");

        }

        // Temporary function, I know its crazy, LINQ expressions exist...
        public bool exist(List<string> l, string e)
        {
            foreach (string s in l)
            {
                if (e == s)
                    return true;
            }
            return false;
        }

        private void ScanURLButtonClicked(object sender, RoutedEventArgs e)
        {
            this.BoardSourceCode.Text = new System.Net.WebClient().DownloadString(this.BoardURL.Text);
            this.ScanSourceCodeButtonClicked(null, null);
        }

        private void DownloadFolderButtonClicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
                this.DownloadFolder.Text = dialog.SelectedPath; 
        }

        private void DownloadButtonClicked(object sender, RoutedEventArgs e)
        {
            WebClient webClient = new WebClient();

            // Scann if the user didnot
            if (this.Images.Count == 0)
                this.ScanSourceCodeButtonClicked(null, null);

            // Check if download folder exist
            if (!Directory.Exists(this.DownloadFolder.Text))
            {
                MessageBox.Show("DownloadFolder err.");
                return;
            }

            foreach (string fileurl in this.Images)
            {
                Uri uri = new Uri(fileurl);
                string filename = System.IO.Path.GetFileName(uri.LocalPath);
                webClient.DownloadFile(fileurl, String.Format("{0}\\{1}", this.DownloadFolder.Text, filename));
            }
        }

        private List<string> Images { get; set; }
    }
}
