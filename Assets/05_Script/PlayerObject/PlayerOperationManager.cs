using System;
using System.Collections;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerOperationManager : MonoBehaviour
{
    private static readonly int DirectionVector = Shader.PropertyToID("_DirectionVector");

    //----------------各マネージャー等の情報を保存------------------------
    [SerializeField] private PlayerAnimationManager _playerAnimationManager;
    [SerializeField] private InGameManager _inGameManager;
    [SerializeField] private Image _splitForCross;
    private InputSystem_Actions _inputSystem;

    //----------------メインとサブそれぞれの情報を保存---------------------
    private bool _mainMoving;
    private bool _subMoving;

    private Action _mainInteractAction;
    private Action _subInteractAction;

    private GameObject _mainPlayerObject;
    private GameObject _subPlayerObject;

    private PlayerMove _mainPm;
    private PlayerMove _subPm;

    private Rigidbody _mainRigidbody;
    private Rigidbody _subRigidbody;

    //-----------------共通の情報を保存---------------------------------
    [SerializeField] private float _moveSpeed;
    private bool _canMove;
    private Vector3 _moveDirection;

    /// <summary>タップ中かどうかを保存</summary>
    [SerializeField] private Vector2 _splitCenter;

    [SerializeField] InputAction _primaryTouchAction;
    [SerializeField] Material _splitMaterial; //

    private bool _tapCheck; //
    private float _maxMagnitude; //
    private float _timer; //

    private Vector2? _tapPosition; //タップした位置
    private SwipeMode _swipeMode; //スワイプモードかチェンジモードか
    private Vector2 _changeTapPos; //直前のタップ位置の変化量

    private Vector2 _directionVector; //現在の角度ベクトル
    private float _directionAngle; //現在の角度
    private Vector2 _modifiedVector; //変更後角度ベクトル
    private float _tapOriginAngle; //タップ位置のデフォルト角度


    /// <summary>Main操作モードならtrue、Sub操作モードならfalseを返す</summary>
    private bool ModeCheck =>
        _inGameManager.cameraMode is InGameManager.CameraMode.MainOnly or InGameManager.CameraMode.MainCameraMove;

    /// <summary>メインプレイヤーオブジェクトが動けるかを返す</summary>
    private bool MainMoveCheck => _canMove && _mainMoving && _mainPm.OnGround;

    /// <summary>サブプレイヤーオブジェクトが動けるかを返す</summary>
    private bool SubMoveCheck => _canMove && _subMoving && _subPm.OnGround;

    private Collider[] _interactColliders = new Collider[10];

    void Awake()
    {
        _swipeMode = SwipeMode.None;
        _directionVector = Vector2.one;
        Material mat = Instantiate(_splitForCross.material);
        mat.SetVector(DirectionVector, _directionVector);
        _splitForCross.material = mat;
        _inGameManager.LoadedStart += LoadedStart;
    }

    private void Start()
    {
        _canMove = true;

        _inputSystem = new InputSystem_Actions();
        _inputSystem.Player.Move.performed += OnMoveInput;
        _inputSystem.Player.Move.canceled += OnMoveInput;
        _inputSystem.Player.Look.started += OnLookInput;
        _inputSystem.Player.Tap.started += _ =>
        {
            _tapPosition = null;
            _tapCheck = true;
            _maxMagnitude = 0;
            _timer = 0;
            _swipeMode = SwipeMode.SwipeToLook;
        };
        _inputSystem.Player.Look.performed += OnLookInput;
        _inputSystem.Player.Tap.canceled += _ =>
        {
            Debug.Log(_directionVector);
            _directionVector = _modifiedVector;
            _tapPosition = null;
            _tapCheck = false;
            _timer = 0;
            _swipeMode = SwipeMode.None;
        };

        _inputSystem.Player.Interact.started += OnInteractInput;
        _inputSystem.Enable();
        _primaryTouchAction.performed += v => _changeTapPos = v.ReadValue<Vector2>();
        _primaryTouchAction.Enable();
    }

    /// <summary>
    /// サブシーンがロードされた直後に発火するメソッド
    /// </summary>
    void LoadedStart()
    {
        _mainPlayerObject = _playerAnimationManager._mainPlayerAnimator.gameObject;
        _subPlayerObject = _playerAnimationManager._subPlayerAnimator.gameObject;
        _mainPm = _mainPlayerObject.GetComponent<PlayerMove>();
        _subPm = _subPlayerObject.GetComponent<PlayerMove>();
        _mainRigidbody = _mainPlayerObject.GetComponent<Rigidbody>();
        _subRigidbody = _subPlayerObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        _swipeMode = SwipeMode.SwipeToChange;
        if (!_inGameManager.LoadedFlag || _swipeMode == SwipeMode.None) return;

        if (_maxMagnitude < 0.5f && _timer > 0.5f)
        {
            Debug.Log("Change");
            _swipeMode = SwipeMode.SwipeToChange;
        }
        else
        {
            _timer += Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (!_inGameManager.LoadedFlag) return;
        switch (ModeCheck)
        {
            case true when MainMoveCheck:
                _mainRigidbody.linearVelocity =
                    _mainRigidbody.transform.TransformDirection(new Vector3(_moveDirection.x,
                        _mainRigidbody.linearVelocity.y, _moveDirection.z));
                break;
            case false when SubMoveCheck:
                _subRigidbody.linearVelocity =
                    _subRigidbody.transform.TransformDirection(new Vector3(_moveDirection.x,
                        _subRigidbody.linearVelocity.y, _moveDirection.z));
                break;
        }
    }

    /// <summary>
    /// 移動のデリゲートに登録するメソッド
    /// </summary>
    /// <param name="context"></param>
    void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            _mainMoving = true;
            Vector2 input = context.ReadValue<Vector2>();
            float magnitude = input.magnitude;
            _moveDirection = new Vector3(input.x, 0, input.y) * _moveSpeed;
            if (ModeCheck) _mainMoving = true;
            else _subMoving = true;
            _playerAnimationManager.MoveAnimation(input.y > 0, magnitude > 0.5f, input.x + 1,
                Mathf.Max(magnitude * 0.8f, 0.5f));
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            _mainMoving = false;
            _subMoving = false;
            _playerAnimationManager.CancelMoveAnimation();
        }
    }

    /// <summary>
    /// 視点操作
    /// </summary>
    /// <param name="context"></param>
    void OnLookInput(InputAction.CallbackContext context)
    {
        if (!_tapCheck) return;
        var pos = context.ReadValue<Vector2>() - new Vector2((float)Screen.width / 2, (float)Screen.height / 2);
        if (_tapPosition == null)
        {
            _tapPosition = pos;
            _tapOriginAngle = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
            _directionAngle = Mathf.Atan2(_directionVector.y, _directionVector.x) * Mathf.Rad2Deg;
        }

        _maxMagnitude = Math.Max(_maxMagnitude, (pos - _tapPosition.Value).magnitude);
        switch (_swipeMode)
        {
            case SwipeMode.SwipeToChange:
                var currentAngle = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
                var deltaAngle = Mathf.DeltaAngle(_tapOriginAngle, currentAngle);
                var modAngle = _directionAngle + deltaAngle;
                _modifiedVector = new Vector2(Mathf.Cos(modAngle * Mathf.Deg2Rad), Mathf.Sin(modAngle * Mathf.Deg2Rad));
                ChangeMaterialDirection(_modifiedVector);
                break;

            case SwipeMode.SwipeToLook:
                if (ModeCheck)
                    _mainRigidbody.transform.eulerAngles += new Vector3(0, _changeTapPos.x / 3, 0);
                else
                    _subRigidbody.transform.eulerAngles += new Vector3(0, _changeTapPos.x / 3, 0);
                break;
        }
    }


    /// <summary>
    /// インタラクト時に起動するメソッド
    /// </summary>
    /// <param name="context"></param>
    void OnInteractInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            GameObject interactObject;
            if (ModeCheck)
                interactObject = InteractObjectCheck(_mainPlayerObject);
            else
                interactObject = InteractObjectCheck(_subPlayerObject);

            if (interactObject == null) return;

            _canMove = false;
            _canMove = false;
            Vector3 movePosition = interactObject.transform.position + interactObject.transform.forward * -0.42f;
            movePosition.y = ModeCheck ? _mainPlayerObject.transform.position.y : _subPlayerObject.transform.position.y;
            if (ModeCheck)
            {
                UseLeverSequence(_mainPlayerObject, interactObject, movePosition, _mainPm, _subPm).Play();
            }
            else
            {
                UseLeverSequence(_subPlayerObject, interactObject, movePosition, _mainPm, _subPm).Play();
            }
        }
    }

    /// <summary>
    /// レバーを起動するために移動する際に使用する。
    /// </summary>
    /// <param name="moveObject">動くプレイヤーオブジェクトを入れる</param>
    /// <param name="interactObject"></param>
    /// <param name="movePosition"></param>
    /// <param name="mainPm"></param>
    /// <param name="subPm"></param>
    /// <returns></returns>
    Sequence UseLeverSequence(GameObject moveObject, GameObject interactObject, Vector3 movePosition, PlayerMove mainPm,
        PlayerMove subPm)
    {
        var xzMovePos = new Vector3(movePosition.x, 0, movePosition.z);
        var xzInteractPos = new Vector3(interactObject.transform.position.x, 0, interactObject.transform.position.z);
        var direction = xzInteractPos - xzMovePos;
        var angle = new Vector3(0, Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg - 90, 0);
        return DOTween.Sequence()
            .Append(moveObject.transform.DOMove(movePosition, 0.5f))
            .Join(moveObject.transform.DORotate(angle, 0.2f))
            .OnComplete(() =>
            {
                //----------------ここに左右を見極めるスクリプトを挟む----------------------
                //---------------------------------------------------------------------
                _playerAnimationManager.UseLever();
                StartCoroutine(DelayAction(5f, () => { _canMove = true; }));
                StartCoroutine(TouchLever(interactObject));
            });
    }

    /// <summary>
    /// インタラクト可能なオブジェクトがあるかどうかを調べ、あった場合GameObjectを返す。
    /// 無かった場合はnullを返す
    /// </summary>
    /// <param name="checkBaseObject"></param>
    /// <returns></returns>
    [CanBeNull]
    GameObject InteractObjectCheck(GameObject checkBaseObject)
    {
        Physics.OverlapSphereNonAlloc(
            checkBaseObject.transform.position + checkBaseObject.transform.forward * 1.05f + Vector3.up
            , 1.2f
            , _interactColliders);

        foreach (var col in _interactColliders)
        {
            if (col == null)
                break;
            if (col.gameObject.CompareTag("Interactive"))
            {
                return col.gameObject;
            }
        }

        return null;
    }

    private void ChangeMaterialDirection(Vector2 direction)
    {
        Material mat = Instantiate(_splitForCross.material);
        mat.SetVector(DirectionVector, direction);
        _splitForCross.material = mat;
    }

    enum SwipeMode
    {
        None,
        SwipeToLook,
        SwipeToChange
    }

    IEnumerator DelayAction(float delay, Action action = null)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    IEnumerator TouchLever(GameObject leverObject)
    {
        var I = leverObject.GetComponent<InteractObjectBase>();
        yield return new WaitForSeconds(1.7f);
        I.Interact();
    }
}