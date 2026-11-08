using System.Windows;
using FlowDroid.Services;

namespace FlowDroid;

public partial class RuntimeDiagnosticsWindow : Window
{
    private readonly AndroidSdkService _android;

    public RuntimeDiagnosticsWindow(AndroidSdkService android)
    {
        InitializeComponent();
        WindowAppearance.Apply(this);
        _android = android;
        Loaded += async (_, _) => await LoadReportAsync();
    }

    private async Task LoadReportAsync()
    {
        ReportBox.Text = "Сбор диагностических данных…";
        try
        {
            ReportBox.Text = await _android.GetDiagnosticsAsync();
        }
        catch (Exception ex)
        {
            ReportBox.Text = "Не удалось собрать полный отчёт.\n\n" + ex;
        }
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(ReportBox.Text);
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}