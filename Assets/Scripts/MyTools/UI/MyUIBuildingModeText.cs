/// <summary>
/// Project : Easy Build System
/// Class : Demo_UIBuildingModeText.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.UI
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEngine.UI;

using EasyBuildSystem.Features.Runtime.Buildings.Placer;

namespace MyTools.UI
{
    public class MyUIBuildingModeText : MonoBehaviour
    {
        Text m_BuildingModeText;

        void Awake()
        {
            m_BuildingModeText = GetComponent<Text>();
        }

        void Update()
        {
            if (BuildingPlacer.Instance == null)
            {
                return;
            }
            switch (BuildingPlacer.Instance.GetBuildMode.ToString())
            {
                case "NONE":
                    m_BuildingModeText.text = "当前模式 : 无";
                    break;
                case "PLACE":
                    m_BuildingModeText.text = "当前模式 : 放置";
                    break;
                case "DESTROY":
                    m_BuildingModeText.text = "当前模式 : 删除";
                    break;
                case "EDIT":
                    m_BuildingModeText.text = "当前模式 : 编辑";
                    break;
            }
        }
    }
}