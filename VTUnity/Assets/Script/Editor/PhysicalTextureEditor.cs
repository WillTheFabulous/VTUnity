using UnityEditor;
using UnityEngine;
using System;


namespace VirtualTexture 
{
    [CustomEditor(typeof(PhysicalTexture))]
    public class PhysicalTextureEditor : EditorBase
    {
        // Start is called before the first frame update
        protected override void OnPlayingInspectorGUI()
        {
            
            var renderer = (PhysicalTexture)target;
            
            foreach (var texture in renderer.PhysicalTextures)
            {
                DrawTexture(texture, "Physical Texture", 0);
                DrawTexture(texture, "Physical Texture", 1);
            }
            
    
        }
    }
}
