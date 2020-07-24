using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Transactions;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace VirtualTexture
{
    public class PageTable : MonoBehaviour
    {
        [SerializeField]
        private int m_TableSize = default;

        public int TableSize { get { return m_TableSize; } }



        [SerializeField]
        private int m_MipLevelLimit = default;

        public int MaxMipLevel { get { return Mathf.Min(m_MipLevelLimit, (int)Mathf.Log(TableSize, 2)); } }

        //GPU 端使用的页表查询贴图
        private Texture2D m_LookupTexture = default;

        //用于terrain layers blending
        private TileGeneration tileGenerator = default;

        //用于管理physical texture
        private PhysicalTexture physicalTiles = default;

        private Feedback feedBack = default;

        //页表为 m_TableSize * m_TableSize 的 quadtree 有 (4 * m_TableSize * m_TableSize - 1)/3 个node 
        //8 bits miplevel, 12 bits pageX, 12 bits pageY
        private int quadRootKey = default;

        //indirection texture 到 physical texture 的 mapping
        public Dictionary<int, PhysicalTileInfo> m_AddressMapping = default;


        void Start()
        {
            quadRootKey = getKey(0, 0, m_MipLevelLimit);

            m_LookupTexture = new Texture2D(TableSize, TableSize, TextureFormat.RGBA32, false);

            Shader.SetGlobalTexture("_LOOKUPTEX", m_LookupTexture);
            Shader.SetGlobalFloat("_MAXMIP", MaxMipLevel);

            tileGenerator = (TileGeneration)GetComponent(typeof(TileGeneration));
            physicalTiles = (PhysicalTexture)GetComponent(typeof(PhysicalTexture));
            feedBack = (Feedback)GetComponent(typeof(Feedback));

            feedBack.OnFeedbackReadComplete += ProcessFeedback;


            int key = getKey(2, 3, 5);
            getPage(key);
            

        }

        void Update()
        {

        }

        private void ProcessFeedback(Texture2D texture)
        {
            foreach(var color in texture.GetRawTextureData<Color32>())
            {

            }
        }

        private int GetOrCreatePage(int x, int y, int mip)
        {
            return 0; 
        }

        private bool Contains(int x, int y, int key)
        {
            return false;
        }

        // 用以生成 8 bits miplevel, 12 bits pageX, 12 bits pageY
        public int getKey(int pageX, int pageY, int mip)
        {
            int result = MortonCode2(pageX) | (MortonCode2(pageY) << 1);
            result |= (mip << 24);

            return result;
        }

        public int getMip(int key)
        {
            return key >> 24;
        }

        public Vector2Int getPage(int key)
        {
            print(key);
            //mask out mip bits
            int mask = 0x00ffffff;
            key &= 

            print(key);

            return new Vector2Int();
        }

        /** Spreads bits to every other. */
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

       

        /**
        public void louzhengqiu(int n)
        {
            int
        }

        public void guojiaqi(int n, int[] input, int[] output)
        {
            if(n == 0)
            {
                for(int i = 0; i < result.Length; i++)
                {
                    print(result[i]);
                }
            }
            else
            {
                for(int j = 0; j < n; j++)
                {
                    
                }
            }
        }
        **/
    }
}
