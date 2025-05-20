using System.Collections.Generic;

namespace YesAlready.IPC;

public class BlockListHandler
{
    internal const string BlockListNamespace = "YesAlready.StopRequests";
    internal HashSet<string> BlockList;
    internal bool Locked => BlockList.Count != 0;

    public BlockListHandler()
    {
        BlockList = Svc.PluginInterface.GetOrCreateData<HashSet<string>>(BlockListNamespace, () => []);
        BlockList.Clear();
    }
}
