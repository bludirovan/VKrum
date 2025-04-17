using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

[ExecuteAlways]
public class MobileCameraSwipe : MonoBehaviour
{
    [Tooltip("UI‑джойстик, которым двигают персонажа")]
    public Joystick moveJoystick;

    // Сохраняем оригинальный делегат, чтобы потом восстанавливать
    private CinemachineCore.AxisInputDelegate _originalGetAxis;

    void OnEnable()
    {
        _originalGetAxis = CinemachineCore.GetInputAxis;
        CinemachineCore.GetInputAxis = GetAxisOverride;
    }

    void OnDisable()
    {
        CinemachineCore.GetInputAxis = _originalGetAxis;
    }

    float GetAxisOverride(string axisName)
    {
        // 1) Блокируем, если джойстик ведут
        if (moveJoystick != null &&
            (Mathf.Abs(moveJoystick.Horizontal) > 0.1f ||
             Mathf.Abs(moveJoystick.Vertical) > 0.1f))
        {
            return 0f;
        }

        // 2) Блокируем, если активен палец джойстика
        int f = JoystickFingerTracker.ActiveFingerId;
        if (f >= 0)
        {
            // Пока этот палец где‑то висит — не даём смотреть
            foreach (var t in Input.touches)
            {
                if (t.fingerId == f)
                    return 0f;
            }
        }

        // 3) Блокируем, если касание/клик над любым UI‑элементом
        // — мышь
        if (Input.GetMouseButton(0) &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return 0f;
        }

        // — тачи
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return 0f;
        }

        // 4) Иначе — отдать настоящий ввод (мышь/тач/клавы)
        return _originalGetAxis(axisName);
    }
}
