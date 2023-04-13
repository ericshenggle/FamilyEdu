using System;
using System.Collections.Generic;
using System.Globalization;
using MyTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.UI;

namespace NetWorkManage
{
    public class MyModel_API : MonoBehaviour
    {
        void Start()
        {
            MyDebug.Log(Application.persistentDataPath);
        }
    }
}
