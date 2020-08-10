using System;
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

        private Mesh mQuadLowMip;

        private LruCache m_TilePool = new LruCache();

        private readonly static object m_Locker = new object();


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
                currLayer.diffuseTexture.wrapMode = TextureWrapMode.Clamp;
                currLayer.normalMapTexture.wrapMode = TextureWrapMode.Clamp;
                currLayer.diffuseTexture.filterMode = FilterMode.Point;
                currLayer.normalMapTexture.filterMode = FilterMode.Point;
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
            
            if(TilesToGenerate.Count == 0)
            {
                return;
            }

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
            colorBuffers[0] = physicalTexture.PhysicalTextures[0].colorBuffer;
            RenderBuffer depthBuffer = physicalTexture.PhysicalTextures[0].depthBuffer;

            //mip 0
            RenderTargetSetup rtsMip0 = new RenderTargetSetup(colorBuffers, depthBuffer, 0, CubemapFace.Unknown);
            Graphics.SetRenderTarget(rtsMip0);
            CommandBuffer tempCB = new CommandBuffer();
            tempCB.DrawMesh(mQuads, Matrix4x4.identity, TileGeneratorMat);
            Graphics.ExecuteCommandBuffer(tempCB);


            //mip 1
            RenderTargetSetup rtsMip1 = new RenderTargetSetup(colorBuffers, depthBuffer, 1, CubemapFace.Unknown);
            Graphics.SetRenderTarget(rtsMip1);
            CommandBuffer tempCBLowMip = new CommandBuffer();
            tempCBLowMip.DrawMesh(mQuads, Matrix4x4.identity, TileGeneratorMat);
            Graphics.ExecuteCommandBuffer(tempCBLowMip);

            /*RenderTargetSetup rtsMip2 = new RenderTargetSetup(colorBuffers, depthBuffer, 2, CubemapFace.Unknown);
            Graphics.SetRenderTarget(rtsMip2);
            CommandBuffer tempCBLowMip2 = new CommandBuffer();
            tempCBLowMip.DrawMesh(mQuads, Matrix4x4.identity, TileGeneratorMat);
            Graphics.ExecuteCommandBuffer(tempCBLowMip2);

            RenderTargetSetup rtsMip3 = new RenderTargetSetup(colorBuffers, depthBuffer, 3, CubemapFace.Unknown);
            Graphics.SetRenderTarget(rtsMip3);
            CommandBuffer tempCBLowMip3 = new CommandBuffer();
            tempCBLowMip.DrawMesh(mQuads, Matrix4x4.identity, TileGeneratorMat);
            Graphics.ExecuteCommandBuffer(tempCBLowMip3);

            RenderTargetSetup rtsMip4 = new RenderTargetSetup(colorBuffers, depthBuffer, 4, CubemapFace.Unknown);
            Graphics.SetRenderTarget(rtsMip4);
            CommandBuffer tempCBLowMip4 = new CommandBuffer();
            tempCBLowMip.DrawMesh(mQuads, Matrix4x4.identity, TileGeneratorMat);
            Graphics.ExecuteCommandBuffer(tempCBLowMip4);

            RenderTargetSetup rtsMip5 = new RenderTargetSetup(colorBuffers, depthBuffer, 5, CubemapFace.Unknown);
            Graphics.SetRenderTarget(rtsMip5);
            CommandBuffer tempCBLowMip5 = new CommandBuffer();
            tempCBLowMip.DrawMesh(mQuads, Matrix4x4.identity, TileGeneratorMat);
            Graphics.ExecuteCommandBuffer(tempCBLowMip5);

            RenderTargetSetup rtsMip6 = new RenderTargetSetup(colorBuffers, depthBuffer, 6, CubemapFace.Unknown);
            Graphics.SetRenderTarget(rtsMip6);
            CommandBuffer tempCBLowMip6 = new CommandBuffer();
            tempCBLowMip.DrawMesh(mQuads, Matrix4x4.identity, TileGeneratorMat);
            Graphics.ExecuteCommandBuffer(tempCBLowMip6);

            RenderTargetSetup rtsMip7 = new RenderTargetSetup(colorBuffers, depthBuffer, 7, CubemapFace.Unknown);
            Graphics.SetRenderTarget(rtsMip4);
            CommandBuffer tempCBLowMip7 = new CommandBuffer();
            tempCBLowMip.DrawMesh(mQuads, Matrix4x4.identity, TileGeneratorMat);
            Graphics.ExecuteCommandBuffer(tempCBLowMip7);*/

            //physicalTexture.PhysicalTextures[0].GenerateMips();


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

                //print(paddingLength);
               
                Vector2 uv0 = new Vector2((float)pageXY.x * rectBaseLength - paddingLength, (float)pageXY.y * rectBaseLength - paddingLength);
                Vector2 uv1 = new Vector2((float)pageXY.x * rectBaseLength + currMipRectLength + paddingLength, (float)pageXY.y * rectBaseLength - paddingLength);
                Vector2 uv2 = new Vector2((float)pageXY.x * rectBaseLength + currMipRectLength + paddingLength, (float)pageXY.y * rectBaseLength + currMipRectLength + paddingLength);
                Vector2 uv3 = new Vector2((float)pageXY.x * rectBaseLength - paddingLength, (float)pageXY.y * rectBaseLength + currMipRectLength + paddingLength);
                //print(uv0.x);
                //print(uv0.y);

                quadUVList.Add(uv0);
                quadUVList.Add(uv1);
                quadUVList.Add(uv2);
                quadUVList.Add(uv3);

                //TODO????? padding 采图？？


                float Width = (float)physicalTexture.PhysicalTextureSize.x;
                float Height = (float)physicalTexture.PhysicalTextureSize.y;

                Vector2Int tile = RequestTile();
                SetActive(tile);     
                //TODO 加tiling
                Vector3 vertex0 = new Vector3(tile.x * 2 / Width - 1, (Height - tile.y) * 2 / Height - 1, 0.1f);
                Vector3 vertex1 = new Vector3((tile.x + 1) * 2 / Width - 1, (Height - tile.y) * 2 / Height - 1, 0.1f);
                Vector3 vertex2 = new Vector3((tile.x + 1) * 2 / Width - 1, (Height - tile.y - 1) * 2 / Height - 1, 0.1f);
                Vector3 vertex3 = new Vector3(tile.x * 2 / Width - 1, (Height - tile.y - 1) * 2 / Height - 1 , 0.1f);



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

                pageTable.m_Pages[quadKey].Payload.TileIndex = tile;
                int oldQuad;
                if (physicalTexture.TileToQuadMapping.TryGetValue(tile, out oldQuad))
                {
                    pageTable.m_Pages.Remove(oldQuad);
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
