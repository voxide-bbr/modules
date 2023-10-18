using BattleBitAPI;
using BattleBitAPI.Common;
using BBRAPIModules;

using System;
using static System.Random;
using System.Reflection;
using System.Text.Json;
using System.Numerics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

using Voxide;
using static Voxide.Library;
using static Voxide.OpenSimplexNoise;
using Microsoft.VisualBasic;

namespace Voxide;
[RequireModule(typeof(Library))]
[RequireModule(typeof(OpenSimplexNoise))]
[Module("Voxel", "1.0.0")]
public class Voxel : BattleBitModule
{
    public static class Statics
    {
        public static bool First = true;
        public static int VoxelCounter = 0;
        //                                         volume.SetCuboid(-256, -179, 1, 255, 179, 4, 1);
        public static MapBoundaries MapBoundariesFortify = new MapBoundaries(256 * 2, 179 * 2 + 1, 100, -256, -179, 1);
        // Spawn zones +1 block border: US = -35, -256 -> 34, -179 | RU = -35, 178 -> 34, 255
        // Flags: A = -127, 0 | B = 0, 0 | C = 128, 0
        public static MapBoundaries MapBoundariesTrench = new MapBoundaries(256 * 2, 256 * 2, 100, -256, -256, 1);
        public static MapVolume MapVolumeTrenchProduction = MapBoundariesTrench.getMapVolume();
        public static MapVolume MapVolumeTrenchDevelopment = MapBoundariesTrench.getMapVolume();
        public static MapVolume MapVolumeFortifyProduction = MapBoundariesFortify.getMapVolume();
        public static void Update(RunnerServer? server=null)
        {
            int _threshold = 3;
            bool refresh = VoxelCounter >= _threshold;
            VoxelCounter = 0;
            if (First || refresh) // MapVolumeTrenchProduction
            {
                MapVolume v = MapBoundariesTrench.getMapVolume();
                int octaves = 5;
                double scale = 100d; // x / scale
                double lacunarity = 2d; // frequency ~> (x/scale)*frequency
                double persistance = 1.0d; // amplitude ~> ((x/scale)*frequency)*amplitude

                int mapHeight = 24;
                int flagRadius = 24;

                // A = -127, 0 | B = 0, 0 | C = 128, 0
                v.SetCuboidNoise(-160, -160, 1, 159, 159, mapHeight, 3, octaves, scale, lacunarity, persistance);

                // Spawn to Spawn
                //v.SetCuboid(-5, -256, 1, 4, -10, mapHeight*2, 0);
                //v.SetCuboid(-5, 10, 1, 4, 255, mapHeight*2, 0);

                // Thru flags
                //v.SetCuboid(-256, -1, 1, 255, 0, 3, 0);

                // Central ring
                v.SetOutsideSphere(0, 0, 1, flagRadius, 5, 0, 1, mapHeight, false, true);

                // Flag A & C
                v.SetOutsideSphere(-127, 0, 1, flagRadius, 5, 0, 1, mapHeight, false, true);
                v.SetOutsideSphere(128, 0, 1, flagRadius, 5, 0, 1, mapHeight, false, true);
                
                // Trim map into a circle
                v.SetOutsideSphere(0, 0, 0, 162, 160, 0, 1, mapHeight, true, true);
                
                // Keep tree away from flags
                List<Vector2> trees = new()
                {
                    new Vector2(-128, 0), new Vector2(0, 0), new Vector2(128, 0)
                };
                v.SetCuboidTrees(-128, -128, 2, 127, 127, mapHeight, 3, 3, 0.5d, 2.0d, 0.5d, 0.773d, 15, trees);

                // Spawn zones +1 block border: US = -35, -256 -> 34, -179 | RU = -35, 178 -> 34, 255
                // US Spawn outer walls
                v.SetCuboid(-35-10, -256-10, 1, 34+10, -179+10, mapHeight, 3, false, true);
                // US Spawn floors
                v.SetCuboid(-35-10, -256-10, mapHeight/2, 34+10, -179+10, mapHeight/2, 3);
                //v.SetCuboid(-35-10, -256-10, mapHeight, 34+10, -179+10, mapHeight, 3);
                // US Spawn inner clearing
                v.SetCuboid(-35-5, -256-5, 1, 34+5, -179+5, mapHeight/2, 0);
                // US Spawn inner walls
                v.SetCuboid(-35, -256-1, 1, 34, -179, 1, 3, false, true);
                // Main hall
                //v.SetCuboid(-5, -179, 1, 4, -179+10, 10, 0);
                v.SetCuboid(-5+1, -179+1, 1, 4-1, -179+10+1, (mapHeight/2)-1, 0);
                
                // RU Spawn outer walls
                v.SetCuboid(-35-10, 178-10, 1, 34+10, 255+10, mapHeight, 3, false, true);
                // RU Spawn floors
                v.SetCuboid(-35-10, 178-10, mapHeight/2, 34+10, 255+10, mapHeight/2, 3);
                //v.SetCuboid(-35-10, 178-10, mapHeight, 34+10, 255+10, mapHeight, 3);
                // RU Spawn inner clearing
                v.SetCuboid(-35-5, 178-5, 1, 34+5, 255+5, mapHeight, 0);
                // RU Spawn inner walls
                v.SetCuboid(-35, 178, 1, 34, 255+1, 1, 3, false, true);
                // Main hall
                //v.SetCuboid(-5, 178-10, 1, 4, 178, 10, 0);
                v.SetCuboid(-5+1, 178-10-1, 1, 4-1, 178-1, (mapHeight/2)-1, 0);
                MapVolumeTrenchProduction = v;
            }
            if (First || refresh) // MapVolumeFortifyProduction
            {
                MapVolume v = MapBoundariesFortify.getMapVolume();
                int flagRadius = 25;
                int flagHeight = 50;
                // Add
                v.SetSphere(-127, -95, flagHeight, flagRadius, 2);
                v.SetSphere(0, -95, flagHeight, flagRadius, 2);
                v.SetSphere(128, -95, flagHeight, flagRadius, 2);
                v.SetSphere(-127, 96, flagHeight, flagRadius, 2);
                v.SetSphere(0, 96, flagHeight, flagRadius, 2);
                v.SetSphere(128, 96, flagHeight, flagRadius, 2);
                // Hollow out
                v.SetSphere(-127, -95, flagHeight, flagRadius - 2, 0);
                v.SetSphere(0, -95, flagHeight, flagRadius - 2, 0);
                v.SetSphere(128, -95, flagHeight, flagRadius - 2, 0);
                v.SetSphere(-127, 96, flagHeight, flagRadius - 2, 0);
                v.SetSphere(0, 96, flagHeight, flagRadius - 2, 0);
                v.SetSphere(128, 96, flagHeight, flagRadius - 2, 0);
                // Cut most of sphere except bottom bowl
                v.SetCuboid(-256, -179, 36, 255, 179, 100, 0);
                MapVolumeFortifyProduction = v;
            }
            if (server != null && IsDevelopmentServer(server)) // MapVolumeTrenchDevelopment
            {
                MapVolume v = MapBoundariesTrench.getMapVolume();
                /*int octaves = 3;
                double scale = 0.5d;
                double lacunarity = 2.0d;
                double persistance = 0.5d;
                double threshold = 0.775;
                v.SetCuboidNoise(-128, -128, 1, 127, 127, 25, 3,
                    5, 100d, lacunarity*2, persistance/2);
                v.SetCuboidTrees(-128, -128, 1, 127, 127, 25, 3,
                    octaves, scale, lacunarity, persistance, threshold);
                v.SetOutsideSphere(0, 0, 0, 164, 128, 0, 1, 100, true, true);*/
                
                int octaves = 5;
                double scale = 100d; // x / scale
                double lacunarity = 2d; // frequency ~> (x/scale)*frequency
                double persistance = 1.0d; // amplitude ~> ((x/scale)*frequency)*amplitude

                int mapHeight = 24;
                int flagRadius = 24;

                // A = -127, 0 | B = 0, 0 | C = 128, 0
                v.SetCuboidNoise(-160, -160, 1, 159, 159, mapHeight, 3, octaves, scale, lacunarity, persistance);

                // Spawn to Spawn
                //v.SetCuboid(-5, -256, 1, 4, -10, mapHeight*2, 0);
                //v.SetCuboid(-5, 10, 1, 4, 255, mapHeight*2, 0);

                // Thru flags
                //v.SetCuboid(-256, -1, 1, 255, 0, 3, 0);

                // Central ring
                v.SetOutsideSphere(0, 0, 1, flagRadius, 5, 0, 1, mapHeight, false, true);

                // Flag A & C
                v.SetOutsideSphere(-127, 0, 1, flagRadius, 5, 0, 1, mapHeight, false, true);
                v.SetOutsideSphere(128, 0, 1, flagRadius, 5, 0, 1, mapHeight, false, true);
                
                // Trim map into a circle
                v.SetOutsideSphere(0, 0, 0, 162, 160, 0, 1, mapHeight, true, true);
                
                // Keep tree away from flags
                List<Vector2> trees = new()
                {
                    new Vector2(-128, 0), new Vector2(0, 0), new Vector2(128, 0)
                };
                v.SetCuboidTrees(-128, -128, 2, 127, 127, mapHeight, 3, 3, 0.5d, 2.0d, 0.5d, 0.773d, 15, trees);

                // Spawn zones +1 block border: US = -35, -256 -> 34, -179 | RU = -35, 178 -> 34, 255
                // US Spawn outer walls
                v.SetCuboid(-35-10, -256-10, 1, 34+10, -179+10, mapHeight, 3, false, true);
                // US Spawn floors
                v.SetCuboid(-35-10, -256-10, mapHeight/2, 34+10, -179+10, mapHeight/2, 3);
                //v.SetCuboid(-35-10, -256-10, mapHeight, 34+10, -179+10, mapHeight, 3);
                // US Spawn inner clearing
                v.SetCuboid(-35-5, -256-5, 1, 34+5, -179+5, mapHeight/2, 0);
                // US Spawn inner walls
                v.SetCuboid(-35, -256-1, 1, 34, -179, 1, 3, false, true);
                // Main hall
                //v.SetCuboid(-5, -179, 1, 4, -179+10, 10, 0);
                v.SetCuboid(-5+1, -179+1, 1, 4-1, -179+10+1, (mapHeight/2)-1, 0);
                
                // RU Spawn outer walls
                v.SetCuboid(-35-10, 178-10, 1, 34+10, 255+10, mapHeight, 3, false, true);
                // RU Spawn floors
                v.SetCuboid(-35-10, 178-10, mapHeight/2, 34+10, 255+10, mapHeight/2, 3);
                //v.SetCuboid(-35-10, 178-10, mapHeight, 34+10, 255+10, mapHeight, 3);
                // RU Spawn inner clearing
                v.SetCuboid(-35-5, 178-5, 1, 34+5, 255+5, mapHeight, 0);
                // RU Spawn inner walls
                v.SetCuboid(-35, 178, 1, 34, 255+1, mapHeight/2, 3, false, true);
                // Main hall
                //v.SetCuboid(-5, 178-10, 1, 4, 178, 10, 0);
                v.SetCuboid(-5+1, 178-10-1, 1, 4-1, 178-1, (mapHeight/2)-1, 0);

                MapVolumeTrenchDevelopment = v;
            }
            //if (First) First = false;
        }
    }
    public override void OnModulesLoaded()
    {
        Statics.Update();
    }

    public override Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        if (IsVoxelServer() || IsDevelopmentServer(this.Server))
        {
            if (newState == GameState.EndingGame)
            {
                Utility.halt = true;
                Statics.Update(this.Server);
            }
            else if ((oldState == GameState.EndingGame) && newState != GameState.EndingGame)
            {
                Utility.halt = false;
                SpawnVoxelWorld(this.Server, true);
                Statics.VoxelCounter += 1;
            }
        }
        return Task.CompletedTask;
    }

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
            return isValidPositionMapVolume(translatePositionGameToMapVolume(game_position)) &&
                game_position.X >= -256 && game_position.X <= 255 && game_position.Y >= -256 && game_position.Y <= 255 && game_position.Z >= 1 && game_position.Z <= 100;
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
            if (isValidPositionGame(game_position)) return SetMapVolume(translatePositionGameToMapVolume(game_position), type);
            return false;
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
            if (isValidPositionGame(game_position)) return GetMapVolume(translatePositionGameToMapVolume(game_position));
            return -1;
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
        public bool SetCuboid(int x1, int y1, int z1, int x2, int y2, int z2, int type, bool hollow = false, bool wall = false, bool setOpposite=false)
        {
            bool success = true;
            if (wall) hollow = true;
            for (int x = x1; x <= x2; x++)
            {
                for (int y = y1; y <= y2; y++)
                {
                    for (int z = z1; z <= z2; z++)
                    {
                        bool outOfScope = false;
                        bool inPerimeter = (x == x1 || y == y1 || z == z1 || x == x2 || y == y2 || z == z2);
                        bool isWall = (x == x1 || x == x2 || y == y1 || y == y2);

                        if ((hollow && !inPerimeter) || (wall && !isWall)) outOfScope = true;

                        if (outOfScope) {
                            if (!setOpposite) continue;
                            if (setOpposite) {
                                if (type >= 1) type = 0;
                                if (type <= 0) type = 3;
                            }
                        }
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
        public void SetTree(int x, int y, int z, int height = 10, bool leaves = true, int radius = 2, int type = 3)
        {
            if (height < 0) return;

            // Trunk
            for (int h = 0; h < height; h++)
                SetVoxelGame(new Vector3(x, y, z + h), type);

            // Leaves
            if (leaves && radius > 1)
                SetSphere(x, y, z + height - 1, radius, type);
        }
        public bool SetCuboidTrees(int x1, int y1, int z1, int x2, int y2, int z2, int type, int octaves = 3, double scale = 10.0d, double lacunarity = 2.0d, double persistance = 0.9d, double threshold = 0.90, double mindistance = 10.0, List<Vector2>? trees=null)
        {
            OpenSimplexNoise noise = new();

            trees = trees ?? new();

            bool success = true;
            for (int x = x1; x <= x2; x++)
            {
                for (int y = y1; y <= y2; y++)
                {
                    double _noise = GetNoise(noise, x, y, octaves, scale, lacunarity, persistance);
                    if (_noise > threshold)
                    {
                        int surface = z2 + 1; // The top of the voxels from top down currently
                        for (int z = surface; z > 0; z--) // need to fix this part, the z will be offset if it doesnt start at z coord 1
                        {
                            if (GetVoxelGame(new Vector3(x, y, z)) <= 0) surface = z;
                            else break;
                        }

                        if (!(surface < z1 || surface > z2))
                        {
                            bool leaves = Utility.Random.NextInt64(1, 100) > 10;
                            int radius = (int)Utility.Random.NextInt64(3, 5);
                            int height = (int)Utility.Random.NextInt64(6, 11);

                            // Check for other trees

                            Vector2 tree = new Vector2(x,y);
                            bool clear = true;
                            foreach(Vector2 other in trees) {
                                double distance = Math.Pow(Math.Pow(other.X-tree.X,2) + Math.Pow(other.Y-tree.Y,2),.5d);
                                if (distance < mindistance) {
                                    clear = false;
                                    break;
                                }
                            }
                            if (clear) {
                                trees.Add(tree);
                                SetTree(x, y, surface, height, leaves, radius, type);
                            }
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
        public bool SetSphere(int centerX, int centerY, int centerZ, int radius, int type, bool cylindrical=false)
        {
            bool success = true;
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int y = centerY - radius; y <= centerY + radius; y++)
                {
                    for (int z = centerZ - radius; z <= centerZ + radius; z++)
                    {
                        if ((cylindrical && IsInSphere(centerX, centerY, z, radius, x, y, z)) || (!cylindrical && IsInSphere(centerX, centerY, centerZ, radius, x, y, z)))
                            if (!SetVoxelGame(new Vector3(x, y, z), type))
                                success = false;
                    }
                }
            }
            return success;
        }
        public bool SetOutsideSphere(int centerX, int centerY, int centerZ, int radiusOuter, int radiusInner, int type, int minZ = 1, int maxZ = 100, bool cuboidOutside = false, bool cylindrical=false) // Like normal, but instead of setting inside, it sets what is outside the sphere between two radius
        {
            if (minZ < 1) minZ = 1;
            if (maxZ > 100) maxZ = 100;
            bool success = true;
            for (int x = Math.Clamp(centerX - radiusOuter, -256, 255); x <= Math.Clamp(centerX + radiusOuter, -256, 255); x++)
            {
                for (int y = Math.Clamp(centerY - radiusOuter, -256, 255); y <= Math.Clamp(centerY + radiusOuter, -256, 255); y++)
                {
                    for (int z = Math.Clamp(centerZ - radiusOuter, minZ, maxZ); z <= Math.Clamp(centerZ + radiusOuter, minZ, maxZ); z++)
                    {
                        if (
                            (
                                (
                                    cuboidOutside &&
                                    IsInCuboid(centerX - radiusOuter, centerY - radiusOuter, centerZ - radiusOuter, centerX + radiusOuter, centerY + radiusOuter, centerZ + radiusOuter, x, y, z)
                                )
                                ||
                                (
                                    cylindrical &&
                                    IsInSphere(centerX, centerY, z, radiusOuter, x, y, z)
                                ) ||
                                IsInSphere(centerX, centerY, centerZ, radiusOuter, x, y, z)
                            ) &&
                            (
                                (
                                    cylindrical &&
                                    !IsInSphere(centerX, centerY, z, radiusInner, x, y, z)
                                ) ||
                                (
                                    !cylindrical &&
                                    !IsInSphere(centerX, centerY, centerZ, radiusInner, x, y, z)
                                )
                            )
                        )
                        {
                            if (!SetVoxelGame(new Vector3(x, y, z), type))
                                success = false;
                        }
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
            foreach (Vector3 point in voxelPoints)
            {
                Vector3 position = new Vector3(point.X, point.Y, point.Z + 1);
                volume.SetVoxel(position, 3);
            }
        }

        public static bool halt = false;
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
                            if (!immediate && (count_voxel % 1000) == 0)
                            {
                                //await Console.Out.WriteLineAsync($"{count_voxel} voxels altered");
                                await Task.Delay(delayMilliseconds);
                            } else if (immediate && (count_voxel % 1000 == 0)) {
                                await Task.Delay(1);
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
                await Utility.SetVoxels(server, Statics.MapVolumeFortifyProduction, 1, 0, immediate);
            else if (server.Gamemode.ToLower().Contains("trench"))
                await Utility.SetVoxels(server, Statics.MapVolumeTrenchProduction, 1, 0, immediate);
        }
        else if (IsDevelopmentServer(server)) //&& server.Map.ToLower().Contains("voxel"))
        {
            //if (server.Gamemode.ToLower().Contains("trench"))
            await Utility.SetVoxels(server, Statics.MapVolumeTrenchDevelopment, 1, 0, immediate);
        }
    }
}