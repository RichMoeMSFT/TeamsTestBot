using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json.Linq;


namespace TestBotCSharp
{


    public class TestReply : TestBotReply
    {
        private static string S_STANDARD_IMGURL = "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/panoramic.png";

 
        private int m_dumpRequested = 0;
            static private int DUMPIN = 1;
            static private int DUMPOUT = 2;
            static private char CHAR_DUMP = '|';

        
        //Dictionary - this is a combo of the about string and the command to run.
        public struct TestDetail
        {
            public string about;
            public Action buildMessage;

            public TestDetail(string x, Action y)
            {
                about = x;
                buildMessage = y;
            }

        }

        private readonly Dictionary<string, TestDetail> m_cmdToTestDetail;

        public TestReply(ConnectorClient c) : base (c)
        {
           

            m_cmdToTestDetail = new Dictionary<string, TestDetail>(StringComparer.InvariantCultureIgnoreCase);
            m_cmdToTestDetail.Add("help", new TestDetail("Show this message", HelpMessage));
            m_cmdToTestDetail.Add("!help", new TestDetail("!Show all commands, including hidden", HelpVerbose));

            m_cmdToTestDetail.Add("hero1", new TestDetail("Hero Card with [3] buttons", Hero1Message));
            m_cmdToTestDetail.Add("hero2", new TestDetail("!Hero Card with no image and [3] buttons", Hero2Message));
            m_cmdToTestDetail.Add("hero3", new TestDetail("!Hero Card with no content and [\"Optional Title\"]", Hero3Message));
            m_cmdToTestDetail.Add("hero4", new TestDetail("!Hero Card with no content and [\"Optional Title\"]", Hero4Message));
            m_cmdToTestDetail.Add("imgCard", new TestDetail("Hero Card with [\"img\"] as Content", ImgCardMessage));
            m_cmdToTestDetail.Add("heroRYO", new TestDetail("Roll your own: [\"Title\"] [\"SubTitle\"] [\"Content\"] [\"ImageURL\"] [Buttons] ", HeroRYOMessage));

            m_cmdToTestDetail.Add("heroInvoke", new TestDetail("Hero Card with [2] buttons using invoke action type", HeroInvokeMessage));

            m_cmdToTestDetail.Add("carousel1", new TestDetail("Show a Carousel with different cards in each", Carousel1Message));
            m_cmdToTestDetail.Add("carouselx", new TestDetail("Show a Carousel with [5] identical cards", CarouselxMessage));

            m_cmdToTestDetail.Add("signin", new TestDetail("Show a Signin Card, with button to launch [URL]",SignInMessage));
            m_cmdToTestDetail.Add("formatxml", new TestDetail("Display a [\"sample\"] selection of XML formats", FormatXMLMessage));
            m_cmdToTestDetail.Add("formatmd", new TestDetail("Display a [\"sample\"] selection of Markdown formats", FormatMDMessage));
            m_cmdToTestDetail.Add("thumb", new TestDetail("Display a Thumbnail Card", ThumbnailMessage));

            m_cmdToTestDetail.Add("echo", new TestDetail("Echo your [\"string\"]", EchoMessage));
            m_cmdToTestDetail.Add("mentions", new TestDetail("Show the @mentions you pass", MentionsTest));
            m_cmdToTestDetail.Add("mentionUser", new TestDetail("@mentions the passed user", MentionUser));

            m_cmdToTestDetail.Add("members", new TestDetail("Show members of the team", MembersTest));

            m_cmdToTestDetail.Add("create", new TestDetail("Create a new conversation", CreateConversation));
            m_cmdToTestDetail.Add("create11", new TestDetail("!Create a new 1:1 conversation", Create11Conversation));

            m_cmdToTestDetail.Add("imback", new TestDetail("!This is just a handler for the imback buttons", ImBackResponse));

            m_cmdToTestDetail.Add("dumpin", new TestDetail("Display the incoming JSON", ActivityDumpIn));
            m_cmdToTestDetail.Add("dumpout", new TestDetail("Display the outgoing JSON", ActivityDumpOut));

        }

        /// <summary>
        /// Convert emoji code into surrogate pair
        /// </summary>
        /// <param name="emoji"></param>
        /// <returns></returns>
        private string EmojiToSurrogatePair(int emoji)
        {
            double H = Math.Floor((double)((emoji - 0x10000) / 0x400)) + 0xD800;
            double L = ((emoji - 0x10000) % 0x400) + 0xDC00;
            char ch = (char)H;
            char cl = (char)L;

            return ch.ToString() + cl.ToString();
        }

        /// <summary>
        /// Remove arg[0] from message string, which should be the command itself.  Used in EchoTest for ow
        /// </summary>
        /// <returns></returns>
        private string StripCommandFromMessage()
        {
            string message = m_sourceMessage.Text;

            message = message.Replace(m_args[0], "");

            return message;
        }

        /// <summary>
        /// Strips out the bot name, which is passed as part of message when bot is referenced in-channel.
        /// </summary>
        /// <param name="message">the Activity text</param>
        /// <returns>the message without the bot, if mentioned</returns>
        private string StripBotNameFromText (string message)
        {
            var messageText = message;

            Mention[] m = m_sourceMessage.GetMentions();

            for (int i = 0;i < m.Length;i++)
            {
                if (m[i].Mentioned.Id == m_sourceMessage.Recipient.Id)
                {
                    if (m[i].Text != null) //the Text field contains the full <at>name</at> string so is useful for stripping out.  If it's null, though, the bot name was passed silently, for e.g. bot-in-channel imBack
                        messageText = messageText.Replace(m[i].Text, "");
                }
            }

  
            return messageText;
        }

        /// <summary>
        /// Safe return of arg# as String
        /// </summary>
        /// <param name="argnum"></param>
        /// <returns>String contained in arg# or null, if no arg</returns>
        private string GetArg(int argnum)
        {
            //Count 1 based, argnum is 0 based
            if (m_args.Count > argnum)
                return m_args[argnum];
            else
                return null;

        }


        /// <summary>
        /// Safe return of arg# as Int
        /// </summary>
        /// <param name="argnum"></param>
        /// <returns>Int contained in arg# or -1, if no arg</returns>
        private int GetArgInt(int argnum)
        {

            if (m_args.Count > argnum)
                return Convert.ToInt32(m_args[argnum]);
            else
                return -1;

        }

        /// <summary>
        /// Core dispatcher.  Parse the messageIn text, stripping out bot name if included and assuming the first string is the command.  Split rest of string into optional args for comands.
        /// </summary>
        /// <param name="messageIn"></param>
        /// <returns></returns>
        public override Activity CreateMessage(Activity messageIn)
        {
            m_sourceMessage = messageIn; //Store off so we don't pass around



            //Create the message as a simple Reply
            m_replyMessage = messageIn.CreateReply();

            if (messageIn.Type == ActivityTypes.Invoke)
            {
                InvokeResponse();  
            }
            else
            {

                string messageText = StripBotNameFromText(messageIn.Text); //This will strip out the botname if the message came via channel and therefore it's mentioned

                //Split into arguments.  If in quotes, treat entire string as a single arg.
                m_args = messageText.Split('"')
                                     .Select((element, index) => index % 2 == 0  // If even index
                                                           ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)  // Split the item
                                                           : new string[] { element })  // Keep the entire item
                                     .SelectMany(element => element).ToList();

                //one more pass to remove empties and whitespace
                m_args = m_args.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                for (int i = 0; i < m_args.Count; i++)
                {
                    m_args[i] = m_args[i].Trim();
                }

                if (m_args.Count > 0)
                {
                    string testcommand = m_args[0];
                    m_dumpRequested = 0;

                    //Scan for dump tag - DumpIn = prepend, DumpOut = postpend
                    if (testcommand[0] == CHAR_DUMP)
                    {
                        testcommand = testcommand.Substring(1);
                        m_dumpRequested += DUMPIN;
                    }

                    if (testcommand[testcommand.Length - 1] == CHAR_DUMP)
                    {
                        testcommand = testcommand.Remove(testcommand.Length - 1);
                        m_dumpRequested += DUMPOUT;
                    }


                    //Dispatch the command - check dictionary for command and run the appropriate function.
                    if (m_cmdToTestDetail.ContainsKey(testcommand))
                    {
                        m_cmdToTestDetail[testcommand].buildMessage();
                    }
                    else
                    {
                        m_dumpRequested = DUMPIN;
                        HelpMessage();
                    }

                }
                else
                {
                    HelpMessage();
                }
            }

            return m_replyMessage;
        }


        /// <summary>
        /// Show the payload for either the source or reply message.  The flag m_dumpRequested is set in the message parsing, based on the location of the pipe character.
        /// </summary>
        /// <param name="messageIn">The message that the bot received</param>
        /// <param name="messageOut">The test message that the bot created</param>
        /// <returns></returns>
        public override Activity DumpMessage(Activity messageIn, Activity messageOut)
        {

            if (m_dumpRequested == 0) return null;

            Activity temp = messageIn.CreateReply();

            temp.Text = "";

            if ((m_dumpRequested & 1) == 1)
            {
                temp.Text += "<b>ActivityIn:</b><br/>";
                temp.Text += ActivityDumper.ActivityDump(messageIn);
            }

            if (m_dumpRequested == 3) temp.Text += "<br />< hr ><br />"; //separator if both

            if ((m_dumpRequested & 2) == 2)
            {
                temp.Text += "<b>ActivityOut:</b><br/>";
                temp.Text += ActivityDumper.ActivityDump(messageOut);

            }
            
            temp.TextFormat = TextFormatTypes.Xml;

            return temp;

        }

        /// <summary>
        /// Show a list of all available commands
        /// </summary>
        /// <param name="showAll">set to True to show hidden tests as well</param>
        private void HelpDisplay (bool showAll = false)
        {
            var outText = "You entered [" + m_sourceMessage.Text + "]<br />";

#if false
            for (int i = 0; i < args.Count; i++)
            {
                outText += "<br />args[" + (i) + "] = [" + args[(i)] + "] - Leng: " + args[i].Length;
            }

#endif

            outText += "<br />** A list of all valid tests.** <br /> <br />  Values in [] can be changed by adding appropriate arguments, e.g. 'hero1 5' makes a hero1 card with 5 buttons; 'hero3 \"This is a Title\"' uses that string as the title.<br /> <br />You can prepend or postpend '|' (pipe) to dump the payload for incoming or outgoing message, respectively. <br /> <br /> ---";

            foreach(var item in m_cmdToTestDetail)
            {
                //If first char of Description is ! don't display it in help, unless command has ! as first char (e.g. !help).  So we can have hidden test cases.
                if (showAll || item.Value.about[0] != '!')
                    outText += "<br />**" + item.Key + "** - " + item.Value.about;
            }

            m_replyMessage.Text = outText;

        }

        private void HelpMessage()
        {
            HelpDisplay(false);
        }
        private void HelpVerbose()
        {
            HelpDisplay(true);
        }

        /// <summary>
        /// Simply pass in the source as the Activity dump source. This is for the appropriate command, not for |
        /// </summary>
        private void ActivityDumpIn ()
        {
            m_replyMessage.Text = ActivityDumper.ActivityDump(m_sourceMessage);
            m_replyMessage.TextFormat = TextFormatTypes.Xml;
           
        }

        /// <summary>
        /// Simply pass in the reply as the Activity dump source.  This is for the appropriate command, not for |
        /// </summary>
        private void ActivityDumpOut()
        {
            m_replyMessage.Text = "Dump out text";
            m_replyMessage.Text = ActivityDumper.ActivityDump(m_replyMessage);
            m_replyMessage.TextFormat = TextFormatTypes.Xml;
        }


        /// <summary>
        /// Helper function to create buttons using ImBack action
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private CardAction[] CreateImBackButtons(int num)
        {
            if (num < 1) return null;

            var buttons = new CardAction[num];
            for (int i = 0; i < num; i++)
            {
                buttons[i] = new CardAction()
                {
                    Title = "ImBack " + i,
                    Type = ActionTypes.ImBack,
                    Value = "ImBack " + i
                };
            }

            return buttons;
        }


        /// <summary>
        /// Helper function to create buttons using Invoke action
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private CardAction[] CreateInvokeButtons(int num)
        {
            if (num < 1) return null;

            var buttons = new CardAction[num];
            for (int i = 0; i < num; i++)
            {
                buttons[i] = new CardAction()
                {
                    Title = "Invoke " + i,
                    Type = "invoke",
                    Value = "{\"invokeValue:\": \"" + i + "\"}"
                };
            }

            return buttons;
        }




        /// <summary>
        /// Hero card using Invoke buttons
        /// </summary>
        private void HeroInvokeMessage()
        {
            int numberOfButtons = GetArgInt(1);
            if (numberOfButtons == -1) numberOfButtons = 2;

            m_replyMessage.Attachments = new List<Attachment>()
            {

                GetHeroCardAttachment(
                    "Invoke",
                    "Hero card with invoke buttons",
                    "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                    null, 
                    CreateInvokeButtons(numberOfButtons)
                )
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private void Hero1Message()
        {
            int numberOfButtons = GetArgInt(1);
            if (numberOfButtons == -1) numberOfButtons = 3;

            m_replyMessage.Attachments = new List<Attachment>()
            {

                GetHeroCardAttachment(
                    "Subject Title",
                    "Subtitle or breadcrumb",
                    "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                    new string[] { S_STANDARD_IMGURL },
                    CreateImBackButtons(numberOfButtons)
                )
            };

        }


        private void Hero2Message()
        {
            int numberOfButtons = GetArgInt(1);
            if (numberOfButtons == -1) numberOfButtons = 3;


            //No Image, 3 buttons
            m_replyMessage.Attachments = new List<Attachment>()
            {
                GetHeroCardAttachment(
                    "Subject Title",
                    "Subtitle or breadcrumb",
                    "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                    null,
                    CreateImBackButtons(numberOfButtons)
                )
            };
        }

        private void Hero3Message()
        {
            string title = GetArg(1);

            m_replyMessage.Attachments = new List<Attachment>()
            {
                GetHeroCardAttachment(
                    title,
                    null,
                    null,
                    null,
                    CreateImBackButtons(5)
                )
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private void Hero4Message()
        {
            string imgURL = GetArg(1);
            if (imgURL == null) imgURL = S_STANDARD_IMGURL;

            m_replyMessage.Attachments = new List<Attachment>()
            {
                GetHeroCardAttachment(
                    null,
                    null,
                    null,
                    new string[] { imgURL  },
                    CreateInvokeButtons(5)
                )
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private void HeroRYOMessage()
        {
            string title = GetArg(1);
            string subTitle = GetArg(2);
            string content = GetArg(3);
            string imgURL = GetArg(4);
            int buttonCount = GetArgInt(5);

            m_replyMessage.Attachments = new List<Attachment>()
            {
                GetHeroCardAttachment(
                    title,
                    subTitle,
                    content,
                    (imgURL == null ? null : new string[] { imgURL  }),
                    CreateImBackButtons(buttonCount)
                )
            };

        }

        /// <summary>
        /// This will display an Img in the Content section of the Attachment, instead of the Image section.
        /// </summary>
        private void ImgCardMessage()
        {

            string imgURL = GetArg(1);
            if (imgURL == null) imgURL = S_STANDARD_IMGURL;


            m_replyMessage.Attachments = new List<Attachment>()
            {
                GetHeroCardAttachment(
                    "Card with image containing no width or height",
                    null,
                    "<img src='" + imgURL + "'/>",
                    null,
                    CreateImBackButtons(2)
                )
            };

        }


        /// <summary>
        /// Carousel with 5 different cards
        /// </summary>
        private void Carousel1Message()
        {

            m_replyMessage.Attachments = new List<Attachment>()
            {
                GetHeroCardAttachment(
                    null,
                    null,
                    null,
                    new string[] { S_STANDARD_IMGURL },
                    CreateImBackButtons(5) 
                ),
                GetHeroCardAttachment(
                    "Subject Title Carousel 2",
                    null,
                    null,
                    null,
                    CreateImBackButtons(4) 
                 ),
                 GetHeroCardAttachment(
                    "Subject Title Carousel 3",
                    "Subtitle or breadcrumb",
                    "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                    null,
                    CreateInvokeButtons(3)
                ),
                GetHeroCardAttachment(
                    "Subject Title Carousel 4",
                    "Subtitle or breadcrumb",
                    "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                    new string[] { S_STANDARD_IMGURL },
                    CreateImBackButtons(2)
                ),
                GetHeroCardAttachment(
                    "Subject Title Caraousel 5",
                    null,
                    "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                    null,
                    CreateImBackButtons(1)
                )                
           };
            m_replyMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;

        }

        /// <summary>
        /// Carousel with 5 duplicate cards
        /// </summary>
        private void CarouselxMessage()
        {
            int numberOfCards = GetArgInt(1);
            if (numberOfCards == -1) numberOfCards = 5;

            var card = GetHeroCardAttachment(
                "Subject Title Carouselx",
                "Note: Teams currently supposrts a max of 5 cards",
                "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                new string[] { S_STANDARD_IMGURL },
                CreateImBackButtons(7)  // Teams only support 6 actions max. Send more.
             );

            var attachments = new List<Attachment>();

            for (var i = 0; i < numberOfCards; i++) // Teams only supports 5 attachments, sending more than that causes a Chat Service issue.
            {
                attachments.Add(card);
            }

            m_replyMessage.Attachments = attachments;
            m_replyMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;

        }


        /// <summary>
        /// Sign-in Card type
        /// </summary>
        private void SignInMessage()
        {
            string openURL = GetArg(1);
            if (openURL == null) openURL = "https://www.bing.com";

            var card = new Attachment()
            {
                ContentType = SigninCard.ContentType,
                Content = new SigninCard()
                {
                    Text = "Sample Sign-in with OpenURL action (launch " + openURL + ")",
                    Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "Sign In (signin type)",
                            Type = "signin",
                            Value = openURL
                        }
                    }
                }
            };
            m_replyMessage.Attachments = new List<Attachment> { card };

        }

        /// <summary>
        /// Respond to Invoke button click
        /// </summary>
        private void InvokeResponse()
        {

            var text = "### Received Invoke action from button. ###\n\n";
            text += "Payload is: \n\n";

            //Get payload here:
            JObject payload = (JObject) m_sourceMessage.Value;
            text += payload.ToString();


            m_replyMessage.Text = text;
            m_dumpRequested = DUMPIN;

        }

        /// <summary>
        /// Respond to ImBack button click
        /// </summary>
        private void ImBackResponse()
        {

            var text = "### Received ImBack action from button. ###\n\n";
            text += "Message is: \n\n";

            //Get payload here:
            text += m_sourceMessage.Text;


            m_replyMessage.Text = text;
            m_dumpRequested = DUMPIN;

        }

        /// <summary>
        /// Test XML formatting
        /// </summary>
        private void FormatXMLMessage()
        {
            var text = GetArg(1);
            if (text == null)
            {
                var tmp = new string[]
                {
                        "<h1>H1</h1>",
                        "<h2>H2</h2>",
                        "<h3>H3</h3>",
                        "<h4>H4</h4>",
                        "<h5>H5 (max)</h5>",
                        "<b>Bold</b>",
                        "<i>Italic</i>",
                        "<strike>Strike</strike>",
                        "<pre>code() using pre</pre>",
                        "<hr />",
                        "emoji - " + EmojiToSurrogatePair(0x1F37D),
                        "<ul><li>Unordered item 1</li><li>Unordered item 2</li><li>Unordered item 3</li></ul>",
                        "<ol><li>Ordered item 1</li><li>Ordered item 2</li><li>Ordered item 3</li></oll>",
 
                        "<a href='https://bing.com'>Link</a>",
                        "<img src='http://aka.ms/Fo983c' alt='Test image' />"
                };

                text = string.Join("<br />", tmp);
            }

            m_replyMessage.Text = text;
            m_replyMessage.TextFormat = TextFormatTypes.Xml;
        }

        /// <summary>
        /// 
        /// </summary>
        private void FormatMDMessage()
        {
            var text = GetArg(1);
            if (text == null)
            {
                var tmp = new string[]
                {
                    "# H1",
                    "## H2",
                    "### H3",
                    "#### H4",
                    "##### H5 (max)",
                    "**Bold**",
                    "*Italic*",
                    "~~Strike~~",
                    "`code()`",
                    "> Block",
                    "---",
                    "emoji - " + EmojiToSurrogatePair(0x1f37a), //\uD83C\uDF20 ",
                    "This is a Table:\n\n|Table Col 1|Col2|Column 3|\n|---|---|---|\n| R1C1 | Row 1 Column 2 | Row 1 Col 3 |\n|R2C1|R2C2|R2C3|\n\n",
                    "* Unordered item 1\n* Unordered item 2\n* Unordered item 3\n",
                    "1. Ordered item 1\n2. Ordered item 2\n3. Ordered item 3\n",

                    "[Link](https://bing.com)",
                    "![Alt Text](http://aka.ms/Fo983c)"
                };

                text = string.Join("\n\n", tmp);
            }

            m_replyMessage.Text = text;
            m_replyMessage.TextFormat = TextFormatTypes.Markdown;
        }


        /// <summary>
        /// Simple echo back with Markdown
        /// </summary>
        private void EchoMessage()
        {
     
            //Remove "echo" and take everything else
            var text = StripCommandFromMessage();

            m_replyMessage.Text = text;
            m_replyMessage.TextFormat = TextFormatTypes.Markdown;
        }


        /// <summary>
        /// 
        /// </summary>
        private void ThumbnailMessage()
        {

            var card = GetThumbnailCardAttachment(
                "Homegrown Thumbnail Card",
                "Sandwiches and salads",
                "104 Lake St, Kirkland, WA 98033<br />(425) 123-4567",
                new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/sandwich_thumbnail.png" },
                new string[] { "View in article", "See more like this" });

            m_replyMessage.Attachments = new List<Attachment> { card };


        }

        private void Create11Conversation()
        {

            m_conversationParams = new ConversationParameters(

                /*
                Bot = new ChannelAccount(ConfigurationManager.AppSettings["BotId"], "Leon's Test Bot"),
                Members = new ChannelAccount[] { new ChannelAccount(userId) },
                ChannelData = new ChannelData { Tenant = new Tenant { Id = tenantId } }
                */
                isGroup: false,
                bot: new ChannelAccount(m_sourceMessage.Recipient.Id, "Rich's bot!"),
                members: new ChannelAccount[] { new ChannelAccount(m_sourceMessage.From.Id) },
                channelData: m_sourceMessage.ChannelData
            );
 

            /*
            //Check to validate this is in group context.
            if (m_sourceMessage.Conversation.IsGroup != true)
            {
                m_replyMessage.Text = "CreateConversation only work in channel context at this time";
                return;
            }
            */

            m_replyMessage.Text = "This is a new Conversation created with CreateConversationAsync().";
            m_replyMessage.Text += "<br/><br/> ChannelID = " + m_sourceMessage.ChannelId;
            m_replyMessage.Text += "<br/>ConversationID (in) = " + m_sourceMessage.Conversation.Id;


            //Trigger a new conversation to be created in MessagesController:
            m_replyMessage.Conversation = null;
        }


        /// <summary>
        /// To test CreateConversationAsync set the Conversation to null, which triggers the creation of a new one in the MessageConroller post flow
        /// </summary>
        private void CreateConversation()
        {

            //Check to validate this is in group context.
            if (m_sourceMessage.Conversation.IsGroup != true)
            {
                m_replyMessage.Text = "CreateConversation only works in channel context at this time";
                return;
            }

            m_replyMessage.Text = "This is a new Conversation created with CreateConversationAsync().";
            m_replyMessage.Text += "<br/><br/> ChannelID = " + m_sourceMessage.ChannelId;
            m_replyMessage.Text += "<br/>ConversationID (in) = " + m_sourceMessage.Conversation.Id;


            m_conversationParams = new ConversationParameters(
                isGroup: true,
                bot: null,
                members: null,
                topicName: "New Conversation",
                activity: (Activity)m_replyMessage,
                channelData: m_sourceMessage.ChannelData
            );

            //Trigger a new conversation to be created in MessagesController:
            m_replyMessage.Conversation = null;

        }

        /// <summary>
        /// Retrieve and display all Team Members, leveraging the GetConversationMembers function from BotFramework.  Note that this only has relevance in a Group context.
        /// </summary>
        private void MembersTest()
        {

            //Check to validate this is in group context.
            if (m_sourceMessage.Conversation.IsGroup != true)
            {
                m_replyMessage.Text = "GetConversationMembers only work in channel context at this time";
                return;
            }

            ChannelAccount[] members = m_connector.Conversations.GetConversationMembers(m_sourceMessage.Conversation.Id);

            m_replyMessage.Text = "These are the member userids returned by the GetConversationMembers() function";

            for (int i = 0; i < members.Length; i++)
            {
                m_replyMessage.Text += "<br />" + members[i].Id + " : " + members[i].Name;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void MentionsTest()
        {
            Mention[]  m = m_sourceMessage.GetMentions();

            var text = "You mentioned " + m.Length + " entities";
            for (int i = 0; i < m.Length; i++)
            {
                text += "<br />Text: " + m[i].Text + ", name: " + m[i].Mentioned.Name;
            }

            m_replyMessage.Text = text;
            m_replyMessage.TextFormat = TextFormatTypes.Markdown;

        }

        /// <summary>
        /// 
        /// </summary>
        private void MentionUser()
        {
            Mention[] m = m_sourceMessage.GetMentions();

            string text = null;
            Mention mentionedUser = null;
            for (int i = 0; i < m.Length; i++)
            {
                if (m[i].Mentioned.Id != m_sourceMessage.Recipient.Id)
                {
                    //get the first non-bot user
                    mentionedUser = m[i];
                    break;
                }

            }
            if (mentionedUser != null)
            {
                text = "Here is a mention:  Hello " + mentionedUser.Text;
                m_replyMessage.Entities.Add((Entity)mentionedUser);
            }
            else
            {
                text = "No **users** mentioned";
            }

            m_replyMessage.Text = text;
            m_replyMessage.TextFormat = TextFormatTypes.Markdown;

        }

        /// <summary>
        /// Builds and returns a <see cref="HeroCard"/> attachment using the supplied info
        /// </summary>
        /// <param name="title">Title of the card</param>
        /// <param name="subTitle">Subtitle of the card</param>
        /// <param name="text">Text of the card</param>
        /// <param name="images">Images in the card</param>
        /// <param name="buttons">Buttons in the card</param>
        /// <returns>Card attachment</returns>
        private static Attachment GetHeroCardAttachment(string title, string subTitle, string text, string[] images, CardAction[] buttons, bool useInvoke = false)
        {
            var heroCard = new HeroCard()
            {
                Title = title,
                Subtitle = subTitle,
                Text = text,
                Images = new List<CardImage>(),
                Buttons = new List<CardAction>(),
            };

            // Set images
            if (images != null)
            {
                foreach (var img in images)
                {
                    heroCard.Images.Add(new CardImage()
                    {
                        Url = img,
                        Alt = img,
                    });
                }
            }

            // Set buttons
            if (buttons != null)
            {
                heroCard.Buttons = buttons;

            }

            return new Attachment()
            {
                ContentType = HeroCard.ContentType,
                Content = heroCard,
            };
        }


        /// <summary>
        /// Builds and returns a <see cref="ThumbnailCard"/> attachment using the supplied info
        /// </summary>
        /// <param name="title">Title of the card</param>
        /// <param name="subTitle">Subtitle of the card</param>
        /// <param name="text">Text of the card</param>
        /// <param name="images">Images in the card</param>
        /// <param name="buttons">Buttons in the card</param>
        /// <returns>Card attachment</returns>
        private static Attachment GetThumbnailCardAttachment(string title, string subTitle, string text, string[] images, string[] buttons)
        {
            var heroCard = new ThumbnailCard()
            {
                Title = title,
                Subtitle = subTitle,
                Text = text,
                Images = new List<CardImage>(),
                Buttons = new List<CardAction>(),
            };

            // Set images
            if (images != null)
            {
                foreach (var img in images)
                {
                    string altText = null;
                    if (img.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        altText = img;
                    }
                    else
                    {
                        altText = "The alt text for an image blob";
                    }

                    heroCard.Images.Add(new CardImage()
                    {
                        Url = img,
                        Alt = altText,
                    });
                }
            }

            // Set buttons
            if (buttons != null)
            {
                foreach (var btn in buttons)
                {
                    heroCard.Buttons.Add(new CardAction()
                    {
                        Title = btn,
                        Type = ActionTypes.ImBack,
                        Value = btn,
                    });
                }
            }

            return new Attachment()
            {
                ContentType = ThumbnailCard.ContentType,
                Content = heroCard,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numWords"></param>
        /// <param name="numSentences"></param>
        /// <param name="numLines"></param>
        /// <returns></returns>
        private static string LoremIpsum(int numWords, int numSentences, int numLines)
        {
            var words = new[] { "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer", "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod", "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat" };

            var rand = new Random();
            bool capitalize = true;

            var sb = new System.Text.StringBuilder();
            for (int p = 0; p < numLines; p++)
            {
                for (int s = 0; s < numSentences; s++)
                {
                    for (int w = 0; w < numWords; w++)
                    {
                        if (w > 0) { sb.Append(" "); }
                        string nextWord = (words[rand.Next(words.Length)]);
                        if (capitalize)
                        {
                            nextWord = char.ToUpper(nextWord[0]) + nextWord.Substring(1);
                            capitalize = false;
                        }
                        sb.Append(nextWord);                   
                        //sb.Append(words[rand.Next(words.Length)]);
                    }
                    sb.Append(". ");
                    capitalize = true;
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }



}

