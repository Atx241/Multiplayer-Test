using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class Player : NetworkBehaviour
{
    private Rigidbody rb;
    public float acceleration;
    public float speed;
    public float jumpForce = 10;
    public GameObject clientObjectsRoot;
    public new GameObject camera;
    NetworkVariable<Color> playerColor = new NetworkVariable<Color>();
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (IsServer)
            playerColor.Value = Color.HSVToRGB(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
    }
    void Update()
    {
        GetComponent<Renderer>().material.color = playerColor.Value;
        if (!IsLocalPlayer)
        {
            camera.GetComponent<Camera>().enabled = false;
            camera.GetComponent<AudioListener>().enabled = false;
            clientObjectsRoot.SetActive(false);
            return;
        }
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * camera.GetComponent<PlayerCamera>().cameraSensetivity);
        rb.AddForce(transform.forward * acceleration * Input.GetAxis("Vertical") + transform.right * acceleration * Input.GetAxis("Horizontal"));
        rb.AddForce(-transform.forward * acceleration * (transform.InverseTransformDirection(rb.velocity).z / speed) + -transform.right * acceleration * (transform.InverseTransformDirection(rb.velocity).x / speed));
        Debug.DrawLine(transform.position, transform.position + transform.up * -1.1f);
        if (Physics.Raycast(transform.position, -transform.up, 1.1f, LayerMask.GetMask("Default")) && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }
}
