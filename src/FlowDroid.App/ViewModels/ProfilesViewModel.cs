using System.Collections.ObjectModel;
using System.Windows.Input;
using FlowDroid.Infrastructure;
using FlowDroid.Models;
using FlowDroid.Services;

namespace FlowDroid.ViewModels;

public enum ProfileSort
{
    Name,
    ApkCount
}

public enum ProfileFilter
{
    All,
    WithApks,
    Empty
}

/// <summary>Owns the list of profiles and everything the profile grids need: search, sort, filter, commands.</summary>
public sealed class ProfilesViewModel : ObservableObject
{
    private readonly ProfileStore _store;
    private readonly IDialogService _dialogs;
    private string _searchText = "";
    private ProfileSort _sort = ProfileSort.Name;
    private ProfileFilter _filter = ProfileFilter.All;

    public ProfilesViewModel(ProfileStore store, IDialogService dialogs)
    {
        _store = store;
        _dialogs = dialogs;

        CreateProfileCommand = new RelayCommand(_ => CreateProfile());
        OpenProfileCommand = new RelayCommand(parameter => OpenProfile(parameter as EmulatorProfile));
        DeleteProfileCommand = new RelayCommand(parameter => DeleteProfile(parameter as EmulatorProfile));
        ClearSearchCommand = new RelayCommand(_ => SearchText = "");

        LoadProfiles();
        RefreshTiles();
    }

    /// <summary>All profiles, unfiltered. Source of truth.</summary>
    public ObservableCollection<EmulatorProfile> Profiles { get; } = new();

    /// <summary>What the grid shows: the create tile first, then filtered and sorted profiles.</summary>
    public ObservableCollection<object> Tiles { get; } = new();

    public CreateProfileTile CreateTile { get; } = new();

    public ICommand CreateProfileCommand { get; }
    public ICommand OpenProfileCommand { get; }
    public ICommand DeleteProfileCommand { get; }
    public ICommand ClearSearchCommand { get; }

    public bool HasProfiles => Profiles.Count > 0;
    public bool HasNoProfiles => Profiles.Count == 0;
    public bool ShowNoResults { get; private set; }

    public string ProfileCountText =>
        $"{Profiles.Count} {Plural(Profiles.Count, "профиль", "профиля", "профилей")}";

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (Set(ref _searchText, value ?? "")) RefreshTiles();
        }
    }

    // Two-way friendly flags for the radio chips in the filter popup.
    public bool SortByName
    {
        get => _sort == ProfileSort.Name;
        set { if (value) SetSort(ProfileSort.Name); }
    }

    public bool SortByApkCount
    {
        get => _sort == ProfileSort.ApkCount;
        set { if (value) SetSort(ProfileSort.ApkCount); }
    }

    public bool FilterAll
    {
        get => _filter == ProfileFilter.All;
        set { if (value) SetFilter(ProfileFilter.All); }
    }

    public bool FilterWithApks
    {
        get => _filter == ProfileFilter.WithApks;
        set { if (value) SetFilter(ProfileFilter.WithApks); }
    }

    public bool FilterEmpty
    {
        get => _filter == ProfileFilter.Empty;
        set { if (value) SetFilter(ProfileFilter.Empty); }
    }

    /// <summary>True when sorting or filtering differs from the defaults (used to mark the filter button).</summary>
    public bool IsFilterActive => _sort != ProfileSort.Name || _filter != ProfileFilter.All;

    private void LoadProfiles()
    {
        try
        {
            foreach (var profile in _store.Load())
            {
                profile.Apks ??= new List<ApkEntry>();
                Profiles.Add(profile);
            }

            if (_store.LastLoadWarning is { } warning)
                _dialogs.ShowMessage("Профили", warning, isError: true);
        }
        catch (Exception ex)
        {
            _dialogs.ShowMessage("FlowDroid", $"Не удалось загрузить профили.\n\n{ex.Message}", isError: true);
        }
    }

    private void CreateProfile()
    {
        var result = _dialogs.ShowCreateProfile();
        if (result is null) return;

        if (Profiles.Any(p => string.Equals(p.Name, result.Name, StringComparison.OrdinalIgnoreCase)))
        {
            _dialogs.ShowMessage("FlowDroid", "Профиль с таким названием уже существует.");
            return;
        }

        Profiles.Add(new EmulatorProfile { Name = result.Name, AndroidVersion = result.AndroidVersion });
        SaveProfiles();
        RefreshTiles();
    }

    private void OpenProfile(EmulatorProfile? profile)
    {
        if (profile is null) return;
        _dialogs.ShowProfile(profile);
        SaveProfiles();
        RefreshTiles();
    }

    private void DeleteProfile(EmulatorProfile? profile)
    {
        if (profile is null) return;
        if (!_dialogs.Confirm("Удаление профиля",
                $"Удалить профиль «{profile.Name}» вместе со всеми импортированными APK-файлами?"))
            return;

        try
        {
            _store.DeleteProfileData(profile.Id);
            Profiles.Remove(profile);
            SaveProfiles();
            RefreshTiles();
        }
        catch (Exception ex)
        {
            _dialogs.ShowMessage("FlowDroid", $"Не удалось полностью удалить данные профиля.\n{ex.Message}", isError: true);
        }
    }

    private void SaveProfiles()
    {
        try { _store.Save(Profiles); }
        catch (Exception ex)
        {
            _dialogs.ShowMessage("FlowDroid", $"Не удалось сохранить профили.\n\n{ex.Message}", isError: true);
        }
    }

    private void SetSort(ProfileSort sort)
    {
        if (_sort == sort) return;
        _sort = sort;
        OnPropertyChanged(nameof(SortByName));
        OnPropertyChanged(nameof(SortByApkCount));
        OnPropertyChanged(nameof(IsFilterActive));
        RefreshTiles();
    }

    private void SetFilter(ProfileFilter filter)
    {
        if (_filter == filter) return;
        _filter = filter;
        OnPropertyChanged(nameof(FilterAll));
        OnPropertyChanged(nameof(FilterWithApks));
        OnPropertyChanged(nameof(FilterEmpty));
        OnPropertyChanged(nameof(IsFilterActive));
        RefreshTiles();
    }

    private void RefreshTiles()
    {
        IEnumerable<EmulatorProfile> query = Profiles;

        var text = _searchText.Trim();
        if (text.Length > 0)
            query = query.Where(p => p.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase));

        query = _filter switch
        {
            ProfileFilter.WithApks => query.Where(p => p.InstalledApkCount > 0),
            ProfileFilter.Empty => query.Where(p => p.InstalledApkCount == 0),
            _ => query
        };

        query = _sort == ProfileSort.ApkCount
            ? query.OrderByDescending(p => p.InstalledApkCount).ThenBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
            : query.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase);

        var visible = query.ToList();
        var desired = new List<object>(visible.Count + 1) { CreateTile };
        desired.AddRange(visible);
        SyncTiles(desired);

        ShowNoResults = Profiles.Count > 0 && visible.Count == 0;
        OnPropertyChanged(nameof(ShowNoResults));
        OnPropertyChanged(nameof(HasProfiles));
        OnPropertyChanged(nameof(HasNoProfiles));
        OnPropertyChanged(nameof(ProfileCountText));
    }

    /// <summary>Turns <see cref="Tiles"/> into <paramref name="desired"/> with the fewest changes, so cards that stay are not rebuilt.</summary>
    private void SyncTiles(IReadOnlyList<object> desired)
    {
        for (var i = 0; i < desired.Count; i++)
        {
            if (i < Tiles.Count && ReferenceEquals(Tiles[i], desired[i])) continue;

            var existing = -1;
            for (var j = i + 1; j < Tiles.Count; j++)
            {
                if (ReferenceEquals(Tiles[j], desired[i])) { existing = j; break; }
            }

            if (existing >= 0) Tiles.Move(existing, i);
            else Tiles.Insert(i, desired[i]);
        }

        while (Tiles.Count > desired.Count) Tiles.RemoveAt(Tiles.Count - 1);
    }

    private static string Plural(int count, string one, string few, string many)
    {
        var lastTwo = count % 100;
        if (lastTwo is >= 11 and <= 14) return many;
        return (count % 10) switch
        {
            1 => one,
            2 or 3 or 4 => few,
            _ => many
        };
    }
}
