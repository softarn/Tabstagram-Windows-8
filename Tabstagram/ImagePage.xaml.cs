using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
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

            viewModel.Comment(Comment.Text);
            WriteComment.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void DeleteCommentClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(((Comment)CommentsList.SelectedItem).text);
        }

        private void CommentsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("Go to user profile");
        }

        private void CommentControl_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            
            // We don't want to obscure content, so pass in a rectangle representing the sender of the context menu event.
            // We registered command callbacks; no need to handle the menu completion event
            //OutputTextBlock.Text = "Context menu shown";
            //var chosenCommand = await menu.ShowForSelectionAsync(GetElementRect((FrameworkElement)sender));
            //if (chosenCommand == null) // The command is null if no command was invoked.
            //{
            //    OutputTextBlock.Text = "Context menu dismissed";
            //}
        }

        public static Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }
        
        private async void CommentsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CommentsList.SelectedItem == null)
                return;

            object o = CommentsList.SelectedItem;
            CommentsList.SelectedItem = null;
            // Create a menu and add commands specifying a callback delegate for each.
            // Since command delegates are unique, no need to specify command Ids.
            var menu = new PopupMenu();
            menu.Commands.Add(new UICommand("Delete", (command) =>
            {
                HandleDelete((Comment)o);
            }));

            ListViewItem lvi = (ListViewItem)CommentsList.ItemContainerGenerator.ContainerFromItem(o);

            var chosenCommand = await menu.ShowForSelectionAsync(GetElementRect((FrameworkElement)lvi));
        }

        private async void HandleDelete(Comment comment)
        {
            bool deleteSuccess = await viewModel.DeleteComment(comment.id);
            if (deleteSuccess == false)
            {
                TopAppBar.IsOpen = true;
            }
        }
    }
}
