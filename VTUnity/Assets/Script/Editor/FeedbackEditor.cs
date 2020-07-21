﻿//using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using System;

namespace VirtualTexture
{
	[CustomEditor(typeof(Feedback))]
	public class FeedbackRendererEditor : EditorBase
	{
		protected override void OnPlayingInspectorGUI()
		{
			var renderer = (Feedback)target;
			DrawTexture(renderer.TargetTexture, "Feedback Texture");
		}
	}
}