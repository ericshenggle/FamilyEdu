#if EBS_PUNV2

using UnityEngine;

using Photon.Pun;

using EasyBuildSystem.Features.Runtime.Buildings.Placer;

namespace EasyBuildSystem.Packages.Integrations.PUNV2
{
    public class PUNBuildingPlacer : BuildingPlacer
    {
        PUNBuildingPlacerReflector m_Reflector;

        void Awake()
        {
            m_Reflector = GetComponentInParent<PUNBuildingPlacerReflector>();
        }

        public override bool PlacingBuildingPart()
        {
            if (!HasPreview())
            {
                return false;
            }

            if (!CanPlacing)
            {
                return false;
            }

            m_Reflector.gameObject.GetPhotonView().RPC("RPCPlace", RpcTarget.MasterClient, GetSelectedBuildingPart.GetGeneralSettings.Identifier,
                GetCurrentPreview.transform.position, GetCurrentPreview.transform.eulerAngles, Vector3.one);

            if (LastBuildMode == BuildMode.EDIT)
            {
                ChangeBuildMode(BuildMode.EDIT, true);
            }
            else
            {
                CancelPreview();
            }

            if (GetAudioSettings.AudioSource != null)
            {
                if (GetAudioSettings.PlacingAudioClips.Length != 0)
                {
                    GetAudioSettings.AudioSource.PlayOneShot(GetAudioSettings.PlacingAudioClips[Random.Range(0,
                        GetAudioSettings.PlacingAudioClips.Length)]);
                }
            }

            return true;
        }

        public override bool EditingBuildingPart()
        {
            if (!HasPreview())
            {
                return false;
            }

            if (!CanEditing)
            {
                return false;
            }

            SelectBuildingPart(GetCurrentPreview);

            m_Reflector.gameObject.GetPhotonView().RPC("RPCDestroy", RpcTarget.MasterClient, GetCurrentPreview.gameObject.GetPhotonView().ViewID);

            GetCurrentPreview.ChangeState(EasyBuildSystem.Features.Runtime.Buildings.Part.BuildingPart.StateType.PREVIEW);

            ChangeBuildMode(BuildMode.PLACE, false);

            if (GetAudioSettings.AudioSource != null)
            {
                if (GetAudioSettings.EditingAudioClips.Length != 0)
                {
                    GetAudioSettings.AudioSource.PlayOneShot(GetAudioSettings.EditingAudioClips[Random.Range(0,
                        GetAudioSettings.EditingAudioClips.Length)]);
                }
            }

            return true;
        }

        public override bool DestroyBuildingPart()
        {
            if (!HasPreview())
            {
                return false;
            }

            if (!CanDestroy)
            {
                return false;
            }

            m_Reflector.gameObject.GetPhotonView().RPC("RPCDestroy", RpcTarget.MasterClient, GetCurrentPreview.gameObject.GetPhotonView().ViewID);

            if (GetAudioSettings.AudioSource != null)
            {
                if (GetAudioSettings.DestroyAudioClips.Length != 0)
                {
                    GetAudioSettings.AudioSource.PlayOneShot(GetAudioSettings.DestroyAudioClips[Random.Range(0,
                        GetAudioSettings.DestroyAudioClips.Length)]);
                }
            }

            return true;
        }
    }
}
#endif