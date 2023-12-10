using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Nexd.MySQL;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;

namespace CTBans;

public partial class CTBans
{
    [ConsoleCommand("css_ctban", "Ban player to CT")]
    public void addban(CCSPlayerController? player, CommandInfo info)
    {
        if (!AdminManager.PlayerHasPermissions(player, "@css/ban"))
        {
            info.ReplyToCommand($" {Config.Prefix} You dosen't have permission to this command!");
            return;
        }
        var SteamID = info.ArgByIndex(1);
        var TimeHours = info.ArgByIndex(2);
        var Reason = info.GetArg(3);
        var Bannedby = "";
        if (player == null)
        {
            Bannedby = "CONSOLE";
        }
        else
        {
            Bannedby = player.SteamID.ToString();
        }

        if (SteamID == null || !IsInt(SteamID))
        {
            info.ReplyToCommand($" {Config.Prefix} Steamid is must be number! Example : css_ctban <SteamID> <Hours> 'REASON' | Example2 : css_ctban 7777777777777 24 Greafing");
            return;
        }
        else if (TimeHours == null || !IsInt(TimeHours))
        {
            info.ReplyToCommand($" {Config.Prefix} Time must be in hours! Example : css_ctban <SteamID> <Hours> 'REASON' | Example2 : css_ctban 7777777777777 24 Greafing");
            return;
        }
        else if (Reason == null || IsInt(Reason))
        {
            info.ReplyToCommand($" {Config.Prefix} Reason canno't be a number! Example : css_ctban <SteamID> <Hours> 'REASON' | Example2 : css_ctban 7777777777777 24 Greafing");
        }
        else
        {


            var TimeToUTC = DateTime.UtcNow.AddHours(Convert.ToInt32(TimeHours)).GetUnixEpoch();
            var BanTime = 0;
            if (TimeHours == "0")
            {
                BanTime = 0;
            }
            else
            {
                BanTime = DateTime.UtcNow.AddHours(Convert.ToInt32(TimeHours)).GetUnixEpoch();
            }


            var timeRemaining = DateTimeOffset.FromUnixTimeSeconds(TimeToUTC) - DateTimeOffset.UtcNow;
            var timeRemainingFormatted = $"{timeRemaining.Days}d {timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";

            MySqlDb MySql = new MySqlDb(Config.DBHost, Config.DBUser, Config.DBPassword, Config.DBDatabase);

            MySqlQueryResult result = MySql!.Table("deadswim_ctbans").Where(MySqlQueryCondition.New("ban_steamid", "=", SteamID)).Select();
            if (result.Rows == 0)
            {
                MySqlQueryValue values = new MySqlQueryValue()
                .Add("ban_steamid", $"{SteamID}")
                .Add("end", $"{BanTime}")
                .Add("reason", $"{Reason}")
                .Add("banned_by", $"{Bannedby}");
                MySql.Table("deadswim_ctbans").Insert(values);

                info.ReplyToCommand($" {Config.Prefix} You successful ban player with steamid {SteamID}");
                foreach (var find_player in Utilities.GetPlayers().Where(player => player is { IsBot: false, IsValid: true }))
                {
                    if(find_player.SteamID.ToString() == SteamID)
                    {
                        find_player.PrintToChat($" {Config.Prefix} You are banned from {ChatColors.LightBlue}CT{ChatColors.Default} by admin {ChatColors.Red}{player.PlayerName}{ChatColors.Default} for reason: {ChatColors.Gold}{Reason} ");
                        find_player.ChangeTeam(CounterStrikeSharp.API.Modules.Utils.CsTeam.Terrorist);
                    }
                }
            }
            else
            {
                info.ReplyToCommand($" {Config.Prefix} This SteamID it already is banned from admin!");
            }
        }
    }
    [ConsoleCommand("css_unctban", "UNBan player to CT")]
    public void UnbanCT(CCSPlayerController? player, CommandInfo info)
    {
        if (!AdminManager.PlayerHasPermissions(player, "@css/ban"))
        {
            info.ReplyToCommand($" {Config.Prefix} You dosen't have permission to this command!");
            return;
        }
        var SteamID = info.ArgByIndex(1);
        if (SteamID == null || !IsInt(SteamID))
        {
            info.ReplyToCommand($" {Config.Prefix} Steamid is must be number! Example : css_unctban <SteamID> | Example2 : css_unctban 7777777777777");
            return;
        }

        MySqlDb MySql = new MySqlDb(Config.DBHost, Config.DBUser, Config.DBPassword, Config.DBDatabase);

        MySqlQueryResult result = MySql!.Table("deadswim_ctbans").Where(MySqlQueryCondition.New("ban_steamid", "=", SteamID)).Select();
        if (result.Rows == 0)
        {
            info.ReplyToCommand($" {Config.Prefix} This steamid is not banned to connect in CT!");
        }
        else
        {
            MySql.Table("deadswim_ctbans").Where($"ban_steamid = '{SteamID}'").Delete();
            info.ReplyToCommand($" {Config.Prefix} You successful unban a player to play in CT Team!");
        }
    }
}