using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnection : NetworkBehaviour {

    [SerializeField]
    private GameObject mPlayerUnit;

	void Start () {

        if (!isLocalPlayer)
        {
            return;
        }

        CmdSpawnMyUnit();

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    [Command]
    void CmdSpawnMyUnit()
    {
        GameObject lUnit = Instantiate(mPlayerUnit);
        NetworkServer.SpawnWithClientAuthority(lUnit, connectionToClient);

    }

}
