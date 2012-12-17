using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace Tabstagram
{
    class GlobalSettings
    {
        Frame frame { get; set; }

        public GlobalSettings(Frame frame) { this.frame = frame; }

        void onLogoutCommand(IUICommand command)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["access_token"] = null;
            frame.Navigate(typeof(LoginPage));
        }

        async void onPrivacyCommand(IUICommand command)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://tabstagram.com/privacy_policy"));
        }

        public void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs eventArgs)
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
