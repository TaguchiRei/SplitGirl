using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class InGameManager : MonoBehaviour
{
    [SerializeField] private string _subScene;
    [SerializeField] private GameObject _Screen;
    private PlayerMove _mainPlayer;
    private PlayerMove _subPlayer;
    private List<PlayerMove> _playerMoves;

    public float LookThreshold = 1;
    public CameraMode cameraMode;
    

    private void Start()
    {
        cameraMode = CameraMode.MainCameraMove;
        SubSceneLoad().Forget();
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
                _Screen.SetActive(true);
                _mainPlayer.MoveMode = true;
                _subPlayer.MoveMode = false;
                break;
            //画面分割時、サブカメラ側の操作をする(キャラクターの位置は両シーンで別)
            case CameraMode.SubCameraMove:
                _Screen.SetActive(true);
                _mainPlayer.MoveMode = false;
                _subPlayer.MoveMode = true;
                break;
            //画面分割をしない純粋なメインカメラ(キャラクターの居場所はメインシーン依存)
            case CameraMode.MainOnly:
                _Screen.SetActive(false);
                break;
            //画面分割をしない純粋なサブカメラ(キャラクターの居場所はサブシーン依存)
            case CameraMode.SubOnly:
                _Screen.SetActive(false);
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
        _playerMoves = FindObjectsByType<PlayerMove>(FindObjectsSortMode.None).ToList();
        _mainPlayer = _playerMoves.FirstOrDefault(x => x.CompareTag("MainPlayer"));
        _subPlayer = _playerMoves.FirstOrDefault(x => x.CompareTag("SubPlayer"));
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