﻿using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using MinecraftLaunch;
using MinecraftLaunch.Base.Models.Network;
using YMCL.Public.Classes;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using Setting = YMCL.Public.Enum.Setting;

namespace YMCL.Public.Module.Mc.Installer.InstallJavaClientByMinecraftLauncher;

public class Dispatcher
{
    public static async Task<bool> Install(VersionManifestEntry versionManifestEntry, string? customId = null,
        ForgeInstallEntry? forgeInstallEntry = null, FabricInstallEntry? fabricInstallEntry = null,
        QuiltInstallEntry? quiltBuildEntry = null, OptifineInstallEntry? optiFineInstallEntity = null,
        TaskEntry? p_task = null, bool closeTaskWhenFinish = true)
    {
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var setting = Const.Data.Setting;

        if (optiFineInstallEntity != null || quiltBuildEntry != null || fabricInstallEntry != null ||
            forgeInstallEntry != null)
        {
            var regex = new Regex(@"[\\/:*?""<>|]");
            var matches = regex.Matches(customId ?? versionManifestEntry.Id);
            if (matches.Count > 0)
            {
                var str = string.Empty;
                foreach (Match match in matches) str += match.Value;
                Toast($"{MainLang.IncludeSpecialWord}: {str}", NotificationType.Error);
                return false;
            }

            if (Directory.Exists(Path.Combine(setting.MinecraftFolder.Path, "versions",
                    customId ?? versionManifestEntry.Id)))
            {
                Toast($"{MainLang.FolderAlreadyExists}: {customId ?? versionManifestEntry.Id}", NotificationType.Error);
                return false;
            }
        }

        var mcPath = Data.Setting.MinecraftFolder.Path;

        var task = p_task ?? new TaskEntry($"{MainLang.Install}: {customId}(Minecraft {versionManifestEntry.Id})",
            state: TaskState.Running);
        YMCL.App.UiRoot.Nav.SelectedItem = YMCL.App.UiRoot.NavTask;
        SubTask[] subTasks =
        [
            new(MainLang.CheckVersionResource),
            new($"{MainLang.DownloadResource}(Vanllia)")
        ];
        task.AddSubTaskRange(subTasks);
        task.UpdateAction(() =>
        {
            task.CancelWaitFinish();
            cts.Cancel();
        });

        var forgeTask = new SubTask($"{MainLang.Install}: Forge");
        var optiFineTask = new SubTask($"{MainLang.Install}: OptiFine");
        var fabricTask = new SubTask($"{MainLang.Install}: Fabric");
        var quiltTask = new SubTask($"{MainLang.Install}: Quilt");

        if (forgeInstallEntry != null)
            task.AddSubTask(forgeTask);

        if (optiFineInstallEntity != null)
            task.AddSubTask(optiFineTask);

        if (fabricInstallEntry != null)
            task.AddSubTask(fabricTask);

        if (quiltBuildEntry != null)
            task.AddSubTask(quiltTask);

        var vanllia = await Vanllia.Install(versionManifestEntry, mcPath, task, subTasks[0], subTasks[1],
            cancellationToken);
        if (!vanllia)
        {
            task.FinishWithError();
            return false;
        }

        subTasks[0].Finish();
        subTasks[1].Finish();

        if (forgeInstallEntry != null)
        {
            var forge = await Forge.Install(forgeInstallEntry, customId!, mcPath, forgeTask, task,
                cancellationToken);
            if (!forge)
            {
                task.FinishWithError();
                return false;
            }

            forgeTask.Finish();
        }
        
        if (optiFineInstallEntity != null)
        {
            var optifine = await OptiFine.Install(optiFineInstallEntity, customId!, mcPath, optiFineTask, task,
                cancellationToken);
            if (!optifine)
            {
                task.FinishWithError();
                return false;
            }

            optiFineTask.Finish();
        }
        
        if (fabricInstallEntry != null)
        {
            var fabric = await Fabric.Install(fabricInstallEntry, customId!, mcPath, fabricTask, task,
                cancellationToken);
            if (!fabric)
            {
                task.FinishWithError();
                return false;
            }

            fabricTask.Finish();
        }
        
        if (quiltBuildEntry != null)
        {
            var quilt = await Quilt.Install(quiltBuildEntry, customId!, mcPath, quiltTask, task,
                cancellationToken);
            if (!quilt)
            {
                task.FinishWithError();
                return false;
            }

            quiltTask.Finish();
        }
        
        if (!closeTaskWhenFinish || cancellationToken.IsCancellationRequested) return true;
        task.FinishWithSuccess();
        Toast($"{MainLang.InstallFinish} - {customId ?? versionManifestEntry.Id}", NotificationType.Success);

        return true;
    }
}