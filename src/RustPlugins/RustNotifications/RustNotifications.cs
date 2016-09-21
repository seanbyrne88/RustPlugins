using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;

using UnityEngine;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust;
using Oxide.Game.Rust.Cui;
using Oxide.Core.Libraries.Covalence;
//using Oxide.Game.Rust.Libraries.Covalence;

namespace Oxide.Plugins
{
    [Info("RustNotifications", "seanbyrne88", "0.5.0")]
    [Description("Configurable Notifications for Rust Events")]
    class RustNotifications : RustPlugin
    {

        [PluginReference]
        Plugin Slack;

        private static NotificationConfigContainer Settings;
        private Dictionary<ulong, DateTime> UserLastNotified;
        private string SlackNotificationType;

        #region oxide methods
        void Init()
        {
            LoadConfigValues();
        }

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
        #endregion

        #region chat commands
        [ChatCommand("rustNotifyResetConfig")]
        void CommandResetConfig(BasePlayer player, string command, string[] args)
        {
            if (player.IsAdmin())
            {
                LoadDefaultConfig();
                LoadDefaultMessages();
                LoadConfigValues();
            }
        }

        [ChatCommand("rustNotifyGetCooldowns")]
        void CommandGetCoolDowns(BasePlayer player, string command, string[] args)
        {
            if (player.IsAdmin())
            {
                foreach (var l in UserLastNotified)
                {
                    PrintToChat(player, String.Format("Key: {0}, Val: {1}", GetDisplayNameByID(l.Key), l.Value));
                }
            }
        }
        #endregion

        #region private methods

        private string GetDisplayNameByID(ulong UserID)
        {
            if (BasePlayer.activePlayerList.Exists(x => UserID == x.userID))
            {
                return BasePlayer.activePlayerList.Find(x => UserID == x.userID).displayName;
            }
            else
            {
                try
                {
                    return BasePlayer.sleepingPlayerList.Find(x => UserID == x.userID).displayName;
                }
                catch (Exception)
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
                if ((DateTime.Now - LastNotificationTime).TotalSeconds > Settings.SlackConfig.NotificationCooldownInSeconds)
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
        #endregion

        #region notifications
        private void SendSlackNotification(BasePlayer player, string MessageText)
        {
            Slack.Call(SlackNotificationType, MessageText, BasePlayerToIPlayer(player));
        }

        private void SendSlackNotification(IPlayer player, string MessageText)
        {
            Slack.Call(SlackNotificationType, MessageText, player);
        }

        private IPlayer BasePlayerToIPlayer(BasePlayer player)
        {
            return covalence.Players.GetPlayer(player.UserIDString);
        }

        private void SendPlayerConnectNotification(BasePlayer player)
        {
            if (Settings.SlackConfig.DoNotifyWhenPlayerConnects)
            {
                //string MessageText = Lang("PlayerConnectedMessageTemplate", player.UserIDString).Replace("{DisplayName}", player.displayName);
                string MessageText = lang.GetMessage("PlayerConnectedMessageTemplate", this, player.UserIDString).Replace("{DisplayName}", player.displayName);
                SendSlackNotification(player, MessageText);
            }
        }

        private void SendPlayerDisconnectNotification(BasePlayer player, string reason)
        {
            if (Settings.SlackConfig.DoNotifyWhenPlayerDisconnects)
            {
                string MessageText = lang.GetMessage("PlayerDisconnectedMessageTemplate", this, player.UserIDString).Replace("{DisplayName}", player.displayName).Replace("{Reason}", reason);
                SendSlackNotification(player, MessageText);
            }
        }

        private void SendBaseAttackedNotification(BasePlayer player, HitInfo info)
        {
            if (info.HitEntity != null)
            {
                if (info.HitEntity.OwnerID != 0 && IsPlayerNotificationCooledDown(info.HitEntity.OwnerID))// && info.HitEntity.OwnerID != player.userID)
                {
                    //string MessageText = Lang("BaseAttackedMessageTemplate", player.UserIDString).Replace("{Attacker}", player.displayName).Replace("{Owner}", GetDisplayNameByID(info.HitEntity.OwnerID).Replace("{Damage}", info.damageTypes.Total().ToString()));
                    string MessageText = lang.GetMessage("BaseAttackedMessageTemplate", this, player.UserIDString).Replace("{Attacker}", player.displayName).Replace("{Owner}", GetDisplayNameByID(info.HitEntity.OwnerID).Replace("{Damage}", info.damageTypes.Total().ToString()));
                    if (Settings.SlackConfig.DoNotifyWhenBaseAttacked)
                    {
                        //if a player is active on the server, no need to send to slack, just notify in chat.
                        if (IsPlayerActive(info.HitEntity.OwnerID))
                        {
                            //find player and message directly
                            BasePlayer p = BasePlayer.activePlayerList.Find(x => x.userID == info.HitEntity.OwnerID);
                            PrintToChat(p, MessageText);
                        }
                        else
                        {
                            SendSlackNotification(player, MessageText);
                        }
                    }
                }
            }
        }
        #endregion notifications

        #region localization
        private void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
                {
                    {"PlayerConnectedMessageTemplate", "{DisplayName} has joined the server"},
                    {"PlayerDisconnectedMessageTemplate", "{DisplayName} has left the server, reason: {Reason}"},
                    {"BaseAttackedMessageTemplate", "{Attacker} has attacked a structure built by {Owner}"},
                    {"TestMessage", "Hello World"}
                }, this);
        }
        #endregion

        #region Config
        NotificationConfigContainer DefaultConfigContainer()
        {
            return new NotificationConfigContainer
            {
                SlackConfig = DefaultNotificationConfig()
            };
        }

        NotificationConfig DefaultNotificationConfig()
        {
            return new NotificationConfig
            {
                DoLinkSteamProfile = true,
                Active = true,
                DoNotifyWhenPlayerConnects = true,
                DoNotifyWhenPlayerDisconnects = true,
                DoNotifyWhenBaseAttacked = true,
                NotificationCooldownInSeconds = 60
            };
        }

        protected override void LoadDefaultConfig()
        {
            Config.Clear();
            Config.WriteObject(DefaultConfigContainer(), true);

            PrintWarning("Default Configuration File Created");

            UserLastNotified = new Dictionary<ulong, DateTime>();
        }



        protected void LoadConfigValues()
        {
            Settings = Config.ReadObject<NotificationConfigContainer>();

            UserLastNotified = new Dictionary<ulong, DateTime>();

            if (Settings.SlackConfig.DoLinkSteamProfile)
                SlackNotificationType = "FancyMessage";
            else
                SlackNotificationType = "SimpleMessage";
        }

        private class NotificationConfig
        {
            public bool DoLinkSteamProfile { get; set; }
            public bool Active { get; set; }
            public bool DoNotifyWhenBaseAttacked { get; set; }
            public bool DoNotifyWhenPlayerConnects { get; set; }
            public bool DoNotifyWhenPlayerDisconnects { get; set; }
            public int NotificationCooldownInSeconds { get; set; }
        }

        private class NotificationConfigContainer
        {
            public NotificationConfig SlackConfig { get; set; }
        }

        #endregion
    }
}

