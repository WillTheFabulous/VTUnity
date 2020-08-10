using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VirtualTexture
{
	public abstract class EditorBase : Editor
	{
		public override void OnInspectorGUI()
		{
			
			if (Application.isPlaying)
			{
				OnPlayingInspectorGUI();
			}
			else
			{
				DrawDefaultInspector();
				serializedObject.ApplyModifiedProperties();
			}
		}

		protected abstract void OnPlayingInspectorGUI();



		//[Conditional("ENABLE_DEBUG_TEXTURE")]
		protected void DrawTexture(Texture texture, string label = null, int mip = -1)
		{
			if (texture == null)
				//UnityEngine.Debug.Log("is null");
				return;

			EditorGUILayout.Space();

			int width = texture.width;
			int height = texture.height;

			if(mip != -1)
            {
				width /= (int)Math.Pow(2, (double)mip);
				height /= (int)Math.Pow(2, (double)mip);

			}
			if (!string.IsNullOrEmpty(label))
			{
				EditorGUILayout.LabelField(label);
				EditorGUILayout.LabelField(string.Format("    Size: {0} X {1}", width, height));
			}
			else
			{
				EditorGUILayout.LabelField(string.Format("Size: {0} X {1}", width, height));
			}
			//UnityEngine.Debug.Log("is null");
			EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetAspectRect((float)width / height), texture, null, ScaleMode.StretchToFill, 0, mip, UnityEngine.Rendering.ColorWriteMask.All);
		}
	}
}