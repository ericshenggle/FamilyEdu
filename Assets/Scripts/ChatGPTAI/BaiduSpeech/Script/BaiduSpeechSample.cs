using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

public class BaiduSpeechSample : MonoBehaviour
{
    #region 参数
    /// <summary>
    /// API Key
    /// </summary>
    [Header("填写应用的API Key")]public string m_Client_id=string.Empty;
    /// <summary>
    /// Secret Key
    /// </summary>
    [Header("填写应用的Secret Key")] public string m_Client_secret=string.Empty;
    /// <summary>
    /// token值
    /// </summary>
    public string m_Token = string.Empty;
    /// <summary>
    /// 音频播放器
    /// </summary>
    public AudioSource m_AudioSource;
    /// <summary>
    /// 获取Token的地址
    /// </summary>
    [SerializeField] private string m_AuthorizeURL = "https://aip.baidubce.com/oauth/2.0/token";
    /// <summary>
    /// 语音合成的api地址
    /// </summary>
    [SerializeField] private string m_PostURL = "http://tsn.baidu.com/text2audio";
    /// <summary>
    /// 语音合成设置
    /// </summary>
    [SerializeField] private PostDataSetting m_Post_Setting;
    #endregion

    private void Start()
    {
        StartCoroutine(GetToken(GetTokenAction));
    }

    #region Public Method
    /// <summary>
    /// 朗读文本
    /// </summary>
    /// <param name="_msg"></param>
    public void Speek(string _msg, System.Action<string> _callback)
    {
        StartCoroutine(GetSpeech(_msg, m_Token, GetAudioClip, _callback));
    }

    #endregion

    #region Private Method

    /// <summary>
    /// 获取到token
    /// </summary>
    /// <param name="_token"></param>
    private void GetTokenAction(string _token)
    {
        m_Token = _token;
    }

    /// <summary>
    /// 获取到合成的音频并播放
    /// </summary>
    /// <param name="_callback"></param>
    /// <returns></returns>
    private  void GetAudioClip(SpeechResponse _callback)
    {
        if (!_callback.Success)
            return;

        m_AudioSource.clip = _callback.clip;
        m_AudioSource.Play();

    }


    /// <summary>
    /// 获取token的方法
    /// </summary>
    /// <param name="_callback"></param>
    /// <returns></returns>
    private IEnumerator GetToken(System.Action<string> _callback)
    {
        //获取token的api地址
        string _token_url = string.Format(m_AuthorizeURL+"?client_id={0}&client_secret={1}&grant_type=client_credentials"
            , m_Client_id, m_Client_secret);

        using (UnityWebRequest request = new UnityWebRequest(_token_url, "GET"))
        {
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            if (request.isDone)
            {
                string _msg = request.downloadHandler.text;
                TokenInfo _textback = JsonUtility.FromJson<TokenInfo>(_msg);
                string _token = _textback.access_token;
                _callback(_token);

            }
        }
    }

    /// <summary>
    /// 语音合成的方法
    /// </summary>
    /// <param name="_msg"></param>
    /// <param name="_token"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    private IEnumerator GetSpeech(string _msg, string _token, System.Action<SpeechResponse> callback, System.Action<string> _callback)
    {
        var _url = m_PostURL;
        var _postParams = new Dictionary<string, string>();
        _postParams.Add("tex", _msg);
        _postParams.Add("tok", _token);
        _postParams.Add("cuid", SystemInfo.deviceUniqueIdentifier);
        _postParams.Add("ctp", m_Post_Setting.ctp);
        _postParams.Add("lan", m_Post_Setting.lan);
        _postParams.Add("spd", m_Post_Setting.spd);
        _postParams.Add("pit", m_Post_Setting.pit);
        _postParams.Add("vol", m_Post_Setting.vol);
        _postParams.Add("per", SetSpeeker(m_Post_Setting.per));
        _postParams.Add("aue", m_Post_Setting.aue);

        //拼接参数到链接里
        int i = 0;
        foreach (var item in _postParams)
        {
            _url += i != 0 ? "&" : "?";
            _url += item.Key + "=" + item.Value;
            i++;
        }

        //合成音频
        var _speech = UnityWebRequestMultimedia.GetAudioClip(_url, AudioType.WAV);
        yield return _speech.SendWebRequest();

        if (_speech.error == null)
        {
            var type = _speech.GetResponseHeader("Content-Type");
            if (type.Contains("audio"))
            {

                var response = new SpeechResponse { clip = DownloadHandlerAudioClip.GetContent(_speech) };
                callback(response);
                _callback(_msg);
            }
        }


    }
    //基础音库:度小宇=1，度小美=0，度逍遥（基础）=3，度丫丫=4
        /// 精品音库:度逍遥（精品）=5003，度小鹿=5118，度博文=106，度小童=110，度小萌=111，度米朵=103，度小娇=5
    private string SetSpeeker(SpeekerRole _role)
    {
        if (_role == SpeekerRole.度小宇) return "1";
        if (_role == SpeekerRole.度小美) return "0";
        if (_role == SpeekerRole.度逍遥) return "3";
        if (_role == SpeekerRole.度丫丫) return "4";
        if (_role == SpeekerRole.JP度小娇) return "5";
        if (_role == SpeekerRole.JP度逍遥) return "5003";
        if (_role == SpeekerRole.JP度小鹿) return "5118";
        if (_role == SpeekerRole.JP度博文) return "106";
        if (_role == SpeekerRole.JP度小童) return "110";
        if (_role == SpeekerRole.JP度小萌) return "111";
        if (_role == SpeekerRole.JP度米朵) return "5";

        return "0";//默认为度小美
    }

    #endregion

    #region 数据格式定义

    /// <summary>
    /// 返回的token
    /// </summary>
    [System.Serializable]public class TokenInfo
    {
        public string access_token = string.Empty;
    }

    /// <summary>
    /// 语音合成的配置信息
    /// </summary>
    [System.Serializable]
    public class PostDataSetting
    {
        /// <summary>
        /// 客户端类型选择，web端填写固定值1
        /// </summary>
        public string ctp = "1";
        /// <summary>
        /// 固定值zh。语言选择,目前只有中英文混合模式，填写固定值zh
        /// </summary>
        [Header("语言设置，固定值zh")] public string lan = "zh";
        /// <summary>
        /// 语速，取值0-15，默认为5中语速
        /// </summary>
        [Header("语速，取值0-15，默认为5中语速")] public string spd = "5";
        /// <summary>
        /// 音调，取值0-15，默认为5中语调
        /// </summary>
        [Header("音调，取值0-15，默认为5中语调")] public string pit ="5";
        /// <summary>
        /// 音量，取值0-15，默认为5中音量（取值为0时为音量最小值，并非为无声）
        /// </summary>
        [Header("音量，取值0-15，默认为5中音量")] public string vol = "5";
        /// <summary>
        /// 基础音库:度小宇=1，度小美=0，度逍遥（基础）=3，度丫丫=4
        /// 精品音库:度逍遥（精品）=5003，度小鹿=5118，度博文=106，度小童=110，度小萌=111，度米朵=103，度小娇=5
        /// </summary>
        [Header("设置朗读的声音")] public SpeekerRole per = SpeekerRole.度小美;
        /// <summary>
        /// 3为mp3格式(默认)； 4为pcm-16k；5为pcm-8k；6为wav（内容同pcm-16k）; 注意aue=4或者6是语音识别要求的格式，
        /// 但是音频内容不是语音识别要求的自然人发音，所以识别效果会受影响。
        /// </summary>
        [Header("设置返回的音频格式")] public string aue = "6";
    }
    /// <summary>
    /// 可选声音
    /// </summary>
    public enum SpeekerRole
    {
        度小宇,
        度小美,
        度逍遥,
        度丫丫,
        JP度逍遥,
        JP度小鹿,
        JP度博文,
        JP度小童,
        JP度小萌,
        JP度米朵,
        JP度小娇
    }

    /// <summary>
    /// 语音合成结果
    /// </summary>
    public class SpeechResponse
    {
        public int error_index;
        public string error_msg;
        public string sn;
        public int idx;
        public bool Success
        {
            get { return error_index == 0; }
        }
        public AudioClip clip;
    }


    #endregion
  


  


}
