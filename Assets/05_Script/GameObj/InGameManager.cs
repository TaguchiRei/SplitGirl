using UnityEngine;
using UnityEngine.SceneManagement;
public class InGameManager : MonoBehaviour
{
    [SerializeField] string _subScene;
    void Start()
    {
        SceneManager.LoadScene(_subScene, LoadSceneMode.Additive);
    }
}
