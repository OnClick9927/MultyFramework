using System.IO;
using UnityEngine;

namespace MultyFramework
{
    public abstract class MarkdownGUIDrawer : ToolDrawer
    {
        protected const string flag = "#MDSTRING#";
        private const string _blackPath = "Assets/MultyFramework/Editor/Res/black.html";
        private const string _whitePath = "Assets/MultyFramework/Editor/Res/white.html";

        protected static WebViewHook _webView { get { return window.webview; } }
        private static string __white;
        private static string __black;

        protected static string _black
        {
            get
            {
                if (string.IsNullOrEmpty(__black))
                {
                    __black = File.ReadAllText(_blackPath);
                }
                return __black;
            }
        }
        protected static string _white
        {
            get
            {
                if (string.IsNullOrEmpty(__white))
                {
                    __white = File.ReadAllText(_whitePath);
                }
                return __white;
            }
        }
        protected string _editorTxt;

        public override void OnDisable()
        {
            window.HideWebView();
        }
    }
}

