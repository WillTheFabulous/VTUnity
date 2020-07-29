using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Tilemaps;
using VirtualTexture;

public class TileGeneration : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private Shader TileGenerator = default;

    private Queue<int> TilesToGenerate = default;

    private Material TileGeneratorMat = default;

    [SerializeField]
    private Terrain DemoTerrain = default;

    [SerializeField]
    private int LayerCount = default;

    private PhysicalTexture physicalTexture = default;

    private Mesh m_Quads;


    void Start()
    {
        TilesToGenerate = new Queue<int>();
        TileGeneratorMat = new Material(TileGenerator);
        LayerCount = DemoTerrain.terrainData.terrainLayers.Length;

        physicalTexture =(PhysicalTexture) GetComponent(typeof(PhysicalTexture));

        //TODO 更好的texture存储方式？？
        TileGeneratorMat.SetTexture("_AlphaMap", DemoTerrain.terrainData.alphamapTextures[0]);
        for(int i = 0; i < LayerCount; i++)
        {
            TerrainLayer currLayer = DemoTerrain.terrainData.terrainLayers[i];
            TileGeneratorMat.SetTexture("_Diffuse" + i, currLayer.diffuseTexture);
            TileGeneratorMat.SetTexture("_Normal" + i, currLayer.normalMapTexture);
        }
        
    }

    public void GeneratePage(int x, int y, int mip)
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




        Pixel.x = (NDC.x + 1) * Width/2
        Pixel.y = (1 − NDC.y) * Height/2

         **/
       


        




    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
