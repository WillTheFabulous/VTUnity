using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineInternal;
using VirtualTexture;

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

    private PageTable pageTable = default;

    // Start is called before the first frame update
    void Start()
    {
        pageTable = (PageTable)GetComponent(typeof(PageTable));
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

            /*if (SystemInfo.supportsAsyncGPUReadback)
            {

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
                {
                    OnFeedbackReadComplete?.Invoke(m_ReadbackTexture);
                }
            }
            else
            {*/

                RenderTexture.active = TargetTexture;
                Rect rectReadPicture = new Rect(0, 0, width, height);
                m_ReadbackTexture.ReadPixels(rectReadPicture, 0, 0);
                OnFeedbackReadComplete?.Invoke(m_ReadbackTexture);
            //}
        }


        //NEW FRAME CONFIGURATION

        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;
        

        if (TargetTexture == null)
        {
            TargetTexture = new RenderTexture(mainCamera.pixelWidth / 8, mainCamera.pixelHeight / 8, 24);
            TargetTexture.useMipMap = false;
            TargetTexture.wrapMode = TextureWrapMode.Clamp;
            TargetTexture.filterMode = FilterMode.Point;
            m_FeedbackCamera.targetTexture = TargetTexture;


            Vector3 terrainSize = DemoTerrain.terrainData.size;
            Vector3 terrainTransform = DemoTerrain.GetPosition();

            Shader.SetGlobalVector("_TERRAINPOS", terrainTransform);
            Shader.SetGlobalVector("_TERRAINSIZE", terrainSize);
            float feedbackBias =(float)Mathf.Floor( Mathf.Log((float)TargetTexture.width / (float)Mathf.Max((float)mainCamera.pixelWidth, (float)mainCamera.pixelHeight), 2.0f));
            Shader.SetGlobalFloat("_FEEDBACKBIAS", feedbackBias);


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
        /*if (SystemInfo.supportsAsyncGPUReadback)
        {
            var request = AsyncGPUReadback.Request(TargetTexture);
            m_ReadbackRequests.Enqueue(request);
        }*/

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
