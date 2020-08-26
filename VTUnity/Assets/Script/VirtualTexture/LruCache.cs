using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VirtualTexture
{
    public class LruCache
    {
        private Dictionary<int, LinkedListNode<int>> m_Map = new Dictionary<int, LinkedListNode<int>>();


        /*
         *  Head |   mip0     | mip1 |     mip3    |        mip4          | Tail
         *  保证低mip的tile被优先替出
         */
        private LinkedList<int> m_List = new LinkedList<int>();

        //每个mip list的结尾
        private LinkedListNode<int>[] m_MipEnd = default;

        //每个mip数量
        private int[] m_NumMip = default;

        //Tile 到 mip的映射
        private int[] m_IdToMip = default;

        private Vector2Int m_PhysicalSize = default;

        public int First { get { return m_List.First.Value; } }

        private PhysicalTexture physicalTexture = default;

        private PageTable pageTable = default;

        public LruCache(int maxMip, int physicalSizeX, int physicalSizeY, PhysicalTexture physical, PageTable table)
        {
            m_MipEnd = new LinkedListNode<int>[maxMip + 1];
            
            m_NumMip = new int[maxMip + 1];
            for(int i = 0; i < maxMip + 1; i ++)
            {
                m_NumMip[i] = 0;
            }
            m_PhysicalSize = new Vector2Int(physicalSizeX, physicalSizeY);
            m_IdToMip = new int[physicalSizeX * physicalSizeY];
            for (int i = 0; i < physicalSizeX * physicalSizeY; i++)
            {
                Add(i);
            }
            //将mip 0 list的结尾放到list 的最后，其他mip list 结尾暂时为null
            m_MipEnd[0] = m_List.Last;

            physicalTexture = physical;
            pageTable = table;

        }
        public void Add(int id)
        {
            if (m_Map.ContainsKey(id))
                return;

            var node = new LinkedListNode<int>(id);
            m_Map.Add(id, node);
            m_List.AddLast(node);
            m_IdToMip[id] = -1;
        }

        public bool SetActive(Vector2Int tile)
        {
            int id = PosToId(tile);

            LinkedListNode<int> node = null;
            if (!m_Map.TryGetValue(id, out node))
                return false;

            //int mip = m_IdToMip[id];
            //if (m_MipEnd[mip] == null)
            //{
            //    if (mip == 0) //mip 0 为 null只可能是已满替换后的结果
            //    {
            //        m_MipEnd[mip] = node;
            //        return true;
            //    }
            //    else
            //    {
            //        int tempMip = mip - 1;
            //        //发现mip list结尾为null，还不存在当前mip的list，往前寻找低mip的结尾
            //        while (tempMip >= 0)
            //        {
            //            if (m_MipEnd[tempMip] != null)
            //            {
            //                m_MipEnd[mip] = m_MipEnd[tempMip];
            //                break;
            //            }
            //            tempMip--;
            //        }

            //        //大于等于0的mip end 都是空的
            //        if (m_MipEnd[mip] == null)
            //        {
            //            m_MipEnd[mip] = node;
            //            return true;
            //        }
            //    }
            //}

            //if (node == m_MipEnd[mip])
            //{
            //    return true;
            //}
            m_List.Remove(node);
            m_List.AddLast(node);
            //m_List.AddAfter(m_MipEnd[mip],node);

            //m_MipEnd[mip] = node;

            return true;
        }

        public Vector2Int RequestTile(int targetMip, int frame)
        {
            LinkedListNode<int> node = m_List.First;
            Vector2Int tile = IdToPos(First);
            //int oldQuad;
            //while (physicalTexture.TileToQuadMapping.TryGetValue(tile, out oldQuad) && (pageTable.m_Pages[oldQuad].Payload.tileStatus == TileStatus.Loading || pageTable.m_Pages[oldQuad].Payload.ActiveFrame == frame))
            //{
            //    node = node.Next;
            //    tile = IdToPos(node.Value);
            //}
            //int oldMip = m_IdToMip[node.Value];

            //UnityEngine.Debug.Log("Request Tile");
            ////UnityEngine.Debug.Log(m_NumMip[0]);
            //UnityEngine.Debug.Log("old mip is " + oldMip);
            //UnityEngine.Debug.Log("first mip is" + m_IdToMip[First]);
            //UnityEngine.Debug.Log(m_MipEnd[0] == m_List.Last);



            //if (oldMip != -1 && oldMip != targetMip)
            //{
            //    m_NumMip[oldMip] = m_NumMip[oldMip] - 1;
            //    m_NumMip[targetMip] = m_NumMip[targetMip] + 1;
            //    if (m_NumMip[oldMip] == 0)
            //    {
            //        m_MipEnd[oldMip] = null;
            //    }
            //}

            //m_IdToMip[node.Value] = targetMip;

            SetActive(tile);

            return tile;
        }

        private Vector2Int IdToPos(int id)
        {
            return new Vector2Int(id % m_PhysicalSize.x, id / m_PhysicalSize.x);
        }

        private int PosToId(Vector2Int tile)
        {
            return (tile.y * m_PhysicalSize.x + tile.x);
        }
    }
}