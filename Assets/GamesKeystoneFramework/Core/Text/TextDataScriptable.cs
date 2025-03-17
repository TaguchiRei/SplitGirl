using System.Collections.Generic;
using System;
using UnityEngine;

namespace GamesKeystoneFramework.Core.Text
{
    [CreateAssetMenu(fileName = "TextData", menuName = "Scriptable Objects/TextDataObject")]
    public class TextDataScriptable : ScriptableObject
    {
        public List<TextDataList> TextDataList;
    }

    [Serializable]
    public class TextDataList
    {
        public string TextLabel = "default";
        public List<TextData> DataList;
    }
    [Serializable]
    public class TextData
    {
        public TextDataType DataType = TextDataType.Text;
        public string Text = "";
        public bool UseEvent  = false;
        public int MethodNumber = 0;
    }
    
    public enum TextDataType
    {
        Text,
        Question,
        Branch,
        QEnd,
        TextEnd,
    }
}
