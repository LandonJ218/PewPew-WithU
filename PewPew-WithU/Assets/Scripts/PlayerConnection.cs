using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerConnection : NetworkBehaviour {

    [SerializeField]
    GameObject mPlayerUnitPrefab;
    [SerializeField][SyncVar]
    string mPlayerName = "Anonymous";

	void Start () {

        if (!isLocalPlayer)
        {
            return;
        }

        CmdSpawnUnit();
	}
	
	void Update () {

        if (!isLocalPlayer)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CmdSpawnUnit();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            string lName = "Sammy" + Random.Range(1, 9);
            Debug.Log("Requesting playerName change...");
            CmdChangePlayerName(lName);
        }

    }

    // Example funtion for using a hook in a [SyncVar] (ie. [SyncVar(hook = "OnPlayerNameChanged")] above the variable mPlayerName)
    //void OnPlayerNameChanged(string aName)
    //{
    //    Debug.Log("PlayerName changed from " + mPlayerName + " to " + aName + ".");

    //    // **** NOTE: When using a hook on a SyncVar, the local value does NOT automatically get updated. ****
    //    mPlayerName = aName;
    //    gameObject.name = "PlayerConnection:" + aName;

    //}

    [Command]
    void CmdSpawnUnit()
    {
        GameObject lUnit = Instantiate(mPlayerUnitPrefab);
        NetworkServer.SpawnWithClientAuthority(lUnit, connectionToClient);
    }

    [Command]
    void CmdChangePlayerName(string aName)
    {
        mPlayerName = aName;
        Debug.Log("PlayerName is now " + mPlayerName);

        RpcChangePlayerName(mPlayerName);
    }

    [ClientRpc]
    void RpcChangePlayerName(string aName)
    {
        gameObject.name = "PlayerConnection: " + aName;
        Debug.Log("PlayerName changed from " + mPlayerName + " to " + aName + ".");
        mPlayerName = aName;
    }

}
