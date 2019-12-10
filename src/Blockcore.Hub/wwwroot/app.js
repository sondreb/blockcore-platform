function elm(id) {
    return document.getElementById(id);
}

window.external.receiveMessage(function (messageJson) {
    log('→ ', messageJson);
    var message = JSON.parse(messageJson);
    var methodName = message.type;
    var body = JSON.parse(message.body);
    var func = window["On" + methodName];

    if (!func) {
        console.error('Received event with no handler: ' + methodName, message);
    }
    else {
        window["On" + methodName](body);
    }
});

function log(key, data) {
    console.log(key, data);

    var entry = document.createElement('div');
    entry.innerHTML = key + data + '<br>';
    elm('log').prepend(entry);
}

function send(action, body) {
    var msg = { type: action + 'Action', body: JSON.stringify(body) };
    var json = JSON.stringify(msg);
    log('← ', json);
    window.external.sendMessage(json);
}

document.addEventListener('DOMContentLoaded', function (event) {
    console.log('Starting up...');
});


var self = null;

// Event Handlers
function OnGatewayConnectedEvent(e) {
    self = e.Self;
    console.log('OnGatewayConnectedEvent', e);
    elm('status').className = 'connected';
}

function OnConnectionAddedEvent(e) {
    console.log('OnConnectionAddedEvent', e);
    UpdateHubInfo(e);
}

function OnConnectionUpdatedEvent(e) {
    console.log('OnConnectionUpdatedEvent', e);
    UpdateHubInfo(e);
}

function OnConnectionRemovedEvent(e) {
    console.log('OnConnectionRemovedEvent', e);
    RemoveHubInfo(e);
}

function OnGatewayDisconnectedEvent(e) {
    console.log('OnGatewayDisconnectedEvent', e);

    // Clear the hubs.
    hubs = [];
    elm('details').innerHTML = '';
    elm('list').innerHTML = '';
    elm('status').className = 'disconnected';
}


// UI Event Handlers
function connect() {
    console.log(elm('server'));

    send('ConnectGateway', { Server: elm('server').value });
}

function disconnect() {
    send('DisconnectGateway');
}

function connectToHub() {
    send('ConnectHub', { Id: window.selectedHub.Id });
}



let hubs = [];

// UI Operations
function UpdateHubInfo(e) {
    var targetHub = e.Data;

    console.log('targetHub:', targetHub);

    var hub = hubs.find((element) => {
        return element.id === targetHub.Id;
    });

    console.log('found hub:', hub);

    if (!hub) {
        var element = document.createElement('div');

        element.onclick = function (e) {
            // get a reference to the data structure:
            var msg = e.target.message;

            var detailsHtml = 'Name: ' + msg.Name + '<br>';
            detailsHtml += 'External: ' + msg.ExternalEndpoint + '<br>';
            detailsHtml += 'Internal: ' + msg.InternalEndpoint + '<br>';
            detailsHtml += 'Method: ' + msg.ConnectionType + '<br>';
            detailsHtml += 'IPs: ' + '<br>';

            // TODO: EXMAPLE HOW WE CAN DISABLE CONNECT FOR SAME-HUB REGISTRATION
            // (hubInfo.Id != client.LocalClientInfo.Id);

            elm('details').innerHTML = detailsHtml;
            window.selectedHub = msg;

            // If the selected Hub is self, then disable the button.
            elm('connect-button').disabled = msg.Id === self.Id;
        };

        element.message = targetHub;
        element.className = 'hub';

        if (targetHub.Id === self.Id) {
            element.innerHTML = targetHub.Name + '(self)';
        } else {
            element.innerHTML = targetHub.Name + '(' + targetHub.ExternalEndpoint + ')';
        }

        elm('list').appendChild(element);

        hubs.push({ id: targetHub.Id, message: targetHub, element: element });
    }
    else {
        hub.Name = targetHub.Name;

        if (targetHub.ExternalEndpoint) {
            hub.ExternalEndpoint = targetHub.ExternalEndpoint;
        }

        if (targetHub.InternalEndpoint) {
            hub.InternalEndpoint = targetHub.InternalEndpoint;
        }

        hub.ConnectionType = targetHub.ConnectionType;
    }
}


function RemoveHubInfo(e) {
    var targetHub = e.Data;

    console.log('targetHub:', targetHub);

    var hub = hubs.find((element) => {
        return element.id === targetHub.Id;
    });

    console.log('found hub:', hub);

    if (hub) {

        elm('list').removeChild(hub.element);
        hubs.splice(hubs.indexOf(hub), 1);
    }
}