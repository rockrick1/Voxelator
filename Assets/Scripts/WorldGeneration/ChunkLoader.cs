using System.Collections.Generic;
using UnityEngine;

namespace WorldGeneration
{
    public class ChunkLoader : MonoBehaviour
    {
        [SerializeField] GameObject _chunkPrefab;
        [SerializeField] float _noiseScale;

        public static ChunkLoader Instance { get; private set; }

        public byte[,,] World => _world;

        Dictionary<int, Dictionary<int, Dictionary<int, Chunk>>> _chunks;
        byte[,,] _world;

        void Awake()
        {
            if (Instance == null) Instance = this;
            GenerateWorld();
            InitializeChunks();
        }
        
        void GenerateWorld()
        {
            _world = WorldGenerator.GenerateWorld(_noiseScale);
        }

        void InitializeChunks()
        {
            _chunks = new Dictionary<int, Dictionary<int, Dictionary<int, Chunk>>>();
            for (int x = 0; x < WorldGenerator.WORLD_SIZE_X; x++)
            {
                _chunks.Add(x, new Dictionary<int, Dictionary<int, Chunk>>());
                for (int y = 0; y < WorldGenerator.WORLD_SIZE_Y; y++)
                {
                    _chunks[x].Add(y, new Dictionary<int, Chunk>());
                    for (int z = 0; z < WorldGenerator.WORLD_SIZE_Z; z++)
                    {
                        GameObject obj = Instantiate(_chunkPrefab, transform);
                        Vector3Int indexInWorld = new Vector3Int(x, y, z);
                        obj.transform.position = new Vector3Int(x * WorldGenerator.CHUNK_SIZE_X,
                            y * WorldGenerator.CHUNK_SIZE_Y, z * WorldGenerator.CHUNK_SIZE_Z);
                        Chunk c = obj.GetComponent<Chunk>();
                        c.Init(GetChunkVoxels(x, y, z), indexInWorld);
                        _chunks[x][y].Add(z, c);
                    }
                }
            }
        }

        //TODO optimize this
        byte[,,] GetChunkVoxels(int x, int y, int z)
        {
            byte[,,] ret = new byte[WorldGenerator.CHUNK_SIZE_X,WorldGenerator.CHUNK_SIZE_Y,WorldGenerator.CHUNK_SIZE_Z];
            int ii = 0;
            for (int i = WorldGenerator.CHUNK_SIZE_X * x; i < WorldGenerator.CHUNK_SIZE_X * (x + 1); i++)
            {
                int jj = 0;
                for (int j = WorldGenerator.CHUNK_SIZE_Y * y; j < WorldGenerator.CHUNK_SIZE_Y * (y + 1); j++)
                {
                    int kk = 0;
                    for (int k = WorldGenerator.CHUNK_SIZE_Z * z; k < WorldGenerator.CHUNK_SIZE_Z * (z + 1); k++)
                    {
                        ret[ii, jj, kk] = _world[i, j, k];
                        kk++;
                    }
                    jj++;
                }
                ii++;
            }

            return ret;
        }
    }

}