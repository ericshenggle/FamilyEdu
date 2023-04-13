using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTools
{
    public class MySingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T m_Instance;
        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = (T)FindObjectOfType(typeof(T));
                }

                return m_Instance;
            }
        }
    }
}

