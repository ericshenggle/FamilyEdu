#if EBS_PUNV2

using Photon.Pun;

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using EasyBuildSystem.Features.Runtime.Buildings.Part;

namespace EasyBuildSystem.Packages.Integrations.PUNV2
{
    public class PUNBuildingPlacerReflector : MonoBehaviourPun
    {
        [PunRPC]
        public void RPCPlace(string partIdentifier, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            BuildingPart buildingPart = BuildingManager.Instance.GetBuildingPartByIdentifier(partIdentifier);

            if (buildingPart != null)
            {
                GameObject instancedBuildingPart = PhotonNetwork.Instantiate("Building Parts/" + buildingPart.gameObject.name.Replace("(Clone)", ""),
                    position, Quaternion.Euler(rotation));

                instancedBuildingPart.GetComponent<BuildingPart>().State = BuildingPart.StateType.PLACED;
            }
        }

        [PunRPC]
        public void RPCDestroy(int viewId)
        {
            if (PhotonView.Find(viewId) != null)
            {
                PhotonNetwork.Destroy(PhotonView.Find(viewId).gameObject);
            }
        }
    }
}
#endif