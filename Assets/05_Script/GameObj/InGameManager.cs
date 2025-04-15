using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
    private static readonly int Sub = Shader.PropertyToID("_Sub");
    private static readonly int Main = Shader.PropertyToID("_Main");
    [SerializeField] private string _subScene;
    [SerializeField] private Image _divisionScreen;
    [SerializeField] private Image _mainScreen;
    [SerializeField] private Image _subScreen;
    [SerializeField] private PlayerAnimationManager _playerAnimationManager;

    [SerializeField] private float _inScreenStandards;

    public Vector2 DirectionVector; //現在の角度ベクトル

    public CameraMode cameraMode;

    public Action LoadedStart;

    public bool LoadedFlag;

    private Camera _mainCamera;
    private Camera _subCamera;

    public static InGameManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI ModeTMP;

    private void Awake()
    {
        Instance = this;
    }


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
        _playerAnimationManager._mainPlayerAnimator =
            GameObject.FindGameObjectWithTag("MainPlayer").GetComponent<Animator>();
        _playerAnimationManager._subPlayerAnimator =
            GameObject.FindGameObjectWithTag("SubPlayer").GetComponent<Animator>();
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
        ModeSetting();
        
        LoadedStart?.Invoke();
        LoadedFlag = true;
    }

    public void ModeSet(CameraMode mode)
    {
        cameraMode = mode;
        ModeSetting();
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
            return cameraMode == CameraMode.MainOnly || crossProduct < _inScreenStandards;
        }

        return cameraMode == CameraMode.SubOnly || crossProduct > _inScreenStandards;
    }

    public void ModeChange()
    {
        if (cameraMode != CameraMode.SubOnly)
            cameraMode++;
        else
            cameraMode = CameraMode.MainCameraMove;
        ModeSetting();
    }

    private void ModeSetting()
    {
        switch (cameraMode)
        {
            case CameraMode.MainCameraMove:
                _divisionScreen.gameObject.SetActive(true);
                _subScreen.gameObject.SetActive(false);
                _mainScreen.gameObject.SetActive(false);
                _divisionScreen.material.SetVector(Sub,Color.gray);
                _divisionScreen.material.SetColor(Main, Color.white);
                break;
            case CameraMode.SubCameraMove:
                _divisionScreen.gameObject.SetActive(true);
                _subScreen.gameObject.SetActive(false);
                _mainScreen.gameObject.SetActive(false);
                _divisionScreen.material.SetVector(Main,Color.gray);
                _divisionScreen.material.SetColor(Sub, Color.white);
                break;
            case CameraMode.MainOnly:
                _divisionScreen.gameObject.SetActive(false);
                _mainScreen.gameObject.SetActive(true);
                _subScreen.gameObject.SetActive(false);
                break;
            case CameraMode.SubOnly:
                _divisionScreen.gameObject.SetActive(false);
                _mainScreen.gameObject.SetActive(false);
                _subScreen.gameObject.SetActive(true);
                break;
        }
        ModeTMP.text = cameraMode.ToString();
    }

    public enum CameraMode
    {
        MainCameraMove,
        MainOnly,
        SubCameraMove,
        SubOnly
    }
}