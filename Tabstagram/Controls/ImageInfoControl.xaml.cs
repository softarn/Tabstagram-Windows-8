using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Tabstagram
{
    public sealed partial class ImageInfoControl : UserControl
    {

        public ImageInfoControl()
        {
            this.InitializeComponent();
        }

        private async void LikeButtonClick(object sender, RoutedEventArgs e)
        {
            LikeButton.IsEnabled = false;
            Media m = (Media)DataContext;
            if (m.user_has_liked)
            {
                m.Unlike();
                await Instagram.Unlike(m.id);
            }
            else
            {
                m.Like();
                await Instagram.Like(m.id);
            }
            LikeButton.IsEnabled = true;
        }
    }
}
