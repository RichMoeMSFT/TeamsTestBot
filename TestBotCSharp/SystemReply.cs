using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace TestBotCSharp
{
    public class SystemReply : TestBotReply
    {

        public SystemReply(ConnectorClient c) : base (c)
        {

        }

        public override Activity CreateMessage(Activity messageIn)
        {
            m_sourceMessage = messageIn;
            m_replyMessage = null;
            string messageString = null;


            if (messageIn.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (messageIn.Type == ActivityTypes.ConversationUpdate)
            {
                messageString = ConversationUpdate();
            }
            else if (messageIn.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (messageIn.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (messageIn.Type == ActivityTypes.Ping)
            {
            }
            else
            {
                messageString = "Unhandled Activity: " + (string)messageIn.Type;


            }
            if (messageString != null)
            {
                //Append full inboard payload:
                messageString += "\r\n\r\n\r\n" + ActivityDumper.ActivityDump(messageIn);

                m_replyMessage = messageIn.CreateReply();
                m_replyMessage.Text = messageString;
            }

            return m_replyMessage;
        }

        private string ConversationUpdate()
        {

            // Handle conversation state changes, like members being added and removed
            // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
            // Not available in all channels
            string messageString = null;

            //This should allow firing to "General" channel
            //Check to validate this is in group context.
            if (m_sourceMessage.Conversation.IsGroup != true)
            {
                return null;
            }

            //Check the channelData eventType:
            JObject channelData = (JObject)m_sourceMessage.ChannelData;

            string eventType = (string)channelData["eventType"];

            if (eventType != null)
            {

                if (eventType == "teamMemberAdded")
                {
                    return AddMemberEvent();
                }
                else if (eventType == "teamMemberRemoved")
                {

                    return RemoveMemberEvent();

                }
                else if (eventType == "channelCreated")
                {
                    return ChannelCreatedEvent();
                }
            else
            {
                //unknown event?
                messageString = "ActivityType: ConversationUpdate\r\n\r\n";
                messageString += "Unhandled event: " + channelData["eventType"];
            }


            }
            else
            {
                //Ignore I guess?
                return null;
            }

            return messageString;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string AddMemberEvent()
        {
            string messageString = "Event: conversationUpdate\r\n\r\nteamEvent: teamMemberAdded\r\n";

            bool addedBot = false;
            //Create a string of the added members.  Or if one of the members added was the bot, show the welcome message instead.
            for (int i = 0; i < m_sourceMessage.MembersAdded.Count; i++)
            {
                messageString += "\r\nMember" + m_sourceMessage.MembersAdded[i].Id;
                if (m_sourceMessage.MembersAdded[i].Id == m_sourceMessage.Recipient.Id)
                {
                    addedBot = true;
                    break;
                }
            }
            if (addedBot)
            {
                messageString = "Hello, I'm Teams TestBot, a handy tool to test Teams functionality.  This message was trigged by `conversationUpdate`, where `ChannelData:eventType` is `teamMemberAdded`, and I was one of the added members.  \r\n\r\nFor more information about what I do, type **help**.";
            }

            return messageString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string RemoveMemberEvent()
        {
            //Note: if you remove the Bot, you cannot send the reply back, as it's no longer part of the team.

            string messageString = "Event: conversationUpdate\r\n\r\neventType: teamMemberRemoved\r\n";

            bool deletedBot = false;
            //Create a string of the deleted members.  Or if one of the members added was the bot, show the welcome message instead.
            for (int i = 0; i < m_sourceMessage.MembersRemoved.Count; i++)
            {
                messageString += "\r\nMember" + m_sourceMessage.MembersRemoved[i].Id;
                if (m_sourceMessage.MembersRemoved[i].Id == m_sourceMessage.Recipient.Id)
                {
                    deletedBot = true;
                    break;
                }
            }
            if (deletedBot)
            {
                //This won't display anything, but you could do something here to clean up cached information for the team.
                // The TeamsID will be returned in ChannelData
                messageString = null;
              }

            return messageString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string ChannelCreatedEvent()
        {
            string messageString = "Event: conversationUpdate\r\n\r\neventType: channelCreated\r\n";


            //Get the channelData eventType:
            JObject channelData = (JObject)m_sourceMessage.ChannelData;
            JObject channelInfo = (JObject)channelData["channel"];

            messageString += "New channel id: " + (string)channelInfo["id"];
            messageString += "New channel name: " + (string)channelInfo["name"];

            return messageString;
        }
    }


}