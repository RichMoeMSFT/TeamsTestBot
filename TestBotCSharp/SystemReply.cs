using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace TestBotCSharp
{
    public class SystemReply
    {

        private Activity sourceMessage;
        private Activity replyMessage;


        private ConnectorClient connector;

        public SystemReply(ConnectorClient c)
        {
            connector = c;

        }

        public Activity CreateMessage(Activity messageIn)
        {
            sourceMessage = messageIn;
            replyMessage = null;
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
            if (messageString != null)
            {
                replyMessage = messageIn.CreateReply();
                replyMessage.Text = messageString;
            }

            return replyMessage;
        }

        private string ConversationUpdate()
        {

            // Handle conversation state changes, like members being added and removed
            // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
            // Not available in all channels
            string messageString = null;

            //This should allow firing to "General" channel
            //Check to validate this is in group context.
            if (sourceMessage.Conversation.IsGroup != true)
            {
                return null;
            }

            //Check the channelData eventType:
            JObject channelData = (JObject)sourceMessage.ChannelData;

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
                else
                {
                    //unkown event?
                    messageString = "ActivityType: ConversationUpdate\r\n\r\n";
                    messageString += "Unknown event: " + channelData["eventType"];
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
            for (int i = 0; i < sourceMessage.MembersAdded.Count; i++)
            {
                messageString += "\r\nMember" + sourceMessage.MembersAdded[i].Id;
                if (sourceMessage.MembersAdded[i].Id == sourceMessage.Recipient.Id)
                {
                    addedBot = true;
                    break;
                }
            }
            if (addedBot)
            {
                messageString = "Hello, I'm Teams TestBot, a handy tool to test Teams functionality.  This message was trigged by conversationUpdate, and I was one of the added members.  For more information about what I do, type help.";
            }

            return messageString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string RemoveMemberEvent()
        {
            //This is not working!


            string messageString = "Event: conversationUpdate\r\n\r\nteamEvent: teamMemberRemoved\r\n";

            bool deletedBot = false;
            //Create a string of the deleted members.  Or if one of the members added was the bot, show the welcome message instead.
            for (int i = 0; i < sourceMessage.MembersRemoved.Count; i++)
            {
                messageString += "\r\nMember" + sourceMessage.MembersRemoved[i].Id;
                if (sourceMessage.MembersRemoved[i].Id == sourceMessage.Recipient.Id)
                {
                    deletedBot = true;
                    break;
                }
            }
            if (deletedBot)
            {
                messageString = "I see you are deleted Teams TestBot, which makes me sad. This message was trigged by conversationUpdate, and I was one of the deleted members members.  Thanks for trying me, come back again real soon!";
            }

            return messageString;
        }
    }


}