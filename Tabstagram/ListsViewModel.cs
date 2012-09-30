using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tabstagram.Models;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Tabstagram
{
    public class InfiniteSequence : ObservableCollection<Media>, ISupportIncrementalLoading
    {

        public InfiniteSequence()
        {
        }

        public bool HasMoreItems
        {
            get { return true; }
        }

        //public Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        //{
        //    CoreDispatcher dispatcher = Window.Current.Dispatcher;

        //    return Task.Run<LoadMoreItemsResult>(
        //        () =>
        //        {
        //            int[] numbers = Enumerable.Range(this.LastOrDefault(), 100).ToArray();

        //            dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        //                 () =>
        //                 {
        //                     foreach (int item in numbers)
        //                         if (this.Selector(item))
        //                             this.Add(item);
        //                 });

        //            return new LoadMoreItemsResult() { Count = 100 };
        //        }).AsAsyncOperation<LoadMoreItemsResult>();
        //}


        public Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            CoreDispatcher dispatcher = Window.Current.Dispatcher;

            return Task.Run<LoadMoreItemsResult>(
                () =>
                {
                    dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            Add(this.Last());
                        });
                    return new LoadMoreItemsResult() { Count = 1 };
                }).AsAsyncOperation<LoadMoreItemsResult>();
        }
    }

    public abstract class MediaList : ObservableCollection<Media>, ISupportIncrementalLoading
    {
        public static List<DispatcherTimer> timers = new List<DispatcherTimer>();
        protected Pagination pagination;
        private ObservableCollection<Media> _subCollection;
        public ObservableCollection<Media> SubCollection
        {
            get
            {
                if (_subCollection == null || _subCollection.Count == 0)
                {
                    _subCollection = new ObservableCollection<Media>();
                    IEnumerable<Media> collection = GetSubCollection();
                    foreach (Media m in collection)
                    {
                        _subCollection.Add(m);
                    }
                }

                return _subCollection;
            }
        }
        public string category { get; set; }
        public bool HasMoreItems
        {
            get
            {
                return pagination.next_url != null;
            }
        }

        public abstract string GetName();

        protected virtual void Init()
        {
            this.category = GetName();
        }

        private IEnumerable<Media> GetSubCollection()
        {
            Random rand = new Random();
            int takeInt = this.Count;
            if(takeInt > 14)
            {
                takeInt = rand.Next(15, takeInt);
            }
            return this.Take(takeInt);
        }

        public static MediaList GetClassFromString(string listString)
        {
            if (listString[0] == '#')
            {
                return new HashTag(listString.Substring(1, listString.Length - 1));
            }

            if (listString.Equals("popular"))
                return new Popular();
            else if (listString.Equals("feed"))
                return new Feed();

            throw new System.ArgumentException("listString must be #hashtagname, popular or feed");
        }

        public virtual async Task<bool> LoadMore()
        {
            if (HasMoreItems == false) return false;

            MultipleMedia mm = await Instagram.LoadFromCustomUrl(pagination.next_url);
            AddAll(mm.data);
            pagination = mm.pagination;
            return true;
        }

        protected void AddAll(List<Media> elements)
        {
            foreach (Media element in elements)
            {
                Add(element);
            }
        }

        public override bool Equals(object obj)
        {
            CoreDispatcher dispatcher = Window.Current.Dispatcher;

            return Task.Run<LoadMoreItemsResult>(
                () =>
                {
                    dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            LoadMore();
                        });
                    return new LoadMoreItemsResult() { Count = 1 };
                }).AsAsyncOperation<LoadMoreItemsResult>();
        }


        public Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            throw new NotImplementedException();
        }
    }

    public class Feed : MediaList
    {

        public Feed() { Init(); }

        protected async override void Init()
        {
            base.Init();
            MultipleMedia mm = await Instagram.LoadFeed();
            pagination = mm.pagination;
            AddAll(mm.data);
        }

        public override string GetName()
        {
            return "Feed";
        }
    }

    public class UserMedia : MediaList
    {
        private User User;

        public UserMedia(User user) { User = user; Init(); }

        protected override async void Init()
        {
            base.Init();
            MultipleMedia mm = await Instagram.LoadUserMedia(this.User);
            pagination = mm.pagination;
            AddAll(mm.data);
        }

        public override string GetName()
        {
            return "Feed";
        }
    }

    public class Popular : MediaList
    {
        public Popular() { Init(); }

        protected override async void Init()
        {
            base.Init();
            MultipleMedia mm = await Instagram.LoadPopular();
            pagination = mm.pagination;
            AddAll(mm.data);
        }
        public override string GetName()
        {
            return "Popular";
        }

        public override async Task<bool> LoadMore()
        {
            MultipleMedia mm = await Instagram.LoadPopular();
            AddAll(mm.data);
            return true;
        }
    }

    public class HashTag : MediaList
    {
        public string Tag { get; set; }

        public HashTag(string tag)
        {
            this.Tag = tag;
            Init();
        }

        protected override async void Init()
        {
            MultipleMedia mm = await Instagram.LoadHashtag(Tag);
            pagination = mm.pagination;
            AddAll(mm.data);
        }

        public override string GetName()
        {
            return "#" + this.Tag;
        }

    }

    class ListsViewModel
    {
        public ObservableCollection<MediaList> ItemGroups = new ObservableCollection<MediaList>();

        public void LoadFromSettings()
        {
            List<string> list = UserSettings.MediaStringsList;

            foreach (string str in list)
            {
                MediaList ml = MediaList.GetClassFromString(str);
                if (AddToItemsGroup(ml))
                    ml.LoadList();
            }
        }

        public bool AddToItemsGroup(MediaList ml)
        {
            if (ItemGroups.Contains(ml))
                return false;

            ItemGroups.Add(ml);
            return true;
        }

        public MediaList GetListFromString(string listName)
        {
            foreach (MediaList g in ItemGroups)
            {
                if (g.category.ToLower().Equals(listName.ToLower()))
                    return g;
            }

            throw new System.ArgumentException("listName must be the name of a Group category");
        }
    }
}