using System;
using System.Collections;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerOperationManager : MonoBehaviour
{
    [SerializeField] private PlayerAnimationManager playerAnimationManager;
    [SerializeField] private InGameManager _inGameManager;
    private InputSystem_Actions _inputSystem;

    /// <summary>移動に使う</summary>
    private Action _moveAction;

    private Action _cancelMoveAction;

    private Action _mainInteractAction;
    private Action _subInteractAction;

    private GameObject _mainPlayerObject;
    private GameObject _subPlayerObject;
    private PlayerMove _mainPM;
    private PlayerMove _subPM;
    
    /// <summary>Main操作モードならtrue、Sub操作モードならfalseを返す</summary>
    private  bool MoveCheck => _inGameManager.cameraMode is InGameManager.CameraMode.MainOnly or InGameManager.CameraMode.MainCameraMove;
    
    //bool mainPlayerCanMove = 

    private Collider[] _interactColliders = new Collider[10];

    void Awake()
    {
        _inGameManager.LoadedStart += LoadedStart;
    }

    private void Start()
    {
        _inputSystem = new InputSystem_Actions();
        _inputSystem.Player.Move.performed += OnMoveInput;
        _inputSystem.Player.Move.canceled += OnMoveInput;
        _inputSystem.Player.Interact.performed += OnInteractInput;
        _inputSystem.Enable();
    }

    /// <summary>
    /// サブシーンがロードされた直後に発火するメソッド
    /// </summary>
    void LoadedStart()
    {
        _mainPlayerObject = playerAnimationManager._mainPlayerAnimator.gameObject;
        _subPlayerObject = playerAnimationManager._subPlayerAnimator.gameObject;
        _mainPM = _mainPlayerObject.GetComponent<PlayerMove>();
        _subPM = _subPlayerObject.GetComponent<PlayerMove>();
    }

    void FixedUpdate()
    {
        if (MoveCheck)
        {
        }
        else
        {
        }
    }

    void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            _moveAction?.Invoke();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            _cancelMoveAction?.Invoke();
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
            if (MoveCheck)
                interactObject = InteractObjectCheck(_mainPlayerObject);
            else
                interactObject = InteractObjectCheck(_subPlayerObject);
            
            if (interactObject == null) return;

            _mainPM._canMove = false;
            _subPM._canMove = false;
            Vector3 movePosition = interactObject.transform.position + interactObject.transform.forward * -0.42f;
            movePosition.y = transform.position.y;
            if (MoveCheck)
            {
                UseLeverSequence(_mainPlayerObject, interactObject, movePosition, _mainPM, _subPM).Play();
            }
            else
            {
                UseLeverSequence(_subPlayerObject, interactObject, movePosition, _mainPM, _subPM);
            }
        }
    }

    /// <summary>
    /// レバーを起動するために移動する際に使用する。
    /// </summary>
    /// <param name="moveObject">動くプレイヤーオブジェクトを入れる</param>
    /// <param name="interactObject"></param>
    /// <param name="movePosition"></param>
    /// <param name="mainPM"></param>
    /// <param name="subPM"></param>
    /// <returns></returns>
    Sequence UseLeverSequence(GameObject moveObject, GameObject interactObject, Vector3 movePosition, PlayerMove mainPM,
        PlayerMove subPM)
    {
        return DOTween.Sequence()
            .Append(moveObject.transform.DOMove(movePosition, 0.5f))
            .Append(moveObject.transform.DORotate(movePosition - moveObject.transform.position, 0.2f))
            .OnComplete(() =>
            {
                //----------------ここに左右を見極めるスクリプトを挟む----------------------
                //---------------------------------------------------------------------
                playerAnimationManager.UseLever();
                StartCoroutine(DelayAction(5f, () =>
                {
                    mainPM._canMove = true;
                    subPM._canMove = true;
                }));
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

    IEnumerator TouchLever(GameObject LeverObject)
    {
        var I = LeverObject.GetComponent<InteractObjectBase>();
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