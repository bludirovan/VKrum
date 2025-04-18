using UnityEngine;

public class GravityAreaUp : GravityArea
{
    [Header("Цвета гравитации")]
    public Color positiveColor = new Color(0.2f, 0.6f, 1f, 1f); // Притяжение — голубой
    public Color negativeColor = new Color(1f, 0.4f, 0.4f, 1f); // Отталкивание — красный

    public override Vector3 GetGravityDirection(GravityBody gravityBody, bool isPositive)
    {
        Vector3 direction = -transform.up;
        return isPositive ? direction : -direction;
    }

    public override Vector3 GetGravityDirection(GravityBody gravityBody)
    {
        return GetGravityDirection(gravityBody, LocalPolarity);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = LocalPolarity ? positiveColor : negativeColor;

        Vector3 origin = transform.position;
        Vector3 direction = GetGravityDirection(null); // Без gravityBody, т.к. он не нужен
        Gizmos.DrawLine(origin, origin + direction * 2f);
        Gizmos.DrawSphere(origin + direction * 2f, 0.1f);
    }
#endif
}
