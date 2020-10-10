using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A test script to control the camera to rotate around world origin with key input.
///     `speed`: rotate speed
/// </summary>
public class CameraController : MonoBehaviour
{
    
    public float speed;
    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        print(string.Format("Speed initialized: {0: 0.0}", speed));
    }

    // Update is called once per frame
    void Update()
    {
        //get the Input from Horizontal axis
        float horizontalInput = Input.GetAxis("Horizontal");
        //get the Input from Vertical axis
        float verticalInput = Input.GetAxis("Vertical");

        transform.position = transform.position + new Vector3(horizontalInput * speed, verticalInput * speed, 0);
        transform.LookAt(target);
    }
}
