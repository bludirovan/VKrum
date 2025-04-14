using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("Цель, за которой следует камера (персонаж)")]
    public Transform target;

    [Tooltip("Смещение камеры в локальных координатах персонажа (например, (0, 5, -10))")]
    public Vector3 localOffset = new Vector3(0, 5, -10);

    [Tooltip("Скорость сглаживания движения камеры")]
    public float smoothSpeed = 0.125f;

    [Tooltip("Слой препятствий для корректировки позиции камеры")]
    public LayerMask obstacleMask;

    // Внутреннее хранение текущего угла смещения для камеры, обновляется кратно 90 градусов
    private Quaternion targetOffsetRotation;
    private Quaternion lastPlayerRotation;

    private void Start()
    {
        // Изначально смещение совпадает с исходным (без поворотов)
        targetOffsetRotation = Quaternion.identity;
        lastPlayerRotation = target.rotation;
    }

    private void LateUpdate()
    {
        // Проверяем изменение ориентации игрока.
        // Если произошло значительное изменение (близкое к 90° повороту) по оси Y – обновляем смещение.
        float angleChange = Quaternion.Angle(lastPlayerRotation, target.rotation);
        if (angleChange > 45f)  // порог можно настроить
        {
            // Вычисляем новый угол вокруг оси Y – округляя до ближайшего кратного 90.
            float newYaw = Mathf.Round(target.eulerAngles.y / 90f) * 90f;
            targetOffsetRotation = Quaternion.Euler(0, newYaw, 0);
            lastPlayerRotation = target.rotation;
        }

        // Вычисляем желаемую позицию камеры:
        // Используем обновлённое смещение, преобразуем его в мировые координаты относительно игрока.
        Vector3 desiredPosition = target.position + (targetOffsetRotation * localOffset);

        // Проверка наличия препятствий между игроком и желаемой позицией камеры
        RaycastHit hit;
        if (Physics.Linecast(target.position, desiredPosition, out hit, obstacleMask))
        {
            desiredPosition = hit.point;
        }

        // Плавно перемещаем камеру к рассчитанной позиции
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Камера всегда смотрит на игрока
        transform.LookAt(target);
    }

    /*
    [Tooltip("Объект, за которым следует камера (например, игрок)")]
    public Transform target;

    [Tooltip("Смещение камеры относительно позиции цели")]
    public Vector3 offset = new Vector3(0, 5, -10);

    [Tooltip("Скорость сглаживания перемещения камеры")]
    public float smoothSpeed = 0.125f;

    [Tooltip("Скорость поворота камеры за игроком")]
    public float rotationSmoothSpeed = 5f;

    private void LateUpdate()
    {
        // Получаем направление игрока (только по Y)
        Quaternion targetRotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);

        // Поворачиваем offset с учётом поворота игрока
        Vector3 rotatedOffset = targetRotation * offset;

        // Целевая позиция камеры
        Vector3 desiredPosition = target.position + rotatedOffset;

        // Плавное перемещение
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Плавный поворот камеры (если нужно сделать поворот более мягким)
        Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSmoothSpeed);
    }
    */
}
