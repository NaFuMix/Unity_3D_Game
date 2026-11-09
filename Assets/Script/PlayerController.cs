
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Controls")]
    public Key forwardKey;
    public Key backwardKey;
    public Key leftKey;
    public Key rightKey;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
        {
            return;
        }

        Vector3 direction = Vector3.zero;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null) return;

        if (keyboard[forwardKey].isPressed)
            direction.z += 1;

        if (keyboard[backwardKey].isPressed)
            direction.z -= 1;

        if (keyboard[leftKey].isPressed)
            direction.x -= 1;

        if (keyboard[rightKey].isPressed)
            direction.x += 1;

        direction = Vector3.ClampMagnitude(direction, 1f);

        controller.Move(
            direction * moveSpeed * Time.deltaTime
        );
    }
}
