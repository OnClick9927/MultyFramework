using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MultyFramework
{
    public class MarkdownWriterDrawer : MarkdownGUIDrawer
    {
        private enum ShowType
        {
            EditorWithBlackPreview,
            EditorWithWhitePreview,
            JustWhitePreview,
            JustBlackPreview,
            JustEditor,
        }
        public override string name { get { return "Markdown Writer"; } }
        public override string version { get { return "0.0.0.1"; } }
        public override string author { get { return  "OnClick"; } }
        public override string describtion { get { return"you can write markdown \n" +
                    "by this tool \n" +
                    "with a little change\n"; } }
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
        public override void OnEnable()
        {
            _webView.LoadHTML(FinalTxt());
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
                case ShowType.EditorWithBlackPreview:
                    content = _black.Replace(flag, content);
                    break;
                case ShowType.JustBlackPreview:
                    content = _black.Replace(flag, content);
                    break;
                case ShowType.EditorWithWhitePreview:
                    content = _white.Replace(flag, content);
                    break;
                case ShowType.JustWhitePreview:
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

        private void ReadMe(Rect rect)
        {

            //if (_webView.Hook(window))
            //{
            //    _webView.LoadHTML(FinalTxt());
            //}

            if (Event.current.type == EventType.Repaint)
            {
                _webView.OnGUI(rect);
            }
        }
        private float width;
        protected override void ToolGUI()
        {
            GUILayout.BeginHorizontal(Styles.toolbar);
            _showType=(ShowType) EditorGUILayout.EnumPopup("", _showType,Styles.toolbarDropDown);
            if (GUILayout.Button(Contents.refresh, Styles.toolBarBtn))
            {
                _webView.LoadHTML(FinalTxt());
            }
            if (GUILayout.Button("Save",Styles.toolBarBtn))
            {
                var path=  EditorUtility.SaveFilePanel("Save md To disk", "Assets", "NewMarkDown.md", "md");
                if (path.EndsWith("md"))
                {
                    System.IO.File.WriteAllText(path, _editorTxt);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            if (GUILayout.Button("Load", Styles.toolBarBtn))
            {
                var path = EditorUtility.OpenFilePanel("Load MarkDown From Disk", "Assets", "md");
                if (!string.IsNullOrEmpty(path) && path.EndsWith("md"))
                {
                    _editorTxt= System.IO.File.ReadAllText(path);
                    _webView.LoadHTML(FinalTxt());
                }
            }
            autoRefresh= GUILayout.Toggle(autoRefresh, "Auto Refresh",Styles.toolBarBtn);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type==  EventType.Repaint)
            {
                width = rect.width;
            }
            else if (Event.current.type== EventType.Layout)
            {
                rect.width = width-10;
            }
            Rect r = new Rect(rect.x, rect.yMax, rect.width, window.position.height - rect.yMax - Contents.gap * 3);
            var rs = r.VerticalSplit(rect.width / 2);
            switch (_showType)
            {
                case ShowType.JustWhitePreview:
                    ReadMe(r);
                    break;
                case ShowType.JustBlackPreview:
                    ReadMe(r);
                    break;
                case ShowType.JustEditor:
                    window.HideWebView();
                    EditorTxt(r, ref _editorTxt);
                    break;
                case ShowType.EditorWithWhitePreview:
                    EditorTxt(rs[0], ref _editorTxt);

                    ReadMe(rs[1]);
                    break;
                case ShowType.EditorWithBlackPreview:
                    EditorTxt(rs[0], ref _editorTxt);

                    ReadMe(rs[1]);
                    break;
                default:
                    break;
            }
         
            
        }

        private Vector2 _scroll;
        private bool autoRefresh;
        
        private void EditorTxt(Rect rect,ref string str)
        {
            var ev = Event.current;
            string tmp = str;
           _scroll= GUILayout.BeginScrollView(_scroll, GUILayout.Width(rect.width), GUILayout.MaxHeight(rect.height));
            str = GUILayout.TextArea(tmp, GUILayout.Width(rect.width), GUILayout.MaxHeight(rect.height));
            GUILayout.EndScrollView();
            if (tmp != str && autoRefresh)
            {

                _webView.LoadHTML(FinalTxt());
            }
        }
    }
}

