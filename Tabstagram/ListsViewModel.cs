using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tabstagram.Models;
using Windows.UI.Xaml;

namespace Tabstagram
{
    public abstract class MediaList
    {
        public static List<DispatcherTimer> timers = new List<DispatcherTimer>();
        protected Pagination pagination;
        public ObservableCollection<Media> ItemsAll { get; set; }
        public ObservableCollection<Media> ItemsSmall { get; set; }
        public string category { get; set; }
        public bool IsLoaded { get; set; }

        public abstract void LoadList();
        public abstract Task<bool> LoadMore();

        public abstract string GetName();

        protected void Init()
        {
            this.category = GetName();
            this.ItemsAll = new ObservableCollection<Media>();
            this.ItemsSmall = new ObservableCollection<Media>();
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

        protected void AddOneByOne(List<Media> elements, ObservableCollection<Media> collection)
        {
            AddAllImmediately(elements, collection);
            //DispatcherTimer timer = new DispatcherTimer();
            //timer.Tick += (sender, e) =>
            //{
            //    if (elements.Count == 0) { return; }
            //    collection.Add(elements.First());
            //    elements.Remove(elements.First());
            //}; // Everytime timer ticks, timer_Tick will be called
            //timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            //timer.Start();                              // Start the timer
            //timers.Add(timer);
        }

        protected void AddAllImmediately(List<Media> elements, ObservableCollection<Media> collection)
        {
            foreach (Media element in elements)
            {
                collection.Add(element);
            }
        }

        public override bool Equals(object obj)
        {
            return this.GetName().Equals(((MediaList)obj).GetName());
        }
    }

    public class Feed : MediaList
    {

        public Feed() { Init(); }

        public override async void LoadList()
        {
            MultipleMedia mm = await Instagram.LoadFeed();
            pagination = mm.pagination;
            AddOneByOne(mm.data, ItemsSmall);
            AddAllImmediately(mm.data, ItemsAll);
        }

        public override string GetName()
        {
            return "Feed";
        }

        public async override Task<bool> LoadMore()
        {
            Args args = new Args();
            args.Add(new Args.Arg(Args.Arg.Type.MAX_ID, ItemsAll.Last().id));
            args.Add(new Args.Arg(Args.Arg.Type.COUNT, "25"));
            MultipleMedia mm = await Instagram.LoadFeed(args);
            pagination = mm.pagination;
            AddAllImmediately(mm.data, ItemsAll);
            return true;
        }
    }

    public class UserMedia : MediaList
    {
        private User User;

        public UserMedia(User user) { User = user; Init(); }

        public override async void LoadList()
        {
            MultipleMedia mm = await Instagram.LoadUserMedia(this.User);
            pagination = mm.pagination;
            AddAllImmediately(mm.data, ItemsAll);
        }

        public override string GetName()
        {
            return "Feed";
        }

        public async override Task<bool> LoadMore()
        {
            MultipleMedia mm = await Instagram.LoadFromCustomUrl(pagination.next_url);
            pagination = mm.pagination;
            AddAllImmediately(mm.data, ItemsAll);
            return true;
        }
    }

    public class Popular : MediaList
    {
        public Popular() { Init(); }

        public override async void LoadList()
        {
            MultipleMedia mm = await Instagram.LoadPopular();
            pagination = mm.pagination;
            AddOneByOne(mm.data, ItemsSmall);
            AddAllImmediately(mm.data, ItemsAll);
        }
        public override string GetName()
        {
            return "Popular";
        }

        public override async Task<bool> LoadMore()
        {
            ItemsAll.Clear();
            MultipleMedia mm = await Instagram.LoadPopular();
            pagination = mm.pagination;
            AddAllImmediately(mm.data, ItemsAll);
            AddAllImmediately(mm.data, ItemsSmall);
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

        public override async void LoadList()
        {
            MultipleMedia mm = await Instagram.LoadHashtag(Tag);
            pagination = mm.pagination;
            AddOneByOne(mm.data, ItemsSmall);
            AddAllImmediately(mm.data, ItemsAll);
        }

        public override string GetName()
        {
            return "#" + this.Tag;
        }

        public override async Task<bool> LoadMore()
        {
            Args args = new Args();
            args.Add(new Args.Arg(Args.Arg.Type.COUNT, "25"));
            MultipleMedia mm = await Instagram.LoadFromCustomUrl(pagination.next_url, args);
            pagination = mm.pagination;
            AddAllImmediately(mm.data, ItemsAll);
            return true;
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