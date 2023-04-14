/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollisionSurface.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Manager;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
{
    [ExecuteInEditMode]
    public class BuildingCollisionSurface : MonoBehaviour
    {
        #region Fields

        [SerializeField] string m_SurfaceIdentifier;
        public string SurfaceIdentifier { get { return m_SurfaceIdentifier; } set { m_SurfaceIdentifier = value; } }

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            m_SurfaceIdentifier = name;

            if (BuildingManager.Instance != null)
            {
                if (!BuildingManager.Instance.AllSurfaces.Contains(m_SurfaceIdentifier))
                {
                    BuildingManager.Instance.AllSurfaces.Add(m_SurfaceIdentifier);
                }
            }
        }

        void OnDisable()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.AllSurfaces.Remove(m_SurfaceIdentifier);
            }
        }

        #endregion
    }
}