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
using Unity.Collections;
using System.Data;

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

        private TableNode quadRoot = default;

        //indirection texture 到 physical texture 的 mapping
        public Dictionary<int, PhysicalTileInfo> AddressMapping { get; set; }

        public Dictionary<int, int[]> ChildList { get; set; }

        private int m_DebugMode = 0;

        public Dictionary<int, TableNode> m_Pages { get; set; }

        //multiple quad trees 
        private TableNode[] quadRoots = default;

        [SerializeField]
        private int m_TerrainDivision = 2;


        private Thread[] m_Threads = default;
        private readonly static object m_Locker = new object();

        private ReaderWriterLockSlim rwl = new ReaderWriterLockSlim();
        private ReaderWriterLockSlim rwl2 = new ReaderWriterLockSlim();

        


        

        public struct ThreadParams
        {
            public Vector2Int start;
            public Vector2Int end;
            public NativeArray<Color32> pixels;
            public int width;
            public int height;
            public int frame;
        }

        void Start()
        {
            if (m_TableSize < 1 || (m_TableSize & m_TableSize - 1) != 0)
            {
                m_TableSize = 64;
            }
            quadRootKey = getKey(0, 0, MaxMipLevel);


            m_LookupTexture = new Texture2D(TableSize, TableSize, TextureFormat.RGBA32, false);
            m_LookupTexture.wrapMode = TextureWrapMode.Clamp;
            m_LookupTexture.filterMode = FilterMode.Point;

            Shader.SetGlobalTexture("_LOOKUPTEX", m_LookupTexture);
            Shader.SetGlobalFloat("_MAXMIP", MaxMipLevel);
            Shader.SetGlobalFloat("_PAGETABLESIZE", TableSize);
            Shader.SetGlobalInt("_DEBUG", m_DebugMode);

            tileGenerator = (TileGeneration)GetComponent(typeof(TileGeneration));
            physicalTiles = (PhysicalTexture)GetComponent(typeof(PhysicalTexture));
            feedBack = (Feedback)GetComponent(typeof(Feedback));

            AddressMapping = new Dictionary<int, PhysicalTileInfo>();
            ChildList = new Dictionary<int, int[]>();

            feedBack.OnFeedbackReadComplete += ProcessFeedback;
            //tileGenerator.OnTileGenerationComplete += OnGenerationComplete;
            tileGenerator.OnTileGenerationComplete += OnGenerationCompletePointer;
            //FOR POINTER
            quadRoot = new TableNode(MaxMipLevel, 0, 0, TableSize, TableSize);
            m_Pages = new Dictionary<int, TableNode>();

            //multiple quad trees
            quadRoots = new TableNode[m_TerrainDivision * m_TerrainDivision];
            for(int i = 0; i < m_TerrainDivision; i++)
            {
                for (int j = 0; j < m_TerrainDivision; j++) {
                    //quadRoots[i] = new TableNode(MaxMipLevel, i )
                }
            }



        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if(m_DebugMode == 0)
                {
                    m_DebugMode = 1;
                }
                else if(m_DebugMode == 1)
                {
                    m_DebugMode = 2;
                }
                else
                {
                    m_DebugMode = 0;
                }
                Shader.SetGlobalInt("_DEBUG", m_DebugMode);

            }
        }



        private void ProcessFeedback(Texture2D texture)
        {
            //TODO: MAKE UNIQUE PAGE LIST 多线程处理？
            //TODO: 预生产周边的mip level的tile

            //每一帧 我们将当前feedback texture中 还没有被开始生产的tile 扔入生产队列 并在 quadtree 中插入他的信息


            int texWidth = texture.width;
            int texHeight = texture.height;
            var textureData = texture.GetRawTextureData<Color32>();


            for (int i = 0; i < texWidth; i += 10)
            {
                for (int j = 0; j < texHeight; j += 10)
                {
                    int pixelIndex = j * texWidth + i;
                    var color = textureData[pixelIndex];
                    //跳过白色背景

                    if (color.b != 255)
                    {
                        //print(color.b);
                        UseOrCreatePagePointer(color.r, color.g, color.b, (int)Time.frameCount);
                    }
                }
            }


            //Todo 多线程!!!!!!!!!!
            //TODO garbage collector!!!!!!!!!!!!!!!!!!!!!!!!

            /*int threadRectSizeWidth = texWidth / m_ThreadRectNum;
            int threadRectSizeHeight = texHeight / m_ThreadRectNum;

            int toProcess = m_ThreadRectNum * m_ThreadRectNum;
            //UnityEngine.Profiling.Profiler.BeginSample("UseOrCreatePage");
            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                for (int i = 0; i < m_ThreadRectNum; i++)
                {
                    for (int j = 0; j < m_ThreadRectNum; j++)
                    {
                        Vector2Int start = new Vector2Int(i * threadRectSizeWidth, j * threadRectSizeHeight);
                        Vector2Int end = new Vector2Int();
                        end.x = (i == m_ThreadRectNum - 1) ? texWidth : (i + 1) * threadRectSizeWidth;
                        end.y = (j == m_ThreadRectNum - 1) ? texHeight : (j + 1) * threadRectSizeHeight;
                        ThreadParams Params = new ThreadParams();
                        Params.start = start;
                        Params.end = end;
                        Params.width = texWidth;
                        Params.height = texHeight;
                        Params.pixels = textureData;
                        Params.frame = (int)Time.frameCount;


                        ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                        {
                            UseOrCreatePageThread(x);
                            if (Interlocked.Decrement(ref toProcess) == 0)
                                resetEvent.Set();
                        }), Params);

                    }
                }
                //print(toProcess);
                resetEvent.WaitOne();

            }*/

            //UnityEngine.Profiling.Profiler.EndSample();

            RefreshLookupTablePointer();


        }

        private void UseOrCreatePageThread(System.Object obj)
        {
            ThreadParams p = (ThreadParams)obj;
            Vector2Int start = p.start;
            Vector2Int end = p.end;

            for(int x = start.x; x < end.x; x += 10)
            {
                for(int y = start.y; y < end.y; y += 10)
                {
                    int pixelIndex = y * p.width + x;
                    var color = p.pixels[pixelIndex];
                    //跳过白色背景
                    if (color.b != 255)
                    {
                        
                        UseOrCreatePage(color.r, color.g, color.b, p.frame);
                    }
                }
            }

        }


        private int UseOrCreatePage(int x, int y, int mip, int frame)
        {
            //print(mip);

            if(mip > MaxMipLevel)
            {
                mip = MaxMipLevel;
            }

            //找到最深miplevel的可用quadtree page
            int page = SearchPage(x, y, mip, quadRootKey);

            //没有任何可用page 加载root
            if (page == -1)
            {

                CreatePage(quadRootKey);
            }//当前page mip大于要求的mip(quadtree 还没加载到那个深度) 我们暂时使用并显示当前page 并把他的child加入生成队列
            else
            {
                //mip 符合要求 直接使用当前page
                rwl.EnterWriteLock();
                try
                {
                    AddressMapping[page].ActiveFrame = frame;
                    tileGenerator.SetActive(AddressMapping[page].TileIndex);
                }
                finally
                {
                    rwl.ExitWriteLock();
                }

                //当前page mip大于要求的mip(quadtree 还没加载到那个深度) 我们暂时使用并显示当前page 并把他的child加入生成队列
                if (getMip(page) > mip)
                {
                    int childQuadKey = getChild(x, y, page);
                    if (childQuadKey == -1)
                    {
                        return -1;
                    }
                    CreatePage(childQuadKey);
                }

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
                int[] childs = getChilds(quadKey);
                foreach(var child in childs)
                {
                    int page = SearchPage(x, y, targetMip, child);
                    if(page != -1)
                    {
                        return page;
                    }
                }
                
            }

            rwl.EnterReadLock();
            try
            {
                if (AddressMapping.ContainsKey(quadKey) && AddressMapping[quadKey].tileStatus == TileStatus.LoadingComplete)
                {
                    return quadKey;
                }
                else
                {
                    return -1;
                }
            }
            finally
            {
                rwl.ExitReadLock();
            }
            
            //找到指定深度
           

        }

        public void CreatePage(int quadKey)
        {
            //不重复生成
            rwl.EnterReadLock();
            try
            {
                if (AddressMapping.ContainsKey(quadKey) && AddressMapping[quadKey].tileStatus == TileStatus.Loading)
                {
                    return;
                }
            }
            finally
            {
                rwl.ExitReadLock();
            }
           

                PhysicalTileInfo info = new PhysicalTileInfo();
                info.tileStatus = TileStatus.Loading;
            rwl.EnterWriteLock();
            try
            {
                info.QuadKey = quadKey;
                AddressMapping[quadKey] = info;
                tileGenerator.GeneratePageTask(quadKey);
            }
            finally
            {
                rwl.ExitWriteLock();
            }
                
            
        }


        private void RefreshLookupTable()
        {
            var pixels = m_LookupTexture.GetRawTextureData<Color32>();
            var currentFrame = (byte)Time.frameCount;


            foreach (var kv in AddressMapping)
            {
                PhysicalTileInfo currMapping = kv.Value;

                if (currMapping.ActiveFrame != Time.frameCount || currMapping.tileStatus != TileStatus.LoadingComplete)
                {
                    continue;
                }
                int currMip = getMip(currMapping.QuadKey);
                Vector2Int pageXY = getPageXY(currMapping.QuadKey);
                int RectLength = mipRectLengthFromMip(currMip);
                Color32 c = new Color32((byte)currMapping.TileIndex.x, (byte)currMapping.TileIndex.y, (byte)currMip, currentFrame);

                for (int x = pageXY.x; x < pageXY.x + RectLength; x++)
                {
                    for (int y = pageXY.y; y < pageXY.y + RectLength; y++)
                    {
                        var id = y * TableSize + x;
                        if (pixels[id].b > c.b || pixels[id].a != currentFrame)
                            pixels[id] = c;
                    }
                }
            }
            m_LookupTexture.Apply(false);
        }


        private void UseOrCreatePagePointer(int x, int y, int mip, int frame)
        {
            if (mip > MaxMipLevel)
            {
                mip = MaxMipLevel;
            }

            var page = quadRoot.GetAvailable(x, y, mip);
            if(page == null)
            {
                CreatePagePointer(x, y, quadRoot);
            }
            else
            {
                page.Payload.ActiveFrame = frame;
                tileGenerator.SetActive(page.Payload.TileIndex);


                if (page.MipLevel > mip)
                {
                    CreatePagePointer(x,y,page.GetChild(x,y));
                }
            }
        }


        public void CreatePagePointer(int x, int y, TableNode node)
        {
            if(node == null)
            {
                return;
            }

            if(node.Payload.tileStatus == TileStatus.Loading || node.Payload.tileStatus == TileStatus.LoadingComplete)
            {
                return;
            }

            node.Payload.tileStatus = TileStatus.Loading;
            int key = getKey(node.Rect.x, node.Rect.y, node.MipLevel);
            m_Pages[key] = node;
            tileGenerator.GeneratePageTask(key);

        }

        public void RefreshLookupTablePointer()
        {
            var pixels = m_LookupTexture.GetRawTextureData<Color32>();
            var currentFrame = (byte)Time.frameCount;


            foreach (var kv in m_Pages)
            {
                TableNode currNode = kv.Value;
                PhysicalTileInfo currMapping = currNode.Payload;

                if (currMapping.ActiveFrame != Time.frameCount || currMapping.tileStatus != TileStatus.LoadingComplete)
                {
                    continue;
                }
                int currMip = currNode.MipLevel;
                Vector2Int pageXY = getPageXY(currMapping.QuadKey);
                int RectLength = mipRectLengthFromMip(currMip);
                Color32 c = new Color32((byte)currMapping.TileIndex.x, (byte)currMapping.TileIndex.y, (byte)currMip, currentFrame);

                for (int x = currNode.Rect.x; x < currNode.Rect.xMax; x++)
                {
                    for (int y = currNode.Rect.y; y < currNode.Rect.yMax; y++)
                    {
                        var id = y * TableSize + x;
                        if (pixels[id].b > c.b || pixels[id].a != currentFrame)
                            pixels[id] = c;
                    }
                }
            }
            m_LookupTexture.Apply(false);
        }

        public void OnGenerationCompletePointer(List<int> quadKeys)
        {
            foreach (var quadKey in quadKeys)
            {
                m_Pages[quadKey].Payload.tileStatus = TileStatus.LoadingComplete;
            }
        }

        public void OnGenerationComplete(List<int> quadKeys)
        {
            rwl.EnterWriteLock();
            try
            {
                foreach (var quadKey in quadKeys)
                {
                    AddressMapping[quadKey].tileStatus = TileStatus.LoadingComplete;
                }
            }
            finally
            {
                rwl.ExitWriteLock();
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
        private int[] getChilds(int key)
        {
            int curr_mip = getMip(key);

            if(curr_mip == 0)
            {
                return null;
            }
            rwl2.EnterReadLock();
            try
            {
                if (ChildList.ContainsKey(key))
                {
                    return ChildList[key];
                }
            }
            finally
            {
                rwl2.ExitReadLock();
            }

            int delta = 1 << ((curr_mip - 1) * 2);
            int child1 = key - 0x1000000;
            int child2 = child1 + delta;
            int child3 = child2 + delta;
            int child4 = child3 + delta;
            
            int[] result = new int[4];

            result[0] = child1;
            result[1] = child2;
            result[2] = child3;
            result[3] = child4;

            rwl2.EnterWriteLock();
            try
            {
                ChildList[key] = result;
            }
            finally
            {
                rwl2.ExitWriteLock();
            }

            return result;
        }

        //返回包含x和y的下一级mip的child
        private int getChild(int x, int y, int quadKey)
        {
            int[] childs = getChilds(quadKey);
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
