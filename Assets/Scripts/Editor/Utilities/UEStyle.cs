﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
    public static class UEGUIStyle_Window
    {
        public static GUIStyle m_TitleLabel => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
        public static GUIStyle m_ErrorLabel => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.BoldAndItalic, richText = true };
    }
    public static class UEGUIStyle_SceneView
    {
        public static GUIStyle m_NormalLabel => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,fontSize=14, fontStyle = FontStyle.Normal};
        public static GUIStyle m_TitleLabel => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,fontSize=20, fontStyle = FontStyle.Bold };
        public static GUIStyle m_ErrorLabel => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 24, fontStyle = FontStyle.BoldAndItalic, richText = true };
    }

    public static class ETime
    {
        public static double m_Cur;

        public static float deltaTime
        {
            get
            {
                var last = m_Cur;
                m_Cur = EditorApplication.timeSinceStartup;
                return Mathf.Max(0, (float)(m_Cur-last));
            }
        }
    }
}