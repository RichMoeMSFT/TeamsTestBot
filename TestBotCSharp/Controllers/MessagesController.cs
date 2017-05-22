using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Microsoft.Bot.Connector.Teams;

namespace TestBotCSharp
{
    [BotAuthentication]
    [TenantFilter]
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
                if (true)
                {
                    await Conversation.SendAsync(activity, () => new TestBotDialog());
                }
                else
                {

                    testReply = new TestBotCSharp.TestReply(connector);
                    reply = await testReply.CreateMessage(activity);
                    dumpReply = testReply.DumpMessage(activity, reply);
                }

            }
            else
            {
                testReply = new TestBotCSharp.SystemReply(connector);
                reply = await testReply.CreateMessage(activity);
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