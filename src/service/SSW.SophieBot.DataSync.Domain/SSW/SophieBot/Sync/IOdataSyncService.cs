using System.Collections.Generic;
using System.Threading;

namespace SSW.SophieBot.Sync
{
    public interface IOdataSyncService<T> : ISyncService<OdataResponse<T>>
    {

    }
}
