using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace VirtualTexture
{
    public class PhysicalTexture : MonoBehaviour
    {
        // Start is called before the first frame update

        [SerializeField]
        private Vector2Int m_PhysicalTextureSize = default;

        private int m_TileSize = 256;

        private int m_PaddingSize = 4;

        private Shader m_TileGenerationShader = default;

        public int testint = 0;
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public int testtest(int i)
        {
            testint = i + 1;
            return testint;
        }
    }
}
