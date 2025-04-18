using UnityEngine;

[CreateAssetMenu(menuName = "Laser/Renderer Settings")]
public class LaserRendererSettings : ScriptableObject
{
    [SerializeField] public Color color = Color.red;
    [SerializeField] public float width = 0.1f;
    [SerializeField, Range(1f, 200f)] public float emissionAmount = 10f;

    public void Apply(LineRenderer lineRenderer)
    {
        // Создаем новый материал с Unlit Shader'ом (URP)
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

        // Настраиваем цвет и свечение
        mat.color = color;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * emissionAmount);

        lineRenderer.material = mat;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
}
