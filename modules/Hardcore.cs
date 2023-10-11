using BattleBitAPI.Common;
using BBRAPIModules;

using System.Threading.Tasks;
using System.Linq;

using Voxide;
using static Voxide.Library;

namespace Voxide;
[RequireModule(typeof(Library))]
[Module("Hardcore", "1.0.0")]
public class Hardcore : BattleBitModule
{
    public bool IsHardcoreServer()
    {
        return Voxide.Library.IsHardcoreServer(this.Server);
    }
    public override async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(RunnerPlayer player, OnPlayerSpawnArguments request)
    {
        if (IsHardcoreServer())
        {
            #region Uniforms
            /* https://discord.com/channels/1139105158769426453/1139105160086442006/1145090099487453194
                Uniforms
                ANY_NU_Uniform_UniCBlue_00
                ANY_NU_Uniform_UniCGreen_00 
                ANY_NU_Uniform_UniCRed_00
                ANY_NU_Uniform_UniCYellow_00

                Armor
                ANV2_Universal_UniC_Armor_00_Blue_N 
                ANV2_Universal_UniC_Armor_00_Green_N 
                ANV2_Universal_UniC_Armor_00_Red_N
                ANV2_Universal_UniC_Armor_00_Yellow_N

                Backpack
                ANV2_Universal_UniC_Backpack_00_Blue_N
                ANV2_Universal_UniC_Backpack_00_Green_N 
                ANV2_Universal_UniC_Backpack_00_Red_N
                ANV2_Universal_UniC_Backpack_00_Yellow_N

                Belt
                ANV2_Universal_UniC_Belt_00_Blue_S
                ANV2_Universal_UniC_Belt_00_Green_S
                ANV2_Universal_UniC_Belt_00_Red_S
                ANV2_Universal_UniC_Belt_00_Yellow_S

                Helmet
                ANV2_Universal_UniC_Helmet_00_Blue_N
                ANV2_Universal_UniC_Helmet_00_Green_N
                ANV2_Universal_UniC_Helmet_00_Red_N
                ANV2_Universal_UniC_Helmet_00_Yellow_N
            */
            #endregion
            if (player.Team == Team.TeamA)
            {
                request.Wearings.Uniform = "ANY_NU_Uniform_UniCRed_00";
            }
            else if (player.Team == Team.TeamB)
            {
                request.Wearings.Uniform = "ANY_NU_Uniform_UniCBlue_00";
            }
        }
        return request; // return null to deny spawning
    }
}