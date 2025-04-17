using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SwipeLook : MonoBehaviour
{
    [Header("Цель и смещение")]
    public Transform target;            // Точка, вокруг которой крутим камеру
    public Vector3 offset = new Vector3(0, 3, -6); // Смещение камеры от target

    [Header("Настройки свайпа")]
    public float sensitivity = 0.2f;    // Чувствительность по X
    public float minYaw = -60f;         // Лимит влево
    public float maxYaw = 60f;          // Лимит вправо

    private float yaw;
    private bool isDragging;
    private Vector2 lastPos;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("SimpleSwipeLook: не задан target!");
            enabled = false;
            return;
        }
        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        HandleSwipe();
        ApplyRotation();
    }

    void HandleSwipe()
    {
        if (Input.touchCount == 1)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                isDragging = true;
                lastPos = t.position;
            }
            else if (t.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 delta = t.position - lastPos;
                lastPos = t.position;

                yaw += delta.x * sensitivity;
                yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }

#if UNITY_EDITOR
        // для отладки мышью
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastPos;
            lastPos = Input.mousePosition;

            yaw += delta.x * sensitivity;
            yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
        }
#endif
    }

    void ApplyRotation()
    {
        Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
        Vector3 camPos = target.position + rot * offset;
        transform.position = camPos;

        Vector3 lookDir = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
    }
}
