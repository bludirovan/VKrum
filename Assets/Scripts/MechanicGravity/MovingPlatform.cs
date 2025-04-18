using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA, pointB;
    public float speed = 2f;
    public bool loop = true;

    private Rigidbody _rb;
    private Vector3 _target;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        transform.position = pointA.position;
        _target = pointB.position;
    }

    void FixedUpdate()
    {
        Vector3 newPos = Vector3.MoveTowards(_rb.position, _target, speed * Time.fixedDeltaTime);
        _rb.MovePosition(newPos);

        if (Vector3.Distance(newPos, _target) < 0.05f)
        {
            if (loop)
                _target = _target == pointA.position ? pointB.position : pointA.position;
            else
                enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (pointA && pointB)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawSphere(pointA.position, 0.1f);
            Gizmos.DrawSphere(pointB.position, 0.1f);
        }
    }

    /*
    [Header("Точки движения")]
    public Transform pointA;
    public Transform pointB;
    [Header("Настройки")]
    public float speed = 2f;
    public bool loop = true;

    private Vector3 _target;

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("Укажите оба Point A и Point B в MovingPlatform!");
            enabled = false;
            return;
        }
        transform.position = pointA.position;
        _target = pointB.position;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _target) < 0.05f)
        {
            if (loop)
                _target = _target == pointA.position ? pointB.position : pointA.position;
            else
                enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (pointA && pointB)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawSphere(pointA.position, 0.1f);
            Gizmos.DrawSphere(pointB.position, 0.1f);
        }
    }*/
}
