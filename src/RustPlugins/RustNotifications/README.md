# RustNotifications

This plugin uses the `Slack` library included in [here](http://oxidemod.org/plugins/slack.1952/). It provides a notification system to either a player or a slack team when a player connects or disconnects from a server and when a player attacks another player's base.


# Overview
When a player disconnects or connects, the message will always be sent to slack channel.

When a player who's online has their base attacked, a message will be sent to the chat window in-game.

When a player who's sleeping in the server but offline has their base attacked a message will be sent to the slack team.

# Setup

To use this plugin the file `Slack.cs` must also be loaded from your server's plugin directory. Do this by dropping the `Slack.cs` file into the plugin directory, review that plugins notes for setup information.

Next drop `RustNotifications.cs` into the servers plugin directory, if the server is running it should automatically recognize and build the plugin and create the default configuration file. If the server isn't running, the config file will be created the next time the server is started.

# Configuration
The default configuration file will look like this:
```json
{
  "DoLinkSteamProfile": true,
  "DoSendSlackMessages": true,
  "DoNotifyWhenBaseAttacked": true,
  "BaseAttackedMessageTemplate": null,
  "DoNotifyWhenPlayerConnects": true,
  "PlayerConnectedMessageTemplate": null,
  "DoNotifyWhenPlayerDisconnects": true,
  "PlayerDisconnectedMessageTemplate": null,
  "NotificationCooldownInSeconds": 60
}
```
- DoLinkSteamProfile: Switch to turn off link to steam profile on notifications.

- DoSendSlackMessages: Switch to turn slack notifications on/off.

- DoNotifyWhenBaseAttacked: Switch to turn notifications for base attacks on/off.

- BaseAttackedMessageTemplate: Template message to display on notifications for base attack. `{Attacker}` and `{Defender}` will be replaced with the Steam Display Names of the attacker and defender respectively. NOTE If the defender is online the message will send to them through the ingame chat command. If the player is offline it will go to the slack channel.

- DoNotifyWhenPlayerConnects: Switch to turn notifications for player connections on/off.

- PlayerConnectedMessageTemplate: Template message to display on player connections.

- DoNotifyWhenPlayerDisconnects: Switch to turn notifications for player disconnections on/off.

- PlayerDisconnectedMessageTemplate: Template message to display on player disconnections.

- NotificationCooldownInSeconds: Time to wait between base attacked notifications. This is on a per player basis. Meaning if two players have their bases attacked inside the cooldown period, they will both receive messages.


# TODO
- Notify on structure destroyed.
- Notify on sleeping player attacked/looted.
- Ideally there would be some sort of in-game item, such as an alarm system where you'd attach this to structures and this would cause the notifications to go out instead of having it turned on for everything.
