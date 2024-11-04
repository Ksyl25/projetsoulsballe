 using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using MyBullet01;

#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
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

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
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

        private float aimVelocityX;
        private float aimVelocityZ;

        public bool Etatguns = false;
        private bool buttonPressed = false; // Nouvelle variable pour suivre l'appui du bouton

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;
        public GameObject playerFollowCamera;
        public GameObject AimingCamera;
        public GameObject weapon01;
        public GameObject weapon02;
        public GameObject muzzleFlashPrefab;
        public GameObject bulletTrailPrefab;
        public AudioSource fireSound;
        public ParticleSystem fire1;
        public ParticleSystem fire2;

        // Le prefab du projectile (à assigner depuis l'éditeur)
        public GameObject bulletPrefab;
        // Le point de tir sur l'arme (à assigner depuis l'éditeur)
        public Transform firingPoint01;
        public Transform firingPoint02;
        // La vitesse du projectile (peut être ajustée dans l'inspecteur)
        public float projectileSpeed = 20f;

        public float fireRate = 0.22f;  // Environ 2 tirs par seconde

        private float nextFireTime = 0f;

        public GameObject reticleCanvas;

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
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            Desactiveguns();
            reticleCanvas.SetActive(false);
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
            Aimshoot();
            hasgunss();
            GunsManager();
        }

        private void Aimshoot()
        {
            if (_input.isAiming && !_input.sprint && !_input.jump)
            {
                //_input.ishasguns = true;
                //hasgunss();
                //lance une animetion 
                //Debug.Log("lancement animation tir ");
                //Activeguns();

                //passage en mode vise 
                playerFollowCamera.SetActive(false);
                AimingCamera.SetActive(true);
                reticleCanvas.SetActive(true);
                

                _animator.SetBool("AimShoot", _input.isAiming);
                //tir
                if (Time.time > nextFireTime)
                {
                    // Définir le moment du prochain tir
                    nextFireTime = Time.time + fireRate;
                    Shoot();
                }
                

                //point de vue perso
                Vector3 cameraForward = _mainCamera.transform.forward;
                Vector3 cameraForwardOnXZ = Vector3.ProjectOnPlane(cameraForward, Vector3.up);

                // Change l'orientation du personnage pour qu'il suive la direction de la caméra
                transform.rotation = Quaternion.LookRotation(cameraForwardOnXZ, Vector3.up);

                // deplacement perso 
                // Récupération des valeurs d'input pour le mouvement en mode visée
                Vector2 inputValue = _input.move;
                float inputValueX = Mathf.SmoothDamp(_animator.GetFloat("SpeedAimX"), inputValue.x, ref aimVelocityX, 0.1f);
                float inputValueZ = Mathf.SmoothDamp(_animator.GetFloat("SpeedAimZ"), inputValue.y, ref aimVelocityZ, 0.1f);

                // Mise à jour des paramètres de mouvement en mode visée dans l'Animator
                _animator.SetFloat("SpeedAimX", inputValueX);
                _animator.SetFloat("SpeedAimZ", inputValueZ);

            }
            else
            {
                playerFollowCamera.SetActive(true);
                AimingCamera.SetActive(false);
                reticleCanvas.SetActive(false);
                fire1.Stop();
                fire2.Stop();
                //arrete l'animation 
                //Debug.Log("arret animation"); //
                _animator.SetBool("AimShoot", false);
                //Debug.Log("arret animation");
                //Desactiveguns();
            }
            
        }
        private void GunsManager()
        {
            if (Etatguns || _input.isAiming)
            {
                Activeguns();
            }
        }

        private void Shoot()
        {
            // Crée un rayon à partir du centre de l'écran vers l'avant
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            Vector3 targetPoint;
            float dureeDebugRay = 2.0f;
            float distanceMax = 100f;

            // Si le rayon touche un objet, fixe la cible à ce point
            if (Physics.Raycast(ray, out hit))
            {
                targetPoint = hit.point; // Cible l’endroit d’impact
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, dureeDebugRay);
                Debug.DrawRay(hit.point, Vector3.up * 0.5f, Color.yellow, dureeDebugRay);

            }
            else
            {
                targetPoint = ray.GetPoint(100); // Sinon, cible un point distant
                //targetPoint = Camera.main.transform.forward;
                Debug.DrawRay(ray.origin, ray.direction * distanceMax, Color.blue, dureeDebugRay);
            }


            // Instancier le projectile au niveau du firingPoint avec la rotation du point de tir
            GameObject projectile01 = Instantiate(bulletPrefab, firingPoint01.position, Quaternion.identity);
            GameObject projectile02 = Instantiate(bulletPrefab, firingPoint02.position, Quaternion.identity);

            // Récupérer le composant Rigidbody du projectile

            Bullet bulletScript01 = projectile01.GetComponent<Bullet>();
            Bullet bulletScript02 = projectile02.GetComponent<Bullet>();

            
            if (bulletScript01 != null && bulletScript02 !=null )
            {
                bulletScript01.Initialize(targetPoint); // Méthode que vous aurez définie pour l'initialisation
                bulletScript02.Initialize(targetPoint);
            }
            else
            {
                Debug.LogWarning("BulletScript n'a pas été trouvé sur le projectile !");
                //Instantiate(muzzleFlashPrefab, transform.position, transform.rotation);
            }
            fireSound.Play();
            fire1.Play();
            fire2.Play();



        }

        private void hasgunss()
        {
            if (_input.ishasguns)
            {
                //Debug.Log("debug hasguns");
                //Debug.Log(_input.ishasguns);

                if (!Etatguns) // Si l'arme est actuellement rangée, on la sort
                {
                    Etatguns = true; // Met à jour l'état de l'arme
                                     //Debug.Log("Arme prise");

                    // Activez l'objet arme
                    //Activeguns();

                    _animator.SetBool("Aiming", _input.ishasguns); // Lance l'animation pour prendre l'arme
                }
                else  // Si l'arme est déjà sortie, on la range
                {
                    
                    Etatguns = false; // Met à jour l'état de l'arme
                                      //Debug.Log("Arme rangée");
                    _animator.SetBool("Aiming", false); // Lance l'animation pour ranger l'arme

                    // Désactivez l'objet arme
                   //Desactiveguns();

                }


                _input.ishasguns = false;
            }
        }
        public void Activeguns()
        {
            if (weapon01 != null)
            {
                weapon01.SetActive(true);
            }
            if (weapon02 != null)
            {
                weapon02.SetActive(true);
            }

        }
        public void Desactiveguns()
        {
            if (weapon01 != null)
            {
                weapon01.SetActive(false);
            }
            if (weapon02 != null)
            {
                weapon02.SetActive(false);
            }

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
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
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

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}