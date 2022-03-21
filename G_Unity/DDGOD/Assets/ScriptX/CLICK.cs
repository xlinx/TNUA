using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLICK : MonoBehaviour
{
    Rigidbody rb;
    public float acc = 1;
    public float rotateS = 111;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce(transform.forward * acc);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0, -rotateS * Time.deltaTime, 0, Space.Self);
        }

         
    }
}
