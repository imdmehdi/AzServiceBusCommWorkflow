using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace performancemessagereceiver
{
    class Program
    {
        const string ServiceBusConnectionString = "Endpoint=sb://mdservicebus0.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Jl3NLB/+sG6X6COxoKY79hRM6FC6pMptHwXTiB9uQeI=";
        const string TopicName = "salesperformancetopic";
        const string SubscriptionName = "Americas";

        private static ServiceBusClient s_client;
        private static ServiceBusAdministrationClient s_adminClient;
        //private static ServiceBusSender s_sender;

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {

            s_client = new ServiceBusClient(ServiceBusConnectionString);
            s_adminClient = new ServiceBusAdministrationClient(ServiceBusConnectionString);
           

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            // Register subscription message handler and receive messages in a loop
          //  RegisterMessageHandler();

            Console.Read();


            await ReceiveMessagesAsync(SubscriptionName);
            Console.ResetColor();

            Console.WriteLine("=======================================================================");
            Console.WriteLine("Completed Receiving all messages. Disposing clients and deleting topic.");
            Console.WriteLine("=======================================================================");

            
            Console.WriteLine("Disposing client");
            await s_client.DisposeAsync();

            
        }
        private static async Task ReceiveMessagesAsync(string subscriptionName)
        {
            await using ServiceBusReceiver subscriptionReceiver = s_client.CreateReceiver(
                TopicName,
                subscriptionName,
                new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete });

            Console.WriteLine($"==========================================================================");
            Console.WriteLine($"{DateTime.Now} :: Receiving Messages From Subscription: {subscriptionName}");
            int receivedMessageCount = 0;
            while (true)
            {
                var receivedMessage = await subscriptionReceiver.ReceiveMessageAsync(TimeSpan.FromSeconds(1));
                if (receivedMessage != null)
                {
                    PrintReceivedMessage(receivedMessage);
                    receivedMessageCount++;
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine($"{DateTime.Now} :: Received '{receivedMessageCount}' Messages From Subscription: {subscriptionName}");
            Console.WriteLine($"==========================================================================");
            await subscriptionReceiver.CloseAsync();
        }
        private static void PrintReceivedMessage(ServiceBusReceivedMessage message)
        {
            Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), message.Subject);
            Console.WriteLine($"Received message with color: {message.ApplicationProperties["Color"]}, CorrelationId: {message.CorrelationId}");
            Console.ResetColor();
        }
        //static void RegisterMessageHandler()
        //{
        //    // Configure the message handler.
        //    var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
        //    {
        //        MaxConcurrentCalls = 1,
        //        AutoComplete = false
        //    };

        //    // Register the function that processes messages.
        //    subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        //}

        //static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        //{
        //    // Display the message.
        //    Console.WriteLine($"Received sale performance message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

        //    await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
        //}

        //static Task ExceptionReceivedHandler(exception exceptionReceivedEventArgs)
        //{
        //    Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
        //    var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
        //    Console.WriteLine("Exception context for troubleshooting:");
        //    Console.WriteLine($"- Endpoint: {context.Endpoint}");
        //    Console.WriteLine($"- Entity Path: {context.EntityPath}");
        //    Console.WriteLine($"- Executing Action: {context.Action}");
        //    return Task.CompletedTask;
        //}  
    }
}
