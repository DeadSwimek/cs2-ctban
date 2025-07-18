using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using System.ComponentModel;
using CounterStrikeSharp.API.Modules.Memory;
using Nexd.MySQL;
using CounterStrikeSharp.API.Modules.Entities;
using System;
using System.Drawing;

namespace CTBans;
[MinimumApiVersion(100)]

public static class GetUnixTime
{
    public static int GetUnixEpoch(this DateTime dateTime)
    {
        var unixTime = dateTime.ToUniversalTime() -
                       new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        return (int)unixTime.TotalSeconds;
    }
}

public partial class CTBans : BasePlugin, IPluginConfig<ConfigBan>
{
    public override string ModuleName => "CTBans";
    public override string ModuleAuthor => "DeadSwim";
    public override string ModuleDescription => "Banning players to join in CT.";
    public override string ModuleVersion => "V. 1.0.0";

    private static readonly bool?[] banned = new bool?[64];
    private static readonly string?[] remaining = new string?[64];
    private static readonly string?[] reason = new string?[64];
    private static readonly int?[] Showinfo = new int?[64];
    private static readonly bool?[] session = new bool?[64];


    public ConfigBan Config { get; set; }


    public void OnConfigParsed(ConfigBan config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        WriteColor("CT BANS - Plugins has been [*LOADED*]", ConsoleColor.Green);
        CreateDatabase();

        AddCommand(Config.SessionBan, "Session Ban Command", addsessionban);
        AddCommand(Config.CTBan, "Ban Command", addban);
        AddCommand(Config.UNBan, "UNBan Command", UnbanCT);
        AddCommand(Config.IsBanned, "IsBanned Command", InfobanCT);

        AddCommandListener("jointeam", OnPlayerChangeTeam);
        RegisterListener<Listeners.OnTick>(() =>
        {
        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            var ent = NativeAPI.GetEntityFromIndex(i);
            if (ent == 0)
                continue;
            var client = new CCSPlayerController(ent);
            if (client == null || !client.IsValid)
                continue;
                if (Showinfo[client.Index] == 1)
                {
                    var remain = remaining[client.Index];
                    var res = reason[client.Index];
                    if (remain == null || res == null) return;
                    client.PrintToCenterHtml(
                            $"<img src='https://icons.iconarchive.com/icons/paomedia/small-n-flat/48/sign-ban-icon.png'><br><br>" +
                            $"{Localizer["PrintToCenter1"]}" +
                            $"{Localizer["PrintToCenter2", remain]}" +
                            $"{Localizer["PrintToCenter3", res]}");
                    AddTimer(10.0f, () =>
                    {
                        Showinfo[client.Index] = null;
                    });
                }
        }
        });

    }
    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;
        if (player == null || !player.IsValid)
            return HookResult.Continue;
        var client = player.Index;
        if(CheckBan(player) == true)
        {
            var timeRemaining = DateTimeOffset.FromUnixTimeSeconds(GetPlayerBanTime(player)) - DateTimeOffset.UtcNow;
            var nowtimeis = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeRemainingFormatted =
            $"{timeRemaining.Days}d {timeRemaining.Hours}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";

            if (GetPlayerBanTime(player) < nowtimeis)
            {
                MySqlDb MySql = new MySqlDb(Config.DBHost, Config.DBUser, Config.DBPassword, Config.DBDatabase);
                var steamid = player.SteamID.ToString();
                MySql.Table("deadswim_ctbans").Where($"ban_steamid = '{steamid}'").Delete();
                banned[client] = false;
                remaining[client] = null;
                reason[client] = null;
                Showinfo[client] = null;
                session[client] = false;
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
            banned[client] = false;
            remaining[client] = null;
            reason[client] = null;
            session[client] = false;
        }
        return HookResult.Continue;
    }
    [GameEventHandler]
    public HookResult OnPlayerChangeTeam(CCSPlayerController? player, CommandInfo command)
    {
        var client = player!.Index;

        if (!Int32.TryParse(command.ArgByIndex(1), out int team_switch))
        {
            return HookResult.Continue;
        }

        if (player == null || !player.IsValid)
            return HookResult.Continue;
        CheckIfIsBanned(player);

        CCSPlayerPawn? playerpawn = player.PlayerPawn.Value;
        var player_team = team_switch;


        if(player_team == 3)
        {
            if (banned[client] == true)
            {
                Showinfo[client] = 1;
                player.ExecuteClientCommand("play sounds/ui/counter_beep.vsnd");
                return HookResult.Stop;
            }
        }

        return HookResult.Continue;
    }
}