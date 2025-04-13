using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GravityFieldVisualizer : MonoBehaviour
{
    public Color positiveColor = Color.cyan;
    public Color negativeColor = Color.red;
    public float fieldRadius = 5f;
    public int lineCount = 64;
    public bool isPositive = true;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = lineCount * 2;
        lineRenderer.widthMultiplier = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineRenderer.endColor = isPositive ? positiveColor : negativeColor;

        UpdateLines();
    }

    void UpdateLines()
    {
        Vector3 center = transform.position;

        for (int i = 0; i < lineCount; i++)
        {
            float angle = i * Mathf.PI * 2 / lineCount;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 from = center + dir * fieldRadius;
            Vector3 to = center;

            lineRenderer.SetPosition(i * 2, from);
            lineRenderer.SetPosition(i * 2 + 1, to);
        }
    }

    void Update()
    {
        // На случай если объект двигается
        UpdateLines();
    }
}

