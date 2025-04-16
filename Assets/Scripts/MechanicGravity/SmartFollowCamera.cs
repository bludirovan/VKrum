using UnityEngine;
using UnityEngine.EventSystems;

public class SmartFollowCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public Vector3 localOffset = new Vector3(0, 3, -6);
    public float smoothSpeed = 10f;
    public float collisionBuffer = 0.2f;
    public LayerMask obstacleLayers;

    [Header("Swipe‑Look Settings")]
    public float yawSpeed = 0.2f;
    public float pitchSpeed = 0.2f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    private bool isDragging;
    private Vector2 lastTouchPos;
    private float yaw, pitch;
    private Vector3 currentVelocity;

    void Start()
    {
        if (target == null) { enabled = false; return; }
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;
    }

    void Update()
    {
        HandleSwipeInput();
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (isDragging)
        {
            // ORBIT MODE
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 baseOff = Vector3.up * localOffset.y;
            Vector3 horOff = rot * new Vector3(localOffset.x, 0f, localOffset.z);
            Vector3 desiredPos = target.position + baseOff + horOff;

            if (Physics.Linecast(target.position + baseOff, desiredPos, out RaycastHit hit, obstacleLayers))
                desiredPos = hit.point + hit.normal * collisionBuffer;

            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref currentVelocity, 1f / smoothSpeed);
            Quaternion lookRot = Quaternion.LookRotation((target.position + baseOff) - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * smoothSpeed);
        }
        else
        {
            // FOLLOW MODE (твоя старая логика)
            Vector3 offset = target.right * localOffset.x + target.up * localOffset.y + target.forward * localOffset.z;
            Vector3 desiredPos = target.position + offset;
            if (Physics.Linecast(target.position, desiredPos, out RaycastHit hit, obstacleLayers))
                desiredPos = hit.point + hit.normal * collisionBuffer;

            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref currentVelocity, 1f / smoothSpeed);
            Vector3 lookDir = target.position - transform.position;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion tRot = Quaternion.LookRotation(lookDir, target.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, tRot, Time.deltaTime * smoothSpeed);
            }
        }
    }

    private void HandleSwipeInput()
    {
        // Мобильный
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(t.fingerId)) return;

            if (t.phase == TouchPhase.Began)
            {
                isDragging = true;
                lastTouchPos = t.position;
            }
            else if (t.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 delta = t.position - lastTouchPos;
                lastTouchPos = t.position;
                yaw += delta.x * yawSpeed;
                pitch -= delta.y * pitchSpeed;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }

        // Редактор (мышь)
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            isDragging = true;
            lastTouchPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastTouchPos;
            lastTouchPos = Input.mousePosition;
            yaw += delta.x * yawSpeed;
            pitch -= delta.y * pitchSpeed;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
#endif
    }
}
