using UnityEngine;

public class SmartFollowCamera : MonoBehaviour
{
    [Header("Настройки следования за игроком")]
    public Transform target;                           // Игрок или другая цель
    public Vector3 baseOffset = new Vector3(0, 5, -10);  // Базовое смещение камеры относительно игрока (локальные координаты)
    public float smoothSpeed = 5f;                       // Скорость сглаживания перемещения камеры (чем выше значение, тем быстрее)

    [Header("Настройки поворота камеры")]
    public float rotationSpeed = 5f;                     // Скорость сглаживания поворота камеры

    [Header("Настройки избежания препятствий")]
    public float minDistance = 2.0f;                     // Минимальное расстояние от камеры до игрока
    public LayerMask obstacleLayers;                     // Слои, в которых находятся препятствия
    public float collisionBuffer = 0.5f;                 // Запас, позволяющий избежать прилипания камеры к препятствию
    public float occlusionCheckRadius = 0.5f;            // Радиус для SphereCast (для лучшего обнаружения препятствий)

    [Header("Настройки поля видимости")]
    // Определяем "безопасную зону" в viewport-координатах (0–1). Если игрок выходит за неё, производится поправка.
    public Vector2 safeZoneMin = new Vector2(0.3f, 0.3f);
    public Vector2 safeZoneMax = new Vector2(0.7f, 0.7f);
    public float viewportAdjustmentSpeed = 2f;         // Скорость поправки камеры при выходе игрока за безопасную зону

    [Header("Ограничение по высоте")]
    public float minCameraHeight = 1.0f;                 // Минимальная допустимая высота камеры (например, чуть выше пола)

    private Camera cam;

    // Для SmoothDamp перемещения
    private Vector3 currentVelocity = Vector3.zero;

    void Start()
    {
        cam = Camera.main;
        if (target == null)
            Debug.LogError("Target не задан для SmartFollowCamera!");
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // --- 1. Вычисляем базовую позицию камеры ---
        // Поворачиваем базовое смещение на угол цели (игрока)
        Vector3 desiredOffset = target.rotation * baseOffset;
        Vector3 desiredPosition = target.position + desiredOffset;

        // --- 2. Проверка препятствий с помощью SphereCast ---
        Vector3 direction = desiredPosition - target.position;
        float distance = direction.magnitude;
        RaycastHit hit;
        if (Physics.SphereCast(target.position, occlusionCheckRadius, direction.normalized, out hit, distance, obstacleLayers))
        {
            float adjustedDistance = Mathf.Max(hit.distance - collisionBuffer, minDistance);
            desiredPosition = target.position + direction.normalized * adjustedDistance;
        }

        // --- 3. Плавное перемещение камеры ---
        // Используем SmoothDamp для более естественного сглаживания
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);

        // Ограничение по высоте: гарантируем, что камера не опускается ниже минимального уровня
        if (smoothedPosition.y < minCameraHeight)
            smoothedPosition.y = minCameraHeight;

        transform.position = smoothedPosition;

        // --- 4. Корректировка для обеспечения видимости цели ---
        Vector3 viewportPos = cam.WorldToViewportPoint(target.position);
        Vector2 adjustment = Vector2.zero;
        bool adjustmentNeeded = false;

        // Корректировка по оси X
        if (viewportPos.x < safeZoneMin.x)
        {
            adjustmentNeeded = true;
            adjustment.x = safeZoneMin.x - viewportPos.x;
        }
        else if (viewportPos.x > safeZoneMax.x)
        {
            adjustmentNeeded = true;
            adjustment.x = safeZoneMax.x - viewportPos.x;
        }

        // Корректировка по оси Y
        if (viewportPos.y < safeZoneMin.y)
        {
            adjustmentNeeded = true;
            adjustment.y = safeZoneMin.y - viewportPos.y;
        }
        else if (viewportPos.y > safeZoneMax.y)
        {
            adjustmentNeeded = true;
            adjustment.y = safeZoneMax.y - viewportPos.y;
        }

        if (adjustmentNeeded)
        {
            // Преобразуем поправку из viewport в мировое пространство
            Vector3 right = transform.right;
            Vector3 up = transform.up;
            Vector3 worldAdjustment = (right * adjustment.x + up * adjustment.y) * viewportAdjustmentSpeed * Time.deltaTime;
            transform.position += worldAdjustment;

            // Повторно проверяем высоту после поправки
            if (transform.position.y < minCameraHeight)
            {
                Vector3 pos = transform.position;
                pos.y = minCameraHeight;
                transform.position = pos;
            }
        }

        // --- 5. Плавный поворот камеры ---
        // Защита от ситуации, когда направление равно нулевому вектору (например, позиции совпадают)
        Vector3 lookDirection = target.position - transform.position;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
