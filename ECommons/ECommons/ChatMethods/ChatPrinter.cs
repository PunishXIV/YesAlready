using Dalamud.Game.Text.SeStringHandling;
using ECommons.DalamudServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.ChatMethods
{
    public static class ChatPrinter
    {
        public static void Red(string text) => PrintColored(UIColor.Red, text);
        public static void Orange(string text) => PrintColored(UIColor.Orange, text);
        public static void Yellow(string text) => PrintColored(UIColor.Yellow, text);
        public static void Green(string text) => PrintColored(UIColor.Green, text);

        public static void PrintColored(UIColor col, string text)
        {
            Svc.Chat.Print(new()
            {
                Message = new SeStringBuilder().AddUiForeground(text, (ushort)col).Build()
            });
        }
    }
}
