using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;
using ECommons.DalamudServices;
using System;
using System.Linq;

namespace YesAlready.Utils;

internal class SEString
{
    internal static unsafe SeString GetSeString(byte* textPtr)
        => GetSeString((IntPtr)textPtr);

    internal static SeString GetSeString(IntPtr textPtr)
        => MemoryHelper.ReadSeStringNullTerminated(textPtr);

    internal static unsafe string GetSeStringText(byte* textPtr)
        => GetSeStringText(GetSeString(textPtr));

    internal static string GetSeStringText(IntPtr textPtr)
        => GetSeStringText(GetSeString(textPtr));

    internal static string GetSeStringText(SeString seString)
    {
        var pieces = seString.Payloads.OfType<TextPayload>().Select(t => t.Text);
        var text = string.Join(string.Empty, pieces).Replace('\n', ' ').Trim();
        return text;
    }

    public static void PrintPluginMessage(string msg)
    {
        var message = new XivChatEntry
        {
            Message = new SeStringBuilder()
            .AddUiForeground($"[{Name}] ", 45)
            .AddText(msg)
            .Build()
        };

        Svc.Chat.Print(message);
    }

    public static void PrintPluginMessage(SeString msg)
    {
        var message = new XivChatEntry
        {
            Message = new SeStringBuilder()
            .AddUiForeground($"[{Name}] ", 45)
            .Append(msg)
            .Build()
        };

        Svc.Chat.Print(message);
    }
}
