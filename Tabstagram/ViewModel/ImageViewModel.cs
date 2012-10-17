using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tabstagram.Models;
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
        public User CurrentUser
        {
            get { return _currentUser; }

            set
            {
                _currentUser = value;
                OnPropertyChanged("CurrentUser");
            }
        }

        public MediaListViewModel RelatedMedia { get; set; }

        public ImageViewModel(User user)
        {
            RelatedMedia = new UserMedia(user);
            CurrentUser = user;
            Init();
        }

        public ImageViewModel(Media media)
        {
            CurrentMedia = media;
            CurrentUser = CurrentMedia.user;
            RelatedMedia = new UserMedia(CurrentMedia.user);
            Init();
        }

        public async void Init()
        {
            if (CurrentMedia.comments.count != CurrentMedia.comments.observableData.Count)
                CurrentMedia.comments.observableData.Clear();

            try
            {
                await RelatedMedia.Init();
                Relationship r = await Instagram.LoadRelationship(CurrentMedia.user.id);
                CurrentMedia.user.user_is_following = r.data.outgoing_follow;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error when trying to init");
                CriticalNetworkErrorNotice(null, new NotificationEventArgs());
            }

            if (CurrentMedia == null)
            {
                if (RelatedMedia.Any())
                    CurrentMedia = RelatedMedia.First();
                else
                {
                    Media tmpMedia = new Media();
                    tmpMedia.images = new Images();
                    tmpMedia.images.standard_resolution = new StandardResolution();
                    tmpMedia.images.standard_resolution.url = "ms-appx:/Assets/HeartIcon.png";
                    tmpMedia.comments = new Comments();
                    tmpMedia.comments.count = 0;
                    tmpMedia.comments.data = new List<Comment>();
                    tmpMedia.user = CurrentUser;
                    CurrentMedia = tmpMedia;
                }

                OnPropertyChanged("CurrentMedia");
            }

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

                if (user != null)
                    CurrentMedia.user = user;
                else
                {
                    CurrentMedia.user.counts = new Counts();
                    CurrentMedia.user.counts.followed_by = 0;
                    CurrentMedia.user.counts.follows = 0;
                    CurrentMedia.user.counts.media = 0;
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Error when trying to load user info");
                CriticalNetworkErrorNotice(null, new NotificationEventArgs());
            }
        }

        public async void LoadComments(bool forced = false)
        {

            if (CurrentMedia.comments.count == CurrentMedia.comments.observableData.Count && !forced)
                return;
            else
                CurrentMedia.comments.observableData.Clear();

            List<Comment> comments = null;
            try
            {
                comments = await Instagram.LoadComments(CurrentMedia.id);
            }
            catch (Exception) { }

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

            try
            {
                MultipleUsers users = await Instagram.LoadFollowedBy(CurrentMedia.user.id);
                UserList userList = new UserList(users.data);
                userList.pagination = users.pagination;
                CurrentMedia.user.followed_by = userList;
            }
            catch (Exception e)
            {
                CurrentMedia.user.followed_by = new UserList(new List<User>());
            }
            return;
        }

        internal async Task LoadFollows()
        {
            if (CurrentMedia.user.follows != null)
                return;

            try
            {
                MultipleUsers users = await Instagram.LoadFollows(CurrentMedia.user.id);
                UserList userList = new UserList(users.data);
                userList.pagination = users.pagination;
                CurrentMedia.user.follows = userList;
            }
            catch (Exception e)
            {
                CurrentMedia.user.follows = new UserList(new List<User>());
            }
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

        internal async void Reset()
        {
            try
            {
                await RelatedMedia.Init();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error when trying reset");
                CriticalNetworkErrorNotice(null, new NotificationEventArgs());
            }
            OnPropertyChanged("CurrentMedia");
            LoadUserInfo(CurrentMedia.user.id);
            CurrentMedia.comments.data = new List<Comment>();
            LoadComments();
        }

        internal async Task FollowOrUnfollow()
        {
            if (CurrentMedia.user == null)
                return;

            bool success;

            bool InitialUserIsFollowing = CurrentMedia.user.user_is_following;

            CurrentMedia.user.user_is_following = !InitialUserIsFollowing;

            if (CurrentMedia.user.user_is_following)
            {
                success = await Instagram.Follow(CurrentMedia.user.id);
            }
            else
            {
                success = await Instagram.Unfollow(CurrentMedia.user.id);
            }

            if (!success)
                CurrentMedia.user.user_is_following = InitialUserIsFollowing;
        }
    }
}
