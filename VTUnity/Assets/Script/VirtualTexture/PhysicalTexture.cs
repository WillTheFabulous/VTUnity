﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace VirtualTexture
{
    public class PhysicalTexture : MonoBehaviour
    {
        // Start is called before the first frame update

        [SerializeField]
        private Vector2Int m_PhysicalTextureSize = new Vector2Int(8, 8);

        public Vector2Int PhysicalTextureSize { get { return m_PhysicalTextureSize; } }

        [SerializeField]
        private int m_TileSize = 128;

        [SerializeField]
        private int m_PaddingSize = 16;

        [SerializeField]
        private int m_NumTextureType = 2;

        [SerializeField]
        private int m_AnisoLevel = default;

        public int NumTextureType {get { return m_NumTextureType; } }

        //用于存储混好的color normal 等等
        public RenderTexture[] PhysicalTextures { get; private set; }

        //physical tile 到 虚拟纹理 quad 的 mapping 用以通过physical tile 找回quad key
        public Dictionary<Vector2Int,int> TileToQuadMapping { get; set; }

        public int PaddingSize { get { return m_PaddingSize; } }

        public int TileSize { get { return m_TileSize; } }

        
        void Start()
        {
            PhysicalTextures = new RenderTexture[m_NumTextureType];
            TileToQuadMapping = new Dictionary<Vector2Int, int>();
            m_AnisoLevel = Mathf.Clamp(m_AnisoLevel, 0, 9);
            for(int i = 0; i < m_NumTextureType; i++)
            {
                PhysicalTextures[i] = new RenderTexture(m_PhysicalTextureSize.x * (m_TileSize + 2 * m_PaddingSize), m_PhysicalTextureSize.y * (m_TileSize + 2 * m_PaddingSize), 0);
                PhysicalTextures[i].filterMode = FilterMode.Trilinear;
                PhysicalTextures[i].anisoLevel = m_AnisoLevel;
                PhysicalTextures[i].useMipMap = true;
                PhysicalTextures[i].autoGenerateMips = false;
                PhysicalTextures[i].wrapMode = TextureWrapMode.Clamp;
                Shader.SetGlobalTexture("_PHYSICALTEXTURE" + i, PhysicalTextures[i]);
            }

            Shader.SetGlobalInt("_TILESIZE", m_TileSize);
            Shader.SetGlobalInt("_PADDINGSIZE", m_PaddingSize);
            Shader.SetGlobalVector("_PHYSICALTEXTURESIZE", new Vector2(m_PhysicalTextureSize.x, m_PhysicalTextureSize.y));

            Vector2Int physicalTexelSize = new Vector2Int((m_TileSize + 2 * m_PaddingSize) * m_PhysicalTextureSize.x, (m_TileSize + 2 * m_PaddingSize) * m_PhysicalTextureSize.y);
            int physicalMaxSize = (int)Math.Log((double)Math.Min(physicalTexelSize.x, physicalTexelSize.y),2);
            Shader.SetGlobalInt("_PHYSICALMAXMIP", physicalMaxSize);

        }
        
        // Update is called once per frame
        void Update()
        {

        }

   
    }
}
