using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Classroom
{
    public class PhotonViewDestroy : MonoBehaviour
    {
        public bool transferOwnership = false;

        void OnDisable()
        {
            PhotonView[] views = gameObject.GetComponentsInChildren<PhotonView>(true);
            if (views == null || views.Length <= 0)
            {
                Debug.LogError("Failed to 'network-remove' GameObject because has no PhotonView components: " + gameObject);
            }

            if (PhotonNetwork.InRoom && transferOwnership)
            {
                if (views[0].OwnerActorNr != PhotonNetwork.LocalPlayer.ActorNumber)
                    views[0].TransferOwnership(PhotonNetwork.LocalPlayer);
            }
        }
    }
}
