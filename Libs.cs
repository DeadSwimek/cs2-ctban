using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Nexd.MySQL;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CTBans;

public partial class CTBans
{
    public bool IsInt(string sVal)
    {
        foreach (char c in sVal)
        {
            int iN = (int)c;
            if ((iN > 57) || (iN < 48))
                return false;
        }
        return true;
    }
    private string ReplaceColorTags(string input)
    {
        string[] colorPatterns =
        {
                "{DEFAULT}", "{RED}", "{LIGHTPURPLE}", "{GREEN}", "{LIME}", "{LIGHTGREEN}", "{LIGHTRED}", "{GRAY}",
                "{LIGHTOLIVE}", "{OLIVE}", "{LIGHTBLUE}", "{BLUE}", "{PURPLE}", "{GRAYBLUE}"
            };
        string[] colorReplacements =
        {
                "\x01", "\x02", "\x03", "\x04", "\x05", "\x06", "\x07", "\x08", "\x09", "\x10", "\x0B", "\x0C", "\x0E",
                "\x0A"
            };

        for (var i = 0; i < colorPatterns.Length; i++)
            input = input.Replace(colorPatterns[i], colorReplacements[i]);

        return input;
    }

    static void WriteColor(string message, ConsoleColor color)
    {
        var pieces = Regex.Split(message, @"(\[[^\]]*\])");

        for (int i = 0; i < pieces.Length; i++)
        {
            string piece = pieces[i];

            if (piece.StartsWith("[") && piece.EndsWith("]"))
            {
                Console.ForegroundColor = color;
                piece = piece.Substring(1, piece.Length - 2);
            }

            Console.Write(piece);
            Console.ResetColor();
        }

        Console.WriteLine();
    }
    public void CreateDatabase()
    {
        try
        {
            MySqlDb MySql = new MySqlDb(Config.DBHost, Config.DBUser, Config.DBPassword, Config.DBDatabase);



            MySql.ExecuteNonQueryAsync(@"CREATE TABLE IF NOT EXISTS `deadswim_ctbans` (`id` INT AUTO_INCREMENT PRIMARY KEY, `ban_steamid` VARCHAR(32) UNIQUE NOT NULL, `end` INT(11) NOT NULL, `reason` VARCHAR(32) NOT NULL, `banned_by` VARCHAR(32) NOT NULL, UNIQUE (`ban_steamid`));");
            MySql.ExecuteNonQueryAsync(@"CREATE TABLE IF NOT EXISTS `deadswim_ctbans_history` (`id` INT AUTO_INCREMENT PRIMARY KEY, `ban_steamid` VARCHAR(32) NOT NULL, `end` INT(11) NOT NULL, `reason` VARCHAR(32) NOT NULL, `banned_by` VARCHAR(32) NOT NULL, UNIQUE (`id`));");

            WriteColor($"CT BANS - *[MySQL {Config.DBHost} Connected]", ConsoleColor.Green);


        }
        catch (Exception ex)
        {
            WriteColor($"CT BANS - *[MYSQL ERROR WHILE LOADING: {ex.Message}]*", ConsoleColor.DarkRed);
        }
    }
    public bool CheckBan(CCSPlayerController? player)
    {
        MySqlDb MySql = new MySqlDb(Config.DBHost, Config.DBUser, Config.DBPassword, Config.DBDatabase);

        MySqlQueryResult result = MySql!.Table("deadswim_ctbans").Where(MySqlQueryCondition.New("ban_steamid", "=", player!.SteamID.ToString())).Select();
        if (result.Rows == 1)
        {
            return true;
        }
        return false;
    }
    public int GetPlayerBanTime(CCSPlayerController? player)
    {
        MySqlDb MySql = new MySqlDb(Config.DBHost, Config.DBUser, Config.DBPassword, Config.DBDatabase);

        MySqlQueryResult result = MySql!.Table("deadswim_ctbans").Where(MySqlQueryCondition.New("ban_steamid", "=", player!.SteamID.ToString())).Select();
        if (result.Rows == 1)
        {
            return result.Get<int>(0, "end");
        }
        return -1;
    }
    public string? GetPlayerBanReason(CCSPlayerController? player)
    {
        MySqlDb MySql = new MySqlDb(Config.DBHost, Config.DBUser, Config.DBPassword, Config.DBDatabase);

        MySqlQueryResult result = MySql!.Table("deadswim_ctbans").Where(MySqlQueryCondition.New("ban_steamid", "=", player!.SteamID.ToString())).Select();
        if (result.Rows == 1)
        {
            return $"{result.Get<string>(0, "reason")}";
        }
        return null;
    }
    public void CheckIfIsBanned(CCSPlayerController? player) 
    {
        if (player == null)
            return;

        var client = player.Index;
        if (CheckBan(player) == true)
        {
            var timeRemaining = DateTimeOffset.FromUnixTimeSeconds(GetPlayerBanTime(player)) - DateTimeOffset.UtcNow;
            var nowtimeis = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeRemainingFormatted =
            $"{timeRemaining.Days}d {timeRemaining.Hours}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";

            if (GetPlayerBanTime(player) < nowtimeis)
            {
                banned[client] = false;
                remaining[client] = null;
                reason[client] = null;
            }
            else
            {
                banned[client] = true;
                remaining[client] = $"{timeRemainingFormatted}";
                reason[client] = GetPlayerBanReason(player);
            }
        }
        else
        {
            if (session[client] == true)
            {
                banned[client] = true;
            }
            else
            {
                banned[client] = false;
                remaining[client] = null;
                reason[client] = null;
            }
        }
    }

}