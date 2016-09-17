using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;

using UnityEngine;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust;
using Oxide.Game.Rust.Cui;

namespace Oxide.Plugins
{
    [Info("RustSlackClient", "seanbyrne88", "0.1.0")]
    [Description("Slack Client for Rust OxideMods")]
    class RustSlackClient : RustPlugin
    {
        void PostMessage(string UrlWithAccessToken, string MessageText, string SlackUserName, string SlackChannel, string EmojiIcon)
        {
            string payloadJson = JsonConvert.SerializeObject(new SlackClientPayload()
            {
                SlackChannel = SlackChannel,
                SlackUsername = SlackUserName,
                MessageText = MessageText,
                EmojiIcon = EmojiIcon
            });

            
            webrequest.EnqueuePost(UrlWithAccessToken, payloadJson, (code, response) => PostCallBack(code, response), this);
        }

        private void PostCallBack(int code, string response)
        {

        }

        //This class serializes into the Json payload required by Slack Incoming WebHooks
        class SlackClientPayload
        {
            [JsonProperty("channel")]
            public string SlackChannel { get; set; }

            [JsonProperty("username")]
            public string SlackUsername { get; set; }

            [JsonProperty("text")]
            public string MessageText { get; set; }

            [JsonProperty("icon_emoji")]
            public string EmojiIcon { get; set; }
        }
    }
}

