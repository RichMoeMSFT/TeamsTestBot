using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace TestBotCSharp
{

    public class TestBotReply
    {
        protected Activity m_sourceMessage;
        protected Activity m_replyMessage;
        protected List<string> m_args;

        protected ConnectorClient m_connector;

        protected ConversationParameters m_conversationParams;

        public TestBotReply(ConnectorClient c)
        {
            m_connector = c;
            m_conversationParams = null;

        }

        public virtual Activity CreateMessage(Activity messageIn)
        {
            return null;
        }

        public virtual Activity DumpMessage(Activity messageIn, Activity messageOut)
        {
            return null;
        }

        public virtual ConversationParameters GetConversationParameters()
        {
            return m_conversationParams;
        }
    }
}