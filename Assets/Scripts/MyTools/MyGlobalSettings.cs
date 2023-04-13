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
            Application.targetFrameRate = targetFrameRate;
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = gameObject;
                DontDestroyOnLoad(gameObject);
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
