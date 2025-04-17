using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitingTrap : MonoBehaviour
{
    [Header("Настройки гравитационного поля")]
    [Tooltip("Ось гравитационного поля, вокруг которой объект будет вращаться (например, Vector3.down для вертикальной оси)")]
    public Vector3 gravityAxis = Vector3.down;

    [Header("Настройки вращения")]
    [Tooltip("Скорость вращения в градусах в секунду")]
    public float rotationSpeed = 90f;

    [Header("Настройки столкновения")]
    [Tooltip("Сила, с которой ловушка отталкивает игрока при столкновении")]
    public float pushForce = 10f;

    void Update()
    {
        // Нормализуем ось, на случай, если её длина отличается от 1
        Vector3 axis = gravityAxis.normalized;
        // Вращаем объект вокруг заданной оси в мировом пространстве
        transform.Rotate(axis, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody rb = collision.rigidbody;
            if (rb != null)
            {
                Vector3 pushDirection = (collision.transform.position - transform.position).normalized;
                rb.velocity = pushDirection * pushForce;
            }
        }
    }

}
