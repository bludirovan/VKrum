using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class GravityArea : MonoBehaviour
{
    [SerializeField] private int _priority;
    public int Priority => _priority;

    // Локальная полярность конкретного гравитационного поля
    [SerializeField] private bool _localPolarity = true;
    public bool LocalPolarity => _localPolarity;

    // Метод для переключения локальной полярности
    public void SwitchPolarity()
    {
        _localPolarity = !_localPolarity;
        Debug.Log($"{gameObject.name}: Полярность изменена на {_localPolarity}");
    }

    // Получение направления с учётом переданного значения полярности
    public abstract Vector3 GetGravityDirection(GravityBody gravityBody, bool isPositive);

    // Получение направления с использованием локальной полярности поля
    public virtual Vector3 GetGravityDirection(GravityBody gravityBody)
    {
        return GetGravityDirection(gravityBody, _localPolarity);
    }

    void Start()
    {
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
