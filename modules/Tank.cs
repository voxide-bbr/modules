using BattleBitAPI.Common;
using BBRAPIModules;

using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

using Voxide;
using static Voxide.Library;

namespace Voxide;
[RequireModule(typeof(Library))]
[Module("Tank", "1.0.0")]
public class Tank : BattleBitModule
{
    public bool IsTankServer()
    {
        return Voxide.Library.IsTankServer(this.Server);
    }
    public override async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(RunnerPlayer player, OnPlayerSpawnArguments request)
    {
        if (IsTankServer())
        {
            if (player.Role == GameRole.Recon)
            {
                IEnumerable<RunnerPlayer> _teammates = Server.AllPlayers.Where(teammate => teammate.Team == player.Team);
                IEnumerable<RunnerPlayer> _snipers = Server.AllPlayers.Where(teammate => teammate.Team == player.Team && teammate.IsAlive && teammate.Role == GameRole.Recon);
                int teammates = _teammates.Count();
                int snipers = _snipers.Count();
                if (_snipers.Count() <= 0 || _teammates.Count() <= 1) return request;

                // Percentage of allowed snipers
                double max_snipers_percentage = 0.25d;

                // Minimum amount of snipers
                int min_snipers = 1;

                // Maximum amount of snipers
                int max_snipers = teammates;

                // Calculated amount of max snipers
                int max_snipers_threshold = Math.Min(max_snipers, Math.Max(min_snipers, (int)Math.Ceiling((double)teammates * max_snipers_percentage)));

                // Round up to nearest even number so we always have 2 per squad
                //if (max_snipers_threshold % 2 != 0)
                //    max_snipers_threshold += 1;

                // If too many snipers, alert user and disallow spawning
                if (snipers >= max_snipers_threshold)
                {
                    player.Message(
                        "Recon is temporarily disabled!\n" +
                        $"Your team is limited to {max_snipers_threshold} Recon.\n" +
                        "Limit adjusts according to team size."
                    , 10.0f);
                    return null;
                }
            }
        }
        return request; // return null to deny spawning
    }
}