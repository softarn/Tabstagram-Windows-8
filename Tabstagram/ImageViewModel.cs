﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tabstagram
{
    class ImageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Media _currentMedia;
        public Media CurrentMedia
        {
            get
            {
                return _currentMedia;
            }

            set
            {
                _currentMedia = value;
                OnPropertyChanged("CurrentMedia");
            }
        }

        private User _currentUser;
        public User currentUser
        {
            get
            {
                return _currentUser;
            }

            set
            {
                _currentUser = value;
                OnPropertyChanged("CurrentUser");
            }
        }

        public MediaList RelatedMedia { get; set; }

        public ImageViewModel(Media media)
        {
            CurrentMedia = media;
            RelatedMedia = new UserMedia(media.user);
            RelatedMedia.LoadList();
            OnPropertyChanged("CurrentMedia");
            LoadUserInfo(media.user.id);
            LoadComments(media.id);
        }

        public void LoadNewMedia(Media media)
        {
            media.user = CurrentMedia.user;
            CurrentMedia = media;
            OnPropertyChanged("CurrentMedia");
        }

        public async void LoadUserInfo(string userId)
        {
            User user = await Instagram.LoadUserInfo(userId);
            CurrentMedia.user = user;
        }

        public async void LoadComments(string mediaId)
        {
            if (CurrentMedia.comments.count == CurrentMedia.comments.data.Count)
                return;

            List<Comment> comments = await Instagram.LoadComments(mediaId);
            CurrentMedia.comments.data = comments;
        }

        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
