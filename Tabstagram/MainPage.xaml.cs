using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Tabstagram
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            tokenTextBlock.Text = "Logged on! \n" + localSettings.Values["access_token"].ToString();

            Instagram.access_token = localSettings.Values["access_token"].ToString();   

            List<Media> feed = await Instagram.Feed();
            Debug.WriteLine("OK WE GOT THE FEED WITH SIZE:" + feed.Count());

            BitmapImage bi = new BitmapImage();
            bi.UriSource = new Uri(feed.First().images.standard_resolution.url);
            ThumbnailView.DataContext = feed.First();
            Debug.WriteLine(feed.First().link);

            testImage.ImageOpened += ImageOpened;

        }

        void ImageOpened(object sender, RoutedEventArgs e)
        {

        }

        private void LogoutButtonClick(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["access_token"] = null;

            this.Frame.Navigate(typeof(LoginPage));
        }
    }
}
