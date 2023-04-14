#if EBS_PUNV2

using Photon.Pun;

using UnityEngine;

namespace EasyBuildSystem.Packages.Integrations.PUNV2
{
    public class Demo_NetworkPlayer : MonoBehaviourPun
    {
        public Camera Camera;

        public MonoBehaviour[] OwnerComponents;

        void Awake()
        {
            Camera.gameObject.SetActive(photonView.IsMine);

            for (int i = 0; i < OwnerComponents.Length; i++)
            {
                if (OwnerComponents[i] != null)
                {
                    OwnerComponents[i].enabled = photonView.IsMine;
                }
            }
        }
    }
}
#endif