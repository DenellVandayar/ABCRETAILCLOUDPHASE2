using Azure.Storage.Queues;

namespace Test4712.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;
        private readonly QueueClient _OrderQueueClient;


        public QueueService(string connectionString)
        {
            _queueClient = new QueueClient(connectionString, "iteminventory");
            _OrderQueueClient = new QueueClient(connectionString, "itemorder");

        }

        public async Task SendMessageToQueueAsync(string message)
        {
            await _queueClient.SendMessageAsync(message);
        }

        public async Task SendOrderAsync(string message)
        {
            await _OrderQueueClient.SendMessageAsync(message);
        }

    }
}
