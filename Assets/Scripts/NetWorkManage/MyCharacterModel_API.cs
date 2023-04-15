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

namespace NetWorkManage
{
    [RequireComponent(typeof(MyUser_API))]
    public class MyCharacterModel_API : MonoBehaviour
    {
        public string requestUrl = RequestSender.url + "home-user/unity/";

        public partial class ResponseData
        {
            [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
            public long? Code { get; set; }

            [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
            public long? Data { get; set; }

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
        [SerializeField] public LocalVariables m_selectedModel;
        [SerializeField] public LocalVariables m_isChild;

        [SerializeField] public ListVariables m_characterModels;

        [SerializeField] public List<GameObject> m_ManModels = new List<GameObject>();
        [SerializeField] public List<GameObject> m_WomanModels = new List<GameObject>();
        [SerializeField] public List<GameObject> m_BoyModels = new List<GameObject>();
        [SerializeField] public List<GameObject> m_GirlModels = new List<GameObject>();

        public ResponseData myModelData;


        public void getUserModelIdRequest()
        {
            string requestData = RequestSender.getUrlParams(new Dictionary<string, string>{
            {"id", MyUser_API.Instance.myUserData.Data.Id.ToString()}
        });
            RequestSender.Instance.SendGETRequest(requestData, requestUrl + "model", (string responseContent) =>
            {
                MyDebug.Log(responseContent);
                this.myModelData = ResponseData.FromJson(responseContent);
                if (this.myModelData.Code != 200)
                {
                    MyDebug.LogError("Get UserModelId failed!");
                    return;
                }
                this.UpdateListVariable((int)this.myModelData.Data);
            });
        }

        public void updateUserModelIdRequest()
        {
            int m_modelId = 0;
            if (m_selectedModel != null)
            {
                m_modelId = Mathf.FloorToInt(m_selectedModel.Get("SelectedModel").Get<float>());
            }
            string requestData = RequestSender.getUrlParams(new Dictionary<string, string>{
            {"id", MyUser_API.Instance.myUserData.Data.Id.ToString()},
            {"modelId", m_modelId.ToString()},
        });
            RequestSender.Instance.SendPostRequest(requestData, requestUrl + "update/model", (string responseContent) =>
            {
                MyDebug.Log(responseContent);
                this.myModelData = ResponseData.FromJson(responseContent);
                if (this.myModelData.Code != 200)
                {
                    MyDebug.LogError("Update UserModelId failed!");
                    return;
                }
            });
        }

        public void updateUserModelIdRequest(int index)
        {
            string requestData = RequestSender.getUrlParams(new Dictionary<string, string>{
            {"id", MyUser_API.Instance.myUserData.Data.Id.ToString()},
            {"modelId", index.ToString()},
        });
            RequestSender.Instance.SendGETRequest(requestData, requestUrl + "update/model", (string responseContent) =>
            {
                MyDebug.Log(responseContent);
                this.myModelData = ResponseData.FromJson(responseContent);
                if (this.myModelData.Code != 200)
                {
                    MyDebug.LogError("Update UserModelId failed!");
                    return;
                }
            });
        }

        public void UpdateListVariable(int index)
        {
            MyUser_API.ResponseData userInfo = MyUser_API.Instance.myUserData;
            List<GameObject> t_characterModels = new List<GameObject>();
            bool isChild = true;

            switch (userInfo.Data.Sex)
            {
                case "男":
                    if (userInfo.Data.RelativeType == MyUser_API.RelativeType.Father ||
                        userInfo.Data.RelativeType == MyUser_API.RelativeType.Grandfather ||
                        userInfo.Data.RelativeType == MyUser_API.RelativeType.Uncle || userInfo.Data.Age > 18)
                    {
                        foreach (GameObject m in m_ManModels)
                        {
                            t_characterModels.Add(m);
                        }
                        isChild = false;
                    }
                    else if (userInfo.Data.RelativeType == MyUser_API.RelativeType.Brother ||
                            userInfo.Data.RelativeType == MyUser_API.RelativeType.Son || userInfo.Data.Age <= 18)
                    {
                        foreach (GameObject m in m_BoyModels)
                        {
                            t_characterModels.Add(m);
                        }
                    }
                    else
                    {
                        foreach (GameObject m in m_ManModels)
                        {
                            t_characterModels.Add(m);
                        }
                        foreach (GameObject m in m_BoyModels)
                        {
                            t_characterModels.Add(m);
                        }
                    }
                    break;
                case "女":
                    if (userInfo.Data.RelativeType == MyUser_API.RelativeType.Mother ||
                            userInfo.Data.RelativeType == MyUser_API.RelativeType.Grandmother ||
                            userInfo.Data.RelativeType == MyUser_API.RelativeType.Aunt || userInfo.Data.Age > 18)
                    {
                        foreach (GameObject m in m_WomanModels)
                        {
                            t_characterModels.Add(m);
                        }
                        isChild = false;
                    }
                    else if (userInfo.Data.RelativeType == MyUser_API.RelativeType.Daughter ||
                            userInfo.Data.RelativeType == MyUser_API.RelativeType.Sister || userInfo.Data.Age <= 18)
                    {
                        foreach (GameObject m in m_GirlModels)
                        {
                            t_characterModels.Add(m);
                        }
                    }
                    else
                    {
                        foreach (GameObject m in m_WomanModels)
                        {
                            t_characterModels.Add(m);
                        }
                        foreach (GameObject m in m_GirlModels)
                        {
                            t_characterModels.Add(m);
                        }
                    }
                    break;
                default:
                    if (userInfo.Data.Age <= 18)
                    {
                        foreach (GameObject m in m_BoyModels)
                        {
                            t_characterModels.Add(m);
                        }
                        foreach (GameObject m in m_GirlModels)
                        {
                            t_characterModels.Add(m);
                        }
                    }
                    else
                    {
                        foreach (GameObject m in m_ManModels)
                        {
                            t_characterModels.Add(m);
                        }

                        foreach (GameObject m in m_WomanModels)
                        {
                            t_characterModels.Add(m);
                        }
                        isChild = false;
                    }
                    break;
            }
            if (index > t_characterModels.Count)
            {
                index = 0;
                this.updateUserModelIdRequest(index);
            }
            CharacterAnimator targetCharAnim = GetComponentInChildren<CharacterAnimator>();
            targetCharAnim.ChangeModel(t_characterModels[index]);
            if (!isChild)
            {
                MyMouseLock myMouseLock = GetComponent<MyMouseLock>();
                if (myMouseLock != null)
                {
                    myMouseLock.setAdventureMotorTargetOffset(new Vector3(0, 1.5f, 0));
                }
            }

            if (m_isChild != null)
            {
                m_isChild.Get("isChild").Update(isChild);
            }
            if (m_characterModels != null)
            {
                for (int i = m_characterModels.variables.Count - 1; i >= 0; --i)
                {
                    m_characterModels.Remove(i);
                }
                foreach (GameObject m in t_characterModels)
                {
                    m_characterModels.Push(m);
                }
            }
            if (m_selectedModel != null)
            {
                m_selectedModel.Get("SelectedModel").Update(index);
            }

        }
    }
}