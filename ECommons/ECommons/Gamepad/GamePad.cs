using Dalamud.Game.ClientState.GamePad;
using ECommons.DalamudServices;
using System.Collections.Generic;

namespace ECommons.Gamepad
{
    public static class GamePad
    {
        /// <summary>
        /// Dictionary containing a mapping of <see cref="GamepadButtons"/> to Playstation / Xbox button names.
        /// </summary>
        public static Dictionary<GamepadButtons, string> ControllerButtons = new Dictionary<GamepadButtons, string>()
        {
            { GamepadButtons.None, "None" },
            { GamepadButtons.DpadUp, "D-Pad Up"},
            { GamepadButtons.DpadLeft, "D-Pad Left" },
            { GamepadButtons.DpadDown, "D-Pad Down" },
            { GamepadButtons.DpadRight, "D-Pad Right" },
            { GamepadButtons.North, "△ / Y" },
            { GamepadButtons.West, "□ / X" },
            { GamepadButtons.South, "X / A" },
            { GamepadButtons.East, "○ / B" },
            { GamepadButtons.L1, "L1 / LB" },
            { GamepadButtons.L2, "L2 / LT" },
            { GamepadButtons.R1, "R1 / RB" },
            { GamepadButtons.R2, "R2 / RT" },
            { GamepadButtons.L3, "L3 / LS" },
            { GamepadButtons.R3, "R3 / RS" },
            { GamepadButtons.Start, "Options / Start" },
            { GamepadButtons.Select, "Share / Back" }
        };
        /// <summary>
        /// Gets the "Enable gamepad" option from FFXIV to indicate if the gamepad has been enabled in FFXIV.
        /// </summary>
        /// <returns>FFXIV is using a controller.</returns>
        public static bool IsControllerEnabled() { Svc.GameConfig.TryGet(Dalamud.Game.Config.SystemConfigOption.PadAvailable, out bool enabled); return enabled; }

        /// <summary>
        /// Checks if a controller button has been pressed. Only true on the first frame it has been pressed.
        /// </summary>
        /// <param name="button">Button to check.</param>
        /// <returns>Button has just been pressed.</returns>
        public static bool IsButtonPressed(GamepadButtons button) => Svc.GamepadState.Pressed(button) == 1;

        /// <summary>
        /// Checks if a controller button is currently held. Returns true for every frame it's held down.
        /// </summary>
        /// <param name="button">Button to check.</param>
        /// <returns>Button is being held down.</returns>
        public static bool IsButtonHeld(GamepadButtons button) => Svc.GamepadState.Raw(button) == 1;

        /// <summary>
        /// Checks if a controller button has just been released. Only true on the first frame after releasing.
        /// </summary>
        /// <param name="button">Button to check.</param>
        /// <returns>Button has just been released.</returns>
        public static bool IsButtonJustReleased(GamepadButtons button) => Svc.GamepadState.Released(button) == 1;

    }
}
