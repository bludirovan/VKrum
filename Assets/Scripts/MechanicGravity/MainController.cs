using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class MainController : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private Transform _cam;

    [Header("Управление UI")]
    public Joystick moveJoystick;
    public Button jumpButton;
    public Button polarityButton;

    [Header("Параметры движения")]
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _turnSpeed = 5f;
    [SerializeField] private float _jumpForce = 50f;
    [SerializeField] private float _groundCheckRadius = 0.3f;
    [SerializeField] private float _deadZone = 0.1f;

    private Rigidbody _rigidbody;
    private GravityBody _gravityBody;
    private Vector3 _inputDir;
    private bool isFrozen = false;
    private bool _isGrounded = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();

        // Предотвращение кувырков
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (_cam == null && Camera.main != null)
            _cam = Camera.main.transform;
    }

    private void Start()
    {
        if (jumpButton != null) jumpButton.onClick.AddListener(Jump);
        if (polarityButton != null) polarityButton.onClick.AddListener(SwitchPolarity);
    }

    private void Update()
    {
        if (isFrozen || _cam == null) return;

        // Обновляем состояние "на земле"
        //_isGrounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundMask);

        float h = moveJoystick != null ? moveJoystick.Horizontal : Input.GetAxis("Horizontal");
        float v = moveJoystick != null ? moveJoystick.Vertical : Input.GetAxis("Vertical");

        // Направление движения относительно камеры
        Vector3 gravityUp = -_gravityBody.GravityDirection;
        Vector3 camForward = Vector3.ProjectOnPlane(_cam.forward, gravityUp).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(_cam.right, gravityUp).normalized;

        Vector3 move = camForward * v + camRight * h;
        _inputDir = move.sqrMagnitude > _deadZone * _deadZone ? move.normalized : Vector3.zero;
    }

    private void FixedUpdate()
    {
        //if (isFrozen || !_isGrounded || _inputDir == Vector3.zero) return;
        if (isFrozen || _inputDir == Vector3.zero) return;

        Vector3 gravityUp = -_gravityBody.GravityDirection;

        // Плавный поворот
        Quaternion targetRot = Quaternion.LookRotation(_inputDir, gravityUp);
        Quaternion smoothRot = Quaternion.Slerp(_rigidbody.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime);
        _rigidbody.MoveRotation(smoothRot);

        // Перемещение
        Vector3 movement = _inputDir * _speed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(_rigidbody.position + movement);
    }

    public void Jump()
    {
        if (isFrozen || !_isGrounded) return;

        Vector3 jumpDir = _rigidbody.useGravity ? Vector3.up : -_gravityBody.GravityDirection;
        _rigidbody.AddForce(jumpDir * _jumpForce, ForceMode.Impulse);
    }

    public void SwitchPolarity()
    {
        if (isFrozen) return;

        var area = _gravityBody.GetActiveGravityArea();
        if (area != null)
        {
            area.LocalPolarity = !area.LocalPolarity;
            Debug.Log($"Полярность в '{area.name}' переключена: {area.LocalPolarity}");
        }
    }

    public void FreezeMovement(float duration)
    {
        if (!isFrozen)
            StartCoroutine(FreezeCoroutine(duration));
    }

    private IEnumerator FreezeCoroutine(float duration)
    {
        isFrozen = true;
        yield return new WaitForSeconds(duration);
        isFrozen = false;
    }

    // Вспомогательное для других скриптов или анимации
    public bool IsGrounded => _isGrounded;
}
