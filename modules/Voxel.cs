using BattleBitAPI;
using BattleBitAPI.Common;
using BBRAPIModules;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

using Voxide;
using static Voxide.Library;
using System;
using static System.Random;
using static Voxide.OpenSimplexNoise;
using System.Reflection;
using System.Text.Json;
using Microsoft.VisualBasic;

namespace Voxide;
[RequireModule(typeof(Library))]
[RequireModule(typeof(OpenSimplexNoise))]
[Module("Voxel", "1.0.0")]
public class Voxel : BattleBitModule
{
    public class MapBoundaries
    {
        public int SizeX { get; private set; }
        public int SizeY { get; private set; }
        public int SizeZ { get; private set; }
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }
        public int OffsetZ { get; private set; }
        public MapBoundaries(int sizeX, int sizeY, int sizeZ, int offsetX, int offsetY, int offsetZ)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;
            OffsetX = offsetX;
            OffsetY = offsetY;
            OffsetZ = offsetZ;
        }
        public MapVolume getMapVolume()
        {
            return new MapVolume(SizeX, SizeY, SizeZ, OffsetX, OffsetY, OffsetZ);
        }
    }
    public class MapVolume
    {
        private int[,,] data; // Multi-dimensional array to represent the voxel data

        public static int errors = 0;

        public int SizeX { get; private set; }
        public int SizeY { get; private set; }
        public int SizeZ { get; private set; }
        public int OffsetX; // Offset X coordinate
        public int OffsetY; // Offset Y coordinate
        public int OffsetZ; // Offset Z coordinate

        // 0 = air, 1 = solid (gray), 2 = solid (orange) 3 = random solid
        public static int[] voxel_types = { 0, 1, 2, 3 };

        public MapVolume(int sizeX, int sizeY, int sizeZ, int offsetX, int offsetY, int offsetZ)
        {
            // Perimiter dimensions
            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;

            // Where the data[0,0,0] is in game space (imagine if your offset is -10, -10, -10 then your data[0,0,0] is at -10, -10, -10)
            OffsetX = offsetX;
            OffsetY = offsetY;
            OffsetZ = offsetZ;

            // The volume data
            data = new int[sizeX, sizeY, sizeZ];
        }
        private Vector3 translatePositionGameToMapVolume(Vector3 game_position)
        {
            return new Vector3(game_position.X - OffsetX, game_position.Y - OffsetY, game_position.Z - OffsetZ);
        }
        private Vector3 translatePositionMapVolumeToGame(Vector3 volume_position)
        {
            return new Vector3(volume_position.X + OffsetX, volume_position.Y + OffsetY, volume_position.Z + OffsetZ);
        }
        private bool isValidPositionMapVolume(Vector3 volume_position)
        {
            return volume_position.X >= 0 && volume_position.X < SizeX && volume_position.Y >= 0 && volume_position.Y < SizeY && volume_position.Z >= 0 && volume_position.Z < SizeZ;
        }
        private bool isValidPositionGame(Vector3 game_position)
        {
            return isValidPositionMapVolume(translatePositionGameToMapVolume(game_position));
        }
        private bool SetMapVolume(Vector3 volume_position, int type)
        {
            // types: 0 = air, 1 = solid (gray), 2 = solid (orange)
            if (isValidPositionMapVolume(volume_position))
            {
                data[(int)volume_position.X, (int)volume_position.Y, (int)volume_position.Z] = type;
                return true;
            }
            return false;
        }
        private bool SetVoxelGame(Vector3 game_position, int type)
        {
            return SetMapVolume(translatePositionGameToMapVolume(game_position), type);
        }
        private int GetMapVolume(Vector3 volume_position)
        {
            int voxel = -1;
            if (isValidPositionMapVolume(volume_position))
                voxel = data[(int)volume_position.X, (int)volume_position.Y, (int)volume_position.Z];

            // Check if voxel is 0, 1, or 2, 3
            if (!(voxel >= voxel_types.Min() && voxel <= voxel_types.Max())) return -1;
            return voxel;
        }
        private int GetVoxelGame(Vector3 game_position)
        {
            return GetMapVolume(translatePositionGameToMapVolume(game_position));
        }
        // PUBLIC FUNCTIONS
        public bool SetVoxel(Vector3 game_position, int type)
        {
            return SetVoxelGame(game_position, type);
        }
        public int GetVoxel(Vector3 game_position)
        {
            return GetVoxelGame(game_position);
        }
        // Is coordinate inside of a cuboid, doesn't require use of data so no coordinate translation needed
        public bool IsInCuboid(int x1, int y1, int z1, int x2, int y2, int z2, int x, int y, int z)
        {
            return x >= x1 && x <= x2 && y >= y1 && y <= y2 && z >= z1 && z <= z2;
        }
        public bool SetCuboid(int x1, int y1, int z1, int x2, int y2, int z2, int type)
        {
            bool success = true;
            for (int x = x1; x <= x2; x++)
            {
                for (int y = y1; y <= y2; y++)
                {
                    for (int z = z1; z <= z2; z++)
                    {
                        if (!SetVoxelGame(new Vector3(x, y, z), type))
                            success = false;
                    }
                }
            }
            return success;
        }
        public bool SetCuboidModulo(int x1, int y1, int z1, int x2, int y2, int z2, int type, int modulo)
        {
            bool success = true;
            for (int x = x1; x <= x2; x++)
            {
                for (int y = y1; y <= y2; y++)
                {
                    for (int z = z1; z <= z2; z++)
                    {
                        if (x % modulo == 0 && y % modulo == 0 && z % modulo == 0)
                            if (!SetVoxelGame(new Vector3(x, y, z), type))
                                success = false;
                    }
                }
            }
            return success;
        }
        public double GetNoise(OpenSimplexNoise noiseGenerator, int x, int y, int octaves = 1, double scale = 1.0d, double lacunarity = 1.0d, double persistance = 1.0d)
        {
            // Returns a double between 0 and 1 exclusive if amplitude is 1
            double result = 0;

            double max = 0;
            double min = 0;

            for (int octave = 0; octave < octaves; octave++)
            {
                double frequency = Math.Pow(lacunarity, octave);
                double amplitude = Math.Pow(persistance, octave);
                double samplex = (x / scale) * frequency;
                double sampley = (y / scale) * frequency;
                double noise = noiseGenerator.Evaluate(samplex, sampley);
                noise *= amplitude;
                max += 1 * amplitude;
                min += -1 * amplitude;
                result += noise;
            }

            result = (double)(result - min) / (max - min);

            string msg = result.ToString();

            return result;
        }
        public bool SetCuboidNoise(int x1, int y1, int z1, int x2, int y2, int z2, int type, int octaves = 1, double scale = 1.0d, double lacunarity = 1.0d, double persistance = 1.0d)
        {
            OpenSimplexNoise noise = new();

            bool success = true;
            for (int x = x1; x <= x2; x++)
            {
                for (int y = y1; y <= y2; y++)
                {
                    double _noise = GetNoise(noise, x, y, octaves, scale, lacunarity, persistance);
                    int zMax = Math.Max(
                        z1,
                        Math.Min(
                            z2,
                            (int)Math.Round(
                                _noise * Math.Max(Math.Abs(z1 - z2), Math.Abs(z2 - z1))
                                )
                            )
                        );
                    for (int z = z1; z <= z2; z++) // need to fix this part, the z will be offset if it doesnt start at z coord 1
                    {
                        if (z <= zMax)
                        {
                            if (!SetVoxelGame(new Vector3(x, y, z), type))
                                success = false;
                        }
                    }
                }
            }
            return success;
        }

        // Is coordinate inside of a sphere, doesn't require use of data so no coordinate translation needed
        public bool IsInSphere(int centerX, int centerY, int centerZ, int radius, int x, int y, int z)
        {
            int dx = x - centerX;
            int dy = y - centerY;
            int dz = z - centerZ;
            return dx * dx + dy * dy + dz * dz <= radius * radius;
        }
        public bool SetSphere(int centerX, int centerY, int centerZ, int radius, int type)
        {
            bool success = true;
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int y = centerY - radius; y <= centerY + radius; y++)
                {
                    for (int z = centerZ - radius; z <= centerZ + radius; z++)
                    {
                        if (IsInSphere(centerX, centerY, centerZ, radius, x, y, z))
                            if (!SetVoxelGame(new Vector3(x, y, z), type))
                                success = false;
                    }
                }
            }
            return success;
        }
    }

    public static class Utility
    {
        public static Random Random = new();
        public async static Task<List<Vector3>> LoadVoxelPointsFromJsonFile(string filePath)
        {
            try
            {
                string jsonString = await File.ReadAllTextAsync(filePath);

                // Parse the JSON array of arrays directly into a List<Vector3>
                List<List<int>>? _data = JsonSerializer.Deserialize<List<List<int>>>(jsonString);
                if (_data == null) _data = new();
                var data = _data.ConvertAll(point => new Vector3(point[0], point[1], point[2]));
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return new();
            }
        }
        public async static Task PlaceMapFile(RunnerServer server, string path, MapVolume volume)
        {
            List<Vector3> voxelPoints = await LoadVoxelPointsFromJsonFile(path);

            if (voxelPoints == null) return;
            foreach (Vector3 point in voxelPoints) {
                Vector3 position = new Vector3(point.X-256,point.Y-256,point.Z+1);
                volume.SetVoxel(position, 3);
            }
        }
        //                                         volume.SetCuboid(-256, -179, 1, 255, 179, 4, 1);
        public static MapBoundaries FORTIFY = new MapBoundaries(256 * 2, 179 * 2 + 1, 100, -256, -179, 1);

        // Check that this one draws correctly with a cubioud
        public static MapBoundaries TRENCH = new MapBoundaries(256 * 2, 256 * 2, 100, -256, -256, 1);

        public static bool halt = false;
        public static bool first = true;
        public static Task SetVoxel(RunnerServer server, int x, int y, int z, int type)
        {
            // Types: <=0: air, 1: solid, 2: solid_orange
            // Vector3(x, z, y)
            if (type <= 0)
                server.DestroyVoxelBlock(new Vector3(x, z, y));
            else if (type == 1)
                server.PlaceVoxelBlock(new Vector3(x, z, y), new VoxelBlockData());
            else if (type == 2)
            {
                VoxelBlockData VoxelBlockData = new VoxelBlockData();
                VoxelBlockData.TextureID = VoxelTextures.NeonOrange;
                server.PlaceVoxelBlock(new Vector3(x, z, y), VoxelBlockData);
            }
            else if (type == 3)
            {
                bool even = (int)Random.NextInt64(1000) % 2 == 0;
                if (even) type = 1;
                else type = 2;
                SetVoxel(server, x, y, z, type);
            }
            return Task.CompletedTask;
        }
        public async static Task SetVoxels(RunnerServer server, MapVolume volume, int mode = 0, int? delay = 0, bool immediate = false)
        {
            if (delay != null && delay > 0)
                await Task.Delay((int)delay);
            bool dev = Voxide.Library.IsDevelopmentServer(server);
            // modes 0=normal 1=place 2=remove 3=placeremove 4=removeplace
            int delayMilliseconds = 10; // Adjust the delay duration as needed
            int count_loop = 0;
            int count_voxel = 0;
            for (int z = volume.OffsetZ; z < (volume.SizeZ + volume.OffsetZ); z++)
            {
                for (int x = volume.OffsetX; x < (volume.SizeX + volume.OffsetX); x++)
                {
                    for (int y = volume.OffsetY; y < (volume.SizeY + volume.OffsetY); y++)
                    {
                        if (Utility.halt)
                        {
                            Utility.halt = !Utility.halt;
                            return;
                        }
                        // Call the game server's voxel placement function asynchronously
                        int voxel = volume.GetVoxel(new Vector3(x, y, z));

                        if (mode == 0 || (mode == 1 && voxel >= 1) || (mode == 2 && voxel < 1) || (mode == 3 && voxel >= 1))
                        {
                            if (mode == 3) voxel = 0;
                            else if (mode == 4) voxel = 3;
                            await Utility.SetVoxel(server, x, y, z, voxel);
                            count_voxel++;

                            // Introduce a delay between voxel placements
                            if (!immediate && (dev && count_voxel % 1000 == 0 || !dev && count_voxel % 1000 == 0))
                            {
                                //await Console.Out.WriteLineAsync($"{count_voxel} voxels altered");
                                await Task.Delay(delayMilliseconds);
                            }
                        }
                        count_loop++;
                    }
                }
            }
            //await Console.Out.WriteLineAsync($"{count_loop} voxels checked, {count_voxel} voxels altered.");
        }
    }

    public static bool IsVoxelServer(RunnerServer server)
    {
        return Voxide.Library.IsVoxelServer(server);
    }
    public bool IsVoxelServer()
    {
        return Voxide.Library.IsVoxelServer(Server);
    }
    public static async void SpawnVoxelWorld(RunnerServer server, bool immediate = false)
    {
        if (IsVoxelServer(server))
        {
            if (server.Gamemode.ToLower().Contains("fortify"))
            {
                /*
                MapVolume volumeRemove = Utility.FORTIFY.getMapVolume();
                // D = -127, 96  | E = 0, 96  | F = 128, 96
                // A = -127, -95 | B = 0, -95 | C = 128, -95

                int flagRadius = 25;
                int flagHeight = 25;
                volumeRemove.SetSphere(-127, -95, flagHeight, flagRadius, 0);
                volumeRemove.SetSphere(0, -95, flagHeight, flagRadius, 0);
                volumeRemove.SetSphere(128, -95, flagHeight, flagRadius, 0);
                volumeRemove.SetSphere(-127, 96, flagHeight, flagRadius, 0);
                volumeRemove.SetSphere(0, 96, flagHeight, flagRadius, 0);
                volumeRemove.SetSphere(128, 96, flagHeight, flagRadius, 0);
                // cut off the top half of the spheres
                volumeRemove.SetCuboid(-256, -179, 26, 255, 179, 2, 100, 1);
                // Remove only mode
                Utility.SetVoxels(this.Server, volumeRemove, 2);
                */

                MapVolume volume = Utility.FORTIFY.getMapVolume();
                int flagRadius = 25;
                int flagHeight = 50;

                // Above Flag Spheres

                // Add
                volume.SetSphere(-127, -95, flagHeight, flagRadius, 2);
                volume.SetSphere(0, -95, flagHeight, flagRadius, 2);
                volume.SetSphere(128, -95, flagHeight, flagRadius, 2);
                volume.SetSphere(-127, 96, flagHeight, flagRadius, 2);
                volume.SetSphere(0, 96, flagHeight, flagRadius, 2);
                volume.SetSphere(128, 96, flagHeight, flagRadius, 2);

                // Hollow out
                volume.SetSphere(-127, -95, flagHeight, flagRadius - 2, 0);
                volume.SetSphere(0, -95, flagHeight, flagRadius - 2, 0);
                volume.SetSphere(128, -95, flagHeight, flagRadius - 2, 0);
                volume.SetSphere(-127, 96, flagHeight, flagRadius - 2, 0);
                volume.SetSphere(0, 96, flagHeight, flagRadius - 2, 0);
                volume.SetSphere(128, 96, flagHeight, flagRadius - 2, 0);

                // Cut most of sphere except bottom bowl

                volume.SetCuboid(-256, -179, 36, 255, 179, 100, 0);

                /*
                // Center spheres
                volume.SetSphere(-127, 0, (int)(flagHeight * 0), flagRadius, 2);
                volume.SetSphere(0, 0, (int)(flagHeight * 0), flagRadius, 2);
                volume.SetSphere(128, 0, (int)(flagHeight * 0), flagRadius, 2);

                // Center spheres hollow out
                volume.SetSphere(-127, 0, (int)(flagHeight * 0), flagRadius - 2, 0);
                volume.SetSphere(0, 0, (int)(flagHeight * 0), flagRadius - 2, 0);
                volume.SetSphere(128, 0, (int)(flagHeight * 0), flagRadius - 2, 0);
                */

                await Utility.SetVoxels(server, volume, 1, 0, immediate);

            }
            else if (server.Gamemode.ToLower().Contains("trench"))
            {
                int octaves = 5;
                double scale = 20d;
                double lacunarity = 2d;
                double persistance = 0.5d;

                int radius = 16;
                int height = 1;

                MapVolume volume = Utility.TRENCH.getMapVolume();
                // A = -127, 0 | B = 0, 0 | C = 128, 0

                //volume.SetCuboid(-256, -192, 1, 255, 191, 2, 1);
                //volume.SetCuboid(-192, -128, 1, 191, 127, 5, 3);
                volume.SetCuboidNoise(-192, -128, 1, 191, 127, 15, 3, octaves, scale, lacunarity, persistance);

                // Spawn to Spawn
                volume.SetCuboid(-3, -256, 1, 2, 255, 25, 0);

                // Thru flags
                //volume.SetCuboid(-256, -1, 1, 255, 0, 3, 0);

                // Central ring
                volume.SetSphere(0, 0, 1, radius, 0);
                volume.SetSphere(0, 0, 1, (int)Math.Round((double)radius / 2), 3);

                // Flag A & C
                volume.SetSphere(-127, -0, height, radius, 0);
                volume.SetSphere(128, 0, height, radius, 0);

                // Spawn zones
                volume.SetSphere(0, 219, height, radius, 3);
                volume.SetSphere(0, -219, height, radius, 3);
                volume.SetSphere(0, 219, height, radius - 1, 0);
                volume.SetSphere(0, -219, height, radius - 1, 0);

                await Utility.SetVoxels(server, volume, 1, 0, immediate);
            }
        }
        else if (IsDevelopmentServer(server)) //&& server.Map.ToLower().Contains("voxel"))
        {
            MapVolume volume = Utility.TRENCH.getMapVolume();
            await Utility.PlaceMapFile(server,"./jsonMaps/Hallway.json",volume);
            await Utility.SetVoxels(server, volume, 1, 0, true);
        }
    }

    public override Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        bool dev = false;
        if (IsVoxelServer() || (IsDevelopmentServer(this.Server) && dev))
        {
            if (newState == GameState.EndingGame)
            {
                Utility.halt = true;
            }
            else if ((oldState == GameState.EndingGame) && newState != GameState.EndingGame)
            {
                Utility.halt = false;
                SpawnVoxelWorld(this.Server, true);
            }
        }
        return Task.CompletedTask;
    }
}