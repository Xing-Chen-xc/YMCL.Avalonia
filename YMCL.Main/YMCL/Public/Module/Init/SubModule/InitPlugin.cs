﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using YMCL.Plugin.Base;
using YMCL.Public.Classes;
using YMCL.Public.Controls;
using YMCL.Public.Langs;
using YMCL.Public.Module.IO.Disk;
using YMCL.Public.Plugin;

namespace YMCL.Public.Module.Init.SubModule;

public class InitPlugin
{
    public static void Dispatch()
    {
        ScanPlugin();
        ExecuteEnablePlugin();
    }

    private static void ExecuteEnablePlugin()
    {
        foreach (var pluginInfoEntry in Data.IdentifiedPlugins)
        {
            try
            {
                if (pluginInfoEntry.IsEnable)
                {
                    pluginInfoEntry.Plugin.Execute(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                ShowShortException(MainLang.LoadPluginError ,e);
            }
        }
    }

    private static void ScanPlugin()
    {
        var dlls = Getter.GetAllFilesByExtension(ConfigPath.PluginFolderPath, "*.dll");
        dlls.ForEach(dll =>
        {
            try
            {
                var pluginAssembly = Loader.LoadPlugin(dll);
                var plugins = Loader.CreateCommands(pluginAssembly);
                foreach (var plugin in plugins)
                {
                    if (Data.IdentifiedPlugins.All(x => x.Path != dll))
                    {
                        Data.IdentifiedPlugins.Add(new PluginInfoEntry(plugin, dll , Data.EnablePlugins.Contains(dll)));
                    }
                }
            }
            catch
            {
            }
        });
    }
}