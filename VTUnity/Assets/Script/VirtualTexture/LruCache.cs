using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VirtualTexture
{
    public class LruCache
    {
        private Dictionary<int, LinkedListNode<int>> m_Map = new Dictionary<int, LinkedListNode<int>>();

        private LinkedList<int> m_List = new LinkedList<int>();


        //保证最先替出mip最低的tile
        //每个mip list的结尾
        private LinkedListNode<int>[] m_MipEnd = default;

        //每个mip数量
        private int[] m_NumMip = default;

        //Tile 到 mip的映射
        private int[] m_IdToMip = default;

        private Vector2Int m_PhysicalSize = default;

        public int First { get { return m_List.First.Value; } }


        public LruCache(int maxMip, int physicalSizeX, int physicalSizeY)
        {
            m_MipEnd = new LinkedListNode<int>[maxMip + 1];
            m_MipEnd[0] = m_List.Last;
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
            int mip = m_IdToMip[id];
            LinkedListNode<int> node = null;
            if (!m_Map.TryGetValue(id, out node))
                return false;

            if (node == m_MipEnd[mip])
            {
                return true;
            }
            m_List.Remove(node);

            m_List.AddAfter(m_MipEnd[mip],node);

            return true;
        }

        public Vector2Int RequestTile(int targetMip)
        {
            int oldMip = m_IdToMip[First];
            Vector2Int tile = IdToPos(First);

            if (oldMip != -1 && oldMip != targetMip)
            {
                m_NumMip[oldMip]--;
                if (m_NumMip[oldMip] == 0)
                {
                    if (oldMip == 0)
                    {
                        m_MipEnd[oldMip] = m_List.Last;
                    }
                    else
                    {
                        m_MipEnd[oldMip] = null;
                    }
                }
            }

            if(m_MipEnd[targetMip] == null)
            {
                int tempMip = targetMip - 1;
                while(tempMip >= 0)
                {
                    if(m_MipEnd[tempMip] != null)
                    {
                        m_MipEnd[targetMip] = m_MipEnd[tempMip];
                        break;
                    }
                    tempMip--;
                }
                if (tempMip < 0)
                {
                    m_MipEnd[targetMip] = m_List.Last;
                }
            }

            m_NumMip[targetMip]++;
            m_IdToMip[First] = targetMip;

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