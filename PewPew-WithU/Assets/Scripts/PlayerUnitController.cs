using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class PlayerUnitController : NetworkBehaviour {

    [SerializeField]
    private float mSpeed = 5f;
    [SerializeField]
    private float mLookSensitivity = 3;
    [SerializeField]
    private Camera mCam;
    private Rigidbody mRB;
    private Vector3 mVelocity = Vector3.zero;
    private Vector3 mRotation = Vector3.zero;
    private Vector3 mCameraPitch = Vector3.zero;

    private void Start()
    {
        mRB = GetComponent<Rigidbody>();
        SetCursorState();
    }

    private void Update()
    {
        if (!hasAuthority)
        {
            return;
        }

        float lMoveX = Input.GetAxisRaw("Horizontal");
        float lMoveZ = Input.GetAxisRaw("Vertical");

        Vector3 lMoveHorizontal = transform.right * lMoveX;
        Vector3 lMoveVertical = transform.forward * lMoveZ;

        mVelocity = (lMoveHorizontal + lMoveVertical).normalized * mSpeed;

        // We'll rotate the player on Y-axis, but not the X-axis.
        // Rotating on X-axis would adversely affect movement, so we just pitch the camera for looking up and down.
        float lRotateY = Input.GetAxisRaw("Mouse X");
        mRotation = new Vector3(0f, lRotateY, 0f) * mLookSensitivity;

        float lRotateX = Input.GetAxisRaw("Mouse Y");
        mCameraPitch = new Vector3(-lRotateX, 0f, 0f) * mLookSensitivity;

    }

    private void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
    }

    void PerformMovement()
    {
        if (mVelocity != Vector3.zero)
        {
            mRB.MovePosition(mRB.position + mVelocity * Time.fixedDeltaTime);
        }
    }

    void PerformRotation()
    {
        mRB.MoveRotation(mRB.rotation * Quaternion.Euler(mRotation));
        if (mCam != null)
        {
            mCam.transform.Rotate(mCameraPitch);
        }
    }

    void SetCursorState()
    {
        Cursor.lockState = CursorLockMode.Locked;
        // Hide cursor when locking
        Cursor.visible = (Cursor.lockState != CursorLockMode.Locked);
    }
}
