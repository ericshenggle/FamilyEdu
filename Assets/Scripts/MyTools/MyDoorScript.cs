/*

 * 		Original Code by: NOT_lonely (www.facebook.com/notlonely92)
 * 		Code Revision by: sluice (www.sluicegaming.com)
 *
 */
using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace MyTools
{

    public class MyDoorScript : MonoBehaviour
    {
        public enum OpenStyle
        {
            BUTTON,
            AUTOMATIC
        }

        [Serializable]
        public class DoorControls
        {
            public float openingSpeed = 1;
            public float closingSpeed = 1.3f;
            [Range(0, 1)]
            public float closeStartFrom = 0.6f;
        }
        [Serializable]
        public class AnimNames //names of the animations, which you use for every action
        {
            public string OpeningAnim = "Door_open";
            public string LockedAnim = "Door_locked";
        }
        [Serializable]
        public class DoorSounds
        {
            public bool enabled = true;
            public AudioClip open;
            public AudioClip close;
            public AudioClip closed;
            [Range(0, 1.0f)]
            public float volume = 1.0f;
            [Range(0, 0.4f)]
            public float pitchRandom = 0.2f;
        }

        public DoorControls controls = new DoorControls();
        public AnimNames AnimationNames = new AnimNames();
        public DoorSounds doorSounds = new DoorSounds();

        Transform player;
        AudioSource SoundFX;
        Animation doorAnimation;
        Animation LockAnim;



        void Start()
        {
            AddAudioSource();
            doorAnimation = GetComponent<Animation>();
        }

        void AddAudioSource()
        {
            GameObject go = new GameObject("SoundFX");
            go.transform.position = transform.position;
            go.transform.rotation = transform.rotation;
            go.transform.parent = transform;
            SoundFX = go.AddComponent<AudioSource>();
            SoundFX.volume = doorSounds.volume;
            SoundFX.spatialBlend = 1;
            SoundFX.playOnAwake = false;
            SoundFX.clip = doorSounds.open;
        }


        void Update()
        {
            if (!doorAnimation.isPlaying && SoundFX.isPlaying)
            {
                SoundFX.Stop();
            }
        }

        #region AUDIO
        /*
         * 	AUDIO
         */
        void PlaySFX(AudioClip clip)
        {
            if (!doorSounds.enabled)
                return;

            SoundFX.pitch = UnityEngine.Random.Range(1 - doorSounds.pitchRandom, 1 + doorSounds.pitchRandom);
            SoundFX.clip = clip;
            SoundFX.Play();
        }

        void PlayClosedFXs()
        {
            if (doorSounds.closed != null)
            {
                SoundFX.clip = doorSounds.closed;
                SoundFX.Play();
                if (doorAnimation[AnimationNames.LockedAnim] != null)
                {
                    doorAnimation.Play(AnimationNames.LockedAnim);
                    doorAnimation[AnimationNames.LockedAnim].speed = 1;
                    doorAnimation[AnimationNames.LockedAnim].normalizedTime = 0;
                }
            }
        }

        void CloseSound()
        {
            if (doorAnimation[AnimationNames.OpeningAnim].speed < 0 && doorSounds.close != null)
                PlaySFX(doorSounds.close);
        }
        #endregion


        public void OpenDoor()
        {
            doorAnimation.Play(AnimationNames.OpeningAnim);
            doorAnimation[AnimationNames.OpeningAnim].speed = controls.openingSpeed;
            doorAnimation[AnimationNames.OpeningAnim].normalizedTime = doorAnimation[AnimationNames.OpeningAnim].normalizedTime;

            if (doorSounds.open != null)
                PlaySFX(doorSounds.open);
        }

        public void CloseDoor()
        {
            if (doorAnimation[AnimationNames.OpeningAnim].normalizedTime < 0.98f && doorAnimation[AnimationNames.OpeningAnim].normalizedTime > 0)
            {
                doorAnimation[AnimationNames.OpeningAnim].speed = -controls.closingSpeed;
                doorAnimation[AnimationNames.OpeningAnim].normalizedTime = doorAnimation[AnimationNames.OpeningAnim].normalizedTime;
                doorAnimation.Play(AnimationNames.OpeningAnim);
            }
            else
            {
                doorAnimation[AnimationNames.OpeningAnim].speed = -controls.closingSpeed;
                doorAnimation[AnimationNames.OpeningAnim].normalizedTime = controls.closeStartFrom;
                doorAnimation.Play(AnimationNames.OpeningAnim);
            }
            if (doorAnimation[AnimationNames.OpeningAnim].normalizedTime > controls.closeStartFrom)
            {
                doorAnimation[AnimationNames.OpeningAnim].speed = -controls.closingSpeed;
                doorAnimation[AnimationNames.OpeningAnim].normalizedTime = controls.closeStartFrom;
                doorAnimation.Play(AnimationNames.OpeningAnim);
            }
        }

    }
}