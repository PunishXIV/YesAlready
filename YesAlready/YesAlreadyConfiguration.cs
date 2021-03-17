using Dalamud.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace YesAlready
{
    internal class YesAlreadyConfiguration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public bool Enabled = true;

        public List<ConfigTextEntry> TextEntries = new();

        public bool DesynthDialogEnabled = false;
        public bool MaterializeDialogEnabled = false;
        public bool ItemInspectionResultEnabled = false;
        public bool RetainerTaskAskEnabled = false;
        public bool RetainerTaskResultEnabled = false;
    }

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
