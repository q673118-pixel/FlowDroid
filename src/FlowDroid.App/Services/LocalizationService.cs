using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlowDroid.Services;

public static class LocalizationService
{
    private static readonly Dictionary<string, string> English = new(StringComparer.Ordinal)
    {
        ["Главная"] = "Home", ["Профили"] = "Profiles",
        ["Приложения"] = "Applications", ["Файлы"] = "Files",
        ["Настройки"] = "Settings", ["О программе"] = "About",
        ["Ваши профили"] = "Your profiles", ["Android-эмулятор для Windows"] = "Android emulator for Windows",
        ["Создать профиль"] = "Create profile", ["Открыть профиль"] = "Open profile",
        ["У вас пока нет профилей"] = "No profiles yet", ["Создайте профиль, чтобы запускать\nсвои игры и приложения"] = "Create a profile to run\nyour games and apps",
        ["Ничего не найдено"] = "No results found", ["Измените поисковый запрос и попробуйте снова."] = "Change your search and try again.",
        ["Поиск профилей"] = "Search profiles", ["Сортировка профилей"] = "Sort profiles",
        ["Настройки FlowDroid"] = "FlowDroid settings", ["Настройки — FlowDroid"] = "Settings — FlowDroid", ["Общие параметры приложения и значения по умолчанию для новых профилей."] = "Application preferences and defaults for new profiles.",
        ["Новые профили"] = "New profiles", ["Оперативная память по умолчанию (ГБ)"] = "Default memory (GB)",
        ["Количество ядер CPU по умолчанию"] = "Default CPU cores", ["Целевая частота кадров по умолчанию"] = "Default target frame rate",
        ["Поведение"] = "Behavior", ["Запрашивать подтверждение перед удалением профиля"] = "Confirm before deleting a profile",
        ["Внешний вид"] = "Appearance", ["Темы, цвета и анимации"] = "Themes, colors and animations",
        ["Светлая, тёмная и системная темы, шесть вариантов акцента."] = "Light, dark and system themes, with six accent colors.", ["Открыть"] = "Open",
        ["Хранилище"] = "Storage", ["Папка данных FlowDroid"] = "FlowDroid data folder",
        ["Открыть папку данных"] = "Open data folder", ["Отмена"] = "Cancel",
        ["Сохранить настройки"] = "Save settings", ["Закрыть"] = "Close",
        ["Внешний вид FlowDroid"] = "FlowDroid appearance", ["Внешний вид — FlowDroid"] = "Appearance — FlowDroid", ["Настройте тему и поведение интерфейса FlowDroid."] = "Customize the FlowDroid theme and interface behavior.",
        ["Режим оформления"] = "Theme mode", ["Тёмная"] = "Dark",
        ["Светлая"] = "Light", ["Системная Windows"] = "Windows system",
        ["Цвет акцента"] = "Accent color", ["Движение интерфейса"] = "Interface motion",
        ["Использовать анимации"] = "Enable animations", ["Отключение убирает декоративные переходы и сокращает визуальные задержки."] = "Disabling this removes decorative transitions and reduces visual delays.",
        ["Применить"] = "Apply", ["Медленно"] = "Slow",
        ["Обычно"] = "Normal", ["Быстро"] = "Fast",
        ["Язык интерфейса"] = "Interface language", ["Переключение языка приложения."] = "Change the application language.",
        ["Шрифт приложения"] = "Application font", ["Выберите гарнитуру для всего интерфейса."] = "Choose a typeface for the entire interface.",
        ["Предпросмотр шрифта"] = "Font preview", ["Скорость анимаций"] = "Animation speed",
        ["Настройте темп переходов и появления элементов."] = "Adjust the pace of transitions and element entrances.", ["Имя профиля"] = "Profile name",
        ["Версия Android"] = "Android version", ["Настройки профиля"] = "Profile settings",
        ["Применить настройки"] = "Apply settings", ["Установить и запустить"] = "Install and launch",
        ["Импортировать APK"] = "Import APK", ["Удалить"] = "Delete",
        ["Запустить"] = "Launch", ["Остановить"] = "Stop",
        ["Диагностика"] = "Diagnostics", ["Копировать отчёт"] = "Copy report",
        ["Разрешение"] = "Resolution", ["Плотность DPI"] = "DPI density",
        ["Ориентация"] = "Orientation", ["Производительность"] = "Performance",
        ["Графический рендерер"] = "Graphics renderer", ["Оперативная память"] = "Memory",
        ["Количество ядер CPU"] = "CPU cores", ["Частота кадров"] = "Frame rate",
        ["Автоматически"] = "Automatic", ["Книжная"] = "Portrait",
        ["Альбомная"] = "Landscape", ["Приложения — FlowDroid"] = "Applications — FlowDroid",
        ["Единая библиотека APK из всех профилей FlowDroid."] = "A unified APK library from all FlowDroid profiles.", ["Поиск по имени APK и профилю"] = "Search by APK name or profile",
        ["Библиотека пока пуста"] = "The library is empty", ["Импортируй APK в любом профиле — он появится здесь."] = "Import an APK in any profile and it will appear here.",
        ["FlowDroid  /  Новый профиль"] = "FlowDroid  /  New profile", ["Отдельная Android-среда со своими приложениями и параметрами."] = "A separate Android environment with its own apps and settings.",
        ["Название профиля"] = "Profile title", ["Мой профиль"] = "My profile",
        ["Файлы — FlowDroid"] = "Files — FlowDroid", ["Импортированные APK и данные профилей на этом компьютере."] = "Imported APKs and profile data on this computer.",
        ["Показать в папке"] = "Show in folder", ["Открыть APK"] = "Open APK",
        ["Пока нет импортированных файлов"] = "No imported files yet", ["APK появятся здесь после импорта в профиль."] = "APKs will appear here after importing them into a profile.",
        ["FlowDroid  /  Настройки профиля"] = "FlowDroid  /  Profile settings", ["Параметры сохраняются отдельно для этой Android-среды."] = "Settings are saved separately for this Android environment.",
        ["Экран и визуал"] = "Display and visuals", ["Разрешение экрана"] = "Screen resolution",
        ["Пользовательское"] = "Custom", ["Например: 1920x1080"] = "For example: 1920x1080",
        ["Плотность пикселей (DPI)"] = "Pixel density (DPI)", ["Ориентация экрана"] = "Screen orientation",
        ["Выделенная оперативная память (ГБ)"] = "Allocated memory (GB)", ["Целевая частота кадров (FPS)"] = "Target frame rate (FPS)",
        ["Аппаратный"] = "Hardware", ["Программный"] = "Software",
        ["Параметры среды"] = "Runtime settings", ["RAM, CPU и графический режим применяются при старте эмулятора. Разрешение, DPI и ориентация задаются после загрузки Android. Частота FPS применяется к отдельному окну приложения через scrcpy."] = "RAM, CPU and graphics mode are applied when the emulator starts. Resolution, DPI and orientation are set after Android boots. FPS is applied to the separate app window through scrcpy.",
        ["Сохранить"] = "Save", ["Профиль — FlowDroid"] = "Profile — FlowDroid",
        ["FlowDroid  /  Управление профилем"] = "FlowDroid  /  Profile workspace", ["+  Добавить APK"] = "+  Add APK",
        ["Проверка Android Runtime…"] = "Checking Android Runtime…", ["Обновить"] = "Refresh",
        ["Запустить среду"] = "Start runtime", ["Настроить автоматически"] = "Set up automatically",
        ["Импортированные APK"] = "Imported APKs", ["Установить и запустить выбранный APK"] = "Install and launch selected APK",
        ["В этом профиле пока нет APK"] = "No APKs in this profile yet", ["Добавь APK-файлы с компьютера, чтобы сохранить их в профиле."] = "Add APK files from your computer to store them in this profile.",
        ["Диагностика Android Runtime — FlowDroid"] = "Android Runtime diagnostics — FlowDroid", ["Диагностика Runtime"] = "Runtime diagnostics",
        ["Проверка SDK, ADB, подключённых устройств и аппаратного ускорения."] = "Check the SDK, ADB, connected devices and hardware acceleration.",
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
        }

        if (node is FrameworkElement element && element.ToolTip is string tip)
            element.ToolTip = Translate(tip, language);

        var count = VisualTreeHelper.GetChildrenCount(node);
        for (var i = 0; i < count; i++)
            Visit(VisualTreeHelper.GetChild(node, i), language);
    }
}
