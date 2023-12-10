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


    public ConfigBan Config { get; set; }


    public void OnConfigParsed(ConfigBan config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        WriteColor("CT BANS - Plugins has been [*LOADED*]", ConsoleColor.Green);
        CreateDatabase();

        AddCommandListener("jointeam", OnPlayerChangeTeam);
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
            banned[client] = false;
            remaining[client] = null;
            reason[client] = null;
        }
        return HookResult.Continue;
    }
    [GameEventHandler]
    public HookResult OnPlayerChangeTeam(CCSPlayerController? player, CommandInfo command)
    {
        var client = player.Index;

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
                player.PrintToChat($" {Config.Prefix} You are banned to connect in {ChatColors.LightBlue}CT{ChatColors.Default}.");
                player.PrintToChat($" {Config.Prefix} Remaining of Ban to {ChatColors.LightBlue}CT{ChatColors.Default} is {ChatColors.Red}{remaining[client]}.");
                player.PrintToChat($" {Config.Prefix} Reason of ban is is {ChatColors.Red}{reason[client]}.");

                player.ExecuteClientCommand("play sounds/ui/counter_beep.vsnd");
                return HookResult.Stop;
            }
        }

        return HookResult.Continue;
    }
}