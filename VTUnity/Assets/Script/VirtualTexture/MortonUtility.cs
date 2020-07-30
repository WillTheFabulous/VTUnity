using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualTexture
{
    public class MortonUtility
    {
        public static int MortonCode2(int x)
        {
            x &= 0x0000ffff;
            x = (x ^ (x << 8)) & 0x00ff00ff;
            x = (x ^ (x << 4)) & 0x0f0f0f0f;
            x = (x ^ (x << 2)) & 0x33333333;
            x = (x ^ (x << 1)) & 0x55555555;
            return x;
        }

        public static int ReverseMortonCode2(int x)
        {
            x &= 0x55555555;
            x = (x ^ (x >> 1)) & 0x33333333;
            x = (x ^ (x >> 2)) & 0x0f0f0f0f;
            x = (x ^ (x >> 4)) & 0x00ff00ff;
            x = (x ^ (x >> 8)) & 0x0000ffff;
            return x;
        }

        public static int getKey(int pageX, int pageY, int mip)
        {
            if (mip < 0)
            {
                mip = 0;
            }
            int result = MortonCode2(pageX) | (MortonCode2(pageY) << 1);
            result |= (mip << 24);

            return result;
        }

        public static int getMip(int key)
        {
            return key >> 24;
        }

        public static Vector2Int getPageXY(int key)
        {
            //mask out mip bits
            int mask = 0x00ffffff;
            key &= mask;
            int pageX = ReverseMortonCode2(key);
            int pageY = ReverseMortonCode2(key >> 1);

            return new Vector2Int(pageX, pageY);
        }
    }
}
