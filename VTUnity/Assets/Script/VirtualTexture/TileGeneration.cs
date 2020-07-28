using System.Collections;
using System.Collections.Generic;
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

    
    
    public void GeneratePage(int quadkey)
    {

    }

    void Start()
    {
        TilesToGenerate = new Queue<int>();
        TileGeneratorMat = new Material(TileGenerator);
        LayerCount = DemoTerrain.terrainData.terrainLayers.Length;
        for(int i = 0; i < LayerCount; i++)
        {
            TerrainLayer currLayer = DemoTerrain.terrainData.terrainLayers[i];
            
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
