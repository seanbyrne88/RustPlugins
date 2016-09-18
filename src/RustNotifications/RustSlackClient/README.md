# RustSlackClient

# Note (Mod not being maintained)
After talking with the mods at oxidemod.org, I'm not going to maintain or use this library. There is already a better one [here](http://oxidemod.org/plugins/slack.1952/) that my `RustNotifications` plugin will rely on.

# Overview

A slack client library for Rust using the [OxideMod](oxidemod.com) framework. This plugin was designed without the configuration in mind. The reason there are no configuration parameters on this plugin is to allow multiple configurations per plugin.

# Prerequisites

To use the client library you must configure the Slack Integration [Incoming Webhooks](https://api.slack.com/incoming-webhooks) on your Slack team account.

# Usage
To load the library, include the following piece of code in your plugin file.

```c#
[PluginReference]
Plugin RustSlackClient;
```
You can then call the `PostMessage` method by using the following:

```c#
RustSlackClient.Call(
    "PostMessage",
    "<urlWithAccessToken>",
    "<MessageText>",
    "<SlackUserName>",
    "<SlackChannel>",
    "<EmojiIcon>"
    );
```

It's a good idea to include some validation when loading this library for usage in plugins. The following code will validate and issue a warning if the RustSlackClient plugin hasn't been loaded correctly.

```c#
if(!RustSlackClient)
{
  PrintWarning("RustSlackClient not loaded correctly, please check your plugin directory for RustSlackClient.cs file");
}
```

NOTE: There is no way of ensuring load order of the plugins so you may want to rename the .cs file to ensure it gets loaded first. That way subsequent plugins will not throw the warning above because of the load order.

# Extra Tips
For my Client library I created a `:rust:` emoji in Slack and used this as the EmojiIcon icon parameter. This lets you easily identify slack messages from the bot. You can also default this on the Incoming Webhook configuration page on your team's app management page.
