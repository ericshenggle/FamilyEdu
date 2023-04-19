/// <summary>
/// Project : Easy Build System
/// Class : StandaloneInputHandlerEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;
using EasyBuildSystem.Features.Editor.Window;

namespace Home.Editor
{
    [CustomEditor(typeof(MyStandaloneInputHandler), true)]
    public class MyStandaloneInputHandlerEditor : UnityEditor.Editor
    {
        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}