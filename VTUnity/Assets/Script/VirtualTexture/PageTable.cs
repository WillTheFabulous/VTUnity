using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PageTable : MonoBehaviour
{    
    [SerializeField]
    private int m_TableSize = default;

    public Dictionary<int, TextureTileMapping> m_AddressMapping = default;

    [SerializeField]
    private int m_MipLevelLimit = default;

    //GPU 端使用的页表查询贴图
    private Texture2D m_LookupTexture = default;

    //用于blending terrain layers
    private TileGeneration tileGenerator = default;

    //用于管理physical texture
    private PhysicalTexture physicalTiles = default;

    //页表为 m_TableSize * m_TableSize 的 quadtree 有 (4 * m_TableSize * m_TableSize - 1)/3 个node 
    //12 bits pageX, 12 bits pageY, 8 bits miplevel
    private int quadRootMorton = default;

    void Start()
    {
        quadRootMorton = getMorton(2, 3, m_MipLevelLimit);
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int getMorton(int pageX, int pageY, int mip)
    {
        int result = 

        string binX = Convert.ToString(pageX, 2);
        string binY = Convert.ToString(pageY, 2);
        string X = Convert.ToString(x, 2);

        print(binX);
        print(binY);
        print(X);
        return x;
    }

    public int MortonCode2(int x)
    {
        x &= 0x0000ffff;
        x = (x ^ (x << 8)) & 0x00ff00ff;
        x = (x ^ (x << 4)) & 0x0f0f0f0f;
        x = (x ^ (x << 2)) & 0x33333333;
        x = (x ^ (x << 1)) & 0x55555555;
        return x;
    }

    public int ReverseMortonCode2(int x)
    {
        x &= 0x55555555;
        x = (x ^ (x >> 1)) & 0x33333333;
        x = (x ^ (x >> 2)) & 0x0f0f0f0f;
        x = (x ^ (x >> 4)) & 0x00ff00ff;
        x = (x ^ (x >> 8)) & 0x0000ffff;
        return x;
    }
}
