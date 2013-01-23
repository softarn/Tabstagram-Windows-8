using System;
using System.Collections.Generic;
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
    public sealed partial class SummerizedGroupControl : UserControl
    {

        public SummerizedGroupControl()
        {
            this.InitializeComponent();

            //MediaListViewModel mlvm = (MediaListViewModel)DataContext;

            //BitmapImage i1 = new BitmapImage();
            //i1.UriSource = new Uri(mlvm.SubCollection.ElementAt(0).images.thumbnail.url);

            //image1.Source = i1;
        }

        private void RightButtonClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
