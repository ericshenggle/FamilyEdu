using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using GameCreator.Core;
using GameCreator.Variables;
using UnityEngine.UI;
using NetWorkManage;
using GameCreator.Characters;
using GameCreator.Localization;
using UnityEngine.Audio;
using GameCreator.Messages;
using NJG.PUN;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MyTools.Actions
{

    public class ActionMySimpleMessage : IAction
    {

        public AudioClip audioClip;

        public Color color = Color.white;
        public float time = 2.0f;

        private bool forceStop = false;

        public StringProperty roomName = new StringProperty();

        public StringProperty actionString = new StringProperty();

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
		{
            if (this.audioClip != null)
            {
                AudioMixerGroup voiceMixer = DatabaseGeneral.Load().voiceAudioMixer;
                AudioManager.Instance.PlayVoice(this.audioClip, 0f, 1f, voiceMixer);
            }
            string rname = this.roomName.GetValue(target);
            string raction = this.actionString.GetValue(target);

			SimpleMessageManager.Instance.ShowText(raction + rname, this.color);

            float waitTime = Time.time + this.time;
			WaitUntil waitUntil = new WaitUntil(() => Time.time > waitTime || this.forceStop);
			yield return waitUntil;

			if (this.audioClip != null) AudioManager.Instance.StopVoice(this.audioClip);
			SimpleMessageManager.Instance.HideText();
			yield return 0;
		}

        public override void Stop()
        {
            this.forceStop = true;
        }

#if UNITY_EDITOR
        public static new string NAME = "Messages/My Simple Message";
        private const string NODE_TITLE = "Show message: {0}{1}";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spAudioClip;
        private SerializedProperty spRoomName;
        private SerializedProperty spActionString;
        private SerializedProperty spColor;
        private SerializedProperty spTime;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE, actionString, roomName
            );
        }

        protected override void OnEnableEditorChild()
        {
            this.spAudioClip = this.serializedObject.FindProperty("audioClip");
            this.spRoomName = this.serializedObject.FindProperty("roomName");
            this.spActionString = this.serializedObject.FindProperty("actionString");
            this.spColor = this.serializedObject.FindProperty("color");
            this.spTime = this.serializedObject.FindProperty("time");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spRoomName);
            EditorGUILayout.PropertyField(this.spActionString);
            EditorGUILayout.PropertyField(this.spAudioClip);
            EditorGUILayout.PropertyField(this.spColor);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spTime);

            if (this.spAudioClip.objectReferenceValue != null)
            {
                AudioClip clip = (AudioClip)this.spAudioClip.objectReferenceValue;
                if (!Mathf.Approximately(clip.length, this.spTime.floatValue))
                {
                    Rect btnRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.miniButton);
                    btnRect = new Rect(
                        btnRect.x + EditorGUIUtility.labelWidth,
                        btnRect.y,
                        btnRect.width - EditorGUIUtility.labelWidth,
                        btnRect.height
                    );

                    if (GUI.Button(btnRect, "Use Audio Length", EditorStyles.miniButton))
                    {
                        this.spTime.floatValue = clip.length;
                    }
                }
            }

            this.serializedObject.ApplyModifiedProperties();
        }
#endif
    }
}

