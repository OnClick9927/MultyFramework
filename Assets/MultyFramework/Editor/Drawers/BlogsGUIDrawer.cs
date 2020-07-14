using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace MultyFramework
{
    public abstract class BlogsGUIDrawer : ToolDrawer
    {
        protected abstract Dictionary<string, string> urlMap { get; }
        private string[] _names;
        private string[] _urls;
        private WebViewHook _webView;
        private int __index;

        public override void Awake()
        {
            if (!_webView)
            {
                _webView = ScriptableObject.CreateInstance<WebViewHook>();
            }
            _names = urlMap.Keys.ToArray();
            _urls = urlMap.Values.ToArray();
            __index = -1;
            _index = 0;
        }
        public override void OnDestroy()
        {
            GameObject.DestroyImmediate(_webView);
        }
        public override void OnDisable()
        {
            if (_webView)
            {
                _webView.Detach();
            }
        }
        private int _index
        {
            get { return __index; }
            set
            {
                if (value != __index)
                {
                    __index = value;
                    _webView.LoadURL(_urls[value]);
                }
            }
        }

        private void ReadMe(Rect rect)
        {
            if (_webView.Hook(window))
                _webView.LoadURL(_urls[_index]);
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

        protected override void ToolGUI()
        {
            GUILayout.BeginHorizontal(Styles.toolbar);
            _index = EditorGUILayout.Popup(_index, _names, Styles.ToolbarDropDown);
            if (GUILayout.Button("<", Styles.toolBarBtn))
                _webView.Back();
            if (GUILayout.Button(">", Styles.toolBarBtn))
                _webView.Forward();
            if (GUILayout.Button(EditorGUIUtility.IconContent("refresh"), Styles.toolBarBtn))
                _webView.Reload();
            if (GUILayout.Button(Contents.help, Styles.boldLabel))
                Help.BrowseURL(_urls[_index]);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            var rect = GUILayoutUtility.GetLastRect();
            Rect r = new Rect(rect.x, rect.yMax, rect.width, window.position.height - rect.yMax - Contents.gap * 3);
            ReadMe(r);
        }
    }
}

