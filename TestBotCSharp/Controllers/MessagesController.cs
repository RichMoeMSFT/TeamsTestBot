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

            if (activity.Type == ActivityTypes.Message)
            {

                TestReply testreply = new TestBotCSharp.TestReply(connector);
                reply = testreply.CreateMessage(activity);
                dumpReply = testreply.DumpMessage(activity, reply);

            }
            else
            {
                SystemReply systemReply = new TestBotCSharp.SystemReply(connector);
                reply = systemReply.CreateMessage(activity);
            }

            if (reply != null)
            {
                if (reply.Conversation == null)
                {
                    ConversationParameters conversationParams = new ConversationParameters(
                        isGroup: true,
                        bot: null,
                        members: null,
                        topicName: "New Conversation",
                        activity: (Activity)reply,
                        channelData: activity.ChannelData
                    );

                    await connector.Conversations.CreateConversationAsync(conversationParams);
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