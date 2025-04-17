using UnityEngine;

public class SmartFollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 localOffset = new Vector3(0, 3, -6);
    public float smoothSpeed = 10f;
    public float rotationSpeed = 2f;
    public float collisionBuffer = 0.2f;
    public LayerMask obstacleLayers;

    private Vector3 currentVelocity;
    private Vector2 lookAngles; // x = yaw, y = pitch
    private bool isDragging = false;

    void Start()
    {
        Vector3 dir = target.position - transform.position;
        lookAngles.x = Quaternion.LookRotation(dir).eulerAngles.y;
        lookAngles.y = Quaternion.LookRotation(dir).eulerAngles.x;
    }

    void Update()
    {
        // Обработка свайпа (или мыши)
        if (Input.GetMouseButtonDown(0))
            isDragging = true;

        if (Input.GetMouseButtonUp(0))
            isDragging = false;

        if (isDragging)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            lookAngles.x += mouseX * rotationSpeed;
            lookAngles.y -= mouseY * rotationSpeed;
            lookAngles.y = Mathf.Clamp(lookAngles.y, -30f, 60f); // ограничение наклона
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Поворот offset согласно lookAngles
        Quaternion lookRotation = Quaternion.Euler(lookAngles.y, lookAngles.x, 0f);
        Vector3 rotatedOffset = lookRotation * localOffset;
        Vector3 desiredPosition = target.position + rotatedOffset;

        // Проверка столкновений
        if (Physics.Linecast(target.position, desiredPosition, out RaycastHit hit, obstacleLayers))
        {
            desiredPosition = hit.point + hit.normal * collisionBuffer;
        }

        // Плавное движение
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);

        // Смотрим на игрока
        Vector3 lookDir = target.position - transform.position;
        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);
        }
    }
}
