using ECommons.Logging;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ECommons.Configuration;

public static class EzConfig
{
    const string DefaultConfigurationName = "DefaultConfig.json";
    public static string DefaultConfigurationFileName => Path.Combine(Svc.PluginInterface.GetPluginConfigDirectory(), DefaultConfigurationName);
    public static IEzConfig Config { get; private set; }

    public static T Init<T>() where T : IEzConfig, new()
    {
        Config = LoadConfiguration<T>(DefaultConfigurationName);
        return (T)Config;
    }

    public static void Migrate<T>() where T : IEzConfig, new()
    {
        if(Config != null)
        {
            throw new NullReferenceException("Migrate must be called instead of initialization");
        }
        var path = DefaultConfigurationFileName;
        if(!File.Exists(path) && Svc.PluginInterface.ConfigFile.Exists)
        {
            PluginLog.Warning($"Migrating {Svc.PluginInterface.ConfigFile} into EzConfig system");
            Config = LoadConfiguration<T>(Svc.PluginInterface.ConfigFile.FullName, false);
            Save();
            Config = null;
            File.Move(Svc.PluginInterface.ConfigFile.FullName, $"{Svc.PluginInterface.ConfigFile}.old");
        }
        else
        {
            PluginLog.Information($"Migrating conditions are not met, skipping...");
        }
    }

    public static void Save()
    {
        if (Config != null)
        {
            SaveConfiguration(Config, DefaultConfigurationName, true);
        }
    }

    public static void SaveConfiguration(this IEzConfig Configuration, string path, bool indented = false, bool appendConfigDirectory = true)
    {
        if (appendConfigDirectory) path = Path.Combine(Svc.PluginInterface.GetPluginConfigDirectory(), path);
        var antiCorruptionPath = $"{path}.new";
        if (File.Exists(antiCorruptionPath))
        {
            var saveTo = $"{antiCorruptionPath}.{DateTimeOffset.Now.ToUnixTimeMilliseconds()}";
            PluginLog.Warning($"Detected unsuccessfully saved file {antiCorruptionPath}: moving to {saveTo}");
            Notify.Warning("Detected unsuccessfully saved configuration file.");
            File.Move(antiCorruptionPath, saveTo);
            PluginLog.Warning($"Success. Please manually check {saveTo} file contents.");
        }
        PluginLog.Verbose($"From caller {new StackTrace().GetFrames().Select(x => x.GetMethod()?.Name ?? "<unknown>").Join(" <- ")} engaging anti-corruption mechanism, writing file to {antiCorruptionPath}");
        File.WriteAllText(antiCorruptionPath, JsonConvert.SerializeObject(Configuration, new JsonSerializerSettings()
        {
            Formatting = indented ? Formatting.Indented : Formatting.None,
            DefaultValueHandling = Configuration.GetType().IsDefined(typeof(IgnoreDefaultValueAttribute), false) ?DefaultValueHandling.Ignore:DefaultValueHandling.Include
        }), Encoding.UTF8);
        PluginLog.Verbose($"Now moving {antiCorruptionPath} to {path}");
        File.Move(antiCorruptionPath, path, true);
        PluginLog.Verbose($"Configuration successfully saved.");
    }

    public static T LoadConfiguration<T>(string path, bool appendConfigDirectory = true) where T : IEzConfig, new()
    {
        if (appendConfigDirectory) path = Path.Combine(Svc.PluginInterface.GetPluginConfigDirectory(), path);
        if (!File.Exists(path))
        {
            return new T();
        }
        return JsonConvert.DeserializeObject<T>(File.ReadAllText(path, Encoding.UTF8), new JsonSerializerSettings()
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
        }) ?? new T();
    }
}
