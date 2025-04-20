using UnityEngine;

public class GravityAreaUp : GravityArea
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
        // отслеживаем смену полярности
        if (lastPolarity != LocalPolarity)
        {
            ApplyMaterial();
            lastPolarity = LocalPolarity;
        }
    }

    /// <summary>
    /// Устанавливает материал в зависимости от LocalPolarity
    /// </summary>
    private void ApplyMaterial()
    {
        if (targetRenderer == null) return;
        targetRenderer.material = LocalPolarity ? positiveMaterial : negativeMaterial;
    }

    /// <summary>
    /// Принудительно выставляет тот материал, который лежит в specificMaterial
    /// </summary>
    public void ChangeToSpecificMaterial()
    {
        if (targetRenderer == null || specificMaterial == null) return;
        targetRenderer.material = specificMaterial;
        // сбрасываем lastPolarity, чтобы при следующей смене полярности ApplyMaterial() снова отработал
        lastPolarity = LocalPolarity;
    }

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
    private void OnValidate()
    {
        // чтобы в редакторе сразу видеть изменение любого из материалов
        ApplyMaterial();
        lastPolarity = LocalPolarity;
    }
#endif
}


/*
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
*/