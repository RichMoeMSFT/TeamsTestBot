//----------------------------------------------------------------------------------------------
// <copyright file="TestAnswerService.cs" company="Microsoft">
// Copyright (c) Microsoft.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace TestBot
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using BotCommon;
    using Microsoft.Bot.Connector;
    using BotCommon.CustomCards;
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Linq;
    using Giphy;
    [ExcludeFromCodeCoverage]
    public class DemoAddTabAction
    {        
        public string TabName { get; set; }
        public string TabUrl { get; set; }
    }

    /// <summary>
    /// Test answer service.
    /// </summary>
    /// <seealso cref="BotCommon.IAnswerService" />
    [ExcludeFromCodeCoverage]
    public class TestAnswerService : IAnswerService
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
        private readonly Dictionary<string, Func<string, IEnumerable<IAnswer>>> triggerToHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAnswerService" /> class.
        /// </summary>
        public TestAnswerService()
        {
            this.triggerToHandler = new Dictionary<string, Func<string, IEnumerable<IAnswer>>>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    TriggerIs(HELPTRIGGER),
                    (input) =>
                    {
                        var reply = string.Join("<br />", this.triggerToHandler.Keys);
                        return TestAnswerService.BuildAnswer(null, 1, reply);
                    }
                },
                {
                    TriggerIs(EVERYTHINGTRIGGER),
                    (input) =>
                    {
                        var results = new List<IAnswer>();
                        foreach (var th in this.triggerToHandler)
                        {
                            if (th.Key != TriggerIs("help") && th.Key != TriggerIs("everything"))
                            {
                                results.AddRange(th.Value(input));
                            }
                        }
                        return results;
                    }
                },
                {
                    TriggerIs("hero1"),
                    (input) =>
                    {
                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            // Card with everything
                            TestAnswerService.GetHeroCardAttachment(
                            "Subject Title",
                            "Subtitle or breadcrumb",
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            "attribution",
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/screenshot_16x9.png" },
                            new string[] { "View in article", "See more like this", "/test ipsum" })
                        });
                    }
                },
                {
                    TriggerIs("hero2"),
                    (input) =>
                    {
                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            // Card with no image but 5 buttons
                            TestAnswerService.GetHeroCardAttachment(
                            "Subject Title",
                            "Subtitle or breadcrumb",
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            "attribution",
                            null,
                            new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" })
                        });
                    }
                },
                {
                    TriggerIs("hero3"),
                    (input) =>
                    {
                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            // Card with only a title and 5 buttons
                            TestAnswerService.GetHeroCardAttachment(
                            "Subject Title",
                            null,
                            null,
                            null,
                            null,
                            new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" })
                        });
                    }
                },
                {
                    TriggerIs("hero4"),
                    (input) =>
                    {
                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            // Card with only an image and 5 buttons
                            TestAnswerService.GetHeroCardAttachment(
                            null,
                            null,
                            null,
                            null,
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/panoramic.png" },
                            new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" })
                        });
                    }
                },
                {
                    TriggerIs("img"),
                    (input) =>
                    {
                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            TestAnswerService.GetHeroCardAttachment(
                            "Card with image containing no width or height",
                            null,
                            "<img src='https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/panoramic.png'/>",
                            null,
                            null,
                            new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" })
                        });
                    }
                },
                {
                    "/giphy",
                    (input) =>
                    {
                        string[] args = input.Split((string[]) null, StringSplitOptions.RemoveEmptyEntries);

                        var queryString = "funny+cat";

                        if (args.Count() > 1)
                        {
                            queryString = string.Join("+", args.Skip(1));
                        }

                        var giphyImageUrl = "";
                        var giphyImageWidth = "400";
                        var giphyImageHeight = "300";
                        
                        using (var httpClient = new HttpClient())
                        {
                            var response = httpClient.GetAsync(string.Format("http://api.giphy.com/v1/gifs/search?q={0}&api_key=dc6zaTOxFJmzC", queryString)).Result;
                            var responseString = response.Content.ReadAsStringAsync().Result;
                            var giphyResults = JsonConvert.DeserializeObject<GiphyResults>(responseString);
                            giphyImageUrl = giphyResults.data[0].images.original.url;
                            giphyImageWidth = giphyResults.data[0].images.original.width;
                            giphyImageHeight = giphyResults.data[0].images.original.height;
                        }

                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            TestAnswerService.GetHeroCardAttachment(null, null, null, null, new string[] { giphyImageUrl }, null),
                        });


                        return new List<IAnswer>()
                        {
                            new Answer()
                            {
                                Confidence = 1.0,
                                Content = string.Format("<img src='{0}' width={1} height={2} />", giphyImageUrl, giphyImageWidth, giphyImageHeight),
                                ContentSummary = "Giphy image",
                                ContentFormat = "xml",
                            }
                        };
                    }
                },
                 {
                    "/g2",
                    (input) =>
                    {
                        string[] args = input.Split((string[]) null, StringSplitOptions.RemoveEmptyEntries);

                        var queryString = "funny+cat";

                        if (args.Count() > 1)
                        {
                            queryString = string.Join("+", args.Skip(1));
                        }

                        var giphyImageUrl = "";
                        var giphyImageWidth = "400";
                        var giphyImageHeight = "300";
                        GiphyResults giphyResults;

                        using (var httpClient = new HttpClient())
                        {
                            var response = httpClient.GetAsync(string.Format("http://api.giphy.com/v1/gifs/search?q={0}&api_key=dc6zaTOxFJmzC", queryString)).Result;
                            var responseString = response.Content.ReadAsStringAsync().Result;
                            giphyResults = JsonConvert.DeserializeObject<GiphyResults>(responseString);
                            giphyImageUrl = giphyResults.data[0].images.original.url;
                            giphyImageWidth = giphyResults.data[0].images.original.width;
                            giphyImageHeight = giphyResults.data[0].images.original.height;
                        }

                        var images = new List<string>();
                        for (var i = 0; i < Math.Min(10, giphyResults.data.Length); i++)
                        {
                            images.Add(giphyResults.data[i].images.original.url);
                        }

                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            TestAnswerService.GetHeroCardAttachment(null, null, null, null, images.ToArray(), null),
                        });
                    }
                },
                {
                    TriggerIs("openurl_hack"),
                    (input) =>
                    {
                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            new Attachment()
                            {
                                ContentType = HeroCard.ContentType,
                                Content = new HeroCard()
                                {
                                    Title = "Card to test JS hack",
                                    Subtitle = null,
                                    Text = null,
                                    Images = null,
                                    Buttons = new List<CardAction>()
                                    {
                                        new CardAction() { Title = "Open URL hack", Type = ActionTypes.OpenUrl, Value = "javascript:alert('Hacked!')" },
                                        new CardAction() { Title = "Im back hack", Type = ActionTypes.ImBack, Value = "<img src=bad onerror=alert('Hacked!') />" },
                                    },
                                },
                            }
                        });
                    }
                },
                {
                    TriggerIs("personcard"),
                    (input) =>
                    {
                        var upn = "lajin@microsoft.com";
                        string[] args = input.Split((string[]) null, StringSplitOptions.RemoveEmptyEntries);
                        if( args.Length >= 3)
                        {
                            upn = args[2];
                        }

                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            new Attachment()
                            {
                                ContentType = PersonCard.ContentType,
                                Content = new PersonCard
                                {
                                    Upn = upn,
                                    Text = "WhoBot has found things this person has shared recently like his availability, recent files, and who he is currently working with to help you get to know him better and get things done",
                                    Buttons = new List<CardAction>()
                                    {
                                        new CardAction() { Title = "Availability", Type = ActionTypes.ImBack, Value = "availability " + upn },
                                        new CardAction() { Title = "Reports To", Type = ActionTypes.ImBack, Value = "reportsto " + upn },
                                        new CardAction() { Title = "Recent Files", Type = ActionTypes.ImBack, Value = "recentfiles " + upn },
                                        new CardAction() { Title = "Works With", Type = ActionTypes.ImBack, Value = "workswith " + upn },
                                    },
                                }
                            }
                        });
                    }
                },
                {
                    TriggerIs("listperson"),
                    (input) =>
                    {
                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            new Attachment()
                            {
                                ContentType = ListCard.ContentType,
                                Content = new ListCard
                                {
                                    Title = "Pedro DeRose Team",
                                    ListItems = new List<ListItemBase>()
                                    {
                                        new SectionListItem() { Title = "Manager" },
                                        new PersonListItem() {ID = "gsheldon@microsoft.com", Title = "Graham Sheldon", Subtitle = "Principal PM Manager - Skypespaces PM", Tap = new CardAction() { Type = ActionTypes.ImBack, Value = "whois gsheldon@microsoft.com"} },
                                        new SectionListItem() { Title = "Direct Reports" },
                                        new PersonListItem() {ID = "lajin@microsoft.com", Title = "Larry Jin", Subtitle = "Senior Program Manager - Skypespaces PM", Tap = new CardAction() { Type = ActionTypes.ImBack, Value = "whois lajin@microsoft.com"} },
                                        new PersonListItem() {ID = "marlong@microsoft.com", Title = "Mark Longton", Subtitle = "Principal Program Manager - Skypespaces PM", Tap = new CardAction() { Type = ActionTypes.ImBack, Value = "whois marlong@microsoft.com"} },
                                        new PersonListItem() {ID = "ritaylor@microsoft.com", Title = "Richard Taylor", Subtitle = "Senior Program Manager - Skypespaces PM", Tap = new CardAction() { Type = ActionTypes.ImBack, Value = "whois ritaylor@microsoft.com"} },
                                        new PersonListItem() {ID = "vchawla@microsoft.com", Title = "Vasudha Chawla", Subtitle = "Program Manager II - Skypespaces PM", Tap = new CardAction() { Type = ActionTypes.ImBack, Value = "whois vchawla@microsoft.com"} }
                                    },
                                    Buttons = new List<CardAction>()
                                    {
                                        new CardAction() { Title = "Select", Type = ActionTypes.ImBack, Value = "whois" }
                                    },
                                }
                            }
                        });
                    }
                },
                {
                    TriggerIs("listfile"),
                    (input) =>
                    {
                        String[] urls = 
                        {
                            "https://microsoft.sharepoint.com/teams/skypespacesteamnew/Shared%20Documents/Bots/Start%20Control.pptx",
                            "https://microsoft-my.sharepoint.com/personal/lajin_microsoft_com/skypenext/Cards%20and%20Redlines.pptx",
                            "https://microsoft.sharepoint.com/teams/skypespacesteamnew/Shared%20Documents/Design/Actions.one",
                            "https://microsoft.sharepoint.com/teams/skypespacesteamnew/Shared%20Documents/Design/FinancialReport.xlsx"
                        };

                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            new Attachment()
                            {
                                ContentType = ListCard.ContentType,
                                Content = new ListCard
                                {
                                    Title = "Larry Jin Recent Files",
                                    ListItems = new List<ListItemBase>()
                                    {
                                        new FileListItem() {ID = urls[0], Title = "Start Control", Subtitle = "teams > skypespacesteamnew > shared documents > bots", Tap = new CardAction() { Type = ActionTypes.OpenUrl, Value = urls[0]} },
                                        new FileListItem() {ID = urls[1], Title = "Cards and Redlines", Subtitle = "OneDrive - Microsoft > Skypenext", Tap = new CardAction() { Type = ActionTypes.OpenUrl, Value = urls[1]} },
                                        new FileListItem() {ID = urls[2], Title = "Actions", Subtitle = "teams > skypespacesteamnew > design", Tap = new CardAction() { Type = ActionTypes.OpenUrl, Value = urls[2]} },
                                        new FileListItem() {ID = urls[3], Title = "FinancialReport", Subtitle = "teams > skypespacesteamnew > design", Tap = new CardAction() { Type = ActionTypes.OpenUrl, Value = urls[3]} }
                                    },
                                    Buttons = new List<CardAction>()
                                    {
                                        new CardAction() { Title = "Open Online", Type = ActionTypes.ImBack, Value = "editOnline" },
                                        new CardAction() { Title = "Open in Office", Type = ActionTypes.ImBack, Value = "editInOffice" }
                                    },
                                }
                            }
                        });
                    }
                },
                {
                    TriggerIs("o365card"),
                    (input) =>
                    {
                        var section = new O365ConnectorCardSection(
                            "This is the **section's title** property",
                            "This is the section's text property. Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                            "This is the section's activityTitle property",
                            "This is the section's activitySubtitle property",
                            "This is the section's activityText property.",
                            "http://connectorsdemo.azurewebsites.net/images/MSC12_Oscar_002.jpg",
                            new List<O365ConnectorCardFact>()
                            {
                                new O365ConnectorCardFact("This is a fact name", "This is a fact value"),
                                new O365ConnectorCardFact("This is a fact name", "This is a fact value"),
                                new O365ConnectorCardFact("This is a fact name", "This is a fact value")
                            },
                            new List<O365ConnectorCardImage>()
                            {
                                new O365ConnectorCardImage("http://connectorsdemo.azurewebsites.net/images/MicrosoftSurface_024_Cafe_OH-06315_VS_R1c.jpg"),
                                new O365ConnectorCardImage("http://connectorsdemo.azurewebsites.net/images/WIN12_Scene_01.jpg"),
                                new O365ConnectorCardImage("http://connectorsdemo.azurewebsites.net/images/WIN12_Anthony_02.jpg")
                            },
                            new List<O365ConnectorCardActionBase>()
                            {
                                new O365ConnectorCardViewAction("View", new string[] { "http://microsoft.com" }),
                                new O365ConnectorCardViewAction("View", new string[] { "http://microsoft.com" })
                            });

                        var card = new O365ConnectorCard(
                            "This is the card title property", 
                            "This is the card's text property. Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.", 
                            "This is the summary property", 
                            "E81123",
                            new List<O365ConnectorCardSection>() { section });

                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            new Attachment()
                            {
                                ContentType = O365ConnectorCard.ContentType,
                                Content = card
                            }
                        });
                    }
                },
                {
                    TriggerIs("carousel1"),
                    (input) =>
                    {
                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            // Card with only an image and 5 buttons
                            TestAnswerService.GetHeroCardAttachment(
                            null,
                            null,
                            null,
                            null,
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/panoramic.png" },
                            new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" }),

                            // Card with only a title and 5 buttons
                            TestAnswerService.GetHeroCardAttachment(
                            "Subject Title Caraousel 1",
                            null,
                            null,
                            null,
                            null,
                            new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" }),

                            // Card with no image but 5 buttons
                            TestAnswerService.GetHeroCardAttachment(
                            "Subject Title Caraousel 1",
                            "Subtitle or breadcrumb",
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            "attribution",
                            null,
                            new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" }),

                            // Card with everything
                            TestAnswerService.GetHeroCardAttachment(
                            "Subject Title Caraousel 1",
                            "Subtitle or breadcrumb",
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            "attribution",
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/screenshot_16x9.png" },
                            new string[] { "View in article", "See more like this", "Action 3" }),

                            // Card with text only and some buttons
                            TestAnswerService.GetHeroCardAttachment(
                            "Subject Title Caraousel 1",
                            null,
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            null,
                            null,
                            new string[] { "View in article", "See more like this", "Action 3" })
                        });
                    }
                },
                {
                    TriggerIs("carouselx"),
                    (input) =>
                    {
                        var card = TestAnswerService.GetHeroCardAttachment(
                            "Subject Title Carouselx",
                            "Subtitle or breadcrumb",
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            "attribution",
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/screenshot_16x9.png" },
                            new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5", "Action 6", "Action 7" }); // Teams only support 6 actions max. Send more.

                        var attachments = new List<Attachment>();

                        for (var i = 0; i < 5; i++) // Teams only supports 5 attachments, sending more than that causes a Chat Service issue.
                        {
                            attachments.Add(card);
                        }

                        return TestAnswerService.BuildAnswer(attachments);
                    }
                },
                {
                    TriggerIs("mixedcarousel"),
                    (input) =>
                    {
                        var card = TestAnswerService.GetHeroCardAttachment(
                            "Subject Title Mixed Carousel",
                            "Subtitle or breadcrumb",
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            "attribution",
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/screenshot_16x9.png" },
                            new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5", "Action 6", "Action 7" }); // Teams only support 6 actions max. Send more.

                        var attachments = new List<Attachment>();

                        for (var i = 0; i < 5; i++) // Teams only supports 5 attachments, sending more than that causes a Chat Service issue.
                        {
                            attachments.Add(card);
                        }

                        return TestAnswerService.BuildAnswer(attachments, 1.0, "Test message content");
                    }
                },
                {
                    TriggerIs("mixedcard"),
                    (input) =>
                    {
                        var card = TestAnswerService.GetHeroCardAttachment(
                            "Subject Title Mixed Card",
                            "Subtitle or breadcrumb",
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            "attribution",
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/screenshot_16x9.png" },
                            new string[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5", "Action 6", "Action 7" }); // Teams only support 6 actions max. Send more.

                        return TestAnswerService.BuildAnswer(new List<Attachment>() { card }, 1.0, "Test message content");
                    }
                },
                {
                    TriggerIs("cardspecial"),
                    (input) =>
                    {
                        var card = TestAnswerService.GetHeroCardAttachment(
                            "Subject Title Cards Special",
                            "Subtitle or breadcrumb",
                            "That's cool. Isn't it?",
                            "attribution",
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/screenshot_16x9.png" },
                            new string[] { "View in article", "That's cool!", "Action 3", "Action 4", "Action 5", "Action 6", "Action 7" }); // Teams only support 6 actions max. Send more.

                        return TestAnswerService.BuildAnswer(new List<Attachment>() { card }, 1.0, "Test message content");
                    }
                },
                {
                    TriggerIs("thumbnail"),
                    (input) =>
                    {
                        var card = TestAnswerService.GetThumbnailCardAttachment(
                            "Homegrown Thumbnail Card",
                            "Sandwiches and salads",
                            "104 Lake St, Kirkland, WA 98033<br />(425) 123-4567",
                            "Attribution",
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/sandwich_thumbnail.png" },
                            new string[] { "View in article", "See more like this" });

                        return TestAnswerService.BuildAnswer(new List<Attachment>() { card }, 1.0, "Test message content");
                    }
                },
                {
                    TriggerIs("carouselmix2"),
                    (input) =>
                    {
                        var thumbnailCard = TestAnswerService.GetThumbnailCardAttachment(
                            "Homegrown Thumbnail Card Carousel Mix 2",
                            "Sandwiches and salads",
                            "104 Lake St, Kirkland, WA 98033<br />(425) 123-4567",
                            "Attribution",
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/sandwich_thumbnail.png" },
                            new string[] { "View in article", "See more like this" });

                        var heroCard = TestAnswerService.GetHeroCardAttachment(
                            "Subject Title Card Carousel Mix 2",
                            "Subtitle or breadcrumb",
                            "That's cool. Isn't it?",
                            "attribution",
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/screenshot_16x9.png" },
                            new string[] { "View in article", "That's cool!", "Action 3", "Action 4", "Action 5", "Action 6", "Action 7" }); // Teams only support 6 actions max. Send more.

                        return TestAnswerService.BuildAnswer(new List<Attachment>() { thumbnailCard, heroCard }, 1.0, "Test message content");
                    }
                },
                {
                    TriggerIs("bingnews"),
                    (input) =>
                    {
                        var cards = new List<Attachment>();
                        cards.Add(TestAnswerService.GetHeroCardAttachment("Bing news", "My interests", null, null, null, null));

                        var storyCard = TestAnswerService.GetThumbnailCardAttachment("Old Tech", "This is what old tech looks like", "Listicle ramp chambray humblebrag, pug scenester waistcoat tofu astopub swag. Cliche heirloom pitchfork, blue bottle <a href='https://bit.ly/2bNaRsa'>https://bit.ly/2bNaRsa</a>", "TechCrunch", new string[] {
                        }, null);

                        for (int i = 0; i < 5; i++)
                        {
                            cards.Add(storyCard);
                        }

                        cards.Add(TestAnswerService.GetHeroCardAttachment(null, null, null, null, null, new string[] { "View more work items" }));

                        return TestAnswerService.BuildAnswer(cards, 1.0, "Test message content", AttachmentLayout.List);
                    }
                },
                {
                    TriggerIs("order"),
                    (input) =>
                    {
                        // input is /test ipsum <num_of_words>
                        var wordCount = 10;
                        var parts = input.Split(' ');
                        if (parts.Length >= 3)
                        {
                            int.TryParse(parts[2], out wordCount);
                        }

                        var answers = new List<IAnswer>();

                        for (int i = 0; i < wordCount; i++)
                        {
                            var response = "Test " + i.ToString();
                            answers.Add(new Answer()
                            {
                                Confidence = 1,
                                Content = response,
                                ContentSummary = response,
                            });
                        }

                        return answers;
                    }
                },
                {
                    TriggerIs("ipsum"),
                    (input) =>
                    {
                        // input is /test ipsum <num_of_words>
                        var wordCount = 10;
                        var parts = input.Split(' ');
                        if (parts.Length >= 3)
                        {
                            int.TryParse(parts[2], out wordCount);
                        }
                        var phrase = Ipsum.GetPhrase(wordCount);
                        return TestAnswerService.BuildAnswer(null, 1.0, phrase);
                    }
                },
                {
                    TriggerIs("formatting"),
                    (input) =>
                    {
                        var response = GetTestFormattedText();
                        return TestAnswerService.BuildAnswer(null, 1.0, response);
                    }
                },
                {
                    TriggerIs("formattedcard"),
                    (input) =>
                    {
                        var parts = input.Split(' ');
                        if (parts.Length != 3)
                        {
                            var help = "/test formattedcard (what)<br />where (what) is one of title, subtitle, and text";
                            return TestAnswerService.BuildAnswer(null, 1.0, help);
                        }
                        else
                        {
                            var title = "Card title";
                            var subtitle = "Card subtitle";
                            var text = "Card text";

                            var what = parts[2].ToLowerInvariant();
                            switch(what)
                            {
                                case "title":
                                    title = GetTestFormattedText();
                                    break;
                                case "subtitle":
                                    subtitle = GetTestFormattedText();
                                    break;
                                case "text":
                                    text = GetTestFormattedText();
                                    break;
                                default:
                                    var help = "/test formattedcard (what)<br />where (what) is one of title, subtitle, and text";
                                    return TestAnswerService.BuildAnswer(null, 1.0, help);
                            }

                            var heroCard = GetHeroCardAttachment(title, subtitle, text, null, null, null);

                            return TestAnswerService.BuildAnswer(new List<Attachment>()
                            {
                                heroCard
                            });
                        }

                    }
                },
                {
                    TriggerIs("html_plaintextformat_ipsum"),
                    (input) =>
                    {
                        // input is /test ipsumplaintext <num_of_words>
                        var wordCount = 10;
                        var parts = input.Split(' ');
                        if (parts.Length >= 3)
                        {
                            int.TryParse(parts[2], out wordCount);
                        }
                        var phrase = Ipsum.GetPhrase(wordCount);
                        phrase = string.Format("<div>PT{0}</div>", phrase);
                        return TestAnswerService.BuildAnswer(
                            attachments: null,
                            confidence: 1.0,
                            content:phrase,
                            summary: "Summary of the card",
                            contentFormat: TextFormatTypes.Plain,
                            attachmentLayout: BotCommon.AttachmentLayout.Carousel
                            );
                    }
                },
                {
                    TriggerIs("html_nulltextformat_ipsum"),
                    (input) =>
                    {
                        // input is /test ipsum <num_of_words>
                        var wordCount = 10;
                        var parts = input.Split(' ');
                        if (parts.Length >= 3)
                        {
                            int.TryParse(parts[2], out wordCount);
                        }
                        var phrase = Ipsum.GetPhrase(wordCount);
                        phrase = string.Format("<div>NT{0}</div>", phrase);
                        return TestAnswerService.BuildAnswer(
                            attachments: null,
                            confidence: 1.0,
                            content:phrase,
                            summary: "Summary of the card",
                            contentFormat: null,
                            attachmentLayout: BotCommon.AttachmentLayout.Carousel
                            );
                    }
                },
                {
                    TriggerIs("html_xmltextformat_ipsum"),
                    (input) =>
                    {
                        // input is /test ipsum <num_of_words>
                        var wordCount = 10;
                        var parts = input.Split(' ');
                        if (parts.Length >= 3)
                        {
                            int.TryParse(parts[2], out wordCount);
                        }
                        var phrase = Ipsum.GetPhrase(wordCount);
                        phrase = string.Format("<div>XT{0}</div>", phrase);
                        return TestAnswerService.BuildAnswer(
                            attachments: null,
                            confidence: 1.0,
                            content:phrase,
                            summary: "Summary of the card",
                            contentFormat: TextFormatTypes.Xml,
                            attachmentLayout: BotCommon.AttachmentLayout.Carousel
                            );
                    }
                },
                                {
                    TriggerIs("markdowntextformat_ipsum"),
                    (input) =>
                    {
                        // input is /test ipsum <num_of_words>
                        var wordCount = 10;
                        var parts = input.Split(' ');
                        if (parts.Length >= 3)
                        {
                            int.TryParse(parts[2], out wordCount);
                        }
                        var phrase = Ipsum.GetPhrase(wordCount);
                        phrase = string.Format("MT**{0}**<div>Random</div>", phrase);
                        return TestAnswerService.BuildAnswer(
                            attachments: null,
                            confidence: 1.0,
                            content:phrase,
                            summary: "Summary of the card",
                            contentFormat: TextFormatTypes.Markdown,
                            attachmentLayout: BotCommon.AttachmentLayout.Carousel
                            );
                    }
                },
                {
                    TriggerIs("summary"),
                    (input) =>
                    {
                        var card = TestAnswerService.GetThumbnailCardAttachment(
                            "Homegrown Thumbnail Card Summary",
                            "Sandwiches and salads",
                            "104 Lake St, Kirkland, WA 98033<br />(425) 123-4567",
                            "Attribution",
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/sandwich_thumbnail.png" },
                            new string[] { "View in article", "See more like this" });

                        return TestAnswerService.BuildAnswer(
                            attachments: new List<Attachment>() { card },
                            confidence: 1.0,
                            content:"Test message content",
                            summary: "Summary of the card",
                            contentFormat: null,
                            attachmentLayout: BotCommon.AttachmentLayout.Carousel
                            );
                    }
                },
                {
                    TriggerIs("signin"),
                    (input) =>
                    {
                        var card =  new Attachment()
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

                        return TestAnswerService.BuildAnswer(new List<Attachment>() { card }, 1.0, "Test message content");
                    }
                },
                {
                    TriggerIs("addtab"),
                    (input) =>
                    {
                        var card =  new Attachment()
                        {
                            ContentType = HeroCard.ContentType,                          

                            Content = new HeroCard()
                            {
                                Title = "AddTab Tester Card",
                                Text = "Clicking on the button below should hoist ",
                                Buttons = new List<CardAction>()
                                {
                                    new CardAction()
                                    {
                                        Title = "Add custom tab",
                                        Type = HoistTabCardAction.ActionType,
                                        Value = JsonConvert.SerializeObject(new HoistTabCardAction()
                                        {
                                            TabUrl = "https://delve.office.com/delve/delve.aspx/?u=4c9041b7-449a-40f7-8855-56da239b9fd1&v=work",
                                            TabName = "Delve",
                                        })
                                    }
                                }
                            }
                        };

                        return TestAnswerService.BuildAnswer(new List<Attachment>() { card }, 1.0, "Test message content");
                    }
                },
                {
                    TriggerIs("smallimage_56x56"),
                    (input) =>
                    {
                        var card = TestAnswerService.GetThumbnailCardAttachment(
                            "Homegrown Thumbnail Card Small Image",
                            "Sandwiches and salads",
                            "<div>SBX - SfB Client Engineering</div><div>anandjo@microsoft.com</div><div>Some Building/12345</div><div>+1 (425) 123456 X3456</div><br/><div>Recent files:<ul><li><a href=\"https://microsoft.sharepoint.com/teams/skypespaces635899515203349203/Shared Documents/Mobile Clients/Installation Instructions for Skype Spaces Mobile.docx\">Installation Instructions for Skype Spaces Mobile.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/Skype_Business/Meetings/Shared Documents/Meetings for Everyone/SfB and Modern Meeting.pptx\">SfB and Modern Meeting.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3169_v03.pptx\">BRK3169_v03.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/CallQuality/Shared Documents/Weekly RTM Service Health Review/Call Quality and Reliability Metric Review Oct 5th 2016.pptx\">Call Quality and Reliability Metric Review Oct 5th 2016.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3167_WESENER.pptx\">BRK3167_WESENER.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3168_Badawy-Meera-New.pptx\">BRK3168_Badawy-Meera-New.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/idstrike/Shared Documents/KPI/ID Fundamentals Dashboard v2.0.2.xlsx\">ID Fundamentals Dashboard v2.0.2.xlsx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AzureScorecard/Shared Documents/2016/SafeDeployment-AuditData.pptx\">SafeDeployment-AuditData.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SkypeDesign/Shared Documents/Skype for business/presentations/Brian_Reviews/Brian_2016_09_12.pptx\">Brian_2016_09_12.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Specifications/Cloud Operations/Cloud Operations Roadmap.pptx\">Cloud Operations Roadmap.pptx</a></li><li><a href=\"https://microsoft-my.sharepoint.com/personal/shefym_microsoft_com/Documents/Shared with Everyone/TransitionPlan.docx\">TransitionPlan.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXIntegration/Shared Documents/D365 review- Integration.pptx\">D365 review- Integration.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/OAcc/Shared Documents/Tracking End of Year Work/ASG Integrated Checkpoint - September 2016.pptx\">ASG Integrated Checkpoint - September 2016.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SContentSearch/Shared Documents/Dev Info/Dev Design Docs/JsonQueryObjectSchema.docx\">JsonQueryObjectSchema.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SkypeChat/Shared Documents/Skype_Teams_Bot_Compliance_v1.docx\">Skype_Teams_Bot_Compliance_v1.docx</a></li></ul></div>",
                            "Attribution",
                            new string[] { "data:image/pjpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAA0JCgsKCA0LCwsPDg0QFCEVFBISFCgdHhghMCoyMS8qLi00O0tANDhHOS0uQllCR05QVFVUMz9dY1xSYktTVFH/2wBDAQ4PDxQRFCcVFSdRNi42UVFRUVFRUVFRUVFRUVFRUVFRUVFRUVFRUVFRUVFRUVFRUVFRUVFRUVFRUVFRUVFRUVH/wAARCAA4ADgDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwDkZJwJZNu7GePpTLiYlgOcAD+VWfKUjlE/75FRzQqzfdHQdPpSAgLA4J5yP/rVesJI47xJJ1LpgFhkc5FUp0XbGWUYC4PHuaUWs94m+CHKIP4f5VVhLU7b/hMLdYzK9pIijjAINZt/q8OryC4gDhFXZhl75z6+9cxI01uwE8bhfccVoaX5Zt3CPgFs4I+ntSstykXsAE5z/n15ooCg9c8dx2/xoplFPHbHNMZSTn2rSbVbNn3eSQB9aZJqunhiXhL+1Oxncx7xHaKNVGd5x9K6awgu7GVbVbdTCFzuI61lnWtPK7Bac9iAvH6VqC6llXzUuSFYYDf0oZdMrXUE9/DKstuIwASGArD0cMTKpGBkfSuha+kt4JRLJ+7xnGc5qKwOnyWcTTt5M3dY8YX86SQ5NIrD5cA8fhnv2oq+1rYkbUvMnPO4f/Xop2FzI5a4VIenJqk8hLk5p0zliSetQZ+ajYgerYbNdPp1wXsITC5RgoVh6kcVzHatHS9USxgkjkjMmTlQKllRdjQ1RwtpJvkJkbGD75rKiZ5B8rHp0qO9vpL2QFsKo+6qjgUyCQo1NIUmXVEo6nt6UVPbuHXLEY9zRRYm4zWtMS0k3wPuib+E9VrFxyaKKRclZjgaUAEc0UUyQJA4FCtzRRQBp6YglDljjbjH40UUVrFKwj//2Q==" },
                            new string[] { "Reportees?", "Availability?", "Collaborators?" });

                        return TestAnswerService.BuildAnswer(new List<Attachment>() { card }, 1.0, "Test message content");
                    }
                },
                {
                    TriggerIs("whobot_sixbuttons"),
                    (input) =>
                    {
                        var card = TestAnswerService.GetThumbnailCardAttachment(
                            "Homegrown Thumbnail Card 6 Buttons",
                            "Sandwiches and salads",
                            "<div>SBX - SfB Client Engineering</div><div>anandjo@microsoft.com</div><div>Some Building/12345</div><div>+1 (425) 123456 X3456</div><br/><div>Recent files:<ul><li><a href=\"https://microsoft.sharepoint.com/teams/skypespaces635899515203349203/Shared Documents/Mobile Clients/Installation Instructions for Skype Spaces Mobile.docx\">Installation Instructions for Skype Spaces Mobile.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/Skype_Business/Meetings/Shared Documents/Meetings for Everyone/SfB and Modern Meeting.pptx\">SfB and Modern Meeting.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3169_v03.pptx\">BRK3169_v03.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/CallQuality/Shared Documents/Weekly RTM Service Health Review/Call Quality and Reliability Metric Review Oct 5th 2016.pptx\">Call Quality and Reliability Metric Review Oct 5th 2016.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3167_WESENER.pptx\">BRK3167_WESENER.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3168_Badawy-Meera-New.pptx\">BRK3168_Badawy-Meera-New.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/idstrike/Shared Documents/KPI/ID Fundamentals Dashboard v2.0.2.xlsx\">ID Fundamentals Dashboard v2.0.2.xlsx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AzureScorecard/Shared Documents/2016/SafeDeployment-AuditData.pptx\">SafeDeployment-AuditData.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SkypeDesign/Shared Documents/Skype for business/presentations/Brian_Reviews/Brian_2016_09_12.pptx\">Brian_2016_09_12.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Specifications/Cloud Operations/Cloud Operations Roadmap.pptx\">Cloud Operations Roadmap.pptx</a></li><li><a href=\"https://microsoft-my.sharepoint.com/personal/shefym_microsoft_com/Documents/Shared with Everyone/TransitionPlan.docx\">TransitionPlan.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXIntegration/Shared Documents/D365 review- Integration.pptx\">D365 review- Integration.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/OAcc/Shared Documents/Tracking End of Year Work/ASG Integrated Checkpoint - September 2016.pptx\">ASG Integrated Checkpoint - September 2016.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SContentSearch/Shared Documents/Dev Info/Dev Design Docs/JsonQueryObjectSchema.docx\">JsonQueryObjectSchema.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SkypeChat/Shared Documents/Skype_Teams_Bot_Compliance_v1.docx\">Skype_Teams_Bot_Compliance_v1.docx</a></li></ul></div><div>Collaborators:<ul><li><div>Sid Uppal</div><div></div><div></div></li><li><div>Xin Gao</div><div></div><div></div></li><li><div>David Federman</div><div></div><div></div></li><li><div>Yuri Dogandjiev</div><div></div><div></div></li><li><div>Bin Lu (DYNAMICS)</div><div></div><div></div></li><li><div>Sergey Pikhulya</div><div></div><div></div></li><li><div>Dimitrios Poulopoulos</div><div></div><div></div></li><li><div>Meera Mahabala</div><div></div><div></div></li><li><div>Bill Bliss</div><div></div><div></div></li><li><div>Navid Azimi-Garakani</div><div></div><div></div></li></ul></div>",
                            "Attribution",
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/screenshot_16x9.png" },
                            new string[] { "Reportees?", "Availability?", "Collaborators?", "Manager?", "RecentFiles?", "Peers?" });

                        return TestAnswerService.BuildAnswer(new List<Attachment>() { card }, 1.0, "Test message content");
                    }
                },
                //// Following 3 entries throw request entities too large.
                ////{
                ////    TriggerIs("whobot_sixbuttons_largeimageblob"),
                ////    (input) =>
                ////    {
                ////        var card = TestAnswerService.GetThumbnailCardAttachment(
                ////            "Homegrown Thumbnail Card Big Image Blob",
                ////            "Sandwiches and salads",
                ////            "<div>SBX - SfB Client Engineering</div><div>anandjo@microsoft.com</div><div>Some Building/12345</div><div>+1 (425) 123456 X3456</div><br/><div>Recent files:<ul><li><a href=\"https://microsoft.sharepoint.com/teams/skypespaces635899515203349203/Shared Documents/Mobile Clients/Installation Instructions for Skype Spaces Mobile.docx\">Installation Instructions for Skype Spaces Mobile.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/Skype_Business/Meetings/Shared Documents/Meetings for Everyone/SfB and Modern Meeting.pptx\">SfB and Modern Meeting.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3169_v03.pptx\">BRK3169_v03.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/CallQuality/Shared Documents/Weekly RTM Service Health Review/Call Quality and Reliability Metric Review Oct 5th 2016.pptx\">Call Quality and Reliability Metric Review Oct 5th 2016.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3167_WESENER.pptx\">BRK3167_WESENER.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3168_Badawy-Meera-New.pptx\">BRK3168_Badawy-Meera-New.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/idstrike/Shared Documents/KPI/ID Fundamentals Dashboard v2.0.2.xlsx\">ID Fundamentals Dashboard v2.0.2.xlsx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AzureScorecard/Shared Documents/2016/SafeDeployment-AuditData.pptx\">SafeDeployment-AuditData.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SkypeDesign/Shared Documents/Skype for business/presentations/Brian_Reviews/Brian_2016_09_12.pptx\">Brian_2016_09_12.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Specifications/Cloud Operations/Cloud Operations Roadmap.pptx\">Cloud Operations Roadmap.pptx</a></li><li><a href=\"https://microsoft-my.sharepoint.com/personal/shefym_microsoft_com/Documents/Shared with Everyone/TransitionPlan.docx\">TransitionPlan.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXIntegration/Shared Documents/D365 review- Integration.pptx\">D365 review- Integration.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/OAcc/Shared Documents/Tracking End of Year Work/ASG Integrated Checkpoint - September 2016.pptx\">ASG Integrated Checkpoint - September 2016.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SContentSearch/Shared Documents/Dev Info/Dev Design Docs/JsonQueryObjectSchema.docx\">JsonQueryObjectSchema.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SkypeChat/Shared Documents/Skype_Teams_Bot_Compliance_v1.docx\">Skype_Teams_Bot_Compliance_v1.docx</a></li></ul></div>",
                ////            "Attribution",
                ////            new string[] { "data:image/pjpeg;base64,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" },
                ////            new string[] { "Reportees?", "Availability?", "Collaborators?", "Manager?", "RecentFiles?", "Peers?" });

                ////        return TestAnswerService.BuildAnswer(new List<Attachment>() { card }, 1.0, "Test message content");
                ////    }
                ////},
                ////{
                ////    TriggerIs("whobot_threebuttons_bigimage27kb"),
                ////    (input) =>
                ////    {
                ////        var card = TestAnswerService.GetThumbnailCardAttachment(
                ////            "Homegrown Thumbnail Card Big Image",
                ////            "Sandwiches and salads",
                ////            "<div>SBX - SfB Client Engineering</div><div>anandjo@microsoft.com</div><div>Some Building/12345</div><div>+1 (425) 123456 X3456</div><br/><div>Recent files:<ul><li><a href=\"https://microsoft.sharepoint.com/teams/skypespaces635899515203349203/Shared Documents/Mobile Clients/Installation Instructions for Skype Spaces Mobile.docx\">Installation Instructions for Skype Spaces Mobile.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/Skype_Business/Meetings/Shared Documents/Meetings for Everyone/SfB and Modern Meeting.pptx\">SfB and Modern Meeting.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3169_v03.pptx\">BRK3169_v03.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/CallQuality/Shared Documents/Weekly RTM Service Health Review/Call Quality and Reliability Metric Review Oct 5th 2016.pptx\">Call Quality and Reliability Metric Review Oct 5th 2016.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3167_WESENER.pptx\">BRK3167_WESENER.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3168_Badawy-Meera-New.pptx\">BRK3168_Badawy-Meera-New.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/idstrike/Shared Documents/KPI/ID Fundamentals Dashboard v2.0.2.xlsx\">ID Fundamentals Dashboard v2.0.2.xlsx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AzureScorecard/Shared Documents/2016/SafeDeployment-AuditData.pptx\">SafeDeployment-AuditData.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SkypeDesign/Shared Documents/Skype for business/presentations/Brian_Reviews/Brian_2016_09_12.pptx\">Brian_2016_09_12.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Specifications/Cloud Operations/Cloud Operations Roadmap.pptx\">Cloud Operations Roadmap.pptx</a></li><li><a href=\"https://microsoft-my.sharepoint.com/personal/shefym_microsoft_com/Documents/Shared with Everyone/TransitionPlan.docx\">TransitionPlan.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXIntegration/Shared Documents/D365 review- Integration.pptx\">D365 review- Integration.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/OAcc/Shared Documents/Tracking End of Year Work/ASG Integrated Checkpoint - September 2016.pptx\">ASG Integrated Checkpoint - September 2016.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SContentSearch/Shared Documents/Dev Info/Dev Design Docs/JsonQueryObjectSchema.docx\">JsonQueryObjectSchema.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SkypeChat/Shared Documents/Skype_Teams_Bot_Compliance_v1.docx\">Skype_Teams_Bot_Compliance_v1.docx</a></li></ul></div><div>Collaborators:<ul><li><div>Sid Uppal</div><div></div><div></div></li><li><div>Xin Gao</div><div></div><div></div></li><li><div>David Federman</div><div></div><div></div></li><li><div>Yuri Dogandjiev</div><div></div><div></div></li><li><div>Bin Lu (DYNAMICS)</div><div></div><div></div></li><li><div>Sergey Pikhulya</div><div></div><div></div></li><li><div>Dimitrios Poulopoulos</div><div></div><div></div></li><li><div>Meera Mahabala</div><div></div><div></div></li><li><div>Bill Bliss</div><div></div><div></div></li><li><div>Navid Azimi-Garakani</div><div></div><div></div></li></ul></div>",
                ////            "Attribution",
                ////            new string[] { "data:image/pjpeg;base64,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" },
                ////            new string[] { "Reportees?", "Details?", "Collaborators?", "Manager?" });

                ////        return TestAnswerService.BuildAnswer(new List<Attachment>() { card }, 1.0, "Test message content");
                ////    }
                ////},
                ////{
                ////    TriggerIs("whobot_onebutton_bigimage27kb_10cards_carousel"),
                ////    (input) =>
                ////    {
                ////       var card = TestAnswerService.GetThumbnailCardAttachment(
                ////            "Homegrown Thumbnail Card Carousel",
                ////            "Sandwiches and salads",
                ////            "<div>SBX - SfB Client Engineering</div><div>anandjo@microsoft.com</div><div>Some Building/12345</div><div>+1 (425) 123456 X3456</div><br/><div>Recent files:<ul><li><a href=\"https://microsoft.sharepoint.com/teams/skypespaces635899515203349203/Shared Documents/Mobile Clients/Installation Instructions for Skype Spaces Mobile.docx\">Installation Instructions for Skype Spaces Mobile.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/Skype_Business/Meetings/Shared Documents/Meetings for Everyone/SfB and Modern Meeting.pptx\">SfB and Modern Meeting.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3169_v03.pptx\">BRK3169_v03.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/CallQuality/Shared Documents/Weekly RTM Service Health Review/Call Quality and Reliability Metric Review Oct 5th 2016.pptx\">Call Quality and Reliability Metric Review Oct 5th 2016.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3167_WESENER.pptx\">BRK3167_WESENER.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Presentations/Ignite 2016/BRK3168_Badawy-Meera-New.pptx\">BRK3168_Badawy-Meera-New.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/idstrike/Shared Documents/KPI/ID Fundamentals Dashboard v2.0.2.xlsx\">ID Fundamentals Dashboard v2.0.2.xlsx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AzureScorecard/Shared Documents/2016/SafeDeployment-AuditData.pptx\">SafeDeployment-AuditData.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SkypeDesign/Shared Documents/Skype for business/presentations/Brian_Reviews/Brian_2016_09_12.pptx\">Brian_2016_09_12.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXPlatform/Shared Documents/Specifications/Cloud Operations/Cloud Operations Roadmap.pptx\">Cloud Operations Roadmap.pptx</a></li><li><a href=\"https://microsoft-my.sharepoint.com/personal/shefym_microsoft_com/Documents/Shared with Everyone/TransitionPlan.docx\">TransitionPlan.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/AXIntegration/Shared Documents/D365 review- Integration.pptx\">D365 review- Integration.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/OAcc/Shared Documents/Tracking End of Year Work/ASG Integrated Checkpoint - September 2016.pptx\">ASG Integrated Checkpoint - September 2016.pptx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SContentSearch/Shared Documents/Dev Info/Dev Design Docs/JsonQueryObjectSchema.docx\">JsonQueryObjectSchema.docx</a></li><li><a href=\"https://microsoft.sharepoint.com/teams/SkypeChat/Shared Documents/Skype_Teams_Bot_Compliance_v1.docx\">Skype_Teams_Bot_Compliance_v1.docx</a></li></ul></div><div>Collaborators:<ul><li><div>Sid Uppal</div><div></div><div></div></li><li><div>Xin Gao</div><div></div><div></div></li><li><div>David Federman</div><div></div><div></div></li><li><div>Yuri Dogandjiev</div><div></div><div></div></li><li><div>Bin Lu (DYNAMICS)</div><div></div><div></div></li><li><div>Sergey Pikhulya</div><div></div><div></div></li><li><div>Dimitrios Poulopoulos</div><div></div><div></div></li><li><div>Meera Mahabala</div><div></div><div></div></li><li><div>Bill Bliss</div><div></div><div></div></li><li><div>Navid Azimi-Garakani</div><div></div><div></div></li></ul></div>",
                ////            "Attribution",
                ////            new string[] { "data:image/pjpeg;base64,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" },
                ////            new string[] { "Details?" });

                ////        List<Attachment> attachments = new List<Attachment>();
                ////        for(int i=0; i< 10; i++)
                ////        {
                ////            attachments.Add(card);
                ////        }

                ////        return TestAnswerService.BuildAnswer(attachments, 1.0, "Test message content", AttachmentLayout.Carousel);
                ////    }
                ////},
                {
                    TriggerIs("xss_hero"),
                    (input) =>
                    {
                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            // Card with everything
                            TestAnswerService.GetHeroCardAttachment(
                            GetHackString("Hero Card Title hacked"),
                            GetHackString("Subtitle hacked"),
                            GetHackString("Text hacked"),
                            GetHackString("Attribution hacked"),
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/screenshot_16x9.png" },
                            new string[] { GetHackString("Button hacked"), })
                        });
                    }
                },
                {
                    TriggerIs("xss_thumbnail"),
                    (input) =>
                    {
                        return TestAnswerService.BuildAnswer(new List<Attachment>()
                        {
                            // Card with everything
                            TestAnswerService.GetThumbnailCardAttachment(
                            GetHackString("Thumbnail Card Title hacked"),
                            GetHackString("Subtitle hacked"),
                            GetHackString("Text hacked"),
                            GetHackString("Attribution hacked"),
                            new string[] { "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/screenshot_16x9.png" },
                            new string[] { GetHackString("Button hacked"), })
                        });
                    }
                },
            };
        }

        /// <summary>
        /// Gets or sets the bot instance.
        /// </summary>
        public IBot Bot { get; set; }

        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <param name="question">Question instance.</param>
        /// <returns>
        /// Task tracking response.
        /// </returns>
        public async Task<IEnumerable<IAnswer>> GetResponse(IQuestion question)
        {
            foreach (var tH in this.triggerToHandler)
            {
                if (question.Content.Contains(tH.Key))
                {
                    await Task.Delay(250); // Thinking delay

                    return await Task.FromResult<IEnumerable<IAnswer>>(tH.Value(question.Content));
                }
            }

            return null;
        }

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

        /// <summary>
        /// Builds an <see cref="IAnswer" /> using the supplied info and returns it/>
        /// </summary>
        /// <param name="attachments">Attachments for the answer</param>
        /// <param name="confidence">Confidence for the answer</param>
        /// <param name="content">Content of the answer</param>
        /// <param name="attachmentLayout">Attachment layout</param>
        /// <returns>Answer capturing the supplied info</returns>
        private static IEnumerable<IAnswer> BuildAnswer(List<Attachment> attachments, double confidence = 1.0, string content = null, AttachmentLayout attachmentLayout = AttachmentLayout.Carousel)
        {
            return TestAnswerService.BuildAnswer(attachments: attachments, confidence: confidence, content: content, summary: null, contentFormat: null, attachmentLayout: attachmentLayout);
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
        private static IEnumerable<IAnswer> BuildAnswer(List<Attachment> attachments, double confidence, string content, string summary, string contentFormat, AttachmentLayout attachmentLayout)
        {
            return new List<IAnswer>()
            {
                new Answer()
                {
                    Confidence = confidence,
                    Content = content,
                    ContentSummary = summary,
                    ContentFormat = contentFormat,
                    Attachments = attachments,
                    AttachmentLayout = attachmentLayout,
                }
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
    }
}
