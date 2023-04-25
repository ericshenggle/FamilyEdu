/// <summary>
/// Project : Easy Build System
/// Class : BuildingPlacerInput.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Runtime.Extensions;
using EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler;
using EasyBuildSystem.Features.Runtime.Buildings.Placer;

namespace HomeScripts
{
    public class MyStandaloneInputHandler : BaseInputHandler
    {
        #region Fields

        [Serializable]
        public class MyInputSettings
        {
            [SerializeField] bool m_BlockWhenCursorOverUI = false;
            public bool BlockWhenCursorOverUI { get { return m_BlockWhenCursorOverUI; } }

            [SerializeField] bool m_CanRotateBuildingPart = true;
            public bool CanRotateBuildingPart { get { return m_CanRotateBuildingPart; } }

            [SerializeField] bool m_CanSelectBuildingPart = true;
            public bool CanSelectBuildingPart { get { return m_CanSelectBuildingPart; } }

            [SerializeField] KeyCode m_ValidateActionKey = KeyCode.F;
            public KeyCode ValidateActionKey { get { return m_ValidateActionKey; } }

            [SerializeField] KeyCode m_CancelActionKey = KeyCode.G;
            public KeyCode CancelActionKey { get { return m_CancelActionKey; } }

            [SerializeField] bool m_ResetModeAfterPlacing = false;
            public bool ResetModeAfterPlacing { get { return m_ResetModeAfterPlacing; } }

            [SerializeField] bool m_ResetModeAfterEditing = false;
            public bool ResetModeAfterEditing { get { return m_ResetModeAfterEditing; } }

            [SerializeField] bool m_ResetModeAfterDestroying = false;
            public bool ResetModeAfterDestroying { get { return m_ResetModeAfterDestroying; } }
        }
        [SerializeField] MyInputSettings m_MyInputSettings = new MyInputSettings();
        public MyInputSettings GetInputSettings { get { return m_MyInputSettings; } set { m_MyInputSettings = value; } }

        int m_SelectionIndex;

        bool isEnabled;

        #endregion

        #region Unity Methods

        void Start()
        {
            isEnabled = false;
        }

        void Update()
        {
            if (Placer == null)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                return;
            }

            if (m_MyInputSettings.BlockWhenCursorOverUI)
            {
                if (UIExtension.IsPointerOverUIElement() && Cursor.lockState != CursorLockMode.Locked)
                {
                    return;
                }
            }

            if (!isEnabled) {
                return;
            }

            this.HandleBuildModes();
        }

        #endregion

        #region Public Methods

        public void startBuilding() 
        {
            isEnabled = true;
        }

        public void stopBuilding() 
        {
            isEnabled = false;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Handle the building part selection and update the build modes.
        /// </summary>
        private void HandleBuildModes()
        {
#if UNITY_ANDROID
            return;
#else

            if (Placer.GetBuildMode == BuildingPlacer.BuildMode.PLACE)
            {
                this.HandlePlacingMode();
            }

            if (Placer.GetBuildMode == BuildingPlacer.BuildMode.DESTROY)
            {
                this.HandleDestroyMode();
            }

            if (Placer.GetBuildMode == BuildingPlacer.BuildMode.EDIT)
            {
                this.HandleEditingMode();
            }
#endif
        }

        /// <summary>
        /// Handle placing mode according to the user inputs.
        /// </summary>
        private void HandlePlacingMode()
        {
            if (Input.GetKeyDown(m_MyInputSettings.ValidateActionKey))
            {
                if (Placer.PlacingBuildingPart())
                {
                    if (m_MyInputSettings.ResetModeAfterPlacing)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }

                    if (m_MyInputSettings.ResetModeAfterEditing && Placer.LastBuildMode == BuildingPlacer.BuildMode.EDIT)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }
                }
            }

            if (m_MyInputSettings.CanRotateBuildingPart)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    Placer.RotatePreview();
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    Placer.RotatePreview(true);
                }
            }

            if (Input.GetKeyDown(m_MyInputSettings.CancelActionKey))
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        /// <summary>
        /// Handle destroy mode according to the user inputs.
        /// </summary>
        private void HandleDestroyMode()
        {
            if (Input.GetKeyDown(m_MyInputSettings.ValidateActionKey))
            {
                if (Placer.DestroyBuildingPart())
                {
                    if (m_MyInputSettings.ResetModeAfterDestroying)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }
                }
            }

            if (Input.GetKeyDown(m_MyInputSettings.CancelActionKey))
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        /// <summary>
        /// Handle editing mode according the user inputs.
        /// </summary>
        private void HandleEditingMode()
        {
            if (Input.GetKeyDown(m_MyInputSettings.ValidateActionKey))
            {
                Placer.EditingBuildingPart();
            }

            if (Input.GetKeyDown(m_MyInputSettings.CancelActionKey))
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        #endregion
    }
}