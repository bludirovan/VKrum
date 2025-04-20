using UnityEngine;

public class GravityAreaCenter : GravityArea
{
    [Header("Ссылки для смены материала")]
    public Renderer targetRenderer;
    public Material positiveMaterial;
    public Material negativeMaterial;

    [Header("Дополнительный материал")]
    [Tooltip("Если указать — при вызове ChangeToSpecificMaterial этот материал будет установлен")]
    public Material specificMaterial;

    private bool lastPolarity;

    void Start()
    {
        ApplyMaterial();
        lastPolarity = LocalPolarity;
    }

    void Update()
    {
        if (lastPolarity != LocalPolarity)
        {
            ApplyMaterial();
            lastPolarity = LocalPolarity;
        }
    }

    private void ApplyMaterial()
    {
        if (targetRenderer == null) return;
        targetRenderer.material = LocalPolarity ? positiveMaterial : negativeMaterial;
    }

    public void ChangeToSpecificMaterial()
    {
        if (targetRenderer == null || specificMaterial == null) return;
        targetRenderer.material = specificMaterial;
        lastPolarity = LocalPolarity;
    }

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
    private void OnValidate()
    {
        ApplyMaterial();
        lastPolarity = LocalPolarity;
    }
#endif
}

/*
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
*/