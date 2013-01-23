using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tabstagram.Data;
using Tabstagram.Helpers;
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

            foreach (string mediaStr in list)
            {
                try 
                {
                    MediaListViewModel ml = FavouriteMediaListHelper.GetClassFromString(mediaStr);
                    AddToItemsGroup(ml);
                } catch (Exception) {}
            }

            ItemGroups.First().Observer = this;

            try
            {
                foreach (MediaListViewModel ml in ItemGroups)
                    ml.Init();
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

        internal void DeleteMediaList(MediaListViewModel m)
        {
            int currentPos = ItemGroups.IndexOf(m);
            bool deleted = FavouriteMediaListHelper.DeleteMediaString(m.GetName());

            if (deleted == false) return;
            ItemGroups.RemoveAt(currentPos);
        }

        /// <summary>
        /// Moves a MediaItem to a new position
        /// </summary>
        /// <param name="direction">Any negative for moving it backwards, any positive for moving it forward. 0 will not change its position</param>
        internal void MoveMediaList(MediaListViewModel m, int direction)
        {
            if (direction == 0) return;

            int currentPos = ItemGroups.IndexOf(m);

            List<string> list = UserSettings.MediaStringsList;
            int nextPos = currentPos;

            if (currentPos == list.Count - 1 && direction > 0)
                nextPos = 0;
            else if (currentPos == 0 && direction < 0)
                nextPos = list.Count - 1;
            else
                nextPos = direction < 0 ? currentPos-1 : currentPos+1;

            string str = list[currentPos];
            ItemGroups.Move(currentPos, nextPos);
            list.Insert(nextPos, str);

            if (nextPos < currentPos)
                list.RemoveAt(currentPos + 1);
            else
                list.RemoveAt(currentPos);

            UserSettings.MediaStringsList = list;
        }
    }

    public interface IObserver<in T>
    {
        void Update(T b);
    }
}
