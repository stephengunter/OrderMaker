using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public enum ProviderName
    {
        CONCORD = 0,
        HUA_NAN = 1,
        FAKE = 2
    }


    public enum ConnectionStatus
    {
        DISCONNECTED = 0,
        CONNECTING = 1,
        CONNECTED = 2
    }


    public enum OrderActionType
    {
        Create= 0,
        Delete = 1,
        None = -1
    }
}
