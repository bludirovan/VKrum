using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityAreaCenter : GravityArea
{
    public float gizmoRadius = 5f;              // Радиус визуализации
    public int gizmoLines = 32;                 // Кол-во линий-гравитации
    public Color gizmoColor = Color.cyan;       // Цвет линий

    public override Vector3 GetGravityDirection(GravityBody gravityBody, bool isPositive)
    {
        Vector3 dir = (transform.position - gravityBody.transform.position).normalized;
        return isPositive ? dir : -dir;
    }

    public override Vector3 GetGravityDirection(GravityBody gravityBody)
    {
        return GetGravityDirection(gravityBody, LocalPolarity);
    }

}
