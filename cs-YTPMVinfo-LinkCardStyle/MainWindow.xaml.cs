using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace OGPInformation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void GetOgpInfoButton_Click(object sender, RoutedEventArgs e)
        {
            // 入力されたURLを取得
            string url = urlTextBox.Text;

            // URLの正当性をチェック
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri validatedUrl))
            {
                MessageBox.Show("Invalid URL format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                statuslabel.Content = "Error : [" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] Invalid URL format.";
                return;
            }

            // HttpClientを使用してWebページのHTMLを取得
            var httpClient = new HttpClient();
            string html;
            try
            {
                html = await httpClient.GetStringAsync(validatedUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to retrieve HTML content: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                statuslabel.Content = "Error : [" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] Failed to retrieve HTML content.";
                return;
            }


            // HtmlAgilityPackを使用してHTMLをパース
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            // OGPプロパティを取得する関数
            string GetOgpProperty(string property)
            {
                var metaElement = htmlDocument.DocumentNode.SelectSingleNode($"//meta[@property='{property}']") ??
                                  htmlDocument.DocumentNode.SelectSingleNode($"//meta[@name='{property}']");
                return metaElement?.GetAttributeValue("content", "");
            }

            // OGP情報を取得
            string ogpTitle = GetOgpProperty("og:title");
            string ogpDescription = GetOgpProperty("og:description");
            string ogpImage = GetOgpProperty("og:image");
            string ogpSiteName = GetOgpProperty("og:site_name");
            string ogpUrl = GetOgpProperty("og:url");
            Uri uri = new Uri(ogpUrl);
            string host = uri.Host;

            // OGP情報を表示
            titleLabel.Text = ogpTitle;
            descriptionLabel.Text = ogpDescription;
            imageLabel.Text = ogpImage;
            siteNameLabel.Text = ogpSiteName;
            urlLabel.Text = host;

            statuslabel.Content = "Status : [" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] Got OGP Info.";
        }
        private void GenerateLinkCardButton_Click(object sender, RoutedEventArgs e)
        {
            string originalHTML = @"
<a class='linkcard_a' href='{A-HREF}' target='_blank' rel='noopener noreferrer'>
    <div class='linkcard_main'>
        <div class='linkcard_text'>
        <div>
            <div class='linkcard_title'>{SITE-TITLE}</div>
            <div class='linkcard_Description'>{SITE-DESCRIPRION}</div>
        </div>
        <div class='linkcard_host'>{SITE-HOST}</div>
        </div>
        <div class='linkcard_image'>
        <img class='linkcard_image' src='{IMAGE-SRC}' alt='' loading='lazy' />
        </div>
    </div>
</a>
";
            string replacedText = originalHTML.Replace("{A-HREF}", urlTextBox.Text).Replace("{IMAGE-SRC}", imageLabel.Text).Replace("{SITE-HOST}", urlLabel.Text).Replace("{SITE-TITLE}", titleLabel.Text).Replace("{SITE-DESCRIPRION}", descriptionLabel.Text);
            LinkCard.Text = replacedText;
            statuslabel.Content = "Status : [" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] Generated Link Card..";
        }
    }
}
