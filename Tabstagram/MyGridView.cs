using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tabstagram
{
    class MyGridView : GridView
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            Media media = item as Media;

            if (media != null && media.IsImportant)
            {
                VariableSizedWrapGrid.SetRowSpan(element as UIElement, 2);
                VariableSizedWrapGrid.SetColumnSpan(element as UIElement, 2);
            }
            else
            {
                VariableSizedWrapGrid.SetRowSpan(element as UIElement, 1);
                VariableSizedWrapGrid.SetColumnSpan(element as UIElement, 1);
            }
        }
    }
}
