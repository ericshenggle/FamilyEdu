using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using MyTools;
using GameCreator.Variables;

namespace NetWorkManage
{
    public class MyLogin_API : MonoBehaviour
    {
        public static string requestUrl = RequestSender.url + "home-auth/login";

        // Start is called before the first frame updat
        public partial class ResponseData
        {
            [JsonProperty("code")]
            public long Code { get; set; }

            [JsonProperty("data")]
            public Data Data { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("success")]
            public bool Success { get; set; }
        }

        public partial class Data
        {
            [JsonProperty("token")]
            public Token Token { get; set; }

            [JsonProperty("user")]
            public User User { get; set; }
        }

        public partial class Token
        {
            [JsonProperty("isLogin")]
            public bool IsLogin { get; set; }

            [JsonProperty("loginDevice")]
            public string LoginDevice { get; set; }

            [JsonProperty("loginId")]
            public string LoginId { get; set; }

            [JsonProperty("loginType")]
            public string LoginType { get; set; }

            [JsonProperty("sessionTimeout")]
            public long SessionTimeout { get; set; }

            [JsonProperty("tokenActivityTimeout")]
            public long TokenActivityTimeout { get; set; }

            [JsonProperty("tokenName")]
            public string TokenName { get; set; }

            [JsonProperty("tokenSessionTimeout")]
            public long TokenSessionTimeout { get; set; }

            [JsonProperty("tokenTimeout")]
            public long TokenTimeout { get; set; }

            [JsonProperty("tokenValue")]
            public string TokenValue { get; set; }
        }

        public partial class User
        {
            [JsonProperty("age")]
            public long Age { get; set; }

            [JsonProperty("avatarUrl")]
            public string AvatarUrl { get; set; }

            [JsonProperty("createTime")]
            public string CreateTime { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("homeId")]
            public long HomeId { get; set; }

            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("phone")]
            public string Phone { get; set; }

            [JsonProperty("sex")]
            public string Sex { get; set; }

            [JsonProperty("updateTime")]
            public string UpdateTime { get; set; }
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

        public ResponseData myLoginData;

        [SerializeField] public InputField m_inputFieldName;
        [SerializeField] public InputField m_inputFieldPwd;
        [SerializeField] public InputField m_inputFieldCaptcha;
        [SerializeField] public Image m_imageCaptcha;
        [SerializeField] public Text m_showProcessing;

        [SerializeField] public LocalVariables loginStatus;

        void Start()
        {
            // StartCoroutine(printToken());
            getCaptcha();
        }

        private IEnumerator printToken()
        {
            RequestSender requestSender = gameObject.GetComponent<RequestSender>();
            while (true)
            {
                MyDebug.Log(requestSender.Token);
                yield return new WaitForSeconds(5);
            }
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

        public void getTokenAndUserId()
        {
            this.m_showProcessing.text = "登录中...";
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
                this.myLoginData = ResponseData.FromJson(responseContent);
                if (this.myLoginData.Code == 400 || this.myLoginData.Code == 500)
                {
                    this.m_showProcessing.text = this.myLoginData.Message + "，请重新登录！！！";
                    return;
                }
                else if (this.myLoginData.Code != 200)
                {
                    this.m_showProcessing.text = "网络问题，请重新登录！！！";
                    return;
                }
                this.m_showProcessing.text = "登录成功！！！";
                RequestSender.Instance.setTokenAndUserId(this.myLoginData.Data.Token.TokenValue, this.myLoginData.Data.User.Id
                            );
                this.isLogin(this.myLoginData.Data.Token.IsLogin);
            });
        }


        public void isLogin(bool status)
        {
            this.loginStatus.Get("isAlreadyLogin").Update(status);
            MyDebug.Log("Update the variable");
        }

    }
}