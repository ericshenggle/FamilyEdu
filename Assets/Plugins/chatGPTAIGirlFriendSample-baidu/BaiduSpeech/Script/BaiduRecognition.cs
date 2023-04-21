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

        //��ȡ�ٶ�����ʶ���Token
        StartCoroutine(GetToken(GetTokenAction));
    }

    #region Unity¼�����������
    /// <summary>
    /// ����Ƿ������������豸
    /// </summary>
    [SerializeField]private bool m_HaveMicrophone = false;
    /// <summary>
    /// ���������豸����
    /// </summary>
    [SerializeField] private string m_MicrophoneDeviceName = string.Empty;
    /// <summary>
    /// ¼�Ƶ�����Ƭ��
    /// </summary>
    [SerializeField]private AudioClip m_AudioClip = null;
    /// <summary>
    /// ���¼��ʱ��
    /// </summary>
    [SerializeField] private int m_SpeechMaxLength = 3;
    /// <summary>
    /// ¼��Ƶ��
    /// </summary>
    [SerializeField] private int m_SpeechFrequency = 8000;
    /// <summary>
    /// ���ʵ������
    /// </summary>
    [SerializeField] private InputField m_CommitInput;

    /// <summary>
    /// ��ʼ¼������
    /// </summary>
    private void BeginSpeechRecord()
    {
        if (!m_HaveMicrophone|| Microphone.IsRecording(m_MicrophoneDeviceName))
        {
            return;
        }

        //��ʼ¼������
        m_AudioClip = Microphone.Start(m_MicrophoneDeviceName, false, m_SpeechMaxLength, m_SpeechFrequency);
    }

    /// <summary>
    /// ����¼��
    /// </summary>
    private void EndSpeechRecord()
    {
        if (!m_HaveMicrophone)
        {
            return;
        }

        //����¼��
        Microphone.End(m_MicrophoneDeviceName);
        //������Ƶʶ��
        StartCoroutine(GetBaiduRecognize(RecognizeBack));

    }


    /// <summary>
    /// ��������ʱ
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        BeginSpeechRecord();
        m_CommitInput.text = "";
        //����һ�°�ť��ɫ���ı�
        this.GetComponent<Image>().color = Color.grey;
        this.transform.Find("Text").GetComponent<Text>().text = "������";
    }

    /// <summary>
    /// �����ɿ�ʱ
    /// </summary>
    /// <param name="eventData"></param>

    public void OnPointerUp(PointerEventData eventData)
    {
        EndSpeechRecord();
        //����һ�°�ť��ɫ���ı�
        this.GetComponent<Image>().color = Color.green;
        this.transform.Find("Text").GetComponent<Text>().text = "����";
    }

    #endregion

    #region �ٶ�����ʶ��
    /// <summary>
    /// APIkey
    /// </summary>
    [SerializeField]private string m_Client_id = string.Empty;
    /// <summary>
    /// SecretKey
    /// </summary>
    [SerializeField] private string m_Client_secret = string.Empty;
    /// <summary>
    /// ��ȡ����Token
    /// </summary>
    [SerializeField]private string m_Token=string.Empty ;
    /// <summary>
    /// ��ȡToken��api��ַ
    /// </summary>
    [SerializeField] private string m_AuthorizeURL = "https://aip.baidubce.com/oauth/2.0/token";
    /// <summary>
    /// ����ʶ��api��ַ
    /// </summary>
    [SerializeField] private string m_SpeechRecognizeURL = "https://vop.baidu.com/server_api";
    /// <summary>
    /// ��ȡ��token
    /// </summary>
    /// <param name="_token"></param>
    private void GetTokenAction(string _token)
    {
        m_Token = _token;
    }
    /// <summary>
    /// ��ȡtoken�ķ���
    /// </summary>
    /// <param name="_callback"></param>
    /// <returns></returns>
    private IEnumerator GetToken(System.Action<string> _callback)
    {
        //��ȡtoken��api��ַ
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
    /// ��ȡ�ٶ�����ʶ��
    /// </summary>
    /// <param name="_callback"></param>
    /// <returns></returns>
    private IEnumerator GetBaiduRecognize(System.Action<string> _callback)
    {

        string asrResult = string.Empty;

        //����ǰ¼������ΪPCM16
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
                    RecognizeBack("����ʶ��ʧ��");
                }
            }
        }

    
    }

    private void RecognizeBack(string _msg) {
        m_CommitInput.text = _msg;
        //������Ϣ��openai�ӿ�
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
