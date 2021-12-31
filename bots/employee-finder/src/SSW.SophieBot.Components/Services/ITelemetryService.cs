using SSW.SophieBot.Components.Models;
using System.Threading.Tasks;

namespace SSW.SophieBot.Components.Services
{
    public interface ITelemetryService
    {
        Task<UsageByUserQueryResult> GetUsageByUserAsync(int? spanDays = null, int? maxItemCount = null);
    }
}
