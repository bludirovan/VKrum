// MainController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody), typeof(GravityBody))]
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
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _jumpForce = 7f;
    [SerializeField] private int _extraJumps = 1;
    [SerializeField] private float _maxTurnSpeed = 180f;
    [SerializeField] private float _runThreshold = 8f;

    [Header("References")]
    [SerializeField] private Joystick _joystick;
    [SerializeField] private Button _jumpButton;
    [SerializeField] private Button _polarityButton;
    [SerializeField] private Animator _animator;

    [Header("Animator Parameters")]
    [SerializeField] private string _speedParam = "Speed";
    [SerializeField] private string _isWalkingParam = "isWalking";
    [SerializeField] private string _isSprintingParam = "isSprinting";
    [SerializeField] private string _isInAirParam = "isInAir";
    [SerializeField] private string _jumpTriggerParam = "Jump";

    private Rigidbody _rb;
    private GravityBody _gravityBody;
    private Vector3 _inputDir;
    private float _currentSpeed;
    private int _jumpsRemaining;
    private bool _frozen;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();

        _currentSpeed = 0f;
        _jumpsRemaining = _extraJumps;

        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (_jumpButton != null) _jumpButton.onClick.AddListener(OnJumpButton);
        if (_polarityButton != null) _polarityButton.onClick.AddListener(SwitchPolarity);
    }

    void Update()
    {
        if (_frozen) return;

        // 1) Ground check & in-air animation
        bool isGrounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundMask);
        _animator.SetBool(_isInAirParam, !isGrounded);
        if (isGrounded) _jumpsRemaining = _extraJumps;

        // 2) Input
        float h = _joystick != null ? _joystick.Horizontal : Input.GetAxis("Horizontal");
        float v = _joystick != null ? _joystick.Vertical : Input.GetAxis("Vertical");
        _inputDir = new Vector3(h, 0f, v);
        float inputMag = _inputDir.magnitude;
        _inputDir = inputMag > 0.1f ? _inputDir.normalized : Vector3.zero;

        // 3) Speed & sprint logic
        bool wantSprint = Input.GetKey(KeyCode.LeftShift) && inputMag > 0.1f;
        float targetSpeed = inputMag > 0.1f
            ? (wantSprint ? _sprintSpeed : _walkSpeed)
            : 0f;
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _acceleration * Time.deltaTime);

        // 4) Animator: Speed float (для blend tree, если нужен)
        _animator.SetFloat(_speedParam, _currentSpeed, 0.1f, Time.deltaTime);

        // 5) Animator: walking / sprinting bool'ы (только на земле)
        if (isGrounded)
        {
            bool isWalking = _currentSpeed > 0.1f;
            bool isSprinting = isWalking && (wantSprint || _currentSpeed > _runThreshold);
            _animator.SetBool(_isWalkingParam, isWalking);
            _animator.SetBool(_isSprintingParam, isSprinting);
        }

        // 6) Jump input
        if (Input.GetKeyDown(KeyCode.Space) && _jumpsRemaining > 0)
        {
            _animator.SetTrigger(_jumpTriggerParam);
            PerformJump();
            _jumpsRemaining--;
        }
    }

    void FixedUpdate()
    {
        if (_frozen || _inputDir == Vector3.zero) return;

        var area = _gravityBody.GetActiveGravityArea();
        if (area != null)
            MoveInCustomGravity(area.GetGravityDirection(_gravityBody, area.LocalPolarity).normalized);
        else
            MoveInNormalGravity();
    }

    private void MoveInNormalGravity()
    {
        float targetAngle = Mathf.Atan2(_inputDir.x, _inputDir.z) * Mathf.Rad2Deg
                            + _cameraTransform.eulerAngles.y;
        Quaternion targetRot = Quaternion.Euler(0f, targetAngle, 0f);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRot, _maxTurnSpeed * Time.fixedDeltaTime
        );
        _rb.MovePosition(_rb.position + transform.forward * (_currentSpeed * Time.fixedDeltaTime));
    }

    private void MoveInCustomGravity(Vector3 gravDir)
    {
        Vector3 camF = Vector3.ProjectOnPlane(_cameraTransform.forward, gravDir).normalized;
        Vector3 camR = Vector3.ProjectOnPlane(_cameraTransform.right, gravDir).normalized;
        Vector3 worldDir = camR * _inputDir.x + camF * _inputDir.z;
        if (worldDir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(worldDir.normalized, -gravDir);
        _rb.MoveRotation(Quaternion.RotateTowards(
            _rb.rotation, targetRot, _maxTurnSpeed * Time.fixedDeltaTime
        ));

        Vector3 moveDir = Vector3.ProjectOnPlane(_rb.rotation * Vector3.forward, gravDir).normalized;
        Vector3 delta = moveDir * (_currentSpeed * Time.fixedDeltaTime);

        if (_rb.SweepTest(delta.normalized, out RaycastHit hit, delta.magnitude))
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

    private void PerformJump()
    {
        var area = _gravityBody.GetActiveGravityArea();
        Vector3 gravDir = area != null
            ? area.GetGravityDirection(_gravityBody, area.LocalPolarity).normalized
            : Physics.gravity.normalized;

        _rb.velocity = Vector3.zero;
        _rb.AddForce(-gravDir * _jumpForce, ForceMode.Impulse);
    }

    private void OnJumpButton()
    {
        if (_jumpsRemaining > 0)
        {
            _animator.SetTrigger(_jumpTriggerParam);
            PerformJump();
            _jumpsRemaining--;
        }
    }

    private void SwitchPolarity()
    {
        var area = _gravityBody.GetActiveGravityArea();
        if (area != null)
        {
            area.LocalPolarity = !area.LocalPolarity;
            Debug.Log($"Polarity switched on: {area.gameObject.name} -> {area.LocalPolarity}");
        }
    }

    public void FreezeMovement(float duration)
    {
        if (!_frozen)
            StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        _frozen = true;
        yield return new WaitForSeconds(duration);
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
    [SerializeField] private float _maxTurnSpeed = 180f;

    [Header("References")]
    [SerializeField] private Joystick _joystick;
    [SerializeField] private Button _jumpButton;
    [SerializeField] private Button _polarityButton;
    [SerializeField] private Animator _animator;         // Animator Controller

    [Header("Animator Parameters")]
    [SerializeField] private string _speedParam = "Speed";          // Blend Tree float
    [SerializeField] private string _isWalkingParam = "isWalking";  // bool
    [SerializeField] private string _isSprintingParam = "isSprinting"; // bool

    private Rigidbody _rb;
    private GravityBody _gravityBody;
    private Vector3 _inputDir;
    private float _currentSpeed;
    private int _jumpsRemaining;
    private bool _frozen;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();

        _currentSpeed = 0f;
        _jumpsRemaining = _extraJumps;

        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (_jumpButton != null)
            _jumpButton.onClick.AddListener(Jump);
        if (_polarityButton != null)
            _polarityButton.onClick.AddListener(SwitchPolarity);
    }

    void Update()
    {
        if (_frozen) return;

        // 1) Input
        float h = _joystick != null ? _joystick.Horizontal : Input.GetAxis("Horizontal");
        float v = _joystick != null ? _joystick.Vertical : Input.GetAxis("Vertical");
        _inputDir = new Vector3(h, 0f, v);
        float inputMag = _inputDir.magnitude;
        _inputDir = inputMag > 0.1f ? _inputDir.normalized : Vector3.zero;

        // 2) Determine target speed
        bool wantSprint = Input.GetKey(KeyCode.LeftShift) && inputMag > 0.1f;
        float targetSpeed = inputMag > 0.1f ? (wantSprint ? _sprintSpeed : _walkSpeed) : 0f;
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _acceleration * Time.deltaTime);

        // 3) Update animator parameters
        // Blend Tree speed
        _animator.SetFloat(_speedParam, _currentSpeed, 0.1f, Time.deltaTime);
        // Boolean states
        bool isWalking = _currentSpeed > 0.1f;
        bool isSprinting = wantSprint && isWalking;
        _animator.SetBool(_isWalkingParam, isWalking);
        _animator.SetBool(_isSprintingParam, isSprinting);

        // 4) Jump logic
        bool isGrounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundMask);
        if (isGrounded) _jumpsRemaining = _extraJumps;
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || _jumpsRemaining > 0))
        {
            PerformJump();
            if (!isGrounded) _jumpsRemaining--;
        }

        // 5) Switch gravity polarity
        if (Input.GetKeyDown(KeyCode.E))
            SwitchPolarity();
    }

    void FixedUpdate()
    {
        if (_frozen) return;
        if (_inputDir.magnitude < 0.1f) return;

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

    private void MoveInNormalGravity()
    {
        float targetAngle = Mathf.Atan2(_inputDir.x, _inputDir.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
        Quaternion targetRot = Quaternion.Euler(0f, targetAngle, 0f);

        float smoothFactor = 1f - Mathf.Exp(-Time.fixedDeltaTime / _turnSmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, smoothFactor);

        Vector3 moveDir = transform.forward;
        _rb.MovePosition(_rb.position + moveDir * (_currentSpeed * Time.fixedDeltaTime));
    }

    private void MoveInCustomGravity(Vector3 gravDir)
    {
        Vector3 camF = Vector3.ProjectOnPlane(_cameraTransform.forward, gravDir).normalized;
        Vector3 camR = Vector3.ProjectOnPlane(_cameraTransform.right, gravDir).normalized;
        Vector3 worldDir = camR * _inputDir.x + camF * _inputDir.z;
        if (worldDir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(worldDir.normalized, -gravDir);
        float smoothFactor = 1f - Mathf.Exp(-Time.fixedDeltaTime / _turnSmoothTime);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, smoothFactor));

        Vector3 delta = (_rb.rotation * Vector3.forward) * (_currentSpeed * Time.fixedDeltaTime);
        delta = Vector3.ProjectOnPlane(delta, gravDir);

        if (_rb.SweepTest(delta.normalized, out RaycastHit hit, delta.magnitude))
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

    private void PerformJump()
    {
        var area = _gravityBody.GetActiveGravityArea();
        Vector3 gravDir = Physics.gravity.normalized;
        if (area != null)
            gravDir = area.GetGravityDirection(_gravityBody, area.LocalPolarity).normalized;

        _rb.velocity = Vector3.zero;
        _rb.AddForce(-gravDir * _jumpForce, ForceMode.Impulse);
    }

    private void Jump()
    {
        bool grounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundMask);
        if (grounded) PerformJump();
    }

    private void SwitchPolarity()
    {
        var area = _gravityBody.GetActiveGravityArea();
        if (area != null)
        {
            area.LocalPolarity = !area.LocalPolarity;
            Debug.Log($"Polarity switched on: {area.gameObject.name} -> {area.LocalPolarity}");
        }
    }

    public void FreezeMovement(float duration)
    {
        if (!_frozen)
            StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        _frozen = true;
        yield return new WaitForSeconds(duration);
        _frozen = false;
    }
}
*/


/*

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody), typeof(GravityBody))]
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
    [SerializeField] private float _maxTurnSpeed = 180f; // градусов в секунду

    [Header("References")]
    [SerializeField] private Joystick _joystick;
    [SerializeField] private Button _jumpButton;
    [SerializeField] private Button _polarityButton;
    [SerializeField] private Animator _animator;                // Animator с blend tree

    // --------------------------------------------------

    private Rigidbody _rb;
    private GravityBody _gravityBody;
    private Vector3 _inputDir;
    private float _currentSpeed;
    private int _jumpsRemaining;
    private bool _frozen;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();

        _currentSpeed = _walkSpeed;
        _jumpsRemaining = _extraJumps;

        // Настройки Rigidbody
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Кнопки UI
        if (_jumpButton != null)
            _jumpButton.onClick.AddListener(Jump);
        if (_polarityButton != null)
            _polarityButton.onClick.AddListener(SwitchPolarity);
    }

    void Update()
    {
        if (_frozen) return;

        // 1) Ввод с джойстика или клавиатуры
        float h = _joystick != null
            ? _joystick.Horizontal
            : Input.GetAxis("Horizontal");
        float v = _joystick != null
            ? _joystick.Vertical
            : Input.GetAxis("Vertical");
        _inputDir = new Vector3(h, 0f, v).normalized;

        // 2) Рассчёт текущей скорости (с учётом спринта)
        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = sprinting ? _sprintSpeed : _walkSpeed;
        _currentSpeed = Mathf.MoveTowards(
            _currentSpeed,
            targetSpeed,
            _acceleration * Time.deltaTime
        );

        // 3) Обновляем анимацию
        bool isMovingForward = _inputDir.z > 0.1f;
        bool isSprinting = sprinting && isMovingForward;

        // Управление анимациями через триггеры
        _animator.SetBool("IsWalking", isMovingForward && !isSprinting);
        _animator.SetBool("IsSprinting", isSprinting);

        // 4) Прыжок
        bool isGrounded = Physics.CheckSphere(
            _groundCheck.position,
            _groundCheckRadius,
            _groundMask
        );
        if (isGrounded)
            _jumpsRemaining = _extraJumps;

        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || _jumpsRemaining > 0))
        {
            PerformJump();
            if (!isGrounded)
                _jumpsRemaining--;
        }

        // 5) Смена полярности гравитации
        if (Input.GetKeyDown(KeyCode.E))
            SwitchPolarity();
    }

    void FixedUpdate()
    {
        if (_frozen) return;

        // Если ввод слишком мал, не двигаем
        if (_inputDir.magnitude < 0.1f) return;

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

    // Обычное движение по земле
    private void MoveInNormalGravity()
    {
        float targetAngle = Mathf.Atan2(_inputDir.x, _inputDir.z) * Mathf.Rad2Deg
                            + _cameraTransform.eulerAngles.y;
        Quaternion targetRot = Quaternion.Euler(0f, targetAngle, 0f);

        float smoothFactor = 1f - Mathf.Exp(-Time.fixedDeltaTime / _turnSmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, smoothFactor);

        Vector3 moveDir = transform.forward;
        _rb.MovePosition(_rb.position + moveDir * (_currentSpeed * Time.fixedDeltaTime));
    }

    // Движение по поверхности в произвольном поле гравитации
    private void MoveInCustomGravity(Vector3 gravDir)
    {
        Vector3 camF = Vector3.ProjectOnPlane(_cameraTransform.forward, gravDir).normalized;
        Vector3 camR = Vector3.ProjectOnPlane(_cameraTransform.right, gravDir).normalized;
        Vector3 worldDir = (camR * _inputDir.x + camF * _inputDir.z);
        if (worldDir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(worldDir.normalized, -gravDir);
        float smoothFactor = 1f - Mathf.Exp(-Time.fixedDeltaTime / _turnSmoothTime);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, smoothFactor));

        Vector3 delta = (_rb.rotation * Vector3.forward) * (_currentSpeed * Time.fixedDeltaTime);
        delta = Vector3.ProjectOnPlane(delta, gravDir);

        if (_rb.SweepTest(delta.normalized, out RaycastHit hit, delta.magnitude))
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

    // Универсальный прыжок, учитывающий текущее поле гравитации
    private void PerformJump()
    {
        var area = _gravityBody.GetActiveGravityArea();
        Vector3 gravDir = Physics.gravity.normalized;
        if (area != null)
            gravDir = area.GetGravityDirection(_gravityBody, area.LocalPolarity).normalized;

        _rb.velocity = Vector3.zero;
        _rb.AddForce(-gravDir * _jumpForce, ForceMode.Impulse);
    }

    private void Jump()
    {
        // для UI‑кнопки
        bool grounded = Physics.CheckSphere(
            _groundCheck.position,
            _groundCheckRadius,
            _groundMask
        );
        if (grounded)
            PerformJump();
    }

    private void SwitchPolarity()
    {
        var area = _gravityBody.GetActiveGravityArea();
        if (area != null)
        {
            area.LocalPolarity = !area.LocalPolarity;
            Debug.Log($"Polarity switched on: {area.gameObject.name} -> {area.LocalPolarity}");
        }
    }

    // Возможность «заморозить» движение
    public void FreezeMovement(float duration)
    {
        if (!_frozen)
            StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        _frozen = true;
        yield return new WaitForSeconds(duration);
        _frozen = false;
    }
}*/


/*
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody), typeof(GravityBody))]
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
    [SerializeField] private float _maxTurnSpeed = 180f; // градусов в секунду


    [Header("References")]
    [SerializeField] private Joystick _joystick;
    [SerializeField] private Button _jumpButton;
    [SerializeField] private Button _polarityButton;
    [SerializeField] private Animator _animator;                // Animator с blend tree
    [SerializeField] private string _speedParam = "Velocity";   // имя float‑параметра в Animator

    // --------------------------------------------------

    private Rigidbody _rb;
    private GravityBody _gravityBody;
    private Vector3 _inputDir;
    private float _currentSpeed;
    private int _jumpsRemaining;
    private bool _frozen;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();

        _currentSpeed = _walkSpeed;
        _jumpsRemaining = _extraJumps;

        // Настройки Rigidbody
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Кнопки UI
        if (_jumpButton != null)
            _jumpButton.onClick.AddListener(Jump);
        if (_polarityButton != null)
            _polarityButton.onClick.AddListener(SwitchPolarity);
    }

    void Update()
    {
        if (_frozen) return;

        // 1) Ввод с джойстика или клавиатуры
        float h = _joystick != null
            ? _joystick.Horizontal
            : Input.GetAxis("Horizontal");
        float v = _joystick != null
            ? _joystick.Vertical
            : Input.GetAxis("Vertical");
        _inputDir = new Vector3(h, 0f, v).normalized;

        // 2) Рассчёт текущей скорости (с учётом спринта)
        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = sprinting ? _sprintSpeed : _walkSpeed;
        _currentSpeed = Mathf.MoveTowards(
            _currentSpeed,
            targetSpeed,
            _acceleration * Time.deltaTime
        );

        // 3) Обновляем параметр blend tree в Animator
        //    magnitude [0;1] показывает, насколько сильно наклонён джойстик
        //    домножаем на отношение current/max, чтобы учитывать спринт
        float speedNorm = _inputDir.magnitude * (_currentSpeed / _sprintSpeed);
        _animator.SetFloat(_speedParam, speedNorm, 0.1f, Time.deltaTime);

        // 4) Прыжок
        bool isGrounded = Physics.CheckSphere(
            _groundCheck.position,
            _groundCheckRadius,
            _groundMask
        );
        if (isGrounded)
            _jumpsRemaining = _extraJumps;

        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || _jumpsRemaining > 0))
        {
            PerformJump();
            if (!isGrounded)
                _jumpsRemaining--;
        }

        // 5) Смена полярности гравитации
        if (Input.GetKeyDown(KeyCode.E))
            SwitchPolarity();
    }

    void FixedUpdate()
    {
        if (_frozen) return;

        // Если ввод слишком мал, не двигаем
        if (_inputDir.magnitude < 0.1f) return;

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

    // Обычное движение по земле
    private void MoveInNormalGravity()
    {
        float targetAngle = Mathf.Atan2(_inputDir.x, _inputDir.z) * Mathf.Rad2Deg
                            + _cameraTransform.eulerAngles.y;
        Quaternion targetRot = Quaternion.Euler(0f, targetAngle, 0f);

        float smoothFactor = 1f - Mathf.Exp(-Time.fixedDeltaTime / _turnSmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, smoothFactor);

        Vector3 moveDir = transform.forward;
        _rb.MovePosition(_rb.position + moveDir * (_currentSpeed * Time.fixedDeltaTime));
    }

    // Движение по поверхности в произвольном поле гравитации
    private void MoveInCustomGravity(Vector3 gravDir)
    {
        Vector3 camF = Vector3.ProjectOnPlane(_cameraTransform.forward, gravDir).normalized;
        Vector3 camR = Vector3.ProjectOnPlane(_cameraTransform.right, gravDir).normalized;
        Vector3 worldDir = (camR * _inputDir.x + camF * _inputDir.z);
        if (worldDir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(worldDir.normalized, -gravDir);
        float smoothFactor = 1f - Mathf.Exp(-Time.fixedDeltaTime / _turnSmoothTime);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, smoothFactor));

        Vector3 delta = (_rb.rotation * Vector3.forward) * (_currentSpeed * Time.fixedDeltaTime);
        delta = Vector3.ProjectOnPlane(delta, gravDir);

        if (_rb.SweepTest(delta.normalized, out RaycastHit hit, delta.magnitude))
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

    // Универсальный прыжок, учитывающий текущее поле гравитации
    private void PerformJump()
    {
        var area = _gravityBody.GetActiveGravityArea();
        Vector3 gravDir = Physics.gravity.normalized;
        if (area != null)
            gravDir = area.GetGravityDirection(_gravityBody, area.LocalPolarity).normalized;

        _rb.velocity = Vector3.zero;
        _rb.AddForce(-gravDir * _jumpForce, ForceMode.Impulse);
    }

    private void Jump()
    {
        // для UI‑кнопки
        bool grounded = Physics.CheckSphere(
            _groundCheck.position,
            _groundCheckRadius,
            _groundMask
        );
        if (grounded)
            PerformJump();
    }

    private void SwitchPolarity()
    {
        var area = _gravityBody.GetActiveGravityArea();
        if (area != null)
        {
            area.LocalPolarity = !area.LocalPolarity;
            Debug.Log($"Polarity switched on: {area.gameObject.name} -> {area.LocalPolarity}");
        }
    }

    // Возможность «заморозить» движение
    public void FreezeMovement(float duration)
    {
        if (!_frozen)
            StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        _frozen = true;
        yield return new WaitForSeconds(duration);
        _frozen = false;
    }
}*/