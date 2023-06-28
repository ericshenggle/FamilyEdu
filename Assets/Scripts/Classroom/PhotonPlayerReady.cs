using System.Collections;
using System.Collections.Generic;
using GameCreator.Characters;
using GameCreator.Core;
using GameCreator.Core.Hooks;
using GameCreator.Variables;
using MyTools;
using NJG.PUN;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Classroom
{
    public class PhotonPlayerReady : MonoBehaviour
    {
        public LocalVariables localPlayerVariables;
        public Text m_text;
        private int playerCount;
        private bool isInControlling;
        void OnEnable()
        {
            if (localPlayerVariables != null)
            {
                playerCount = Mathf.FloorToInt(localPlayerVariables.Get("MaxPlayer").Get<float>());
                MyDebug.Log(playerCount);
            }
            isInControlling = false;
            m_text.text = "请等待参与当前课程所有人入会";
        }

        void OnDisable()
        {
            isInControlling = false;
            m_text.text = "请等待参与当前课程所有人入会";
        }

        // Update is called once per frame
        void Update()
        {
            if (localPlayerVariables != null)
            {
                playerCount = Mathf.FloorToInt(localPlayerVariables.Get("MaxPlayer").Get<float>());
            }
            if (PhotonNetwork.InRoom && !isInControlling)
            {
                Player[] players = PhotonNetwork.PlayerList;
                int t_count = players.Length;
                Player player = PhotonNetwork.LocalPlayer;
                if (player?.TagObject != null)
                {
                    GameObject invoker = player.TagObject as GameObject;
                    Character charater = invoker.GetComponent<Character>();
                    if (t_count != 0 && t_count == playerCount - 1)
                    {
                        charater.characterLocomotion.SetIsControllable(true);
                        isInControlling = true;
                        m_text.text = "";
                    }
                    else
                    {
                        charater.characterLocomotion.SetIsControllable(false);
                    }
                }
            }
            else if (PhotonNetwork.InRoom && isInControlling)
            {
                Player[] players = PhotonNetwork.PlayerList;
                int t_count = players.Length;
                if (t_count != 0 && t_count < playerCount - 1)
                {
                    isInControlling = false;
                    PhotonNetwork.Disconnect();
                }
            }

        }
    }
}

