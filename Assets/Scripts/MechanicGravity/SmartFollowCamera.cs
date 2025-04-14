using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartFollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 localOffset = new Vector3(0, 3, -6);
    public float smoothSpeed = 0.15f;
    public float collisionBuffer = 0.2f;
    public LayerMask obstacleLayers;

    void LateUpdate()
    {
        if (target == null) return;

        // Расчёт смещения с учётом локальных осей игрока
        Vector3 right = target.right;
        Vector3 up = target.up;
        Vector3 forward = target.forward;

        // Локальный offset относительно ориентации игрока
        Vector3 desiredOffset =
            right * localOffset.x +
            up * localOffset.y +
            forward * localOffset.z;

        Vector3 desiredPosition = target.position + desiredOffset;

        // Проверка столкновений
        RaycastHit hit;
        if (Physics.Linecast(target.position, desiredPosition, out hit, obstacleLayers))
        {
            desiredPosition = hit.point + hit.normal * collisionBuffer;
        }

        // Плавное движение камеры
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Камера смотрит на игрока, но "вверх" тоже подстраивается под локальный up
        transform.rotation = Quaternion.LookRotation(target.position - transform.position, target.up);
    }
}
