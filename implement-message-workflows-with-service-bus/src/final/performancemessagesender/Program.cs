using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace performancemessagesender
{    
    class Program
    {
        const string ServiceBusConnectionString = "Endpoint=sb://mdservicebus0.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Jl3NLB/+sG6X6COxoKY79hRM6FC6pMptHwXTiB9uQeI=";
        const string TopicName = "salesperformancetopic";
        const string TopicNameNew = "salesperformancetopicNew";
        private const string NoFilterSubscriptionName = "NoFilterSubscription";

        private static ServiceBusClient s_client;
        private static ServiceBusAdministrationClient s_adminClient;
        private static ServiceBusSender s_sender;

        static void Main(string[] args)
        {
            s_client = new ServiceBusClient(ServiceBusConnectionString);
            s_adminClient = new ServiceBusAdministrationClient(ServiceBusConnectionString);
            Console.WriteLine("Sending a message to the Sales Performance topic...");

            SendPerformanceMessageAsync().GetAwaiter().GetResult();

            Console.WriteLine("Message was sent successfully.");

        }

        static async Task SendPerformanceMessageAsync()
        {
            

            // Send messages.
            try
            {
                Console.WriteLine($"Creating topic {TopicNameNew}");
                await s_adminClient.CreateTopicAsync(TopicNameNew);

                s_sender = s_client.CreateSender(TopicNameNew);
               

                
                Console.WriteLine($"Creating subscription {NoFilterSubscriptionName}");
                await s_adminClient.CreateSubscriptionAsync(TopicName, NoFilterSubscriptionName);
                await SendMessagesAsync();

                s_sender = s_client.CreateSender(TopicName);

                await SendMessagesAsync();


                

                // Send the message to the queue.
               

            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }

            await s_sender.CloseAsync();
            Console.WriteLine("Disposing client");
            await s_client.DisposeAsync();

            Console.WriteLine("Deleting topic");

            // Deleting the topic will handle deleting all the subscriptions as well.
            await s_adminClient.DeleteTopicAsync(TopicNameNew);
        }
        private static async Task SendMessagesAsync()
        {
            Console.WriteLine($"==========================================================================");
            Console.WriteLine("Creating messages to send to Topic");
            List<ServiceBusMessage> messages = new();
            messages.Add(CreateMessage(subject: "Red"));
            messages.Add(CreateMessage(subject: "Blue"));
            messages.Add(CreateMessage(subject: "Red", correlationId: "important"));
            messages.Add(CreateMessage(subject: "Blue", correlationId: "important"));
            messages.Add(CreateMessage(subject: "Red", correlationId: "notimportant"));
            messages.Add(CreateMessage(subject: "Blue", correlationId: "notimportant"));
            messages.Add(CreateMessage(subject: "Green"));
            messages.Add(CreateMessage(subject: "Green", correlationId: "important"));
            messages.Add(CreateMessage(subject: "Green", correlationId: "notimportant"));

            Console.WriteLine("Sending messages to send to Topic");
            await s_sender.SendMessagesAsync(messages);
            Console.WriteLine($"==========================================================================");
        }
        private static ServiceBusMessage CreateMessage(string subject, string correlationId = null)
        {
            ServiceBusMessage message = new() { Subject = subject };
            message.ApplicationProperties.Add("Color", subject);

            if (correlationId != null)
            {
                message.CorrelationId = correlationId;
            }

            PrintMessage(message);

            return message;
        }

        private static void PrintMessage(ServiceBusMessage message)
        {
            Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), message.Subject);
            Console.WriteLine($"Created message with color: {message.ApplicationProperties["Color"]}, CorrelationId: {message.CorrelationId}");
            Console.ResetColor();
        }
    }
}
