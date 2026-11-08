using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using FlowDroid.Animation;

namespace FlowDroid.Controls;

/// <summary>
/// Left navigation rail. Raises <see cref="Navigated"/> with the key of the chosen section and slides
/// a single pill indicator to the active item instead of redrawing it.
/// </summary>
public partial class SidebarNav : UserControl
{
    private RadioButton? _current;
    private bool _ready;

    public SidebarNav()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SizeChanged += (_, _) => SnapIndicator();
    }

    /// <summary>Raised with the Tag of the checked item: home, profiles, apps, files, settings, about.</summary>
    public event EventHandler<string>? Navigated;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateLayout();
        _current ??= HomeItem;
        MoveIndicator(_current, animate: false);
        _ready = true;
    }

    private void NavItem_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton item) return;
        _current = item;
        if (IsLoaded) MoveIndicator(item, animate: _ready);
        if (item.Tag is string key) Navigated?.Invoke(this, key);
    }

    private void SnapIndicator()
    {
        if (_current is not null && IsLoaded) MoveIndicator(_current, animate: false);
    }

    private void MoveIndicator(RadioButton item, bool animate)
    {
        if (!item.IsLoaded) return;

        var target = item.TranslatePoint(new Point(0, 0), OverlayRoot).Y;
        Indicator.Height = item.ActualHeight;

        if (!animate || !Motion.Enabled)
        {
            IndicatorTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, null);
            IndicatorTransform.Y = target;
            return;
        }

        // From = current position, so a click during a running move turns smoothly instead of jumping.
        var move = new DoubleAnimation
        {
            To = target,
            Duration = Motion.Navigation,
            EasingFunction = Motion.Decelerate
        };
        IndicatorTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, move);
    }
}
