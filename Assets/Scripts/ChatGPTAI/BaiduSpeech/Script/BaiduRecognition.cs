using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using static BaiduSpeechSample;

public class BaiduRecognition : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{


    private void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            m_HaveMicrophone = true;
            m_MicrophoneDeviceName = Microphone.devices[0];
        }

        //获取百度语音识别的Token
        StartCoroutine(GetToken(GetTokenAction));
    }

    #region Unity录制输入的声音
    /// <summary>
    /// 检测是否有语音输入设备
    /// </summary>
    [SerializeField]private bool m_HaveMicrophone = false;
    /// <summary>
    /// 语音输入设备名称
    /// </summary>
    [SerializeField] private string m_MicrophoneDeviceName = string.Empty;
    /// <summary>
    /// 录制的声音片段
    /// </summary>
    [SerializeField]private AudioClip m_AudioClip = null;
    /// <summary>
    /// 最大录音时长
    /// </summary>
    [SerializeField] private int m_SpeechMaxLength = 3;
    /// <summary>
    /// 录音频率
    /// </summary>
    [SerializeField] private int m_SpeechFrequency = 8000;
    /// <summary>
    /// 提问的输入框
    /// </summary>
    [SerializeField] private InputField m_CommitInput;

    /// <summary>
    /// 开始录制声音
    /// </summary>
    private void BeginSpeechRecord()
    {
        if (!m_HaveMicrophone|| Microphone.IsRecording(m_MicrophoneDeviceName))
        {
            return;
        }

        //开始录制声音
        m_AudioClip = Microphone.Start(m_MicrophoneDeviceName, false, m_SpeechMaxLength, m_SpeechFrequency);
    }

    /// <summary>
    /// 结束录制
    /// </summary>
    private void EndSpeechRecord()
    {
        if (!m_HaveMicrophone)
        {
            return;
        }

        //结束录制
        Microphone.End(m_MicrophoneDeviceName);
        //进行音频识别
        StartCoroutine(GetBaiduRecognize(RecognizeBack));

    }


    /// <summary>
    /// 按键按下时
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        BeginSpeechRecord();
        m_CommitInput.text = "";
        //调整一下按钮颜色及文本
        this.GetComponent<Image>().color = Color.grey;
        this.transform.Find("Text").GetComponent<Text>().text = "提问中";
    }

    /// <summary>
    /// 按键松开时
    /// </summary>
    /// <param name="eventData"></param>

    public void OnPointerUp(PointerEventData eventData)
    {
        EndSpeechRecord();
        //调整一下按钮颜色及文本
        this.GetComponent<Image>().color = Color.green;
        this.transform.Find("Text").GetComponent<Text>().text = "提问";
    }

    #endregion

    #region 百度语音识别
    /// <summary>
    /// APIkey
    /// </summary>
    [SerializeField]private string m_Client_id = string.Empty;
    /// <summary>
    /// SecretKey
    /// </summary>
    [SerializeField] private string m_Client_secret = string.Empty;
    /// <summary>
    /// 获取到的Token
    /// </summary>
    [SerializeField]private string m_Token=string.Empty ;
    /// <summary>
    /// 获取Token的api地址
    /// </summary>
    [SerializeField] private string m_AuthorizeURL = "https://aip.baidubce.com/oauth/2.0/token";
    /// <summary>
    /// 语音识别api地址
    /// </summary>
    [SerializeField] private string m_SpeechRecognizeURL = "https://vop.baidu.com/server_api";
    /// <summary>
    /// 获取到token
    /// </summary>
    /// <param name="_token"></param>
    private void GetTokenAction(string _token)
    {
        m_Token = _token;
    }
    /// <summary>
    /// 获取token的方法
    /// </summary>
    /// <param name="_callback"></param>
    /// <returns></returns>
    private IEnumerator GetToken(System.Action<string> _callback)
    {
        //获取token的api地址
        string _token_url = string.Format(m_AuthorizeURL + "?client_id={0}&client_secret={1}&grant_type=client_credentials"
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
    /// 获取百度语音识别
    /// </summary>
    /// <param name="_callback"></param>
    /// <returns></returns>
    private IEnumerator GetBaiduRecognize(System.Action<string> _callback)
    {

        string asrResult = string.Empty;

        //处理当前录音数据为PCM16
        float[] samples = new float[m_AudioClip.samples];
        m_AudioClip.GetData(samples, 0);
        var samplesShort = new short[samples.Length];
        for (var index = 0; index < samples.Length; index++)
        {
            samplesShort[index] = (short)(samples[index] * short.MaxValue);
        }
        byte[] datas = new byte[samplesShort.Length * 2];

        Buffer.BlockCopy(samplesShort, 0, datas, 0, datas.Length);

        string url = string.Format(m_SpeechRecognizeURL+"?cuid={0}&token={1}",  SystemInfo.deviceUniqueIdentifier, m_Token);

        WWWForm wwwForm = new WWWForm();
        wwwForm.AddBinaryData("audio", datas);

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(url, wwwForm))
        {
            unityWebRequest.SetRequestHeader("Content-Type", "audio/pcm;rate=" + m_SpeechFrequency);

            yield return unityWebRequest.SendWebRequest();

            if (string.IsNullOrEmpty(unityWebRequest.error))
            {
                asrResult = unityWebRequest.downloadHandler.text;
                RecogizeBackData _data = JsonUtility.FromJson<RecogizeBackData>(asrResult);
                if (_data.err_no == "0")
                {
                    RecognizeBack(_data.result[0]);
                }
                else
                {
                    RecognizeBack("语音识别失败");
                }
            }
        }

    
    }

    private void RecognizeBack(string _msg) {
        m_CommitInput.text = _msg;
        //发送信息给openai接口
        ChatScript.Instance.SendData(_msg);

    }
    
    #endregion


    [System.Serializable]public class RecogizeBackData
    {
        public string corpus_no = string.Empty;
        public string err_msg=string.Empty;
        public string err_no = string.Empty;
        public List<string> result;
        public string sn = string.Empty;
    }

}
