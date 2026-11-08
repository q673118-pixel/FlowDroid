using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using FlowDroid.Models;
using FlowDroid.Services;

namespace FlowDroid;

public sealed class AppCatalogItem
{
    public required EmulatorProfile Profile { get; init; }
    public required ApkEntry Apk { get; init; }
    public string FileName => Apk.FileName;
    public string ProfileName => Profile.Name;
    public string SizeLabel => Apk.SizeBytes < 1024 * 1024
        ? $"{Apk.SizeBytes / 1024d:N0} КБ"
        : $"{Apk.SizeBytes / (1024d * 1024d):N1} МБ";
}

public partial class AppsWindow : Window
{
    private readonly ProfileStore _store = new();
    private readonly ObservableCollection<AppCatalogItem> _items = new();

    public AppsWindow()
    {
        InitializeComponent();
        WindowAppearance.Apply(this);
        AppsList.ItemsSource = _items;
        LoadItems();
        Loaded += (_, _) => MotionService.Show(EmptyState.Visibility == Visibility.Visible ? (UIElement)EmptyState : AppsList, 190, 8);
    }

    private void LoadItems()
    {
        _items.Clear();
        foreach (var profile in _store.Load())
        {
            foreach (var apk in profile.Apks ?? new List<ApkEntry>())
                if (File.Exists(apk.StoredPath))
                    _items.Add(new AppCatalogItem { Profile = profile, Apk = apk });
        }
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = SearchBox.Text.Trim();
        var view = System.Windows.Data.CollectionViewSource.GetDefaultView(_items);
        view.Filter = item => item is AppCatalogItem app &&
            (string.IsNullOrWhiteSpace(query) ||
             app.FileName.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
             app.ProfileName.Contains(query, StringComparison.CurrentCultureIgnoreCase));
        CountText.Text = $"{view.Cast<object>().Count()} приложений";
        var empty = view.Cast<object>().Count() == 0;
        EmptyState.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;
        EmptyTitle.Text = _items.Count == 0 ? "Библиотека пока пуста" : "Ничего не найдено";
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void OpenProfile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: AppCatalogItem item }) return;
        var window = new ProfileWindow(item.Profile) { Owner = this };
        window.ShowDialog();
        LoadItems();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}