using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyTools;
using NetWorkManage;
using UnityEngine.Events;
using System;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using NJG.PUN;
using GameCreator.Variables;
using Button = GameCreator.Core.ButtonActions;

public class UserConnectInitate : MonoBehaviour
{
    #region UI

    [Header("UI Components")]
    public Text userName;
    public Text roomName;
    public Text roomDescription;
    public Button connectToPhoton;

    #endregion

    public LocalVariables localPlayerVariables;

    #region PRIVATE PROPERTY

    private bool updateUserProperty;
    private bool updateUserCourseProperty;
    private bool updateUserModelProperty;

    private MyUser_API.ResponseData myUserData;
    private MyUserCourse_API.ResponseData myUserCourseData;
    private MyCharacterModel_API.ResponseData myUserModelData;

    public UnityEvent<MyUser_API.ResponseData> userResponseEvent = new UnityEvent<MyUser_API.ResponseData>();
    public UnityEvent<MyUserCourse_API.ResponseData> userCourseResponseEvent = new UnityEvent<MyUserCourse_API.ResponseData>();
    public UnityEvent<MyCharacterModel_API.ResponseData> userModelResponseEvent = new UnityEvent<MyCharacterModel_API.ResponseData>();
    private string userPrebName;
    private bool isChild;

    [SerializeField] private List<string> m_ManModels = new List<string> { "Player MAN 1", };
    [SerializeField] private List<string> m_WomanModels = new List<string> { };
    [SerializeField] private List<string> m_BoyModels = new List<string> { };
    [SerializeField] private List<string> m_GirlModels = new List<string> { };

    private Player m_player
    {
        get
        {
            return PhotonNetwork.LocalPlayer == null ? null : PhotonNetwork.LocalPlayer;
        }
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        userResponseEvent.AddListener(updateUserData);
        userCourseResponseEvent.AddListener(updateUserCourseData);
        userModelResponseEvent.AddListener(updateUserModelData);

        if (RequestSender.Instance != null)
        {
            long id = RequestSender.Instance.UserId;
            MyUser_API.getUserInfoRequestInClassroom(id, this);
            MyUserCourse_API.getUserCourseInfoRequestInClassroom(id, this);
            MyCharacterModel_API.getUserModelIdRequestInClassroom(id, this);
        }
        this.InvokeRepeating("UpdateProperty", 2, 1);
    }

    void Update()
    {
        if (updateUserProperty && updateUserCourseProperty && updateUserModelProperty)
        {
            connectToPhoton.interactable = true;
        }
        else
        {
            connectToPhoton.interactable = false;
        }
    }

    void OnDestroy()
    {
        userResponseEvent.RemoveListener(updateUserData);
        userCourseResponseEvent.RemoveListener(updateUserCourseData);
        userModelResponseEvent.RemoveListener(updateUserModelData);
    }

    public void UpdateProperty()
    {
        if (myUserData != null)
        {
            userName.text += myUserData.Data.Name;
            m_player.NickName = myUserData.Data.Name;
            m_player.SetInt("userId", (int)myUserData.Data.Id, true);
            updateUserProperty = true;
        }
        else
        {
            updateUserProperty = false;
        }
        if (myUserCourseData != null)
        {
            roomName.text += myUserCourseData.Data.Name;
            roomDescription.text += myUserCourseData.Data.Description;
            int i = Array.IndexOf(myUserCourseData.Data.TeacherIds, myUserData.Data.Id);
            m_player.SetBool("isTeacher", (i != -1 ? true : false), true);
            if (localPlayerVariables != null) {
                localPlayerVariables.Get("RoomName").Update(myUserCourseData.Data.Name + " ID " + myUserCourseData.Data.Id);
            }
            updateUserCourseProperty = true;
        }
        else
        {
            updateUserCourseProperty = false;
        }
        if (myUserData != null && myUserModelData != null)
        {
            chooseModel(myUserData, (int)myUserModelData.Data);
            m_player.SetString("userPrebName", userPrebName, true);
            m_player.SetBool("isChild", isChild, true);
            updateUserModelProperty = true;
        }
        else
        {
            updateUserModelProperty = false;
        }

    }

    public void chooseModel(MyUser_API.ResponseData userInfo, int index)
    {
        List<string> t_characterModels = new List<string>();
        isChild = true;

        switch (userInfo.Data.Sex)
        {
            case "男":
                if (userInfo.Data.RelativeType == MyUser_API.RelativeType.Father ||
                    userInfo.Data.RelativeType == MyUser_API.RelativeType.Grandfather ||
                    userInfo.Data.RelativeType == MyUser_API.RelativeType.Uncle || userInfo.Data.Age > 18)
                {
                    foreach (string m in m_ManModels)
                    {
                        t_characterModels.Add(m);
                    }
                    isChild = false;
                }
                else if (userInfo.Data.RelativeType == MyUser_API.RelativeType.Brother ||
                        userInfo.Data.RelativeType == MyUser_API.RelativeType.Son || userInfo.Data.Age <= 18)
                {
                    foreach (string m in m_BoyModels)
                    {
                        t_characterModels.Add(m);
                    }
                }
                else
                {
                    foreach (string m in m_ManModels)
                    {
                        t_characterModels.Add(m);
                    }
                    foreach (string m in m_BoyModels)
                    {
                        t_characterModels.Add(m);
                    }
                }
                break;
            case "女":
                if (userInfo.Data.RelativeType == MyUser_API.RelativeType.Mother ||
                        userInfo.Data.RelativeType == MyUser_API.RelativeType.Grandmother ||
                        userInfo.Data.RelativeType == MyUser_API.RelativeType.Aunt || userInfo.Data.Age > 18)
                {
                    foreach (string m in m_WomanModels)
                    {
                        t_characterModels.Add(m);
                    }
                    isChild = false;
                }
                else if (userInfo.Data.RelativeType == MyUser_API.RelativeType.Daughter ||
                        userInfo.Data.RelativeType == MyUser_API.RelativeType.Sister || userInfo.Data.Age <= 18)
                {
                    foreach (string m in m_GirlModels)
                    {
                        t_characterModels.Add(m);
                    }
                }
                else
                {
                    foreach (string m in m_WomanModels)
                    {
                        t_characterModels.Add(m);
                    }
                    foreach (string m in m_GirlModels)
                    {
                        t_characterModels.Add(m);
                    }
                }
                break;
            default:
                if (userInfo.Data.Age <= 18)
                {
                    foreach (string m in m_BoyModels)
                    {
                        t_characterModels.Add(m);
                    }
                    foreach (string m in m_GirlModels)
                    {
                        t_characterModels.Add(m);
                    }
                }
                else
                {
                    foreach (string m in m_ManModels)
                    {
                        t_characterModels.Add(m);
                    }

                    foreach (string m in m_WomanModels)
                    {
                        t_characterModels.Add(m);
                    }
                    isChild = false;
                }
                break;
        }
        if (index > t_characterModels.Count)
        {
            index = 0;
        }

        userPrebName = t_characterModels[index];
    }

    public string getPrebName()
    {
        return "";
    }

    public void updateUserData(MyUser_API.ResponseData userInfo)
    {
        this.myUserData = userInfo;
    }

    public void updateUserCourseData(MyUserCourse_API.ResponseData userCourseInfo)
    {
        this.myUserCourseData = userCourseInfo;
    }

    public void updateUserModelData(MyCharacterModel_API.ResponseData userModelInfo)
    {
        this.myUserModelData = userModelInfo;
    }


}
