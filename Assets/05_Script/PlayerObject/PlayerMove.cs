using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] float _moveSpeed;
    [SerializeField] float _jumpForce;
    [SerializeField] float _walkBaseAnimationSpeed;
    [SerializeField] float _runBaseAnimationSpeed;

    [SerializeField] Animator _animator;
    [SerializeField] Rigidbody _rigidBody;

    public bool _canMove;
    public bool _moving;
    public bool _onGround;

    public bool MoveMode;
    public Vector3 MoveDirection;

    //トータル画面長押し時間、距離
    private bool _isTouch = false;
    private float _splitTimer = 0f;
    private Vector2 _moveToTouch = Vector2.zero;
    private Collider[] _interactColliders = new Collider[10];

    private InputSystem_Actions _inputSystem;
    private InGameManager _inGameManager;

    private void Start()
    {
        MoveMode = false;
        _onGround = true;
        _canMove = true;
        _inputSystem = new InputSystem_Actions();
        _inputSystem.Player.Move.performed += OnMove;
        _inputSystem.Player.Move.canceled += CanselMove;
        _inputSystem.Enable();

        _inGameManager = FindAnyObjectByType<InGameManager>();
        
        //アニメーション関連初期化
        var pam = FindAnyObjectByType<PlayerAnimationManager>();
        var pom = FindAnyObjectByType<PlayerOperationManager>();
        //アニメーションを設定
        if (gameObject.CompareTag("MainPlayer"))
        {
            pam._mainPlayerAnimator = _animator;
        }
        else
        {
            pam._subPlayerAnimator = _animator;
        }
    }

    private void Update()
    {
        if (_moving && _onGround && _canMove)
        {
            _rigidBody.linearVelocity = new Vector3(MoveDirection.x, _rigidBody.linearVelocity.y, MoveDirection.z);
        }
    }
    
    void Interacted()
    {
        _canMove = true;
    }

    private void CanselMove(InputAction.CallbackContext context)
    {
        _moving = false;
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            _onGround = true;
        }
    }

    private enum LookSplit
    {
        Look,
        Split
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1.05f + Vector3.up, 1.2f);
    }
}