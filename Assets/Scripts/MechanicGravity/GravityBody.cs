using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    private static float GRAVITY_FORCE = 800f;

    private Rigidbody _rigidbody;
    private List<GravityArea> _gravityAreas;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gravityAreas = new List<GravityArea>();
        _rigidbody.useGravity = true;
    }

    public GravityArea GetActiveGravityArea()
    {
        if (_gravityAreas.Count == 0)
            return null;
        _gravityAreas.Sort((area1, area2) => area1.Priority.CompareTo(area2.Priority));
        // Берём зону с максимальным приоритетом (последний элемент)
        return _gravityAreas[_gravityAreas.Count - 1];
    }


    // Вычисление направления гравитации.
    // Если присутствуют зоны, используется последняя (по приоритету) и её локальная полярность.
    public Vector3 GravityDirection
    {
        get
        {
            if (_gravityAreas.Count == 0)
                return Vector3.zero;
            _gravityAreas.Sort((area1, area2) => area1.Priority.CompareTo(area2.Priority));
            return _gravityAreas.Last().GetGravityDirection(this).normalized;
        }
    }

    void FixedUpdate()
    {
        if (_gravityAreas.Count > 0)
        {
            _rigidbody.useGravity = false;
            Vector3 gravityDir = GravityDirection;
            _rigidbody.AddForce(gravityDir * (GRAVITY_FORCE * Time.fixedDeltaTime), ForceMode.Acceleration);

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDir) * _rigidbody.rotation;
            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, targetRotation, Time.fixedDeltaTime * 3f));
        }
        else
        {
            _rigidbody.useGravity = true;
            Vector3 currentEuler = _rigidbody.rotation.eulerAngles;
            Quaternion targetRotation = Quaternion.Euler(0, currentEuler.y, 0);
            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, targetRotation, Time.fixedDeltaTime * 3f));
        }
    }

    public void AddGravityArea(GravityArea gravityArea)
    {
        if (!_gravityAreas.Contains(gravityArea))
            _gravityAreas.Add(gravityArea);
    }

    public void RemoveGravityArea(GravityArea gravityArea)
    {
        if (_gravityAreas.Contains(gravityArea))
            _gravityAreas.Remove(gravityArea);
    }
}