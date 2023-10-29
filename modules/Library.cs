using BattleBitAPI.Common;
using BBRAPIModules;

using System;
using System.Threading.Tasks;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

using Voxide;
using Commands;
using BattleBitBaseModules;
using BattleBitDiscordWebhooks;

namespace Voxide;
[RequireModule(typeof(CommandHandler))]
[RequireModule(typeof(RichText))]
[RequireModule(typeof(DiscordWebhooks))]
[Module("Library", "1.0.0")]
public class Library : BattleBitModule
{
    [ModuleReference]
    public static CommandHandler CommandHandler { get; set; } = null!;
    [ModuleReference]
    public static RichText RichText { get; set; } = null!;
    public static DiscordWebhooks DiscordWebhooks = new DiscordWebhooks();

    #region Server Checkers
    public static bool IsDevelopmentServer(RunnerServer server)
    {
        if (server.ServerName.Contains("#1")) return true;
        return false;
    }
    public static bool IsVoxelServer(RunnerServer server)
    {
        if (server.ServerName.Contains("#2")) return true;
        return false;
    }
    public static bool IsTankServer(RunnerServer server)
    {
        if (server.ServerName.Contains("#3")) return true;
        return false;
    }
    public static bool IsHardcoreServer(RunnerServer server)
    {
        if (server.ServerName.Contains("#4")) return true;
        return false;
    }
    public static bool IsCoreServer(RunnerServer server)
    {
        if (server.ServerName.Contains("#5")) return true;
        return false;
    }
    #endregion Server Checkers
    #region Helpers
    public static string FirstToUpper(string s) {
        try {
            return $"{s[0].ToString().ToUpper()}{s.Substring(1)}";
        } catch {
            return s;
        }
    }
    #endregion Helpers
    #region Classes
    public class Dynamic
    {
        [ModuleReference]
        public dynamic? RichText { get; set; }
    }
    #endregion Classes
}