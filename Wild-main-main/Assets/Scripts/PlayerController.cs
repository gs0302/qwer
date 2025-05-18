using System;
using System.Collections;
using UnityEngine;


// Rigidbody 컴포넌트를 항시 추가
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // 컴포넌트 시작
    [Header("컴포넌트")]

    [SerializeField]
    private Camera vision;                  // 눈
    [SerializeField]
    private CapsuleCollider capsuleCollider;// 콜라이더

    private Rigidbody rigid;                // 강체

    // 컴포넌트 종료


    [Space(10f)]

    // 실수 변수 시작
    [Header("실수 변수")]
    [SerializeField]
    private float walkSpeed = 5f;          // 걷는 속도
    [SerializeField]
    private float runSpeed = 7f;           // 뛰는 속도
    [SerializeField]
    private float crouchSpeed = 3f;         // 앉은 자세 속도
    [SerializeField]
    private float jumpForce = 4f;           // 각력
    [SerializeField]
    [Range(0.01f, 10f)]                     // 0.01f ~ 10f까지 제한조건
    private float lookSensitivity = 2.5f;          // 마우스 감도
    [SerializeField]
    private float cameraRotationLimit = 70f;      // 시야 상/하한 각도
    private float currentCameraRotationX = 0f;  // 카메라 상하한 회전값

    private float curSpeed;                 // 이동 속도

    [SerializeField]
    private float crouchPosY;               // 앉았을 때 얼마나 낮게 앉을지 정함
    private float originPosY;               // 앉기 전의 위치
    private float applyCrouchPosY;          // 현재 앉은자세 높이

    private float _moveDirX;                // X축 입력 
    private float _moveDirZ;                // Z축 입력
    private float _xRotation;               // 카메라 X축 회전 입력값
    private float _yRotation;               // 캐릭터 Y축 회전 입력값

    // 실수 변수 종료

    [Space(10f)]
    [Header("상태 변수")]
    [SerializeField]
    private bool isRun = false;
    [SerializeField]
    private bool isGround = true;
    [SerializeField]
    private bool isCrouch = false;
    private bool isJump = false;


    // 최초 프레임 호출 전 실행됨.(초기화)
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        curSpeed = walkSpeed;
        originPosY = vision.transform.localPosition.y;
        applyCrouchPosY = originPosY;
        vision.fieldOfView = 80f;
    }


    // 매 프레임 마다 호출됨.
    void Update()
    {
        InputManage();
        IsGround();
        TryJump();
        TryCrouch();
        ControlCurrentSpeed();

    }

    void FixedUpdate()
    {
        Move();
        CharacterRotation();
        CameraRotation();

    }

    void InputManage()
    {
        // LeftCtrl 누르고 있는 동안 isCrouch = true 아니면 false
        isCrouch = Input.GetKey(KeyCode.LeftControl);
        isJump = Input.GetKeyDown(KeyCode.Space);
        isRun = Input.GetKey(KeyCode.LeftShift);
        // X축 입력
        _moveDirX = Input.GetAxisRaw("Horizontal");
        // Z축 입력
        _moveDirZ = Input.GetAxisRaw("Vertical");
        // 마우스 상하 입력
        _xRotation = Input.GetAxisRaw("Mouse Y");
        // 마우스 좌우 입력
        _yRotation = Input.GetAxisRaw("Mouse X");
    }


    // 부드럽게 앉기를 위한 Coroutine
    IEnumerator CrouchCoroutine()
    {
        float _posY = vision.transform.localPosition.y;

        // 시점 변환을 부드럽게
        while (_posY != applyCrouchPosY)
        {
            // 선형 보간
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.05f);
            vision.transform.localPosition =
            new Vector3(
                vision.transform.localPosition.x,
                _posY,
                vision.transform.localPosition.z);
            yield return null;
        }

        vision.transform.localPosition =
            new Vector3(
                vision.transform.localPosition.x,
                applyCrouchPosY,
                vision.transform.localPosition.z);
    }

    /// <summary>
    /// 지면에 서있는지 판별
    /// </summary>
    void IsGround()
    {
        // position에서 Vector3.down방향으로 collider의 y길이 절반 + .1f만큼 레이캐스팅
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + .1f);
        // Debug.DrawRay(transform.position, Vector3.down, Color.red, capsuleCollider.bounds.extents.y + .1f);
    }

    /// <summary>
    /// 점프 수행
    /// </summary>
    void TryJump()
    {
        // 공중에 있으면 취소
        if (!isGround) return;

        // 스페이스바를 누르면 jumpForce만큼 점프
        if (isJump)
        {
            rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            // curSpeed = walkSpeed;
        }
    }

    /// <summary>
    /// 웅크리기 수행
    /// </summary>
    void TryCrouch()
    {

        // 웅크리고 있는 동안 웅크리기 속도 적용, 시점 변환
        if (isCrouch)
        {
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applyCrouchPosY = originPosY;
        }

        StartCoroutine("CrouchCoroutine");
    }



    /// <summary>
    /// 전후 좌우 이동 수행
    /// </summary>
    void Move()
    {


        // (1,0,0) * hor
        Vector3 _moveHorizontal = transform.right * _moveDirX;
        // (0,0,1) * ver
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        // 단위벡터로 변환 후 속도를 곱한다.
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * curSpeed;

        // 현재 위치 + (이동 방향 * 이동할 거리) * 프레임 간격시간
        rigid.MovePosition(transform.position + _velocity * Time.fixedDeltaTime);

    }

    /// <summary>
    /// 플레이어 속도 조절
    /// </summary>
    void ControlCurrentSpeed()
    {
        if (isCrouch && isRun)
        {
            curSpeed = crouchSpeed;
            return;
        }

        if (isRun)
        {
            curSpeed = runSpeed;
            return;
        }

        if (isCrouch)
        {
            curSpeed = crouchSpeed;
        }

        curSpeed = walkSpeed;
    }

    /// <summary>
    /// 카메라 상하 회전
    /// </summary>
    void CameraRotation()
    {
        // 마우스 상하 입력

        float _cameraRotationX = _xRotation * lookSensitivity;

        // X축을 기준으로 회전하면 카메라가 상하로 바라봄.
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Math.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        // 카메라 transform의 local앵글을 바라보고자 하는 방향으로 바꿈꿈
        vision.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0, 0);
    }

    /// <summary>
    /// 캐릭터 좌우 회전
    /// </summary>
    void CharacterRotation()
    {

        // 입력값 * 감도
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        // 현재 rotation +
        rigid.MoveRotation(rigid.rotation * Quaternion.Euler(_characterRotationY));
    }
}
