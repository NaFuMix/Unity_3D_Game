
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerId = 1;

    [Header("References")]
    public Animator animator;

    [Header("Controls")]
    public Key forwardKey = Key.W;
    public Key backKey = Key.S;
    public Key leftKey = Key.A;
    public Key rightKey = Key.D;
    public Key jumpKey = Key.Space;
    public Key runKey = Key.LeftShift;

    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 5f;
    public float rotationSpeed = 12f;

    [Header("Jump")]
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    private CharacterController cc;
    private float verticalVelocity;

    private int speedHash;
    private int jumpHash;
    private int collectHash;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        speedHash = Animator.StringToHash("Speed");
        jumpHash = Animator.StringToHash("Jump");
        collectHash = Animator.StringToHash("Collect");
    }

    public void ResetVerticalVelocity()
    {
        verticalVelocity = 0f;
        UpdateAnimationSpeed(0f);
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
        {
            UpdateAnimationSpeed(0f);
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        MovePlayer(keyboard);
    }

    void MovePlayer(Keyboard keyboard)
    {
        // อ่านปุ่มควบคุม
        Vector2 input = Vector2.zero;

        if (keyboard[forwardKey].isPressed)
            input.y += 1f;

        if (keyboard[backKey].isPressed)
            input.y -= 1f;

        if (keyboard[leftKey].isPressed)
            input.x -= 1f;

        if (keyboard[rightKey].isPressed)
            input.x += 1f;

        input = Vector2.ClampMagnitude(input, 1f);

        // ทิศทางการเดินใน World Space
        Vector3 direction = new Vector3(
            input.x,
            0f,
            input.y
        );

        // เลือกความเร็วเดินหรือวิ่ง
        bool isRunning =
            keyboard[runKey].isPressed &&
            direction.sqrMagnitude > 0.01f;

        float currentSpeed =
            isRunning ? runSpeed : walkSpeed;

        Vector3 horizontalVelocity =
            direction * currentSpeed;

        // หันหน้าไปตามทิศทางที่กำลังเดิน
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // กระโดดและแรงโน้มถ่วง
        if (cc.isGrounded)
        {
            verticalVelocity = -1f;

            if (keyboard[jumpKey].wasPressedThisFrame)
            {
                verticalVelocity = Mathf.Sqrt(
                    jumpHeight * -2f * gravity
                );

                if (HasParameter(jumpHash))
                    animator.SetTrigger(jumpHash);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity =
            horizontalVelocity +
            Vector3.up * verticalVelocity;

        cc.Move(velocity * Time.deltaTime);

        // ส่งความเร็วไปยัง Blend Tree
        UpdateAnimationSpeed(horizontalVelocity.magnitude);
    }

    void UpdateAnimationSpeed(float speed)
    {
        if (animator != null && HasParameter(speedHash))
        {
            animator.SetFloat(
                speedHash,
                speed,
                0.1f,
                Time.deltaTime
            );
        }
    }

    // ใช้เรียก Animation ตอนเก็บ Gem
    public void PlayCollect()
    {
        if (animator != null && HasParameter(collectHash))
        {
            animator.SetTrigger(collectHash);
        }
    }

    bool HasParameter(int parameterHash)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter parameter
            in animator.parameters)
        {
            if (parameter.nameHash == parameterHash)
                return true;
        }

        return false;
    }
}
