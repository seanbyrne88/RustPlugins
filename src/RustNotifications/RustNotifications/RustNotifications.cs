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
    [Info("RustNotifications", "seanbyrne88", "0.5.0")]
    [Description("Configurable Notifications for Rust Events")]
    class RustNotifications : RustPlugin
    {
        private static PluginConfig Settings;
        
        [PluginReference]
        Plugin RustSlackClient;

        private Dictionary<ulong, DateTime> UserLastNotified;

        #region chat commands
        [ChatCommand("resetConfig")]
        void CommandResetConfig(BasePlayer player, string command, string[] args)
        {
            if (player.IsAdmin())
            {
                LoadDefaultConfig();
                LoadConfigValues();
            }
        }
        #endregion

        #region oxide methods
        void OnPlayerInit(BasePlayer player)
        {
            SendPlayerConnectNotification(player);
        }

        void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            SendPlayerDisconnectNotification(player, reason);
        }

        void OnPlayerAttack(BasePlayer attacker, HitInfo info)
        {
            SendBaseAttackedNotification(attacker, info);
        }

        void Init()
        {
            LoadConfigValues();
        }
        #endregion

        private void AlertServer(string MessageText)
        {
            PrintToChat(MessageText);
        }

        private void AlertIndividual(BasePlayer player, string MessageText)
        {
            PrintToChat(player, MessageText);
        }

        private string GetDisplayNameByID(ulong UserID)
        {            
            if(BasePlayer.activePlayerList.Exists(x => UserID == x.userID))
            {
                return BasePlayer.activePlayerList.Find(x => UserID == x.userID).displayName;
            }
            else
            {
                try
                {
                    return BasePlayer.sleepingPlayerList.Find(x => UserID == x.userID).displayName;
                }
                catch(Exception)
                {
                    PrintWarning(String.Format("Tried to find player with ID {0} but they weren't in active or sleeping player list", UserID.ToString()));
                    throw;
                }
                
            }
        }

        private bool IsPlayerActive(ulong UserID)
        {
            if (BasePlayer.activePlayerList.Exists(x => x.userID == UserID))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsPlayerNotificationCooledDown(ulong UserID)
        {
            if (UserLastNotified.ContainsKey(UserID))
            {
                //check notification time per user, if it's cooled down send a message
                DateTime LastNotificationTime = UserLastNotified[UserID];
                if ((DateTime.Now - LastNotificationTime).TotalSeconds > Settings.NotificationCooldownInSeconds)
                {
                    UserLastNotified[UserID] = DateTime.Now;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                UserLastNotified.Add(UserID, DateTime.Now);
                return true;
            }
        }

        #region notifications
        private void SendSlackNotification(string MessageText)
        {
            if(Settings.IsSlackActive)
            {
                RustSlackClient.Call(
                    "PostMessage",
                    Settings.UrlWithAccessToken,
                    MessageText,
                    Settings.SlackUserName,
                    Settings.SlackChannel,
                    Settings.EmojiIcon
                    );
            }
        }

        private void SendPlayerConnectNotification(BasePlayer player)
        {
            string MessageText = Settings.PlayerConnectedMessageTemplate.Replace("{DisplayName}", player.displayName);
            if(Settings.DoNotifyWhenPlayerConnects)
            {
                SendSlackNotification(MessageText);
            }
        }

        private void SendPlayerDisconnectNotification(BasePlayer player, string reason)
        {
            string MessageText = Settings.PlayerDisconnectedMessageTemplate.Replace("{DisplayName}", player.displayName).Replace("{Reason}", reason);
            if(Settings.DoNotifyWhenPlayerDisconnects)
            {
                SendSlackNotification(MessageText);
            }
        }

        private void SendBaseAttackedNotification(BasePlayer player, HitInfo info)
        {
            if (info.HitEntity != null)
            {
                if (info.HitEntity.OwnerID != 0 && IsPlayerNotificationCooledDown(info.HitEntity.OwnerID))// && info.HitEntity.OwnerID != player.userID)
                {
                    string MessageText = Settings.BaseAttackedMessageTemplate.Replace("{Attacker}", player.displayName).Replace("{Owner}", GetDisplayNameByID(info.HitEntity.OwnerID).Replace("{Damage}", info.damageTypes.Total().ToString()));
                    if (Settings.DoNotifyWhenBaseAttacked)
                    {
                        //if a player is active on the server, no need to send to slack, just notify in chat.
                        if(IsPlayerActive(info.HitEntity.OwnerID))
                        {
                            //find player and message directly
                            BasePlayer p = BasePlayer.activePlayerList.Find(x => x.userID == info.HitEntity.OwnerID);
                            PrintToChat(p, MessageText);
                        }
                        else
                        {
                            SendSlackNotification(MessageText);
                        }
                    }
                }
            }
        }
        #endregion notifications

        #region Config

        PluginConfig DefaultConfig()
        {
            return new PluginConfig
            {
                IsSlackActive = true,
                UrlWithAccessToken = "",
                SlackChannel = "#rust",
                EmojiIcon = ":rust:",
                SlackUserName = "Rust Notifications",
                DoNotifyWhenPlayerConnects = true,
                PlayerConnectedMessageTemplate = "{DisplayName} has joined the server",
                DoNotifyWhenPlayerDisconnects = true,
                PlayerDisconnectedMessageTemplate = "{DisplayName} has left the server, reason: {Reason}",
                DoNotifyWhenBaseAttacked = true,
                BaseAttackedMessageTemplate = "{Attacker} has attacked a structure built by {Owner}",
                NotificationCooldownInSeconds = 60
            }; 
        }

        protected override void LoadDefaultConfig()
        {
            Config.Clear();
            Config.WriteObject(DefaultConfig(), true);
            PrintWarning("Default Configuration File Created");
            UserLastNotified = new Dictionary<ulong, DateTime>();
        }

        protected void LoadConfigValues()
        {
            Settings = Config.ReadObject<PluginConfig>();
            UserLastNotified = new Dictionary<ulong, DateTime>();
        }

        private class PluginConfig
        {
            public bool IsSlackActive { get; set; }
            public string UrlWithAccessToken { get; set; }
            public string SlackChannel { get; set; }
            public string SlackUserName { get; set; }
            public string EmojiIcon { get; set; }
            public bool DoNotifyWhenBaseAttacked { get; set; }
            public string BaseAttackedMessageTemplate { get; set; }
            public bool DoNotifyWhenPlayerConnects { get; set; }
            public string PlayerConnectedMessageTemplate { get; set; }
            public bool DoNotifyWhenPlayerDisconnects { get; set; }
            public string PlayerDisconnectedMessageTemplate { get; set; }
            public int NotificationCooldownInSeconds { get; set; }
        }
        #endregion
    }
}

