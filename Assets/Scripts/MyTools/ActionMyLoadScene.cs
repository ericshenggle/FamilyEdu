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
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MyTools.SceneAction
{
    [AddComponentMenu("")]
    public class ActionMyLoadScene : IAction
    {
        public StringProperty sceneName = new StringProperty();
        public LoadSceneMode mode = LoadSceneMode.Single;

        public GameObject loadScreen;
        public Slider slider;
        public Text text;

        public bool withBuildingSaver = false;
        public bool upload = false;
        public bool player = false;

        public Vector3 playerPosition = Vector3.zero;
        public Quaternion playerRotation = Quaternion.identity;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
        {
            loadScreen.SetActive(true);
            if (player) {
                PlayerCharacter.ON_LOAD_SCENE_DATA = new Character.OnLoadSceneData(
                this.playerPosition,
                this.playerRotation
            );
            }

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(
                this.sceneName.GetValue(target),
                this.mode
            );
            if (withBuildingSaver && upload) {
                MyHomeModel_API.Instance.updateUserModelTextRequest();
            } else if (withBuildingSaver) {
                MyHomeModel_API.Instance.getUserModelTextRequest();
            }

            asyncOperation.allowSceneActivation = false;

            while (!asyncOperation.isDone)
            {
                slider.value = asyncOperation.progress;

                text.text = asyncOperation.progress * 100 + "%";

                if (asyncOperation.progress >= 0.9f)
                {
                    slider.value = 1;
                    text.text = "100%";
                    yield return new WaitForSeconds(0.5f);
                    if (withBuildingSaver && ((upload && MyHomeModel_API.Instance.isCompletedUpLoad) ||
                        (!upload && MyHomeModel_API.Instance.isCompletedDownLoad)))
                    {
                        asyncOperation.allowSceneActivation = true;
                    } else if (!withBuildingSaver) {
                        asyncOperation.allowSceneActivation = true;
                    }

                }

                yield return new WaitForSeconds(0.01f);
            }

            yield return asyncOperation;

        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

        public static new string NAME = "Scene/My Load Scene";
        private const string NODE_TITLE = "Load scene {0}{1}{2}";

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spSceneName;
        private SerializedProperty spMode;
        private SerializedProperty sploadScreen;
        private SerializedProperty spSlider;
        private SerializedProperty spText;
        private SerializedProperty spWithBuildingSaver;
        private SerializedProperty spUpload;
        private SerializedProperty spPlayer;
        private SerializedProperty spPosition;
        private SerializedProperty spRotation;

        // INSPECTOR METHODS: ---------------------------------------------------------------------

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.sceneName,
                (this.withBuildingSaver ? " (Use Building Saver)" : ""),
                (this.player ? " with Player" : "")
            );
        }

        protected override void OnEnableEditorChild()
        {
            this.spSceneName = this.serializedObject.FindProperty("sceneName");
            this.spMode = this.serializedObject.FindProperty("mode");
            this.sploadScreen = this.serializedObject.FindProperty("loadScreen");
            this.spSlider = this.serializedObject.FindProperty("slider");
            this.spText = this.serializedObject.FindProperty("text");
            this.spWithBuildingSaver = this.serializedObject.FindProperty("withBuildingSaver");
            this.spUpload = this.serializedObject.FindProperty("upload");

            this.spPlayer = this.serializedObject.FindProperty("player");
            this.spPosition = this.serializedObject.FindProperty("playerPosition");
            this.spRotation = this.serializedObject.FindProperty("playerRotation");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spSceneName);
            EditorGUILayout.PropertyField(this.spMode);
            EditorGUILayout.PropertyField(this.sploadScreen);
            EditorGUILayout.PropertyField(this.spSlider);
            EditorGUILayout.PropertyField(this.spText);
            EditorGUILayout.PropertyField(this.spWithBuildingSaver);
            EditorGUILayout.PropertyField(this.spUpload);

            EditorGUILayout.PropertyField(this.spPlayer);
            EditorGUILayout.PropertyField(this.spPosition);

            Vector3 rotation = EditorGUILayout.Vector3Field(
                this.spRotation.displayName,
                this.spRotation.quaternionValue.eulerAngles
            );

            this.spRotation.quaternionValue = Quaternion.Euler(rotation);


            this.serializedObject.ApplyModifiedProperties();
        }

#endif
    }
}