# Discord

# Overview
A universal discord client library using the [OxideMod](oxidemod.com) framework.

# Prerequisites

To use the client library you must configure an application and bot on your Discord server. For more information on setting up a Discord bot see the [Discord API Docs](https://discordapp.com/developers/docs/reference).

# Configuration

When the Discord plugin is loaded, a default `Discord.json` file will be created in your configuration directory. The configuration values will be as follows:
```json
{
  "BotToken": "Bot <your bot token here>",
  "ChannelID": 12345
}
```

- BotToken: This is the bot token you get when setting up your bot as part of your discord server. NOTE: The `Bot ` at the start of this token is optional, if you only provide the token the plugin will prepend the auth header with `Bot `.

- ChannelID: This is the ID of the channel you want to send the messsages to. To get this, enable developer mode on your discord client, Right/Alt-Click on the channel and select "Get ChannelID". This will copy the ChannelID to your clipboard and you can paste it in the configuration file.

# Usage
To load the library, include the following piece of code in your plugin file.

```c#
[PluginReference]
Plugin Discord;
```
You can then call the `PostMessage` method by using the following:

```c#
Discord.Call(
    "SendMessage",
    "<Your message text goes here>"
    );
```

It's a good idea to include some validation when loading this library for usage in plugins. The following code will validate and issue a warning if the Discord plugin hasn't been loaded correctly.

```c#
if(!Discord)
{
  PrintWarning("Discord not loaded correctly, please check your plugin directory for Discord.cs file");
}
```

NOTE: There is no way of ensuring load order of the plugins so you may want to rename the .cs file to ensure it gets loaded first. That way subsequent plugins will not throw the warning above because of the load order.
