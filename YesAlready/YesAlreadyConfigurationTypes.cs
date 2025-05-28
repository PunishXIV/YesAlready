using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace YesAlready;

public interface ITextNode
{
    bool Enabled { get; set; }
    string Name { get; }
    bool IsRegex { get; }
    Regex? Regex { get; }
}

public interface IZoneRestrictedNode
{
    bool ZoneRestricted { get; set; }
    string ZoneText { get; set; }
    bool ZoneIsRegex { get; }
    Regex? ZoneRegex { get; }
}

public interface ITargetRestrictedNode
{
    bool TargetRestricted { get; set; }
    string TargetText { get; set; }
    bool TargetIsRegex { get; }
    Regex? TargetRegex { get; }
}

public interface IPlayerConditionRestrictedNode
{
    bool RequiresPlayerConditions { get; set; }
    string PlayerConditions { get; set; }
}

public interface INumberRestrictedNode
{
    bool IsConditional { get; set; }
    string ConditionalNumberTemplate { get; set; }
    int ConditionalNumber { get; set; }
    ComparisonType ComparisonType { get; set; }
    public bool IsConditionalNumberRegex => ConditionalNumberTemplate.StartsWith('/') && ConditionalNumberTemplate.EndsWith('/');
    public Regex? ConditionalNumberRegex => RegexExtensions.TryCreateRegex(ConditionalNumberTemplate.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
}

public interface IValidatable
{
    bool IsValid { get; }
    string? ValidationError { get; }
}

public static class RegexExtensions
{
    public static bool IsValidRegex(string pattern)
    {
        try
        {
            _ = new Regex(pattern);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static Regex? TryCreateRegex(string pattern, RegexOptions options = RegexOptions.None)
    {
        try
        {
            return new Regex(pattern, options);
        }
        catch
        {
            return null;
        }
    }
}

public abstract class BaseTextNode : ITextNode
{
    public bool Enabled { get; set; } = true;
    public string Text { get; set; } = string.Empty;

    [JsonIgnore]
    public virtual bool IsTextRegex => Text.StartsWith('/') && Text.EndsWith('/');

    [JsonIgnore]
    public virtual Regex? TextRegex
    {
        get
        {
            try
            {
                return IsTextRegex ? new(Text.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase) : null;
            }
            catch
            {
                return null;
            }
        }
    }

    [JsonIgnore]
    public virtual string Name => Text;

    [JsonIgnore]
    public virtual bool IsRegex => IsTextRegex;

    [JsonIgnore]
    public virtual Regex? Regex => TextRegex;
}

public enum ComparisonType
{
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    Equal
}

public class TextEntryNode : BaseTextNode, IZoneRestrictedNode, IPlayerConditionRestrictedNode, INumberRestrictedNode, IValidatable
{
    public bool RequiresPlayerConditions { get; set; } = false;
    public string PlayerConditions { get; set; } = string.Empty;
    public bool IsConditional { get; set; } = false;
    public string ConditionalNumberTemplate { get; set; } = string.Empty;
    public int ConditionalNumber { get; set; } = 0;
    public ComparisonType ComparisonType { get; set; } = ComparisonType.GreaterThanOrEqual;
    public bool ZoneRestricted { get; set; } = false;
    public string ZoneText { get; set; } = string.Empty;
    public bool IsYes { get; set; } = true;

    [JsonIgnore]
    public bool ZoneIsRegex => ZoneText.StartsWith('/') && ZoneText.EndsWith('/');

    [JsonIgnore]
    public Regex? ZoneRegex => RegexExtensions.TryCreateRegex(ZoneText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);

    [JsonIgnore]
    public override string Name => !string.IsNullOrEmpty(ZoneText)
        ? $"({ZoneText}) {Text}"
        : Text;

    [JsonIgnore]
    public bool IsValid => !IsTextRegex || TextRegex != null;

    [JsonIgnore]
    public string? ValidationError => IsTextRegex && TextRegex == null ? "Invalid regex pattern" : null;
}

public class OkEntryNode : BaseTextNode, IValidatable
{
    [JsonIgnore]
    public bool IsValid => !IsTextRegex || TextRegex != null;

    [JsonIgnore]
    public string? ValidationError => IsTextRegex && TextRegex == null ? "Invalid regex pattern" : null;
}

public class ListEntryNode : BaseTextNode, ITargetRestrictedNode, IValidatable
{
    public bool TargetRestricted { get; set; } = false;
    public string TargetText { get; set; } = string.Empty;

    [JsonIgnore]
    public bool TargetIsRegex => TargetText.StartsWith('/') && TargetText.EndsWith('/');

    [JsonIgnore]
    public Regex? TargetRegex => RegexExtensions.TryCreateRegex(TargetText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);

    [JsonIgnore]
    public override string Name => !string.IsNullOrEmpty(TargetText)
        ? $"({TargetText}) {Text}"
        : Text;

    [JsonIgnore]
    public bool IsValid => !IsTextRegex || TextRegex != null;

    [JsonIgnore]
    public string? ValidationError => IsTextRegex && TextRegex == null ? "Invalid regex pattern" : null;
}

public class TalkEntryNode : BaseTextNode, IValidatable
{
    public string TargetText { get; set; } = string.Empty;

    [JsonIgnore]
    public bool TargetIsRegex => TargetText.StartsWith('/') && TargetText.EndsWith('/');

    [JsonIgnore]
    public Regex? TargetRegex => RegexExtensions.TryCreateRegex(TargetText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);

    [JsonIgnore]
    public override string Name => TargetText;

    [JsonIgnore]
    public bool IsValid => !TargetIsRegex || TargetRegex != null;

    [JsonIgnore]
    public string? ValidationError => TargetIsRegex && TargetRegex == null ? "Invalid regex pattern" : null;
}

public class NumericsEntryNode : BaseTextNode, IValidatable
{
    public bool IsPercent { get; set; } = true;
    public int Percentage { get; set; } = 100;
    public int Quantity { get; set; } = 0;

    [JsonIgnore]
    public override string Name => IsPercent ? $"({Percentage}%) {Text}" : $"({Quantity}f) {Text}";

    [JsonIgnore]
    public bool IsValid => !IsTextRegex || TextRegex != null;

    [JsonIgnore]
    public string? ValidationError => IsTextRegex && TextRegex == null ? "Invalid regex pattern" : null;
}

public class CustomEntryNode : BaseTextNode
{
    public string Addon { get; set; } = string.Empty;
    public bool UpdateState { get; set; } = true;
    public string CallbackParams { get; set; } = string.Empty;

    [JsonIgnore]
    public override string Name => $"{Text} ({Addon})";
}

public class TextFolderNode : ITextNode
{
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;

    [JsonProperty(ItemConverterType = typeof(ConcreteNodeConverter))]
    public List<ITextNode> Children { get; } = [];

    [JsonIgnore]
    public bool IsRegex => false;

    [JsonIgnore]
    public Regex? Regex => null;
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

        return jType switch
        {
            var t when t == SimpleName(typeof(TextEntryNode)) => CreateObject<TextEntryNode>(jObject, serializer),
            var t when t == SimpleName(typeof(OkEntryNode)) => CreateObject<OkEntryNode>(jObject, serializer),
            var t when t == SimpleName(typeof(ListEntryNode)) => CreateObject<ListEntryNode>(jObject, serializer),
            var t when t == SimpleName(typeof(TalkEntryNode)) => CreateObject<TalkEntryNode>(jObject, serializer),
            var t when t == SimpleName(typeof(TextFolderNode)) => CreateObject<TextFolderNode>(jObject, serializer),
            var t when t == SimpleName(typeof(NumericsEntryNode)) => CreateObject<NumericsEntryNode>(jObject, serializer),
            var t when t == SimpleName(typeof(CustomEntryNode)) => CreateObject<CustomEntryNode>(jObject, serializer),
            _ => throw new NotSupportedException($"Node type \"{jType}\" is not supported.")
        };
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        => throw new NotImplementedException();

    private static T CreateObject<T>(JObject jObject, JsonSerializer serializer) where T : new()
    {
        var obj = new T();
        serializer.Populate(jObject.CreateReader(), obj);
        return obj;
    }

    private static string SimpleName(Type type)
        => $"{type.FullName}, {type.Assembly.GetName().Name}";
}
