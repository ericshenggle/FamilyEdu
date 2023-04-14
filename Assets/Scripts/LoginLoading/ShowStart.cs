using System.Collections;
using System.Collections.Generic;
using GameCreator.Variables;
using UnityEngine;

public class ShowStart : MonoBehaviour
{
    [SerializeField] public LocalVariables loginStatus;

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
        bool m_login = loginStatus.Get("isAlreadyLogin").Get<bool>();
        bool m_register = loginStatus.Get("isAlreadyRegister").Get<bool>();
        if(m_login && !childStart.activeSelf) {
            childStart.SetActive(true);
            childLogin.SetActive(false);
            childRegister.SetActive(false);
        } else if(m_register && !childLogin.activeSelf) {
            childStart.SetActive(false);
            childLogin.SetActive(true);
            childRegister.SetActive(false);
        } else if(!m_register && !childRegister.activeSelf) {
            childStart.SetActive(false);
            childLogin.SetActive(false);
            childRegister.SetActive(true);
        }
    }
}
