using System;
using System.Collections;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerOperationManager : MonoBehaviour
{
    //----------------各マネージャー等の情報を保存------------------------
    [SerializeField] private PlayerAnimationManager playerAnimationManager;
    [SerializeField] private InGameManager _inGameManager;
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
        _inGameManager.LoadedStart += LoadedStart;
    }

    private void Start()
    {
        _canMove = true;

        _inputSystem = new InputSystem_Actions();
        _inputSystem.Player.Move.performed += OnMoveInput;
        _inputSystem.Player.Move.canceled += OnMoveInput;
        _inputSystem.Player.Interact.started += OnInteractInput;
        _inputSystem.Enable();
    }

    /// <summary>
    /// サブシーンがロードされた直後に発火するメソッド
    /// </summary>
    void LoadedStart()
    {
        _mainPlayerObject = playerAnimationManager._mainPlayerAnimator.gameObject;
        _subPlayerObject = playerAnimationManager._subPlayerAnimator.gameObject;
        _mainPm = _mainPlayerObject.GetComponent<PlayerMove>();
        _subPm = _subPlayerObject.GetComponent<PlayerMove>();
        _mainRigidbody = _mainPlayerObject.GetComponent<Rigidbody>();
        _subRigidbody = _subPlayerObject.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!_inGameManager.LoadedFlag) return;
        switch (ModeCheck)
        {
            case true when MainMoveCheck:
                _mainRigidbody.linearVelocity =
                    new Vector3(_moveDirection.x, _mainRigidbody.linearVelocity.y, _moveDirection.z);
                break;
            case false when SubMoveCheck:
                _subRigidbody.linearVelocity =
                    new Vector3(_moveDirection.x, _subRigidbody.linearVelocity.y, _moveDirection.z);
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
            playerAnimationManager.MoveAnimation(input.y > 0, magnitude > 0.5f, input.x + 1,
                Mathf.Max(magnitude * 0.8f, 0.5f));
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            _mainMoving = false;
            _subMoving = false;
            playerAnimationManager.CancelMoveAnimation();
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
            movePosition.y = transform.position.y;
            if (ModeCheck)
            {
                UseLeverSequence(_mainPlayerObject, interactObject, movePosition, _mainPm, _subPm).Play();
            }
            else
            {
                UseLeverSequence(_subPlayerObject, interactObject, movePosition, _mainPm, _subPm);
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
        return DOTween.Sequence()
            .Append(moveObject.transform.DOMove(movePosition, 0.5f))
            .Append(moveObject.transform.DORotate(movePosition - moveObject.transform.position, 0.2f))
            .OnComplete(() =>
            {
                //----------------ここに左右を見極めるスクリプトを挟む----------------------
                //---------------------------------------------------------------------
                playerAnimationManager.UseLever();
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

    public void SetMoveAction(Action moveAction)
    {
    }

    public void SetInteractAction(Action interactAction)
    {
    }
}