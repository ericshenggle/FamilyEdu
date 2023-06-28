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
        /// <summary>
        /// 输入Agora提供的：
        /// 1. 私用appID
        /// 2. 临时频道Token
        /// 3. 频道名
        /// </summary>
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
        public const uint VOLUME_THRESHOLD = 150;

        public Room CurrentRoom
        {
            get
            {
                if (PhotonNetwork.InRoom)
                {
                    return PhotonNetwork.CurrentRoom;
                }
                return null;
            }
        }

        public Player LocalPlayer
        {
            get
            {
                if (PhotonNetwork.InRoom)
                {
                    return PhotonNetwork.LocalPlayer;
                }
                return null;
            }
        }

        public int LocalActNumber
        {
            get
            {
                return LocalPlayer == null ? 0 : LocalPlayer.ActorNumber;
            }
        }

        internal int _count;
        internal int _writeCount;
        internal int _readCount;

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
            RtcEngine.AdjustRecordingSignalVolume(300);
            RtcEngine.EnableAudioVolumeIndication(200, 3, true);

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


            RtcEngine.JoinChannel(_token, _channelName, "", (uint)LocalActNumber);
            MakeVideoView(0, LocalPlayer, GetChannelName());
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
                StopPlayAudio(player);
            }
        }

        public void StartPreview()
        {
            RtcEngine.StartPreview();
            MakeVideoView(0, LocalPlayer, GetChannelName());
        }

        public void StopPreview()
        {
            DestroyVideoView(0);
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

        internal void StartPlayAudio(Player player)
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
            aud.enabled = true;
            aud.mute = true;
            aud.loop = true;
            aud.Play();
        }

        internal void StopPlayAudio(Player player)
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
        private List<uint> speakingPlayers;

        internal UserEventHandler(MyJoinChannelVideo videoSample)
        {
            _myJoinChannelVideo = videoSample;
            speakingPlayers = new List<uint>();
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
            Player player = _myJoinChannelVideo.CurrentRoom.GetPlayer((int)uid);
            _myJoinChannelVideo.MakeVideoView(uid, player, _myJoinChannelVideo.GetChannelName());
            _myJoinChannelVideo.StopPlayAudio(player);
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _myJoinChannelVideo.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            Player player = _myJoinChannelVideo.CurrentRoom.GetPlayer((int)uid);
            _myJoinChannelVideo.DestroyVideoView(uid);
            _myJoinChannelVideo.StopPlayAudio(player);
        }

        public override void OnUplinkNetworkInfoUpdated(UplinkNetworkInfo info)
        {
            _myJoinChannelVideo.Log.UpdateLog("OnUplinkNetworkInfoUpdated");
        }

        public override void OnDownlinkNetworkInfoUpdated(DownlinkNetworkInfo info)
        {
            _myJoinChannelVideo.Log.UpdateLog("OnDownlinkNetworkInfoUpdated");
        }

        public override void OnAudioVolumeIndication(RtcConnection connection, AudioVolumeInfo[] speakers, uint speakerNumber, int totalVolume)
        {
            if (connection.channelId != _myJoinChannelVideo.GetChannelName())
            {
                return;
            }
            foreach (AudioVolumeInfo audioVolumeInfo in speakers)
            {
                MyDebug.Log("用户: " + audioVolumeInfo.uid + " 音量: " + audioVolumeInfo.volume + " 人声: " + audioVolumeInfo.vad);
                if (audioVolumeInfo.uid == 0)   // 本地用户 
                {
                    if (audioVolumeInfo.vad == 1)
                    {
                        _myJoinChannelVideo.StartPlayAudio(_myJoinChannelVideo.LocalPlayer);
                    }
                    else
                    {
                        _myJoinChannelVideo.StopPlayAudio(_myJoinChannelVideo.LocalPlayer);
                    }
                }
                else if (speakingPlayers.Contains(audioVolumeInfo.uid))
                {
                    if (audioVolumeInfo.volume <= MyJoinChannelVideo.VOLUME_THRESHOLD)
                    {
                        Player player = _myJoinChannelVideo.CurrentRoom.GetPlayer((int)audioVolumeInfo.uid);
                        _myJoinChannelVideo.StopPlayAudio(player);
                        speakingPlayers.Remove(audioVolumeInfo.uid);
                    }
                }
                else
                {
                    if (audioVolumeInfo.volume > MyJoinChannelVideo.VOLUME_THRESHOLD)
                    {
                        Player player = _myJoinChannelVideo.CurrentRoom.GetPlayer((int)audioVolumeInfo.uid);
                        _myJoinChannelVideo.StartPlayAudio(player);
                        speakingPlayers.Add(audioVolumeInfo.uid);
                    }
                }

            }
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
            _audioParams.sample_rate = 8000;
            _audioParams.channels = 1;
            _audioParams.mode = RAW_AUDIO_FRAME_OP_MODE_TYPE.RAW_AUDIO_FRAME_OP_MODE_READ_ONLY;
            _audioParams.samples_per_call = 1024;
        }

        public override bool OnPlaybackAudioFrameBeforeMixing(string channel_id,
                                                        uint uid,
                                                        AudioFrame audio_frame)
        {
            Debug.LogFormat("OnPlaybackAudioFrameBeforeMixing-----------uid:{0} {1} {2}",
                        uid, audio_frame.samplesPerChannel, audio_frame.samplesPerSec);
            return true;
        }
    }

    #endregion
}