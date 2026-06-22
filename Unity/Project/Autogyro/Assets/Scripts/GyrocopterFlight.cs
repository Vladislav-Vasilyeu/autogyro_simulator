using UnityEngine;

public class GyrocopterFlight : MonoBehaviour
{
    [Header("Ссылки на компоненты")]
    public AviationSystems aviationSystems;
    public Transform mainRotor;
    public Transform pusherPropeller;
    public Transform rudderKiel;

    [Header("Звуковое сопровождение двигателя")]
    [Tooltip("Компонент AudioSource для однократного звука запуска")]
    public AudioSource startAudioSource;
    [Tooltip("Компонент AudioSource для циклического гула работы")]
    public AudioSource loopAudioSource;
    [Tooltip("Аудиоклип «Запуск мотора» (1-3 секунды)")]
    public AudioClip engineStartClip;
    [Tooltip("Аудиоклип «Зацикленный гул работы мотора»")]
    public AudioClip engineLoopClip;

    [Header("Лётная аэродинамика (Твои кастомные оси)")]
    [Tooltip("Максимальная тяга маршевого винта (вдоль красной оси X прямо)")]
    public float maxEngineThrust = 5800f; 
    [Tooltip("Эффективность подъемной силы ротора (вверх по зеленой оси Y)")]
    public float liftCoefficient = 3.5f;
    [Tooltip("Сопротивление воздуха фюзеляжа")]
    public float airDrag = 0.28f;

    [Header("Настройки чувствительности (РУС и Педали)")]
    public float pitchSensitivity = 4.0f;   // Тангаж (мышь вверх/вниз)
    public float rollSensitivity = 4.5f;    // Крен (мышь влево/вправо)
    public float yawSensitivity = 5.0f;     // Руль направления (A/D)

    [Header("Пределы механического наклона ротора (Градусы)")]
    [Tooltip("Максимальный угол наклона втулки ротора назад/вперед")]
    public float maxRotorTiltPitch = 12f;
    [Tooltip("Максимальный угол наклона втулки ротора влево/вправо")]
    public float maxRotorTiltRoll = 9f;

    [Header("Телеметрия состояния")]
    public float forwardSpeed = 0f;
    public float rotorRPM = 0f;
    [Range(0f, 1f)] public float throttle = 0f;
    public bool isParkingBrakeOn = true;

    
    private float currentRotorTiltPitch = 0f;
    private float currentRotorTiltRoll = 0f;
    private float currentRotorVisualAngle = 0f;

    private Rigidbody rb;
    private bool isGrounded = true;

    private bool wasEngineRunningLastFrame = false;
    private bool starterFinished = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 450f;
        rb.useGravity = true;
        rb.isKinematic = false;

       
        rb.linearDamping = 0.02f;
        rb.angularDamping = 2.0f;

        isParkingBrakeOn = true;

        if (aviationSystems != null)
        {
            wasEngineRunningLastFrame = aviationSystems.isEngineRunning;
        }
    }

    void Update()
    {
        
        isGrounded = Physics.Raycast(transform.position, -transform.up, 1.65f);

       
        if (Input.GetKeyDown(KeyCode.B) && isGrounded)
        {
            isParkingBrakeOn = !isParkingBrakeOn;
        }

        HandleRecovery();
        HandleEngineAudio();

        if (aviationSystems == null || !aviationSystems.isEngineRunning)
        {
            throttle = 0f;
            rotorRPM = Mathf.MoveTowards(rotorRPM, 0f, Time.deltaTime * 15f);
            RotateVisualVanes();
            return;
        }

        
        if (!Input.GetKey(KeyCode.LeftAlt))
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                throttle += scroll * 2.0f;
                throttle = Mathf.Clamp01(throttle);
            }
        }

    
        
        float currentSidewaysSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float totalHorizontalSpeed = Mathf.Sqrt(forwardSpeed * forwardSpeed + currentSidewaysSpeed * currentSidewaysSpeed);

        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            
            float enginePreSpinDest = Mathf.Lerp(0f, 290f, aviationSystems.engineRPM / aviationSystems.maxRPM);
            rotorRPM = Mathf.MoveTowards(rotorRPM, enginePreSpinDest, Time.deltaTime * 70f);
        }
        else
        {
            float verticalAirflow = Vector3.Dot(rb.linearVelocity, transform.up);

            
            float tiltFactor = Mathf.Clamp01(currentRotorTiltPitch / maxRotorTiltPitch);
            float autorotationDrive = (totalHorizontalSpeed * 9.8f) * (1.0f + tiltFactor * 0.5f) - (verticalAirflow * 4.5f);

            float targetRotorRPM = Mathf.Clamp(autorotationDrive, 0f, 450f);

            
            if (!isGrounded && totalHorizontalSpeed > 12f && targetRotorRPM < 200f)
            {
                targetRotorRPM = 210f;
            }

            
            rotorRPM = Mathf.MoveTowards(rotorRPM, targetRotorRPM, Time.deltaTime * 16f);
        }

        RotateVisualVanes();
    }

    void FixedUpdate()
    {
        rb.WakeUp();

        
        forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.right);
        if (forwardSpeed < 0f) forwardSpeed = 0f;

        
        float sidewaysSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float totalHorizontalSpeed = Mathf.Sqrt(forwardSpeed * forwardSpeed + sidewaysSpeed * sidewaysSpeed);

        
        float rotorDragModifier = 1.0f + (currentRotorTiltPitch > 0 ? (currentRotorTiltPitch / maxRotorTiltPitch) * 1.2f : 0f);
        float currentAirDrag = isGrounded ? Mathf.Lerp(0.01f, airDrag, forwardSpeed / 14f) : airDrag * rotorDragModifier;
        float dragForceValue = 0.5f * currentAirDrag * forwardSpeed * forwardSpeed;
        rb.AddForce(-transform.right * dragForceValue, ForceMode.Force);

        
        float maxSpeedMPS = 44.4f;
        if (forwardSpeed > maxSpeedMPS)
        {
            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            localVelocity.x = maxSpeedMPS;
            rb.linearVelocity = transform.TransformDirection(localVelocity);
            forwardSpeed = maxSpeedMPS;
        }

        
        if (isParkingBrakeOn && isGrounded)
        {
            rb.AddForce(-rb.linearVelocity * 35f, ForceMode.Acceleration);
        }

        
        float pitchInput = 0f;
        float rollInput = 0f;
        float yawInput = 0f;

        if (!Input.GetKey(KeyCode.LeftAlt))
        {
            
            if (Input.GetMouseButton(0))
            {
                pitchInput = Input.GetAxis("Mouse Y"); 
                rollInput = Input.GetAxis("Mouse X");  
            }

            if (Input.GetKey(KeyCode.A)) yawInput = -1f;
            if (Input.GetKey(KeyCode.D)) yawInput = 1f;
        }

       
        currentRotorTiltPitch = Mathf.MoveTowards(currentRotorTiltPitch, pitchInput * maxRotorTiltPitch, Time.fixedDeltaTime * 22f);
        currentRotorTiltRoll = Mathf.MoveTowards(currentRotorTiltRoll, rollInput * maxRotorTiltRoll, Time.fixedDeltaTime * 22f);

        Vector3 rotorNormal = transform.up;
        rotorNormal = Quaternion.AngleAxis(currentRotorTiltPitch, transform.forward) * rotorNormal;
        rotorNormal = Quaternion.AngleAxis(-currentRotorTiltRoll, transform.right) * rotorNormal;

       
        if (rotorRPM > 100f)
        {
            
            float speedFactor = Mathf.Clamp(totalHorizontalSpeed / 10f, 0.15f, 2.4f);
            float lift = (rotorRPM * rotorRPM) * liftCoefficient * 0.0055f * speedFactor;

            float groundDistance = transform.position.y;
            if (groundDistance < 5.0f)
            {
                lift *= Mathf.Lerp(1.25f, 1.0f, groundDistance / 5.0f);
            }

            lift = Mathf.Clamp(lift, 0f, rb.mass * 9.81f * 3.5f);
            rb.AddForce(rotorNormal * lift, ForceMode.Force);

            float pendulumTorquePitch = currentRotorTiltPitch * pitchSensitivity * 70f * (rotorRPM / 300f);
            float pendulumTorqueRoll = currentRotorTiltRoll * rollSensitivity * 70f * (rotorRPM / 300f);

            rb.AddTorque(transform.forward * -pendulumTorquePitch, ForceMode.Force);
            rb.AddTorque(transform.right * -pendulumTorqueRoll, ForceMode.Force);
        }

        
        if (aviationSystems != null && aviationSystems.isEngineRunning)
        {
            float engineRPMPerc = aviationSystems.engineRPM / aviationSystems.maxRPM;

            
            float thrustModifier = Mathf.Lerp(0.4f, 1.0f, engineRPMPerc);
            float currentThrust = throttle * maxEngineThrust * thrustModifier;

            
            if (isGrounded && throttle > 0.02f && !isParkingBrakeOn)
            {
                float targetTaxiSpeed = throttle * 5.5f; 
                if (forwardSpeed < targetTaxiSpeed)
                {
                    
                    currentThrust += (targetTaxiSpeed - forwardSpeed) * 950f * throttle;
                }
            }

            rb.AddForce(transform.right * currentThrust, ForceMode.Force);
        }

       
        if (isGrounded)
        {
            if (yawInput != 0)
            {
                float groundTurnModifier = Mathf.Clamp(forwardSpeed / 2.5f, 0.4f, 1.5f);
                rb.AddTorque(transform.up * yawInput * yawSensitivity * 2500f * groundTurnModifier, ForceMode.Force);
            }

            if (yawInput == 0)
            {
                Vector3 localAngularVel = transform.InverseTransformDirection(rb.angularVelocity);
                localAngularVel.y = 0f;
                rb.angularVelocity = transform.TransformDirection(localAngularVel);
            }

            Vector3 predictedUp = Quaternion.AngleAxis(rb.angularVelocity.magnitude * Mathf.Rad2Deg * 0.1f / 2f, rb.angularVelocity) * transform.up;
            Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
            rb.AddTorque(torqueVector * 1000f);
        }
        else
        {
            float tailEffectiveness = Mathf.Clamp01(forwardSpeed / 14f);

            rb.AddTorque(transform.up * -sidewaysSpeed * 22f * tailEffectiveness, ForceMode.Force);

            float pitchVelocity = Vector3.Dot(rb.angularVelocity, transform.forward);
            rb.AddTorque(transform.forward * -pitchVelocity * 500f * tailEffectiveness, ForceMode.Force);

            float rollVelocity = Vector3.Dot(rb.angularVelocity, transform.right);
            rb.AddTorque(transform.right * -rollVelocity * 250f * tailEffectiveness, ForceMode.Force);

            rb.AddTorque(transform.up * yawInput * yawSensitivity * 480f * (tailEffectiveness + 0.25f), ForceMode.Force);

            
            if (forwardSpeed > 2f)
            {
                Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
                float horizontalMag = Mathf.Sqrt(localVel.x * localVel.x + localVel.z * localVel.z);

                
                localVel.z = Mathf.Lerp(localVel.z, 0f, Time.fixedDeltaTime * 3.0f);
                localVel.x = Mathf.Sign(localVel.x) * Mathf.Sqrt(Mathf.Max(0f, horizontalMag * horizontalMag - localVel.z * localVel.z));

                rb.linearVelocity = transform.TransformDirection(localVel);
            }
        }

        
        if (Mathf.Abs(rollInput) < 0.01f && Mathf.Abs(yawInput) < 0.01f && forwardSpeed > 2f)
        {
            float trimEffect = Mathf.Clamp01(forwardSpeed / 15f);
            rb.AddTorque(transform.up * 35f * trimEffect, ForceMode.Force); 
        }

        if (rudderKiel != null)
        {
            float targetRudderAngle = yawInput * 28f;
            rudderKiel.localRotation = Quaternion.Slerp(rudderKiel.localRotation, Quaternion.Euler(0f, targetRudderAngle, 0f), Time.fixedDeltaTime * 10f);
        }
    }

    void HandleEngineAudio()
    {
        if (aviationSystems == null) return;

        bool isRunning = aviationSystems.isEngineRunning;

        if (isRunning)
        {
            if (!wasEngineRunningLastFrame)
            {
                starterFinished = false;

                if (startAudioSource != null && engineStartClip != null)
                {
                    startAudioSource.clip = engineStartClip;
                    startAudioSource.loop = false;
                    startAudioSource.Play();
                }
                else
                {
                    starterFinished = true;
                }
            }

            if (!starterFinished)
            {
                if (startAudioSource == null || !startAudioSource.isPlaying)
                {
                    starterFinished = true;
                }
            }

            if (starterFinished)
            {
                if (loopAudioSource != null && engineLoopClip != null && !loopAudioSource.isPlaying)
                {
                    loopAudioSource.clip = engineLoopClip;
                    loopAudioSource.loop = true;
                    loopAudioSource.Play();
                }
            }

            if (loopAudioSource != null && loopAudioSource.isPlaying)
            {
                float engineRPMPerc = aviationSystems.engineRPM / aviationSystems.maxRPM;

                loopAudioSource.pitch = Mathf.Lerp(0.65f, 1.35f, engineRPMPerc);
                loopAudioSource.volume = Mathf.Lerp(0.5f, 1.0f, engineRPMPerc);
            }
        }
        else
        {
            if (wasEngineRunningLastFrame)
            {
                if (startAudioSource != null) startAudioSource.Stop();
                if (loopAudioSource != null) loopAudioSource.Stop();
                starterFinished = false;
            }
        }

        wasEngineRunningLastFrame = isRunning;
    }

    

    /// <summary>
    /// Восстановление положения машины (Эвакуация / Переворот) на клавишу R
    /// </summary>
    void HandleRecovery()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

            transform.position += Vector3.up * 2.0f;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            
            isParkingBrakeOn = true;
        }
    }

    void RotateVisualVanes()
    {
        currentRotorVisualAngle += rotorRPM * 6f * Time.deltaTime;
        if (currentRotorVisualAngle >= 360f) currentRotorVisualAngle -= 360f;

        float deltaAngleProp = (aviationSystems != null) ? aviationSystems.engineRPM * 6f * Time.deltaTime : 0f;

        if (mainRotor != null)
        {
            Quaternion tiltRotation = Quaternion.AngleAxis(currentRotorTiltPitch, Vector3.forward) * Quaternion.AngleAxis(-currentRotorTiltRoll, Vector3.right);
            mainRotor.localRotation = tiltRotation * Quaternion.Euler(0f, currentRotorVisualAngle, 0f);
        }

        if (pusherPropeller != null && aviationSystems != null && aviationSystems.isEngineRunning)
        {
            pusherPropeller.Rotate(0f, 0f, deltaAngleProp, Space.Self);
        }
    }

    public float GetAltitude() => transform.position.y;
}