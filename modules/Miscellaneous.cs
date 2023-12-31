using BattleBitAPI.Common;
using BBRAPIModules;

using System;
using System.Threading.Tasks;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

using Voxide;
using static Voxide.Library;
using static Voxide.Voxel;
using Commands;
using static BattleBitDiscordWebhooks.DiscordWebhooks;
using BattleBitDiscordWebhooks;
using System.Diagnostics.CodeAnalysis;

namespace Voxide;
[RequireModule(typeof(Library))]
[RequireModule(typeof(Voxel))]
[RequireModule(typeof(CommandHandler))]
[RequireModule(typeof(OpenSimplexNoise))]
[Module("Miscellaneous", "1.0.0")]
public class Miscellaneous : BattleBitModule
{
    [ModuleReference]
    public CommandHandler CommandHandler { get; set; } = null!;
    #region Settings
    public static bool isPeakHours(bool buffer = false) {
        // Get the current time in UTC
        DateTime currentTimeUtc = DateTime.UtcNow;

        // 10 AM local time
        int start = 14;

        // 10 PM local time
        int end = 2;

        if (buffer) {
            start += 1;
            end -= 1;
        }

        // Actual peak according to data was around 13 PM UTC to 5 AM UTC.

        // Define the start and end times for the range
        DateTime startTimeUtc = new DateTime(currentTimeUtc.Year, currentTimeUtc.Month, currentTimeUtc.Day, start, 0, 0);
        DateTime endTimeUtc = new DateTime(currentTimeUtc.Year, currentTimeUtc.Month, currentTimeUtc.Day, end, 0, 0);

        // Check if the current time is within the range
        if (currentTimeUtc >= startTimeUtc || currentTimeUtc < endTimeUtc)
            return true;
        return false;
    }
    public static int GetSoftPlayers(RunnerServer server)
    {
        if (Voxide.Library.IsDevelopmentServer(server))
            return 0;
        else if (Voxide.Library.IsVoxelServer(server))
            return 50;
        else if (Voxide.Library.IsTankServer(server))
            return 200;
        else if (Voxide.Library.IsHardcoreServer(server))
            return 100;
        else if (Voxide.Library.IsCoreServer(server))
            return 150;
        return 100;
    }
    public static string GetSoftPassword(RunnerServer server)
    {
        return "voxide";
    }
    public static void UpdateSoftPassword(RunnerServer? server, int offset)
    {
        if (server == null) return;
        bool isLimit = server.CurrentPlayerCount + offset >= GetSoftPlayers(server);
        bool hasPassword = server.IsPasswordProtected;

        if (isLimit) // Add password
            server.ExecuteCommand($"setpass {GetSoftPassword(server)}");
        else if (!isLimit) // Remove password
            server.ExecuteCommand("setpass");
    }
    public static void UpdateServer(RunnerServer? server)
    {
        if (server == null) return;

        server.ExecuteCommand("setspeedhackdetection false");

        if (isPeakHours() && (server.CurrentPlayerCount + server.InQueuePlayerCount) >= 2) server.ForceStartGame();

        if (Voxide.Library.IsDevelopmentServer(server))
        {
            // Everything super fast respawn
            server.ServerSettings.TankSpawnDelayMultipler = 1000.0f;
            server.ServerSettings.APCSpawnDelayMultipler = 1000.0f;
            server.ServerSettings.TransportSpawnDelayMultipler = 1000.0f;
            server.ServerSettings.SeaVehicleSpawnDelayMultipler = 1000.0f;
            server.ServerSettings.HelicopterSpawnDelayMultipler = 1000.0f;
        }
        else if (Voxide.Library.IsVoxelServer(server)) // #2 Voxel server
        {
            // Only transport vehicles spawn
            server.ServerSettings.APCSpawnDelayMultipler = 0.0f;
            server.ServerSettings.HelicopterSpawnDelayMultipler = 0.0f;
            server.ServerSettings.SeaVehicleSpawnDelayMultipler = 0.0f;
            server.ServerSettings.TankSpawnDelayMultipler = 0.0f;
            server.ServerSettings.TransportSpawnDelayMultipler = 1.0f;
        }
        else if (Voxide.Library.IsTankServer(server)) // #3 Tank server
        {
            // Tanks respawn twice as fast
            server.ServerSettings.APCSpawnDelayMultipler = 1.0f;
            server.ServerSettings.HelicopterSpawnDelayMultipler = 1.0f;
            server.ServerSettings.SeaVehicleSpawnDelayMultipler = 1.0f;
            server.ServerSettings.TankSpawnDelayMultipler = 2.0f;
            server.ServerSettings.TransportSpawnDelayMultipler = 1.0f;
            //server.ServerSettings.ReconLimitPerSquad = 8;

            // Disable night
            server.ServerSettings.CanVoteNight = false;

            // Dynamic size within limits
            if (server.CurrentPlayerCount >= 64 || (isPeakHours(true) && server.CurrentPlayerCount >= 32))
                server.SetServerSizeForNextMatch(MapSize._127vs127);
            else if (server.CurrentPlayerCount >= 32 || (isPeakHours(true) && server.CurrentPlayerCount >= 16))
                server.SetServerSizeForNextMatch(MapSize._64vs64);
            else
                server.SetServerSizeForNextMatch(MapSize._32vs32);
        }
        else if (Voxide.Library.IsHardcoreServer(server))
        {
            // Damage increase
            server.ServerSettings.DamageMultiplier = 1.1f;

            // Friendly fire
            server.ServerSettings.FriendlyFireEnabled = true;

            // Encourage transport use over other vehicles
            server.ServerSettings.APCSpawnDelayMultipler = 0.5f;
            server.ServerSettings.HelicopterSpawnDelayMultipler = 0.5f;
            server.ServerSettings.SeaVehicleSpawnDelayMultipler = 0.5f;
            server.ServerSettings.TankSpawnDelayMultipler = 0.5f;
            server.ServerSettings.TransportSpawnDelayMultipler = 2.0f;

            // Dynamic size within limits
            if (server.CurrentPlayerCount >= 64)
                server.SetServerSizeForNextMatch(MapSize._127vs127);
            else if (server.CurrentPlayerCount >= 32)
                server.SetServerSizeForNextMatch(MapSize._64vs64);
            else if (server.CurrentPlayerCount >= 16)
                server.SetServerSizeForNextMatch(MapSize._32vs32);
            else if (server.CurrentPlayerCount >= 8)
                server.SetServerSizeForNextMatch(MapSize._16vs16);
            else
                server.SetServerSizeForNextMatch(MapSize._8v8);
        }
        else if (Voxide.Library.IsCoreServer(server))
        {
            // Dynamic size within limits
            if (server.CurrentPlayerCount >= 64)
                server.SetServerSizeForNextMatch(MapSize._127vs127);
            else if (server.CurrentPlayerCount >= 32)
                server.SetServerSizeForNextMatch(MapSize._64vs64);
            else if (server.CurrentPlayerCount >= 16)
                server.SetServerSizeForNextMatch(MapSize._32vs32);
            else if (server.CurrentPlayerCount >= 8)
                server.SetServerSizeForNextMatch(MapSize._16vs16);
            else
                server.SetServerSizeForNextMatch(MapSize._8v8);
        }
    }
    public static void UpdatePlayer(RunnerServer? server, RunnerPlayer? player)
    {
        if (server == null || player == null) return;
        UpdateServer(server);
        if (Voxide.Library.IsDevelopmentServer(server))
        {
            // Double some stuff, halve some stuff, etc
            player.Modifications.ReloadSpeedMultiplier = 2.0f;
            player.Modifications.RunningSpeedMultiplier = 2.0f;
            player.Modifications.JumpHeightMultiplier = 2.0f;
            player.Modifications.ReceiveDamageMultiplier = 0.5f;
            player.Modifications.IsExposedOnMap = true;
        }
        else if (Voxide.Library.IsVoxelServer(server))
        {
            if (server.CurrentPlayerCount <= 64)
                player.Modifications.KillFeed = true;
                
            // Restrict vehicles that can be entered into
            player.Modifications.AllowedVehicles = (VehicleType.All ^ VehicleType.Tank);

            // Restrict vehicles that can be spawned into
            player.Modifications.SpawningRule = (SpawningRule.All ^ SpawningRule.Tanks);
        }
        else if (Voxide.Library.IsTankServer(server))
        {
            if (server.CurrentPlayerCount <= 64)
                player.Modifications.KillFeed = true;
        }
        else if (Voxide.Library.IsHardcoreServer(server))
        {
            // Disable some spawnings
            player.Modifications.SpawningRule = SpawningRule.All ^ SpawningRule.SquadMates;

            // Make player die faster
            player.Modifications.DownTimeGiveUpTime = 20.0f;
            player.Modifications.ReviveHP = 15.0f;
            player.Modifications.HpPerBandage = 30.0f;

            // Disable hud
            player.Modifications.FriendlyHUDEnabled = false;
            player.Modifications.HideOnMap = true;
            player.Modifications.IsExposedOnMap = false;
            player.Modifications.HitMarkersEnabled = false;
        }
        else if (Library.IsCoreServer(server))
        {
            if (server.CurrentPlayerCount <= 64)
                player.Modifications.KillFeed = true;
        }
    }
    #endregion Settings
    #region Commands
    [CommandCallback("Start", Description = "Force start the round", Permissions = new[] { "Miscellaneous.Start" })]
    public void Start(RunnerPlayer commandSource)
    {
        commandSource.SayToChat("Force starting game!");
        Server.ForceStartGame();
    }
    [CommandCallback("End", Description = "Force end the round", Permissions = new[] { "Miscellaneous.End" })]
    public void End(RunnerPlayer commandSource)
    {
        commandSource.SayToChat("Force ending game!");
        Server.ForceEndGame();
    }
    [CommandCallback("Build", Description = "Start the voxel build process", Permissions = new[] { "Miscellaneous.Build" })]
    public void Build(RunnerPlayer commandSource)
    {
        commandSource.SayToChat("Running build!");
        Voxel.Utility.halt = false;
        Voxel.SpawnVoxelWorld(this.Server);
    }
    [CommandCallback("BuildEnable", Description = "Enable voxel build function", Permissions = new[] { "Miscellaneous.BuildEnable" })]
    public void BuildEnable(RunnerPlayer commandSource)
    {
        commandSource.SayToChat("Enabling build function!");
        Voxel.Utility.halt = false;
    }
    [CommandCallback("BuildDisable", Description = "Stop & disable voxel build function", Permissions = new[] { "Miscellaneous.BuildDisable" })]
    public void BuildDisable(RunnerPlayer commandSource)
    {
        commandSource.SayToChat("Disabling build function!");
        Voxel.Utility.halt = true;
    }
    [CommandCallback("BuildQuery", Description = "Get state of voxel build function", Permissions = new[] { "Miscellaneous.BuildQuery" })]
    public void BuildQuery(RunnerPlayer commandSource)
    {
        commandSource.SayToChat("Build halt is set to " + Voxel.Utility.halt.ToString() + "!");
    }
    [CommandCallback("Noise", Description = "Test function", Permissions = new[] { "Miscellaneous.Noise" })]
    public void Noise(RunnerPlayer commandSource)
    {
        OpenSimplexNoise noise = new OpenSimplexNoise();
        List<double> noises = new();
        int size = 2;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    noises.Add(noise.Evaluate(x, y, z));
                }
            }
        }
        commandSource.SayToChat(string.Join(",",noises));
    }


    #endregion Commands
    public override void OnModulesLoaded()
    {
        this.CommandHandler.Register(this);
        if (Server != null)
        {
            UpdateServer(Server);
            UpdateSoftPassword(Server, 0);
        }
    }
    public override Task OnConnected()
    {
        UpdateServer(Server);
        UpdateSoftPassword(Server, 0);
        return Task.CompletedTask;
    }
    public override Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        UpdateServer(Server);
        return Task.CompletedTask;
    }
    public override Task OnPlayerConnected(RunnerPlayer player)
    {
        UpdatePlayer(Server, player);

        UpdateSoftPassword(Server, 1);

        // Player join message
        this.Server.UILogOnServer($"{player.Name} (＋)", 3.0f);

        return Task.CompletedTask;
    }
    public override Task OnPlayerDisconnected(RunnerPlayer player)
    {
        UpdateServer(Server);

        UpdateSoftPassword(Server, -1);

        // Player leave message
        this.Server.UILogOnServer($"{player.Name} (－)", 3.0f);

        return Task.CompletedTask;
    }
    public override Task OnPlayerSpawned(RunnerPlayer player)
    {
        UpdatePlayer(Server, player);
        return Task.CompletedTask;
    }
    public override Task OnPlayerDied(RunnerPlayer player)
    {
        UpdatePlayer(Server, player);
        return Task.CompletedTask;
    }
    public override Task OnTick()
    {
        if (Voxide.Library.IsDevelopmentServer(Server))
        {
            // For each player, output their position in log
            foreach (RunnerPlayer player in Server.AllPlayers)
            {
                if (player.IsAlive && player.Name.ToLower().Contains("sonicscream"))
                {
                    player.Message(
                        "Position:\n" +
                        player.Position.ToString()
                    );
                }
            }
        }
        return Task.CompletedTask;
    }
}
