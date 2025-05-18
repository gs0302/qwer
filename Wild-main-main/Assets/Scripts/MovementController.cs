using System;
using System.Net.Sockets;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(InputReader))]
public class MovementController : MonoBehaviour
{
  [Header("Move Settings")]
  public float walkSpeed = 3f;
  public float runSpeed = 4.5f; // 달리기 속도
  public float speedSmoothTime = 0.1f; // 달리기 지속 시간
  private float speedVelocity; // 달리기 속도(smoothDamp 사용)
  public float moveSpeed;// 이동 속도
  [Header("Jump/Gravity Settings")]
  public float jumpHeight = 1.5f; // 점프 높이
  public float gravity = -9.81f;  // 중력 가속도
  public float gravityScale = 1f; // 중력 스케일

  public float mouseMoveSpeed = 5f; // 마우스 이동 속도
  private float mouseMoveHorizontal; // 마우스 이동(수평 방향)
  private bool isSprinting; // 달리기 상태
  private float verticalVelocity; // y축 속도
  private InputReader inputReader; // InputReader 컴포넌트
  private CharacterController controller;
  private Vector2 moveInput;

  public bool GetIsSprinting => isSprinting; // 달리기 상태를 외부에서 확인할 수 있도록 프로퍼티로 제공
  public Vector2 GetMoveInput => moveInput; // 외부에서 이동 입력을 확인할 수 있도록 프로퍼티로 제공

  void Awake()
  {
    controller = GetComponent<CharacterController>();
    inputReader = GetComponent<InputReader>();
    moveSpeed = walkSpeed;
  }

  void OnEnable()
  {
    // InputReader의 MovePerformed 이벤트에 OnMove 메서드를 구독합니다.
    inputReader.MovePerformed += OnMove;
    inputReader.JumpPerformed += OnJump;
    inputReader.OnSprintStateChanged += OnSprint; // 달리기 이벤트
    inputReader.MouseMovePerformed += OnMouseMove; // 마우스 이동 이벤트
  }


  void OnDisable()
  {
    // InputReader의 MovePerformed 이벤트에서 OnMove 메서드 구독을 해제합니다.
    inputReader.MovePerformed -= OnMove;
    inputReader.OnSprintStateChanged -= OnSprint;
  }

  // 마우스 이동 처리
  private void OnMouseMove(Vector2 mouseMove)
  {
    mouseMoveHorizontal = mouseMove.x; // 마우스 이동(수평 방향)
    Debug.Log(mouseMove);
  }

  private void CharacterRotation()
  {
    // 캐릭터 회전
    Vector3 rotation = new Vector3(0, mouseMoveHorizontal * mouseMoveSpeed, 0);
    transform.Rotate(rotation * Time.deltaTime); // 마우스 이동 속도에 따라 회전 속도 조절
    mouseMoveHorizontal = 0; // 마우스 이동 초기화
  }

  private void OnSprint(bool isSprinting)
  {
    this.isSprinting = isSprinting;
  }

  void OnMove(Vector2 input)
  {
    // MovePerformed 이벤트에서 전달된 Vector3 값을 moveInput에 저장합니다.

    moveInput = input;
  }

  void OnJump()
  {
    if (controller.isGrounded)
    {
      // 원하는 높이만큼 올라갔다가 딱 멈추도록 초기 속도(v₀)를 물리 공식을 통해 계산하기 위함.
      // v² = v₀² + 2a * s (v: 최종 속도, v₀: 초기 속도, a: 가속도, s: 거리)
      // v = 0 (최종 속도), a = gravity (중력), s = jumpHeight (점프 높이)
      // v₀ = √(v² - 2a * s) = √(0 - 2 * gravity * jumpHeight)
      // gravity는 음수이므로 -2 * gravity는 양수가 됨.
      // 따라서 v₀ = √(2 * jumpHeight * -gravity) = √(jumpHeight * -2 * gravity)

      /*
        v² = v₀² + 2a * s
        0 = v₀² + 2·gravity·jumpHeight  
        → v₀² = –2·gravity·jumpHeight  
        → v₀  = √(–2·gravity·jumpHeight)
      */
      verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
  }

  void Update()
  {
    if (controller.isGrounded && verticalVelocity < 0)
      verticalVelocity = -1f;

    // 공중에 있는 동안 중력 적용  
    verticalVelocity += gravity * gravityScale * Time.deltaTime;

    // 이동 속도 설정
    if (isSprinting && moveInput.y > 0)
    {
      // Mathf.SmoothDamp(moveSpeed, targetSpeed, ref speedVelocity, speedSmoothTime);
      moveSpeed = Mathf.SmoothDamp(moveSpeed, runSpeed, ref speedVelocity, speedSmoothTime);
    }
    else
    {
      // Mathf.SmoothDamp(moveSpeed, targetSpeed, ref speedVelocity, speedSmoothTime);
      moveSpeed = Mathf.SmoothDamp(moveSpeed, walkSpeed, ref speedVelocity, speedSmoothTime);
    }


    // 이동 방향
    Vector3 horizontalDir = (transform.right * moveInput.x) + (transform.forward * moveInput.y);
    Vector3 velticalDir = new Vector3(0, verticalVelocity, 0);
    Vector3 dir = horizontalDir * moveSpeed + velticalDir;

    // TODO: 달리기, 걷기 속도 설정 및 부드러운 변환
    // 이동 속도
    controller.Move(dir * Time.deltaTime);

    // 캐릭터 회전
    CharacterRotation();
  }

  public bool GetIsJumping()
  {
    return verticalVelocity > 0;
  }
}
