using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web;


namespace TestBotCSharp
{


    public class TestReply
    {
        private static string S_STANDARD_IMGURL = "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/panoramic.png";

        private Activity sourceMessage;
        private Activity replyMessage;
        private List<string> args;

        private string debugStr;

        private int dumpRequested = 0;
            static private int DUMPIN = 1;
            static private int DUMPOUT = 2;
            static private char CHAR_DUMP = '|';

        private ConnectorClient connector;
        
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

        private readonly Dictionary<string, TestDetail> cmdToTestDetail;

        public TestReply(ConnectorClient c)
        {
            connector = c;

            cmdToTestDetail = new Dictionary<string, TestDetail>(StringComparer.InvariantCultureIgnoreCase);
            cmdToTestDetail.Add("help", new TestDetail("Show this message", HelpMessage));

            cmdToTestDetail.Add("hero1", new TestDetail("Hero Card with [3] buttons", Hero1Message));
            cmdToTestDetail.Add("hero2", new TestDetail("Hero Card with no image and [3] buttons", Hero2Message));
            cmdToTestDetail.Add("hero3", new TestDetail("Hero Card with no content and [\"Optional Title\"]", Hero3Message));
            cmdToTestDetail.Add("hero4", new TestDetail("Hero Card with no content and [\"Optional Title\"]", Hero4Message));
            cmdToTestDetail.Add("imgCard", new TestDetail("Hero Card with [\"img\"] as Content", ImgCardMessage));
            cmdToTestDetail.Add("heroRYO", new TestDetail("Roll your own: [\"Title\"] [\"SubTitle\"] [\"Content\"] [\"ImageURL\"] [Buttons] ", HeroRYOMessage));

            cmdToTestDetail.Add("carousel1", new TestDetail("Show a Carousel with different cards in each", Carousel1Message));
            cmdToTestDetail.Add("carouselx", new TestDetail("Show a Carousel with [5] identical cards", CarouselxMessage));

            cmdToTestDetail.Add("signin", new TestDetail("Show a Signin Card",SignInMessage));
            cmdToTestDetail.Add("formatxml", new TestDetail("Display a [\"sample\"] selection of XML formats", FormatXMLMessage));
            cmdToTestDetail.Add("formatmd", new TestDetail("Display a [\"sample\"] selection of Markdown formats", FormatMDMessage));
            cmdToTestDetail.Add("thumb", new TestDetail("Display a Thumbnail Card", ThumbnailMessage));

            cmdToTestDetail.Add("echo", new TestDetail("Echo you [\"string\"]", EchoMessage));
            cmdToTestDetail.Add("mentions", new TestDetail("Show the @mentions you pass", MentionsTest));
            cmdToTestDetail.Add("members", new TestDetail("Show members of the team", MembersTest));

            cmdToTestDetail.Add("create", new TestDetail("Create a new conversation", CreateTest));

            cmdToTestDetail.Add("dumpin", new TestDetail("Display the incoming JSON", ActivityDumpIn));
            cmdToTestDetail.Add("dumpout", new TestDetail("Display the outgoing JSON", ActivityDumpOut));
        }

        /// <summary>
        /// Remove arg[0] from message string
        /// </summary>
        /// <returns></returns>
        private string StripCommandFromMessage()
        {
            string message = sourceMessage.Text;

            message = message.Replace(args[0], "");

            return message;
        }

        /// <summary>
        /// Strips out the bot name, which is passed as part of message when bot is referenced in-channel.
        /// </summary>
        /// <param name="message">the Activity text</param>
        /// <returns></returns>
        string StripBotNameFromText (string message)
        {
            var messageText = message;

            Mention[] m = sourceMessage.GetMentions();

            for (int i = 0;i < m.Length;i++)
            {
                if (m[i].Mentioned.Id == sourceMessage.Recipient.Id)
                {
                    messageText = messageText.Replace(m[i].Text, "");
                }
            }


            return messageText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageIn"></param>
        /// <returns></returns>
        public Activity CreateTestMessage(Activity messageIn)
        {
            sourceMessage = messageIn;
            string messageText = StripBotNameFromText(messageIn.Text);

            //For cases where bot is mentioned in channel, the bot name will be in the text, so cut it out.
#if false


            string botName = sourceMessage.Recipient.Name;
            if (botName != null)
            {
                messageText = messageText.Replace(botName, "");
                debugStr = messageText;
            } else
            {
                debugStr = null;
            }
#endif


            //Split into arguments.  If in quotes, treat entire string as a single arg.
            args = messageText.Split('"')
                                 .Select((element, index) => index % 2 == 0  // If even index
                                                       ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)  // Split the item
                                                       : new string[] { element })  // Keep the entire item
                                 .SelectMany(element => element).ToList();

            //one more pass to remove empties and whitespace
            args = args.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            for (int i = 0;i < args.Count;i++)
            {
                args[i] = args[i].Trim();
            }

#if false
            debugStr = "";
            for (int i = 0; i < args[0].Length; i++)
            {
                debugStr += "<br> [" + args[0][i] + "]";
            }

#endif
            replyMessage = messageIn.CreateReply();


            if (args.Count > 0)
            {
                string testcommand = args[0];

                //Trace.WriteLine("Received test command: " + testcommand);

                //Scan for dump tag - DumpIn = prepend, DumpOut = postpend
                if (testcommand[0] == CHAR_DUMP)
                {
                    testcommand = testcommand.Substring(1);
                    dumpRequested = DUMPIN;
                }
                else
                {
                    if (testcommand[testcommand.Length-1] == CHAR_DUMP)
                    {
                        testcommand = testcommand.Remove(testcommand.Length - 1);
                        dumpRequested = DUMPOUT;
                    }
                }


                if (cmdToTestDetail.ContainsKey(testcommand))
                {
                    cmdToTestDetail[testcommand].buildMessage();
                }
                else
                {
                    dumpRequested = DUMPIN;
                    HelpMessage();
                }

            }
            else
            {
                HelpMessage();
            }


            return replyMessage;
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void CreateTest()
        {

            //Check to validate this is in group context.
            if (sourceMessage.Conversation.IsGroup != true)
            {
                replyMessage.Text = "CreateConversation only work in channel context at this time";
                return;
            }

            var channelData = sourceMessage.ChannelData;


            replyMessage.Text = "This is a new Conversation created with CreateConversation().";
            replyMessage.Text += "<br/><br/> ChannelID = " + sourceMessage.ChannelId;
            replyMessage.Text += "<br/>ConversationID (in) = " + sourceMessage.Conversation.Id;
 
            ConversationParameters conversationParams = new ConversationParameters(
                isGroup: true,
                bot: null,
                members: null,
                topicName: "Test Conversation",
                activity: (Activity)replyMessage ,
                channelData: channelData
            );

            var conversationID = connector.Conversations.CreateConversation(conversationParams);

            replyMessage.Text += "<br/>ConversationID (out) = " + conversationID.Id;
            replyMessage.Text += "<br/>ChannelData: " + channelData.ToString();

            //replyMessage.Conversation = new ConversationAccount(id: conversationID.Id);
            replyMessage.Conversation = new ConversationAccount(id: conversationID.Id);

              /*
            ConversationParameters cpMessage = new ConversationParameters(message.Recipient, true, participants, "Quarter End Discussion");
            var conversationId = await connector.Conversations.CreateConversationAsync(cpMessage);
            IMessageActivity message = Activity.CreateMessageActivity();
            message.From = botAccount;
            message.Recipient = new ChannelAccount("lydia@contoso.com", "Lydia the CFO"));
            message.Conversation = new ConversationAccount(id: conversationId.Id);
            message.ChannelId = incomingMessage.ChannelId;
            */

            //await connector.Conversations.SendToConversationAsync(replyMessage);
            //if (dumpReply != null)
            //await connector.Conversations.SendToConversationAsync(dumpReply);


        }

        /// <summary>
        /// 
        /// </summary>
        private void MembersTest()
        {

            //Check to validate this is in group context.
            if (sourceMessage.Conversation.IsGroup != true)
            {
                replyMessage.Text = "GetConversationMembers only work in channel context at this time";
                return;
            }
            replyMessage.Text = "These are the member userids returned by the GetConversationMembers() function";

            ChannelAccount[] members = connector.Conversations.GetConversationMembers(sourceMessage.Conversation.Id);

            replyMessage.Text = "These are the member userids returned by the GetConversationMembers() function";

            for (int i = 0; i < members.Length; i++)
            {
                replyMessage.Text += "<br />" + members[i].Id + " : " + members[i].Name;
            }
        }


        /// <summary>
        /// Show the payload for either the source or reply message
        /// </summary>
        /// <param name="messageIn">The message that the bot received</param>
        /// <param name="messageOut">The test message that the bot created</param>
        /// <returns></returns>
        public Activity DumpMessage(Activity messageIn, Activity messageOut)
        {

            Activity temp = messageIn.CreateReply();

            if (dumpRequested == 1)
            {
                temp.Text = ActivityDumper.ActivityDump(messageIn);
            } else
            {
                if (dumpRequested == 2)
                {
                    temp.Text = ActivityDumper.ActivityDump(messageOut);
                }
                else
                {
                    return null;
                }
            }

            return temp;

        }

        /// <summary>
        /// Display a list of all the tests supported
        /// </summary>
        private void HelpMessage ()
        {
            var outText = "You entered [" + sourceMessage.Text + "]<br />";

#if false
            for (int i = 0; i < args.Count; i++)
            {
                outText += "<br />args[" + (i) + "] = [" + args[(i)] + "] - Leng: " + args[i].Length;
            }

#endif
            if (debugStr != null)
            {
                outText += "Debug: [" + debugStr + "]<br /><br />";
            }


            outText += "<br />** A list of all valid tests.** <br /> <br />  Values in [] can be changed by adding appropriate arguments, e.g. 'hero1 5' makes a hero1 card with 5 buttons; 'hero3 \"This is a Title\"' uses that string as the title.<br /> <br />You can prepend or postpend '|' (pipe) to dump the payload for incoming or outgoing message, respectively. <br /> <br /> ---";

            foreach(var item in cmdToTestDetail)
            {
                outText += "<br />**" + item.Key + "** - " + item.Value.about;
            }


            replyMessage.Text = outText;

        }

        /// <summary>
        /// 
        /// </summary>
        private void ActivityDumpIn ()
        {
            replyMessage.Text = ActivityDumper.ActivityDump(sourceMessage);
            replyMessage.TextFormat = TextFormatTypes.Markdown;
           
        }

        /// <summary>
        /// 
        /// </summary>
        private void ActivityDumpOut()
        {
            replyMessage.Text = "Dump out text";
            replyMessage.Text = ActivityDumper.ActivityDump(replyMessage);
            replyMessage.TextFormat = TextFormatTypes.Markdown;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argnum"></param>
        /// <returns></returns>
        private string GetArg(int argnum)
        {
            //Count 1 based, argnum is 0 based
            if (args.Count > argnum)
                return args[argnum];
            else
                return null;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argnum"></param>
        /// <returns></returns>
        private int GetArgInt(int argnum)
        {

            if (args.Count > argnum)
                return Convert.ToInt32(args[argnum]);
            else
                return -1;

        }

        /// <summary>
        /// 
        /// </summary>
        private void Hero1Message()
        {
            int numberOfButtons = GetArgInt(1);
            if (numberOfButtons == -1) numberOfButtons = 3;

            replyMessage.Attachments = new List<Attachment>()
            {

                GetHeroCardAttachment(
                    "Subject Title",
                    "Subtitle or breadcrumb",
                    "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                    new string[] { S_STANDARD_IMGURL },
                    //new string[] { "View in article", "See more like this", "/test ipsum" }
                    CreateButtons(numberOfButtons)
                )
            };

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private string[] CreateButtons(int num)
        {
            if (num < 1) return null;

            var buttons = new string[num];
            for(int i = 1; i <=num;i++)
            {
                buttons[(i-1)] = "Button " + i;
            }

            return buttons;
        }


        private void Hero2Message()
        {
            int numberOfButtons = GetArgInt(1);
            if (numberOfButtons == -1) numberOfButtons = 3;


            //No Image, 3 buttons
            replyMessage.Attachments = new List<Attachment>()
            {
                GetHeroCardAttachment(
                    "Subject Title",
                    "Subtitle or breadcrumb",
                    "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                    null,
                    CreateButtons(numberOfButtons)
                )
            };
        }

        private void Hero3Message()
        {
            string title = GetArg(1);

            replyMessage.Attachments = new List<Attachment>()
            {
                GetHeroCardAttachment(
                    title,
                    null,
                    null,
                    null,
                    CreateButtons(5)
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

            replyMessage.Attachments = new List<Attachment>()
            {
                GetHeroCardAttachment(
                    null,
                    null,
                    null,
                    new string[] { imgURL  },
                    CreateButtons(5)
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

            replyMessage.Attachments = new List<Attachment>()
            {
                GetHeroCardAttachment(
                    title,
                    subTitle,
                    content,
                    (imgURL == null ? null : new string[] { imgURL  }),
                    CreateButtons(buttonCount)
                )
            };

        }

        private void ImgCardMessage()
        {

            replyMessage.Attachments = new List<Attachment>()
            {
                GetHeroCardAttachment(
                    "Card with image containing no width or height",
                    null,
                    "<img src='https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/panoramic.png'/>",
                    null,
                    new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" })
            };

        }


        private void Carousel1Message()
        {

            replyMessage.Attachments = new List<Attachment>()
            {
                GetHeroCardAttachment(
                    null,
                    null,
                    null,
                    new string[] { S_STANDARD_IMGURL },
                    new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" }
                ),
                GetHeroCardAttachment(
                    "Subject Title Caraousel 1",
                    null,
                    null,
                    null,
                    new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" }
                ),
                 GetHeroCardAttachment(
                    "Subject Title Caraousel 1",
                    "Subtitle or breadcrumb",
                    "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                    null,
                    new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" }
                ),
                GetHeroCardAttachment(
                    "Subject Title Caraousel 1",
                    "Subtitle or breadcrumb",
                    "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                    new string[] { S_STANDARD_IMGURL },
                    new string[] { "View in article", "See more like this", "Action 3" }
                ),
                GetHeroCardAttachment(
                    "Subject Title Caraousel 1",
                    null,
                    "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                    null,
                    new string[] { "View in article", "See more like this", "Action 3" }
                )                
           };
            replyMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;

        }

        private void CarouselxMessage()
        {
            int numberOfCards = GetArgInt(1);
            if (numberOfCards == -1) numberOfCards = 5;

            var card = GetHeroCardAttachment(
                "Subject Title Carouselx",
                "Note: Teams currentlysupposrts a max of 5 cards",
                "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                new string[] { S_STANDARD_IMGURL },
                new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5", "Action 6", "Action 7" }); // Teams only support 6 actions max. Send more.

            var attachments = new List<Attachment>();

            for (var i = 0; i < numberOfCards; i++) // Teams only supports 5 attachments, sending more than that causes a Chat Service issue.
            {
                attachments.Add(card);
            }

            replyMessage.Attachments = attachments;
            replyMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;

        }

        private void SignInMessage()
        {
            var card = new Attachment()
            {
                ContentType = SigninCard.ContentType,
                Content = new SigninCard()
                {
                    Text = "Time to sign in",
                    Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "Please sign in",
                            Type = ActionTypes.OpenUrl,
                            Value = "https://www.bing.com"
                        }
                    }
                }
            };
            replyMessage.Attachments = new List<Attachment> { card };

        }

 
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
                        "<ul><li>Unordered item 1</li><li>Unordered item 2</li><li>Unordered item 3</li></ul>",
                        "<ol><li>Ordered item 1</li><li>Ordered item 2</li><li>Ordered item 3</li></oll>",
 
                        "<a href='https://bing.com'>Link</a>",
                        "<img src='http://aka.ms/Fo983c' alt='Test image' />"
                };

                text = string.Join(string.Empty, tmp);
            }

            replyMessage.Text = text;
            replyMessage.TextFormat = TextFormatTypes.Xml;
        }

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
                    "---",
                    "* Unordered item 1<br />* Unordered item 2<br />* Unordered item 3",
                    "1. Ordered item 1<br />2. Ordered item 2<br />3. Ordered item 3",

                    "[Link](https://bing.com)",
                    "![Alt Text](http://aka.ms/Fo983c)"
                };

                text = string.Join("<br />", tmp);
            }

            replyMessage.Text = text;
            replyMessage.TextFormat = TextFormatTypes.Markdown;
        }


        /// <summary>
        /// 
        /// </summary>
        private void EchoMessage()
        {

            /*
            var text = GetArg(1);
            if (text == null)
            {
                text = "Input your message and it will display here in Markdown.";
            }
            */

            var text = StripCommandFromMessage();

            replyMessage.Text = text;
            replyMessage.TextFormat = TextFormatTypes.Markdown;
        }


        private void ThumbnailMessage()
        {

            var card = GetThumbnailCardAttachment(
                "Homegrown Thumbnail Card",
                "Sandwiches and salads",
                "104 Lake St, Kirkland, WA 98033<br />(425) 123-4567",
                new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/sandwich_thumbnail.png" },
                new string[] { "View in article", "See more like this" });

            replyMessage.Attachments = new List<Attachment> { card };


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
        private static Attachment GetHeroCardAttachment(string title, string subTitle, string text, string[] images, string[] buttons)
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
                ContentType = HeroCard.ContentType,
                Content = heroCard,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private void MentionsTest()
        {
            Mention[]  m = sourceMessage.GetMentions();

            var text = "You mentioned " + m.Length + " entities";
            for (int i = 0; i < m.Length; i++)
            {
                text += "<br />Text: " + m[i].Text + ", name: " + m[i].Mentioned.Name;
            }

            replyMessage.Text = text;
            replyMessage.TextFormat = TextFormatTypes.Markdown;

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

