using System.Collections.Generic;

namespace YesAlready.IPC
{
    internal class BlockListHandler
    {
        internal const string BlockListNamespace = "YesAlready.StopRequests";
        internal HashSet<string> BlockList;
        internal bool Locked => BlockList.Count != 0;

        internal BlockListHandler()
        {
            BlockList = Svc.PluginInterface.GetOrCreateData<HashSet<string>>(BlockListNamespace, () => []);
            BlockList.Clear();
        }
    }
}
