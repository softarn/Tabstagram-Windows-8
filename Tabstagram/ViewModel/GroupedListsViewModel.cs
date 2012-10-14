using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tabstagram.Models;

namespace Tabstagram
{
    class GroupedListsViewModel : INotifyPropertyChanged, IObserver<Boolean>
    {
        public event EventHandler<NotificationEventArgs> CriticalNetworkErrorNotice;
        public ObservableCollection<MediaListViewModel> ItemGroups = new ObservableCollection<MediaListViewModel>();
        private bool _isLoading;
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }

            set
            {
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public GroupedListsViewModel()
        {
            IsLoading = true;
        }

        public async Task Reset()
        {
            IsLoading = true;
            ItemGroups.Clear();
            await this.LoadFromSettings();
        }

        public async Task LoadFromSettings()
        {
            List<string> list = UserSettings.MediaStringsList;

            foreach (MediaListViewModel ml in list.Select(MediaListViewModel.GetClassFromString))
            {
                AddToItemsGroup(ml);
            }

            ItemGroups.First().Observer = this;

            try
            {
                foreach (MediaListViewModel ml in ItemGroups)
                    await ml.Init();
            }
            catch (Exception)
            {
                CriticalNetworkErrorNotice(this, new NotificationEventArgs());
            }
        }

        public bool AddToItemsGroup(MediaListViewModel ml)
        {
            if (ItemGroups.Contains(ml))
                return false;

            ItemGroups.Add(ml);
            return true;
        }

        public MediaListViewModel GetListFromString(string listName)
        {
            foreach (MediaListViewModel g in ItemGroups.Where(g => g.category.ToLower().Equals(listName.ToLower())))
            {
                return g;
            }

            throw new System.ArgumentException("listName must be the name of a Group category");
        }

        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public void Update(bool b)
        {
            if (b)
                ItemGroups.First().Observer = null;

            IsLoading = !b;
        }
    }

    public interface IObserver<in T>
    {
        void Update(T b);
    }
}
