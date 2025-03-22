using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class InGameManager : MonoBehaviour
{
    [SerializeField] private string _subScene;
    [SerializeField] private GameObject _Screen;
    [SerializeField] private PlayerAnimationManager _playerAnimationManager;
    
    public CameraMode cameraMode;

    public Action LoadedStart;
    
    public bool LoadedFlag;
    

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
        LoadedStart?.Invoke();
        LoadedFlag = true;
    }

    public void ModeSet(CameraMode mode)
    {
        cameraMode = mode;
    }
    
    public enum CameraMode
    {
        MainCameraMove,
        SubCameraMove,
        MainOnly,
        SubOnly
    }

}