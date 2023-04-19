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
using Photon.Pun;
using Photon.Realtime;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MyTools.Actions
{

    public class ActionMyPhotonPlayerID : IAction
    {

        public IntProperty number = new IntProperty();

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.NetworkingClient.ChangeLocalID(number.GetInt(target));
                Player player = PhotonNetwork.LocalPlayer;
                if (player?.TagObject != null) {
                    GameObject owner = player.TagObject as GameObject;
                    PhotonView views = owner.GetComponentInChildren<PhotonView>(true);
                    views.OwnerActorNr = number.GetInt(target);
                }
            }
            return true;
        }

#if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/Actions/";

        public static new string NAME = "Photon/Set Local Player ID";
        //private const string NODE_TITLE = "Set Player ID {0} {1}";
        private const string NODE_TITLE = "Set Local Player ID {0}";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spNumber;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
        {
            //return string.Format(NODE_TITLE, this.source.target == TargetText.Target.String ? "to" : "from", this.source);
            return string.Format(NODE_TITLE, number);
        }

        protected override void OnEnableEditorChild()
        {
            this.spNumber = this.serializedObject.FindProperty("number");
        }

        protected override void OnDisableEditorChild()
        {
            this.spNumber = null;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spNumber);

            this.serializedObject.ApplyModifiedProperties();
        }

#endif
    }
}
