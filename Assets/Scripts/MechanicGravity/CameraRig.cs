using UnityEngine;

public class CameraRotateWithPlayer : MonoBehaviour
{
    [Tooltip("Трансформ игрока")]
    public Transform player;
    [Tooltip("Скорость сглаживания поворота")]
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        if (player == null) return;

        // Хотим оставить текущий pitch (X) и roll (Z), но взять yaw игрока (Y):
        Vector3 current = transform.eulerAngles;
        float targetYaw = player.eulerAngles.y;

        // Сглаженно интерполируем от текущего угла к целевому
        float newYaw = Mathf.LerpAngle(current.y, targetYaw, Time.deltaTime * smoothSpeed);

        // Применяем новый поворот
        transform.rotation = Quaternion.Euler(current.x, newYaw, current.z);
    }
}
