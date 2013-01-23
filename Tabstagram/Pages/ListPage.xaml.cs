using System;
using System.Collections.Generic;
using Tabstagram.Helpers;
using Tabstagram.Models;
using Windows.Data.Xml.Dom;
using Windows.UI.ApplicationSettings;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Tabstagram
{
    /// <summary>
    /// A page that displays a collection of item previews. In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ListPage : Tabstagram.Common.LayoutAwarePage
    {
        MediaListViewModel mediaList = null;
        private bool settingsMenuRegistered;
        private GlobalSettings globalSettings;
        private Media selectedItem;
        private string lastItemId;

        public ListPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            globalSettings = new GlobalSettings(this.Frame);

            mediaList = e.Parameter as MediaListViewModel;
            mediaList.CriticalNetworkErrorNotice += OnErrorNotice;
            pageTitle.Text = "Tabstagram - " + mediaList.category;
            this.DefaultViewModel["Items"] = mediaList;

            if (!this.settingsMenuRegistered)
            {
                SettingsPane.GetForCurrentView().CommandsRequested += globalSettings.onCommandsRequested;
                this.settingsMenuRegistered = true;
            }

            this.Loaded += (sender, args) =>
            {
                if (lastItemId != "")
                {
                    itemGridView.ScrollIntoView(mediaList.Find(lastItemId));
                }
            };
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (mediaList != null)
                mediaList.CriticalNetworkErrorNotice -= OnErrorNotice;

            if (this.settingsMenuRegistered)
            {
                SettingsPane.GetForCurrentView().CommandsRequested -= globalSettings.onCommandsRequested;
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
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            base.LoadState(navigationParameter, pageState);

            if (pageState != null)
            {
                lastItemId = (string)pageState["lastMediaId"];
            }
        }

        protected override void SaveState(Dictionary<string, object> pageState)
        {
            base.SaveState(pageState);

            if(selectedItem != null)
                pageState["lastMediaId"] = selectedItem.id;
        }

        private void itemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {

        }

        private void fullImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            StackPanel sp = FindAncestor<StackPanel>((Image)sender);
            sp.Opacity = 0;
            sp.Visibility = Visibility.Visible;

            var storyboard = new Storyboard();

            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5),
            };
            storyboard.Children.Add(opacityAnimation);

            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
            Storyboard.SetTarget(storyboard, sp);

            storyboard.Begin();
        }

        public static T FindAncestor<T>(DependencyObject dependencyObject)
        where T : class
        {
            DependencyObject target = dependencyObject;
            do
            {
                target = VisualTreeHelper.GetParent(target);
            }
            while (target != null && !(target is T));
            return target as T;
        }

        private void ImageClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Media m = ((Button)sender).DataContext as Media;
            selectedItem = m;

            this.Frame.Navigate(typeof(ImagePage), m);
        }

        private void AddNewMediaListClick(object sender, RoutedEventArgs e)
        {
            bool added = FavouriteMediaListHelper.AddMediaString(mediaList.GetName());

            if (added == false) return;

            ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode("Added " + mediaList.GetName() + " to start screen"));

            XmlNodeList toastImageAttributes = toastXml.GetElementsByTagName("image");
            if(mediaList.IsLoaded)
                ((XmlElement)toastImageAttributes[0]).SetAttribute("src", mediaList[0].images.low_resolution.url);

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "short");

            ((XmlElement)toastNode).SetAttribute("launch", "{\"type\":\"toast\"}");

            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}