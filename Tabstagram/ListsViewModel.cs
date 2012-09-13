using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Tabstagram
{
    public class Group : Media
    {
        public ObservableCollection<Media> Items { get; set; }
        public string category { get; set; }
        public bool IsLoaded { get; set; }

        public Group(string name, ObservableCollection<Media> items)
        {
            this.category = name;
            Items = items;
        }
    }

    class ListsViewModel
    {
        public List<DispatcherTimer> timers = new List<DispatcherTimer>();
        public ObservableCollection<Group> ItemGroups = new ObservableCollection<Group>();

        public async void AddMediaListType(MediaListType mlt, int position = -1)
        {
            ObservableCollection<Media> List = new ObservableCollection<Media>();
            
            if(position == -1)
                ItemGroups.Add(new Group(mlt.GetName(), List));
            else
                ItemGroups.Insert(position, new Group(mlt.GetName(), List));

            List<Media> tmpList = await Instagram.LoadMediaList(mlt);
            AddAllTo(tmpList, List);        
        }

        private void AddAllTo(List<Media> elements, ObservableCollection<Media> list)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += (sender, e) => { if(elements.Count == 0){return;} list.Add(elements.First()); elements.Remove(elements.First()); }; // Everytime timer ticks, timer_Tick will be called             // Timer will tick evert second                       // Enable the timer
            timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            timer.Start();                              // Start the timer
            timers.Add(timer);
        }

    }
}