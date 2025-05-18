using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputReader : MonoBehaviour
{
  [Header("Input Actions Asset")]
  public InputActionAsset inputActions;  // Inspector에서 할당

  // InputAction 변수
  private InputAction moveAction;
  private InputAction jumpAction;
  private InputAction sprintAction;
  private InputAction mouseMoveAction; // 마우스 이동

  // 외부 구독용 이벤트(발행자 설정)
  public event Action<Vector2> MovePerformed;
  public event Action JumpPerformed;
  public event Action<bool> OnSprintStateChanged; // 달리기 이벤트 구독
  public event Action<Vector2> MouseMovePerformed; // 마우스 이동 이벤트

  void Awake()
  {
    // InputAction에서 Gameplay 액션 맵과 Move, Jump 액션을 찾습니다.
    var gameplay = inputActions.FindActionMap("Gameplay");
    moveAction = gameplay.FindAction("Move");
    jumpAction = gameplay.FindAction("Jump");
    sprintAction = gameplay.FindAction("Sprint");
    mouseMoveAction = gameplay.FindAction("MouseMove"); // 마우스 이동
  }

  void OnEnable()
  {
    // moveAction과 jumpAction을 활성화합니다.
    moveAction.Enable();    // 이동
    jumpAction.Enable();    // 점프
    sprintAction.Enable();  // 달리기 액션 활성화
    mouseMoveAction.Enable(); // 마우스 이동 액션 활성화

    // 이벤트 구독 설정
    // moveAction이 실행되면 MovePerformed 이벤트를 호출합니다.
    // moveAction이 취소되면 MovePerformed 이벤트를 호출합니다.
    moveAction.performed += OnMovePerformed;
    moveAction.canceled += OnMoveCanceled;

    // jumpAction이 실행되면 JumpPerformed 이벤트를 호출합니다.
    // jumpAction은 Vector3를 사용하지 않으므로 _로 무시합니다.(실행 확인 여부만 하면 됨)
    jumpAction.performed += OnJumpPerformed;

    sprintAction.performed += OnSprint; // 달리기 이벤트
    sprintAction.canceled += OnSprint; // 달리기 이벤트

    mouseMoveAction.performed += OnMouseMovePerformed; // 마우스 이동 이벤트
    // mouseMoveAction.performed += OnMouseMoveCanceled; // 마우스 이동 이벤트
  }

  void OnDisable()
  {
    moveAction.performed -= OnMovePerformed;
    moveAction.canceled -= OnMoveCanceled;
    jumpAction.performed -= OnJumpPerformed;
    sprintAction.performed -= OnSprint; // 달리기 이벤트
    sprintAction.canceled -= OnSprint;
    mouseMoveAction.performed -= OnMouseMovePerformed; // 마우스 이동 이벤트
    // mouseMoveAction.performed -= OnMouseMoveCanceled; // 마우스 이동 이벤트

    inputActions.Disable();
  }

  private void OnMouseMovePerformed(InputAction.CallbackContext context)
  {
    if (context.performed)
    {
      // 마우스 이동 이벤트를 호출합니다.
      MouseMovePerformed?.Invoke(context.ReadValue<Vector2>());
    }
  }

  private void OnMouseMoveCanceled(InputAction.CallbackContext context)
  {
    // 마우스 이동 이벤트를 호출합니다.
    MouseMovePerformed?.Invoke(Vector2.zero);
  }

  private void OnSprint(InputAction.CallbackContext context)
  {
    if (context.performed)
      OnSprintStateChanged?.Invoke(true);  // true: 버튼 누름

    else if (context.canceled)
      OnSprintStateChanged?.Invoke(false); // false: 버튼 뗌
  }

  private void OnMovePerformed(InputAction.CallbackContext context)
  {
    // MovePerformed 이벤트를 호출합니다.
    MovePerformed?.Invoke(context.ReadValue<Vector2>());
  }

  private void OnMoveCanceled(InputAction.CallbackContext context)
  {
    // MovePerformed 이벤트를 호출합니다.
    MovePerformed?.Invoke(Vector2.zero);
  }

  private void OnJumpPerformed(InputAction.CallbackContext context)
  {
    // JumpPerformed 이벤트를 호출합니다.
    JumpPerformed?.Invoke();
  }
}
