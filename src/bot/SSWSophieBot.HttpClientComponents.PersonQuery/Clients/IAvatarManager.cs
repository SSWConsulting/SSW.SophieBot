using SSWSophieBot.HttpClientAction.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Clients
{
    public interface IAvatarManager
    {
        Task<List<GetEmployeeModel>> GetAvatarUrlsAsync(IEnumerable<GetEmployeeModel> employees);
    }
}
