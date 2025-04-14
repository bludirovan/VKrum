using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class GravityArea : MonoBehaviour
{
    [SerializeField] private int _priority = 0;
    public int Priority => _priority;

    // Локальная полярность поля (true = положительная, false = отрицательная)
    [SerializeField] protected bool _localPolarity = true;
    public bool LocalPolarity { get => _localPolarity; set => _localPolarity = value; }

    // Абстрактные методы для получения направления гравитации
    public abstract Vector3 GetGravityDirection(GravityBody gravityBody, bool isPositive);
    public abstract Vector3 GetGravityDirection(GravityBody gravityBody);

    void Start()
    {
        // Обеспечиваем, что Collider работает как триггер
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out GravityBody gravityBody))
        {
            gravityBody.AddGravityArea(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out GravityBody gravityBody))
        {
            gravityBody.RemoveGravityArea(this);
        }
    }
}
