using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.DalamudServices.Legacy
{
    public static class SignatureHelper
    {
        public static void Initialise(object which, bool log = false)
        {
            Svc.Hook.InitializeFromAttributes(which);
        }
    }
}
