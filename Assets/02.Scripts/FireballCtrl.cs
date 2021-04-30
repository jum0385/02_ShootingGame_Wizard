using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballCtrl : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.AddRelativeForce(Vector3.forward * 1000.0f);
    }
}
