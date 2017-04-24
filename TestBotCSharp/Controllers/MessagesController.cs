using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace TestBotCSharp
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            Activity reply = null;
            Activity dumpReply = null;

            TestBotReply testReply = null;

            if (activity.Type == ActivityTypes.Message || activity.Type == ActivityTypes.Invoke)
            {

                testReply = new TestBotCSharp.TestReply(connector);
                reply = testReply.CreateMessage(activity);
                dumpReply = testReply.DumpMessage(activity, reply);

            }
            else
            {
                testReply = new TestBotCSharp.SystemReply(connector);
                reply = testReply.CreateMessage(activity);
            }

            if (reply != null)
            {
                if (reply.Conversation == null)
                {
                    ConversationParameters conversationParams = testReply.GetConversationParameters();

                    await connector.Conversations.CreateConversationAsync(conversationParams);
                    if (dumpReply != null)
                        await connector.Conversations.ReplyToActivityAsync(dumpReply);
                }
                else if (reply.Conversation.Id == "1:1")
                {
                    ConversationParameters conversationParams = testReply.GetConversationParameters();

                    var conversationId = await connector.Conversations.CreateConversationAsync(conversationParams);
                    if (conversationId != null)
                    {

                        IMessageActivity message = Activity.CreateMessageActivity();
                        message.From = new ChannelAccount(activity.Recipient.Id, activity.Recipient.Name);
                        //message.Recipient = ;
                        message.Conversation = new ConversationAccount(id: conversationId.Id);
                        message.Text = "Hello, this is a 1:1 message created by me - " + activity.Recipient.Name;
                        await connector.Conversations.SendToConversationAsync((Activity)message);
                    }
                    if (dumpReply != null)
                        await connector.Conversations.ReplyToActivityAsync(dumpReply);
                }
                else if (reply.Conversation.Id != activity.Conversation.Id)
                {
                    await connector.Conversations.SendToConversationAsync(reply);
                    if (dumpReply != null)
                        await connector.Conversations.SendToConversationAsync(dumpReply);
                }
                else
                {
                    await connector.Conversations.ReplyToActivityAsync(reply);
                    if (dumpReply != null)
                        await connector.Conversations.ReplyToActivityAsync(dumpReply);
                }
            }


            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
    }
}