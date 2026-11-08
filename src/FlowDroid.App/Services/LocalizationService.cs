using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlowDroid.Services;

public static class LocalizationService
{
    private static readonly Dictionary<string, string> English = new(StringComparer.Ordinal)
    {
        ["Главная"] = "Home", ["Профили"] = "Profiles", ["Приложения"] = "Applications",
        ["Файлы"] = "Files", ["Настройки"] = "Settings", ["О программе"] = "About",
        ["Ваши профили"] = "Your profiles", ["Android-эмулятор для Windows"] = "Android emulator for Windows",
        ["Создать профиль"] = "Create profile", ["Открыть профиль"] = "Open profile",
        ["У вас пока нет профилей"] = "No profiles yet", ["Создайте профиль, чтобы запускать\nсвои игры и приложения"] = "Create a profile to run\nyour games and apps",
        ["Ничего не найдено"] = "No results found", ["Измените поисковый запрос и попробуйте снова."] = "Change your search and try again.",
        ["Поиск профилей"] = "Search profiles", ["Сортировка профилей"] = "Sort profiles",
        ["Настройки FlowDroid"] = "FlowDroid settings",
        ["Общие параметры приложения и значения по умолчанию для новых профилей."] = "Application preferences and defaults for new profiles.",
        ["Новые профили"] = "New profiles", ["Оперативная память по умолчанию (ГБ)"] = "Default memory (GB)",
        ["Количество ядер CPU по умолчанию"] = "Default CPU cores",
        ["Целевая частота кадров по умолчанию"] = "Default target frame rate",
        ["Поведение"] = "Behavior", ["Запрашивать подтверждение перед удалением профиля"] = "Confirm before deleting a profile",
        ["Внешний вид"] = "Appearance", ["Темы, цвета и анимации"] = "Themes, colors and animations",
        ["Светлая, тёмная и системная темы, шесть вариантов акцента."] = "Light, dark and system themes, with six accent colors.",
        ["Открыть"] = "Open", ["Хранилище"] = "Storage", ["Папка данных FlowDroid"] = "FlowDroid data folder",
        ["Открыть папку данных"] = "Open data folder", ["Отмена"] = "Cancel",
        ["Сохранить настройки"] = "Save settings", ["Закрыть"] = "Close",
        ["Внешний вид FlowDroid"] = "FlowDroid appearance",
        ["Настройте тему и поведение интерфейса FlowDroid."] = "Customize the FlowDroid theme and interface behavior.",
        ["Режим оформления"] = "Theme mode", ["Тёмная"] = "Dark", ["Светлая"] = "Light",
        ["Системная Windows"] = "Windows system", ["Цвет акцента"] = "Accent color",
        ["Движение интерфейса"] = "Interface motion", ["Использовать анимации"] = "Enable animations",
        ["Отключение убирает декоративные переходы и сокращает визуальные задержки."] = "Disabling this removes decorative transitions and reduces visual delays.",
        ["Применить"] = "Apply", ["Темы, цвета и анимации"] = "Themes, colors and animations",
        ["Имя профиля"] = "Profile name", ["Версия Android"] = "Android version",
        ["Настройки профиля"] = "Profile settings", ["Применить настройки"] = "Apply settings",
        ["Установить и запустить"] = "Install and launch", ["Импортировать APK"] = "Import APK",
        ["Удалить"] = "Delete", ["Запустить"] = "Launch", ["Остановить"] = "Stop",
        ["Диагностика"] = "Diagnostics", ["Копировать отчёт"] = "Copy report",
        ["Разрешение"] = "Resolution", ["Плотность DPI"] = "DPI density",
        ["Ориентация"] = "Orientation", ["Производительность"] = "Performance",
        ["Графический рендерер"] = "Graphics renderer", ["Оперативная память"] = "Memory",
        ["Количество ядер CPU"] = "CPU cores", ["Частота кадров"] = "Frame rate",
        ["Автоматически"] = "Automatic", ["Книжная"] = "Portrait", ["Альбомная"] = "Landscape"
    };

    public static string Translate(string value, string language)
    {
        if (language == "English")
            return English.TryGetValue(value, out var translated) ? translated : value;
        foreach (var pair in English)
            if (pair.Value == value) return pair.Key;
        return value;
    }

    public static void ApplyToOpenWindows()
    {
        if (Application.Current is null) return;
        foreach (Window window in Application.Current.Windows)
            Apply(window);
    }

    public static void Apply(Window window)
    {
        if (window is null) return;
        var language = ThemeService.Current.Language;
        window.Title = Translate(window.Title, language);
        Visit(window, language);
    }

    private static void Visit(DependencyObject node, string language)
    {
        switch (node)
        {
            case TextBlock text:
                text.Text = Translate(text.Text, language);
                break;
            case ContentControl content when content.Content is string value:
                content.Content = Translate(value, language);
                break;
            case HeaderedContentControl headered when headered.Header is string header:
                headered.Header = Translate(header, language);
                break;
        }

        if (node is FrameworkElement element && element.ToolTip is string tip)
            element.ToolTip = Translate(tip, language);

        var count = VisualTreeHelper.GetChildrenCount(node);
        for (var i = 0; i < count; i++)
            Visit(VisualTreeHelper.GetChild(node, i), language);
    }
}
