using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using MyTools;

namespace NetWorkManage
{
    [RequireComponent(typeof(MyUser_API))]
    public class MyHome_API : MonoBehaviour
    {
        private string requestUrl = RequestSender.url + "home-user/home/info";

        /// <summary>
        /// ResponseWrapperHomeInfo
        /// </summary>
        public partial class ResponseData
        {
            [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
            public long? Code { get; set; }

            [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
            public HomeInfo Data { get; set; }

            [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
            public string Message { get; set; }

            [JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]
            public bool? Success { get; set; }
        }

        /// <summary>
        /// HomeInfo
        /// </summary>
        public partial class HomeInfo
        {
            [JsonProperty("adminIds", NullValueHandling = NullValueHandling.Ignore)]
            public long[] AdminIds { get; set; }

            [JsonProperty("avatarUrl", NullValueHandling = NullValueHandling.Ignore)]
            public string AvatarUrl { get; set; }

            [JsonProperty("createTime", NullValueHandling = NullValueHandling.Ignore)]
            public DateTimeOffset? CreateTime { get; set; }

            [JsonProperty("createUserId", NullValueHandling = NullValueHandling.Ignore)]
            public long? CreateUserId { get; set; }

            [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
            public string Description { get; set; }

            [JsonProperty("favCount", NullValueHandling = NullValueHandling.Ignore)]
            public long? FavCount { get; set; }

            [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
            public long? Id { get; set; }

            [JsonProperty("imageUrls", NullValueHandling = NullValueHandling.Ignore)]
            public string[] ImageUrls { get; set; }

            [JsonProperty("likeCount", NullValueHandling = NullValueHandling.Ignore)]
            public long? LikeCount { get; set; }

            [JsonProperty("memberIds", NullValueHandling = NullValueHandling.Ignore)]
            public long[] MemberIds { get; set; }

            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }

            [JsonProperty("open", NullValueHandling = NullValueHandling.Ignore)]
            public bool? Open { get; set; }

            [JsonProperty("updateTime", NullValueHandling = NullValueHandling.Ignore)]
            public DateTimeOffset? UpdateTime { get; set; }
        }

        public partial class ResponseData
        {
            public static ResponseData FromJson(string json) => JsonConvert.DeserializeObject<ResponseData>(json, Converter.Settings);
        }

        public static class Serialize
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

        public ResponseData myHome;

        public Text m_name;
        public Text m_creatorName;
        public Text m_memberCount;
        public Text m_isOpen;
        public Text m_likeCount;
        public Text m_favCount;

        public void getHomeInfoRequest()
        {
            string requestData = RequestSender.getUrlParams(new Dictionary<string, string>{
            {"homeId", MyUser_API.Instance.myUserData.Data.HomeId.ToString()}
        });
            RequestSender.Instance.SendGETRequest(requestData, requestUrl, (string responseContent) =>
            {
                MyDebug.Log(responseContent);
                this.myHome = ResponseData.FromJson(responseContent);
                if (this.myHome.Code != 200)
                {
                    MyDebug.LogError("Get HomeInfo failed!");
                    return;
                }
                this.m_name.text += this.myHome.Data.Name;
                if (this.myHome.Data.CreateUserId != null)
                {
                    this.getCreatorName((long)this.myHome.Data.CreateUserId);
                }
                this.m_memberCount.text += this.myHome.Data.MemberIds.Length.ToString();
                if (this.myHome.Data.Open != null)
                {
                    this.m_isOpen.text += ((bool)this.myHome.Data.Open) ? "是" : "否";
                }
                this.m_likeCount.text += this.myHome.Data.LikeCount.ToString();
                this.m_favCount.text += this.myHome.Data.FavCount.ToString();
            });

        }

        public void getCreatorName(long id)
        {
            string requestData = RequestSender.getUrlParams(new Dictionary<string, string>{
            {"userId", id.ToString()}
        });
            RequestSender.Instance.SendGETRequest(requestData, requestUrl, (string responseContent) =>
            {
                MyDebug.Log(responseContent);
                MyUser_API.ResponseData myCreator = MyUser_API.ResponseData.FromJson(responseContent);
                if (myCreator.Code != 200)
                {
                    MyDebug.LogError("Get HomeCreatorInfo failed!");
                }
                this.m_creatorName.text += myCreator.Data.Name;
            });
        }
    }
}