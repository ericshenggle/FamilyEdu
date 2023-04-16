using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using GameCreator.Variables;
using MyTools;

namespace NetWorkManage
{
    public class RequestSender : MySingleton<RequestSender>
    {
        public static string url = "http://192.168.5.137:8080/";
        public static string captcha_url = url + "home-auth/captcha.jpg";

        /// <summary>
        /// 当前用户的UserId
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 当前系统传递文本信息的Path
        /// </summary>
        private string ioPath = @"";

        /// <summary>
        /// 后端请求的Token字段
        /// </summary>
        private string m_token = "";
        public string Token
        {
            get
            {
                if (m_token != "")
                {
                    return "Bearer " + m_token;
                }
                return "";
            }
            set
            {
                m_token = value;
            }
        }

        /// <summary>
        /// 后端请求的KAPTCHA_SESSION_KEY字段
        /// </summary>
        private string m_session_key = "";
        public string KAPTCHA_SESSION_KEY
        {
            get
            {
                return m_session_key;
            }
            set
            {
                m_session_key = value;
            }
        }

        /// <summary>
        /// OpenAI's API_KEY
        /// </summary>
        [SerializeField] public string apiKey = "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";
        public string API_KEY
        {
            get
            {
                if (apiKey != "")
                {
                    return "Bearer " + apiKey;
                }
                return "";
            }
            set
            {
                apiKey = value;
            }
        }

        // 定义回调函数
        public delegate void DownloadImageCallback(Sprite image);
        public delegate void SendRequestCallback(string jsonString);


        /// <summary>
        /// 本方法将以POST方式请求ChatGPT
        /// </summary>
        /// <param name="originJson">要发送的Json对象</param>
        /// <param name="url"></param>
        /// <param name="callback">回调函数，不会处理异常，将信息原样返回</param>
        public void SendChatGPTRequest(string originJson, string url, SendRequestCallback callback)
        {
            StartCoroutine(SendRequest(originJson, url, callback, "POST",
            new Dictionary<string, string>{
                {"Authorization", API_KEY}
            }));
        }

        /// <summary>
        /// 本方法将以POST方式请求
        /// </summary>
        /// <param name="originJson">要发送的Json对象</param>
        /// <param name="url"></param>
        /// <param name="callback">回调函数，不会处理异常，将信息原样返回</param>
        public void SendPostRequest(string originJson, string url, SendRequestCallback callback)
        {
            StartCoroutine(SendRequest(originJson, url, callback, "POST",
            new Dictionary<string, string>{
                {"Authorization", Token}
            }));
        }

        /// <summary>
        /// 本方法将以GET方式请求
        /// </summary>
        /// <param name="originJson">要发送的Json对象</param>
        /// <param name="url"></param>
        /// <param name="callback">回调函数，不会处理异常，将信息原样返回</param>
        public void SendGETRequest(string originJson, string url, SendRequestCallback callback)
        {
            StartCoroutine(SendRequest(originJson, url, callback, "GET",
            new Dictionary<string, string>{
                {"Authorization", Token}
            }));
        }

        /// <summary>
        /// 本方法将以GET方式请求
        /// </summary>
        /// <param name="originJson">要发送的Json对象</param>
        /// <param name="url"></param>
        /// <param name="callback">回调函数，不会处理异常，将信息原样返回</param>
        public void SendGETRequestByJson(string originJson, string url, SendRequestCallback callback)
        {
            StartCoroutine(SendRequestByJson(originJson, url, callback, "GET",
            new Dictionary<string, string>{
                {"Authorization", Token}
            }));
        }

        /// <summary>
        /// 本方法将以POST方式带有验证码的请求
        /// </summary>
        /// <param name="originJson">要发送的Json对象</param>
        /// <param name="url"></param>
        /// <param name="callback">回调函数，不会处理异常，将信息原样返回</param>
        public void SendPostRequestWithCaptcha(string originJson, string url, SendRequestCallback callback)
        {
            StartCoroutine(SendRequest(originJson, url, callback, "POST",
            new Dictionary<string, string>{
                {"KAPTCHA_SESSION_KEY", KAPTCHA_SESSION_KEY}
            }));
        }

        /// <summary>
        /// 本方法将以Params格式在向后端发送请求
        /// </summary>
        /// <param name="originJson">要发送的Json对象</param>
        /// <param name="url"></param>
        /// <param name="call">回调函数，不会处理异常，将信息原样返回</param>
        /// <param name="type">可以是"POST"、"GET"等</param>
        IEnumerator SendRequest(string originJson, string url, SendRequestCallback callback, string type, Dictionary<string, string> header = null)
        {

            // 创建UnityWebRequest对象，用以发送请求。使用type指定请求的类型。
            using (UnityWebRequest webRequest = new UnityWebRequest(url + originJson, type))
            {
                // 创建后端返回数据的接收端
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

                foreach (KeyValuePair<string, string> kvp in header)
                    webRequest.SetRequestHeader(kvp.Key, kvp.Value);

                // 发送请求，并等待后端返回后继续调用。
                yield return webRequest.SendWebRequest();

                // 当后端炸了，可能返回一个空字符串，对空字符串进行json解析会导致错误
                if (string.IsNullOrEmpty(webRequest.downloadHandler.text))
                {
                    callback(null);
                }
                else
                {
                    // 将后端返回的json字符串使用callback传递。
                    callback(webRequest.downloadHandler.text);
                }

            }
        }

        /// <summary>
        /// 本方法将以Json格式在向后端发送请求
        /// </summary>
        /// <param name="originJson">要发送的Json对象</param>
        /// <param name="url"></param>
        /// <param name="call">回调函数，不会处理异常，将信息原样返回</param>
        /// <param name="type">可以是"POST"、"GET"等</param>
        IEnumerator SendRequestByJson(string originJson, string url, SendRequestCallback callback, string type, Dictionary<string, string> header = null)
        {
            // 将字符串使用UTF-8编码成字节流
            byte[] postBytes = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(originJson);

            // 创建UnityWebRequest对象，用以发送请求。使用type指定请求的类型。
            using (UnityWebRequest webRequest = new UnityWebRequest(url, type))
            {
                // 设置要上传的数据
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(postBytes);
                // 创建后端返回数据的接收端
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

                foreach (KeyValuePair<string, string> kvp in header)
                    webRequest.SetRequestHeader(kvp.Key, kvp.Value);

                webRequest.SetRequestHeader("Content-Type", "application/json");

                // 发送请求，并等待后端返回后继续调用。
                yield return webRequest.SendWebRequest();

                // 当后端炸了，可能返回一个空字符串，对空字符串进行json解析会导致错误
                if (string.IsNullOrEmpty(webRequest.downloadHandler.text))
                {
                    callback(null);
                }
                else
                {
                    // 将后端返回的json字符串使用callback传递。
                    callback(webRequest.downloadHandler.text);
                }

            }
        }

        /// <summary>
        /// 本方法将以GET方式请求验证码图片
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback">回调函数，不会处理异常，将信息原样返回</param>
        public void SendCaptchaRequest(DownloadImageCallback callback)
        {
            // 设置请求内容
            StartCoroutine(DownloadCaptcha(captcha_url, callback));
        }

        /// <summary>
        /// 下载图片，转换成sprite的格式并传递给callback.
        /// 如果网络出错，就传一个null
        /// 目前验证过的图片格式：PNG, JPG
        /// </summary>
        /// <param name="url">图片的url</param>
        /// <param name="callback"></param>
        IEnumerator DownloadCaptcha(string url, DownloadImageCallback callback)
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(url, "GET"))
            {
                // 将UnityWebRequest处理下载数据的类设置为DownloadHandlerTexture
                // 之前用的是DownloadHandlerBuffer
                DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
                webRequest.downloadHandler = texDl;
                // 发送请求
                yield return webRequest.SendWebRequest();

                // 为了提高方法的间接性，不让用户自己处理网络错误了。回调函数只会传递sprite
                if (webRequest.result != UnityWebRequest.Result.ConnectionError
                    && webRequest.result != UnityWebRequest.Result.ProtocolError)
                {
                    KAPTCHA_SESSION_KEY = webRequest.GetResponseHeader("KAPTCHA_SESSION_KEY");
                    // 没有网络错误，将下载的图片转换成sprite给回调函数
                    Sprite sprite = Sprite.Create(texDl.texture, new Rect(0, 0, texDl.texture.width, texDl.texture.height), new Vector2(0.5f, 0.5f));
                    callback(sprite);
                }
                else
                {
                    // 出现网络错误，传一个null
                    callback(null);
                }
            }
        }

        /// <summary>
        /// 本方法将以GET方式请求头像图片
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback">回调函数，不会处理异常，将信息原样返回</param>
        public void SendAvatarRequest(string avatarUrl, DownloadImageCallback callback)
        {
            // 设置请求内容
            StartCoroutine(DownloadAvatar(url + avatarUrl, callback,
            new Dictionary<string, string>{
                {"Authorization", Token}
            }));
        }

        /// <summary>
        /// 下载图片，转换成sprite的格式并传递给callback.
        /// 如果网络出错，就传一个null
        /// 目前验证过的图片格式：PNG, JPG
        /// </summary>
        /// <param name="url">图片的url</param>
        /// <param name="callback"></param>
        IEnumerator DownloadAvatar(string url, DownloadImageCallback callback, Dictionary<string, string> header = null)
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(url, "GET"))
            {
                // 将UnityWebRequest处理下载数据的类设置为DownloadHandlerTexture
                // 之前用的是DownloadHandlerBuffer
                DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
                webRequest.downloadHandler = texDl;

                foreach (KeyValuePair<string, string> kvp in header)
                    webRequest.SetRequestHeader(kvp.Key, kvp.Value);

                // 发送请求
                yield return webRequest.SendWebRequest();

                // 为了提高方法的间接性，不让用户自己处理网络错误了。回调函数只会传递sprite
                if (webRequest.result != UnityWebRequest.Result.ConnectionError
                    && webRequest.result != UnityWebRequest.Result.ProtocolError)
                {
                    // 没有网络错误，将下载的图片转换成sprite给回调函数
                    Sprite sprite = Sprite.Create(texDl.texture, new Rect(0, 0, texDl.texture.width, texDl.texture.height), new Vector2(0.5f, 0.5f));
                    callback(sprite);
                }
                else
                {
                    // 出现网络错误，传一个null
                    callback(null);
                }
            }
        }

        public static string getUrlParams(Dictionary<string, string> param)
        {
            string res = "?";
            foreach (KeyValuePair<string, string> pair in param)
            {
                res += (pair.Key + "=" + pair.Value + "&");
            }
            return res.Substring(0, res.Length - 1);
        }

        void Start()
        {
            getSavedTokenAndUserId();
        }

        public void getSavedTokenAndUserId()
        {
            if (System.IO.File.Exists(this.ioPath))
            {
                using (FileStream fs = System.IO.File.Open(this.ioPath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    byte[] b = new byte[1024];
                    UTF8Encoding temp = new UTF8Encoding(true);
                    while (fs.Read(b, 0, b.Length) > 0)
                    {
                        // TODO: 
                        MyDebug.Log(temp.GetString(b));
                    }
                }
            }
        }

        public void setTokenAndUserId(string tokenValue, long id)
        {
            this.Token = tokenValue;
            this.UserId = id;
        }
    }
}
