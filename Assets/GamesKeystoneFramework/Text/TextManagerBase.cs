using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GamesKeystoneFramework.Core.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamesKeystoneFramework.Text
{
    public abstract class TextManagerBase : MonoBehaviour
    {
        //使用者が設定に使用
        [SerializeField] private bool useBranch = true;
        [SerializeField] private bool displayCharOneByOne;
        [SerializeField] private bool resetForQuestion;
        [SerializeField] private int line = 3;
        [SerializeField] private float writeSpeed = 0.1f;
        [SerializeField] private string[] names;


        //必須
        [SerializeField] private TextMeshProUGUI mainText;
        [SerializeField] private TextMeshProUGUI selectionText;
        [SerializeField] private Image mainTextImage;
        [SerializeField] private Image selectionTextImage;

        //処理に使用する
        private readonly List<(string, int)> _choices = new();
        private List<TextData> _dataList;
        private int _lineNumber;
        private int _questionIndentation;
        private bool _movingCoroutine;
        private bool _selectMode;
        private Coroutine _typeTextCoroutine;

        public int selectNumber;


        private void Start()
        {
            TextBox();
            if (useBranch)
            {
                SelectionBox();
            }
        }

        protected virtual void TextStart(TextDataScriptable textDataScriptable, int selectionIndex,
            Action action = null)
        {
            _lineNumber = 0;
            action?.Invoke();
            TextBox(true);
            _dataList = TextUpdate(textDataScriptable.TextDataList[selectionIndex].DataList);
            mainText.text = string.Empty;
            Next();
        }

        /// <summary>
        /// 文章表示中に決定ボタンを押したときの処理を一括で管理
        /// 戻り値がtrueならその後にテキストが続き、falseなら終了
        /// </summary>
        protected bool Next()
        {
            if (_dataList == null || _dataList.Count <= _lineNumber)
            {
                TextBox();
                if(useBranch)
                    SelectionBox();
                return false;
            }

            if (_movingCoroutine)
            {
                //一文字づつ表示を強制終了させてすべて表示
                StopCoroutine(_typeTextCoroutine);
                _typeTextCoroutine = null;
                if (_dataList[_lineNumber].DataType == TextDataType.Text)
                {
                    _lineNumber++;
                }
                else if (_dataList[_lineNumber].DataType == TextDataType.Question)
                {
                    _lineNumber++;
                    SelectorShow();
                }
                mainText.maxVisibleCharacters = mainText.GetParsedText().Length;
                _movingCoroutine = false;
            }
            else
            {
                if (_selectMode)
                {
                    //セレクトモード中に決定が押されたときの処理
                    _selectMode = false;
                    _lineNumber = _choices[selectNumber].Item2 + 1;
                    SelectionBox();
                    Next();
                    if (_dataList.Count <= _lineNumber)
                    {
                        return false;
                    }
                    return true;
                }
                //通常時の処理
                BranchCheck();
            }
            return true;
        }

        /// <summary>
        /// DataTypeから次の処理を決める
        /// </summary>
        private void BranchCheck()
        {
            //次のテキストを読み込んで適切な処理をする
            switch (_dataList[_lineNumber].DataType)
            {
                case TextDataType.Text:
                    _typeTextCoroutine = StartCoroutine(TypeText(_dataList[_lineNumber].Text));
                    break;
                case TextDataType.Question:
                    _selectMode = true;
                    _questionIndentation++;
                    //テキストボックス初期化
                    selectionText.text = string.Empty;
                    if (resetForQuestion)
                        mainText.text = string.Empty;
                    selectNumber = 0;
                    _typeTextCoroutine = StartCoroutine(TypeText(_dataList[_lineNumber].Text, (SelectorShow)));
                    break;
                case TextDataType.Branch:
                case TextDataType.QEnd:
                    FindOutsideTheQuestion();
                    break;
                case TextDataType.TextEnd:
                    _lineNumber = _dataList.Count;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// テキストを表示するコルーチン。
        /// </summary>
        /// <param name="text">表示したいテキスト</param>
        /// <param name="action">テキスト表示後に実行される</param>
        /// <returns></returns>
        private IEnumerator TypeText(string text, Action action = null)
        {
            _movingCoroutine = true;
            //何行あるかを調べ、新規で行が追加されることを念頭に基底行数より多ければ一行目を削除して繰り上げする。
            var s = mainText.text.Split('\n').ToList();
            var sb = new StringBuilder();
            if (s.Count > line)
            {
                s.RemoveAt(0);
            }

            sb.Append(string.Join("\n", s));

            mainText.maxVisibleCharacters = sb.Length;
            sb.Append(text);
            sb.Append("\n");
            mainText.text = sb.ToString();
            if (displayCharOneByOne)
            {
                //一文字づつ表示
                for (int i = 0; i < text.Length; i++)
                {
                    mainText.maxVisibleCharacters++;
                    yield return new WaitForSeconds(writeSpeed);
                }
            }
            else
            {
                //一気に表示
                mainText.maxVisibleCharacters = mainText.GetParsedText().Length;
            }

            //完了後の処理
            _movingCoroutine = false;
            _lineNumber++;
            action?.Invoke();
            _typeTextCoroutine = null;
        }

        /// <summary>
        /// 選択肢を表示するのに使用
        /// </summary>
        private void SelectorShow()
        {
            SelectionBox(true);
            selectionText.text = string.Empty;
            _choices.Clear();
            for (int i = _lineNumber; i < _dataList.Count; i++)
            {
                if (_dataList[i].DataType == TextDataType.Branch)
                {
                    _choices.Add((_dataList[i].Text, i));
                }
                else if (_dataList[i].DataType == TextDataType.QEnd || _dataList[i].DataType == TextDataType.Question)
                {
                    break;
                }
            }
            selectionText.text = string.Join("\n", _choices.Select(x => x.Item1));
            _lineNumber++;
        }

        private void FindOutsideTheQuestion()
        {
            Debug.Log(_questionIndentation);
            while (_questionIndentation > 0)
            {
                if (_dataList[_lineNumber].DataType == TextDataType.QEnd)
                {
                    _questionIndentation--;
                }
                _lineNumber++;
            }
            Next();
        }

        /// <summary>
        /// テキストの中の名前を設定する
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns></returns>
        private List<TextData> TextUpdate(List<TextData> dataList)
        {
            const string pattern = @"/name(\d)";
            foreach (var t in dataList)
            {
                var matches = Regex.Matches(t.Text, pattern);
                var replacementText = t.Text;
                for (var j = 0; j < matches.Count; j++)
                {
                    replacementText =
                        replacementText.Replace(matches[j].ToString(), names[int.Parse(matches[j].Value)]);
                }

                t.Text = replacementText;
            }

            return dataList;
        }

        /// <summary>
        /// テキストボックスを表示非表示する
        /// </summary>
        /// <param name="show"></param>
        protected virtual void TextBox(bool show = false)
        {
            mainText.gameObject.SetActive(show);
            mainTextImage.gameObject.SetActive(show);
        }

        /// <summary>
        /// セレクトボックスを表示非表示する
        /// </summary>
        /// <param name="show"></param>
        protected virtual void SelectionBox(bool show = false)
        {
            selectionText.gameObject.SetActive(show);
            selectionTextImage.gameObject.SetActive(show);
        }
    }
}