using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Tabstagram.Models;
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

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace Tabstagram
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class GroupedListsPage : Tabstagram.Common.LayoutAwarePage
    {
        ListsViewModel lvm = null;

        public GroupedListsPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
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
            if (lvm == null)
            {
                lvm = new ListsViewModel();
                Instagram.AccessToken = UserSettings.AccessToken;
                this.DefaultViewModel["Groups"] = lvm.ItemGroups;
                lvm.LoadFromSettings();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MediaList m = (MediaList)((Button)sender).DataContext;

            this.Frame.Navigate(typeof(ListPage), m);
        }

        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            MediaList m = (MediaList)((Button)sender).DataContext;

            m.Refresh();
        }

        private void Item_Click(object sender, ItemClickEventArgs e)
        {
            Media m = e.ClickedItem as Media;

            this.Frame.Navigate(typeof(ImagePage), m);
        }

        private void thumbnail_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image MainGrid = (Image)sender;
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
