using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Tabstagram
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ImagePage : Tabstagram.Common.LayoutAwarePage
    {
        ImageViewModel viewModel;

        public ImagePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
             
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Media m = e.Parameter as Media;
            viewModel = new ImageViewModel(m);
            pageRoot.DataContext = viewModel;
            base.OnNavigatedTo(e);  
        }

        private void RelatedMediaList_Click(object sender, ItemClickEventArgs e)
        {
            FullImage.FadeOut();
            Media media = e.ClickedItem as Media;
            viewModel.LoadNewMedia(media);
        }

        private void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            viewModel.LikeOrUnlike();
        }

        private void NewCommentClick(object sender, RoutedEventArgs e)
        {
            WriteComment.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void MakeCommentButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(
            Comment.Text
            );

            WriteComment.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
