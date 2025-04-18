using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SmartFollowCamera : MonoBehaviour
{
    [Header("Target and Offsets")]
    public Transform target;             // Ссылка на игрока
    public Vector3 offset = new Vector3(0, 2, -5);  // Локальный оффсет относительно игрока (x, y, z)

    [Header("Smoothness")]
    [Range(0.01f, 1f)]
    public float positionSmoothTime = 0.15f;
    [Range(0.01f, 1f)]
    public float rotationSmoothTime = 0.1f;

    private Vector3 currentVelocity;     // Для SmoothDamp позиции
    private Vector3 rotationVelocity;    // Для SmoothDamp углов (в градусах)

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Получаем вектор гравитации в точке игрока (можно заменить на свой метод)
        Vector3 gravity = Physics.gravity.normalized;

        // 2. Вычисляем мировую точку, в которой должна стоять камера:
        // сначала поворачиваем локальный offset по ориентации игрока:
        Quaternion targetRotation = Quaternion.LookRotation(target.forward, -gravity);
        Vector3 desiredPosition = target.position + targetRotation * offset;

        // 3. Плавно перемещаем камеру:
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            positionSmoothTime
        );

        // 4. Плавно поворачиваем камеру так, чтобы она смотрела на игрока «правильным верхом»:
        Quaternion desiredRotation = Quaternion.LookRotation(
            target.position - transform.position,
            -gravity
        );

        // Интерполируем через углы Эйлера, чтобы не получить «мимолётные» кривые кватернионы:
        Vector3 smoothedEuler = Vector3.SmoothDamp(
            transform.eulerAngles,
            desiredRotation.eulerAngles,
            ref rotationVelocity,
            rotationSmoothTime
        );
        transform.rotation = Quaternion.Euler(smoothedEuler);

        // 5. (Опционально) Обработка коллизий: 
        //    Raycast от игрока к камере, если попали в стену/объект — укоротить дистанцию.
        //    var dir = transform.position - target.position;
        //    if (Physics.Raycast(target.position, dir.normalized, out RaycastHit hit, dir.magnitude))
        //        transform.position = hit.point + hit.normal * 0.3f;
    }
}
