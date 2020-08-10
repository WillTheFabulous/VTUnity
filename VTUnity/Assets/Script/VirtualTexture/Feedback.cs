using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineInternal;

public class Feedback : MonoBehaviour
{

    public event Action<Texture2D> OnFeedbackReadComplete;

    [SerializeField]
    private Shader m_FeedbackShader = default;

    private Camera m_FeedbackCamera = default;

    private Queue<AsyncGPUReadbackRequest> m_ReadbackRequests = new Queue<AsyncGPUReadbackRequest>();
    
    public RenderTexture TargetTexture { get; private set; }

    //CPU SIDE FEEDBACK TEXTURE
    public Texture2D m_ReadbackTexture;

    [SerializeField]
    private Terrain DemoTerrain = default;

    // Start is called before the first frame update
    void Start()
    {
        InitCamera();
    }

    private void OnPreCull()
    {
        //CHECK FOR REQUESTS LIST AND UPDATE TEXTURE FROM LAST FRAME
        bool complete = false;

        if (TargetTexture != null)
        {
            var width = TargetTexture.width;
            var height = TargetTexture.height;

            if(m_ReadbackTexture == null || m_ReadbackTexture.width != width || m_ReadbackTexture.height != height)
            {
                m_ReadbackTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                m_ReadbackTexture.filterMode = FilterMode.Point;
                m_ReadbackTexture.wrapMode = TextureWrapMode.Clamp;
            }

            while (m_ReadbackRequests.Count > 0)
            {
                var req = m_ReadbackRequests.Peek();

                if (req.hasError)
                {
                    m_ReadbackRequests.Dequeue();
                }
                else if (req.done)
                {
                    m_ReadbackTexture.GetRawTextureData<Color32>().CopyFrom(req.GetData<Color32>());
                    complete = true;
                    m_ReadbackRequests.Dequeue();
                }
                else
                {
                    break;
                }
            }

            if (complete)
            {   /**
                Color color = m_ReadbackTexture.GetPixel(250,50);
                print(color.r * 255);
                print(color.g * 255);
                print(color.b * 255);
                print(color.a);
                **/
                OnFeedbackReadComplete?.Invoke(m_ReadbackTexture);
            }
        }


        //NEW FRAME CONFIGURATION

        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;
        

        if (TargetTexture == null)
        {
            TargetTexture = new RenderTexture(mainCamera.pixelWidth, mainCamera.pixelHeight, 0);
            TargetTexture.useMipMap = false;
            TargetTexture.wrapMode = TextureWrapMode.Clamp;
            TargetTexture.filterMode = FilterMode.Point;
            m_FeedbackCamera.targetTexture = TargetTexture;


            Vector3 terrainSize = DemoTerrain.terrainData.size;
            Vector3 terrainTransform = DemoTerrain.GetPosition();

            Shader.SetGlobalVector("_TERRAINPOS", terrainTransform);
            Shader.SetGlobalVector("_TERRAINSIZE", terrainSize);
            

        }
        m_FeedbackCamera.targetTexture = TargetTexture;


        //var mainCamera = Camera.main;
        m_FeedbackCamera.transform.position = mainCamera.transform.position;
        m_FeedbackCamera.transform.rotation = mainCamera.transform.rotation;
        m_FeedbackCamera.cullingMask = mainCamera.cullingMask;
        m_FeedbackCamera.projectionMatrix = mainCamera.projectionMatrix;
        m_FeedbackCamera.fieldOfView = mainCamera.fieldOfView;
        m_FeedbackCamera.nearClipPlane = mainCamera.nearClipPlane;
        m_FeedbackCamera.farClipPlane = mainCamera.farClipPlane;
        m_FeedbackCamera.SetReplacementShader(m_FeedbackShader, "VirtualTextureType");


    }



    private void OnPostRender()
    {
        if (TargetTexture == null)
            return;

        // Readback
        //TODO: DOWNSCALE THE TEXTURE? 338847 pixels ??????????
        
        var request = AsyncGPUReadback.Request(TargetTexture);
        m_ReadbackRequests.Enqueue(request);

    }

    private void InitCamera()
    {
        //设置单独的 feedback camera
        m_FeedbackCamera = GetComponent<Camera>();
        if (m_FeedbackCamera == null)
            m_FeedbackCamera = gameObject.AddComponent<Camera>();
        
        m_FeedbackCamera.allowHDR = false;
        m_FeedbackCamera.allowMSAA = false;
        m_FeedbackCamera.renderingPath = RenderingPath.Forward;

        //白色背景
        m_FeedbackCamera.clearFlags = CameraClearFlags.Color;
        m_FeedbackCamera.backgroundColor = Color.white;       
    }


}
