using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace VirtualTexture
{
    public class TileGeneration : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField]
        private Shader TileGenerator = default;

        private Queue<int> TilesToGenerate = default;

        private List<int> GeneratingTiles = default;

        private Material TileGeneratorMat = default;

        [SerializeField]
        private Terrain DemoTerrain = default;

        [SerializeField]
        private int LayerCount = default;

        private PhysicalTexture physicalTexture = default;

        private PageTable pageTable = default;

        private Mesh m_Quads;

        private LruCache m_TilePool;


        void Start()
        {
            TilesToGenerate = new Queue<int>();
            TileGeneratorMat = new Material(TileGenerator);
            LayerCount = DemoTerrain.terrainData.terrainLayers.Length;

            physicalTexture = (PhysicalTexture)GetComponent(typeof(PhysicalTexture));
            pageTable = (PageTable)GetComponent(typeof(PageTable));
            TilesToGenerate = new Queue<int>();

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
                TilesToGenerate.Enqueue(quadKey);
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

            Graphics.ExecuteCommandBuffer(tempCB);//DEBUG\




           

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

            //TODO 做成一次贴三个的
            List<int> TilesForMesh = new List<int>();
            
            int quadKey = TilesToGenerate.Peek();
            
            TilesForMesh.Add(quadKey);

            SetUpMesh(TilesForMesh);


        }

        private void SetUpMesh(List<int> quadKeys)
        {
            List<Vector3> quadVertexList = new List<Vector3>();
            List<int> quadTriangleList = new List<int>();
            List<Vector2> quadUVList = new List<Vector2>();
            int tableSize = pageTable.TableSize;
            //有可能出现的最高mip
            int maxMip = (int)Math.Log(tableSize, 2);
            
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
                //Calculate screen space tile width and height

                float tileBaseWidth = 1.0f / (float)physicalTexture.PhysicalTextureSize.x;
                float tileBaseHeight = 1.0f / (float)physicalTexture.PhysicalTextureSize.y;
                /**
                Pixel.x = (NDC.x + 1) * Width / 2
                Pixel.y = (1 − NDC.y) *Height / 2
                */
                Vector2Int tile = RequestTile();
                //quadVertexList.Add(new Vector3)

                
                
            }
           
        }

        private 
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
