using System.Windows;
using System.Windows.Controls;

namespace FlowDroid.Views;

public partial class HomeView : UserControl
{
    private long _popupClosedAt;

    public HomeView()
    {
        InitializeComponent();
        FilterPopup.Closed += (_, _) => _popupClosedAt = Environment.TickCount64;
    }

    // With StaysOpen=False the popup closes on mouse-down, before Click. Without this guard
    // the same click would immediately reopen it.
    private void FilterButton_Click(object sender, RoutedEventArgs e)
    {
        if (FilterPopup.IsOpen)
        {
            FilterPopup.IsOpen = false;
            return;
        }

        if (Environment.TickCount64 - _popupClosedAt < 250) return;
        FilterPopup.IsOpen = true;
    }
}
