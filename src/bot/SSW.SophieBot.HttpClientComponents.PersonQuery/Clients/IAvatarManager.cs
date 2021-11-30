using SSW.SophieBot.HttpClientAction.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery.Clients
{
    public interface IAvatarManager
    {
        Task<List<GetEmployeeModel>> GetAvatarUrlsAsync(IEnumerable<GetEmployeeModel> employees);
    }
}
