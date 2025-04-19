// MainController.cs

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MainController : MonoBehaviour
{
    [Header("Ground & Camera")]
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.3f;
    [SerializeField] private Transform _cameraTransform;

    [Header("Movement Settings")]
    [SerializeField] private float _walkSpeed = 6f;
    [SerializeField] private float _sprintSpeed = 12f;
    [SerializeField] private float _turnSmoothTime = 0.1f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _jumpForce = 7f;
    [SerializeField] private int _extraJumps = 1;

    [Header("References")]
    [SerializeField] private Joystick _joystick;

    private Rigidbody _rb;
    private GravityBody _gravityBody;
    private Vector3 _inputDir;
    private float _turnSmoothVel;
    private float _currentSpeed;
    private int _jumpsRemaining;
    private bool _frozen;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();
        _currentSpeed = _walkSpeed;
        _jumpsRemaining = _extraJumps;

        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

    }

    void Update()
    {
        if (_frozen) return;

        // — Читаем ввод для движения
        float h = (_joystick != null)
            ? _joystick.Horizontal
            : Input.GetAxis("Horizontal");
        float v = (_joystick != null)
            ? _joystick.Vertical
            : Input.GetAxis("Vertical");
        _inputDir = new Vector3(h, 0f, v).normalized;

        // — Спринт
        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = sprinting ? _sprintSpeed : _walkSpeed;
        _currentSpeed = Mathf.MoveTowards(
            _currentSpeed, targetSpeed,
            _acceleration * Time.deltaTime
        );

        // — Обработка прыжка
        bool isGrounded = Physics.CheckSphere(
            _groundCheck.position,
            _groundCheckRadius,
            _groundMask
        );
        if (isGrounded) _jumpsRemaining = _extraJumps;

        if (Input.GetKeyDown(KeyCode.Space)
            && (isGrounded || _jumpsRemaining > 0))
        {
            // берём активную область (если есть)
            var area = _gravityBody.GetActiveGravityArea();
            // направление гравитации: либо из области с учётом LocalPolarity, либо Physics.gravity
            Vector3 gravDir = Physics.gravity.normalized;
            if (area != null)
                gravDir = area.GetGravityDirection(_gravityBody, area.LocalPolarity).normalized;

            // прыжок
            _rb.velocity = Vector3.zero;
            _rb.AddForce(-gravDir * _jumpForce,
                         ForceMode.Impulse);

            if (!isGrounded) _jumpsRemaining--;
        }

        // — Смена полярности текущего поля
        if (Input.GetKeyDown(KeyCode.E))
        {
            var area = _gravityBody.GetActiveGravityArea();
            if (area != null)
            {
                area.LocalPolarity = !area.LocalPolarity;
                Debug.Log($"GravityArea \"{area.name}\" polarity = {area.LocalPolarity}");
            }
        }
    }

    void FixedUpdate()
    {
        if (_frozen || _inputDir.magnitude < 0.1f) return;

        var area = _gravityBody.GetActiveGravityArea();
        if (area != null)
        {
            Vector3 gravDir = area.GetGravityDirection(_gravityBody, area.LocalPolarity).normalized;
            MoveInCustomGravity(gravDir);
        }
        else
        {
            MoveInNormalGravity();
        }
    }

    // — Обычная гравитация — движение «по земле»
    private void MoveInNormalGravity()
    {
        // (1) вычисляем угол и targetRot
        float targetAngle = Mathf.Atan2(_inputDir.x, _inputDir.z) * Mathf.Rad2Deg
                            + _cameraTransform.eulerAngles.y;
        Quaternion targetRot = Quaternion.Euler(0f, targetAngle, 0f);

        // (2) плавный переход
        float smoothFactor = 1f - Mathf.Exp(-Time.fixedDeltaTime / _turnSmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, smoothFactor);

        // (3) движение вперёд
        Vector3 moveDir = transform.forward;
        _rb.MovePosition(_rb.position + moveDir * (_currentSpeed * Time.fixedDeltaTime));
    }


    // — Кастомная гравитация — движение по поверхности поля
    private void MoveInCustomGravity(Vector3 gravDir)
    {
        // (1) Считаем желаемое направление worldDir, как у вас было...
        Vector3 camF = Vector3.ProjectOnPlane(_cameraTransform.forward, gravDir).normalized;
        Vector3 camR = Vector3.ProjectOnPlane(_cameraTransform.right, gravDir).normalized;
        Vector3 worldDir = (camR * _inputDir.x + camF * _inputDir.z);
        if (worldDir.sqrMagnitude < 0.01f) return;

        // (2) Определяем targetRot
        Quaternion targetRot = Quaternion.LookRotation(worldDir.normalized, -gravDir);

        // (3) Плавный поворот
        float smoothFactor = 1f - Mathf.Exp(-Time.fixedDeltaTime / _turnSmoothTime);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, smoothFactor));

        // (4) Движение по поверхности (SweepTest + slide), как у вас уже настроено...
        Vector3 delta = (_rb.rotation * Vector3.forward) * (_currentSpeed * Time.fixedDeltaTime);
        delta = Vector3.ProjectOnPlane(delta, gravDir);

        RaycastHit hit;
        if (_rb.SweepTest(delta.normalized, out hit, delta.magnitude))
        {
            Vector3 slide = Vector3.ProjectOnPlane(delta, hit.normal);
            slide = Vector3.ProjectOnPlane(slide, gravDir);
            _rb.MovePosition(_rb.position + slide);
        }
        else
        {
            _rb.MovePosition(_rb.position + delta);
        }
    }



    // — Для TeleportTrap и прочих — «заморозка» движения на время
    public void FreezeMovement(float duration)
    {
        if (!_frozen) StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float d)
    {
        _frozen = true;
        yield return new WaitForSeconds(d);
        _frozen = false;
    }
}





/*
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody), typeof(GravityBody))]
public class MainController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private Transform _cam;

    [Header("UI Controls")]
    public Joystick moveJoystick;
    public Button jumpButton;
    public Button polarityButton;

    [Header("Movement Settings")]
    [SerializeField] private float _groundCheckRadius = 0.3f;
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _turnSpeed = 10f;
    [SerializeField] private float _jumpForce = 50f;

    private Rigidbody _rb;
    private GravityBody _gravityBody;
    private Vector3 _moveDirection;
    private bool _isFrozen = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();
        jumpButton.onClick.AddListener(Jump);
        polarityButton.onClick.AddListener(SwitchPolarity);
    }

    void Update()
    {
        if (_isFrozen) return;

        // Read joystick input
        float rawX = moveJoystick.Horizontal;
        float rawZ = moveJoystick.Vertical;
        Vector3 input = new Vector3(rawX, 0f, rawZ);

        if (input.magnitude > 0.1f)
        {
            // Align movement to camera and gravity
            Vector3 gravityUp = -_gravityBody.GravityDirection.normalized;
            Vector3 camForward = Vector3.ProjectOnPlane(_cam.forward, gravityUp).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(_cam.right, gravityUp).normalized;
            _moveDirection = (camForward * input.z + camRight * input.x).normalized;
        }
        else
        {
            _moveDirection = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        if (_isFrozen) return;

        if (_moveDirection.magnitude > 0.1f)
        {
            // Move
            Vector3 newPos = _rb.position + _moveDirection * (_speed * Time.fixedDeltaTime);
            _rb.MovePosition(newPos);

            // Rotate smoothly towards move direction
            Quaternion targetRot = Quaternion.LookRotation(_moveDirection, -_gravityBody.GravityDirection);
            Quaternion slerped = Quaternion.Slerp(_rb.rotation, targetRot, _turnSpeed * Time.fixedDeltaTime);
            _rb.MoveRotation(slerped);
        }
    }

    private void Jump()
    {
        if (_isFrozen) return;
        bool grounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundMask);
        if (!grounded) return;

        Vector3 jumpDir = _rb.useGravity ? Vector3.up : -_gravityBody.GravityDirection.normalized;
        _rb.AddForce(jumpDir * _jumpForce, ForceMode.Impulse);
    }

    private void SwitchPolarity()
    {
        if (_isFrozen) return;
        var area = _gravityBody.GetActiveGravityArea();
        if (area != null)
        {
            area.LocalPolarity = !area.LocalPolarity;
            Debug.Log($"Polarity switched on: {area.gameObject.name}");
        }
    }

    public void FreezeMovement(float duration)
    {
        if (!_isFrozen)
            StartCoroutine(FreezeCoroutine(duration));
    }

    private IEnumerator FreezeCoroutine(float duration)
    {
        _isFrozen = true;
        yield return new WaitForSeconds(duration);
        _isFrozen = false;
    }
}*/




/*
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private Transform _cam;

    [Header("Управление UI")]
    public Joystick moveJoystick;
    public Button jumpButton;
    public Button polarityButton;

    [Header("Параметры движения")]
    private float _groundCheckRadius = 0.3f;
    private float _speed = 8f;
    private float _turnSpeed = 1500f;
    private float _jumpForce = 50f;

    private Rigidbody _rigidbody;
    private GravityBody _gravityBody;
    private Vector3 _direction;
    private bool isFrozen = false;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();

        jumpButton.onClick.AddListener(Jump);
        polarityButton.onClick.AddListener(SwitchPolarity);
    }

    void Update()
    {
        if (isFrozen) return;

        // Считаем сырые значения с джойстика
        float rawX = moveJoystick.Horizontal;
        float rawZ = moveJoystick.Vertical;

        // Запрещаем отрицательное значение по Z (назад)
        float forwardZ = Mathf.Max(0f, rawZ);

        // Собираем и нормализуем направление
        _direction = new Vector3(rawX, 0f, forwardZ).normalized;
    }


    void FixedUpdate()
    {
        if (isFrozen) return;

        bool isRunning = _direction.magnitude > 0.1f;
        if (isRunning)
        {
            // Движение вперёд/назад
            Vector3 movement = transform.forward * _direction.z;
            _rigidbody.MovePosition(_rigidbody.position + movement * (_speed * Time.fixedDeltaTime));

            // Поворот
            Quaternion turnRotation = Quaternion.Euler(0f, _direction.x * (_turnSpeed * Time.fixedDeltaTime), 0f);
            Quaternion newRotation = Quaternion.Slerp(_rigidbody.rotation, _rigidbody.rotation * turnRotation, Time.fixedDeltaTime * 3f);
            _rigidbody.MoveRotation(newRotation);
        }
    }

    public void Jump()
    {
        if (isFrozen) return;

        bool isGrounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundMask);
        if (!isGrounded) return;

        if (_rigidbody.useGravity)
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        else
            _rigidbody.AddForce(-_gravityBody.GravityDirection * _jumpForce, ForceMode.Impulse);
    }

    public void SwitchPolarity()
    {
        if (isFrozen) return;

        var activeArea = _gravityBody.GetActiveGravityArea();
        if (activeArea != null)
        {
            activeArea.LocalPolarity = !activeArea.LocalPolarity;
            Debug.Log("Переключили полярность: " + activeArea.gameObject.name);
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
}*/