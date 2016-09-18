# RustPlugins

Plugins I've made for the PC game Rust. This was meant as a learning experience and if anything can be improved upon, create an issue and I'll try to get working on it.

# Overview

- RustSlackClient: This is a plugin API to allow other plugins to utilize a SlackClient for sending messages. NOTE: The plugin requires the Slack Integration [Incoming Webhooks](https://api.slack.com/incoming-webhooks).

- RustNotifications: This utilizes RustSlackClient to alert server admins when a player connects, disconnects or attacks another players base.

For more detailed information and usage instructions please reference the readme files in the `src/RustNotifications/<pluginname>` directory.

# Contributors
Although not direct contributors I would not have been able to make these without referencing design and implementation details from the following places.
- [InfoPanel](http://oxidemod.org/plugins/infopanel.1356/)

- [Everything in this repository](https://github.com/lukespragg/oxide-plugins)
