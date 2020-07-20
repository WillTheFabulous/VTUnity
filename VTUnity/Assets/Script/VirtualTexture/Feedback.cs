using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feedback : MonoBehaviour
{

    private Shader m_FeedbackShader = default;

    private Camera m_FeedbackCamera = default;
    
    public RenderTexture TargetTexture { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        InitCamera();
    }

    private void OnPreCull()
    {

    }

    private void OnPreRender()
    {
     
    }

    private void OnPostRender()
    {
        if (TargetTexture == null)
            return;

    }

    private void InitCamera()
    {
        //设置单独的 feedback camera
        m_FeedbackCamera = GetComponent<Camera>();
        if (m_FeedbackCamera == null)
            m_FeedbackCamera = gameObject.AddComponent<Camera>();


        var mainCamera = Camera.main;
        m_FeedbackCamera.CopyFrom(mainCamera);
        
        
        m_FeedbackCamera.allowHDR = false;
        m_FeedbackCamera.allowMSAA = false;
        m_FeedbackCamera.renderingPath = RenderingPath.Forward;

        //白色背景
        m_FeedbackCamera.clearFlags = CameraClearFlags.Color;
        m_FeedbackCamera.backgroundColor = Color.white;


        m_FeedbackCamera.SetReplacementShader(m_FeedbackShader, "VirtualTextureType");


        
    }


}
