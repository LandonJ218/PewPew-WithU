using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class PlayerUnit : NetworkBehaviour {

    bool mCursorIsLocked = true;
    Vector3 mVelocity = Vector3.zero;
    Vector3 mRotation = Vector3.zero;
    Vector3 mCameraPitch = Vector3.zero;
    Vector3 mPredictedPosition; // If we are authority, this will be equal to transform.position
    float mSpeed = 5;
    float mLookSensitivity = 3;
    float mOurLatency = 0.0f;   // This should be kept updated, and might need some info from the PlayerConnection
    float mLatencySmoothing = 10; // The higher this value, the faster our local position will reach the mPredictedPosition

    [SerializeField]
    Rigidbody mRB;
    [SerializeField]
    GameObject mPlayerCam;

    void Start () {

        if (!hasAuthority)
        {
            mPlayerCam.SetActive(false);
            return;
        }
        mPlayerCam.SetActive(true);
        SetCursorLock();
        Camera lSceneCamera;
        lSceneCamera = Camera.main;
        if (lSceneCamera != null)
        {
            lSceneCamera.gameObject.SetActive(false);
        }

    }

    // I don't like that I have two types of Start() functions that do the same thing, 
    // but this one (OnStartAuthority()) is apparently needed to have host behave properly.
    public override void OnStartAuthority()  
    {
        Start();
    }

    void Update () {

        // Code above this check run for EVERY client
        if (!hasAuthority)
        {
            return;
        }

        float lMoveX = Input.GetAxisRaw("Horizontal");
        float lMoveZ = Input.GetAxisRaw("Vertical");
        float lMoveY = Input.GetAxisRaw("Jump");
        float lRotateY = 0;
        float lRotateX = 0;
        if (mCursorIsLocked)
        {
            lRotateY = Input.GetAxisRaw("Mouse X");
            lRotateX = Input.GetAxisRaw("Mouse Y");
        }

        Vector3 lMoveSagittal = transform.right * lMoveX;
        Vector3 lMoveFrontal = transform.forward * lMoveZ;
        Vector3 lMoveTransverse = transform.up * lMoveY;
        if (transform.position.y <= 1 && lMoveY < 0)
        {
            lMoveTransverse = Vector3.zero;
        }
        
        mVelocity = (lMoveSagittal + lMoveFrontal + lMoveTransverse).normalized * mSpeed;

        // We'll rotate the player on Y-axis, but not the X-axis. Rotating on X-axis would adversely affect movement
        mRotation = new Vector3(0f, lRotateY, 0f) * mLookSensitivity;
        mCameraPitch = new Vector3(-lRotateX, 0f, 0f) * mLookSensitivity;

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            mCursorIsLocked = !mCursorIsLocked;
            SetCursorLock();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        // Code above this check run for EVERY client
        if (mVelocity != Vector3.zero)
        {
            if (!hasAuthority)
            {
                mPredictedPosition = mPredictedPosition + (mVelocity * Time.deltaTime);
                mRB.MovePosition(mPredictedPosition);
            }
            else
            {
                mRB.MovePosition(mRB.position + mVelocity * Time.fixedDeltaTime);
            }
        }
        mRB.MoveRotation(mRB.rotation * Quaternion.Euler(mRotation));
        if (mPlayerCam != null)
        {
            mPlayerCam.transform.Rotate(mCameraPitch);
        }

        if (hasAuthority)
        {
            CmdUpdateUnit(transform.position, mVelocity, mRotation, mCameraPitch);
        }
    }

    void SetCursorLock()
    {
        if (mCursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    [Command]
    void CmdUpdateUnit(Vector3 aPosition, Vector3 aVelocity, Vector3 aRotation, Vector3 aCameraPitch)
    {
        transform.position = aPosition;
        mVelocity = aVelocity;
        mRotation = aRotation;
        mCameraPitch = aCameraPitch;

        // If we know latency of the clients involved, we can compensate with some prediction...
        //transform.position = aPosition + (mVelocity * thisPlayersLatencyToServer);

        RpcUpdateUnit(aPosition, aVelocity, aRotation, aCameraPitch);
    }

    [ClientRpc]
    void RpcUpdateUnit(Vector3 aPosition, Vector3 aVelocity, Vector3 aRotation, Vector3 aCameraPitch)
    {
        // If this is an object I control, my client's info about the object is likely the most "accurate"
        if (hasAuthority)
        {
            return;
        }

        // If we know latency of the clients involved, we can compensate with some prediction...
        mVelocity = aVelocity;
        mPredictedPosition = aPosition + (mVelocity * mOurLatency);
        mRotation = aRotation;
        mCameraPitch = aCameraPitch;
    }

}
