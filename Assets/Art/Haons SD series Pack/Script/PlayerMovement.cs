using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 4f;
    public float runSpeed = 7f;
    public float rotationSpeed = 10f;

    [Header("카메라")]
    public Transform cameraTransform;

    [Header("중력")]
    public float gravity = -9.81f;

    private CharacterController controller;
    private Animator animator;
    private float verticalVelocity;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        // 1. WASD 입력
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal += 1f;

        if (Keyboard.current.wKey.isPressed)
            vertical += 1f;

        if (Keyboard.current.sKey.isPressed)
            vertical -= 1f;

        Vector2 input = Vector2.ClampMagnitude(
            new Vector2(horizontal, vertical), 1f
        );

        // 2. 카메라 방향을 지면(XZ)에 투영
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
            right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();
        }

        // 3. 카메라 방향 기준 이동
        Vector3 direction =
            forward * input.y + right * input.x;

        direction = Vector3.ClampMagnitude(direction, 1f);

        bool isMoving = direction.sqrMagnitude > 0.001f;

        // 4. Shift 달리기
        bool isRunning = isMoving &&
            (Keyboard.current.leftShiftKey.isPressed ||
             Keyboard.current.rightShiftKey.isPressed);

        float currentSpeed =
            isRunning ? runSpeed : moveSpeed;

        // 5. 이동하는 방향으로 회전
        if (isMoving)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // 6. 중력
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        // 7. Character Controller 이동
        Vector3 velocity = direction * currentSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        // 8. 애니메이션
        if (animator != null)
        {
            float speedValue = 0f;

            if (isRunning)
                speedValue = 2f;
            else if (isMoving)
                speedValue = 1f;

            animator.SetFloat(SpeedHash, speedValue);
        }
    }
}