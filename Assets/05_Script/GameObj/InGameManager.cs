using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

public class InGameManager : MonoBehaviour
{
    [SerializeField] string _subScene;
    private GameObject _mainCamera;
    private GameObject _subCamera;
    private List<GameObject> _allCamera;

    public CameraMode cameraMode;
    

    private void Start()
    {
        SubSceneLoad().Forget();
        cameraMode = CameraMode.MainCameraMove;
    }

    /// <summary>
    /// モード変更直後の処理を行う。
    /// </summary>
    private void ModeChange()
    {
        switch (cameraMode)
        {
            //画面分割時、メインカメラ側の操作をする(キャラクターの位置は両シーンで別)
            case CameraMode.MainCameraMove:
                break;
            //画面分割時、サブカメラ側の操作をする(キャラクターの位置は両シーンで別)
            case CameraMode.SubCameraMove:
                break;
            //画面分割をしない純粋なメインカメラ(キャラクターの居場所はメインシーン依存)
            case CameraMode.MainOnly:
                break;
            //画面分割をしない純粋なサブカメラ(キャラクターの居場所はサブシーン依存)
            case CameraMode.SubOnly:
                break;
        }
    }

    void Update()
    {
        
    }

    /// <summary>
    /// サブシーンのロードを行う。
    /// ロード後、カメラの取得と位置と回転の初期化
    /// </summary>
    private async UniTask SubSceneLoad()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(_subScene, LoadSceneMode.Additive);
        await op.ToUniTask();
        _allCamera = FindObjectsByType<Camera>(FindObjectsSortMode.None).Select(x => x.gameObject).ToList();
        _mainCamera = _allCamera.Find(o => o.gameObject.CompareTag("MainCamera"));
        _subCamera = _allCamera.Find(o => o.gameObject.CompareTag("SubCamera"));
        _subCamera.transform.SetPositionAndRotation(_mainCamera.transform.position, _mainCamera.transform.rotation);
        ModeChange();
    }

    public void ModeSet(CameraMode mode)
    {
        cameraMode = mode;
        ModeChange();
    }
    
    public enum CameraMode
    {
        MainCameraMove,
        SubCameraMove,
        MainOnly,
        SubOnly
    }

}