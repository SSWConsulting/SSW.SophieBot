const { DirectLine } = require('botframework-directlinejs');
const AdaptiveCards = require('adaptivecards');
const marked = require('marked');
const moment = require('moment');

var replyToId = '';
const directLine = new DirectLine({
    secret: 'aQwYsxuu2JA._35eQDOfa22hZEXVpsa3R3fHKi_ibGJvZJr30c0nYNI',
});
const adaptiveCard = new AdaptiveCards.AdaptiveCard();
AdaptiveCards.AdaptiveCard.onProcessMarkdown = function (text, result) {
    result.outputHtml = marked(text);
    result.didProcess = true;
};

directLine.activity$
    .filter(activity => activity.type === 'message' && activity.from.id === 'SSWSophieBot')
    .subscribe(
        message => {
            addIncomingMsg(message);
            replyToId = message.replyToId;
            scrollToBottom();
        }
    );

document.onkeydown = function (event) {
    var e = event || window.event;
    if (e && e.keyCode == 13) {
        send();
    }
};

send = function () {
    var sentInput = getSendInput();
    var msg = sentInput.value;

    if (!!msg) {
        addOutgoingMsg(msg);
        postActivity(msg);
    }

    sentInput.value = '';
    scrollToBottom();
}

scrollToBottom();

function scrollToBottom() {
    var historyDiv = getHistoryDiv();
    historyDiv.scrollTo(0, historyDiv.scrollHeight);
}

function postActivity(msg) {
    directLine.postActivity({
        from: {
            id: 'DirectLineSampleClientUser'
        },
        type: 'message',
        text: msg
    }).subscribe(
        id => { },
        error => console.log("Error posting activity", error)
    );
}

function getSendInput() {
    return document.getElementById('send');
}

function getHistoryDiv() {
    return document.getElementById('history');
}

function addOutgoingMsg(msg) {
    getHistoryDiv().appendChild(createOutgoingMsg(msg));
}

function addIncomingMsg(msg) {
    getHistoryDiv().appendChild(createIncomingMsg(msg));
}

function createOutgoingMsg(msg) {
    var outgoingMsgDiv = document.createElement('div');
    outgoingMsgDiv.className = 'outgoing_msg';

    var sentMsgDiv = document.createElement('div');
    sentMsgDiv.className = 'sent_msg';

    var sentMsgP = document.createElement('p');
    sentMsgP.innerText = msg;

    var sentMsgSpan = document.createElement('span');
    sentMsgSpan.className = 'time_date';
    var now = moment().format('h:mm:ss A | MMMM Do');
    sentMsgSpan.innerText = now;

    sentMsgDiv.appendChild(sentMsgP);
    sentMsgDiv.appendChild(sentMsgSpan);

    outgoingMsgDiv.appendChild(sentMsgDiv);

    return outgoingMsgDiv;
}

function createIncomingMsg(msg) {
    var incomingMsgDiv = document.createElement('div');
    incomingMsgDiv.className = 'incoming_msg';

    var receivedMsgDiv = document.createElement('div');
    receivedMsgDiv.className = 'received_msg';

    var receivedWithdMsgDiv = document.createElement('div');
    receivedWithdMsgDiv.className = 'received_withd_msg';

    renderIncomingMsg(msg, receivedWithdMsgDiv);

    receivedMsgDiv.appendChild(receivedWithdMsgDiv);
    incomingMsgDiv.appendChild(receivedMsgDiv);

    return incomingMsgDiv;
}

function renderIncomingMsg(msg, parentDom) {
    if (!!msg.text) {
        parentDom.innerHTML = marked(msg.text || 'none');
    } else if (msg.attachments && Array.isArray(msg.attachments) && msg.attachments.length > 0) {
        var content = msg.attachments[0].content;

        adaptiveCard.hostConfig = new AdaptiveCards.HostConfig({
            fontFamily: "Segoe UI, Helvetica Neue, sans-serif"
        });

        adaptiveCard.onProcessMarkdown = function (text, result) {
            result.outputHtml = marked(text);
            result.didProcess = true;
        };

        adaptiveCard.onExecuteAction = (action) => {
            if (action instanceof AdaptiveCards.OpenUrlAction) {
                if (!!action.url) {
                    window.open(action.url);
                }
            }
        }

        adaptiveCard.parse(content);

        var renderedCard = adaptiveCard.render();

        parentDom.appendChild(renderedCard);
    } else {
        parentDom.innerHTML = '<p>Unknown response</p>';
    }
}