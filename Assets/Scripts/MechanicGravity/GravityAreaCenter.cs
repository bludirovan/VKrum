using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityAreaCenter : GravityArea
{
    public float gizmoRadius = 5f;              // Радиус визуализации (для Gizmos)
    public int gizmoLines = 32;                 // Количество линий (если потребуется дополнительное рисование)

    [Header("Цвета гравитации")]
    public Color positiveColor = new Color(0f, 0.5f, 1f, 1f);  // Синий (притяжение)
    public Color negativeColor = new Color(1f, 0.2f, 0.2f, 1f); // Красный (отталкивание)

    public override Vector3 GetGravityDirection(GravityBody gravityBody, bool isPositive)
    {
        Vector3 dir = (transform.position - gravityBody.transform.position).normalized;
        return isPositive ? dir : -dir;
    }

    public override Vector3 GetGravityDirection(GravityBody gravityBody)
    {
        return GetGravityDirection(gravityBody, LocalPolarity);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Цвет меняется в зависимости от полярности поля
        Gizmos.color = LocalPolarity ? positiveColor : negativeColor;

        Collider col = GetComponent<Collider>();
        if (col is SphereCollider sphere)
        {
            // Учтём смещение центра и масштаб объекта
            Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius * transform.lossyScale.x);
        }
        else if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
#endif
}
