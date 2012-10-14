using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Tabstagram
{
    public class ErrorDisplayer
    {
        public static async Task DisplayNetworkError(UICommand button1, UICommand button2 = null, string message = "Could not retrieve images. Please check your internet connection")
        {
            // Create the message dialog and set its content
            var messageDialog = new MessageDialog(message);

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            messageDialog.Commands.Add(button1);

            if(button2 != null)
                messageDialog.Commands.Add(button2);

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        private void DoNothing(IUICommand command) { }
    }
}
