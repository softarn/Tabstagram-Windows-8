using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Tabstagram
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();           
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void SimulateForceLogoutClick(object sender, RoutedEventArgs args)
        {
            NotifyAboutBeingLoggedOut();
        }

        private async void NotifyAboutBeingLoggedOut()
        {
            // Create the message dialog and set its content
            var messageDialog = new MessageDialog("You have been logged out. This could be due to numerous reasons. Please log in again to use Tabstagram.");

            messageDialog.Commands.Add(new UICommand(
                "Ok",
                new UICommandInvokedHandler(this.CommandInvokedHandler)));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 1;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            // Display message showing the label of the command that was invoked
            Debug.WriteLine("The '" + command.Label + "' command has been selected.");
        }

        private void ResetLoginAndNotify()
        {
            authButton.IsEnabled = true;
            signinRequiredTextBlock.Visibility = Visibility.Visible;
        }

        private void NavigateToLoggedIn()
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void AuthButtonClick(object sender, RoutedEventArgs args)
        {
            signinRequiredTextBlock.Visibility = Visibility.Collapsed;
            try
            {
                authButton.IsEnabled = false;
                String clientId = "692bbb1e60d34b048988fdf205d67696";
                String redirectURI = "tabstagram://oauth";

                String InstagramURL = "https://instagram.com/oauth/authorize/?client_id=" + Uri.EscapeDataString(clientId) + "&redirect_uri=" + Uri.EscapeDataString(redirectURI) + "&response_type=token&display=touch&scope=relationships+likes+comments";

                System.Uri StartUri = new Uri(InstagramURL);
                System.Uri EndUri = new Uri(redirectURI);

                WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(
                                                       WebAuthenticationOptions.None,
                                                       StartUri,
                                                       EndUri);
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    string tokenUrl = WebAuthenticationResult.ResponseData.ToString();
                    Debug.WriteLine(tokenUrl);

                    AuthCallbackParser parser = new AuthCallbackParser(tokenUrl);

                    Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    localSettings.Values["access_token"] = parser.getAccessToken();

                    NavigateToLoggedIn();
                }
                else if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    Debug.WriteLine("HTTP Error returned by AuthenticateAsync() : " + WebAuthenticationResult.ResponseErrorDetail.ToString());
                    ResetLoginAndNotify();
                }
                else
                {
                    Debug.WriteLine("Error returned by AuthenticateAsync() : " + WebAuthenticationResult.ResponseStatus.ToString());
                    ResetLoginAndNotify();
                }
            }
            catch (Exception Error)
            {
                //
                // Bad Parameter, SSL/TLS Errors and Network Unavailable errors are to be handled here.
                //
                Debug.WriteLine(Error.ToString());
                ResetLoginAndNotify();
            }
        }
    }
}
