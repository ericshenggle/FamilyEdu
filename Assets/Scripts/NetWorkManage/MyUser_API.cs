using System;
using System.Collections.Generic;

using System.Globalization;
using MyTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.UI;

namespace NetWorkManage
{
    public class MyUser_API : MySingleton<MyUser_API>
    {
        public string getUrl = RequestSender.url + "home-user/user/info";

        public string updateUrl = RequestSender.url + "home-user/user/update";

        /// <summary>
        /// ResponseWrapperUserInfo
        /// </summary>
        public partial class ResponseData
        {
            [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
            public long? Code { get; set; }

            [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
            public UserInfo Data { get; set; }

            [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
            public string Message { get; set; }

            [JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]
            public bool? Success { get; set; }
        }

        /// <summary>
        /// UserInfo
        /// </summary>
        public class UserInfo
        {
            [JsonProperty("age", NullValueHandling = NullValueHandling.Ignore)]
            public long? Age { get; set; }

            [JsonProperty("avatarUrl", NullValueHandling = NullValueHandling.Ignore)]
            public string AvatarUrl { get; set; }

            [JsonProperty("createTime", NullValueHandling = NullValueHandling.Ignore)]
            public DateTimeOffset? CreateTime { get; set; }

            [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
            public string Description { get; set; }

            [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
            public string Email { get; set; }

            [JsonProperty("favCount", NullValueHandling = NullValueHandling.Ignore)]
            public long? FavCount { get; set; }

            [JsonProperty("homeId", NullValueHandling = NullValueHandling.Ignore)]
            public long? HomeId { get; set; }

            [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
            public long? Id { get; set; }

            [JsonProperty("likeCount", NullValueHandling = NullValueHandling.Ignore)]
            public long? LikeCount { get; set; }

            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }

            [JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
            public string Phone { get; set; }

            [JsonProperty("relativeType", NullValueHandling = NullValueHandling.Ignore)]
            public RelativeType? RelativeType { get; set; }

            [JsonProperty("sex", NullValueHandling = NullValueHandling.Ignore)]
            public string Sex { get; set; }

            [JsonProperty("updateTime", NullValueHandling = NullValueHandling.Ignore)]
            public DateTimeOffset? UpdateTime { get; set; }
        }

        public enum RelativeType { Aunt, Brother, Daughter, Father, Grandfather, Grandmother, Mother, None, Other, Sister, Son, Uncle };

        public string[] show_RelativeType = new string[] { "", "", "", "", "", "", "", "", "", "", "", "" };

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
                RelativeTypeConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }

        internal class RelativeTypeConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(RelativeType) || t == typeof(RelativeType?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                switch (value)
                {
                    case "AUNT":
                        return RelativeType.Aunt;
                    case "BROTHER":
                        return RelativeType.Brother;
                    case "DAUGHTER":
                        return RelativeType.Daughter;
                    case "FATHER":
                        return RelativeType.Father;
                    case "GRANDFATHER":
                        return RelativeType.Grandfather;
                    case "GRANDMOTHER":
                        return RelativeType.Grandmother;
                    case "MOTHER":
                        return RelativeType.Mother;
                    case "NONE":
                        return RelativeType.None;
                    case "OTHER":
                        return RelativeType.Other;
                    case "SISTER":
                        return RelativeType.Sister;
                    case "SON":
                        return RelativeType.Son;
                    case "UNCLE":
                        return RelativeType.Uncle;
                }
                throw new Exception("Cannot unmarshal type RelativeType");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (RelativeType)untypedValue;
                switch (value)
                {
                    case RelativeType.Aunt:
                        serializer.Serialize(writer, "AUNT");
                        return;
                    case RelativeType.Brother:
                        serializer.Serialize(writer, "BROTHER");
                        return;
                    case RelativeType.Daughter:
                        serializer.Serialize(writer, "DAUGHTER");
                        return;
                    case RelativeType.Father:
                        serializer.Serialize(writer, "FATHER");
                        return;
                    case RelativeType.Grandfather:
                        serializer.Serialize(writer, "GRANDFATHER");
                        return;
                    case RelativeType.Grandmother:
                        serializer.Serialize(writer, "GRANDMOTHER");
                        return;
                    case RelativeType.Mother:
                        serializer.Serialize(writer, "MOTHER");
                        return;
                    case RelativeType.None:
                        serializer.Serialize(writer, "NONE");
                        return;
                    case RelativeType.Other:
                        serializer.Serialize(writer, "OTHER");
                        return;
                    case RelativeType.Sister:
                        serializer.Serialize(writer, "SISTER");
                        return;
                    case RelativeType.Son:
                        serializer.Serialize(writer, "SON");
                        return;
                    case RelativeType.Uncle:
                        serializer.Serialize(writer, "UNCLE");
                        return;
                }
                throw new Exception("Cannot marshal type RelativeType");
            }

            public static readonly RelativeTypeConverter Singleton = new RelativeTypeConverter();
        }

        public ResponseData myUserData;

        public MyHome_API myHome_API;

        public bool isClassroom = false;
        public Text m_name;
        public Text m_age;
        public Text m_email;
        public Text m_sex;
        public Text m_phone;
        public Text m_relativeTag;
        public Text m_likeCount;
        public Text m_favCount;
        public Image m_avatar;

        public MyCharacterModel_API myModel_API;

        void Start()
        {
            if (!isClassroom)
            {
                this.myHome_API = gameObject.GetComponent<MyHome_API>();
            }
            this.myModel_API = gameObject.GetComponent<MyCharacterModel_API>();
            if (RequestSender.Instance != null)
            {
                this.getUserInfoRequest(RequestSender.Instance.UserId);
            }
        }


        public void getUserInfoRequest(long id)
        {
            string requestData = RequestSender.getUrlParams(new Dictionary<string, string>{
            {"userId", id.ToString()}
        });
            RequestSender.Instance.SendGETRequest(requestData, getUrl, (string responseContent) =>
            {
                MyDebug.Log(responseContent);
                this.myUserData = ResponseData.FromJson(responseContent);
                if (this.myUserData.Code != 200)
                {
                    MyDebug.LogError("Get UserInfo failed!");
                    return;
                }
                if (!isClassroom)
                {
                    if (this.myUserData.Data.HomeId != null && this.myHome_API != null)
                    {
                        this.myHome_API.getHomeInfoRequest();
                    }
                    this.m_name.text += this.myUserData.Data.Name;
                    if (this.myUserData.Data.Age != null && this.myUserData.Data.Age != 0)
                    {
                        this.m_age.text += this.myUserData.Data.Age.ToString();
                    }
                    this.m_email.text += this.myUserData.Data.Email;
                    this.m_sex.text += this.myUserData.Data.Sex;
                    this.m_phone.text += this.myUserData.Data.Phone;
                    this.m_relativeTag.text += this.show_RelativeType[(int)this.myUserData.Data.RelativeType];
                    this.m_likeCount.text += this.myUserData.Data.LikeCount.ToString();
                    this.m_favCount.text += this.myUserData.Data.FavCount.ToString();
                    this.getAvatar();
                }
                this.myModel_API.getUserModelIdRequest();
            });
        }

        public void updateUserInfoRequest()
        {
            string requestData = RequestSender.getUrlParams(new Dictionary<string, string>
            {

            });
            RequestSender.Instance.SendPostRequest(requestData, updateUrl, (string responseContent) =>
            {
                MyDebug.Log(responseContent);
                this.myUserData = ResponseData.FromJson(responseContent);
                if (this.myUserData.Code != 200)
                {
                    MyDebug.LogError("Update UserInfo failed!");
                    return;
                }
            });
        }

        public void getAvatar()
        {
            RequestSender.Instance.SendAvatarRequest(this.myUserData.Data.AvatarUrl, (Sprite sprite) =>
            {
                if (sprite == null)
                {
                    MyDebug.LogError("Get Avatar failed!");
                    return;
                }
                this.m_avatar.sprite = sprite;
            });
        }


    }
}
