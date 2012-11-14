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
            RelatedMedia = new UserMedia(CurrentUser);
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
                Debug.WriteLine("Error when trying to init");
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
            else
            {
                if (CurrentMedia.comments.count != CurrentMedia.comments.observableData.Count)
                    CurrentMedia.comments.observableData.Clear();
            }

            LoadUserInfo(CurrentUser.id);
            LoadComments();
        }

        public void LoadNewMedia(Media media)
        {
            media.user = CurrentUser;
            CurrentMedia = media;
            OnPropertyChanged("CurrentMedia");
        }

        public async void LoadUserInfo(string userId)
        {
            try
            {
                User user = await Instagram.LoadUserInfo(userId);

                if (user != null)
                {
                    CurrentUser = user;
                }
                else
                {
                    CurrentUser.counts = new Counts();
                    CurrentUser.counts.followed_by = 0;
                    CurrentUser.counts.follows = 0;
                    CurrentUser.counts.media = 0;
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Error when trying to load user info");
                CriticalNetworkErrorNotice(null, new NotificationEventArgs());
            }

            LoadRelationship();
        }

        public async void LoadRelationship()
        {
            try
            {
                Relationship r = await Instagram.LoadRelationship(CurrentUser.id);
                CurrentUser.user_is_following = r.data.outgoing_follow;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error when trying to load user info");
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
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
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
            if (CurrentUser.followed_by != null)
                return;

            try
            {
                MultipleUsers users = await Instagram.LoadFollowedBy(CurrentUser.id);
                UserList userList = new UserList(users.data);
                userList.pagination = users.pagination;
                CurrentUser.followed_by = userList;
            }
            catch (Exception e)
            {
                CurrentUser.followed_by = new UserList(new List<User>());
            }
            return;
        }

        internal async Task LoadFollows()
        {
            if (CurrentUser.follows != null)
                return;

            try
            {
                MultipleUsers users = await Instagram.LoadFollows(CurrentUser.id);
                UserList userList = new UserList(users.data);
                userList.pagination = users.pagination;
                CurrentUser.follows = userList;
            }
            catch (Exception e)
            {
                CurrentUser.follows = new UserList(new List<User>());
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
            LoadUserInfo(CurrentUser.id);
            CurrentMedia.comments.data = new List<Comment>();
            LoadComments();
        }

        internal async Task FollowOrUnfollow()
        {
            if (CurrentUser == null)
                return;

            bool success;

            bool InitialUserIsFollowing = CurrentUser.user_is_following;

            CurrentUser.user_is_following = !InitialUserIsFollowing;

            if (CurrentUser.user_is_following)
            {
                success = await Instagram.Follow(CurrentUser.id);
            }
            else
            {
                success = await Instagram.Unfollow(CurrentUser.id);
            }

            if (!success)
                CurrentUser.user_is_following = InitialUserIsFollowing;
        }
    }
}
