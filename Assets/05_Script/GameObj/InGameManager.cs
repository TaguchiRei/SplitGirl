using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class InGameManager : MonoBehaviour
{
    [SerializeField] private string _subScene;
    [SerializeField] private GameObject _Screen;
    [SerializeField] private PlayerAnimationManager _playerAnimationManager;

    [SerializeField] private float _inScreenStandards;
    
    public Vector2 DirectionVector; //現在の角度ベクトル
    
    public CameraMode cameraMode;

    public Action LoadedStart;
    
    public bool LoadedFlag;
    
    private Camera _mainCamera;
    private Camera _subCamera;
    

    private void Start()
    {
        LoadedFlag = false;
        cameraMode = CameraMode.MainCameraMove;
        SubSceneLoad().Forget();
    }

    void Update()
    {
        
    }

    /// <summary>
    /// サブシーンのロードを行う。
    /// </summary>
    private async UniTask SubSceneLoad()
    {
        await SceneManager.LoadSceneAsync(_subScene, LoadSceneMode.Additive).ToUniTask();
        _playerAnimationManager._mainPlayerAnimator = GameObject.FindGameObjectWithTag("MainPlayer").GetComponent<Animator>();
        _playerAnimationManager._subPlayerAnimator = GameObject.FindGameObjectWithTag("SubPlayer").GetComponent<Animator>();
        var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (var camera in cameras)
        {
            if (camera.CompareTag("MainCamera"))
            {
                _mainCamera = camera;
            }
            else if (camera.CompareTag("SubCamera"))
            {
                _subCamera = camera;
            }
        }
        LoadedStart?.Invoke();
        LoadedFlag = true;
    }

    public void ModeSet(CameraMode mode)
    {
        cameraMode = mode;
    }

    /// <summary>
    /// 画面内に写っているかを調べるメソッド。ある場合はtrue、無ければfalseを返す
    /// </summary>
    /// <param name="main"></param>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public bool CheckInScreen(bool main, Vector3 worldPos)
    {
        Vector3 screenPos = main ? _mainCamera.WorldToScreenPoint(worldPos) : _subCamera.WorldToScreenPoint(worldPos);
        float crossProduct = DirectionVector.x * screenPos.y - DirectionVector.y * screenPos.x;
        if (main)
        {
            return crossProduct > _inScreenStandards;
        }

        return crossProduct < _inScreenStandards;
    }
    
    public enum CameraMode
    {
        MainCameraMove,
        SubCameraMove,
        MainOnly,
        SubOnly
    }
}