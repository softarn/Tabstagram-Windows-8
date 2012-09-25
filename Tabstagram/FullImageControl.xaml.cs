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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Tabstagram
{
    public sealed partial class FullImageControl : UserControl
    {

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register
           (
                "Size",
                typeof(int),
                typeof(ThumbnailControl),
                new PropertyMetadata(150)
           );

        public int Size
        {
            get { return (int)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); MainGrid.Width = (int)value; MainGrid.Height = (int)value; }
        }

        public FullImageControl()
        {
            this.InitializeComponent();
        }

        private void fullImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            MainGrid.Opacity = 0;
            MainGrid.Visibility = Visibility.Visible;

            var storyboard = new Storyboard();

            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.8),
            };
            storyboard.Children.Add(opacityAnimation);

            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
            Storyboard.SetTarget(storyboard, MainGrid);

            storyboard.Begin();
        }
    }
}
