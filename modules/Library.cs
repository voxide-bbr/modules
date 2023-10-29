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
    
    // Zombie stuff from https://github.com/RainOrigami/BattleBitZombies/blob/f375a8b15779362cb42e2c76180b0abc61d65d3b/Zombies.cs#L32C1-L45C93
    // Easy randomization like:
    /*
        request.Wearings.Eye = ZOMBIE_EYES[Random.Shared.Next(ZOMBIE_EYES.Length)];
        request.Wearings.Face = ZOMBIE_FACE[Random.Shared.Next(ZOMBIE_FACE.Length)];
        request.Wearings.Hair = ZOMBIE_HAIR[Random.Shared.Next(ZOMBIE_HAIR.Length)];
        request.Wearings.Skin = ZOMBIE_BODY[Random.Shared.Next(ZOMBIE_BODY.Length)];
        request.Wearings.Uniform = ZOMBIE_UNIFORM[Random.Shared.Next(ZOMBIE_UNIFORM.Length)];
        request.Wearings.Head = ZOMBIE_HELMET[Random.Shared.Next(ZOMBIE_HELMET.Length)];
        request.Wearings.Chest = ZOMBIE_ARMOR[Random.Shared.Next(ZOMBIE_ARMOR.Length)];
        request.Wearings.Backbag = ZOMBIE_BACKPACK[Random.Shared.Next(ZOMBIE_BACKPACK.Length)];
        request.Wearings.Belt = ZOMBIE_BELT[Random.Shared.Next(ZOMBIE_BELT.Length)];
    */
    public static readonly string[] HUMAN_UNIFORM = new[] { "ANY_NU_Uniform_Survivor_00", "ANY_NU_Uniform_Survivor_01", "ANY_NU_Uniform_Survivor_02", "ANY_NU_Uniform_Survivor_03", "ANY_NU_Uniform_Survivor_04" };
    public static readonly string[] HUMAN_HELMET = new[] { "ANV2_Survivor_All_Helmet_00_A_Z", "ANV2_Survivor_All_Helmet_00_B_Z", "ANV2_Survivor_All_Helmet_01_A_Z", "ANV2_Survivor_All_Helmet_02_A_Z", "ANV2_Survivor_All_Helmet_03_A_Z", "ANV2_Survivor_All_Helmet_04_A_Z", "ANV2_Survivor_All_Helmet_05_A_Z", "ANV2_Survivor_All_Helmet_05_B_Z" };
    public static readonly string[] HUMAN_BACKPACK = new[] { "ANV2_Survivor_All_Backpack_00_A_H", "ANV2_Survivor_All_Backpack_00_A_N", "ANV2_Survivor_All_Backpack_01_A_H", "ANV2_Survivor_All_Backpack_01_A_N", "ANV2_Survivor_All_Backpack_02_A_N" };
    public static readonly string[] HUMAN_ARMOR = new[] { "ANV2_Survivor_All_Armor_00_A_L", "ANV2_Survivor_All_Armor_00_A_N", "ANV2_Survivor_All_Armor_01_A_L", "ANV2_Survivor_All_Armor_02_A_L" };
    public static readonly string[] ZOMBIE_EYES = new[] { "Eye_Zombie_01" };
    public static readonly string[] ZOMBIE_FACE = new[] { "Face_Zombie_01" };
    public static readonly string[] ZOMBIE_HAIR = new[] { "Hair_Zombie_01" };
    public static readonly string[] ZOMBIE_BODY = new[] { "Zombie_01" };
    public static readonly string[] ZOMBIE_UNIFORM = new[] { "ANY_NU_Uniform_Zombie_01" };
    public static readonly string[] ZOMBIE_HELMET = new[] { "ANV2_Universal_Zombie_Helmet_00_A_Z" };
    public static readonly string[] ZOMBIE_ARMOR = new[] { "ANV2_Universal_All_Armor_Null" };
    public static readonly string[] ZOMBIE_BACKPACK = new[] { "ANV2_Universal_All_Backpack_Null" };
    public static readonly string[] ZOMBIE_BELT = new[] { "ANV2_Universal_All_Belt_Null" };
    #endregion Helpers
    #region Classes
    public class Dynamic
    {
        [ModuleReference]
        public dynamic? RichText { get; set; }
    }
    #endregion Classes
}