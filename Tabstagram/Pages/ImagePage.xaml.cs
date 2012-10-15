using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Tabstagram.Models;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Tabstagram
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ImagePage
    {
        private ImageViewModel _viewModel;
        private Button MarkedButton;
        private Grid VisibleGrid;
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        public ImagePage()
        {
            this.InitializeComponent();
        }

        private void OnErrorNotice(object sender, NotificationEventArgs nea)
        {
            ErrorDisplayer.DisplayNetworkError(new UICommand("Try again", this.TryAgainCommand));
        }

        private void TryAgainCommand(IUICommand command)
        {
            _viewModel.Reset();
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
            if (e.Parameter is Media)
            {
                Media m = e.Parameter as Media;
                _viewModel = new ImageViewModel(m);
                pageTitle.Text = "Tabstagram - " + _viewModel.CurrentMedia.user.username;
            }
            else if (e.Parameter is User)
            {
                User u = e.Parameter as User;
                _viewModel = new ImageViewModel(u);
                pageTitle.Text = "Tabstagram - " + u.username;
            }

            _viewModel.CriticalNetworkErrorNotice += OnErrorNotice;
            pageRoot.DataContext = _viewModel;

            if (MarkedButton == null || VisibleGrid == null)
            {
                SwitchMarkedButton(CommentsButton);
                SwitchVisibleGrid(CommentsGrid);
            }

            base.OnNavigatedTo(e);
        }

        void FollowersGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("loaded followersgrid");
        }

        private void SwitchMarkedButton(Button b)
        {
            if (MarkedButton != null)
                MarkedButton.IsEnabled = true;

            MarkedButton = b;
            MarkedButton.IsEnabled = false;
        }

        private void SwitchVisibleGrid(Grid g)
        {
            if (VisibleGrid != null)
                VisibleGrid.Visibility = Visibility.Collapsed;

            VisibleGrid = g;
            VisibleGrid.Visibility = Visibility.Visible;
        }

        private async void RightGridInfoSwitchClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Task loaded = null;
            SwitchMarkedButton(button);
            RightGridProgressBar.IsActive = true;
            switch (button.Name)
            {
                case "FollowedByButton": loaded = _viewModel.LoadFollowedBy(); SwitchVisibleGrid(FollowedByGrid); break;
                case "CommentsButton":  SwitchVisibleGrid(CommentsGrid); break;
                case "FollowsButton": loaded = _viewModel.LoadFollows(); SwitchVisibleGrid(FollowsGrid); break;
                case "LikesButton": loaded = _viewModel.LoadLikes(); SwitchVisibleGrid(LikesGrid); break;
            }

            if (loaded != null)
               await loaded;

            RightGridProgressBar.IsActive = false;
        }

        private void RelatedMediaListClick(object sender, ItemClickEventArgs e)
        {
            FullImage.FadeOut();
            Media media = e.ClickedItem as Media;
            _viewModel.LoadNewMedia(media);
        }

        private async void ImageDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await _viewModel.LikeOrUnlike();
        }

        private void NewCommentClick(object sender, RoutedEventArgs e)
        {
            WriteComment.Visibility = WriteComment.Visibility != Visibility.Visible
                                          ? Visibility.Visible
                                          : Visibility.Collapsed;
        }

        private void MakeCommentButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(
                Comment.Text
                );

            _viewModel.Comment(Comment.Text);
            WriteComment.Visibility = Visibility.Collapsed;
        }

        public static Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }

        private async void CommentsListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CommentsList.SelectedItem == null)
                return;

            object o = CommentsList.SelectedItem;
            CommentsList.SelectedItem = null;
            // Create a menu and add commands specifying a callback delegate for each.
            // Since command delegates are unique, no need to specify command Ids.
            var menu = new PopupMenu();

            string userId = await UserSettings.retreiveUserId();
            Comment comment = (Comment)o;

            if (userId == _viewModel.CurrentMedia.user.id || userId == comment.from.id)
            {
                menu.Commands.Add(new UICommand("Delete", command => HandleDelete(comment)));
            }
            
            menu.Commands.Add(new UICommand("Go to user", command => HandleGoToUser(comment.from)));

            ListViewItem lvi = (ListViewItem)CommentsList.ItemContainerGenerator.ContainerFromItem(o);

            await menu.ShowForSelectionAsync(GetElementRect(lvi));
        }

        private async void UserListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView activeList = (ListView)sender;
            if (activeList.SelectedItem == null)
                return;

            object o = activeList.SelectedItem;
            activeList.SelectedItem = null;
            // Create a menu and add commands specifying a callback delegate for each.
            // Since command delegates are unique, no need to specify command Ids.
            var menu = new PopupMenu();

            string userId = await UserSettings.retreiveUserId();

            menu.Commands.Add(new UICommand("Go to user", command => HandleGoToUser((User)o)));
            ListViewItem lvi = (ListViewItem)activeList.ItemContainerGenerator.ContainerFromItem(o);

            await menu.ShowForSelectionAsync(GetElementRect(lvi));
        }

        private async void HandleDelete(Comment comment)
        {
            bool deleteSuccess = await _viewModel.DeleteComment(comment.id);

            if (deleteSuccess != false) return;

            CommentError.Visibility = Visibility.Visible;
            _timer.Tick += (o, e) => { CommentError.Visibility = Visibility.Collapsed; };
            _timer.Tick += (o, e) => _timer.Stop();
            _timer.Interval = new TimeSpan(0, 0, 5);
            _timer.Start();
        }

        private void HandleGoToUser(User user)
        {
            this.Frame.Navigate(typeof(ImagePage), user);
        }

        private void HomeClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GroupedListsPage));
        }
    }
}
