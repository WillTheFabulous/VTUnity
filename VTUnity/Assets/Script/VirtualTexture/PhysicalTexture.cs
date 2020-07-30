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
        private Vector2Int m_PhysicalTextureSize = new Vector2Int(5, 7);

        public Vector2Int PhysicalTextureSize { get { return m_PhysicalTextureSize; } }

        [SerializeField]
        private int m_TileSize = 256;

        [SerializeField]
        private int m_PaddingSize = 4;

        [SerializeField]
        private int m_NumTextureType = 2;

        //用于存储混好的color normal 等等
        public RenderTexture[] PhysicalTextures { get; private set; }

        
        void Start()
        {
            PhysicalTextures = new RenderTexture[m_NumTextureType]; 
            for(int i = 0; i < m_NumTextureType; i++)
            {
                PhysicalTextures[i] = new RenderTexture(m_PhysicalTextureSize.x * (m_TileSize + m_PaddingSize), m_PhysicalTextureSize.y * (m_TileSize + m_PaddingSize), 0);
                PhysicalTextures[i].useMipMap = false;
                PhysicalTextures[i].wrapMode = TextureWrapMode.Clamp;
                Shader.SetGlobalTexture("_PhysicalTexture" + i, PhysicalTextures[i]);
            }

            Shader.SetGlobalInt("_TILESIZE", m_TileSize);
            Shader.SetGlobalInt("_PADDINGSIZE", m_PaddingSize);
            Shader.SetGlobalVector("_PHYSICALTEXTURESIZE", new Vector2(m_PhysicalTextureSize.x, m_PhysicalTextureSize.y));

        }
        
        // Update is called once per frame
        void Update()
        {

        }

   
    }
}
