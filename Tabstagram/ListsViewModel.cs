using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tabstagram.Models;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Tabstagram
{
    public abstract class MediaList : ObservableCollection<Media>, ISupportIncrementalLoading, INotifyPropertyChanged
    {
        public string category { get; set; }
        protected override event PropertyChangedEventHandler PropertyChanged;
        public static List<DispatcherTimer> timers = new List<DispatcherTimer>();
        protected Pagination pagination;

        private ObservableCollection<Media> _subCollection;
        public ObservableCollection<Media> SubCollection
        {
            get
            {
                if (_subCollection == null)
                    _subCollection = new ObservableCollection<Media>();
                return _subCollection;
            }
        }

        private bool _isLoaded;
        public bool IsLoaded 
        {
            get
            {
                return _isLoaded;
            }

            set
            {
                _isLoaded = value;
                OnPropertyChanged("IsLoaded");
            }
        }

        public bool HasMoreItems
        {
            get
            {
                return CanLoadMoreItems();
            }
        }

        protected virtual bool CanLoadMoreItems()
        {
            if (pagination == null)
                return false;

            return pagination.next_url != null;
        }

        public abstract string GetName();
        public abstract Task<List<Media>> FetchNewMedia();

        protected virtual void Init()
        {
            Debug.WriteLine(this.GetType());

            this.category = GetName();
            IsLoaded = true;
            MarkImportantMedia();
            PopulateSubCollection();
        }

        public async void Refresh()
        {
            List<Media> newMedia = await FetchNewMedia();

            if (newMedia.Count > 0)
            {
                AddAll(newMedia);
                MarkImportantMedia();
                UpdateSubCollection(newMedia);
            }
        }

        private void UpdateSubCollection(IEnumerable<Media> newMedia)
        {
            foreach (Media m in newMedia)
            {
                SubCollection.Remove(SubCollection.Last());
                SubCollection.Insert(0, m);
            }
        }

        private void PopulateSubCollection()
        {
            Random rand = new Random();
            int takeInt = this.Count;
            if (takeInt > 14)
            {
                int max = takeInt > 20 ? 20 : takeInt;
                takeInt = rand.Next(15, max);
            }
            foreach (Media m in this.Take(takeInt))
            {
                SubCollection.Add(m);
            }
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

        public virtual async Task<MultipleMedia> FetchMoreMedia()
        {
            MultipleMedia mm = await Instagram.LoadFromCustomUrl(pagination.next_url);
            pagination = mm.pagination;
            return mm;
        }

        public virtual void MarkImportantMedia()
        {
            int count = 0;
            Media mostImportant = this.ElementAt(0);
            foreach(Media m in this)
            {
                m.IsImportant = false;
                if (mostImportant == null)
                    mostImportant = m;

                count++;

                if(m.likes.count > mostImportant.likes.count)
                    mostImportant = m;

                if (count > 10)
                {
                    mostImportant.IsImportant = true;
                    mostImportant = null;
                    count = 0;
                }
            }
        }

        protected void AddAll(IEnumerable<Media> elements)
        {
            foreach (Media element in elements)
            {
                Add(element);
            }
        }

        public override bool Equals(object obj)
        {
            return this.GetName().Equals(((MediaList)obj).GetName());
        }

        public Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            CoreDispatcher dispatcher = Window.Current.Dispatcher;

            return Task.Run<LoadMoreItemsResult>(
               async () =>
               {
                   MultipleMedia mm = await FetchMoreMedia();

                   await dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            AddAll(mm.data);
                        });
                   return new LoadMoreItemsResult() { Count = 40 };
               }).AsAsyncOperation<LoadMoreItemsResult>();
        }

        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public class Feed : MediaList
    {
        public Feed() { Init(); }

        protected async override void Init()
        {
            Args args = new Args(new Arg(Arg.Type.COUNT, "40"));
            MultipleMedia mm = await Instagram.LoadFeed(args);
            pagination = mm.pagination;
            AddAll(mm.data);
            base.Init();
        }

        public override string GetName()
        {
            return "Feed";
        }

        public async override Task<List<Media>> FetchNewMedia()
        {
            Args args = new Args();
            args.Add(new Arg(Arg.Type.MIN_ID, this.ElementAt(0).id));
            args.Add(new Arg(Arg.Type.COUNT, "100"));
            MultipleMedia mm = await Instagram.LoadFeed(args);

            return mm.data;
        }
    }

    public class UserMedia : MediaList
    {
        private User User;

        public UserMedia(User user) { User = user; Init(); }

        protected override async void Init()
        {
            MultipleMedia mm = await Instagram.LoadUserMedia(this.User);
            pagination = mm.pagination;
            AddAll(mm.data);
            base.Init();
        }

        public override string GetName()
        {
            return User.username;
        }

        public async override Task<List<Media>> FetchNewMedia()
        {
            Args args = new Args();
            args.Add(new Arg(Arg.Type.MIN_ID, this.ElementAt(0).id));
            args.Add(new Arg(Arg.Type.COUNT, "100"));
            MultipleMedia mm = await Instagram.LoadUserMedia(User, args);

            return mm.data;
        }
    }

    public class Popular : MediaList
    {
        public Popular() { Init(); }

        protected override async void Init()
        {
            MultipleMedia mm = await Instagram.LoadPopular();
            AddAll(mm.data);
            base.Init();
        }

        public override string GetName()
        {
            return "Popular";
        }

        public override async Task<MultipleMedia> FetchMoreMedia()
        {
            MultipleMedia mm = await Instagram.LoadPopular();
            return mm;
        }

        protected override bool CanLoadMoreItems()
        {
            return true;
        }

        public async override Task<List<Media>> FetchNewMedia()
        {
            MultipleMedia mm = await FetchMoreMedia();
            return mm.data;
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
            base.Init();
        }

        public override string GetName()
        {
            return "#" + this.Tag;
        }

        public async override Task<List<Media>> FetchNewMedia()
        {
            Args args = new Args();
            args.Add(new Arg(Arg.Type.COUNT, "100"));
            args.Add(new Arg(Arg.Type.MIN_TAG_ID, pagination.min_tag_id));
            MultipleMedia mm = await Instagram.LoadHashtag(Tag, args);
            return mm.data;
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
                AddToItemsGroup(ml);
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