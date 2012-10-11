using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Tabstagram
{
    class ImageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<NotificationEventArgs> CriticalNetworkErrorNotice;

        private readonly DispatcherTimer _timer = new DispatcherTimer(); 

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

        public ImageViewModel(User user)
        {
            RelatedMedia = new UserMedia(user);
            Init();
        }

        public ImageViewModel(Media media)
        {
            CurrentMedia = media;
            RelatedMedia = new UserMedia(CurrentMedia.user);
            Init();
        }

        public async void Init()
        {
            try
            {
                await RelatedMedia.Init();
            }
            catch (Exception)
            {
                CriticalNetworkErrorNotice(null, new NotificationEventArgs());
            }

            /* If we haven't selected any media yet, select the first one in the list */
            if (CurrentMedia == null)
            {
                /* NEEDS A CHECK TO SEE THAT WE'VE GOT ITEMS! */
                /* Otherwise, just close the page and show a message about the user not having any images */
                CurrentMedia = RelatedMedia.First();
            }

            OnPropertyChanged("CurrentMedia");
            LoadUserInfo(CurrentMedia.user.id);
            LoadComments();
        }

        public void LoadNewMedia(Media media)
        {
            media.user = CurrentMedia.user;
            CurrentMedia = media;
            OnPropertyChanged("CurrentMedia");
        }

        public async void LoadUserInfo(string userId)
        {
            try
            {
                User user = await Instagram.LoadUserInfo(userId);
                CurrentMedia.user = user;
            }
            catch (Exception)
            {
                CriticalNetworkErrorNotice(null, new NotificationEventArgs());
            }
        }

        public async void LoadComments(bool forced = false)
        {
            if (CurrentMedia.comments.count == CurrentMedia.comments.observableData.Count && !forced)
                return;

            List<Comment> comments = await Instagram.LoadComments(CurrentMedia.id);
            CurrentMedia.comments.ClearAndAddComments(comments);

            Media relatedMediaItem = null;
            int index = RelatedMedia.IndexOf(CurrentMedia);
            if (index > -1)
                relatedMediaItem = RelatedMedia.ElementAt(index);

            if (relatedMediaItem != null && relatedMediaItem != CurrentMedia)
                relatedMediaItem.comments.ClearAndAddComments(comments);
        }

        public async Task<bool> LikeOrUnlike()
        {
            Media relatedMediaItem = null;
            int index = RelatedMedia.IndexOf(CurrentMedia);
            if (index > -1)
                relatedMediaItem = RelatedMedia.ElementAt(index);

            bool success = false;
            if (CurrentMedia.user_has_liked)
            {
                success = await Instagram.Unlike(CurrentMedia.id);
                if (success)
                {
                    CurrentMedia.Unlike();
                    if (relatedMediaItem != null && relatedMediaItem != CurrentMedia)
                        relatedMediaItem.Unlike();
                }
            }
            else
            {
                success = await Instagram.Like(CurrentMedia.id);
                if (success)
                {
                    CurrentMedia.Like();
                    if (relatedMediaItem != null && relatedMediaItem != CurrentMedia)
                        relatedMediaItem.Like();
                }
            }

            return success;
        }

        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        internal async void Comment(string comment)
        {
            await Instagram.CommentMedia(this.CurrentMedia.id, comment);
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 400);
            _timer.Tick += TimerLoadComments;
            _timer.Start();
        }

        void TimerLoadComments(object sender, object e)
        {
            LoadComments(true);
            _timer.Stop();
        }

        internal async Task<bool> DeleteComment(string commentId)
        {
            bool success = await Instagram.DeleteComment(CurrentMedia.id, commentId);

            if (success)
            {
                Media relatedMediaItem = null;
                int index = RelatedMedia.IndexOf(CurrentMedia);
                if (index > -1)
                    relatedMediaItem = RelatedMedia.ElementAt(index);

                Comment tmpComment = new Comment {id = commentId};
                CurrentMedia.comments.RemoveComment(tmpComment);
                if (relatedMediaItem != null) relatedMediaItem.comments.RemoveComment(tmpComment);
            }
            return success;
        }

        internal async Task LoadFollowedBy()
        {
            if (CurrentMedia.user.followed_by != null)
                return;

            MultipleUsers users = await Instagram.LoadFollowedBy(CurrentMedia.user.id);
            UserList userList = new UserList(users.data);
            userList.pagination = users.pagination;
            CurrentMedia.user.followed_by = userList;
            return;
        }

        internal async Task LoadFollows()
        {
            if (CurrentMedia.user.follows != null)
                return;

            MultipleUsers users = await Instagram.LoadFollows(CurrentMedia.user.id);
            UserList userList = new UserList(users.data);
            userList.pagination = users.pagination;
            CurrentMedia.user.follows = userList;
            return;
        }

        internal async Task LoadLikes()
        {
            if (CurrentMedia.likes.data.Count == CurrentMedia.likes.count)
                return;

            CurrentMedia.likes.data = null;
            MultipleUsers users = await Instagram.LoadLikes(CurrentMedia.id);
            CurrentMedia.likes.data = users.data.ToList();
            return;
        }

        internal void Reset()
        {
            try
            {
                RelatedMedia.Init();
            }
            catch (Exception)
            {
                CriticalNetworkErrorNotice(null, new NotificationEventArgs());
            }
            OnPropertyChanged("CurrentMedia");
            LoadUserInfo(CurrentMedia.user.id);
            CurrentMedia.comments.data = new List<Comment>();
            LoadComments();
        }
    }
}
