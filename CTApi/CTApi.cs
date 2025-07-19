using CounterStrikeSharp.API.Core;
using System.Collections.Generic;
using System;

namespace CTAPI;

public interface IAPI
{
    bool IsBanned(CCSPlayerController player);
    public void BanInfo(CCSPlayerController player);
    public void BanPlayer(CCSPlayerController banned_by, string hours, string reason, string target);
}