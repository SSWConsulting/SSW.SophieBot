using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSW.SophieBot.CosmosClient
{
	public class ChatHistoryService : IChatHistoryService
	{
		private readonly ChatHistoryCosmosContext _chatHistoryCosmosContext;

		public ChatHistoryService(ChatHistoryCosmosContext chatHistoryCosmosContext)
		{
			_chatHistoryCosmosContext = chatHistoryCosmosContext;
		}

		public async Task<ChatHistoryModel> SaveHistory(ChatHistoryModel chatHistory)
		{
			return await _chatHistoryCosmosContext.ChatHistory.Add(chatHistory);
		}

		public async Task<IEnumerable<ChatHistoryModel>> GetLastMonthChatHistory()
		{
			var StartTime = DateTime.Today;
			var EndTime = StartTime.AddDays(-30);

			return await _chatHistoryCosmosContext.ChatHistory.Query(q => q
				.Where(c => c.Time >= StartTime && c.Time <= EndTime)
				.OrderBy(c => c.Time));
		}

		public async Task<IEnumerable<ChatHistoryModel>> GetAll()
		{
			return await _chatHistoryCosmosContext.ChatHistory.GetAll();
		}

		public async Task<IEnumerable<ChatHistoryModel>> GetByEmployeeName(string EmployeeName)
		{
			return await _chatHistoryCosmosContext.ChatHistory.Query(q => q
				.Where(e => e.UserName.Equals(EmployeeName))
				.OrderBy(o => o.Time));
		}
	}
}

