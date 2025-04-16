using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class npc : MonoBehaviour
{
    [Header("Параметры движения")]
    [Tooltip("Точка A перемещения пилы")]
    public Transform pointA;
    [Tooltip("Точка B перемещения пилы")]
    public Transform pointB;
    [Tooltip("Скорость перемещения пилы")]
    public float speed = 2f;

    [Header("Настройки столкновения и смерти")]
    [Tooltip("Задержка перед перезапуском сцены")]
    public float restartDelay = 3f;
    [Tooltip("Панель смерти (назначить в Inspector)")]
    public GameObject deathScreen;

    // Метод Update отвечает за движение пилы и её разворот
    void Update()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogWarning("Не назначены pointA или pointB для пилы.");
            return;
        }

        // Вычисляем параметр t, который будет изменяться от 0 до 1 (туда-обратно)
        float t = Mathf.PingPong(Time.time * speed, 1f);

        // Определяем текущую позицию между точками A и B
        Vector3 currentPosition = Vector3.Lerp(pointA.position, pointB.position, t);
        transform.position = currentPosition;

        // Для разворота вычисляем положение немного вперёд
        float deltaT = 0.01f;
        float tNext = Mathf.PingPong((Time.time + deltaT) * speed, 1f);
        Vector3 nextPosition = Vector3.Lerp(pointA.position, pointB.position, tNext);

        // Определяем направление движения
        Vector3 moveDirection = (nextPosition - currentPosition).normalized;
        if (moveDirection != Vector3.zero)
        {
            // Камера разворачивается, чтобы «смотреть» в направлении движения пилы
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    // При соприкосновении с игроком вызываем его смерть
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Die();
            }
            else
            {
                Debug.LogWarning("PlayerHealth не найден на объекте Player. Перезагрузка сцены.");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
