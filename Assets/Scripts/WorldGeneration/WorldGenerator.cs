using Unity.Mathematics;
using UnityEngine;

namespace WorldGeneration
{
    public class WorldGenerator
    {
        // world size in chunks
        public const int WORLD_SIZE_X = 15;
        public const int WORLD_SIZE_Y = 15;
        public const int WORLD_SIZE_Z = 15;
        
        public const int CHUNK_SIZE_X = 12;
        public const int CHUNK_SIZE_Y = 12;
        public const int CHUNK_SIZE_Z = 12;
        
        public static Vector3Int WorldSize = new Vector3Int(WORLD_SIZE_X * CHUNK_SIZE_X, WORLD_SIZE_Y * CHUNK_SIZE_Y,
            WORLD_SIZE_Z * CHUNK_SIZE_Z);
        
        public static byte[,,] GenerateWorld(float noiseScale)
        {
            byte[,,] ret = new byte[WORLD_SIZE_X * CHUNK_SIZE_X, WORLD_SIZE_Y * CHUNK_SIZE_Y,
                WORLD_SIZE_Z * CHUNK_SIZE_Z];
            
            for (int x = 0; x < CHUNK_SIZE_X * WORLD_SIZE_X; x++)
            {
                for (int y = 0; y < CHUNK_SIZE_Y * WORLD_SIZE_Y; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE_Z * WORLD_SIZE_Z; z++)
                    {
                        var noise = PerlinNoise.Get3DPerlinNoise(new float3(x, y, z), noiseScale);
                        ret[x, y, z] = noise >= 0 ? (byte) 1 : (byte) 0;
                    }
                }
            }

            return ret;
        }
    }
}