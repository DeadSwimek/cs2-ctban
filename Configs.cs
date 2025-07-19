using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.Json.Serialization;

namespace CTBans;


public class ConfigBan : BasePluginConfig
{
    [JsonPropertyName("Prefix")] public string Prefix { get; set; } = $" {ChatColors.Default}[{ChatColors.Green}MadGames.eu{ChatColors.Default}]";
    [JsonPropertyName("permission")] public string permission { get; set; } = "@css/reservation";
    [JsonPropertyName("SessionBan")] public string SessionBan { get; set; } = "css_ctsessionban";
    [JsonPropertyName("CTBan")] public string CTBan { get; set; } = "css_ctban";
    [JsonPropertyName("UNBan")] public string UNBan { get; set; } = "css_ctunban";
    [JsonPropertyName("IsBanned")] public string IsBanned { get; set; } = "css_isctbanned";
    [JsonPropertyName("DennySound")] public string DennySound { get; set; } = "sounds/ui/counter_beep.vsnd";
    [JsonPropertyName("TeamOfBan")] public int TeamOfBan { get; set; } = 3;



    [JsonPropertyName("DBDatabase")] public string DBDatabase { get; set; } = "database";
    [JsonPropertyName("DBUser")] public string DBUser { get; set; } = "user";
    [JsonPropertyName("DBPassword")] public string DBPassword { get; set; } = "password";
    [JsonPropertyName("DBHost")] public string DBHost { get; set; } = "localhost";
    [JsonPropertyName("DBPort")] public int DBPort { get; set; } = 3306;
}