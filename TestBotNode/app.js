var restify = require('restify');
var builder = require('botbuilder');

var Promise = require('bluebird');
var url = require('url');
var Swagger = require('swagger-client');

const S_STANDARD_IMGURL = "https://skypeteamsbotstorage.blob.core.windows.net/bottestartifacts/panoramic.png";

//=========================================================
// Bot Setup
//=========================================================

// Setup Restify Server
var server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});
  
// Create chat bot
var connector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});
var bot = new builder.UniversalBot(connector);
server.post('/api/messages', connector.listen());

bot.use({
    botbuilder: (session, next) => {
            let message = session.message;
            if (message) {
                let botMri = message.address.bot.id.toLowerCase();
                let botAtMention = message.entities && message.entities.find((entity) => (entity.type === "mention") && (entity.mentioned.id.toLowerCase() === botMri));
                if (botAtMention) {
                    // Save original text as property of the message
                    message.textWithBotMentions = message.text;
                    message.text = message.text.replace(botAtMention.text, "").trim();
                }
            }
            next();
        }
});

// Swagger client for Bot Connector API
var connectorApiClient = new Swagger(
    {
        url: 'https://raw.githubusercontent.com/Microsoft/BotBuilder/master/CSharp/Library/Microsoft.Bot.Connector.Shared/Swagger/ConnectorAPI.json',  
        usePromise: true
    });

//Helper functions
function StripCommandFromMessage(msgString)
{
    var words = msgString.split(" ");

    return msgString.replace(words[0] + " ", "");
}

// Inject the connector's JWT token into to the Swagger client
function addTokenToClient(connector, clientPromise) {
    // ask the connector for the token. If it expired, a new token will be requested to the API
    var obtainToken = Promise.promisify(connector.getAccessToken.bind(connector));
    return Promise.all([obtainToken(), clientPromise]).then((values) => {
        var token = values[0];
        var client = values[1];
        client.clientAuthorizations.add('AuthorizationBearer', new Swagger.ApiKeyAuthorization('Authorization', 'Bearer ' + token, 'header'));
        return client;
    });
}


//=========================================================
// Bots Dialogs
//=========================================================


var intents = new builder.IntentDialog();
bot.dialog('/', intents);

var testCommands = [
    ['help', 'help', 'Show this message'],
    ['hero1', 'hero1Test', 'Hero card'],
    ['imgCard', 'imgCard', 'Hero card with img as content'],
    ['signin', 'signin', 'Show a Signin Card, with button to launch URL'],
    ['formatmd', 'formatmd', 'Display a sample selection of markdown formats'],
    ['formatxml', 'formatxml', 'Display a sample selection of XML formats'],
    ['echo', 'echo', 'Echo your string'],
    ['members', 'members', 'Show members of the team'],
    ['mentions', 'mentions', 'Show the @mentions you pass']
];


for (i = 0; i < testCommands.length;i++)
{
    var testCase = testCommands[i];
    var re = new RegExp("^"+testCase[0],"i");
    intents.matches(re,testCase[1]);
}


intents.onDefault('help');


// hero1
bot.dialog('hero1Test', function (session) {

    var currentDialog = session.sessionState.callstack[session.sessionState.callstack.length-1];

    var msg = new builder.Message(session);
    msg.attachmentLayout(builder.AttachmentLayout.list);
    msg.attachments([
        new builder.HeroCard(session)
        .title("Title")
        .subtitle("Subtitle " + currentDialog.id)
        .text("Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim")
        .buttons([
            builder.CardAction.imBack(session, "This is button 1", "Button1")
        ])
    ]);

    session.send(msg).endDialog();

});


// imgCard
bot.dialog('imgCard', function (session) {

    var msg = new builder.Message(session);
    msg.textFormat(builder.TextFormat.xml);
    msg.attachmentLayout(builder.AttachmentLayout.list);
    msg.attachments([
        new builder.HeroCard(session)
        .title("Card with image containing no width or height")
        .subtitle("Subtitle ")
        .text("<img src='" + S_STANDARD_IMGURL + "'/>")
        .buttons([
            builder.CardAction.imBack(session, "This is button 1", "Button1")
        ])
    ]);

    session.send(msg).endDialog();

});

bot.dialog('formatmd', function(session) {

    var msg = new builder.Message(session);

    msg.textFormat(builder.TextFormat.markdown);

    var tmp = [
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
        //"emoji - " + EmojiToSurrogatePair(0x1f37a), //\uD83C\uDF20 ",
        "This is a Table:\n\n|Table Col 1|Col2|Column 3|\n|---|---|---|\n| R1C1 | Row 1 Column 2 | Row 1 Col 3 |\n|R2C1|R2C2|R2C3|\n\n",
        "* Unordered item 1\n* Unordered item 2\n* Unordered item 3\n",
        "1. Ordered item 1\n2. Ordered item 2\n3. Ordered item 3\n",

        "[Link](https://bing.com)",
        "![Alt Text](http://aka.ms/Fo983c)"
    ];

    msg.text(tmp.join("\n\n"));

    session.send(msg).endDialog();

});

bot.dialog('formatxml', function(session) {

    var msg = new builder.Message(session);

    msg.textFormat(builder.TextFormat.xml);

    var tmp = [
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
        //"emoji - " + EmojiToSurrogatePair(0x1F37D),
        "<ul><li>Unordered item 1</li><li>Unordered item 2</li><li>Unordered item 3</li></ul>",
        "<ol><li>Ordered item 1</li><li>Ordered item 2</li><li>Ordered item 3</li></oll>",
        "<a href='https://bing.com'>Link</a>",
        "<img src='http://aka.ms/Fo983c' alt='Test image' />"
    ];

    msg.text(tmp.join("<br />"));

    session.send(msg).endDialog();

});

bot.dialog('echo', function(session) {

    var strOut = StripCommandFromMessage(session.message.text);
    session.send(strOut);
    session.endDialog();

});

bot.dialog('members', function(session) {

    if (session.message.address.conversation.isGroup != true)
    {
        session.send("GetConversationMembers only work in channel context at this time");
        session.endDialog();
    }
    else
    {
        let conversationId = session.message.address.conversation.id;
    
        // when a group conversation message is recieved,
        // get the conversation members using the REST API and print it on the conversation.

        // 1. inject the JWT from the connector to the client on every call
        addTokenToClient(connector, connectorApiClient).then((client) => {
            // 2. override API client host (api.botframework.com) with channel's serviceHost (e.g.: smba.trafficmanager.net)
            var serviceUrl = url.parse(session.message.address.serviceUrl);
            client.setHost(serviceUrl.host);
            client.setBasePath(serviceUrl.path);
            // 3. GET /v3/conversations/{conversationId}/members
            client.Conversations.Conversations_GetConversationMembers({ conversationId: conversationId })
                .then((res) => printMembersInChannel(session.message.address, res.obj))
                .catch((error) => console.log('Error retrieving conversation members: ' + error.statusText));
        });
    }
    session.endDialog();
});

bot.dialog('signin', function(session) {

    var msg = new builder.Message(session);
    //msg.attachmentLayout(builder.AttachmentLayout.list);
    msg.attachments([
        new builder.SigninCard(session)
            .text("Sample Sign-in using SigninCard")
            .button("Sign-in", "https://microsoft.com")
    ]);

    session.send(msg);
    session.endDialog();
});

bot.dialog('help', function(session) {

    var txt = "## A list of all valid tests. ## \n\n" +
        //"Values in [] can be changed by adding appropriate arguments, e.g. 'hero1 5' makes a hero1 card with 5 buttons; 'hero3 \"This is a Title\"' uses that string as the title.\n\n" +
        //"You can prepend or postpend '|' (pipe) to dump the payload for incoming or outgoing message, respectively. \n\n" +
        "---";

    for (i = 0; i < testCommands.length;i++)
    {
        var testCase = testCommands[i];
        txt += "\n\n" + "** " + testCase[0] + " ** - " + testCase[2];    
    }

    session.send(txt);
    session.endDialog();

});


bot.dialog('mentions', function(session) {

    let entities = session.message.entities;
    let mentionCount = 0;
    let txt = "";
    for (i = 0;i < entities.length;i++)
    {
        if (entities[i].type === 'mention')
        {
            ++mentionCount;
            txt += "\n---\n" + JSON.stringify(entities[i].mentioned,null, '\n');
        }
    }


    session.send("You mentioned " + mentionCount + " entities:\n" + txt);
    session.endDialog();

});

// Create a message with the member list and send it to the conversationAddress
function printMembersInChannel(conversationAddress, members) {
    if(!members || members.length == 0) return;
    
    var memberList = members.map((m) => '* ' + m.name + ' (Id: ' + m.id + ')')
        .join('\n ');

    var reply = new builder.Message()
        .address(conversationAddress)
        .text('These are the members of this conversation: \n ' + memberList);
    bot.send(reply);
}

/*

// Send welcome when conversation with bot is started, by initiating the root dialog
bot.on('conversationUpdate', function (message) {
    if (message.membersAdded) {
        message.membersAdded.forEach(function (identity) {
            if (identity.id === message.address.bot.id) {
                bot.beginDialog(message.address, '/');
            }
        });
    }
});
*/