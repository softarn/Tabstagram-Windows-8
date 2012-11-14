using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tabstagram.Models;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Tabstagram
{
    public class UserList : ObservableCollection<User>, ISupportIncrementalLoading
    {
        public event EventHandler<NotificationEventArgs> CriticalNetworkErrorNotice;
        public bool Error;

        public UserList(IEnumerable<User> enumerable) : base(enumerable) { }
        public UserList(MultipleUsers users) : base(users.data) 
        {
            pagination = users.pagination;
        }

        public Pagination pagination { get; set; }

        public bool HasMoreItems
        {
            get { return CanLoadMoreItems(); }
        }

        protected virtual bool CanLoadMoreItems()
        {
            if (Error)
                return false;

            if (pagination == null)
                return false;

            return pagination.next_url != null;
        }

        public Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            CoreDispatcher dispatcher = Window.Current.Dispatcher;

            return Task.Run(
               async () =>
               {
                   MultipleUsers mu = null;
                   try { mu = await FetchMoreUsers(); }
                   catch (Exception) { this.Error = true; }

                   await dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            if (this.Error)
                                this.CriticalNetworkErrorNotice(null, new NotificationEventArgs());

                            if (mu != null)
                                AddAll(mu.data);
                        });
                   return new LoadMoreItemsResult() { Count = 40 };
               }).AsAsyncOperation();
        }

        private void AddAll(IEnumerable<User> users)
        {
            foreach (User u in users)
            {
                Add(u);
            }
        }

        private async Task<MultipleUsers> FetchMoreUsers()
        {
            if (pagination == null || pagination.next_url == null)
                return null;

            MultipleUsers mu = await Instagram.LoadMultipleUsersFromCustomUrl(pagination.next_url);
            pagination = mu.pagination;
            return mu;
        }
    }
}
