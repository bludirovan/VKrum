using UnityEngine;

public class SmartFollowCamera : MonoBehaviour
{
    public Transform target;                   // Цель (игрок)
    public Vector3 localOffset = new Vector3(0, 3, -6); // Смещение камеры
    public float smoothSpeed = 10f;            // Скорость сглаживания
    public float collisionBuffer = 0.2f;       // Отступ от препятствия
    public LayerMask obstacleLayers;           // Слои, которые считаются препятствиями

    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (target == null) return;

        // Вычисление смещённой позиции относительно локальной оси игрока
        Vector3 offset =
            target.right * localOffset.x +
            target.up * localOffset.y +
            target.forward * localOffset.z;

        Vector3 desiredPosition = target.position + offset;

        // Проверка на столкновение между игроком и камерой
        if (Physics.Linecast(target.position, desiredPosition, out RaycastHit hit, obstacleLayers))
        {
            // Камера помещается ближе к точке удара, отступая от препятствия
            desiredPosition = hit.point + hit.normal * collisionBuffer;
        }

        // Плавное перемещение камеры
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);

        // Камера всегда смотрит на игрока, с учётом локального "вверх"
        Vector3 lookDir = target.position - transform.position;
        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir, target.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);
        }
    }
}
