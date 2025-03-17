using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
namespace GamesKeystoneFramework.Core.Input
{
    public class InputSystemProgramEditor : EditorWindow
    {
        private InputActionAsset asset;
        private string className;
        private List<string> mapName = new();
        private List<List<string>> actionName = new();
        private string programText;
        Vector2 scrollPosition = Vector2.zero;
        private bool useLegacy = false;

        [MenuItem("Window/GamesKeystoneFramework/InputSystemProgramingEditor")]
        public static void ShowWindow()
        {
            InputSystemProgramEditor window = GetWindow<InputSystemProgramEditor>("InputSystemProgramEditor");
            window.Show();
        }
        private void OnGUI()
        {
            asset = (InputActionAsset)EditorGUILayout.ObjectField("InputActionAsset", asset, typeof(InputActionAsset), true);
            className = EditorGUILayout.TextField("クラスの名前を決めてください",className);
            useLegacy = EditorGUILayout.Toggle("Use Legacy", useLegacy);
            if (asset != null)
            {
                GUILayout.Label("アタッチ済み: " + asset.name);
            }
            else
            {
                GUILayout.Label("InputActionAssetをアタッチしてください");
            }
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300)); // スクロールビューの開始
            programText = EditorGUILayout.TextArea(programText, GUILayout.ExpandHeight(true)); // TextAreaの高さを拡張
            EditorGUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Write", GUILayout.Width(50), GUILayout.Height(20)))
            {
                if (asset != null)
                {
                    mapName.Clear();
                    actionName.Clear();
                    foreach (var map in asset.actionMaps)
                    {
                        mapName.Add(map.name);
                        actionName.Add(new List<string>());
                        foreach (var action in map.actions)
                        {
                            actionName[^1].Add(action.name);
                        }
                    }
                    if (!useLegacy)
                    {
                        programText = MakePrograme(className, mapName, actionName);
                    }
                    else
                    {
                        programText = MakeProgramLegacy(className, mapName, actionName);
                    }
                    EditorGUI.FocusTextInControl(null);
                    Repaint();
                }
                else
                {
                    Debug.LogWarning("InputActionAssetがセットされていません");
                    programText = "";
                    EditorGUI.FocusTextInControl(null);
                    Repaint();
                }
            }
            GUILayout.EndHorizontal();
        }
        private string MakeProgramLegacy(string className, List<string> mapName, List<List<string>> actionName)
        {
            string returnString =
                "using System;\nusing System.Collections.Generic;\nusing UnityEngine;\nusing UnityEngine.InputSystem\n\npublic class " 
                + className + " : MonoBehaviour\n{\n    private PlayerInput _playerInput;\n    private Queue<Func<object>> inputQuere;\n    void Start()\n    {\n        _playerInput = new();\n        //アクションイベントを登録\n";
            for (int i = 0; i < mapName.Count; i++)
            {
                for (int j = 0; j < actionName[i].Count; j++)
                {
                    var map = mapName[i];
                    var action = actionName[i][j];
                    returnString += $"\n        _playerInput.{map}.{action}.performed += On{action};";
                    returnString += $"\n        _playerInput.{map}.{action}.started += On{action};";
                    returnString += $"\n        _playerInput.{map}.{action}.canceled += On{action};";
                    returnString += "\n";
                }
            }
            returnString += "\n    }";
            for (int i = 0; i < mapName.Count; i++)
            {
                for (int j = 0; j < actionName[i].Count; j++)
                {
                    returnString += "\n    private void On" + actionName[i][j] + "(InputAction.CallbackContext context)\n    {\n    \n    }";
                    returnString += "\n";
                }
            }
            returnString += "\n}";
            return returnString;
        }
        private string MakePrograme(string className, List<string> mapName, List<List<string>> actionName)
        {
            string returnString =
               "using System;\nusing System.Collections.Generic;\nusing UnityEngine;\nusing UnityEngine.InputSystem\n\npublic class "
               + className + " : MonoBehaviour\n{\n    private PlayerInput _playerInput;\n    private Queue<Func<object>> inputQuere;\n    void Start()\n    {\n        _playerInput = new();\n        //アクションイベントを登録\n";
            for (int i = 0; i < mapName.Count; i++)
            {
                for (int j = 0; j < actionName[i].Count; j++)
                {
                    var action = actionName[i][j];
                    returnString += $"\n        _playerInput.actions[\"{action}\"].performed += On{action};";
                    returnString += $"\n        _playerInput.actions[\"{action}\"].started += On{action};";
                    returnString += $"\n        _playerInput.actions[\"{action}\"].canceled += On{action};";
                    returnString += "\n";
                }
            }
            returnString += "\n    }";
            for (int i = 0; i < mapName.Count; i++)
            {
                for (int j = 0; j < actionName[i].Count; j++)
                {
                    returnString += "\n    private void On" + actionName[i][j] + "(InputAction.CallbackContext context)\n    {\n    \n    }";
                    returnString += "\n";
                }
            }
            returnString += "\n}";
            return returnString;
        }
    }

}