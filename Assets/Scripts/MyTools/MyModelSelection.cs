using System;
using System.Collections;
using System.Collections.Generic;
using GameCreator.Characters;
using GameCreator.Core;
using GameCreator.Variables;
using MyTools;
using NetWorkManage;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MyUser_API))]
public class MyModelSelection : MonoBehaviour
{
    [SerializeField] public LocalVariables m_selectedModel;
    [SerializeField] public LocalVariables m_isChild;

    [SerializeField] public ListVariables m_characterModels;

    [SerializeField] public List<GameObject> m_ManModels = new List<GameObject>();
    [SerializeField] public List<GameObject> m_WomanModels = new List<GameObject>();
    [SerializeField] public List<GameObject> m_BoyModels = new List<GameObject>();
    [SerializeField] public List<GameObject> m_GirlModels = new List<GameObject>();

    public UnityEvent<MyUser_API.ResponseData> updateModels = new UnityEvent<MyUser_API.ResponseData>();


    // Start is called before the first frame update
    void Start()
    {
        UnityEventTools.AddPersistentListener<MyUser_API.ResponseData>(this.updateModels, this.UpdateListVariable);
    }

    private void UpdateListVariable(MyUser_API.ResponseData userInfo)
    {
        for (int i = m_characterModels.variables.Count - 1; i >= 0; --i)
        {
            m_characterModels.Remove(i);
        }
        bool isChild = true;
        switch (userInfo.Data.Sex)
        {
            case "男":
                if (userInfo.Data.RelativeType == MyUser_API.RelativeType.Father ||
                    userInfo.Data.RelativeType == MyUser_API.RelativeType.Grandfather ||
                    userInfo.Data.RelativeType == MyUser_API.RelativeType.Uncle || userInfo.Data.Age > 18)
                {
                    foreach (GameObject m in m_ManModels)
                    {
                        m_characterModels.Push(m);
                    }
                    isChild = false;
                }
                else if (userInfo.Data.RelativeType == MyUser_API.RelativeType.Brother ||
                        userInfo.Data.RelativeType == MyUser_API.RelativeType.Son || userInfo.Data.Age <= 18)
                {
                    foreach (GameObject m in m_BoyModels)
                    {
                        m_characterModels.Push(m);
                    }
                }
                else
                {
                    foreach (GameObject m in m_ManModels)
                    {
                        m_characterModels.Push(m);
                    }
                    foreach (GameObject m in m_BoyModels)
                    {
                        m_characterModels.Push(m);
                    }
                }
                break;
            case "女":
                if (userInfo.Data.RelativeType == MyUser_API.RelativeType.Mother ||
                        userInfo.Data.RelativeType == MyUser_API.RelativeType.Grandmother ||
                        userInfo.Data.RelativeType == MyUser_API.RelativeType.Aunt || userInfo.Data.Age > 18)
                {
                    foreach (GameObject m in m_WomanModels)
                    {
                        m_characterModels.Push(m);
                    }
                    isChild = false;
                }
                else if (userInfo.Data.RelativeType == MyUser_API.RelativeType.Daughter ||
                        userInfo.Data.RelativeType == MyUser_API.RelativeType.Sister || userInfo.Data.Age <= 18)
                {
                    foreach (GameObject m in m_GirlModels)
                    {
                        m_characterModels.Push(m);
                    }
                }
                else
                {
                    foreach (GameObject m in m_WomanModels)
                    {
                        m_characterModels.Push(m);
                    }
                    foreach (GameObject m in m_GirlModels)
                    {
                        m_characterModels.Push(m);
                    }
                }
                break;
            default:
                if (userInfo.Data.Age <= 18)
                {
                    foreach (GameObject m in m_BoyModels)
                    {
                        m_characterModels.Push(m);
                    }
                    foreach (GameObject m in m_GirlModels)
                    {
                        m_characterModels.Push(m);
                    }
                }
                else
                {
                    foreach (GameObject m in m_ManModels)
                    {
                        m_characterModels.Push(m);
                    }

                    foreach (GameObject m in m_WomanModels)
                    {
                        m_characterModels.Push(m);
                    }
                    isChild = false;
                }
                break;
        }
        
        if (m_isChild)
        {
            m_isChild.Get("isChild").Update(isChild);
        }
        if (!isChild)
        {
            MyMouseLock myMouseLock = GetComponent<MyMouseLock>();
            if (myMouseLock != null)
            {
                myMouseLock.setAdventureMotorTargetOffset(new Vector3(0, 1.5f, 0));
            }
        }

        // TODO: 
        int index = 0;
        CharacterAnimator targetCharAnim = GetComponentInChildren<CharacterAnimator>();
        targetCharAnim.ChangeModel(m_characterModels.Get(index).Get() as GameObject);
        if (m_selectedModel != null)
        {
            m_selectedModel.Get("SelectedModel").Update(index);
        }
    }
}
