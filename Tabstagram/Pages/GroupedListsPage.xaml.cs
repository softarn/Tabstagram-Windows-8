using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tabstagram.Models;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ApplicationSettings;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace Tabstagram
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class GroupedListsPage
    {
        private bool settingsMenuRegistered;
        GroupedListsViewModel _lvm;

        public GroupedListsPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void OnErrorNotice(object sender, NotificationEventArgs nea)
        {
            ErrorDisplayer.DisplayNetworkError(new UICommand("Try again", this.TryAgainCommand));
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
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (_lvm != null) return;

            _lvm = new GroupedListsViewModel();
            _lvm.CriticalNetworkErrorNotice += OnErrorNotice;
            Instagram.AccessToken = UserSettings.AccessToken;
            this.DefaultViewModel["Groups"] = _lvm.ItemGroups;
            LoadingGrid.DataContext = _lvm;
            itemGridView.DataContext = _lvm;
            await _lvm.LoadFromSettings();
            Debug.WriteLine("Loaded state");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!this.settingsMenuRegistered)
            {
                SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;
                this.settingsMenuRegistered = true;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (this.settingsMenuRegistered)
            {
                SettingsPane.GetForCurrentView().CommandsRequested -= onCommandsRequested;
                this.settingsMenuRegistered = false;
            }
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var m = (MediaListViewModel)((Button)sender).DataContext;

            this.Frame.Navigate(typeof(ListPage), m);
        }

        private async void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            var m = (MediaListViewModel)((Button)sender).DataContext;

            var b = (Button)sender;
            int to = 359;
            CompositeTransform rotation = b.RenderTransform as CompositeTransform;
            if (rotation != null)
            {
                to = (int)rotation.Rotation + 359;
            }
            var rotatingStoryboard = new Storyboard();
            var opacityAnimation = new DoubleAnimation
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(500),
                RepeatBehavior = RepeatBehavior.Forever
            };
            rotatingStoryboard.Children.Add(opacityAnimation);

            Storyboard.SetTargetProperty(opacityAnimation, "(UIElement.RenderTransform).(CompositeTransform.Rotation)");
            Storyboard.SetTarget(rotatingStoryboard, b);

            rotatingStoryboard.Begin();
            b.IsEnabled = false;
            m.IsLoaded = false;
            m.CriticalNetworkErrorNotice += OnErrorNotice;
            await m.Refresh();
            m.CriticalNetworkErrorNotice -= OnErrorNotice;
            m.IsLoaded = true;
            b.IsEnabled = true;
            rotatingStoryboard.Pause();
            ButtonPointerExited(sender, null);
        }

        private async void TryAgainCommand(IUICommand command)
        {
            await _lvm.Reset();
        }

        private void ItemClick(object sender, ItemClickEventArgs e)
        {
            var m = e.ClickedItem as Media;

            this.Frame.Navigate(typeof(ImagePage), m);
        }

        private void ButtonPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var b = (Button)sender;
            var storyboard = new Storyboard();

            var opacityAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0.5,
                Duration = TimeSpan.FromSeconds(0.1)
            };
            storyboard.Children.Add(opacityAnimation);

            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
            Storyboard.SetTarget(storyboard, b);

            storyboard.Begin();
        }

        private void ButtonPointerExited(object sender, PointerRoutedEventArgs e)
        {
            Button b = (Button)sender;
            var storyboard = new Storyboard();

            var opacityAnimation = new DoubleAnimation
            {
                From = 0.5,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.1)
            };
            storyboard.Children.Add(opacityAnimation);

            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
            Storyboard.SetTarget(storyboard, b);

            storyboard.Begin();
        }

        void onLogoutCommand(IUICommand command)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["access_token"] = null;
            this.Frame.Navigate(typeof(LoginPage));
        }

        void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            UICommandInvokedHandler logoutHandler = new UICommandInvokedHandler(onLogoutCommand);

            SettingsCommand logoutCommand = new SettingsCommand("LogoutId", "Logout", logoutHandler);
            eventArgs.Request.ApplicationCommands.Add(logoutCommand);
        }
    }
}
