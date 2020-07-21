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
        private WebViewHook _webView { get { return window.webview; } }
        private int __index;

        public override void Awake()
        {
            _names = urlMap.Keys.ToArray();
            _urls = urlMap.Values.ToArray();
          
        }

        public override void OnDisable()
        {
            window.HideWebView();
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
        public override void OnEnable()
        {
            __index = -1;
            _index = 0;
        }

        private void ReadMe(Rect rect)
        {
            //if (_webView.Hook(window))
            //    _webView.LoadURL(_urls[_index]);

            if (Event.current.type == EventType.Repaint)
            {
                _webView.OnGUI(rect);
            }
        }

        protected override void ToolGUI()
        {
            GUILayout.BeginHorizontal(Styles.toolbar);
            _index = EditorGUILayout.Popup(_index, _names, Styles.toolbarDropDown);
            if (GUILayout.Button("<", Styles.toolBarBtn))
                _webView.Back();
            if (GUILayout.Button(">", Styles.toolBarBtn))
                _webView.Forward();
            if (GUILayout.Button(Contents.refresh, Styles.toolBarBtn))
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

