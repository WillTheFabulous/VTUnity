using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VirtualTexture
{
    public enum TileStatus
    {
        Uninitialized,
        Loading,
        LoadingComplete,
    };
    public class PhysicalTileInfo
    {
        public Vector2Int TileIndex = default;

        public int ActiveFrame;

        public TileStatus tileStatus = TileStatus.Uninitialized;
       
    }
}
