using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityAreaPlane : GravityArea
{
    [Header("Параметры плоскости")]
    public float attractionRadius = 5f;   // Радиус действия притяжения от центра
    public float repulsionRadius = 10f;   // Радиус, после которого начинается отталкивание

    [Header("Цвета визуализации")]
    public Color attractionColor = Color.cyan;
    public Color repulsionColor = Color.red;

    public override Vector3 GetGravityDirection(GravityBody gravityBody, bool isPositive)
    {
        Vector3 objectPos = gravityBody.transform.position;
        Vector3 planeCenter = transform.position;
        Vector3 planeNormal = transform.up;

        // Вектор от центра к объекту в плоскости
        Vector3 toObject = objectPos - planeCenter;
        Vector3 inPlaneOffset = Vector3.ProjectOnPlane(toObject, planeNormal);
        float distanceFromCenter = inPlaneOffset.magnitude;

        // Притягиваем или отталкиваем по нормали в зависимости от расстояния
        if (distanceFromCenter <= attractionRadius)
        {
            return isPositive ? -planeNormal : planeNormal; // Притяжение вниз к плоскости
        }
        else if (distanceFromCenter >= repulsionRadius)
        {
            return isPositive ? planeNormal : -planeNormal; // Отталкивание от плоскости
        }
        else
        {
            // Переходная зона: ослабление силы или отсутствие
            return Vector3.zero;
        }
    }

    public override Vector3 GetGravityDirection(GravityBody gravityBody)
    {
        return GetGravityDirection(gravityBody, LocalPolarity);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Отобразим зону действия
        Gizmos.color = attractionColor;
        DrawCircle(transform.position, transform.up, attractionRadius);

        Gizmos.color = repulsionColor;
        DrawCircle(transform.position, transform.up, repulsionRadius);

        // Нарисуем направление нормали
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 2f);
    }

    private void DrawCircle(Vector3 center, Vector3 normal, float radius, int segments = 64)
    {
        Vector3 axisA = Vector3.Cross(normal, Vector3.up);
        if (axisA.magnitude < 0.1f)
            axisA = Vector3.Cross(normal, Vector3.right);

        axisA.Normalize();
        Vector3 axisB = Vector3.Cross(normal, axisA);

        Vector3 prevPoint = center + radius * axisA;
        for (int i = 1; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2;
            Vector3 nextPoint = center + radius * (Mathf.Cos(angle) * axisA + Mathf.Sin(angle) * axisB);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
#endif
}
