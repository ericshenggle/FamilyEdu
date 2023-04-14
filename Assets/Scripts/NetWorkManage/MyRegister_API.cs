using System;
using System.Collections.Generic;

using System.Globalization;
using GameCreator.Variables;
using MyTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.UI;

namespace NetWorkManage
{
    public class MyRegister_API : MonoBehaviour
    {
        private string requestUrl = RequestSender.url + "home-auth/register";

        // Start is called before the first frame updat
        public partial class ResponseData
        {
            [JsonProperty("code")]
            public long Code { get; set; }

            [JsonProperty("data")]
            public long Data { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("success")]
            public bool Success { get; set; }
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

        public ResponseData myRegisterData;

        [SerializeField] public InputField m_inputFieldName;
        [SerializeField] public InputField m_inputFieldPwd;
        [SerializeField] public InputField m_inputFieldCaptcha;
        [SerializeField] public Image m_imageCaptcha;
        [SerializeField] public Text m_showProcessing;
        [SerializeField] public LocalVariables loginStatus;

        void Start()
        {
            getCaptcha();
        }

        public void getCaptcha()
        {
            RequestSender.Instance.SendCaptchaRequest((Sprite sprite) =>
            {
                if (sprite == null)
                {
                    MyDebug.LogError("Get Captcha failed!");
                    return;
                }
                this.m_imageCaptcha.sprite = sprite;
                MyDebug.Log("KAPTCHA_SESSION_KEY: " + RequestSender.Instance.KAPTCHA_SESSION_KEY);
            });
        }

        public void registerUser()
        {
            this.m_showProcessing.text = "注册中...";
            SendLoginRequest(this.m_inputFieldName.text, this.m_inputFieldPwd.text, this.m_inputFieldCaptcha.text);
        }

        public void SendLoginRequest(string name, string pwd, string captcha)
        {
            string requestData = RequestSender.getUrlParams(new Dictionary<string, string>{
            {"username", name},
            {"password", pwd},
            {"captcha", captcha}
        });
            RequestSender.Instance.SendPostRequestWithCaptcha(requestData, requestUrl, (string responseContent) =>
            {
                MyDebug.Log(responseContent);
                this.myRegisterData = ResponseData.FromJson(responseContent);
                if (this.myRegisterData.Code == 400 || this.myRegisterData.Code == 500)
                {
                    this.m_showProcessing.text = this.myRegisterData.Message + "，请重新注册！！！";
                    return;
                }
                else if (this.myRegisterData.Code != 200)
                {
                    this.m_showProcessing.text = "网络问题，请重新注册！！！";
                    return;
                }
                this.m_showProcessing.text = "注册成功！！！";
                this.isRegister(true);
            });
        }


        public void isRegister(bool status)
        {
            this.loginStatus.Get("isAlreadyRegister").Update(status);
            MyDebug.Log("Update the variable");
        }
    }
}