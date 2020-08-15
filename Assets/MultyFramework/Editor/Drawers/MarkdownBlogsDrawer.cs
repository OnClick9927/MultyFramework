using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace MultyFramework
{
    public abstract class MarkdownBlogsDrawer : MarkdownGUIDrawer
    {
        private enum ShowType
        {
            Black,
            White,
        }
        private ShowType __showType;
        private ShowType _showType
        {
            get { return __showType; }
            set
            {
                if (__showType != value)
                {
                    __showType = value;
                    _webView.LoadHTML(FinalTxt());
                }

            }
        }
        private string FinalTxt()
        {
            string content = MultyFrameworkEditorTool.EscapeURL("", Encoding.UTF8)
                .Replace("+", "%20");
            try
            {
                content = MultyFrameworkEditorTool.EscapeURL(_editorTxt, Encoding.UTF8)
                    .Replace("+", "%20");
            }
            catch
            {
            }

            // Debug.Log(content);
            switch (_showType)
            {
                case ShowType.Black:
                    content = _black.Replace(flag, content);
                    break;

                case ShowType.White:
                    content = _white.Replace(flag, content);
                    break;
                default:
                    return "";
            }

            byte[] b = Encoding.Default.GetBytes(content);
            //转成 Base64 形式的 System.String
            content = Convert.ToBase64String(b);
            return content;
        }


        protected abstract Dictionary<string, string> mardownMap { get; }

        private string[] _paths;
        private string[] _names;
        private int _index
        {
            get { return __index; }
            set
            {
                if (__index != value)
                {
                    __index = value;
                    _editorTxt = File.ReadAllText(_paths[value]);
                    _webView.LoadHTML(FinalTxt());
                }
            }
        }
        private float width;
        private int __index;

        public override void Awake()
        {
            base.Awake();
            _paths = mardownMap.Values.ToArray();
            _names = mardownMap.Keys.ToArray();
           
        }
        public override void OnEnable()
        {
            if (_paths != null && _paths.Length != 0)
            {
                __index = -1;
                _index = 0;
            }
        }

        protected override void ToolGUI()
        {
            GUILayout.BeginHorizontal(Styles.toolbar);
            if (GUILayout.Button(Contents.refresh, Styles.toolBarBtn))
            {
                _webView.LoadHTML(FinalTxt());
            }
            _showType = (ShowType)EditorGUILayout.EnumPopup("", _showType, Styles.toolbarDropDown, GUILayout.Width(Contents.gap * 5));
            GUILayout.FlexibleSpace();
            if (_paths != null && _paths.Length != 0)
            {
                _index = EditorGUILayout.Popup(_index, _names, Styles.toolbarDropDown);
            }
            GUILayout.EndHorizontal();


            if (_paths != null && _paths.Length != 0)
            {
                var rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.Repaint)
                {
                    width = rect.width;
                }
                else if (Event.current.type == EventType.Layout)
                {
                    rect.width = width - 10;
                }
                Rect r = new Rect(rect.x, rect.yMax, rect.width, window.position.height - rect.yMax - Contents.gap * 3);
                if (Event.current.type == EventType.Repaint)
                {
                    _webView.OnGUI(r);
                }
            }

        }

      
    }
}

