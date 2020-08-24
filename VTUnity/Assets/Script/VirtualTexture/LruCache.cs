using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace VirtualTexture
{
    public class LruCache
    {
        private Dictionary<int, LinkedListNode<int>> m_Map = new Dictionary<int, LinkedListNode<int>>();
        private LinkedList<int> m_List = new LinkedList<int>();
        private LinkedListNode<int>[] m_MipStart = default;

        public int First { get { return m_List.First.Value; } }


        public LruCache(int maxMip)
        {
            m_MipStart = new LinkedListNode<int>[maxMip];
        }
        public void Add(int id)
        {
            if (m_Map.ContainsKey(id))
                return;

            var node = new LinkedListNode<int>(id);
            m_Map.Add(id, node);
            m_List.AddLast(node);
        }

        public bool SetActive(int id)
        {
            LinkedListNode<int> node = null;
            if (!m_Map.TryGetValue(id, out node))
                return false;

            m_List.Remove(node);
            m_List.AddLast(node);

            return true;
        }

        //public Vector2Int RequestTile(int mip)
        //{
        //    return null;
        //}
    }
}