using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityAffected : MonoBehaviour
{
    public float globalGravity = -9.81f;
    public float mass = 1f;
    public float gravityRange = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 gravityForce = Vector3.down * globalGravity * mass; // глобальная
        GravityObject[] gravitySources = FindObjectsOfType<GravityObject>();

        foreach (var source in gravitySources)
        {
            Vector3 dir = (source.transform.position - transform.position);
            float distance = dir.magnitude;
            if (distance > 0 && distance < gravityRange)
            {
                // Гравитационная сила: F = G * (m1 * m2) / r^2
                float forceMagnitude = (source.mass * mass) / (distance * distance);
                gravityForce += dir.normalized * forceMagnitude;
            }
        }

        rb.AddForce(gravityForce);
    }
}
