# https://discord.com/invite/WNK777rhwg CONNECT FOR LIST ALL MY PLUGINS!!!



##### Lists of my plugins
> [VIP](https://github.com/DeadSwimek/cs2-vip), [VIP Premium](https://github.com/DeadSwimek/cs2-vip-premium), [SpecialRounds](https://github.com/DeadSwimek/cs2-specialrounds), [Countdown](https://github.com/DeadSwimek/cs2-countdown), [CTBans](https://github.com/DeadSwimek/cs2-ctban), [HideAdmin](https://github.com/DeadSwimek/cs2-hideadmin)

> If you wanna you can support me on this link - **https://www.paypal.com/paypalme/deadswim**

### Features

- Can banning player to connect in CT Team
- Can unbanning player to connect in CT Team
- Session banning the player
- Database maked
- Can make banlist


# Donators
***GreeNyTM*** Value **200 CZK**

# Commands
**css_ctban**

`Usage: /ctban <SteamID/PLAYERNAME> <Hours> 'REASON'`

**css_unctban**

`Usage: /unctban <SteamID>`

**css_isctbanned**

`Usage: /isctbanned <SteamID>`

**css_ctsessionban**

`Usage: /ctsessionban <PLAYERNAME> <REASON>`

#Config

```JSON
{
  "Prefix": " \u0001[\u0004MadGames.eu\u0001]",
  "permission": "@css/reservation",
  "SessionBan": "css_ctsessionban",
  "CTBan": "css_ctban",
  "UNBan": "css_ctunban",
  "IsBanned": "css_isctbanned",
  "DennySound": "sounds/ui/counter_beep.vsnd",
  "TeamOfBan": 3, // TeamNumber (3=CT)
  "DBDatabase": "database",
  "DBUser": "user",
  "DBPassword": "password",
  "DBHost": "localhost",
  "DBPort": 3306,
  "ConfigVersion": 1
}
```

