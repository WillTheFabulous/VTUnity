﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Dynamic;

namespace VirtualTexture
{
    public class TileGeneration : MonoBehaviour
    {
        public event Action<List<int>> OnTileGenerationComplete;
        // Start is called before the first frame update
        [SerializeField]
        private Shader TileGenerator = default;

        private List<int> TilesToGenerate = default;

        private List<int> GeneratingTiles = default;

        private Material TileGeneratorMat = default;

        [SerializeField]
        private Terrain DemoTerrain = default;

        [SerializeField]
        private int LayerCount = default;

        private PhysicalTexture physicalTexture = default;

        private PageTable pageTable = default;

        private Mesh mQuads;

        private LruCache m_TilePool = new LruCache();



        void Start()
        {
            TilesToGenerate = new List<int>();
            TileGeneratorMat = new Material(TileGenerator);
            LayerCount = DemoTerrain.terrainData.terrainLayers.Length;

            physicalTexture = (PhysicalTexture)GetComponent(typeof(PhysicalTexture));
            pageTable = (PageTable)GetComponent(typeof(PageTable));

            for (int i = 0; i < physicalTexture.PhysicalTextureSize.x * physicalTexture.PhysicalTextureSize.y; i++)
            {
                m_TilePool.Add(i);
            }

                //TODO 更好的texture存储方式？？
            TileGeneratorMat.SetTexture("_AlphaMap", DemoTerrain.terrainData.alphamapTextures[0]);

            Vector4[] tileInfo = new Vector4[4];
            
            for (int i = 0; i < LayerCount && i < 4; i++)
            {
                TerrainLayer currLayer = DemoTerrain.terrainData.terrainLayers[i];

                currLayer.diffuseTexture.wrapMode = TextureWrapMode.Repeat;
                currLayer.normalMapTexture.wrapMode = TextureWrapMode.Repeat;
                currLayer.diffuseTexture.filterMode = FilterMode.Bilinear;
                currLayer.normalMapTexture.filterMode = FilterMode.Bilinear;
                TileGeneratorMat.SetTexture("_Diffuse" + i, currLayer.diffuseTexture);
                TileGeneratorMat.SetTexture("_Normal" + i, currLayer.normalMapTexture);
                tileInfo[i].x = currLayer.tileSize.x;
                tileInfo[i].y = currLayer.tileSize.y;
                tileInfo[i].w = currLayer.tileOffset.x;
                tileInfo[i].z = currLayer.tileOffset.y;
            }


            TileGeneratorMat.SetVector("_TerrainSize", DemoTerrain.terrainData.size);
            TileGeneratorMat.SetVectorArray("_TileInfo", tileInfo);

        }

        public void GeneratePageTask(int quadKey)
        {

            if (!TilesToGenerate.Contains(quadKey))
            {
                TilesToGenerate.Add(quadKey);
            }

        }

        public void GeneratePage()
        {
            
            if(TilesToGenerate.Count == 0)
            {
                return;
            }
            //print("generating page" + Time.frameCount + "  " + TilesToGenerate.Count);

            List<int> TilesForMesh = new List<int>();

            // 优先处理mipmap等级高的tile
            TilesToGenerate.Sort((x,y) => { return MortonUtility.getMip(x).CompareTo(MortonUtility.getMip(y)); });

            //一次最多贴5个
            for (int i = 0; i < 5 && TilesToGenerate.Count > 0; i++)
            {
                int quadKey = TilesToGenerate[TilesToGenerate.Count - 1];
                TilesToGenerate.RemoveAt(TilesToGenerate.Count - 1);
                TilesForMesh.Add(quadKey);
            }

            SetUpMesh(TilesForMesh);

            RenderBuffer[] colorBuffers = new RenderBuffer[1];

            for (int textureType = 0; textureType < physicalTexture.NumTextureType; textureType++)
            {
                colorBuffers[0] = physicalTexture.PhysicalTextures[textureType].colorBuffer;
                RenderBuffer depthBuffer = physicalTexture.PhysicalTextures[textureType].depthBuffer;
                for (int i = 0; i < 4; i++)
                {
                    RenderTargetSetup rtsMip = new RenderTargetSetup(colorBuffers, depthBuffer, i, CubemapFace.Unknown);
                    Graphics.SetRenderTarget(rtsMip);
                    CommandBuffer tempCB = new CommandBuffer();
                    tempCB.DrawMesh(mQuads, Matrix4x4.identity, TileGeneratorMat, 0, textureType);
                    Graphics.ExecuteCommandBuffer(tempCB);
                }
            }



            OnTileGenerationComplete?.Invoke(TilesForMesh);


        }

        private void SetUpMesh(List<int> quadKeys)
        {
            List<Vector3> quadVertexList = new List<Vector3>();
            List<int> quadTriangleList = new List<int>();
            List<Vector2> quadUVList = new List<Vector2>();
            int tableSize = pageTable.TableSize;
            //有可能出现的最高mip
            int maxMip = (int)Math.Log(tableSize, 2);
            mQuads = new Mesh();
            
            for(int i = 0; i < quadKeys.Count; i++)
            {
                int quadKey = quadKeys[i];
                int mip = MortonUtility.getMip(quadKey);
                Vector2Int pageXY = MortonUtility.getPageXY(quadKey);
                //最高mip到当前mip的偏移
                int mipBias = maxMip - mip;
                float rectBaseLength = 1.0f / (float)tableSize;
                float currMipRectLength = 1.0f / (float)Math.Pow(2.0, mipBias);
                float paddingLength = currMipRectLength * ((float)physicalTexture.PaddingSize / (float)physicalTexture.TileSize);

               
                Vector2 uv0 = new Vector2((float)pageXY.x * rectBaseLength - paddingLength, (float)pageXY.y * rectBaseLength - paddingLength);
                Vector2 uv1 = new Vector2((float)pageXY.x * rectBaseLength + currMipRectLength + paddingLength, (float)pageXY.y * rectBaseLength - paddingLength);
                Vector2 uv2 = new Vector2((float)pageXY.x * rectBaseLength + currMipRectLength + paddingLength, (float)pageXY.y * rectBaseLength + currMipRectLength + paddingLength);
                Vector2 uv3 = new Vector2((float)pageXY.x * rectBaseLength - paddingLength, (float)pageXY.y * rectBaseLength + currMipRectLength + paddingLength);

                quadUVList.Add(uv0);
                quadUVList.Add(uv1);
                quadUVList.Add(uv2);
                quadUVList.Add(uv3);



                float Width = (float)physicalTexture.PhysicalTextureSize.x;
                float Height = (float)physicalTexture.PhysicalTextureSize.y;

                Vector2Int tile = RequestTile();
                SetActive(tile);
                //TODO 加tiling offset

                Vector3 vertex0;
                Vector3 vertex1;
                Vector3 vertex2;
                Vector3 vertex3;

                if (SystemInfo.graphicsDeviceType.ToString().Contains("Direct3D"))
                {
                    vertex0 = new Vector3(tile.x * 2 / Width - 1, (Height - tile.y) * 2 / Height - 1, 0.1f);
                    vertex1 = new Vector3((tile.x + 1) * 2 / Width - 1, (Height - tile.y) * 2 / Height - 1, 0.1f);
                    vertex2 = new Vector3((tile.x + 1) * 2 / Width - 1, (Height - tile.y - 1) * 2 / Height - 1, 0.1f);
                    vertex3 = new Vector3(tile.x * 2 / Width - 1, (Height - tile.y - 1) * 2 / Height - 1, 0.1f);
                }
                else
                {
                    vertex0 = new Vector3(tile.x * 2 / Width - 1, tile.y * 2 / Height - 1, 0.1f);
                    vertex1 = new Vector3((tile.x + 1) * 2 / Width - 1, tile.y * 2 / Height - 1, 0.1f);
                    vertex2 = new Vector3((tile.x + 1) * 2 / Width - 1, (tile.y + 1) * 2 / Height - 1, 0.1f);
                    vertex3 = new Vector3(tile.x * 2 / Width - 1, (tile.y + 1) * 2 / Height - 1, 0.1f);
                }

               



                /*Vector3 vertex0 = new Vector3(0.3f, 0.3f, 0.1f);
                Vector3 vertex1 = new Vector3(0.4f, 0.3f, 0.1f);
                Vector3 vertex2 = new Vector3(0.4f, 0.2f, 0.1f);
                Vector3 vertex3 = new Vector3(0.3f, 0.2f, 0.1f);*/


                quadVertexList.Add(vertex0);
                quadVertexList.Add(vertex1);
                quadVertexList.Add(vertex2);
                quadVertexList.Add(vertex3);
           
                quadTriangleList.Add(4 * i);
                quadTriangleList.Add(4 * i + 1);
                quadTriangleList.Add(4 * i + 2);

                quadTriangleList.Add(4 * i + 2);
                quadTriangleList.Add(4 * i + 3);
                quadTriangleList.Add(4 * i);

                //Mapping Processsssss
                // morton version
                /*pageTable.AddressMapping[quadKey].TileIndex = tile;

                int oldQuad;
                if(physicalTexture.TileToQuadMapping.TryGetValue(tile,out oldQuad))
                {
                    pageTable.AddressMapping.Remove(oldQuad);
                }*/

                //Pointer version

                
                int oldQuad;
                if (physicalTexture.TileToQuadMapping.TryGetValue(tile, out oldQuad))
                {
                    pageTable.m_Pages[oldQuad].Payload.tileStatus = TileStatus.Uninitialized;
                    pageTable.m_Pages.Remove(oldQuad);
                }
                physicalTexture.TileToQuadMapping[tile] = quadKey;

                pageTable.m_Pages[quadKey].Payload.TileIndex = tile;

            }

            mQuads.SetVertices(quadVertexList);
            mQuads.SetUVs(0, quadUVList);
            mQuads.SetTriangles(quadTriangleList, 0);

        }

        public bool SetActive(Vector2Int tile)
        {
            bool success = m_TilePool.SetActive(PosToId(tile));

            return success;
        }


        public Vector2Int RequestTile()
        {
            return IdToPos(m_TilePool.First);
        }


        private Vector2Int IdToPos(int id)
        {
            return new Vector2Int(id % physicalTexture.PhysicalTextureSize.x, id / physicalTexture.PhysicalTextureSize.x);
        }

        private int PosToId(Vector2Int tile)
        {
            return (tile.y * physicalTexture.PhysicalTextureSize.x + tile.x);
        }

        // Update is called once per frame
        void Update()
        {
            GeneratePage();
        }
    }
}
