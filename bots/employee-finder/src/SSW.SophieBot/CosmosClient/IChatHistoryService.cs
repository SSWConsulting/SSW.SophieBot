using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSW.SophieBot.CosmosClient
{
	public interface IChatHistoryService
	{
		Task<IEnumerable<ChatHistoryModel>> GetAll();
		Task<IEnumerable<ChatHistoryModel>> GetByEmployeeName(string EmployeeName);
		Task<IEnumerable<ChatHistoryModel>> GetLastMonthChatHistory();
		Task<ChatHistoryModel> SaveHistory(ChatHistoryModel chatHistory);
	}
}