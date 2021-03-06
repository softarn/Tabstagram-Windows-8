﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tabstagram.Models;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ApplicationSettings;
using Windows.ApplicationModel.Search;
using Tabstagram.Data;
using Windows.ApplicationModel.Store;
using Tabstagram.Helpers;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace Tabstagram
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class GroupedListsPage
    {
        private bool settingsMenuRegistered;
        GroupedListsViewModel _lvm;
        GlobalSettings globalSettings;

        public GroupedListsPage()
        {
            this.InitializeComponent();
            
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private async void OnErrorNotice(object sender, NotificationEventArgs nea)
        {
            await ErrorDisplayer.DisplayNetworkError(new UICommand("Try again", this.TryAgainCommand));
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
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            Instagram.AccessToken = UserSettings.AccessToken;

            if (_lvm != null) return;

            _lvm = new GroupedListsViewModel();
            _lvm.CriticalNetworkErrorNotice += OnErrorNotice;
            this.DefaultViewModel["Groups"] = _lvm.ItemGroups;
            LoadingGrid.DataContext = _lvm;
            itemGridView.DataContext = _lvm;
            UserSettings.MediaListChanged = false;
            await _lvm.LoadFromSettings();
            Debug.WriteLine("Loaded state");

            LicenseInformation licenseInformation;
            // Initialize the license info for use in the app that is uploaded to the Store.
            // uncomment for release
               licenseInformation = CurrentApp.LicenseInformation;

            // Initialize the license info for testing.
            // comment the next line for release
            //licenseInformation = CurrentAppSimulator.LicenseInformation;

            if (licenseInformation.IsActive && UserSettings.HasAskedForRating == false)
            {
                if (licenseInformation.IsTrial == false)
                {
                    RatingDisplayer.AskForRating();
                    UserSettings.HasAskedForRating = true;
                }
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (UserSettings.MediaListChanged)
            {
                await _lvm.Reset();
                UserSettings.MediaListChanged = false;
            }

            globalSettings = new GlobalSettings(this.Frame);
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = true;

            //Set the same itemsource for zoomedIn and zoomedOut
            var collectionGroups = groupedItemsViewSource.View.CollectionGroups;
            ((ListViewBase)this.Zoom.ZoomedOutView).ItemsSource = collectionGroups;

            if (!this.settingsMenuRegistered)
            {
                SettingsPane.GetForCurrentView().CommandsRequested += globalSettings.onCommandsRequested;
                this.settingsMenuRegistered = true;
            }

            if (UserSettings.FollowTabstagram)
            {
                try
                {
                    await Instagram.Follow("229801072");
                    UserSettings.FollowTabstagram = false;
                }
                catch (Exception)
                {
                    Debug.WriteLine("Error when trying to follow Tabstagram from checkbox");
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = false;

            if (this.settingsMenuRegistered)
            {
                SettingsPane.GetForCurrentView().CommandsRequested -= globalSettings.onCommandsRequested;
                this.settingsMenuRegistered = false;
            }
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var m = (MediaListViewModel)((Button)sender).DataContext;

            this.Frame.Navigate(typeof(ListPage), m);
        }

        private async void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            ButtonPointerExited(sender, null);

            var m = (MediaListViewModel)((Button)sender).DataContext;

            var b = (Button)sender;
            int to = 359;
            CompositeTransform rotation = b.RenderTransform as CompositeTransform;
            if (rotation != null)
            {
                to = (int)rotation.Rotation + 359;
            }
            var rotatingStoryboard = new Storyboard();
            var opacityAnimation = new DoubleAnimation
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(500),
                RepeatBehavior = RepeatBehavior.Forever
            };
            rotatingStoryboard.Children.Add(opacityAnimation);

            Storyboard.SetTargetProperty(opacityAnimation, "(UIElement.RenderTransform).(CompositeTransform.Rotation)");
            Storyboard.SetTarget(rotatingStoryboard, b);

            rotatingStoryboard.Begin();
            b.IsEnabled = false;
            m.IsLoaded = false;
            m.CriticalNetworkErrorNotice += OnErrorNotice;
            await m.Refresh();
            m.CriticalNetworkErrorNotice -= OnErrorNotice;
            m.IsLoaded = true;
            b.IsEnabled = true;
            rotatingStoryboard.Pause();
            ButtonPointerExited(sender, null);
        }

        private async void TryAgainCommand(IUICommand command)
        {
            await _lvm.Reset();
        }

        private void ItemClick(object sender, ItemClickEventArgs e)
        {
            var m = e.ClickedItem as Media;

            this.Frame.Navigate(typeof(ImagePage), m);
        }

        private void GroupClick(object sender, ItemClickEventArgs e)
        {
            var m = e.ClickedItem as Media;
        }

        private void MoveRightButtonClick(object sender, RoutedEventArgs e)
        {
            var m = (MediaListViewModel)((Button)sender).DataContext;
            _lvm.MoveMediaList(m, 1);
        }

        private void MoveLeftButtonClick(object sender, RoutedEventArgs e)
        {
            var m = (MediaListViewModel)((Button)sender).DataContext;
            _lvm.MoveMediaList(m, -1);
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var m = (MediaListViewModel)((Button)sender).DataContext;
            _lvm.DeleteMediaList(m);
        }

        private void EditListAppBarClick(object sender, RoutedEventArgs e)
        {
            if(this.Zoom.IsZoomedInViewActive)
                this.Zoom.ToggleActiveView();
        }

        private void AddListAppBarClick(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.Search.SearchPane searchPane = SearchPane.GetForCurrentView();
            searchPane.Show();
        }

        private void ButtonPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var b = (Button)sender;
            var storyboard = new Storyboard();

            var opacityAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0.4,
                Duration = TimeSpan.FromSeconds(0.1)
            };
            storyboard.Children.Add(opacityAnimation);

            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
            Storyboard.SetTarget(storyboard, b);

            storyboard.Begin();
        }

        private void ButtonPointerExited(object sender, PointerRoutedEventArgs e)
        {
            Button b = (Button)sender;
            var storyboard = new Storyboard();

            var opacityAnimation = new DoubleAnimation
            {
                From = 0.4,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.1)
            };
            storyboard.Children.Add(opacityAnimation);

            Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
            Storyboard.SetTarget(storyboard, b);

            storyboard.Begin();
        }
    }
}
