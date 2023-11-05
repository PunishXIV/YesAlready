using ECommons.DalamudServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
