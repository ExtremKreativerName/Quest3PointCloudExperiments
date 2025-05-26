using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.EnvironmentDepth;
using UnityEngine;
using UnityEngine.UI;using Unity.XR.Oculus;
public class DepthMapVisualizer : MonoBehaviour
{
    //private static readonly int virtualDepthTextureID = Shader.PropertyToID("_CameraDepthTexture");
    
    [SerializeField]
        private RawImage _phisikalDepthImage;
        
  //  [SerializeField] private RawImage _virtualDepthImage;
    
    public static readonly int Meta_EnvironmentDepthTexture_ID = Shader.PropertyToID("_EnvironmentDepthTexture");



    private void ShowTextures()
    {
        var phisikalDepthTex = Shader.GetGlobalTexture(Meta_EnvironmentDepthTexture_ID);

        if (phisikalDepthTex != null) //&& virtualDepthTex != null)
        { 
          
            _phisikalDepthImage.texture = phisikalDepthTex; 
           // _virtualDepthImage.texture = virtualDepthTex;

        }
    }
    // Update is called once per frame
    void Update()
    {
        ShowTextures();
    }
}
