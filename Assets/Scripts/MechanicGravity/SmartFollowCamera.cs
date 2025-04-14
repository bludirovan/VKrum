using UnityEngine;

public class SmartFollowCamera : MonoBehaviour
{
    [Header("Цель")]
    public Transform target;

    [Header("Настройки смещения камеры")]
    public Vector3 offset = new Vector3(0f, 10f, -10f); // Наклон сверху-сзади
    public float followSmoothness = 5f;                 // Насколько плавно камера следует

    [Header("Ограничения по высоте")]
    public float minY = 5f;
    public float maxY = 15f;

    [Header("Настройки поворота")]
    public float lookSmoothness = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        // --- 1. Целевая позиция камеры ---
        Vector3 desiredPosition = target.position + offset;

        // Немного варьируем Y, если нужно (например, можешь сюда вставить динамику от скорости или местности)
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

        // --- 2. Плавное движение ---
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothness * Time.deltaTime);

        // --- 3. Плавный поворот к игроку ---
        Vector3 lookDir = target.position - transform.position;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(lookDir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, lookSmoothness * Time.deltaTime);
        }
    }
}


