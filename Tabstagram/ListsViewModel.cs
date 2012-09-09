using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabstagram
{
    public class Group : Media
    {
        public ObservableCollection<Media> Items { get; set; }
        public string category { get; set; }

        public Group(string name, ObservableCollection<Media> items)
        {
            this.category = name;
            Items = items;
        }
    }

    class ListsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Group> ItemGroups = new ObservableCollection<Group>();

        public async void LoadAll()
        {
            LoadFeed();
            LoadPopular();
            LoadTabstagram();
        }
        public async void LoadFeed()
        {
            ObservableCollection<Media> feed = new ObservableCollection<Media>();
            ItemGroups.Add(new Group("Feed", feed));
            List<Media> tmpFeed = await Instagram.Feed();

            foreach (Media m in tmpFeed)
            {
                feed.Add(m);
            }

            Notify("List");
        }

        public async void LoadPopular()
        {
            ObservableCollection<Media> feed = new ObservableCollection<Media>();
            ItemGroups.Add(new Group("Popular", feed));
            List<Media> tmpFeed = await Instagram.Popular();

            foreach (Media m in tmpFeed)
            {
                feed.Add(m);
            }

            Notify("List");
        }

        public async void LoadTabstagram()
        {
            ObservableCollection<Media> feed = new ObservableCollection<Media>();
            ItemGroups.Add(new Group("#tabstagram", feed));
            List<Media> tmpFeed = await Instagram.Tabstagram();

            foreach (Media m in tmpFeed)
            {
                feed.Add(m);
            }

            Notify("List");
        }

        private void Notify(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
