# Flows

## Handshaking (Scenario 1)

As oppose to Scenario 2, where the gateway relays messages and does not expose the IP until the node accepts the request, the gateway could expose all information needed for any hub to connect to any hub.

This would allow hubs to connect with other hubs of their choice, without the gateway having any knowledge of who is connected to who.

One issue with this solution, is the obvious issue of distributed denial of service attacks. With all the IPs easily available from the gateway, then someone could initiate attack on the IPs provided.

This can be mitigated by having a trusted gateway, that is pre-configured either with a known encryption key, or with a list of public keys that is allowed to connect. Then only those hubs will receive
information about IP address of other hubs.

After the hub receives HubInfo, and it will continue to do so while connected to gateway, it can attempt to connect directly to a hub. Based on configuration, the hub might auto-accept the incoming request,
or wait for a manual approval of the hub administrator.




## Handshaking (Scenario 2)

Notes of how handshaking might work.

1. Gateway startup and hosts endpoint that allows hubs to connect and shared HubInfo instances.
2. Hubs share HubInfo, which includes the public key of the hub. The public key is derived from the auto-generated recovery phrase that is stored encrypted on disk using a key stored with data protection API.
3. Hubs receive all HubInfo updates and keeps a list of online hubs. Archieve hubs/monitoring hubs/surveilance hubs might continuously store HubInfo for all possible future - that is important to remember. If you connect to untrusted gateway and hubs, you might be monitored. While all traffic is encrypted, your IP will still be exposed when connected to third party gateway/hubs. You can configure only to connect with trusted gateway and hubs.
4. A Hub sends an handhake message to a specific Hub. This message is encrypted with the public key of the Hub. The message includes the origin IP, the public key of the connecting hub, and the whole message is signed with the private key.
5. The receiver hub can decide if it will accept the connection. This can happen automatically based on configuration ("approved public keys" or "approved IPs").
6. When a manual acceptance is done, it is the receiver of the invite that does the actual initiation of peer to peer connection.

Thoughts:

- Perhaps the HubInfo that gateway sends to connected hubs should not include the IP address, to protect the individual peers. Instead only expose the public keys (and perhaps additional metadata a hub want to share). Then when a hub want to connect, the gateway will forward the message, if the hub accepts, then the IP will be returned.

- Initial connection with gateway should be based on some sort of trust, perhaps based on a public list of gateways (which should include public key + IP + signature that validates the IP).

Public Key | IP | Signature

```json
{
    "gateways": [
        {
            "pubKey": "3213123"
            "IP": "127.0.0.1"
            "Port": 6610
        }
    ]
}
```

This could be exposed in a public API, potentially hosted on the Blockcore Platform, that returns a list of all public gateways. At startup, a hub could then query this API to get the most recent IP address of their trusted gateways. The public key should be put in a configuration file.

```json
{
    "Hub": {
        "Gateways": 
        [ 
            { "name": "MyFriend1", pubKey: "3213123", lastKnownIP: "127.0.0.1" },
            { "name": "MyFriend2", pubKey: "7423421" },
        ]
    }
}
```

## Security

Gateways are obvious targets of infiltration, and there are multiple scenarios for that:

- If the IP address of the gateway is taken over by someone else, they can attempt to spoof the previous gateway. This will fail due to handshake protocol between hub and gateway, where the private key will be missing.

- If the infiltration is complete, meaning the server has been fully taken over and private key is available, then there is no way that hubs can distinguish it from the previously trusted gateway.

The only way to discover exploited gateways, is for a manual verification with the owner. This has to be done in-person, with a video conference or voice call. And even then you might not be entirely sure if the gateway host is verifying under duress or not.

One solution to mitigate this issue, is to rotate gateway on interval. This will make the attack surface much larger, especially if the nodes are physically different places hosted by different individuals/operators.

As long as you attempt to initiate a connection to an exploited gateway, your IP will be logged. Therefore expect that if you run a hub, your IP will be known by third parties at one time or another. If you run a public API hub, you are obviously very public.

