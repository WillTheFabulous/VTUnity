using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Json;
using System.Transactions;
using System.Threading;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Experimental.U2D;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.WSA;

namespace VirtualTexture
{
    public class PageTable : MonoBehaviour
    {
        [SerializeField]
        private int m_TableSize = default;

        public int TableSize { get { return m_TableSize; } }

        [SerializeField]
        public int MaxMipLevel { get { return (int)Mathf.Log(TableSize, 2); } }

        //GPU 端使用的页表查询贴图
        [SerializeField]
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
        public Dictionary<int, PhysicalTileInfo> AddressMapping { get; set; }


        [SerializeField]
        private int m_ThreadRectNum = 4;


        private Thread[] m_Threads = default;



        void Start()
        {
            quadRootKey = getKey(0, 0, MaxMipLevel);

            m_LookupTexture = new Texture2D(TableSize, TableSize, TextureFormat.RGBA32, false);
            m_LookupTexture.wrapMode = TextureWrapMode.Clamp;
            m_LookupTexture.filterMode = FilterMode.Point;

            Shader.SetGlobalTexture("_LOOKUPTEX", m_LookupTexture);
            Shader.SetGlobalFloat("_MAXMIP", MaxMipLevel);
            Shader.SetGlobalFloat("_PAGETABLESIZE", TableSize);

            m_Threads = new Thread[m_ThreadRectNum * m_ThreadRectNum];


            tileGenerator = (TileGeneration)GetComponent(typeof(TileGeneration));
            physicalTiles = (PhysicalTexture)GetComponent(typeof(PhysicalTexture));
            feedBack = (Feedback)GetComponent(typeof(Feedback));

            AddressMapping = new Dictionary<int, PhysicalTileInfo>();

            feedBack.OnFeedbackReadComplete += ProcessFeedback;
            tileGenerator.OnTileGenerationComplete += OnGenerationComplete;

            /**
            List<int> childs = getChilds(quadRootKey);
            List<int> childschilds = getChilds(childs[0]);
            int child0 = quadRootKey - 0x1000000;
            print(child0 == childs[0]);
            print(getMip(child0));
            print(childs[1] - childs[0]);
            print(childschilds[1] - childschilds[0]);

            **/
            //print(getMip(childs[0]) - getMip(childschilds[0]));


        }

        void Update()
        {
            /**
            if(Time.frameCount == 2)
            {
                CreatePage(quadRootKey);
                
                List<int> childs = getChilds(quadRootKey);
                foreach (var child in childs)
                {
                    CreatePage(child);
                    List<int> childchilds = getChilds(child);
                    foreach (var childchild in childchilds)
                    {
                        CreatePage(childchild);
                    }
                }
                

            }
            **/

        }

        private void ProcessFeedback(Texture2D texture)
        {
            //TODO: MAKE UNIQUE PAGE LIST 多线程处理？
            //TODO: 预生产周边的mip level的tile

            //每一帧 我们将当前feedback texture中 还没有被开始生产的tile 扔入生产队列 并在 quadtree 中插入他的信息

            List<int> UniquePageList = new List<int>();


            int texWidth = texture.width;
            int texheight = texture.height;
            var textureData = texture.GetRawTextureData<Color32>();


            
            //Todo 多线程!!!!!!!!!!

            for (int i = 0; i < texWidth; i += 10)
            {
                for(int j = 0; j < texheight; j += 10)
                {
                    int pixelIndex = j * texWidth + i;
                    var color = textureData[pixelIndex];
                    //跳过白色背景

                    if (color.b != 255)
                    {

                        UseOrCreatePage(color.r, color.g, color.b);
                    }
                }
            }
            
            //print(count);

            //Update after DownScale is implemented
            /**
            foreach (var color in texture.GetRawTextureData<Color32>())
            {
                if (color.b != 255)
                    {
                        
                        UseOrCreatePage(color.r, color.g, color.b);
                    }
            }
            **/
            
            var pixels = m_LookupTexture.GetRawTextureData<Color32>();
            var currentFrame = (byte)Time.frameCount;
            foreach (var kv in AddressMapping)
            {
                PhysicalTileInfo currMapping = kv.Value;
                
                if(currMapping.ActiveFrame != Time.frameCount || currMapping.tileStatus != TileStatus.LoadingComplete)
                {
                    continue;
                }
                int currMip = getMip(currMapping.QuadKey);
                Vector2Int pageXY = getPageXY(currMapping.QuadKey);
                int RectLength = mipRectLengthFromMip(currMip);
                Color32 c = new Color32((byte)currMapping.TileIndex.x, (byte)currMapping.TileIndex.y, (byte)currMip, currentFrame);
                //print(c);
                //print(RectLength);

                for (int x = pageXY.x; x < pageXY.x + RectLength; x++)
                {
                    for(int y = pageXY.y; y < pageXY.y + RectLength; y++)
                    {
                        var id = y * TableSize + x;
                        if (pixels[id].b > c.b || pixels[id].a != currentFrame)
                            pixels[id] = c;
                    }
                }                
            }
            m_LookupTexture.Apply(false);


        }


        //
        private int UseOrCreatePage(int x, int y, int mip)
        {

            if(mip > MaxMipLevel)
            {
                mip = MaxMipLevel;
            }
            
            //找到最深miplevel的可用quadtree page
            int page = SearchPage(x, y, mip, quadRootKey);


            //没有任何可用page 加载root
            if(page == -1)
            {
                CreatePage(quadRootKey);
            }//当前page mip大于要求的mip(quadtree 还没加载到那个深度) 我们暂时使用并显示当前page 并把他的child加入生成队列
            else if(getMip(page) > mip)
            {

                    AddressMapping[page].ActiveFrame = Time.frameCount;
                    tileGenerator.SetActive(AddressMapping[page].TileIndex);

                int childQuadKey = getChild(x, y, page);
                
                if(childQuadKey == -1)
                {
                    return -1;
                }
                CreatePage(childQuadKey);
            }//mip 符合要求 直接使用当前page
            else
            {
                AddressMapping[page].ActiveFrame = Time.frameCount;
            }

            return page; 
        }


        //找到最深miplevel的可用quadtree page
        private int SearchPage(int x, int y, int targetMip, int quadKey)
        {
            if(!Contains(x, y, quadKey))
            {
                return -1;
            }

            int currMip = getMip(quadKey);
            if (currMip > targetMip)
            {
                List<int> childs = getChilds(quadKey);
                foreach(var child in childs)
                {
                    int page = SearchPage(x, y, targetMip, child);
                    if(page != -1)
                    {
                        return page;
                    }
                }
            }


            if (AddressMapping.ContainsKey(quadKey) && AddressMapping[quadKey].tileStatus == TileStatus.LoadingComplete)
            {
                return quadKey;
            }
            else
            {
                return -1;
            }
            //找到指定深度
            /**
            if (targetMip == currMip)
            {
                if (AddressMapping.ContainsKey(quadKey) && AddressMapping[quadKey].tileStatus == TileStatus.LoadingComplete)
                {
                    return quadKey;
                }
                else
                {
                    return -1;
                }
            }//未到达指定深度
            else if(targetMip < currMip)
            {
                List<int> childs = getChilds(quadKey);

                if(childs == null)
                {
                    return -1;
                }
                foreach(var child in childs)
                {
                    int page = SearchPage(x, y, targetMip, child);
                    if(page != -1)
                    {
                        return page;
                    }
                }

                if (AddressMapping.ContainsKey(quadKey) && AddressMapping[quadKey].tileStatus == TileStatus.LoadingComplete)
                {
                    return quadKey;
                }
                else
                {
                    return -1;
                }
            }
            
            return -1;
            **/

        }

        public void CreatePage(int quadKey)
        {
            //不重复生成
            if(AddressMapping.ContainsKey(quadKey) && AddressMapping[quadKey].tileStatus == TileStatus.Loading)
            {
                return;
            }

            PhysicalTileInfo info = new PhysicalTileInfo();
            info.tileStatus = TileStatus.Loading;

            info.QuadKey = quadKey;

            AddressMapping[quadKey] = info;
            tileGenerator.GeneratePageTask(quadKey);
        }

        public void OnGenerationComplete(List<int> quadKeys)
        {
            foreach(var quadKey in quadKeys)
            {
                AddressMapping[quadKey].tileStatus = TileStatus.LoadingComplete;
            }
        }


        //提供的x y 值是否在提供的quadkey 范围内
        private bool Contains(int x, int y, int key)
        {
            Vector2Int pageXY = getPageXY(key);
            int rectLength = mipRectLengthFromKey(key);

            if(pageXY.x <= x && pageXY.y <= y && (pageXY.x + rectLength) > x && (pageXY.y + rectLength) > y)
            {
                return true;
            }
            return false;
        }



        //返回一个四个方向上childs的quadkey的list
        private List<int> getChilds(int key)
        {
            int curr_mip = getMip(key);

            if(curr_mip == 0)
            {
                return null;
            }

            /**
            Vector2Int pageXY = getPageXY(key);

            int rectLength = mipRectLengthFromMip(curr_mip - 1);
            
            int child1 = getKey(pageXY.x, pageXY.y, curr_mip - 1);
            int child2 = getKey(pageXY.x + rectLength, pageXY.y, curr_mip - 1);
            int child3 = getKey(pageXY.x, pageXY.y + rectLength, curr_mip - 1);
            int child4 = getKey(pageXY.x + rectLength, pageXY.y + rectLength, curr_mip - 1);
            **/
            int delta = 1 << ((curr_mip - 1) * 2);
            int child1 = key - 0x1000000;
            int child2 = child1 + delta;
            int child3 = child2 + delta;
            int child4 = child3 + delta;
            
            List<int> result = new List<int>();

            result.Add(child1);
            result.Add(child2);
            result.Add(child3);
            result.Add(child4);

            return result;
        }

        //返回包含x和y的下一级mip的child
        private int getChild(int x, int y, int quadKey)
        {
            List<int> childs = getChilds(quadKey);
            foreach(var child in childs)
            {
                if (Contains(x, y, child))
                {
                    return child;
                }
            }

            return -1;
        }

        // 用以生成 8 bits miplevel, 12 bits pageX, 12 bits pageY
        public int getKey(int pageX, int pageY, int mip)
        {
            if(mip > MaxMipLevel)
            {
                mip = MaxMipLevel;
            }
            int result = MortonCode2(pageX) | (MortonCode2(pageY) << 1);
            result |= (mip << 24);

            return result;
        }

        public int getMip(int key)
        {
            return key >> 24;
        }

        public Vector2Int getPageXY(int key)
        {
            //mask out mip bits
            int mask = 0x00ffffff;
            key &= mask;
            int pageX = ReverseMortonCode2(key);
            int pageY = ReverseMortonCode2(key >> 1);

            return new Vector2Int(pageX, pageY);
        }

        public int mipRectLengthFromKey(int key)
        {
            return mipRectLengthFromMip(getMip(key));
        }

        public int mipRectLengthFromMip(int mip)
        {
            return TableSize / (int)Math.Pow(2, MaxMipLevel -  mip);
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

        
    }
}
