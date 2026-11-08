using System.Windows;
using System.Windows.Controls;

namespace FlowDroid;

public partial class CreateProfileWindow : Window
{
    public string ProfileName => ProfileNameBox.Text;
    public string AndroidVersion => (AndroidVersionBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Android 14";

    public CreateProfileWindow()
    {
        InitializeComponent();
        ProfileNameBox.Focus();
        ProfileNameBox.SelectAll();
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProfileNameBox.Text))
        {
            MessageBox.Show("Введи название профиля.", "FlowDroid", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void CloseWindow_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
