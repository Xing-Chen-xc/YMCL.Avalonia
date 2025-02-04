﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using MinecraftLaunch.Classes.Models.Game;
using YMCL.Public.Classes;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using SearchOption = System.IO.SearchOption;

namespace YMCL.Views.Main.Pages.LaunchPages.SubPages;

public sealed partial class Mod : UserControl, INotifyPropertyChanged
{
    private readonly ObservableCollection<LocalModEntry> _mods = [];
    public ObservableCollection<LocalModEntry> FilteredMods { get; set; } = [];
    private readonly GameEntry _entry;
    private string _filter = string.Empty;

    public Mod(GameEntry entry)
    {
        _entry = entry;
        InitializeComponent();
        LoadMods();
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Filter))
            {
                FilterMods();
            }
        };
        DataContext = this;
        RefreshModBtn.Click += (_, _) => { LoadMods(); };
        Loaded += (_, _) => { LoadMods(); };
        DeselectAllModBtn.Click += (_, _) => { ModManageList.SelectedIndex = -1; };
        SelectAllModBtn.Click += (_, _) => { ModManageList.SelectAll(); };
        DisableSelectModBtn.Click += (_, _) =>
        {
            var mods = ModManageList.SelectedItems;
            foreach (var item in mods)
            {
                var mod = item as LocalModEntry;
                if (mod.Name.Length <= 0) continue;
                if (Path.GetExtension(mod.Path) == ".jar")
                    File.Move(mod.Path, mod.Path + ".disabled");
            }

            LoadMods();
        };
        EnableSelectModBtn.Click += (_, _) =>
        {
            var mods = ModManageList.SelectedItems;
            foreach (var item in mods)
            {
                var mod = item as LocalModEntry;
                if (mod.Name.Length <= 0) continue;
                if (string.IsNullOrWhiteSpace(Path.GetDirectoryName(mod.Path))) continue;
                if (Path.GetExtension(mod.Path) == ".disabled")
                    File.Move(mod.Path, Path.Combine(Path.GetDirectoryName(mod.Path)!, $"{mod.Name}.jar"));
            }

            LoadMods();
        };
        DeleteSelectModBtn.Click += async (_, _) =>
        {
            var mods = ModManageList.SelectedItems;
            if (mods is null) return;
            var text = (from object? item in mods select item as LocalModEntry).Aggregate(string.Empty,
                (current, mod) => current + $"• {Path.GetFileName(mod.Name)}\n");

            var title = YMCL.Public.Const.Data.DesktopType == DesktopRunnerType.Windows
                ? MainLang.MoveToRecycleBin
                : MainLang.DeleteSelect;
            var dialog = await ShowDialogAsync(title, text, b_cancel: MainLang.Cancel,
                b_primary: MainLang.Ok);
            if (dialog != ContentDialogResult.Primary) return;

            foreach (var item in mods)
            {
                var mod = item as LocalModEntry;
                if (YMCL.Public.Const.Data.DesktopType == DesktopRunnerType.Windows)
                {
                    FileSystem.DeleteFile(mod.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                }
                else
                {
                    File.Delete(mod.Path);
                }
            }

            LoadMods();
        };
        ModManageList.SelectionChanged += (_, _) =>
        {
            SelectedModCount.Text = $"{MainLang.SelectedItem} {ModManageList.SelectedItems.Count}";
        };
        SelectedModCount.Text = $"{MainLang.SelectedItem} 0";
    }

    public string Filter
    {
        get => _filter;
        set => SetField(ref _filter, value);
    }

    private void LoadMods()
    {
        _mods.Clear();

        var mods = Directory.GetFiles(
            Public.Module.Mc.GameSetting.GetGameSpecialFolder(_entry, GameSpecialFolder.ModsFolder)
            , "*.*", SearchOption.AllDirectories);
        foreach (var mod in mods)
        {
            if (Path.GetExtension(mod) == ".jar")
                _mods.Add(new LocalModEntry
                {
                    Name = Path.GetFileName(mod)[..(Path.GetFileName(mod).Length - 4)],
                    IsEnable = true, Path = mod, Callback = () => { LoadMods(); }
                });
            if (Path.GetExtension(mod) == ".disabled")
                _mods.Add(new LocalModEntry
                {
                    Name = Path.GetFileName(mod)[..(Path.GetFileName(mod).Length - 13)],
                    IsEnable = false, Path = mod, Callback = () => { LoadMods(); }
                });
        }

        FilterMods();
    }

    private void FilterMods()
    {
        FilteredMods.Clear();
        _mods.Where(item => item.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .ToList().OrderBy(mod => mod.IsEnable).ToList().ForEach(mod => FilteredMods.Add(mod));
        NoMatchResultTip.IsVisible = FilteredMods.Count == 0;
        SelectedModCount.Text = $"{MainLang.SelectedItem} {ModManageList.SelectedItems.Count}";
    }


    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }
}