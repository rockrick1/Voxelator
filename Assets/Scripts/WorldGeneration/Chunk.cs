using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace WorldGeneration
{
    public class Chunk : MonoBehaviour
    {
        static readonly Vector3[] _vertPos =
        {
            new Vector3(-1, 1, -1), new Vector3(-1, 1, 1),
            new Vector3(1, 1, 1), new Vector3(1, 1, -1),
            new Vector3(-1, -1, -1), new Vector3(-1, -1, 1),
            new Vector3(1, -1, 1), new Vector3(1, -1, -1),
        };

        static readonly int[,] _faces =
        {
            {0, 1, 2, 3, 0, 1, 0, 0, 0}, //top
            {7, 6, 5, 4, 0, -1, 0, 1, 0}, //bottom
            {2, 1, 5, 6, 0, 0, 1, 1, 1}, //right
            {0, 3, 7, 4, 0, 0, -1, 1, 1}, //left
            {3, 2, 6, 7, 1, 0, 0, 1, 1}, //front
            {1, 0, 4, 5, -1, 0, 0, 1, 1} //back
        };

        [SerializeField] MeshFilter _meshFilter;
        [SerializeField] MeshCollider _meshCollider;

        public Vector3Int IndexInWorld { get; private set; }
        
        byte[,,] _voxels;
        Vector3Int _dimentions;
        float _noiseScale;

        public void Init(byte[,,] voxels, Vector3Int indexInWorld)
        {
            _dimentions = new Vector3Int(WorldGenerator.CHUNK_SIZE_X, WorldGenerator.CHUNK_SIZE_Y, WorldGenerator.CHUNK_SIZE_Z);
            IndexInWorld = indexInWorld;
            _voxels = voxels;
            
            GenerateMesh();
        }

        void GenerateMesh()
        {
            List<int> triangles = new List<int>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();

            for (int x = 0; x < _dimentions.x; x++)
            {
                for (int y = 0; y < _dimentions.y; y++)
                {
                    for (int z = 0; z < _dimentions.z; z++)
                    {
                        if (_voxels[x, y, z] == 0) continue;

                        for (int facenum = 0; facenum < 6; facenum++)
                        {
                            if (!ShouldAddQuad(facenum, x, y, z)) continue;
                            AddQuad(facenum, vertices.Count);
                        }

                        void AddQuad(int facenum, int v)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                vertices.Add(new Vector3(x, y, z) + _vertPos[_faces[facenum, i]] / 2f);
                            }

                            triangles.AddRange(new List<int>() {v, v + 1, v + 2, v, v + 2, v + 3});

                            Vector2 bottomleft = GetTextureBottomLeft(facenum, x, y, z);
                            uv.AddRange(new List<Vector2>()
                            {
                                bottomleft + new Vector2(0, 0.5f), bottomleft + new Vector2(0.5f, 0.5f),
                                bottomleft + new Vector2(0.5f, 0), bottomleft
                            });
                        }
                    }
                }
            }

            _meshFilter.mesh = new Mesh()
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray(),
                uv = uv.ToArray()
            };
            _meshCollider.sharedMesh = _meshFilter.mesh;
        }
        
        /// <summary>
        /// A quad will be added only if the neighbouring voxel to the given face is empty
        /// </summary>
        bool ShouldAddQuad(int facenum, int x, int y, int z)
        {
            Vector3Int relativePos = new Vector3Int(_faces[facenum, 4], _faces[facenum, 5], _faces[facenum, 6]);
            bool isBorder = x + relativePos.x >= _dimentions.x || x + relativePos.x < 0 ||
                            y + relativePos.y >= _dimentions.y || y + relativePos.y < 0 ||
                            z + relativePos.z >= _dimentions.z || z + relativePos.z < 0;

            if (!isBorder) return _voxels[x + relativePos.x, y + relativePos.y, z + relativePos.z] == 0;
            
            // index of the other block in the entire world lalalalalalalalalala
            Vector3Int indexInWorld = GetIndexInWorld(x, y, z) + relativePos;
            if (indexInWorld.x < 0 || indexInWorld.x >= WorldGenerator.WorldSize.x ||
                indexInWorld.y < 0 || indexInWorld.y >= WorldGenerator.WorldSize.y ||
                indexInWorld.z < 0 || indexInWorld.z >= WorldGenerator.WorldSize.z)
            {
                return true;
            }
            
            return ChunkLoader.Instance.World[indexInWorld.x, indexInWorld.y, indexInWorld.z] == 0;
        }

        Vector3Int GetIndexInWorld(int x, int y, int z)
        {
            return new Vector3Int(x, y, z) + (IndexInWorld * _dimentions);
        }

        /// <summary>
        /// Returns the index of the texture used in the atlas
        /// </summary>
        Vector2 GetTextureBottomLeft(int facenum, int x, int y, int z)
        {
            // if there is a block above, use same as bottom face
            if (y < _dimentions.y - 1 && _voxels[x, y + 1, z] == 1)
            {
                return new Vector2(_faces[1, 7], _faces[1, 8]) / 2f;
            }

            return new Vector2(_faces[facenum, 7], _faces[facenum, 8]) / 2f;
        }
    }
}