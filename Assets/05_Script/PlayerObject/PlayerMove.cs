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
    private bool _onGround;

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
        _inputSystem.Player.Interact.started += OnInteract;
        _inputSystem.Enable();

        _inGameManager = FindAnyObjectByType<InGameManager>();
        
        //アニメーション関連初期化
        var pom = FindAnyObjectByType<PlayerOperationManager>();
        //アニメーションを設定
        if (gameObject.CompareTag("MainPlayer"))
        {
            
            pom._mainPlayerAnimator = _animator;
        }
        else
        {
            pom._subPlayerAnimator = _animator;
        }
    }

    private void Update()
    {
        if (_moving && _onGround && _canMove)
        {
            _rigidBody.linearVelocity = new Vector3(MoveDirection.x, _rigidBody.linearVelocity.y, MoveDirection.z);
        }
    }


    //仮想パッドを動かしたときに呼び出される。
    private void OnMove(InputAction.CallbackContext context)
    {
        _moving = true;
        Vector2 input = context.ReadValue<Vector2>();
        float magnitude = input.magnitude;
        float lrWeight = input.x + 1;
        if (MoveMode)
            MoveDirection = new Vector3(input.x, 0, input.y) * _moveSpeed;
        //前後移動で別のアニメーションにし、左右移動アニメーションとブレンドする
        if (input.y > 0)
        {
            _animator.SetBool(Back, false);
            _animator.SetBool(Forward, true);
        }
        else
        {
            _animator.SetBool(Forward, false);
            _animator.SetBool(Back, true);
        }

        _animator.SetFloat(BlendLr, lrWeight);
        //走っているか歩いているか
        if (magnitude < 0.5f)
        {
            _animator.SetBool(Run, false);
            _animator.SetFloat(Speed, Mathf.Max(magnitude * _walkBaseAnimationSpeed, 0.5f));
        }
        else
        {
            _animator.SetBool(Run, true);
            _animator.SetFloat(Speed, magnitude * _runBaseAnimationSpeed);
        }
    }
    
    void Interacted()
    {
        _canMove = true;
    }

    private void CanselMove(InputAction.CallbackContext context)
    {
        _moving = false;
        _animator.SetBool(Run, false);
        _animator.SetBool(Forward, false);
        _animator.SetBool(Back, false);
        _animator.SetFloat(BlendLr, 1);
        _animator.SetFloat(Speed, 1);
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