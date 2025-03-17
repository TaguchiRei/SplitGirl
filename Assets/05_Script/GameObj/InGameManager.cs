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
        cameraMode = CameraMode.MainCamera;
    }

    private void ModeChange()
    {
        switch (cameraMode)
        {
            case CameraMode.MainCamera:
                _subCamera.transform.SetParent(_mainCamera.transform);
                break;
            case CameraMode.SubCamera:
                
                break;
            case CameraMode.BothCamera:
                break;
        }
    }

    void Update()
    {
        
    }

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

    public enum CameraMode
    {
        MainCamera,
        BothCamera,
        SubCamera,
    }

}