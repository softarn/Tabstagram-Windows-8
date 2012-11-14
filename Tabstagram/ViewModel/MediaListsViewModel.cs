using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tabstagram.Models;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Tabstagram
{
    public abstract class MediaListViewModel : ObservableCollection<Media>, ISupportIncrementalLoading, INotifyPropertyChanged
    {
        public event EventHandler<NotificationEventArgs> CriticalNetworkErrorNotice;
        private string _category;
        public string category { get { return _category; }
            set
            {
                _category = value;
                OnPropertyChanged("category");
            }
        }
        protected override event PropertyChangedEventHandler PropertyChanged;
        public static List<DispatcherTimer> timers = new List<DispatcherTimer>();
        protected Pagination pagination;
        public bool Error = false;

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
            get { return _isLoaded; }

            set
            {
                _isLoaded = value;
                OnPropertyChanged("IsLoaded");
                
                if(Observer != null)
                    Observer.Update(value);
            }
        }

        public bool HasMoreItems
        {
            get
            {
                return CanLoadMoreItems();
            }
        }

        public IObserver<Boolean> Observer;

        protected virtual bool CanLoadMoreItems()
        {
            if (Error)
                return false;

            if (pagination == null)
                return false;

            return pagination.next_url != null;
        }

        public abstract string GetName();
        public abstract Task<IEnumerable<Media>> FetchNewMedia();

        public async virtual Task<bool> Init()
        {
            this.category = GetName();
            MarkImportantMedia(this);
            PopulateSubCollection();
            IsLoaded = true;
            return true;
        }

        public async Task<bool> Refresh()
        {
            IEnumerable<Media> newMedia;
            try
            {
                newMedia = await FetchNewMedia();
            }
            catch (Exception)
            {
                CriticalNetworkErrorNotice(null, new NotificationEventArgs());
                return false;
            }

            var enumerable = newMedia as Media[] ?? newMedia.ToArray();
            if (enumerable != null && enumerable.Any())
            {
                AddAll(enumerable);
                MarkImportantMedia(enumerable);
                UpdateSubCollection(enumerable);
            }
            return true;
        }

        private void UpdateSubCollection(IEnumerable<Media> newMedia)
        {
            int initialCount = SubCollection.Count();
            if (SubCollection.Count <= newMedia.Count())
                SubCollection.Clear();
            else
            {
                for (int i = 0; i < newMedia.Count(); i++)
                {
                    SubCollection.RemoveAt(SubCollection.Count - 1);
                }
            }
            int toAddCount = initialCount - SubCollection.Count();
            for (int i = 0; i < toAddCount; i++)
            {
                SubCollection.Insert(i, newMedia.ElementAt(i));
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

        public static MediaListViewModel GetClassFromString(string listString)
        {
            if (listString[0] == '#')
            {
                return new HashTag(listString.Substring(1, listString.Length - 1));
            }

            if (listString.Equals("popular"))
                return new Popular();
            if (listString.Equals("feed"))
                return new Feed();
            if (listString.Equals("selfmedia"))
                return new SelfMedia();

            throw new ArgumentException("listString must be #hashtagname, popular, selfmedia or feed");
        }

        public virtual async Task<MultipleMedia> FetchMoreMedia()
        {
            MultipleMedia mm = await Instagram.LoadMultipleMediaFromCustomUrl(pagination.next_url);
            pagination = mm.pagination;
            return mm;
        }

        public virtual void MarkImportantMedia(IEnumerable<Media> enumerable)
        {
            if (enumerable.Count() < 3)
                return;

            const int initialSpan = 2; //Elements between first and second that could be marked as important
            const int span = 4;

            if(enumerable.Count() > 0)
                enumerable.ElementAt(0).IsImportant = true;

            if (enumerable.Count() < initialSpan + 1)
                return;

            int count = 0;
            Media mostImportant = enumerable.ElementAt(initialSpan);
            foreach(Media m in enumerable.Skip(initialSpan))
            {
                m.IsImportant = false;
                if (mostImportant == null)
                    mostImportant = m;

                count++;

                if(m.likes.count > mostImportant.likes.count)
                    mostImportant = m;

                if (count < span) continue;

                mostImportant.IsImportant = true;
                mostImportant = null;
                count = 0;
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
            return this.GetName().Equals(((MediaListViewModel)obj).GetName());
        }

        public Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            CoreDispatcher dispatcher = Window.Current.Dispatcher;

            return Task.Run(
               async () =>
               {
                   MultipleMedia mm = null;
                   try { mm = await FetchMoreMedia(); }
                   catch (Exception) { this.Error = true; }

                   await dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            if (this.Error)
                                this.CriticalNetworkErrorNotice(null, new NotificationEventArgs());

                            if (mm != null)
                                AddAll(mm.data);
                        });
                   return new LoadMoreItemsResult() { Count = 40 };
               }).AsAsyncOperation();
        }

        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        internal void Reset()
        {
            Error = false;
        }
    }

    public class Feed : MediaListViewModel
    {
        public async override Task<bool> Init()
        {
            Args args = new Args(new Arg(Arg.Type.COUNT, "30"));
            MultipleMedia mm = await Instagram.LoadFeed(args);
            pagination = mm.pagination;
            AddAll(mm.data);
            await base.Init();
            return true;
        }

        public override string GetName()
        {
            return "Feed";
        }

        public async override Task<IEnumerable<Media>> FetchNewMedia()
        {
            Args args = new Args();
            args.Add(new Arg(Arg.Type.MIN_ID, this.ElementAt(0).id));
            args.Add(new Arg(Arg.Type.COUNT, "30"));
            MultipleMedia mm = await Instagram.LoadFeed(args);

            return mm.data;
        }
    }

    public class SelfMedia : MediaListViewModel
    {
        public async override Task<bool> Init()
        {
            Args args = new Args(new Arg(Arg.Type.COUNT, "30"));
            MultipleMedia mm = await Instagram.LoadSelfMedia(args);
            pagination = mm.pagination;
            AddAll(mm.data);
            base.Init();
            return true;
        }

        public override string GetName()
        {
            return "My Images";
        }

        public async override Task<IEnumerable<Media>> FetchNewMedia()
        {
            Args args = new Args();
            args.Add(new Arg(Arg.Type.MIN_ID, this.ElementAt(0).id));
            args.Add(new Arg(Arg.Type.COUNT, "30"));
            MultipleMedia mm = await Instagram.LoadSelfMedia(args);

            if (mm.data.Count > 0)
                return mm.data.Skip(1);

            return mm.data;
        }
    }
    
    public class UserMedia : MediaListViewModel
    {
        private readonly User _user;

        public UserMedia(User user) { _user = user; }

        public override async Task<bool> Init()
        {
            try
            {
                Args args = new Args(new Arg(Arg.Type.COUNT, "30"));
                MultipleMedia mm = await Instagram.LoadUserMedia(this._user, args);
                pagination = mm.pagination;
                AddAll(mm.data);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not init UserMedia");
            }
            base.Init();
            return true;
        }

        public override string GetName()
        {
            return _user.username;
        }

        public async override Task<IEnumerable<Media>> FetchNewMedia()
        {
            Args args = new Args {new Arg(Arg.Type.MIN_ID, this.ElementAt(0).id), new Arg(Arg.Type.COUNT, "30")};
            MultipleMedia mm = await Instagram.LoadUserMedia(_user, args);

            return mm.data;
        }
    }

    public class Popular : MediaListViewModel
    {
        public override async Task<bool> Init()
        {
            Args args = new Args(new Arg(Arg.Type.COUNT, "30"));
            MultipleMedia mm = await Instagram.LoadPopular(args);
            AddAll(mm.data);
            await base.Init();
            return true;
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

        public async override Task<IEnumerable<Media>> FetchNewMedia()
        {
            MultipleMedia mm = await FetchMoreMedia();
            return mm.data;
        }
    }

    public class HashTag : MediaListViewModel
    {
        public string Tag { get; set; }

        public HashTag(string tag)
        {
            this.Tag = tag;
            this.category = GetName();
        }

        public override async Task<bool> Init()
        {
            Args args = new Args(new Arg(Arg.Type.COUNT, "30"));
            MultipleMedia mm = await Instagram.LoadHashtag(Tag, args);
            pagination = mm.pagination;
            AddAll(mm.data);
            await base.Init();
            return true;
        }

        public override string GetName()
        {
            return "#" + this.Tag;
        }

        public async override Task<IEnumerable<Media>> FetchNewMedia()
        {
            var args = new Args();
            args.Add(new Arg(Arg.Type.COUNT, "30"));
            args.Add(new Arg(Arg.Type.MIN_TAG_ID, pagination.min_tag_id));
            MultipleMedia mm = await Instagram.LoadHashtag(Tag, args);
            return mm.data;
        }
    }
}