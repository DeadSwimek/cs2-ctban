using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CTAPI;
using Nexd.MySQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable CS0436

namespace CTBans
{
    public class CoreAPI: IAPI
    {
        private CTBans _api;
        public CoreAPI(CTBans api)
        {
            _api = api;
        }
        public void BanPlayer(CCSPlayerController player, string hours, string reason, string target)
        {
            if (player == null) return;
            if (target == "" || !_api.IsInt(target)) return;
            if (hours == "0" || !_api.IsInt(hours)) return;


            var SteamID = target;
            var TimeHours = hours;
            var Reason = reason;
            var Bannedby = player.SteamID.ToString();

            var TimeToUTC = DateTime.UtcNow.AddHours(Convert.ToInt32(TimeHours)).GetUnixEpoch();
            var BanTime = 0;
            if (TimeHours.ToString() == "0")
            {
                BanTime = 0;
            }
            else
            {
                BanTime = DateTime.UtcNow.AddHours(Convert.ToInt32(TimeHours)).GetUnixEpoch();
            }


            var timeRemaining = DateTimeOffset.FromUnixTimeSeconds(TimeToUTC) - DateTimeOffset.UtcNow;
            var timeRemainingFormatted = $"{timeRemaining.Days}d {timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";

            MySqlDb MySql = new MySqlDb(_api.Config.DBHost, _api.Config.DBUser, _api.Config.DBPassword, _api.Config.DBDatabase);

            MySqlQueryResult result = MySql!.Table("deadswim_ctbans").Where(MySqlQueryCondition.New("ban_steamid", "=", SteamID)).Select();
            if (result.Rows == 0)
            {
                MySqlQueryValue values = new MySqlQueryValue()
                .Add("ban_steamid", $"{SteamID}")
                .Add("end", $"{BanTime}")
                .Add("reason", $"{Reason}")
                .Add("banned_by", $"{Bannedby}");
                MySql.Table("deadswim_ctbans").Insert(values);

                MySqlQueryValue values_his = new MySqlQueryValue()
                .Add("ban_steamid", $"{SteamID}")
                .Add("end", $"{BanTime}")
                .Add("reason", $"{Reason}")
                .Add("banned_by", $"{Bannedby}");
                MySql.Table("deadswim_ctbans_history").Insert(values_his);

                foreach (var find_player in Utilities.GetPlayers())
                {
                    if (find_player.SteamID.ToString() == SteamID)
                    {
                        find_player.PrintToChat($" {_api.Config.Prefix} {_api.Localizer["GotBan", player!.PlayerName, Reason]}");
                        find_player.ChangeTeam(CounterStrikeSharp.API.Modules.Utils.CsTeam.Terrorist);
                    }
                }
            }
            else
            {
                return;
            }
        }
        public bool IsBanned(CCSPlayerController player)
        {
            if (player == null) return false;

            MySqlDb MySql = new MySqlDb(_api.Config.DBHost, _api.Config.DBUser, _api.Config.DBPassword, _api.Config.DBDatabase);
            var SteamID = player.SteamID.ToString();
            MySqlQueryResult result = MySql!.Table("deadswim_ctbans").Where(MySqlQueryCondition.New("ban_steamid", "=", SteamID)).Select();
            if (result.Rows == 0)
            {
                return false;
            }
            return true; ;
        }
        public void BanInfo(CCSPlayerController player)
        {
            if (player == null) return;
            player.PrintToChat($" {_api.Config.Prefix} Remaining time of ban : {_api.GetPlayerBanTime(player)}");
            player.PrintToChat($" {_api.Config.Prefix} Reason of Ban : {_api.GetPlayerBanReason(player)}");
        }
        
    }
}
