using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tabstagram
{
    class MyGridView : GridView
    {
        Random rand = new Random();

        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            Media media = item as Media;

            if (media.IsImportant)
            {
                int colVal = 2;
                int rowVal = 2;

                VariableSizedWrapGrid.SetRowSpan(element as UIElement, rowVal);
                VariableSizedWrapGrid.SetColumnSpan(element as UIElement, colVal);
            }
        }
    }
}
