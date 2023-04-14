using System;
using System.Collections;
using System.Collections.Generic;
using GameCreator.Variables;
using UnityEngine;

namespace MyTools
{

    public class MyGlobalSettings : MonoBehaviour
    {
        [SerializeField] public int targetFrameRate = 60;

        public static GameObject Instance { get; set; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                MyDebug.Log("Destroy latest gameobject");
            }
            else
            {
                Instance = gameObject;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            Application.targetFrameRate = targetFrameRate;
        }

    }
}
