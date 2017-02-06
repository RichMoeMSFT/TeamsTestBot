namespace TestBotCSharp
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System.Collections.Generic;

    /// <summary>
    /// Summary description for Class1
    /// </summary>


    [Serializable]
    public class TestsDialog : IDialog<object>
    {


        /// <summary>
        /// Trigger prefix
        /// </summary>
        private const string TRIGGER = "/test";

        /// <summary>
        /// User utterance to 
        /// </summary>
        private const string HELPTRIGGER = "help";

        /// <summary>
        /// 
        /// </summary>
        private const string EVERYTHINGTRIGGER = "everything";

        /// <summary>
        /// The mapping of trigger strings to their handlers
        /// </summary>
        private readonly Dictionary<string, Func<string, IEnumerable<TestReply>>> triggerToHandler;


        public TestsDialog()
        {
            this.triggerToHandler = new Dictionary<string, Func<string, IEnumerable<TestReply>>>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    TriggerIs(HELPTRIGGER),
                    (input) =>
                    {
                        var reply = string.Join("<br />", this.triggerToHandler.Keys);
                        return TestsDialog.BuildMessage(null, reply);
                    }
                },
                {
                    TriggerIs("hero1"),
                    (input) =>
                    {
                        return TestsDialog.BuildMessage(new List<Attachment>()
                        {
                            // Card with everything
                            TestsDialog.GetHeroCardAttachment(
                            "Subject Title",
                            "Subtitle or breadcrumb",
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            "attribution",
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/screenshot_16x9.png" },
                            new string[] { "View in article", "See more like this", "/test ipsum" })
                        });
                    }
                }




            };
        }


        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var testmessage = await argument;

            var testreply = makeTestReply(context, testmessage);

            await context.PostAsync("You said: " + testmessage.Text + ", " + testreply.Text);
            context.Wait(MessageReceivedAsync);
        }

        private IMessageActivity makeTestReply(IDialogContext context, IMessageActivity argument)
        {
            var testreply = context.MakeMessage();

            var temp = triggerToHandler[argument.Text]();
            testreply.Text = "Poop";
            return testreply;
        }


        /// <summary>
        /// Builds and returns a <see cref="HeroCard"/> attachment using the supplied info
        /// </summary>
        /// <param name="title">Title of the card</param>
        /// <param name="subTitle">Subtitle of the card</param>
        /// <param name="text">Text of the card</param>
        /// <param name="attribution">Attribution of the card</param>
        /// <param name="images">Images in the card</param>
        /// <param name="buttons">Buttons in the card</param>
        /// <returns>Card attachment</returns>
        private static Attachment GetHeroCardAttachment(string title, string subTitle, string text, string attribution, string[] images, string[] buttons)
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

        private static string GetTestFormattedText()
        {
            var tests = new string[]
            {
                "<b>Bold</b>",
                "<i>Italic</i>",
                "<h1>H1</h1>",
                "<strike>Strike</strike>",
                //"<hr />",
                //"<ul><li>Unordered item 1</li><li>Unordered item 2</li><li>Unordered item 3</li></ul>",
                //"<ol><li>Ordered item 1</li><li>Ordered item 2</li><li>Ordered item 3</li></oll>",
                "<pre>Pre</pre>",
                "<a href='https://bing.com'>Link</a>",
                "<img src='http://aka.ms/Fo983c' alt='Test image' />"
            };

            return string.Join(string.Empty, tests);
        }

        private static string GetHackString(string hackPrompt)
        {
            var result = string.Format(@"{0} <img src=a onerror=alert(document.cookie) />", hackPrompt);
            return result;
        }

        /// <summary>
        /// Builds and returns a <see cref="ThumbnailCard"/> attachment using the supplied info
        /// </summary>
        /// <param name="title">Title of the card</param>
        /// <param name="subTitle">Subtitle of the card</param>
        /// <param name="text">Text of the card</param>
        /// <param name="attribution">Attribution of the card</param>
        /// <param name="images">Images in the card</param>
        /// <param name="buttons">Buttons in the card</param>
        /// <returns>Card attachment</returns>
        private static Attachment GetThumbnailCardAttachment(string title, string subTitle, string text, string attribution, string[] images, string[] buttons)
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
        /// Returns the trigger prefix for supplied keyword
        /// </summary>
        /// <param name="keyword">The keyword</param>
        /// <returns>Trigger string</returns>
        private static string TriggerIs(string keyword)
        {
            return TRIGGER + " " + keyword;
        }
        /*
        /// <summary>
        /// Returns a response representing if this service should service the request.
        /// </summary>
        /// <param name="question">Question instance.</param>
        /// <returns>
        /// True if request should be serviced, false otherwise.
        /// </returns>
        public Task<bool> ShouldTrigger(IQuestion question)
        {
            return Task.FromResult<bool>(this.triggerToHandler.Keys.Any((val) => question.Content.StartsWith(val, StringComparison.InvariantCultureIgnoreCase)));
        }
         */

        /// <summary>
        /// Builds an <see cref="IAnswer" /> using the supplied info and returns it/>
        /// </summary>
        /// <param name="attachments">Attachments for the answer</param>
        /// <param name="confidence">Confidence for the answer</param>
        /// <param name="content">Content of the answer</param>
        /// <param name="attachmentLayout">Attachment layout</param>
        /// <returns>Answer capturing the supplied info</returns>
        private static IEnumerable<TestReply> BuildMessage(List<Attachment> attachments, string content = null, AttachmentLayout attachmentLayout = AttachmentLayout.Carousel)
        {
            return TestsDialog.BuildMessage(attachments: attachments, content: content, summary: null, contentFormat: null, attachmentLayout: attachmentLayout);
        }

        /// <summary>
        /// Builds an <see cref="IAnswer" /> using the supplied info and returns it/&gt;
        /// </summary>
        /// <param name="attachments">Attachments for the answer</param>
        /// <param name="confidence">Confidence for the answer</param>
        /// <param name="content">Content of the answer</param>
        /// <param name="summary">The summary.</param>
        /// <param name="contentFormat">The content format.</param>
        /// <param name="attachmentLayout">Attachment layout</param>
        /// <returns>
        /// Answer capturing the supplied info
        /// </returns>
        private static IEnumerable<TestReply> BuildMessage(List<Attachment> attachments, string content, string contentFormat, AttachmentLayout attachmentLayout)
        {
            return new List<TestReply>()
            {
                new TestReply()
                {
                    Content = content,
                    ContentFormat = contentFormat,
                    Attachments = attachments,
                    AttachmentLayout = attachmentLayout,
                }
            };
        }
               
    }


}
