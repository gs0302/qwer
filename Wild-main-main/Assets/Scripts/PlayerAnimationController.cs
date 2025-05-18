using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
  private Animator animator;
  private MovementController movementController;

  void Start()
  {
    animator = GetComponent<Animator>();
    movementController = GetComponent<MovementController>();
  }

  void Update()
  {
    // 플레이어의 이동 입력을 가져옵니다.
    Vector2 moveInput = movementController.GetMoveInput;
    bool isSprinting = movementController.GetIsSprinting;

    if (moveInput.y > 0 && isSprinting)
    {
      moveInput.y *= 2;
    }
    PlayMoveAnimation(moveInput);
  }

  public void PlayMoveAnimation(Vector2 moveInput)
  {
    animator.SetFloat("Horizontal", moveInput.x, 0.1f, Time.deltaTime);
    animator.SetFloat("Vertical", moveInput.y, 0.1f, Time.deltaTime);
  }
}
