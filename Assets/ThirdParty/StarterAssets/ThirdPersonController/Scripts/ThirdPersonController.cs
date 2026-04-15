using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private PlayerStats playerStats;

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("Environment / Footsteps")]
        [SerializeField] private EnvironmentController environment;
        public AudioClip[] SnowFootstepClips;
        public AudioClip[] CaveFootstepClips;

        [Header("Landing Sounds")]
        public AudioClip SnowLandingClip;
        public AudioClip CaveLandingClip;

        // === Attack Lock ===
        [Header("Combat")]
        [Tooltip("If true, movement is blocked while attack animation plays (via Animation Events).")]
        public bool LockMoveWhileAttacking = true;

        private bool _isAttacking;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private bool _wasBackward;
        private float _backwardLockedYaw;
        private float _environmentMoveMultiplier = 1f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDMoveX;
        private int _animIDMoveY;


#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

            // ⭐ 중요: Animator는 보통 자식(워리어 모델)에 있음
            _animator = GetComponentInChildren<Animator>(true);
            _hasAnimator = _animator != null;

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            AssignAnimationIDs();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            // 안전장치: 런타임에 모델 갈아끼우면 Animator가 바뀔 수 있음
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>(true);
                _hasAnimator = _animator != null;
            }

            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDMoveX = Animator.StringToHash("MoveX");
            _animIDMoveY = Animator.StringToHash("MoveY");

        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            if (_hasAnimator)
                _animator.SetBool(_animIDGrounded, Grounded);
        }

        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw,
                0.0f
            );
        }

        private void Move()
        {
            // 공격 중 이동/회전 잠금
            if (LockMoveWhileAttacking && _isAttacking)
            {
                _speed = 0f;
                _animationBlend = Mathf.Lerp(_animationBlend, 0f, Time.deltaTime * SpeedChangeRate);
                _controller.Move(new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

                if (_hasAnimator)
                {
                    _animator.SetFloat(_animIDSpeed, _animationBlend);
                    _animator.SetFloat(_animIDMotionSpeed, 0f);
                    // 추가: 뒷걸음 파라미터도 0으로
                    _animator.SetFloat(_animIDMoveX, 0f);
                    _animator.SetFloat(_animIDMoveY, 0f);
                }
                return;
            }

            // 뒷걸음 판단 
            bool isBackward = _input.move.y < -0.1f;

            // 뒤로일 때만 달리기 금지 (좌/우/전진은 허용)
            bool canSprint = _input.sprint && !isBackward;

            //기본 이동속도
            float baseTargetSpeed = canSprint ? SprintSpeed : MoveSpeed;
            //장비를 통해 이동속도 배율 적용, 환경에 따른 이동속도 배율 적용
            float targetSpeed = baseTargetSpeed * MoveSpeedMultiplier * _environmentMoveMultiplier;
          
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // 입력(카메라 기준) 방향
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            //  회전 : 뒤로 입력이면 회전하지 않음 
            if (_input.move != Vector2.zero && !isBackward)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
            // 뒷걸음질 칠때는 _targetRotation 유지(= 현재 바라보는 방향 유지)

            // ---- 이동 방향: 항상 "카메라 기준 입력 방향"으로 이동 (회전과 분리) ----
            Vector3 moveDirection = Quaternion.Euler(0.0f, _mainCamera.transform.eulerAngles.y, 0.0f) * inputDirection;

            _controller.Move(moveDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
                // 기본은 "앞(또는 제자리)"로만 보냄
                _animator.SetFloat(_animIDMoveX, 0f);

                // 뒤로일 때만 -1, 아니면 +1(또는 0)
                if (_input.move == Vector2.zero)
                    _animator.SetFloat(_animIDMoveY, 0f);
                else
                    _animator.SetFloat(_animIDMoveY, isBackward ? -1f : 1f);
            }

            // ---- 뒷걸음 "진입 순간"에 방향을 잠금 ----
            if (isBackward && !_wasBackward)
            {
                // S를 처음 누른 순간: 카메라 정면 방향으로 딱 맞추고 그 방향을 잠금
                _backwardLockedYaw = _mainCamera.transform.eulerAngles.y;
                transform.rotation = Quaternion.Euler(0f, _backwardLockedYaw, 0f);
                _targetRotation = _backwardLockedYaw; // 기존 코드 흐름 유지용
            }
            _wasBackward = isBackward;


        }

        // 이동 속도 배율 - 장비를 통해 이동속도 변경 
        private float MoveSpeedMultiplier
        {
            get
            {
                if (playerStats == null)
                    return 1f;

                return playerStats.MoveSpeedMultiplier;
            }
        }

        private void JumpAndGravity()
        {
            // 공격 중 점프 입력 무시
            if (LockMoveWhileAttacking && _isAttacking)
                _input.jump = false;

            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f)
                    _verticalVelocity = -2f;

                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    if (_hasAnimator)
                        _animator.SetBool(_animIDJump, true);
                }

                if (_jumpTimeoutDelta >= 0.0f)
                    _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                    _fallTimeoutDelta -= Time.deltaTime;
                else
                {
                    if (_hasAnimator)
                        _animator.SetBool(_animIDFreeFall, true);
                }

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
                _verticalVelocity += Gravity * Time.deltaTime;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = Grounded ? transparentGreen : transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius
            );
        }

        // =========================
        // Animation Events receiver
        // =========================
        // 클립에서 AttackStart / AttackEnd 이벤트가 이 함수 이름과 정확히 같아야 함
        public void AttackStart()
        {
            _isAttacking = true;
        }

        public void AttackEnd()
        {
            _isAttacking = false;
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight <= 0.5f) return;

            bool inCave = environment != null &&
               environment.CurrentState == EnvironmentController.EnvironmentState.Cave;

            var clips = inCave ? CaveFootstepClips : SnowFootstepClips;

            if (clips == null || clips.Length == 0) return;

            AudioManager.Instance?.PlayRandom3DSfx(
                clips,
                transform.TransformPoint(_controller.center),
                FootstepAudioVolume,
                0.95f,
                1.05f
            );
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight <= 0.5f) return;

            bool inCave = environment != null &&
               environment.CurrentState == EnvironmentController.EnvironmentState.Cave;

            var clips = inCave ? CaveFootstepClips : SnowFootstepClips;
            AudioClip clip = inCave ? CaveLandingClip : SnowLandingClip;

            if (clip == null) return;

            AudioManager.Instance?.Play3DSfx(
                clip,
                transform.TransformPoint(_controller.center),
                FootstepAudioVolume
            );
        }

        //환경에 따른 이동속도 배율 적용 함수 (EnvironmentController에서 호출)
        public void SetEnvironmentMoveMultiplier(float multiplier)
        {
            _environmentMoveMultiplier = multiplier;
        }

    }
}
