using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using Agora.Util;
using Logger = Agora.Util.Logger;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using MyTools;
using GameCreator.Core.Hooks;
using UnityEngine.Animations;
using GameCreator.Variables;
using RingBuffer;

namespace Classroom.AgoraScripts
{
    public class MyJoinChannelVideo : MonoBehaviour
    {
        [FormerlySerializedAs("appIdInput")]
        [SerializeField]
        private AppIdInput _appIdInput;

        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        private string _appID = "";

        [FormerlySerializedAs("TOKEN")]
        [SerializeField]
        private string _token = "";

        [FormerlySerializedAs("CHANNEL_NAME")]
        [SerializeField]
        private string _channelName = "";

        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine = null;

        public Dropdown _videoDeviceSelect;
        private IVideoDeviceManager _videoDeviceManager;
        private DeviceInfo[] _videoDeviceInfos;
        private Dictionary<uint, VideoSurface> local_VideoSurface = new Dictionary<uint, VideoSurface>();
        public GameObject floatingVideoPrefab;
        public GameObject floatingVideoPrefab_teacher;
        public Text cameraText;
        public Text microphoneText;

        #region Audio Attributions
        private const int CHANNEL = 1;
        private const int PULL_FREQ_PER_SEC = 100;
        public const int SAMPLE_RATE = 32000; // this should = CLIP_SAMPLES x PULL_FREQ_PER_SEC
        public const int CLIP_SAMPLES = 320;

        internal int _count;
        internal int _writeCount;
        internal int _readCount;


        internal Dictionary<(uint, string), RingBuffer<float>> local_audioBuffer = new Dictionary<(uint, string), RingBuffer<float>>();

        #endregion

        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                SetBasicConfiguration();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        private void LoadAssetData()
        {
            if (_appIdInput == null) return;
            _appID = _appIdInput.appID;
            _token = _appIdInput.token;
            _channelName = _appIdInput.channelName;
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, AREA_CODE.AREA_CODE_GLOB, new LogConfig("./log.txt"));
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);

            RtcEngine.RegisterAudioFrameObserver(new AudioFrameObserver(this), OBSERVER_MODE.RAW_DATA);
            RtcEngine.SetPlaybackAudioFrameBeforeMixingParameters(SAMPLE_RATE, 1);
        }

        private void SetBasicConfiguration()
        {
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
            VideoEncoderConfiguration config = new VideoEncoderConfiguration();
            config.dimensions = new VideoDimensions(640, 360);
            config.frameRate = 15;
            config.bitrate = 0;
            RtcEngine.SetVideoEncoderConfiguration(config);
            RtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }

        #region -- Button Events ---

        public void JoinChannel()
        {


            RtcEngine.JoinChannel(_token, _channelName, "", PhotonNetwork.InRoom ? (uint)PhotonNetwork.LocalPlayer.ActorNumber : 0);
            MakeVideoView(0, PhotonNetwork.LocalPlayer, GetChannelName());
            SetupAudio(0, PhotonNetwork.LocalPlayer, GetChannelName());
            cameraText.text = "摄像头状态：开启";
            microphoneText.text = "麦克风状态：开启";
        }

        public void LeaveChannel()
        {
            RtcEngine.LeaveChannel();
            foreach (VideoSurface videoSurface in local_VideoSurface.Values)
            {
                Destroy(videoSurface.gameObject);
            }
            local_VideoSurface.Clear();
            cameraText.text = "摄像头状态：";
            microphoneText.text = "麦克风状态：";
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                DestroyAudio(player, GetChannelName());
            }
            local_audioBuffer.Clear();
        }

        public void StartPreview()
        {
            RtcEngine.StartPreview();
            MakeVideoView(0, PhotonNetwork.LocalPlayer, GetChannelName());
            SetupAudio(0, PhotonNetwork.LocalPlayer, GetChannelName());
        }

        public void StopPreview()
        {
            DestroyVideoView(0);
            DestroyAudio(PhotonNetwork.LocalPlayer, GetChannelName());
            RtcEngine.StopPreview();
        }

        public void StartCameraPublish()
        {
            var options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(true);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);
            cameraText.text = "摄像头状态：开启";
        }

        public void StopCameraPublish()
        {
            var options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(false);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);
            cameraText.text = "摄像头状态：关闭";
        }

        public void StartMicrophonePublish()
        {
            var options = new ChannelMediaOptions();
            options.publishMicrophoneTrack.SetValue(true);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);
            microphoneText.text = "麦克风状态：开启";
        }

        public void StopMicrophonePublish()
        {
            var options = new ChannelMediaOptions();
            options.publishMicrophoneTrack.SetValue(false);
            var nRet = RtcEngine.UpdateChannelMediaOptions(options);
            this.Log.UpdateLog("UpdateChannelMediaOptions: " + nRet);
            microphoneText.text = "麦克风状态：关闭";
        }

        public void GetVideoDeviceManager()
        {
            _videoDeviceSelect.ClearOptions();

            _videoDeviceManager = RtcEngine.GetVideoDeviceManager();
            _videoDeviceInfos = _videoDeviceManager.EnumerateVideoDevices();
            Log.UpdateLog(string.Format("VideoDeviceManager count: {0}", _videoDeviceInfos.Length));
            for (var i = 0; i < _videoDeviceInfos.Length; i++)
            {
                Log.UpdateLog(string.Format("VideoDeviceManager device index: {0}, name: {1}, id: {2}", i,
                    _videoDeviceInfos[i].deviceName, _videoDeviceInfos[i].deviceId));
            }

            _videoDeviceSelect.AddOptions(_videoDeviceInfos.Select(w =>
                    new Dropdown.OptionData(
                        string.Format("{0} :{1}", w.deviceName, w.deviceId)))
                .ToList());
        }

        public void SelectVideoCaptureDevice()
        {
            if (_videoDeviceSelect == null) return;
            var option = _videoDeviceSelect.options[_videoDeviceSelect.value].text;
            if (string.IsNullOrEmpty(option)) return;

            var deviceId = option.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            var ret = _videoDeviceManager.SetDevice(deviceId);
            Log.UpdateLog("SelectVideoCaptureDevice ret:" + ret + " , DeviceId: " + deviceId);
        }

        #endregion

        private void OnDestroy()
        {
            MyDebug.Log("OnDestroy");
            if (RtcEngine == null) return;
            foreach (VideoSurface videoSurface in local_VideoSurface.Values)
            {
                Destroy(videoSurface.gameObject);
            }
            local_audioBuffer.Clear();
            local_VideoSurface.Clear();
            RtcEngine.UnRegisterAudioFrameObserver();
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }

        internal string GetChannelName()
        {
            return _channelName;
        }

        #region -- Video Render UI Logic ---

        internal void MakeVideoView(uint uid, Player player, string channelId)
        {
            if (uid != 0 && uid != player.ActorNumber)
            {
                MyDebug.Log(String.Format("uid:{0} is not equals with player:{1}", uid, player.ActorNumber));
                return;
            }
            GameObject tagObject = player?.TagObject as GameObject;
            if (local_VideoSurface.ContainsKey(uid))
            {
                return;
            }

            // create a GameObject and assign to this new user
            VideoSurface videoSurface = MakeImageSurface(uid.ToString(), tagObject);
            if (ReferenceEquals(videoSurface, null))
            {
                return;
            }
            // configure videoSurface
            if (uid == 0)
            {
                videoSurface.SetForUser(uid, channelId);
            }
            else
            {
                videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            }
            videoSurface.SetEnable(true);
            local_VideoSurface.Add(uid, videoSurface);
        }

        private VideoSurface MakeImageSurface(string goName, GameObject tagObject)
        {
            bool isTeaching = (bool)VariablesManager.GetLocal(tagObject, "isTeaching", false);
            GameObject go;
            if (isTeaching)
            {
                go = GameObject.Instantiate(floatingVideoPrefab_teacher);
            }
            else
            {
                go = GameObject.Instantiate(floatingVideoPrefab);
            }
            if (go == null)
            {
                return null;
            }
            go.name = goName;
            // to be renderered onto
            if (tagObject != null)
            {
                go.transform.parent = tagObject.transform;
                MyDebug.Log("add video view");
            }
            else
            {
                MyDebug.Log("player's tagobject is null");
            }
            if (isTeaching)
            {
                go.transform.localPosition = new Vector3(1.87f, 1.65f, 0.4f);
            }
            else
            {
                go.transform.localPosition = Vector3.up + Vector3.right;

                Camera camera = HookCamera.Instance ? HookCamera.Instance.Get<Camera>() : null;
                if (!camera) camera = GameObject.FindObjectOfType<Camera>();

                LookAtConstraint constraint = go.GetComponent<LookAtConstraint>();
                if (!constraint) constraint = go.AddComponent<LookAtConstraint>();
                constraint.rotationOffset = new Vector3(0, 180, 180);
                constraint.SetSources(new List<ConstraintSource>()
                {
                    new ConstraintSource()
                    {
                        sourceTransform = camera.transform,
                        weight = 1.0f
                    }
                });

                Canvas canvas = go.GetComponent<Canvas>();
                if (canvas) canvas.worldCamera = camera;

                constraint.constraintActive = true;
            }
            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        internal void DestroyVideoView(uint uid)
        {
            if (!local_VideoSurface.ContainsKey(uid))
            {
                return;
            }
            VideoSurface videoSurface;
            local_VideoSurface.Remove(uid, out videoSurface);
            Destroy(videoSurface.gameObject);
        }
        # endregion

        #region -- Audio Logic ---

        internal void SetupAudio(uint uid, Player player, string channel_id)
        {
            GameObject tagObject = player?.TagObject as GameObject;
            if (tagObject == null)
            {
                MyDebug.Log("player's tagobject is null");
                return;
            }
            AudioSource aud = tagObject.GetComponent<AudioSource>();
            if (aud == null)
            {
                MyDebug.Log("player's tagobject has not AudioSource Component");
                return;
            }
            // //The larger the buffer, the higher the delay
            var bufferLength = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL * 100; // 1-sec-length buffer
            RingBuffer<float> audioBuffer = new RingBuffer<float>(bufferLength, true);
            local_audioBuffer.Add((uid, channel_id), audioBuffer);

            AudioClip audioClip = AudioClip.Create("externalClip" + uid,
                CLIP_SAMPLES,
                CHANNEL, SAMPLE_RATE, true,
                (data) =>
                {
                    for (var i = 0; i < data.Length; i++)
                    {
                        lock (local_audioBuffer[(uid, channel_id)])
                        {
                            if (local_audioBuffer[(uid, channel_id)].Count > 0)
                            {
                                data[i] = local_audioBuffer[(uid, channel_id)].Get();
                            }
                        }
                    }
                });
            aud.clip = audioClip;
            aud.mute = true;
            aud.loop = true;
            aud.Play();
        }

        internal void DestroyAudio(Player player, string channel_id)
        {
            GameObject tagObject = player?.TagObject as GameObject;
            if (tagObject == null)
            {
                MyDebug.Log("player's tagobject is null");
                return;
            }
            AudioSource aud = tagObject.GetComponent<AudioSource>();
            if (aud == null)
            {
                MyDebug.Log("player's tagobject has not AudioSource Component");
                return;
            }
            aud.Stop();
            Destroy(aud.clip);
            aud.clip = null;
            if (!local_audioBuffer.ContainsKey(((uint)player.ActorNumber, channel_id)))
            {
                return;
            }
            local_audioBuffer.Remove(((uint)player.ActorNumber, channel_id));
        }

        internal static float[] ConvertByteToFloat16(byte[] byteArray)
        {
            var floatArray = new float[byteArray.Length / 2];
            for (var i = 0; i < floatArray.Length; i++)
            {
                floatArray[i] = BitConverter.ToInt16(byteArray, i * 2) / 32768f; // -Int16.MinValue
            }

            return floatArray;
        }

        #endregion
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly MyJoinChannelVideo _myJoinChannelVideo;

        internal UserEventHandler(MyJoinChannelVideo videoSample)
        {
            _myJoinChannelVideo = videoSample;
        }

        public override void OnError(int err, string msg)
        {
            _myJoinChannelVideo.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            MyDebug.Log("Agora: OnJoinChannelSuccess ");
            _myJoinChannelVideo.Log.UpdateLog(string.Format("sdk version: ${0}",
                _myJoinChannelVideo.RtcEngine.GetVersion(ref build)));
            _myJoinChannelVideo.Log.UpdateLog(string.Format("sdk build: ${0}",
              build));
            _myJoinChannelVideo.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _myJoinChannelVideo.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _myJoinChannelVideo.Log.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _myJoinChannelVideo.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _myJoinChannelVideo.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            Player player = PhotonNetwork.CurrentRoom.GetPlayer((int)uid);
            _myJoinChannelVideo.MakeVideoView(uid, player, _myJoinChannelVideo.GetChannelName());
            _myJoinChannelVideo.SetupAudio(uid, player, _myJoinChannelVideo.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _myJoinChannelVideo.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            Player player = PhotonNetwork.CurrentRoom.GetPlayer((int)uid);
            _myJoinChannelVideo.DestroyVideoView(uid);
            _myJoinChannelVideo.DestroyAudio(player, _myJoinChannelVideo.GetChannelName());
        }

        public override void OnUplinkNetworkInfoUpdated(UplinkNetworkInfo info)
        {
            _myJoinChannelVideo.Log.UpdateLog("OnUplinkNetworkInfoUpdated");
        }

        public override void OnDownlinkNetworkInfoUpdated(DownlinkNetworkInfo info)
        {
            _myJoinChannelVideo.Log.UpdateLog("OnDownlinkNetworkInfoUpdated");
        }
    }

    internal class AudioFrameObserver : IAudioFrameObserver
    {
        private readonly MyJoinChannelVideo _myJoinChannelVideo;
        private AudioParams _audioParams;


        internal AudioFrameObserver(MyJoinChannelVideo myJoinChannelVideo)
        {
            _myJoinChannelVideo = myJoinChannelVideo;
            _audioParams = new AudioParams();
            _audioParams.sample_rate = 16000;
            _audioParams.channels = 2;
            _audioParams.mode = RAW_AUDIO_FRAME_OP_MODE_TYPE.RAW_AUDIO_FRAME_OP_MODE_READ_ONLY;
            _audioParams.samples_per_call = 1024;
        }

        public override bool OnRecordAudioFrame(string channelId, AudioFrame audioFrame)
        {
            var floatArray = MyJoinChannelVideo.ConvertByteToFloat16(audioFrame.RawBuffer);
            if (!_myJoinChannelVideo.local_audioBuffer.ContainsKey((0, channelId)))
            {
                return false;
            }

            lock (_myJoinChannelVideo.local_audioBuffer[(0, channelId)])
            {
                MyDebug.Log("OnRecordAudioFrame-----------");
                _myJoinChannelVideo.local_audioBuffer[(0, channelId)].Put(floatArray);
            }
            return true;
        }

        public override int GetObservedAudioFramePosition()
        {
            MyDebug.Log("GetObservedAudioFramePosition-----------");
            return (int)(
                AUDIO_FRAME_POSITION.AUDIO_FRAME_POSITION_RECORD |
                AUDIO_FRAME_POSITION.AUDIO_FRAME_POSITION_BEFORE_MIXING);
        }

        public override AudioParams GetRecordAudioParams()
        {
            MyDebug.Log("GetRecordAudioParams-----------");
            return this._audioParams;
        }

        public override bool OnPlaybackAudioFrameBeforeMixing(string channel_id,
                                                        uint uid,
                                                        AudioFrame audio_frame)
        {
            var floatArray = MyJoinChannelVideo.ConvertByteToFloat16(audio_frame.RawBuffer);
            if (!_myJoinChannelVideo.local_audioBuffer.ContainsKey((uid, channel_id)))
            {
                return false;
            }
            lock (_myJoinChannelVideo.local_audioBuffer[(uid, channel_id)])
            {
                MyDebug.Log("OnPlaybackAudioFrameBeforeMixing-----------");
                _myJoinChannelVideo.local_audioBuffer[(uid, channel_id)].Put(floatArray);
            }
            return true;
        }
    }

    #endregion
}