#if EBS_PUNV2

using UnityEngine;

using Photon.Pun;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Manager.Saver;

namespace EasyBuildSystem.Packages.Integrations.PUNV2
{
    public class PUNBuildingManager : MonoBehaviourPun
    {
        void Awake()
        {
            if (BuildingSaver.Instance != null)
            {
                BuildingSaver.Instance.LoadBuildingAtStart = false;
                BuildingSaver.Instance.SaveBuildingAtExit = false;

                BuildingSaver.Instance.OnEndingLoadingEvent.AddListener((BuildingPart[] buildingParts, long time) =>
                {
                    if (buildingParts == null)
                    {
                        return;
                    }

                    if (PhotonNetwork.IsMasterClient)
                    {
                        foreach (BuildingPart buildingPart in buildingParts)
                        {
                            GameObject instanced = PhotonNetwork.Instantiate("Building Parts/" + buildingPart.name.Replace("(Clone)", ""), buildingPart.transform.position, buildingPart.transform.rotation);
                            instanced.GetComponent<BuildingPart>().State = BuildingPart.StateType.PLACED;
                            Destroy(buildingPart.gameObject);
                        }
                    }
                });
            }
        }

        void OnApplicationQuit()
        {
            if (BuildingSaver.Instance != null)
            {
                BuildingSaver.Instance.ForceSave();
            }
        }

        bool m_Loaded;

        void FixedUpdate()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (!m_Loaded)
                {
                    if (BuildingSaver.Instance != null)
                    {
                        BuildingSaver.Instance.ForceLoad();
                    }

                    m_Loaded = true;
                }
            }
        }
    }
}
#endif