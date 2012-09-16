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
        public ObservableCollection<Media> Items { get; set; }
        public string category { get; set; }
        public bool IsLoaded { get; set; }

        public MediaList()
        {
            this.category = GetName();
            this.Items = new ObservableCollection<Media>();
            LoadList();
        }

        protected abstract void LoadList();
        public abstract string GetName();

        public static MediaList GetClassFromString(string listString)
        {
            if (listString[0] == '#')
            {
                return new HashTag(listString.Substring(1, listString.Length));
            }

            if (listString.Equals("popular"))
                return new Popular();
            else if (listString.Equals("feed"))
                return new Feed();

            throw new System.ArgumentException("listString must be #hashtagname, popular or feed");
        }

        protected void AddAllTo(List<Media> elements, ObservableCollection<Media> list)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += (sender, e) => { if (elements.Count == 0) { return; } list.Add(elements.First()); elements.Remove(elements.First()); }; // Everytime timer ticks, timer_Tick will be called             // Timer will tick evert second                       // Enable the timer
            timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            timer.Start();                              // Start the timer
            timers.Add(timer);
        }
    }

    public class Feed : MediaList
    {

        protected override async void LoadList()
        {
           List<Media> list = await Instagram.LoadFeed();
           AddAllTo(list, this.Items);
        }

        public override string GetName()
        {
            return "Feed";
        }
    }

    public class Popular : MediaList
    {
        protected override async void LoadList()
        {
            List<Media> list = await Instagram.LoadPopular();
            AddAllTo(list, this.Items);
        }
        public override string GetName()
        {
            return "Popular";
        }
    }

    public class HashTag : MediaList
    {
        public string Tag { get; set; }

        public HashTag(string tag) : base()
        {
            this.Tag = tag;
        }

        protected override async void LoadList()
        {
            List<Media> list = await Instagram.LoadFeed();
            AddAllTo(list, this.Items);
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
                ItemGroups.Add(MediaList.GetClassFromString(str));
            }
        }

        //public async void AddMediaListType(MediaListType mlt, int position = -1)
        //{

        //    ObservableCollection<Media> List = new ObservableCollection<Media>();
            
        //    if(position == -1)
        //        ItemGroups.Add(new MediaList(mlt.GetName(), List));
        //    else
        //        ItemGroups.Insert(position, new MediaList(mlt.GetName(), List));

        //    List<Media> tmpList = await Instagram.LoadMediaList(mlt);
        //    AddAllTo(tmpList, List);        
        //}

        public ObservableCollection<Media> GetListFromString(string listName)
        {
            foreach (MediaList g in ItemGroups)
            {
                if(g.category.ToLower().Equals(listName.ToLower()))
                    return g.Items;
            }

            throw new System.ArgumentException("listName must be the name of a Group category");
        }



    }
}