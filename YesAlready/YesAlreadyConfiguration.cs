using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace YesAlready
{
    internal partial class YesAlreadyConfiguration : IPluginConfiguration
    {
        public static YesAlreadyConfiguration Load(DalamudPluginInterface pluginInterface)
        {
            var pluginConfigPath = pluginInterface.ConfigFile;
            if (!pluginConfigPath.Exists)
                return new YesAlreadyConfiguration();
            else
                return JsonConvert.DeserializeObject<YesAlreadyConfiguration>(File.ReadAllText(pluginConfigPath.FullName));
        }

        public int Version { get; set; } = 1;

        public bool Enabled = true;

        public TextFolderNode RootFolder { get; private set; } = new TextFolderNode { Name = "/" };

        public bool DesynthDialogEnabled = false;
        public bool DesynthBulkDialogEnabled = false;
        public bool MaterializeDialogEnabled = false;
        public bool ItemInspectionResultEnabled = false;
        public bool RetainerTaskAskEnabled = false;
        public bool RetainerTaskResultEnabled = false;
        public bool GrandCompanySupplyReward = false;
        public bool ShopCardDialog = false;

        internal void Upgrade()
        {
            if (Version == 1)
                UpgradeV1();
        }

        #region Version1

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable IDE1006 // Naming Styles

        private void UpgradeV1()
        {
            var folders = new Dictionary<string, TextFolderNode> { { RootFolder.Name, RootFolder } };

            foreach (var textEntry in _TextEntriesBacker)
            {
                var folderName = string.IsNullOrEmpty(textEntry.Folder) ? RootFolder.Name : textEntry.Folder;
                if (!folders.TryGetValue(folderName, out var folder))
                {
                    folder = new TextFolderNode() { Name = folderName };
                    folders.Add(folderName, folder);
                    RootFolder.Children.Add(folder);
                }

                folder.Children.Add(new TextEntryNode() { Enabled = textEntry.Enabled, Text = textEntry.Text, });
            }
        }

        [JsonProperty("TextEntries")]
        [Obsolete("Removed in v2")]
        public List<ConfigTextEntry> _TextEntries { set => _TextEntriesBacker = value; }

        [JsonIgnore]
        [Obsolete("Removed in v2")]
        public List<ConfigTextEntry> _TextEntriesBacker = new();

#pragma warning restore IDE1006
#pragma warning restore CS0618

        #endregion

        public IEnumerable<ITextNode> GetAllNodes() => new ITextNode[] { RootFolder }.Concat(GetAllNodes(RootFolder.Children));

        public IEnumerable<ITextNode> GetAllNodes(IEnumerable<ITextNode> nodes)
        {
            foreach (var node in nodes)
            {
                yield return node;
                if (node is TextFolderNode)
                {
                    var children = (node as TextFolderNode).Children;
                    foreach (var childNode in GetAllNodes(children))
                    {
                        yield return childNode;
                    }
                }
            }
        }

        public bool TryFindParent(ITextNode node, out TextFolderNode parent)
        {
            foreach (var candidate in GetAllNodes())
            {
                if (candidate is TextFolderNode folder && folder.Children.Contains(node))
                {
                    parent = folder;
                    return true;
                }
            }

            parent = null;
            return false;
        }
    }

    public interface ITextNode
    {
        public string Name { get; }
    }

    public class TextEntryNode : ITextNode
    {
        public bool Enabled { get; set; } = true;

        [JsonIgnore]
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(ZoneText))
                    return Text;
                else
                    return $"{Text} ({ZoneText})";
            }
        }

        public string Text { get; set; } = "";

        [JsonIgnore]
        public bool IsTextRegex => Text.StartsWith("/") && Text.EndsWith("/");

        [JsonIgnore]
        public Regex TextRegex
        {
            get
            {
                try
                {
                    return new(Text.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool ZoneRestricted { get; set; } = false;

        public string ZoneText { get; set; } = "";

        [JsonIgnore]
        public bool ZoneIsRegex => ZoneText.StartsWith("/") && ZoneText.EndsWith("/");

        [JsonIgnore]
        public Regex ZoneRegex
        {
            get
            {
                try
                {
                    return new(ZoneText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                catch
                {
                    return null;
                }
            }
        }
    }

    public class TextFolderNode : ITextNode
    {
        public string Name { get; set; }

        [JsonProperty(ItemConverterType = typeof(ConcreteNodeConverter))]
        public List<ITextNode> Children { get; } = new();
    }

    public interface ITalkNode
    {
        public string Name { get; }
    }

    public class TalkEntryNode : ITextNode
    {
        public bool Enabled { get; set; } = true;

        public string Name { get; set; } = "";

        public List<string> Text { get; set; } = new();

        public List<string> ReplacmentText { get; set; } = new();
    }

    public class TalkFolderNode : ITextNode
    {
        public string Name { get; set; }

        [JsonProperty(ItemConverterType = typeof(ConcreteNodeConverter))]
        public List<ITalkNode> Children { get; } = new();
    }

    public class ConcreteNodeConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override bool CanConvert(Type objectType) => objectType == typeof(ITextNode);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var jType = jObject["$type"].Value<string>();

            if (jType == SimpleName(typeof(TextEntryNode)))
                return CreateObject<TextEntryNode>(jObject, serializer);
            else if (jType == SimpleName(typeof(TextFolderNode)))
                return CreateObject<TextFolderNode>(jObject, serializer);
            else
                throw new NotSupportedException($"Node type \"{jType}\" is not supported.");
        }

        private T CreateObject<T>(JObject jObject, JsonSerializer serializer) where T : new()
        {
            var obj = new T();
            serializer.Populate(jObject.CreateReader(), obj);
            return obj;
        }

        private string SimpleName(Type type)
        {
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }

    [Obsolete("Removed in v2")]
    internal class ConfigTextEntry : ICloneable
    {
        public bool Enabled = false;
        public string Folder = "";
        public string Text = "";

        [JsonIgnore]
        public bool IsRegex => Text.StartsWith("/") && Text.EndsWith("/");

        [JsonIgnore]
        public Regex Regex
        {
            get
            {
                try
                {
                    return new(Text.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public object Clone()
        {
            return new ConfigTextEntry
            {
                Enabled = Enabled,
                Folder = Folder,
                Text = Text,
            };
        }
    }
}
