using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tabstagram.Models;
using Windows.Data.Json;

namespace Tabstagram
{
    public class User : INotifyPropertyChanged
    {
        public User()
        {
            counts = new Counts();
        }

        public string id { get; set; }
        public string username { get; set; }
        private string _full_name;
        public string full_name { 
            get
            {
                if (_full_name != null && _full_name.Trim().Equals(""))
                {
                    return username;
                }

                return _full_name;
            }
            set
            {
                _full_name = value;
            }
        }
        public string profile_picture { get; set; }
        public string bio { get; set; }
        public string website { get; set; }
        public Counts counts { get; set; }
        private UserList _followed_by;
        public UserList followed_by
        {
            get
            {
                return _followed_by;
            }
            set
            {
                _followed_by = value;
                OnPropertyChanged("followed_by");
            }
        }
        private UserList _follows;
        public UserList follows
        {
            get
            {
                return _follows;
            }
            set
            {
                _follows = value;
                OnPropertyChanged("follows");
            }
        }

        public static User SingleFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<SingleUser>(jsonString).data;
        }

        public static MultipleUsers MultipleFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<MultipleUsers>(jsonString);
        }

        private class SingleUser
        {
            public User data { get; set; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public class Counts : INotifyPropertyChanged
    {
        private int _media;
        public int media
        {
            get { return _media; }
            set
            {
                _media = value;
                OnPropertyChanged("media");
            }
        }

        private int _follows;
        public int follows         
        {
            get { return _follows; }
            set
            {
                _follows = value;
                OnPropertyChanged("follows");
            }
        }

        private int _followed_by;
        public int followed_by {
            get { return _followed_by; }
            set
            {
                _followed_by = value;
                OnPropertyChanged("followed_by");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public class MultipleUsers
    {
        public Pagination pagination { get; set; }
        public IEnumerable<User> data { get; set; }
    }
}
