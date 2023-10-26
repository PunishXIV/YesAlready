using Dalamud.Game.Command;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ECommons.Logging;
using Dalamud.Plugin;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using ECommons.Reflection;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.Loader
{
    public static class PluginLoader
    {
        internal static bool IsUsed = false;
        public static bool IsLoaded { get; internal set; } = false;

        private static Action Success = null;
        private static string PluginLoadCommand = null;
        private static ILoadable Plugin;

        static Vector2 Pos = Vector2.Zero;

        public static void Init(ILoadable plugin, DalamudPluginInterface pluginInterface, string blacklistURL, Action success)
        {
            if (IsUsed)
            {
                throw new Exception($"Loader has been already called before");
            }
            Plugin = plugin;
            IsUsed = true;
            pluginInterface.Create<MicroServices>();
            Success = success;
            PluginLoadCommand = $"/load_{MicroServices.PluginInterface.InternalName}";
            var cInfo = new CommandInfo(Load) { HelpMessage = $"Load plugin skipping version check" };
            GenericHelpers.Safe(delegate
            {
                cInfo.SetFoP("LoaderAssemblyName", MicroServices.PluginInterface.InternalName);
            });
            MicroServices.Commands.AddHandler(PluginLoadCommand, cInfo);

            var currentVersion = AssemblyName.GetAssemblyName(MicroServices.PluginInterface.AssemblyLocation.FullName).Version ?? new Version(0,0,0,0);

            Task.Run(delegate
            {
                var verdict = true;
                try
                {
                    HttpClient client = new()
                    {
                        Timeout = TimeSpan.FromSeconds(5),
                    };
                    var res = client.GetAsync(blacklistURL).Result;
                    res.EnsureSuccessStatusCode();
                    using var stream = res.Content.ReadAsStream();
                    using var streamReader = new StreamReader(stream);
                    var lines = streamReader.ReadToEnd().Split("\n");


                    foreach(var x in lines)
                    {
                        try
                        {
                            PluginLog.Debug($"Parsing line {x}");
                            var versions = x.Split(",").Select(z => z.Trim()).Select(z => Version.Parse(z)).ToArray();
                            if(versions.Length == 1)
                            {
                                if(currentVersion == versions[0])
                                {
                                    verdict = false;
                                    PluginLog.Debug($"Blacklisted current version from line {x} (match {versions[0]})");
                                }
                            }
                            else if(versions.Length > 1)
                            {
                                if (currentVersion >= versions[0] && currentVersion <= versions[1])
                                {
                                    verdict = false;
                                    PluginLog.Debug($"Blacklisted current version from line {x} (match {versions[0]}-{versions[1]})");
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            PluginLog.Debug($"Invalid line received: {x}");
                            PluginLog.Debug($"{e.Message} \n{e.StackTrace}");
                        }
                    }

                }
                catch(Exception e)
                {
                    PluginLog.Error($"Error during loader call");
                    e.Log();
                }
                if (verdict)
                {
                    MicroServices.Framework.Update += CallLoad;
                }
                else
                {
                    MicroServices.PluginInterface.UiBuilder.Draw += Draw;
                }
            });
        }

        private static void Load(object _, object __)
        {
            if (Plugin.IsDisposed)
            {
                PluginLog.Error($"Plugin is already disposed, won't call load");
                return;
            }
            if (IsLoaded)
            {
                PluginLog.Error($"Loading already called before, won't load again");
            }
            else 
            {
                try
                {
                    Success();
                }
                catch (Exception e)
                {
                    e.Log();
                }
            }
        }

        private static void CallLoad(object _)
        {
            if (Plugin.IsDisposed)
            {
                PluginLog.Error($"Plugin is already disposed, won't call load");
                return;
            } 
            Load(null, null);
            Dispose();
        }

        public static void Dispose()
        {
            if (IsUsed)
            {
                MicroServices.Framework.Update -= CallLoad;
                MicroServices.PluginInterface.UiBuilder.Draw -= Draw;
                if (PluginLoadCommand != null) MicroServices.Commands.RemoveHandler(PluginLoadCommand);
                Plugin = null;
            }
        }

        private static void Draw()
        {
            if (Plugin.IsDisposed)
            {
                PluginLog.Error($"Plugin is already disposed, won't show UI");
                Dispose();
                return;
            }
            ImGui.SetNextWindowSizeConstraints(new(500, 100), new(500, 500));
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Pos);
            ImGuiHelpers.ForceNextWindowMainViewport();
            if (ImGui.Begin($"{MicroServices.PluginInterface.InternalName} - version revoked", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings))
            {
                ImGuiEx.TextWrapped("Current version of the plugin has been marked as revoked.");
                ImGuiEx.TextWrapped("This means that there is an issue that can cause problems. Usually, updated version either already available or will be available as soon as possible. ");
                if (ImGui.Button("Open plugin installer"))
                {
                    MicroServices.PluginInterface.OpenPluginInstaller();
                }
                if (ImGui.Button("Close this window"))
                {
                    MicroServices.PluginInterface.UiBuilder.Draw -= Draw;
                }
                if(ImGui.Button("Load the plugin anyway"))
                {
                    MicroServices.Framework.Update += CallLoad;
                }
            }
            Pos = ImGuiHelpers.MainViewport.Size / 2 - ImGui.GetWindowSize() / 2;
            ImGui.End();
        }
    }
}
