using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace MyTools
{
    public class MyDebug : MonoBehaviour
    {
     //需要在Unity PlayerSettings -> Scripting Define Symbol 添加VERBOSE
     //Release发布时去掉VERBOSE，所有的MyDebug.Log函数调用将不会参与编译。
     //这是简单的实现方式 , 后期功能的扩展以及Debug和Release自动切换机制，可以开发者进行封装。
     [Conditional("VERBOSE")] 
     public static void Log(object message, UnityEngine.Object obj = null) {
         UnityEngine.Debug.Log(message, obj);
     }
 
     public static void LogWarning(object message, UnityEngine.Object obj = null) {
         UnityEngine.Debug.LogWarning(message, obj);
     }
 
     public static void LogError(object message, UnityEngine.Object obj = null) {
         UnityEngine.Debug.LogError(message, obj);
     }
 }
}

