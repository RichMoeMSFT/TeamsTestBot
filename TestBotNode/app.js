var restify = require('restify');
var builder = require('botbuilder');

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

//=========================================================
// Bots Dialogs
//=========================================================

var intents = new builder.IntentDialog();
bot.dialog('/', intents);

var testCommands = [
    ['hero1', 'hero1Test', 'Hero1 help'],
    ['help', 'help', 'Help help']
];


for (i = 0; i < testCommands.length;i++)
{
    var testCase = testCommands[i];
    var re = new RegExp("^"+testCase[0],"i");
    intents.matches(re,testCase[1]);
}
//intents.matches(/^hero1/i, 'hero1Test');


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

bot.dialog('help', function(session) {
    session.send("Help goes here").endDialog();

});