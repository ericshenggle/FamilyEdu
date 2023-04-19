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

namespace Classroom
{
    public class UserConnectInitate : MonoBehaviour
    {
        #region UI

        [Header("UI Components")]
        public InputField m_Input_username;
        public Text roomName;
        public Text roomDescription;
        public Button m_Button_connectToPhoton;
        public Dropdown m_Dropdown_courseSelector;

        #endregion

        public LocalVariables localPlayerVariables;
        public MyMouseLock myMouseLock;

        #region PRIVATE PROPERTY

        private bool userProperty;
        private bool userCourseProperty;
        private bool userModelProperty;

        private MyUser_API.ResponseData myUserData;
        private MyUserCourse_API.ResponseData myUserCourseData;
        private MyCharacterModel_API.ResponseData myUserModelData;
        private string userPrebName;
        private bool isChild;

        [SerializeField] private List<string> m_ManModels = new List<string> { "Player MAN 1", "Player MAN 2", "Player MAN 3", "Player MAN 4", "Player MAN 5" };
        [SerializeField] private List<string> m_WomanModels = new List<string> { "Player WOMAN 1", "Player WOMAN 2", "Player WOMAN 3", "Player WOMAN 4", "Player WOMAN 5" };
        [SerializeField] private List<string> m_BoyModels = new List<string> { "Player BOY 1", "Player BOY 2", "Player BOY 3", "Player BOY 4", "Player BOY 5" };
        [SerializeField] private List<string> m_GirlModels = new List<string> { "Player GIRL 1", "Player GIRL 1" };

        public UnityEvent<MyUser_API.ResponseData> userResponseEvent = new UnityEvent<MyUser_API.ResponseData>();
        public UnityEvent<MyUserCourse_API.ResponseData> userCourseResponseEvent = new UnityEvent<MyUserCourse_API.ResponseData>();
        public UnityEvent<MyCharacterModel_API.ResponseData> userModelResponseEvent = new UnityEvent<MyCharacterModel_API.ResponseData>();


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
            userResponseEvent.AddListener(UpdateUserData);
            userCourseResponseEvent.AddListener(UpdateUserCourseData);
            userModelResponseEvent.AddListener(UpdateUserModelData);

            if (RequestSender.Instance != null)
            {
                long id = RequestSender.Instance.UserId;
                MyUser_API.getUserInfoRequestInClassroom(id, this);
                MyUserCourse_API.getUserCourseInfoRequestInClassroom(id, this);
            }
            myMouseLock.openMenu();
        }

        void Update()
        {
            if (userProperty)
            {
                m_Input_username.interactable = true;
            }
            else
            {
                m_Input_username.interactable = false;
            }
            if (userProperty && userModelProperty && userCourseProperty)
            {
                m_Button_connectToPhoton.interactable = true;
            }
            else
            {
                m_Button_connectToPhoton.interactable = false;
            }
        }

        void OnDestroy()
        {
            userResponseEvent.RemoveListener(UpdateUserData);
            userCourseResponseEvent.RemoveListener(UpdateUserCourseData);
            userModelResponseEvent.RemoveListener(UpdateUserModelData);
        }

        public void UpdateUserProperty()
        {
            if (localPlayerVariables != null)
            {
                if (!userProperty && myUserData != null)
                {
                    m_Input_username.text += myUserData.Data.Name;
                    localPlayerVariables.Get("userName").Update(myUserData.Data.Name);
                    localPlayerVariables.Get("userId").Update((int)myUserData.Data.Id);
                    userProperty = true;
                    MyDebug.Log("Update UserProperty!");
                }
            }
        }

        public void UpdateUserCourseProperty(int index)
        {
            if (localPlayerVariables != null)
            {
                if (!userCourseProperty && myUserCourseData != null)
                {
                    roomName.text += myUserCourseData.Data[index].Name;
                    roomDescription.text += myUserCourseData.Data[index].Description;
                    int i = Array.IndexOf(myUserCourseData.Data[index].TeacherIds, myUserData.Data.Id);
                    localPlayerVariables.Get("isTeacher").Update(i != -1 ? true : false);
                    localPlayerVariables.Get("RoomName").Update(myUserCourseData.Data[index].Name + "(ID:" + myUserCourseData.Data[index].Id + ")");
                    localPlayerVariables.Get("MaxPlayer").Update(myUserCourseData.Data[index].TeacherIds.Length + myUserCourseData.Data[index].StudentIds.Length);
                    userCourseProperty = true;
                    MyDebug.Log("Update UserCourseProperty!");
                }
            }
        }

        public void UpdateUserModelProperty()
        {
            if (localPlayerVariables != null)
            {
                if (!userModelProperty && myUserData != null && myUserModelData != null)
                {
                    chooseModel(myUserData, (int)myUserModelData.Data);
                    localPlayerVariables.Get("userPrebName").Update(userPrebName);
                    localPlayerVariables.Get("isChild").Update(isChild);
                    userModelProperty = true;
                    MyDebug.Log("Update UserModelProperty!");
                }
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

        public void UpdateUserData(MyUser_API.ResponseData userInfo)
        {
            this.myUserData = userInfo;
            UpdateUserProperty();
            MyCharacterModel_API.getUserModelIdRequestInClassroom((long)userInfo.Data.Id, this);
        }

        public void UpdateUserCourseData(MyUserCourse_API.ResponseData userCourseInfo)
        {
            this.myUserCourseData = userCourseInfo;
            m_Dropdown_courseSelector.ClearOptions();
            List<Dropdown.OptionData> listOptions = new List<Dropdown.OptionData>();
            foreach (MyUserCourse_API.CourseInfo courseInfo in this.myUserCourseData.Data)
            {
                listOptions.Add(new Dropdown.OptionData(courseInfo.Name + "    课程老师人数：" + courseInfo.TeacherIds.Length
                 + " 课程学生人数：" + courseInfo.StudentIds.Length));
            }
            listOptions.Add(new Dropdown.OptionData("请选择入会的课程"));
            m_Dropdown_courseSelector.AddOptions(listOptions);
            m_Dropdown_courseSelector.value = m_Dropdown_courseSelector.options.Count - 1;
            m_Dropdown_courseSelector.onValueChanged.AddListener((value) =>
            {
                if (value == m_Dropdown_courseSelector.options.Count - 1)
                {
                    userCourseProperty = false;
                    roomName.text = "";
                    roomDescription.text = "";
                    return;
                }
                UpdateUserCourseProperty(value);
            });
        }

        public void UpdateUserModelData(MyCharacterModel_API.ResponseData userModelInfo)
        {
            this.myUserModelData = userModelInfo;
            MyDebug.Log("arrive: updateUserModelData");
            UpdateUserModelProperty();
        }


    }
}