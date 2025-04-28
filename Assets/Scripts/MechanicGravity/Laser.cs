using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour, IOutput
{
    bool activated = false;
    LineRenderer lineRenderer;

    [SerializeField] LaserRendererSettings laserRendererSettings;
    Vector3 sourcePosition;
    const float farDistance = 1000f;

    List<Vector3> bouncePositions;
    int maxBounces = 100;

    LaserSensor prevStruckLaserSensor = null;

    [SerializeField] GameObject inputGO;
    public IInput input { get; private set; }

    private void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        laserRendererSettings.Apply(lineRenderer);

        if (inputGO == null)
        {
            Debug.LogWarning($"{name}: Input GameObject is null. Laser will not activate.");
            return;
        }

        input = inputGO.GetComponent<IInput>();
        if (input == null)
        {
            Debug.LogWarning($"{name}: The input GameObject must contain a component implementing IInput.");
            return;
        }

        RegisterToInput(input);
    }

    private void RegisterToInput(IInput input)
    {
        input.onTriggered += Activate;
        input.onUntriggered += Deactivate;
    }

    private void FixedUpdate()
    {
        if (!activated)
        {
            lineRenderer.positionCount = 0;
            if (prevStruckLaserSensor != null)
            {
                LaserSensor.HandleLaser(this, prevStruckLaserSensor, null);
                prevStruckLaserSensor = null;
            }
            return;
        }

        sourcePosition = transform.position + transform.forward * 0.25f;
        bouncePositions = new List<Vector3> { sourcePosition };
        CastBeam(sourcePosition, transform.forward);

        lineRenderer.positionCount = bouncePositions.Count;
        lineRenderer.SetPositions(bouncePositions.ToArray());
    }

    public void CastBeam(Vector3 origin, Vector3 direction)
    {
        if (bouncePositions.Count > maxBounces)
            return;

        Ray ray = new Ray(origin, direction);
        bool didHit = Physics.Raycast(ray, out RaycastHit hitInfo, farDistance);

        if (!didHit)
        {
            var endPoint = origin + direction * farDistance;
            bouncePositions.Add(endPoint);

            if (prevStruckLaserSensor != null)
            {
                LaserSensor.HandleLaser(this, prevStruckLaserSensor, null);
                prevStruckLaserSensor = null;
            }
            return;
        }

        bouncePositions.Add(hitInfo.point);

        var reflective = hitInfo.collider.GetComponent<ILaserReflective>();
        if (reflective != null)
        {
            reflective.Reflect(this, ray, hitInfo);
        }
        else
        {
            var currentSensor = hitInfo.collider.GetComponent<LaserSensor>();
            if (currentSensor != prevStruckLaserSensor)
            {
                LaserSensor.HandleLaser(this, prevStruckLaserSensor, currentSensor);
            }
            prevStruckLaserSensor = currentSensor;
        }
    }

    public void Activate(IInput source)
    {
        activated = true;
    }

    public void Deactivate(IInput source)
    {
        activated = false;
    }

    
}
