using AzureStorageLibrary;
using AzureStorageLibrary.Services;
using System.Text;

ConnectionString.AzureConnectionString = "***";

AzQueue queue = new AzQueue("ornekkuyruk");

string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("emre çörü"));

//await queue.SendMessageAsync(base64);

var queueMessage = await queue.RetrieveNextMessageAsync();

//var base64EncodedMessage = queueMessage.Body.ToString();
//byte[] messageBytes = Convert.FromBase64String(base64EncodedMessage);
//string decodedMessage = Encoding.UTF8.GetString(messageBytes);

//Console.WriteLine(decodedMessage);

await queue.DeleteMessage(queueMessage.MessageId, queueMessage.PopReceipt);
