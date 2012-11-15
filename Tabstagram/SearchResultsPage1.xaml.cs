using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Search Contract item template is documented at http://go.microsoft.com/fwlink/?LinkId=234240

namespace Tabstagram
{
    /// <summary>
    /// This page displays search results when a global search is directed to this application.
    /// </summary>
    public sealed partial class SearchResultsPage1 : Tabstagram.Common.LayoutAwarePage
    {
        private const string USERS = "Users";
        private const string TAGS = "Tags";

        private const string USER_VM = "UserResults";
        private const string TAG_VM = "TagResults";

        List<User> userList = null;
        List<Tag> tagList = null;

        public SearchResultsPage1()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            var queryText = navigationParameter as String;

            // TODO: Application-specific searching logic.  The search process is responsible for
            //       creating a list of user-selectable result categories:
            //
            //       filterList.Add(new Filter("<filter name>", <result count>));
            //
            //       Only the first filter, typically "All", should pass true as a third argument in
            //       order to start in an active state.  Results for the active filter are provided
            //       in Filter_SelectionChanged below.

            var filterList = new List<Filter>();

            int numOfUsers = 0;
            int numOfTags = 0;
            bool hasUsers = true;

            try
            {
                MultipleUsers users = await Instagram.SearchUsers(queryText);
                userList = users.data.ToList();
                numOfUsers = users.data.Count();
            }
            catch (Exception)
            {
                Debug.WriteLine("Exception when trying to search for users");
            }

            try
            {
                MultipleTags tags = await Instagram.SearchTags(queryText);
                tagList = tags.data.ToList();
                numOfTags = tags.data.Count();
            }
            catch (Exception)
            {
                Debug.WriteLine("Exception when trying to search for tags");
            }

            hasUsers = numOfUsers > 0;

            filterList.Add(new Filter(USERS, numOfUsers, hasUsers));
            filterList.Add(new Filter(TAGS, numOfTags, !hasUsers));

            // Communicate results through the view model
            this.DefaultViewModel["QueryText"] = '\u201c' + queryText + '\u201d';
            this.DefaultViewModel["Filters"] = filterList;
            this.DefaultViewModel["ShowFilters"] = filterList.Count > 1;
        }

        /// <summary>
        /// Invoked when a filter is selected using the ComboBox in snapped view state.
        /// </summary>
        /// <param name="sender">The ComboBox instance.</param>
        /// <param name="e">Event data describing how the selected filter was changed.</param>
        void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Determine what filter was selected
            var selectedFilter = e.AddedItems.FirstOrDefault() as Filter;
            if (selectedFilter != null)
            {
                // Mirror the results into the corresponding Filter object to allow the
                // RadioButton representation used when not snapped to reflect the change
                selectedFilter.Active = true;
                string vmValue = USER_VM;
                if (selectedFilter.Name.Equals(USERS))
                {
                    vmValue = USER_VM;
                    this.DefaultViewModel[USER_VM] = userList;
                    this.DefaultViewModel["ShowUsers"] = true;
                    this.DefaultViewModel["ShowTags"] = false;
                    
                }
                else if (selectedFilter.Name.Equals(TAGS))
                {
                    vmValue = TAG_VM;
                    this.DefaultViewModel[TAG_VM] = tagList;
                    this.DefaultViewModel["ShowUsers"] = false;
                    this.DefaultViewModel["ShowTags"] = true;
                }
                // TODO: Respond to the change in active filter by setting this.DefaultViewModel["Results"]
                //       to a collection of items with bindable Image, Title, Subtitle, and Description properties

                // Ensure results are found
                object results;
                ICollection resultsCollection;
                if (this.DefaultViewModel.TryGetValue(USER_VM, out results) &&
                    (resultsCollection = results as ICollection) != null &&
                    resultsCollection.Count != 0)
                {
                    VisualStateManager.GoToState(this, "ResultsFound", true);
                    return;
                }
            }

            // Display informational text when there are no search results.
            VisualStateManager.GoToState(this, "NoResultsFound", true);
        }

        /// <summary>
        /// Invoked when a filter is selected using a RadioButton when not snapped.
        /// </summary>
        /// <param name="sender">The selected RadioButton instance.</param>
        /// <param name="e">Event data describing how the RadioButton was selected.</param>
        void Filter_Checked(object sender, RoutedEventArgs e)
        {
            // Mirror the change into the CollectionViewSource used by the corresponding ComboBox
            // to ensure that the change is reflected when snapped
            if (filtersViewSource.View != null)
            {
                var filter = (sender as FrameworkElement).DataContext;
                filtersViewSource.View.MoveCurrentTo(filter);
            }
        }

        /// <summary>
        /// View model describing one of the filters available for viewing search results.
        /// </summary>
        private sealed class Filter : Tabstagram.Common.BindableBase
        {
            private String _name;
            private int _count;
            private bool _active;

            public Filter(String name, int count, bool active = false)
            {
                this.Name = name;
                this.Count = count;
                this.Active = active;
            }

            public override String ToString()
            {
                return Description;
            }

            public String Name
            {
                get { return _name; }
                set { if (this.SetProperty(ref _name, value)) this.OnPropertyChanged("Description"); }
            }

            public int Count
            {
                get { return _count; }
                set { if (this.SetProperty(ref _count, value)) this.OnPropertyChanged("Description"); }
            }

            public bool Active
            {
                get { return _active; }
                set { this.SetProperty(ref _active, value); }
            }

            public String Description
            {
                get { return String.Format("{0} ({1})", _name, _count); }
            }
        }

        private void UserItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(ImagePage), (User)e.ClickedItem);
        }

        private void TagItemClick(object sender, ItemClickEventArgs e)
        {
            HashTag tagViewModel = new HashTag(((Tag)e.ClickedItem).name);
            tagViewModel.Init();
            this.Frame.Navigate(typeof(ListPage), tagViewModel);
        }
    }
}
