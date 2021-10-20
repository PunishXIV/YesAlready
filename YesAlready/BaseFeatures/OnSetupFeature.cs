using System;

using Dalamud.Hooking;
using Dalamud.Logging;

namespace YesAlready.BaseFeatures
{
    /// <summary>
    /// An abstract that hooks OnSetup to provide a feature.
    /// </summary>
    internal abstract class OnSetupFeature : IBaseFeature
    {
        private readonly Hook<OnSetupDelegate> onSetupHook;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnSetupFeature"/> class.
        /// </summary>
        /// <param name="onSetupAddress">Address to the OnSetup method.</param>
        public OnSetupFeature(IntPtr onSetupAddress)
        {
            this.onSetupHook = new Hook<OnSetupDelegate>(onSetupAddress, this.OnSetupDetour);
            this.onSetupHook.Enable();
        }

        /// <summary>
        /// A delegate matching AtkUnitBase.OnSetup.
        /// </summary>
        /// <param name="addon">Addon address.</param>
        /// <param name="a2">Unused for our purposes.</param>
        /// <param name="data">Data address.</param>
        /// <returns>The addon address.</returns>
        internal delegate IntPtr OnSetupDelegate(IntPtr addon, uint a2, IntPtr data);

        /// <summary>
        /// Gets the name of the addon being hooked.
        /// </summary>
        protected abstract string AddonName { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.onSetupHook?.Disable();
            this.onSetupHook?.Dispose();
        }

        /// <summary>
        /// A method that is run within the OnSetup detour.
        /// </summary>
        /// <param name="addon">Addon address.</param>
        /// <param name="a2">Unknown paramater.</param>
        /// <param name="data">Setup data address.</param>
        protected abstract unsafe void OnSetupImpl(IntPtr addon, uint a2, IntPtr data);

        private IntPtr OnSetupDetour(IntPtr addon, uint a2, IntPtr data)
        {
            PluginLog.Debug($"Addon{this.AddonName}.OnSetup");
            var result = this.onSetupHook.Original(addon, a2, data);

            if (!Service.Configuration.Enabled)
                return result;

            try
            {
                this.OnSetupImpl(addon, a2, data);
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }

            return result;
        }
    }
}
