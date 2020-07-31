﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Tilemaps;
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
            for (int i = 0; i < LayerCount; i++)
            {
                TerrainLayer currLayer = DemoTerrain.terrainData.terrainLayers[i];
                TileGeneratorMat.SetTexture("_Diffuse" + i, currLayer.diffuseTexture);
                TileGeneratorMat.SetTexture("_Normal" + i, currLayer.normalMapTexture);
            }

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
            /**

            mRenderTextureMaterials[terrainIndex].SetVector("_Tile_ST", tileST);
            
            RenderBuffer[] colorBuffers = new RenderBuffer[2];
            RenderBuffer depthBuffer = textureArrayArray[mipLevel].depthBuffer;
            colorBuffers[0] = textureArrayArray[mipLevel].colorBuffer;
            colorBuffers[1] = normalArrayArray[mipLevel].colorBuffer;
            RenderTargetSetup rts = new RenderTargetSetup(colorBuffers, depthBuffer, 0, CubemapFace.Unknown);
            rts.depthSlice = tileNode.Value.tileIndex;
            Graphics.SetRenderTarget(rts);//DEBUG

            //We are using command buffer here to bypass the problem that the regular DrawMeshInstanced must 
            //be used with a camera and SetRenderTarget doesn't affect it.
            //We are not using command buffer for performance, because creating command buffer every frame 
            //will not make the rendering faster. However we need to change the command every frame, since
            //the matrix list is not guairanteed to remain same every time. 
            //So we are creating command buffer just to use CommandBuffer.DrawMeshInstanced.
            CommandBuffer tempCB = new CommandBuffer();
            tempCB.ClearRenderTarget(true, true, Color.black, 1);
            tempCB.DrawMesh(mQuad, Matrix4x4.identity, mRenderTextureMaterials[terrainIndex]);

            if (pageMipLevelTable[mipLevel].chunks[chunkIndex].decalMatrixList.Count > 0)
            {
                //Set Graphics Instance Variants to Keep All if this draw call doesn't work
                tempCB.DrawMeshInstanced(mQuad, 0, mDecalMaterial, 0, pageMipLevelTable[mipLevel].chunks[chunkIndex].decalMatrixList.ToArray());
            }

            Graphics.ExecuteCommandBuffer(tempCB);/

             **/
            /**
            int meshCount = 0;
            while(meshCount < 3 && TilesToGenerate.Count > 0)
            {
                int quadKey 



                meshCount++;
            }
            **/
            
            if(TilesToGenerate.Count == 0)
            {
                return;
            }

            List<int> TilesForMesh = new List<int>();

            // 优先处理mipmap等级高的tile
            TilesToGenerate.Sort((x,y) => { return MortonUtility.getMip(x).CompareTo(MortonUtility.getMip(y)); });

            int quadKey = TilesToGenerate[TilesToGenerate.Count - 1];
            //print("creating " + MortonUtility.getMip(quadKey));
            TilesToGenerate.RemoveAt(TilesToGenerate.Count - 1);

            //TODO 做成一次贴三个的
            TilesForMesh.Add(quadKey);
            SetUpMesh(TilesForMesh);

            RenderBuffer[] colorBuffers = new RenderBuffer[1];
            colorBuffers[0] = physicalTexture.PhysicalTextures[0].colorBuffer;
            RenderBuffer depthBuffer = physicalTexture.PhysicalTextures[0].depthBuffer;
            Graphics.SetRenderTarget(colorBuffers, depthBuffer);
            CommandBuffer tempCB = new CommandBuffer();
            tempCB.DrawMesh(mQuads, Matrix4x4.identity, TileGeneratorMat);
            Graphics.ExecuteCommandBuffer(tempCB);

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

                //TODO????? padding 采图？？
                quadUVList.Add(new Vector2((float)pageXY.x * rectBaseLength, (float)pageXY.y * rectBaseLength));
                quadUVList.Add(new Vector2((float)pageXY.x * rectBaseLength, (float)pageXY.y * rectBaseLength + currMipRectLength));
                quadUVList.Add(new Vector2((float)pageXY.x * rectBaseLength + currMipRectLength, (float)pageXY.y * rectBaseLength + currMipRectLength));
                quadUVList.Add(new Vector2((float)pageXY.x * rectBaseLength, (float)pageXY.y * rectBaseLength + currMipRectLength));


                float Width = (float)physicalTexture.PhysicalTextureSize.x;
                float Height = (float)physicalTexture.PhysicalTextureSize.y;

                float physicalWidth = (float)physicalTexture.PhysicalTextures[0].width;
                float physicalHeight = (float)physicalTexture.PhysicalTextures[0].height;

                Vector2Int tile = RequestTile();
                SetActive(tile);


                /**
                quadVertexList.Add(new Vector3(tile.x * 2 / Width - 1, tile.y * 2 / Height, 0.1f));
                quadVertexList.Add(new Vector3((tile.x + 1) * 2 / Width - 1, tile.y * 2 / Height, 0.1f));
                quadVertexList.Add(new Vector3((tile.x + 1) * 2 / Width - 1, (tile.y + 1) * 2 / Height, 0.1f));
                quadVertexList.Add(new Vector3(tile.x * 2 / Width - 1, (tile.y + 1) * 2 / Height, 0.1f));
                **/
                
                quadVertexList.Add(new Vector3((tile.x * 2.0f / Width - 1.0f) * physicalWidth, (tile.y * 2.0f / Height) * physicalHeight, 0.1f));
                quadVertexList.Add(new Vector3(((tile.x + 1.0f) * 2.0f / Width - 1.0f) * physicalWidth, (tile.y * 2.0f / Height) * physicalHeight, 0.1f));

                quadVertexList.Add(new Vector3(((tile.x + 1.0f) * 2.0f / Width - 1.0f) * physicalWidth, ((tile.y + 1.0f) * 2.0f / Height) * physicalHeight, 0.1f));
                quadVertexList.Add(new Vector3((tile.x * 2.0f / Width - 1.0f) * physicalWidth, ((tile.y + 1.0f) * 2.0f / Height) * physicalHeight, 0.1f));
                


                quadTriangleList.Add(4 * i);
                quadTriangleList.Add(4 * i + 1);
                quadTriangleList.Add(4 * i + 2);

                quadTriangleList.Add(4 * i + 2);
                quadTriangleList.Add(4 * i + 3);
                quadTriangleList.Add(4 * i);

                //Mapping Processsssss
                pageTable.AddressMapping[quadKey].TileIndex = tile;

                int oldQuad;
                if(physicalTexture.TileToQuadMapping.TryGetValue(tile,out oldQuad))
                {
                    pageTable.AddressMapping.Remove(oldQuad);
                }
                physicalTexture.TileToQuadMapping[tile] = quadKey;

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
