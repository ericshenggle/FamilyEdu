using System.Collections;
using System.Collections.Generic;
using MyTools;
using Newtonsoft.Json;
using UnityEngine;

namespace NetWorkManage
{
    public class MyChatGPT_API : MonoBehaviour
    {
        public partial class ResponseData
        {
            [JsonProperty("choices")]
            public Choice[] Choices { get; set; }
        }

        public class Choice
        {
            [JsonProperty("index")]
            public int Index { get; set; }
            [JsonProperty("message")]
            public Message Message { get; set; }
            [JsonProperty("finish_reason")]
            public string Finish_reason { get; set; }
        }

        public class Message
        {
            [JsonProperty("role")]
            public string Role { get; set; }
            [JsonProperty("content")]
            public string Content { get; set; }
        }

        public class Parameters
        {
            [JsonProperty("model")]
            public string Model { get; set; }
            [JsonProperty("messages")]
            public IList<Message> Messages { get; set; }
        }

        public partial class ResponseData
        {
            public static ResponseData FromJson(string json) => JsonConvert.DeserializeObject<ResponseData>(json);
        }

        public RequestSender requestSender;

        public ResponseData currentResponse;

        public void SendChatGPTRequest(string content)
        {
            Message message = new Message()
            {
                Role = "user",
                Content = content
            };
            Parameters parameters = new Parameters()
            {
                Model = "gpt-3.5-turbo",
                Messages = new List<Message> {
                message
            }
            };
            // 设置请求内容
            if (requestSender == null)
            {
                requestSender = FindAnyObjectByType<RequestSender>();
            }
            string requestData = JsonConvert.SerializeObject(parameters);
            requestSender.SendChatGPTRequest(requestData, RequestSender.openai_url, (string responseContent) =>
            {
                currentResponse = ResponseData.FromJson(responseContent);
                // 输出结果
                MyDebug.Log(currentResponse.Choices[0].Message.Content);
            });

        }
    }
}