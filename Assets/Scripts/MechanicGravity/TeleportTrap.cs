using UnityEngine;

public class TeleportTrap : MonoBehaviour
{
    [Header("Настройки телепорта")]
    public Transform teleportTarget;  // Точка, в которую телепортируем игрока

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = teleportTarget.position;
            // При желании можно сбросить скорость:
            var rb = other.GetComponent<Rigidbody>();
            if (rb != null) rb.velocity = Vector3.zero;
        }
    }
}
