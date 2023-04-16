using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using MyTools;
using System.Linq;

namespace NetWorkManage
{

    public class MyUserCourse_API : MonoBehaviour
    {
        public static string requestUrl = RequestSender.url + "home-course/course/info/user";

        /// <summary>
        /// ResponseWrapperCourseInfo
        /// </summary>
        public partial class ResponseData
        {
            [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
            public long? Code { get; set; }

            [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
            public CourseInfo Data { get; set; }

            [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
            public string Message { get; set; }

            [JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]
            public bool? Success { get; set; }
        }

        /// <summary>
        /// CourseInfo
        /// </summary>
        public partial class CourseInfo
        {
            [JsonProperty("commentCount", NullValueHandling = NullValueHandling.Ignore)]
            public long? CommentCount { get; set; }

            [JsonProperty("coverUrl", NullValueHandling = NullValueHandling.Ignore)]
            public string CoverUrl { get; set; }

            [JsonProperty("createTime", NullValueHandling = NullValueHandling.Ignore)]
            public DateTimeOffset? CreateTime { get; set; }

            [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
            public string Description { get; set; }

            [JsonProperty("endTime", NullValueHandling = NullValueHandling.Ignore)]
            public DateTimeOffset? EndTime { get; set; }

            [JsonProperty("favCount", NullValueHandling = NullValueHandling.Ignore)]
            public long? FavCount { get; set; }

            [JsonProperty("homeId", NullValueHandling = NullValueHandling.Ignore)]
            public long? HomeId { get; set; }

            [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
            public long? Id { get; set; }

            [JsonProperty("issueIds", NullValueHandling = NullValueHandling.Ignore)]
            public long[] IssueIds { get; set; }

            [JsonProperty("likeCount", NullValueHandling = NullValueHandling.Ignore)]
            public long? LikeCount { get; set; }

            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }

            [JsonProperty("open", NullValueHandling = NullValueHandling.Ignore)]
            public bool? Open { get; set; }

            [JsonProperty("score", NullValueHandling = NullValueHandling.Ignore)]
            public double? Score { get; set; }

            [JsonProperty("scoreCount", NullValueHandling = NullValueHandling.Ignore)]
            public long? ScoreCount { get; set; }

            [JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
            public DateTimeOffset? StartTime { get; set; }

            [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
            public string Status { get; set; }

            [JsonProperty("studentIds", NullValueHandling = NullValueHandling.Ignore)]
            public long[] StudentIds { get; set; }

            [JsonProperty("teacherIds", NullValueHandling = NullValueHandling.Ignore)]
            public long[] TeacherIds { get; set; }

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

        public static void getUserCourseInfoRequestInClassroom(long id, UserConnectInitate userConnectInitate)
        {
            string requestData = RequestSender.getUrlParams(new Dictionary<string, string>{
            {"userId", id.ToString()}
        });
            RequestSender.Instance.SendGETRequest(requestData, requestUrl, (string responseContent) =>
            {
                MyDebug.Log(responseContent);
                ResponseData data = ResponseData.FromJson(responseContent);
                if (data.Code != 200)
                {
                    MyDebug.LogError("Get UserCourseInfo failed!");
                    return;
                }
                userConnectInitate.userCourseResponseEvent?.Invoke(data);
            });
        }

    }
}
