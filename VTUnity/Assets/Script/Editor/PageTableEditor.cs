using UnityEditor;
using UnityEngine;
using System;

namespace VirtualTexture
{
	[CustomEditor(typeof(PageTable))]
	public class PageTableEditor : EditorBase
	{
		protected override void OnPlayingInspectorGUI()
		{
			var renderer = (PageTable)target;
		}
	}
}