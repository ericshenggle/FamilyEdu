using System.Collections;
using System.Collections.Generic;
using GameCreator.Camera;
using UnityEngine;

namespace MyTools
{


    public class MyMouseLock : MonoBehaviour
    {
        public CameraMotor cameraMotor;

        public KeyCode keyCode = KeyCode.LeftControl;

        private bool isCursorLocked = true;
        private bool isMenuOpen = false;
        private bool isGamePaused = false;

        void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isCursorLocked = true;
        }

        void Update()
        {
            if (!isGamePaused && !isMenuOpen)
            {
                if (Input.GetKeyDown(this.keyCode))
                {
                    UnlockCursor();
                }
                if (Input.GetKeyUp(this.keyCode))
                {
                    LockCursor();
                }

                if (isCursorLocked)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        UnlockCursor();
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        LockCursor();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                isGamePaused = !isGamePaused;
                if (isGamePaused)
                {
                    UnlockCursor();
                }
                else
                {
                    LockCursor();
                }
            }
        }

        void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isCursorLocked = true;
            fixAdventureMotor(true);
        }

        void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isCursorLocked = false;
            fixAdventureMotor(false);
        }

        public void fixAdventureMotor(bool status)
        {
            if (this.cameraMotor != null && this.cameraMotor.cameraMotorType.GetType() == typeof(CameraMotorTypeAdventure))
            {
                CameraMotorTypeAdventure adventureMotor = (CameraMotorTypeAdventure)this.cameraMotor.cameraMotorType;
                adventureMotor.allowOrbitInput = status;
                adventureMotor.allowZoom = status;
                adventureMotor.autoRepositionBehind = status;
            }
        }

        public void setAdventureMotorTargetOffset(Vector3 offset) {
            if (this.cameraMotor != null && this.cameraMotor.cameraMotorType.GetType() == typeof(CameraMotorTypeAdventure))
            {
                CameraMotorTypeAdventure adventureMotor = (CameraMotorTypeAdventure)this.cameraMotor.cameraMotorType;
                adventureMotor.targetOffset = offset;
            }
        }

        public void openMenu()
        {
            isMenuOpen = true;
            UnlockCursor();
        }

        public void closeMenu()
        {
            isMenuOpen = false;
            LockCursor();
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (isCursorLocked && hasFocus)
            {
                LockCursor();
            }
        }

    }

}