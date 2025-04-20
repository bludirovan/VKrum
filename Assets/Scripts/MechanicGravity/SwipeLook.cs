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


/*
 * 
 * 


// CameraSwipeController.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class CameraSwipeController : MonoBehaviour
{
    [Header("Target and Orbit")]
    public Transform target;
    public float distance = 5f;
    public Vector2 pitchLimits = new Vector2(-30f, 60f);

    [Header("Swipe Settings")]
    public float rotationSpeed = 0.2f;

    private float yaw;
    private float pitch;
    private Vector2 lastPointerPos;
    private bool isDragging = false;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        // initialize position
        UpdatePosition();
    }

    void Update()
    {
        // Handle touch or mouse      
        #if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI(Input.mousePosition))
        {
            lastPointerPos = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 curr = Input.mousePosition;
            Vector2 delta = curr - lastPointerPos;
            lastPointerPos = curr;
            RotateCamera(delta);
        }
        else if (Input.GetMouseButtonUp(0))
            isDragging = false;
        #else
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began && !IsPointerOverUI(touch.position))
            {
                lastPointerPos = touch.position;
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touch.position - lastPointerPos;
                lastPointerPos = touch.position;
                RotateCamera(delta);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                isDragging = false;
        }
        #endif
    }

    private void RotateCamera(Vector2 delta)
    {
        yaw += delta.x * rotationSpeed;
        pitch -= delta.y * rotationSpeed;
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pos = target.position - rot * Vector3.forward * distance;
        transform.rotation = rot;
        transform.position = pos;
    }

    private bool IsPointerOverUI(Vector2 screenPos)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        { position = screenPos };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}*/