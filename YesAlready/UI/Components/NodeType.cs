using Dalamud.Interface;

namespace YesAlready.UI.Components;

public enum NodeType
{
    YesNo,
    Ok,
    List,
    Talk,
    Numerics
}

public static class NodeTypeExtensions
{
    public static FontAwesomeIcon GetIcon(this NodeType type) => type switch
    {
        NodeType.YesNo => FontAwesomeIcon.CheckSquare,
        NodeType.Ok => FontAwesomeIcon.Check,
        NodeType.List => FontAwesomeIcon.List,
        NodeType.Talk => FontAwesomeIcon.Comment,
        NodeType.Numerics => FontAwesomeIcon.Calculator,
        _ => FontAwesomeIcon.Question
    };

    public static string GetDisplayName(this NodeType type) => type switch
    {
        NodeType.YesNo => "Yes/No Dialog",
        NodeType.Ok => "OK Dialog",
        NodeType.List => "List Selection",
        NodeType.Talk => "Talk Dialog",
        NodeType.Numerics => "Numeric Input",
        _ => "Unknown"
    };
}