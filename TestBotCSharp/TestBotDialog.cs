using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace TestBotCSharp
{
    [Serializable]
    public class TestBotDialog : IDialog<object>
    {

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            var testReply = new TestBotCSharp.TestReply(context);
            var reply = await testReply.CreateMessage((Activity)message);
            if (reply != null) await context.PostAsync(reply);
            
            //Dump payload if requested:
            var dumpReply = testReply.DumpMessage((Activity)message, reply);
            if (dumpReply != null) await context.PostAsync(dumpReply);

            //if we had a reply, we handled simple message so wait:
            if (reply != null)
            {
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}