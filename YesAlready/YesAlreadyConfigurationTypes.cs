using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YesAlready;

public interface ITextNode
{
    public string Name { get; }
    public bool Enabled { get; set; }
    [JsonIgnore]
    public bool IsRegex => Name.StartsWith("/") && Name.EndsWith("/");

    [JsonIgnore]
    public Regex? Regex
    {
        get
        {
            try
            {
                return new(Name.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch
            {
                return null;
            }
        }
    }
}

public enum ComparisonType
{
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    Equal
}

public class TextEntryNode : ITextNode
{
    public bool Enabled { get; set; } = true;

    [JsonIgnore]
    public string Name => !string.IsNullOrEmpty(ZoneText)
                ? $"({ZoneText}) {Text}"
                : Text;

    public string Text { get; set; } = string.Empty;
    [JsonIgnore]
    public bool IsTextRegex => Text.StartsWith("/") && Text.EndsWith("/");

    [JsonIgnore]
    public Regex? TextRegex
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


    public bool IsConditional { get; set; } = false;
    public string ConditionalNumberTemplate { get; set; } = string.Empty;
    public int ConditionalNumber { get; set; } = 0;

    public ComparisonType ComparisonType { get; set; } = ComparisonType.GreaterThanOrEqual;

    [JsonIgnore]
    public bool IsConditionalNumberRegex => ConditionalNumberTemplate.StartsWith("/") && ConditionalNumberTemplate.EndsWith("/");

    [JsonIgnore]
    public Regex? ConditionalNumberRegex
    {
        get
        {
            try
            {
                return new(ConditionalNumberTemplate.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch
            {
                return null;
            }
        }
    }


    public bool ZoneRestricted { get; set; } = false;

    public string ZoneText { get; set; } = string.Empty;

    [JsonIgnore]
    public bool ZoneIsRegex => ZoneText.StartsWith("/") && ZoneText.EndsWith("/");

    [JsonIgnore]
    public Regex? ZoneRegex
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

    public bool IsYes { get; set; } = true;
}

public class OkEntryNode : ITextNode
{
    public bool Enabled { get; set; } = true;

    [JsonIgnore]
    public string Name => Text;

    public string Text { get; set; } = string.Empty;

    [JsonIgnore]
    public bool IsTextRegex => Text.StartsWith("/") && Text.EndsWith("/");

    [JsonIgnore]
    public Regex? TextRegex
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
}

public class ListEntryNode : ITextNode
{
    public bool Enabled { get; set; } = true;

    [JsonIgnore]
    public string Name => !string.IsNullOrEmpty(TargetText)
                ? $"({TargetText}) {Text}"
                : Text;

    public string Text { get; set; } = string.Empty;

    [JsonIgnore]
    public bool IsTextRegex => Text.StartsWith("/") && Text.EndsWith("/");

    [JsonIgnore]
    public Regex? TextRegex
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

    public bool TargetRestricted { get; set; } = false;

    public string TargetText { get; set; } = string.Empty;

    [JsonIgnore]
    public bool TargetIsRegex => TargetText.StartsWith("/") && TargetText.EndsWith("/");

    [JsonIgnore]
    public Regex? TargetRegex
    {
        get
        {
            try
            {
                return new(TargetText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch
            {
                return null;
            }
        }
    }
}

public class TalkEntryNode : ITextNode
{
    public bool Enabled { get; set; } = true;

    [JsonIgnore]
    public string Name => TargetText;

    public string TargetText { get; set; } = string.Empty;

    [JsonIgnore]
    public bool TargetIsRegex => TargetText.StartsWith("/") && TargetText.EndsWith("/");

    [JsonIgnore]
    public Regex? TargetRegex
    {
        get
        {
            try
            {
                return new(TargetText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch
            {
                return null;
            }
        }
    }
}

public class NumericsEntryNode : ITextNode
{
    public bool Enabled { get; set; } = true;

    [JsonIgnore]
    public string Name => IsPercent ? $"({Percentage}%) {Text}" : $"({Quantity}f) {Text}";

    public string Text { get; set; } = string.Empty;
    public bool IsPercent { get; set; } = true;
    public int Percentage { get; set; } = 100;
    public int Quantity { get; set; } = 0;

    [JsonIgnore]
    public bool IsTextRegex => Text.StartsWith("/") && Text.EndsWith("/");

    [JsonIgnore]
    public Regex? TextRegex
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
}

public class TextFolderNode : ITextNode
{
    public string Name { get; set; } = string.Empty;

    [JsonProperty(ItemConverterType = typeof(ConcreteNodeConverter))]
    public List<ITextNode> Children { get; } = [];
    public bool Enabled { get; set; } = true;
}

public class ConcreteNodeConverter : JsonConverter
{
    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) => objectType == typeof(ITextNode);

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var jType = jObject["$type"]!.Value<string>();

        if (jType == SimpleName(typeof(TextEntryNode)))
        {
            return CreateObject<TextEntryNode>(jObject, serializer);
        }
        else if (jType == SimpleName(typeof(OkEntryNode)))
        {
            return CreateObject<OkEntryNode>(jObject, serializer);
        }
        else if (jType == SimpleName(typeof(ListEntryNode)))
        {
            return CreateObject<ListEntryNode>(jObject, serializer);
        }
        else if (jType == SimpleName(typeof(TalkEntryNode)))
        {
            return CreateObject<TalkEntryNode>(jObject, serializer);
        }
        else if (jType == SimpleName(typeof(TextFolderNode)))
        {
            return CreateObject<TextFolderNode>(jObject, serializer);
        }
        else if (jType == SimpleName(typeof(NumericsEntryNode)))
        {
            return CreateObject<NumericsEntryNode>(jObject, serializer);
        }
        else
        {
            throw new NotSupportedException($"Node type \"{jType}\" is not supported.");
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

    private static T CreateObject<T>(JObject jObject, JsonSerializer serializer) where T : new()
    {
        var obj = new T();
        serializer.Populate(jObject.CreateReader(), obj);
        return obj;
    }

    private static string SimpleName(Type type)
    {
        return $"{type.FullName}, {type.Assembly.GetName().Name}";
    }
}
