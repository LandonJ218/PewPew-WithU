using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerUnitSetup : NetworkBehaviour
{

    [SerializeField]
    Behaviour[] mComponentsToDisable;

    Camera mSceneCamera;

    private void Start()
    {
        if (!hasAuthority)
        {
            for (int i = 0; i < mComponentsToDisable.Length; i++)
            {
                mComponentsToDisable[i].enabled = false;
            }
        }
        else
        {
            mSceneCamera = Camera.main;
            if (mSceneCamera != null)
            {
                mSceneCamera.gameObject.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        if (mSceneCamera != null)
        {
            mSceneCamera.gameObject.SetActive(true);
        }
    }

}
