using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using World;

namespace WorldGeneration
{
    public class ChunkLoader : MonoBehaviour
    {
        [SerializeField] GameObject _chunkPrefab;
        [SerializeField] float _noiseScale;
        [SerializeField] float _drawDistance;

        public static ChunkLoader Instance { get; set; }
        public byte[,,] World => _world;

        Queue<UniTask> _chunkLoadQueue;

        Dictionary<int, Dictionary<int, Dictionary<int, Chunk>>> _chunks;
        byte[,,] _world;
        Vector3 _playerPosition => WorldController.Instance.PlayerController.transform.position;
        float _buildDistanceDiff => _drawDistance / 4;

        void Awake()
        {
            _chunkLoadQueue = new Queue<UniTask>();
            if (Instance == null) Instance = this;
            GenerateWorld();
            InstantiateChunks();
        }
        
        void GenerateWorld()
        {
            _world = WorldGenerator.GenerateWorld(_noiseScale);
        }

        void InstantiateChunks()
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

        void Update()
        {
            if (_chunkLoadQueue.Count > 0)
            {
                var task = _chunkLoadQueue.Dequeue();
                //TODO CANCELLATION TOKEN I DONT KNOW MAN PLS HELP ME
                task.Forget();
            }
            
            foreach (var pairX in _chunks)
            {
                foreach (var pairY in _chunks[pairX.Key])
                {
                    foreach (var pairZ in _chunks[pairX.Key][pairY.Key])
                    {
                        var chunk = _chunks[pairX.Key][pairY.Key][pairZ.Key];
                        var distanceToPlayer = Vector3.Distance(_playerPosition, chunk.transform.position);
                        if (distanceToPlayer > _drawDistance && chunk.IsBuilt)
                        {
                            chunk.Free();
                        }
                        else if (distanceToPlayer < _drawDistance - _buildDistanceDiff && !chunk.IsBuilt &&
                                 !_chunkLoadQueue.Contains(chunk.Build()))
                        {
                            _chunkLoadQueue.Enqueue(chunk.Build());
                        }
                    }
                }
            }
        }
    }

}