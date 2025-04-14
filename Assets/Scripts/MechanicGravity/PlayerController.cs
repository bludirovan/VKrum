using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Параметры игрока")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public Transform cameraTransform;

    [Tooltip("Скорость выравнивания по нормали поверхности")]
    public float alignSpeed = 5f;

    // Переменная для определения, находится ли персонаж на поверхности
    private bool isGrounded = false;

    private Rigidbody rb;

    // Можно задать слой, с которым будем считать «землёй»
    public LayerMask groundLayer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Начинаем с глобальной гравитацией (она будет отключаться при входе в зону поля)
        rb.useGravity = true;
    }

    private void Update()
    {
        // Обрабатываем ввод перемещения только если персонаж на поверхности
        if (isGrounded)
        {
            // Получаем ввод от пользователя
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 moveInput = new Vector3(h, 0, v).normalized;

            // При изменении ориентации персонажа его "верх" (transform.up) уже может быть наклонен,
            // поэтому «горизонталь» – это плоскость, перпендикулярная transform.up.
            Vector3 localUp = transform.up;
            Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, localUp).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, localUp).normalized;
            Vector3 moveDirection = (camForward * moveInput.z + camRight * moveInput.x).normalized;

            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            rb.MovePosition(transform.position + movement);

            // Прыжок разрешён только если персонаж на поверхности
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
            }

        }
    }

    private void FixedUpdate()
    {
        bool inField = false;
        GravityField[] fields = FindObjectsOfType<GravityField>();
        foreach (GravityField field in fields)
        {
            Vector3 direction = field.transform.position - transform.position;
            float distance = direction.magnitude;
            if (distance < field.effectRadius && distance > 0.1f)
            {
                inField = true;
                // Расчитываем силу поля с обратной зависимостью от квадрата расстояния
                float forceMagnitude = field.gravitationalForce / (distance * distance);
                Vector3 force = direction.normalized * forceMagnitude;
                if (!field.isAttractive)
                    force = -force;

                // Отключаем глобальную гравитацию, если находимся в поле
                rb.useGravity = false;
                rb.AddForce(force, ForceMode.Acceleration);

                // Выравнивание ориентации персонажа: новая "верхняя" ориентация – против силы
                Vector3 targetUp = (transform.position - field.transform.position).normalized;
                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);


                /*
                Vector3 newUp = -force.normalized;
                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, newUp) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, alignSpeed * Time.fixedDeltaTime);
                */
            }
        }
        if (!inField)
        {
            rb.useGravity = true;
        }
    }

    
    // Используем методы коллизии для определения, когда персонаж касается поверхности (земли)
    private void OnCollisionEnter(Collision collision)
    {
        // Можно дополнительно проверять тег или слой, например:
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
        }
    }
    
}
