using AzureGems.Repository.Abstractions;

namespace SSW.SophieBot.CosmosClient
{
	public class ChatHistoryCosmosContext : CosmosContext
	{
		public IRepository<ChatHistoryModel> ChatHistory { get; set; }
	}
}
