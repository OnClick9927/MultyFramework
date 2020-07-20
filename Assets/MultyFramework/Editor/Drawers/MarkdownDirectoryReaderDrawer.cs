using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace MultyFramework
{
    public class MarkdownDirectoryReaderDrawer : MarkdownGUIDrawer
    {
        private enum ShowType
        {
            Black,
            White,
        }
        private ShowType __showType;
        private string __dir;

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
        public override string name { get { return "Markdown Directory Reader"; } }
        public override string version { get { return "0.0.0.1"; } }
        public override string author { get { return "OnClick"; } }

        public override string describtion
        {
            get
            {
                return "you can read markdown \n" +
                    "by this tool \n" +
                    " that in a Directory\n";
            }
        }
        private string FinalTxt()
        {
            string content = UnityEngine.Networking.UnityWebRequest.EscapeURL("", Encoding.UTF8)
                .Replace("+", "%20");
            try
            {
                content = UnityEngine.Networking.UnityWebRequest.EscapeURL(_editorTxt, Encoding.UTF8)
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
        private string _dir
        {
            get { return __dir; }
            set
            {
                __dir = value;
                _index = 0;
                _editorTxt = "";
                _paths = Directory.GetFiles(value, "*.md", SearchOption.AllDirectories);
                if (_paths != null && _paths.Length > 0)
                {
                    _names = _paths.ToList().ConvertAll((path) =>
                    {
                        return Path.GetFileNameWithoutExtension(path);
                    }).ToArray();
                    _editorTxt = File.ReadAllText(_paths[0]);
                }
                _webView.LoadHTML(FinalTxt());
            }
        }
        private string[] _paths;
        private string[] _names;
        private int __index;
        private int _index
        {
            get { return __index; }
            set
            {
                if (__index != value)
                {
                    __index = value;
                    _editorTxt = File.ReadAllText(_paths[_index]);
                    _webView.LoadHTML(FinalTxt());
                }
            }
        }
        private float width;
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
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("refresh")), Styles.toolBarBtn))
            {
                _webView.LoadHTML(FinalTxt());
            }
            if (GUILayout.Button("Load", Styles.toolBarBtn))
            {
                var path = EditorUtility.OpenFolderPanel("Load MarkDown From Disk", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    _dir = path;
                }
            }
            _showType = (ShowType)EditorGUILayout.EnumPopup("", _showType, Styles.ToolbarDropDown, GUILayout.Width(Contents.gap * 5));
            GUILayout.FlexibleSpace();
            if (_paths != null && _paths.Length != 0)
            {
                _index = EditorGUILayout.Popup(_index, _names, Styles.ToolbarDropDown);
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
                ReadMe(r);

            }

        }
        private void ReadMe(Rect rect)
        {
            if (_webView.Hook(window))
            {
                _webView.LoadHTML(FinalTxt());
            }
            GUI.SetNextControlName("urlfield");
            var ev = Event.current;
            if (ev.isKey && GUI.GetNameOfFocusedControl().Equals("urlfield"))
                if (ev.keyCode == KeyCode.Return)
                {
                    _webView.LoadURL(helpurl);
                    GUIUtility.keyboardControl = 0;
                    _webView.SetApplicationFocus(true);
                    ev.Use();
                }
            if (Event.current.type == EventType.Repaint)
            {
                _webView.OnGUI(rect);
            }
        }
    }
}

