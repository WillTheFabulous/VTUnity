using System.Diagnostics;
using UnityEditor;
using UnityEngine;

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



		[Conditional("ENABLE_DEBUG_TEXTURE")]
		protected void DrawTexture(Texture texture, string label = null)
		{
			if (texture == null)
				return;

			EditorGUILayout.Space();
			if (!string.IsNullOrEmpty(label))
			{
				EditorGUILayout.LabelField(label);
				EditorGUILayout.LabelField(string.Format("    Size: {0} X {1}", texture.width, texture.height));
			}
			else
			{
				EditorGUILayout.LabelField(string.Format("Size: {0} X {1}", texture.width, texture.height));
			}

			EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetAspectRect((float)texture.width / texture.height), texture);
		}
	}
}