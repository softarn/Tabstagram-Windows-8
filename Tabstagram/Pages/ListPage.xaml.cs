using System;
using System.Collections.Generic;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Tabstagram
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ListPage : Tabstagram.Common.LayoutAwarePage
    {
        MediaListViewModel mediaList = null;
        private bool settingsMenuRegistered;

        public ListPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            mediaList = e.Parameter as MediaListViewModel;
            mediaList.CriticalNetworkErrorNotice += OnErrorNotice;
            pageTitle.Text = "Tabstagram - " + mediaList.category;
            this.DefaultViewModel["Items"] = mediaList;

            if (!this.settingsMenuRegistered)
            {
                SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;
                this.settingsMenuRegistered = true;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (mediaList != null)
                mediaList.CriticalNetworkErrorNotice -= OnErrorNotice;

            if (this.settingsMenuRegistered)
            {
                SettingsPane.GetForCurrentView().CommandsRequested -= onCommandsRequested;
                this.settingsMenuRegistered = false;
            }

            base.OnNavigatedFrom(e);
        }

        private void OnErrorNotice(object sender, NotificationEventArgs nea)
        {
            ErrorDisplayer.DisplayNetworkError(new UICommand("Try again", this.TryAgainCommand));
        }

        private async void TryAgainCommand(IUICommand command)
        {
            mediaList.Reset();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided wh     en recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {

        }

        private void itemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            Media m = e.ClickedItem as Media;

            this.Frame.Navigate(typeof(ImagePage), m);
        }

        void onLogoutCommand(IUICommand command)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["access_token"] = null;
            this.Frame.Navigate(typeof(LoginPage));
        }

        async void onPrivacyCommand(IUICommand command)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://tabstagram.com/privacy_policy"));
        }

        void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            UICommandInvokedHandler logoutHandler = new UICommandInvokedHandler(onLogoutCommand);
            UICommandInvokedHandler privacyHandler = new UICommandInvokedHandler(onPrivacyCommand);

            SettingsCommand logoutCommand = new SettingsCommand("LogoutId", "Logout", logoutHandler);
            SettingsCommand privacyCommand = new SettingsCommand("PrivacyId", "Privacy policy", privacyHandler);

            eventArgs.Request.ApplicationCommands.Add(logoutCommand);
            eventArgs.Request.ApplicationCommands.Add(privacyCommand);
        }
    }
}