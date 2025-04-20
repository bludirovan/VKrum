using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SawSpinner : MonoBehaviour
{
    [Header("Скорость вращения (градусы в секунду)")]
    public float spinSpeed = 360f; // полное вращение за 1 секунду

    [Header("Ось вращения")]
    public Vector3 spinAxis = Vector3.forward; // вокруг Z‑оси

    void Update()
    {
        // вращаем объект на угол = скорость * время кадра
        transform.Rotate(spinAxis.normalized * spinSpeed * Time.deltaTime, Space.Self);
    }
}

