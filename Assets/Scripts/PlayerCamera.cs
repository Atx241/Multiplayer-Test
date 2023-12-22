using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float cameraSensetivity = 10;
    public float yAngle;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        if (!GetComponentInParent<NetworkObject>().IsOwner) return;
        yAngle -= Input.GetAxis("Mouse Y") * cameraSensetivity * Time.deltaTime;
        yAngle = Mathf.Clamp(yAngle,-80,80);
        transform.localEulerAngles = new Vector3(yAngle,0,0);
    }
}
