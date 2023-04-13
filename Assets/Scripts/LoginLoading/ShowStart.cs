using System.Collections;
using System.Collections.Generic;
using GameCreator.Variables;
using UnityEngine;

public class ShowStart : MonoBehaviour
{
    [SerializeField] public Transform isAlreadyLogin;

    private GameObject childStart;

    private GameObject childLogin;

    private GameObject childRegister;

    // Start is called before the first frame update
    void Start()
    {
        childStart = transform.Find("StartCanvas").gameObject;
        childLogin = transform.Find("LoginCanvas").gameObject;
        childRegister = transform.Find("RegisterCanvas").gameObject;
        this.InvokeRepeating("updateLogin", 5, 2);
    }

    // Update is called once per frame
    private void updateLogin()
    {
        bool m_login = (bool)VariablesManager.GetLocal(isAlreadyLogin.gameObject, "isAlreadyLogin");
        if(m_login && !childStart.activeSelf) {
            childStart.SetActive(true);
            childLogin.SetActive(false);
            childRegister.SetActive(false);
        }
    }
}
