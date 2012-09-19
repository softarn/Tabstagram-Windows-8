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
using Windows.UI.Xaml.Navigation;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Tabstagram
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ItemsPage1 : Tabstagram.Common.LayoutAwarePage
    {
        MediaList mediaList = null;

        public ItemsPage1()
        {
            this.InitializeComponent();
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
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            mediaList = App.lvm.GetListFromString(UserSettings.ActiveList);
            this.DefaultViewModel["Items"] = mediaList.ItemsAll;
            if (mediaList.ItemsAll.Count < 50 && !mediaList.category.Equals("Popular"))
                 await mediaList.LoadMore();
            AddLoadMore();
        }

        private void AddLoadMore()
        {
            Media m = new Media();
            m.id = "load more";
            m.images = new Images();
            m.images.thumbnail = new Thumbnail();
            m.images.thumbnail.url = "ms-appx:/Assets/HeartIcon.png";
            mediaList.ItemsAll.Add(m);
        }

        private async void itemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            Media m = e.ClickedItem as Media;

            if (m.id.Equals("load more"))
            {
                mediaList.ItemsAll.RemoveAt(mediaList.ItemsAll.Count - 1);
                await mediaList.LoadMore();
                AddLoadMore();
            }
        }
    }
}