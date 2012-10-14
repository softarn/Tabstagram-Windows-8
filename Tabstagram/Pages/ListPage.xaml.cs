using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Tabstagram
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ListPage : Tabstagram.Common.LayoutAwarePage
    {
        MediaListViewModel mediaList = null;

        public ListPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            mediaList = e.Parameter as MediaListViewModel;
            mediaList.CriticalNetworkErrorNotice += OnErrorNotice;
            pageTitle.Text = "Tabstagram - " + mediaList.category;
            this.DefaultViewModel["Items"] = mediaList;

            base.OnNavigatedTo(e);
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
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided wh     en recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {

        }

        //private void AddLoadMore()
        //{
        //    Media m = new Media();
        //    m.id = "load more";
        //    m.images = new Images();
        //    m.images.thumbnail = new Thumbnail();
        //    m.images.thumbnail.url = "ms-appx:/Assets/HeartIcon.png";
        //    mediaList.ItemsAll.Add(m);
        //}

        private void itemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            Media m = e.ClickedItem as Media;

            //if (m.id.Equals("load more"))
            //{
            //    mediaList.ItemsAll.RemoveAt(mediaList.ItemsAll.Count - 1);
            //    await mediaList.LoadMore();
            //    AddLoadMore();
            //}
            //else
            //{
                this.Frame.Navigate(typeof(ImagePage), m);
            //}
        }
    }
}