using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace privatemessagesender
{
    class Program
    {
        const string ServiceBusConnectionString = "Endpoint=sb://mdservicebus0.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Jl3NLB/+sG6X6COxoKY79hRM6FC6pMptHwXTiB9uQeI=";
        const string QueueName = "servicebusqueue-mdsb0";

        static void Main(string[] args)
        {
            Console.WriteLine("Sending a message to the Sales Messages queue...");
            SendSalesMessageAsync().GetAwaiter().GetResult();
            Console.WriteLine("Message was sent successfully.");
        }

        static async Task SendSalesMessageAsync()
        {
            await using var client = new ServiceBusClient(ServiceBusConnectionString);

            await using ServiceBusSender sender = client.CreateSender(QueueName);
            try
            {
                string messageBody = $"$10,000 order for bicycle parts from retailer Adventure Works.";
                IList<ServiceBusMessage> messages = new List<ServiceBusMessage>();
                messages.Add(new ServiceBusMessage("First"));
                messages.Add(new ServiceBusMessage("Second"));
                //var message = new ServiceBusMessage(messageBody);
                Console.WriteLine($"Sending message: {messageBody}");
                //await sender.SendMessageAsync(message);
                await sender.SendMessagesAsync(messages);//batch can also add trybatch
                /*
                 // add the messages that we plan to send to a local queue
Queue<ServiceBusMessage> messages = new Queue<ServiceBusMessage>();
messages.Enqueue(new ServiceBusMessage("First message"));
messages.Enqueue(new ServiceBusMessage("Second message"));
messages.Enqueue(new ServiceBusMessage("Third message"));

// create a message batch that we can send
// total number of messages to be sent to the Service Bus queue
int messageCount = messages.Count;

// while all messages are not sent to the Service Bus queue
while (messages.Count > 0)
{
    // start a new batch
    using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

    // add the first message to the batch
    if (messageBatch.TryAddMessage(messages.Peek()))
    {
        // dequeue the message from the .NET queue once the message is added to the batch
        messages.Dequeue();
    }
    else
    {
        // if the first message can't fit, then it is too large for the batch
        throw new Exception($"Message {messageCount - messages.Count} is too large and cannot be sent.");
    }

    // add as many messages as possible to the current batch
    while (messages.Count > 0 && messageBatch.TryAddMessage(messages.Peek()))
    {
        // dequeue the message from the .NET queue as it has been added to the batch
        messages.Dequeue();
    }

    // now, send the batch
    await sender.SendMessagesAsync(messageBatch);

    // if there are any remaining messages in the .NET queue, the while loop repeats
}
                 */

            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}
