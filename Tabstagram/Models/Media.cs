using Newtonsoft.Json;
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

    public class Comments : INotifyPropertyChanged
    {
        public Comments() { observableData = new ObservableCollection<Comment>(); }
        public Comments(List<Comment> comments)
        {
            observableData = new ObservableCollection<Comment>();
            data = comments;
            // Commented out due to counts being wrong - didn't look like it broke anything. Kept in case.
            //count = comments.Count;
        }

        public void AddAllToObservable(List<Comment> from)
        {
            foreach(Comment c in from)
            {
                observableData.Add(c);
            }
        }

        public ObservableCollection<Comment> observableData { get; set; }

        public List<Comment> data
        {
            set
            {
                ClearAndAddComments(value);
            }
        }

        private int _count;
        public int count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
                OnPropertyChanged("count");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void ClearAndAddComments(List<Comment> comments)
        {
            observableData.Clear();
            AddAllToObservable(comments);
            // Commented out due to counts being wrong - didn't look like it broke anything. Kept in case.
            //count = comments.Count();
        }

        internal bool RemoveComment(Comment comment)
        {
            bool removed = observableData.Remove(comment);
            if (removed == false) return false;

            count--;
            return true;
        }

        internal bool AddComment(Comment comment)
        {
            observableData.Add(comment);
            count++;
            return true;
        }
    }

    public class Likes : INotifyPropertyChanged
    {
        private int _count;
        public int count
        {
            get { return _count; }
            set
            {
                _count = value;
                OnPropertyChanged("count");
            }
        }
        public List<User> data { get; set; }

        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class LowResolution
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Thumbnail
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class StandardResolution
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Images
    {
        public LowResolution low_resolution { get; set; }
        public Thumbnail thumbnail { get; set; }
        public StandardResolution standard_resolution { get; set; }
    }

    public class Caption
    {
        public string created_time { get; set; }
        public string text { get; set; }
        public User from { get; set; }
        public string id { get; set; }
    }

    public class Media : INotifyPropertyChanged
    {
        public bool IsImportant { get; set; }
        public string type { get; set; }
        public string filter { get; set; }
        public List<object> tags { get; set; }
        private Comments _comments;
        public Comments comments
        {
            get
            {
                return _comments;
            }
            set
            {
                _comments = value;
                OnPropertyChanged("comments");
            }
        }
        public Caption caption { get; set; }
        public Likes likes { get; set; }
        public string link { get; set; }
        private User _user;
        public User user
        {
            get { return _user; }
            set 
            {
                _user = value;
                OnPropertyChanged("user");
            }
        }
        private bool _userHasLiked;
        public bool user_has_liked
        {
            get { return _userHasLiked; }
            set
            {
                _userHasLiked = value;
                OnPropertyChanged("user_has_liked");
            }
        }
        public string created_time { get; set; }
        public Images images { get; set; }
        public string id { get; set; }
        public Location location { get; set; }

        public void Like()
        {
            user_has_liked = true;
            likes.count++;
        }

        public void Unlike()
        {
            user_has_liked = false;
            likes.count--;
        }

        public static Media SingleFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<SingleMedia>(jsonString).data;
        }

        public static MultipleMedia ListFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<MultipleMedia>(jsonString);
        }

        public override string ToString()
        {
            return link;
        }

        public override bool Equals(object obj)
        {
            return ((Media)obj).id == this.id;
        }

        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class SingleMedia
    {
        public Pagination pagination { get; set; }
        public Meta meta { get; set; }
        public Media data { get; set; }
    }

    public class MultipleMedia
    {
        public Pagination pagination { get; set; }
        public Meta meta { get; set; }
        public List<Media> data { get; set; }
    }
}
