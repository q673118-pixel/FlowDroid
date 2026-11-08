using System.Windows;
using System.Windows.Controls;

namespace FlowDroid.Controls;

public partial class TitleBar : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(TitleBar), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ShowMinimizeProperty = DependencyProperty.Register(
        nameof(ShowMinimize), typeof(bool), typeof(TitleBar), new PropertyMetadata(true, OnButtonVisibilityChanged));

    public static readonly DependencyProperty ShowMaximizeProperty = DependencyProperty.Register(
        nameof(ShowMaximize), typeof(bool), typeof(TitleBar), new PropertyMetadata(true, OnButtonVisibilityChanged));

    private Window? _window;

    public TitleBar()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool ShowMinimize
    {
        get => (bool)GetValue(ShowMinimizeProperty);
        set => SetValue(ShowMinimizeProperty, value);
    }

    public bool ShowMaximize
    {
        get => (bool)GetValue(ShowMaximizeProperty);
        set => SetValue(ShowMaximizeProperty, value);
    }

    private static void OnButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((TitleBar)d).UpdateButtons();

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _window = Window.GetWindow(this);
        if (_window is not null) _window.StateChanged += Window_StateChanged;
        UpdateButtons();
    }

    // Detach so the window does not keep this control alive after it is removed.
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_window is not null) _window.StateChanged -= Window_StateChanged;
        _window = null;
    }

    private void Window_StateChanged(object? sender, EventArgs e) => UpdateButtons();

    private void UpdateButtons()
    {
        MinimizeButton.Visibility = ShowMinimize ? Visibility.Visible : Visibility.Collapsed;
        MaximizeButton.Visibility = ShowMaximize ? Visibility.Visible : Visibility.Collapsed;

        var maximized = _window?.WindowState == WindowState.Maximized;
        MaximizeButton.Content = maximized ? "" : "";
        MaximizeButton.ToolTip = maximized ? "Восстановить" : "Развернуть";
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        if (_window is not null) _window.WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        if (_window is null) return;
        _window.WindowState = _window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => _window?.Close();
}
