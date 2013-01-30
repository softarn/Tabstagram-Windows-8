using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Tabstagram.Helpers
{
    class RatingDisplayer
    {

        public static async void AskForRating()
        {
            // Create the message dialog and set its content
            var messageDialog = new MessageDialog("Thank you for purchasing this app!\n\nIt is thanks to people like you we can develop apps that you will love to use. Please spread your thoughts on our app by giving it a rating at the Store.");

            UICommand ok = new UICommand("Sure", RatingDisplayer.GoToMarket);
            UICommand no = new UICommand("No thanks");

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            messageDialog.Commands.Add(ok);
            messageDialog.Commands.Add(no);

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        private static async void GoToMarket(IUICommand command)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=61816Tabstagram.Tabstagram_xsygjktbm46gj"));
        }
    }
}
