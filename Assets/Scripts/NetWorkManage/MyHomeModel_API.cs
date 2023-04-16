using System;
using System.Collections;
using System.Collections.Generic;
using GameCreator.Characters;
using GameCreator.Core;
using GameCreator.Variables;
using MyTools;
using NetWorkManage;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using System.IO;
using EasyBuildSystem.Features.Runtime.Buildings.Manager.Saver;
using UnityEngine.Networking;

namespace NetWorkManage
{


    public class MyHomeModel_API : MySingleton<MyHomeModel_API>
    {

        public static string requestUrl = RequestSender.url + "home-user/unity/";

        public partial class ResponseData
        {
            [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
            public long? Code { get; set; }

            [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
            public string Data { get; set; }

            [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
            public string Message { get; set; }

            [JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]
            public bool? Success { get; set; }
        }

        public partial class ResponseData
        {
            public static ResponseData FromJson(string json) => JsonConvert.DeserializeObject<ResponseData>(json, Converter.Settings);
        }

        public class Serialize
        {
            public static string ToJson(ResponseData self) => JsonConvert.SerializeObject(self, Converter.Settings);
        }

        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }

        public ResponseData myHomeModelData;

        private string text;

        public bool isCompletedDownLoad;

        public bool isCompletedUpLoad;

        public string path
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    return "sceneData_Save";
                }
                else
                {
                    return Application.persistentDataPath + "/sceneData_Save.txt";
                }
            }
            set
            {
                path = value;
            }
        }

        void Start()
        {
            isCompletedUpLoad = false;
            isCompletedDownLoad = false;
        }

        void OnApplicationQuit()
        {
            if (BuildingSaver.Instance)
            {
                BuildingSaver.Instance.OnApplicationQuit();
                updateUserModelTextRequest();
            }
        }

        public void getUserModelTextRequest()
        {
            string requestData = RequestSender.getUrlParams(new Dictionary<string, string>{
            {"id", MyUser_API.Instance.myUserData.Data.Id.ToString()}
        });
            RequestSender.Instance.SendGETRequest(requestData, requestUrl + "text", (string responseContent) =>
            {
                MyDebug.Log(responseContent);
                this.myHomeModelData = ResponseData.FromJson(responseContent);
                if (this.myHomeModelData.Code != 200)
                {
                    MyDebug.LogError("Get UserModelText failed!");
                    return;
                }
                StartCoroutine(writeTextInSaver(this.myHomeModelData.Data));
                isCompletedDownLoad = true;
            });
        }

        public void updateUserModelTextRequest()
        {
            StartCoroutine(readTextInSaver());
            string requestData = RequestSender.getUrlParams(new Dictionary<string, string>{
            {"id", MyUser_API.Instance.myUserData.Data.Id.ToString()},
            {"text", UnityWebRequest.EscapeURL(this.text)}
        });
            RequestSender.Instance.SendPostRequest(requestData, requestUrl + "update/text", (string responseContent) =>
            {
                MyDebug.Log(responseContent);
                this.myHomeModelData = ResponseData.FromJson(responseContent);
                if (this.myHomeModelData.Code != 200)
                {
                    MyDebug.LogError("Update UserModelText failed!");
                    return;
                }
                isCompletedUpLoad = true;
            });
        }
        bool m_Save;
        IEnumerator writeTextInSaver(string text)
        {
            if (m_Save)
            {
                yield break;
            }

            m_Save = true;

            if (Application.platform != RuntimePlatform.Android)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                File.AppendAllText(path, text);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {

                PlayerPrefs.SetString(path, text);
                PlayerPrefs.Save();
            }

            m_Save = false;
            yield break;
        }

        bool m_Loading;
        IEnumerator readTextInSaver()
        {
            this.text = "";

            if (m_Loading)
            {
                yield break;
            }

            m_Loading = true;

            if (!File.Exists(path))
            {
                m_Loading = false;
                yield break;
            }
            if (Application.platform != RuntimePlatform.Android)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        this.text = File.ReadAllText(path);
                    }
                    catch (Exception ex)
                    {
                        MyDebug.LogError(ex.ToString());
                    }
                }
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                this.text = PlayerPrefs.GetString(path);
            }

            yield break;
        }
    }
}