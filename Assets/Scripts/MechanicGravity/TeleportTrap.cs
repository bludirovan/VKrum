using UnityEngine;

public class TeleportTrap : MonoBehaviour
{
    [Header("Настройки телепорта")]
    public Transform teleportTarget;  // Куда телепортируем
    public float freezeDuration = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Телепорт
            other.transform.position = teleportTarget.position;

            // Сброс скорости
            if (other.TryGetComponent<Rigidbody>(out var rb))
                rb.velocity = Vector3.zero;

            // Блокировка управления
            if (other.TryGetComponent<MainController>(out var player))
                player.FreezeMovement(freezeDuration);
        }
    }
}
