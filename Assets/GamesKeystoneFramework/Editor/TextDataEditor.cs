using System;
using System.Collections.Generic;
using System.Text;
using GamesKeystoneFramework.Core.Text;
using UnityEditor;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

namespace GamesKeystoneFramework.Editor
{
    public class TextDataEditor : EditorWindow
    {
        //編集するスクリプタブルオブジェクト
        private TextDataScriptable _textDataScriptable;

        /// <summary>
        /// どの会話データかを示す数値
        /// </summary>
        private int _selectionNumber;

        private List<string> _selectionList;
        private string[] _selectionArray;

        //editor側で保存する情報
        private int _textMaxLength = 20;
        private int _selectionMaxLength = 8;
        private int _indentation;
        private Vector2 _scrollPosition;
        private Color _lineColor;
        private GUIStyle _normalTextStyle;

        private string _lineDesign;

        //editor側で保存するためのキー
        private const string LineColor1PrefKey = "TextDataEditor_LineColor";
        private const string LineColor2PrefKey = "TextDataEditor_LineColor2";
        private const string LineColor3PrefKey = "TextDataEditor_LineColor3";
        private const string LineColor4PrefKey = "TextDataEditor_LineColor4";
        private const string TextMaxLengthPrefKey = "TextDataEditor_TextMaxLength";
        private const string SelectionMaxLengthPrefKey = "TextDataEditor_SelectionMaxLength";


        //保存用のSerializedProperty等
        private SerializedObject _textDataScriptableSerializedObject;
        private SerializedProperty _textDataListProperty;
        private SerializedProperty _dataListProperty;
        private SerializedProperty _labelProperty;
        private List<SerializedProperty> _dataTypePropertyList;
        private List<SerializedProperty> _textPropertyList;
        private List<SerializedProperty> _useEventPropertyList;
        private List<SerializedProperty> _methodNumPropertyList;


        [MenuItem("Window/GamesKeystoneFramework/TextDataEditor")]
        public static void ShowWindow()
        {
            GetWindow<TextDataEditor>("TextDataEditor").Show();
        }

        private void OnEnable()
        {
            try
            {
                _lineColor = new Color(
                    EditorPrefs.GetFloat(LineColor1PrefKey),
                    EditorPrefs.GetFloat(LineColor2PrefKey),
                    EditorPrefs.GetFloat(LineColor3PrefKey),
                    EditorPrefs.GetFloat(LineColor4PrefKey));
                _textMaxLength = EditorPrefs.GetInt(TextMaxLengthPrefKey);
                _selectionMaxLength = EditorPrefs.GetInt(SelectionMaxLengthPrefKey);
            }
            catch (Exception e)
            {
                Debug.Log("Initialize" + e.Message);
                _lineColor = Color.yellow;
                _textMaxLength = 25;
                _selectionMaxLength = 8;
            }

            var st = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                st.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(_lineColor)}>｜</color>｜｜");
            }

            _lineDesign = st.ToString();
            _selectionNumber = 0;
        }

        private void OnGUI()
        {
            _normalTextStyle ??= new(GUI.skin.label)
            {
                richText = true
            };

            EditorGUI.BeginChangeCheck();
            _textDataScriptable = (TextDataScriptable)EditorGUILayout.ObjectField("会話データをアタッチ", _textDataScriptable,
                typeof(TextDataScriptable), false);
            if (EditorGUI.EndChangeCheck())
            {
                //エラー回避
                if (_textDataScriptable == null)
                    return;
                SelecterReset();
                _textDataScriptableSerializedObject = null;
            }

            //エラー回避
            if (_textDataScriptable == null)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            _selectionNumber = EditorGUILayout.Popup(_selectionNumber, _selectionArray, GUILayout.Width(100));
            if (GUILayout.Button("読み込み"))
            {
                LoadData();
                if (_textDataScriptableSerializedObject == null)
                {
                    Debug.Log("データを正常にロードできませんでした");
                    return;
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            _textMaxLength = EditorGUILayout.IntSlider("テキストの長さ", _textMaxLength, 1, 300);
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetInt(TextMaxLengthPrefKey, _textMaxLength);

            EditorGUI.BeginChangeCheck();
            _selectionMaxLength = EditorGUILayout.IntSlider("選択肢の長さ", _selectionMaxLength, 1, 300);
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetInt(SelectionMaxLengthPrefKey, _selectionMaxLength);

            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            _lineColor = EditorGUILayout.ColorField(_lineColor, GUILayout.Width(180));
            if (EditorGUI.EndChangeCheck())
            {
                var st = new StringBuilder();
                for (int i = 0; i < 5; i++)
                {
                    st.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(_lineColor)}>｜</color>｜｜");
                }

                _lineDesign = st.ToString();
                EditorPrefs.SetFloat(LineColor1PrefKey, _lineColor.r);
                EditorPrefs.SetFloat(LineColor2PrefKey, _lineColor.g);
                EditorPrefs.SetFloat(LineColor3PrefKey, _lineColor.b);
                EditorPrefs.SetFloat(LineColor4PrefKey, _lineColor.a);
            }


            if (_textDataScriptableSerializedObject == null)
            {
                GUILayout.Label("データを読み込んでください");
                return;
            }


            _textDataScriptableSerializedObject.Update();
            _indentation = 0;

            EditorGUILayout.PropertyField(_labelProperty, GUIContent.none);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(position.height - 180));
            for (int i = 0; i < _dataTypePropertyList.Count; i++)
            {
                var dataType = (TextDataType)_dataTypePropertyList[i].intValue;
                var text = _textPropertyList[i].stringValue;
                var useEvent = _useEventPropertyList[i].boolValue;

                #region 文字数超過チェック

                if (dataType == TextDataType.Text)
                {
                    if (text.Length > _textMaxLength)
                    {
                        GUILayout.Label($"　　　{i}行目文字数超過");
                    }
                }
                else if (dataType == TextDataType.Branch)
                {
                    if (text.Length > _selectionMaxLength)
                    {
                        GUILayout.Label($"　　　{i}行目文字数超過");
                    }
                }

                #endregion

                #region 処理前の_indentation調整

                if (dataType != TextDataType.Text)
                {
                    if (dataType == TextDataType.Branch)
                    {
                        _indentation--;
                    }
                    else if (dataType == TextDataType.QEnd)
                    {
                        _indentation -= 2;
                    }
                }

                if (_indentation < 0)
                {
                    _indentation = 0;
                    GUILayout.Label(
                        $"          <color=red>インデントが異常です</color><color=white>{i}行目でQEndが多すぎるかBranchがQuestionがないまま使用されています</color>",
                        _normalTextStyle);
                }

                #endregion

                GUILayout.BeginHorizontal();
                GUILayout.Label(i.ToString(), GUILayout.Width(30));
                EditorGUILayout.PropertyField(_dataTypePropertyList[i], GUIContent.none, GUILayout.Width(80));
                GUILayout.Label(_lineDesign, _normalTextStyle, GUILayout.Width(_indentation * 20));
                if (dataType == TextDataType.QEnd || dataType == TextDataType.TextEnd)
                {
                    GUI.enabled = false;
                    GUILayout.TextField($"{dataType}には入力できません");
                    GUI.enabled = true;
                }
                else
                {
                    EditorGUILayout.PropertyField(_textPropertyList[i], GUIContent.none, GUILayout.ExpandWidth(true));
                }

                if (useEvent)
                {
                    EditorGUILayout.PropertyField(_methodNumPropertyList[i], GUIContent.none, GUILayout.Width(30));
                }

                EditorGUILayout.PropertyField(_useEventPropertyList[i], GUIContent.none, GUILayout.Width(20));

                if (GUILayout.Button("×", GUILayout.Width(20)))
                {
                    if (_textDataScriptable.TextDataList[_selectionNumber].DataList.Count != 1)
                    {
                        _textDataScriptable.TextDataList[_selectionNumber].DataList.RemoveAt(i);
                        LoadData();
                        GUILayout.EndHorizontal();
                        GUILayout.EndScrollView();
                        return;
                    }
                }

                if (GUILayout.Button("↓", GUILayout.Width(20)))
                {
                    _textDataScriptable.TextDataList[_selectionNumber].DataList.Insert(i + 1, new TextData());
                    LoadData();
                    GUILayout.EndHorizontal();
                    GUILayout.EndScrollView();
                    return;
                }

                GUILayout.EndHorizontal();

                #region 処理後の_indentation調整

                if (dataType == TextDataType.Question)
                {
                    _indentation += 2;
                }
                else if (dataType == TextDataType.Branch)
                {
                    _indentation++;
                }

                #endregion
            }

            EditorGUILayout.EndScrollView();

            _textDataScriptableSerializedObject.ApplyModifiedProperties();
        }


        void SelecterReset()
        {
            _selectionList = new List<string>();
            foreach (var t in _textDataScriptable.TextDataList)
            {
                _selectionList.Add(t.TextLabel);
            }

            _selectionArray = _selectionList.ToArray();
        }

        /// <summary>
        /// シリアライズドオブジェクトを設定、textDataListとtextData
        /// </summary>
        private void LoadData()
        {
            _textDataScriptableSerializedObject = new SerializedObject(_textDataScriptable);
            _textDataListProperty = _textDataScriptableSerializedObject.FindProperty("TextDataList");
            _dataListProperty = _textDataListProperty.GetArrayElementAtIndex(_selectionNumber)
                .FindPropertyRelative("DataList");
            _labelProperty = _textDataListProperty.GetArrayElementAtIndex(_selectionNumber)
                .FindPropertyRelative("TextLabel");
            if (_dataTypePropertyList == null)
            {
                _dataTypePropertyList = new();
                _textPropertyList = new();
                _useEventPropertyList = new();
                _methodNumPropertyList = new();
            }
            else
            {
                _dataTypePropertyList.Clear();
                _textPropertyList.Clear();
                _useEventPropertyList.Clear();
                _methodNumPropertyList.Clear();
            }

            for (int i = 0; i < _textDataScriptable.TextDataList[_selectionNumber].DataList.Count; i++)
            {
                var textData = _dataListProperty.GetArrayElementAtIndex(i);
                _dataTypePropertyList.Add(textData.FindPropertyRelative("DataType"));
                _textPropertyList.Add(textData.FindPropertyRelative("Text"));
                _useEventPropertyList.Add(textData.FindPropertyRelative("UseEvent"));
                _methodNumPropertyList.Add(textData.FindPropertyRelative("MethodNumber"));
            }
        }
    }
}