using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YesAlready;

/// <summary>
/// Base node interface type.
/// </summary>
public interface ITextNode
{
    /// <summary>
    /// Gets the name of the node.
    /// </summary>
    public string Name { get; }
}

/// <summary>
/// Text entry node type.
/// </summary>
public class TextEntryNode : ITextNode
{
    /// <summary>
    /// Gets or sets a value indicating whether the node is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets the name of the node.
    /// </summary>
    [JsonIgnore]
    public string Name
    {
        get
        {
            return !string.IsNullOrEmpty(this.ZoneText)
                ? $"({this.ZoneText}) {this.Text}"
                : this.Text;
        }
    }

    /// <summary>
    /// Gets or sets the matching text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the matching text is a regex.
    /// </summary>
    [JsonIgnore]
    public bool IsTextRegex => this.Text.StartsWith("/") && this.Text.EndsWith("/");

    /// <summary>
    /// Gets the matching text as a compiled regex.
    /// </summary>
    [JsonIgnore]
    public Regex? TextRegex
    {
        get
        {
            try
            {
                return new(this.Text.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this entry should be zone restricted.
    /// </summary>
    public bool ZoneRestricted { get; set; } = false;

    /// <summary>
    /// Gets or sets the matching zone text.
    /// </summary>
    public string ZoneText { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the matching zone text is a regex.
    /// </summary>
    [JsonIgnore]
    public bool ZoneIsRegex => this.ZoneText.StartsWith("/") && this.ZoneText.EndsWith("/");

    /// <summary>
    /// Gets the matching zone text as a compiled regex.
    /// </summary>
    [JsonIgnore]
    public Regex? ZoneRegex
    {
        get
        {
            try
            {
                return new(this.ZoneText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether yes should be pressed instead of no.
    /// </summary>
    public bool IsYes { get; set; } = true;
}

/// <summary>
/// List entry node type.
/// </summary>
public class ListEntryNode : ITextNode
{
    /// <summary>
    /// Gets or sets a value indicating whether the node is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets the name of the node.
    /// </summary>
    [JsonIgnore]
    public string Name
    {
        get
        {
            return !string.IsNullOrEmpty(this.TargetText)
                ? $"({this.TargetText}) {this.Text}"
                : this.Text;
        }
    }

    /// <summary>
    /// Gets or sets the matching text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the matching text is a regex.
    /// </summary>
    [JsonIgnore]
    public bool IsTextRegex => this.Text.StartsWith("/") && this.Text.EndsWith("/");

    /// <summary>
    /// Gets the matching text as a compiled regex.
    /// </summary>
    [JsonIgnore]
    public Regex? TextRegex
    {
        get
        {
            try
            {
                return new(this.Text.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this entry should be target restricted.
    /// </summary>
    public bool TargetRestricted { get; set; } = false;

    /// <summary>
    /// Gets or sets the matching target name.
    /// </summary>
    public string TargetText { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the matching target text is a regex.
    /// </summary>
    [JsonIgnore]
    public bool TargetIsRegex => this.TargetText.StartsWith("/") && this.TargetText.EndsWith("/");

    /// <summary>
    /// Gets the matching target text as a compiled regex.
    /// </summary>
    [JsonIgnore]
    public Regex? TargetRegex
    {
        get
        {
            try
            {
                return new(this.TargetText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch
            {
                return null;
            }
        }
    }
}

/// <summary>
/// Talk entry node type.
/// </summary>
public class TalkEntryNode : ITextNode
{
    /// <summary>
    /// Gets or sets a value indicating whether the node is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets the name of the node.
    /// </summary>
    [JsonIgnore]
    public string Name
    {
        get
        {
            return this.TargetText;
        }
    }

    /// <summary>
    /// Gets or sets the matching target name.
    /// </summary>
    public string TargetText { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the matching target text is a regex.
    /// </summary>
    [JsonIgnore]
    public bool TargetIsRegex => this.TargetText.StartsWith("/") && this.TargetText.EndsWith("/");

    /// <summary>
    /// Gets the matching target text as a compiled regex.
    /// </summary>
    [JsonIgnore]
    public Regex? TargetRegex
    {
        get
        {
            try
            {
                return new(this.TargetText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch
            {
                return null;
            }
        }
    }
}

/// <summary>
/// Folder node type.
/// </summary>
public class TextFolderNode : ITextNode
{
    /// <inheritdoc/>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the child nodes of this folder.
    /// </summary>
    [JsonProperty(ItemConverterType = typeof(ConcreteNodeConverter))]
    public List<ITextNode> Children { get; } = new();
}

/// <summary>
/// Convert a serialized interface to a concrete type.
/// </summary>
public class ConcreteNodeConverter : JsonConverter
{
    /// <inheritdoc/>
    public override bool CanRead => true;

    /// <inheritdoc/>
    public override bool CanWrite => false;

    /// <inheritdoc/>
    public override bool CanConvert(Type objectType) => objectType == typeof(ITextNode);

    /// <inheritdoc/>
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var jType = jObject["$type"]!.Value<string>();

        if (jType == this.SimpleName(typeof(TextEntryNode)))
        {
            return this.CreateObject<TextEntryNode>(jObject, serializer);
        }
        else if (jType == this.SimpleName(typeof(ListEntryNode)))
        {
            return this.CreateObject<ListEntryNode>(jObject, serializer);
        }
        else if (jType == this.SimpleName(typeof(TalkEntryNode)))
        {
            return this.CreateObject<TalkEntryNode>(jObject, serializer);
        }
        else if (jType == this.SimpleName(typeof(TextFolderNode)))
        {
            return this.CreateObject<TextFolderNode>(jObject, serializer);
        }
        else
        {
            throw new NotSupportedException($"Node type \"{jType}\" is not supported.");
        }
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

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
}
