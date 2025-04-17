using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickFingerTracker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// Тот самый палец, что ведёт джойстик. -1, если никого.
    /// </summary>
    public static int ActiveFingerId = -1;

    public void OnPointerDown(PointerEventData eventData)
    {
        ActiveFingerId = eventData.pointerId;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (ActiveFingerId == eventData.pointerId)
            ActiveFingerId = -1;
    }
}