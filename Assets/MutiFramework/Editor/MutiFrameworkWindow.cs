#if MutiFramework
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

namespace MutiFramework
{
    partial class MutiFrameworkWindow : EditorWindow
    {
        class CollectionDrawer : DescriptionGUIDrawer
        {
            private string _name;
            private string _version;
            private string _author;
            private string _describtion;
            private string _assetPath;
            private string[] _dependences;
            private string _helpurl;
            private string _unityVersion;
            private string _downloadUrl;

            public string unityVersion { get { return _unityVersion; } }
            public string downloadUrl { get { return _downloadUrl; } }

            public override string name { get { return _name; } }
            public override string version { get { return _version; } }
            public override string author { get { return _author; } }
            public override string describtion { get { return _describtion; } }
            public override string assetPath { get { return _assetPath; } }
            public override string helpurl
            {
                get
                {
                    if (string.IsNullOrEmpty(_helpurl))
                        return base.helpurl;
                    return _helpurl;
                }
            }
            public override string[] dependences { get { return _dependences; } }

            public CollectionDrawer(string name, string version, string author, string describtion, string assetPath, string[] dependences, string helpurl, string unityVersion,string downloadUrl)
            {
                _name = name;
                _version = version;
                _author = author;
                _describtion = describtion;
                _assetPath = assetPath;
                _dependences = dependences;
                _helpurl = helpurl;
                _unityVersion = unityVersion;
                _downloadUrl = downloadUrl;
            }

            private bool _describtionFold = true;
            private bool _dependencesFold = true;
            private Vector2 _scroll;

            public override void OnGUI(Rect rect)
            {
                base.OnGUI(rect);
                GUILayout.BeginArea(rect);
                {
                    _scroll = GUILayout.BeginScrollView(_scroll);
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(name, Styles.header);
                        if (GUILayout.Button(Contents.help, Styles.controlLabel))
                        {
                            Help.BrowseURL(helpurl);
                        }
                        GUILayout.Space(10);
                        GUILayout.Label(unityVersion);
                        GUILayout.FlexibleSpace();

                        bool exist = System.IO.File.Exists(assetPath);
                        if (exist)
                        {
                            if (GUILayout.Button("Install Again"))
                            {
                                InstallPakageAgain(this);
                            }
                            if (GUILayout.Button("Remove"))
                            {
                                RemovePakage(assetPath);
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Install"))
                            {
                                InstallPakage(this);
                            }
                            using (new EditorGUI.DisabledScope(true))
                            {
                                if (GUILayout.Button("Remove"))
                                {
                                    RemovePakage(assetPath);
                                }
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.Label("Version  " + version, Styles.boldLabel);
                    GUILayout.Label("Author " + author, Styles.boldLabel);

                    {
                        GUILayout.Label("Dependences", Styles.in_title);
                        Rect last = GUILayoutUtility.GetLastRect();
                        last.width -= 10;
                        last.xMin += 10;
                        if (Event.current.type == EventType.MouseUp && last.Contains(Event.current.mousePosition))
                        {
                            _dependencesFold = !_dependencesFold;
                            Event.current.Use();
                        }
                        last.xMin -= 10;
                        last.width = 10;

                        _dependencesFold = UnityEditor.EditorGUI.Foldout(last, _dependencesFold, "");
                        if (_dependencesFold)
                        {
                            if (dependences != null)
                            {
                                for (int i = 0; i < dependences.Length; i++)
                                {
                                    GUILayout.Label(dependences[i]);
                                }
                            }
                        }
                    }
                    {
                        GUILayout.Label("Describtion ", Styles.in_title);
                        Rect last = GUILayoutUtility.GetLastRect();
                        last.width -= 10;
                        last.xMin += 10;
                        if (Event.current.type == EventType.MouseUp && last.Contains(Event.current.mousePosition))
                        {
                            _describtionFold = !_describtionFold;
                            Event.current.Use();
                        }
                        last.xMin -= 10;
                        last.width = 10;
                        _describtionFold = EditorGUI.Foldout(last, _describtionFold, "");
                        if (_describtionFold)
                        {
                            GUILayout.Label(describtion);
                        }
                    }
                    GUILayout.Label("", Styles.in_title, GUILayout.Height(0));
                    GUILayout.EndScrollView();
                }
                GUILayout.EndArea();
            }

            private void InstallPakageAgain(CollectionInfo info)
            {
                MutiFrameworkWindowUtil.InstallPakageAgain(info);
            }

            private void InstallPakage(CollectionInfo info)
            {
                MutiFrameworkWindowUtil.InstallPakage(info);
            } 
            protected virtual void RemovePakage(string path)
            {
                MutiFrameworkWindowUtil.RemovePakage(path);
            }
            public static implicit operator CollectionDrawer(CollectionInfo info)
            {
                return new CollectionDrawer(info.name, info.version, info.author, info.describtion, info.assetPath, info.dependences, info.helpurl, info.unityVersion,info.downloadUrl);
            }
            public static implicit operator CollectionInfo(CollectionDrawer drawer)
            {
                return new CollectionInfo(drawer.name, drawer.version, drawer.author, drawer.describtion, drawer.assetPath, drawer.dependences, drawer.helpurl, drawer.unityVersion,drawer.downloadUrl);
            }
        }
        class Styles
        {
            public static GUIStyle box = "box";
            public static GUIStyle in_title = new GUIStyle("IN Title") { fixedHeight=25};
            public static GUIStyle settingsHeader = "SettingsHeader";
            public static GUIStyle header = "DD HeaderStyle";
            public static GUIStyle searchTextField = "SearchTextField";
            public static GUIStyle searchCancelButton = "SearchCancelButton";
            public static GUIStyle searchCancelButtonEmpty = "SearchCancelButtonEmpty";
            public static GUIStyle foldout = "Foldout";
            public static GUIStyle toolbar = new GUIStyle("Toolbar") { fixedHeight = 20 }; 
            public static GUIStyle ToolbarDropDown = "ToolbarDropDown";
            public static GUIStyle selectionRect = "SelectionRect";

        }

        private enum WindowSelectType
        {
            ReadMe,
            FrameworksInProject,
            Tools,
            FrameworkCollection,
        }
        [Serializable]
        public class SplitView
        {
            private SplitType _splitType = SplitType.Vertical;
            private float _split = 200;
            public Action<Rect> fistPan, secondPan;
            public event Action onBeginResize;
            public event Action onEndResize;
            public bool dragging
            {
                get { return _resizing; }
                private set
                {
                    if (_resizing != value)
                    {
                        _resizing = value;
                        if (value)
                        {
                            if (onBeginResize != null)
                            {
                                onBeginResize();
                            }
                        }
                        else
                        {
                            if (onEndResize != null)
                            {
                                onEndResize();
                            }
                        }
                    }
                }
            }
            private bool _resizing;
            public void OnGUI(Rect position)
            {
                var rs = position.Split(_splitType, _split, 4); 
                var mid = position.SplitRect(_splitType, _split, 4);
                if (fistPan != null)
                {
                    fistPan(rs[0]);
                }
                if (secondPan != null)
                {
                    secondPan(rs[1]);
                }
                GUI.Box(mid, "");
                Event e = Event.current;
                if (mid.Contains(e.mousePosition))
                {
                    if (_splitType == SplitType.Vertical)
                        EditorGUIUtility.AddCursorRect(mid, MouseCursor.ResizeHorizontal);
                    else
                        EditorGUIUtility.AddCursorRect(mid, MouseCursor.ResizeVertical);
                }
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        if (mid.Contains(Event.current.mousePosition))
                        {
                            dragging = true;
                        }
                        break;
                    case EventType.MouseDrag:
                        if (dragging)
                        {
                            switch (_splitType)
                            {
                                case SplitType.Vertical:
                                    _split += Event.current.delta.x;
                                    break;
                                case SplitType.Horizontal:
                                    _split += Event.current.delta.y;
                                    break;
                            }
                            _split = Mathf.Clamp(_split, 100, position.width - 100);
                        }
                        break;
                    case EventType.MouseUp:
                        if (dragging)
                        {
                            dragging = false;
                        }
                        break;
                }
            }
        }
        [MenuItem("MutiFramework/Window")]
        static void OpenWindow()
        {
            GetWindow<MutiFrameworkWindow>();
        }

        private IEnumerable<Type> GetTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany((a) => { return a.GetTypes(); });
        }
        private List<DescriptionGUIDrawer> GetTools(IEnumerable<Type> types)
        {
            return types
                .Where((type) => { return !type.IsAbstract && type.IsSubclassOf(typeof(ToolDrawer)); })
                .Select((type) => { return Activator.CreateInstance(type) as DescriptionGUIDrawer; })
                .ToList();
        }
        private List<DescriptionGUIDrawer> GetFrameworksInProject(IEnumerable<Type> types)
        {
            return types
                .Where((type) => { return !type.IsAbstract && type.IsSubclassOf(typeof(FrameworkDrawer)); })
                .Select((type) => { return Activator.CreateInstance(type) as DescriptionGUIDrawer; }).ToList();

        }

        private List<DescriptionGUIDrawer> GetCollection()
        {
            return MutiFrameworkWindowUtil.GetCollections().ConvertAll((info)=> {
                CollectionDrawer drawer = info;
                return drawer as DescriptionGUIDrawer;
            });
        }


        private void SplitFirstView(Rect rect)
        {
            rect = rect.Zoom(AnchorType.MiddleCenter, -5);
            GUI.BeginClip(rect);
            GUILayout.BeginArea(rect);
            switch (_windowSelectType)
            {
                case WindowSelectType.ReadMe:
                   // DescibtionAndBaseToolLeftView();
                    break;
                case WindowSelectType.FrameworksInProject:
                    FrameworksInProjectLeftView(_search);
                    break;
                case WindowSelectType.FrameworkCollection:
                    FrameworksCollectionLeftView(_search);
                    break;
                case WindowSelectType.Tools:
                    ToolsLeftView(_search);
                    break;
                default:
                    break;
            }
            GUILayout.EndArea();
            GUI.EndClip();
        }
        private void SplitSecondView(Rect rect)
        {
            GUI.BeginClip(rect);
            rect.Set(0, 0, rect.width, rect.height);
            rect = rect.Zoom(AnchorType.MiddleCenter, -15);
            switch (_windowSelectType)
            {
                case WindowSelectType.ReadMe:
                   // DescibtionAndBaseToolRightView(rect);
                    break;
                case WindowSelectType.FrameworkCollection:
                case WindowSelectType.FrameworksInProject:
                case WindowSelectType.Tools:
                    RightSelectView(rect);
                    break;
                default:
                    break;
            }
            GUI.EndClip();
        }

        private void LeftSelectView(string search,List<DescriptionGUIDrawer> guis)
        {
            if (guis == null || guis.Count == 0) return;
            for (int i = 0; i < guis.Count; i++)
            {
                if (string.IsNullOrEmpty(search) || guis[i].name.ToLower().Contains(search.ToLower()))
                {
                    GUILayout.BeginHorizontal(Styles.in_title);
                    GUILayout.Label(guis[i].name);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("v " + guis[i].version);
                    GUILayout.EndHorizontal();
                    Rect rect = GUILayoutUtility.GetLastRect();
                    if (_selectDrawer == guis[i])
                    {
                        GUI.Box(rect, "", Styles.selectionRect);
                    }
                    if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
                    {
                        _selectDrawer = guis[i];
                        Event.current.Use();
                    }
                    GUILayout.Label("", Styles.in_title, GUILayout.Height(0));
                }
            }
        }
        private void RightSelectView(Rect rect)
        {
            if (_selectDrawer != null)
            {
                _selectDrawer.OnGUI(rect);
            }
        }

        private void ToolsLeftView(string search)
        {
            LeftSelectView(search, _tools);
        }
        private void FrameworksInProjectLeftView(string search)
        {
            LeftSelectView(search, _frameworks);
        }
        private void FrameworksCollectionLeftView(string search)
        { 
            LeftSelectView(search, _Collection );
        }

        private void OnSelectWindowTypeChange(WindowSelectType value)
        {
            _selectDrawer = null;
            if (value != WindowSelectType.ReadMe)
            {
                HideWebView();
            }
            switch (value)
            {
                case WindowSelectType.ReadMe:
                    break;
                case WindowSelectType.FrameworksInProject:
                    break;
                case WindowSelectType.Tools:
                    break;
                case WindowSelectType.FrameworkCollection:
                    _Collection = GetCollection();
                    break;
                default:
                    break;
            }
        }


        private List<DescriptionGUIDrawer> _frameworks;
        private List<DescriptionGUIDrawer> _tools;
        private List<DescriptionGUIDrawer> _Collection;

        private WindowSelectType _windowSelectType
        {
            get { return __windowSelectType; }
            set
            {
                if (__windowSelectType!=value)
                {
                    __windowSelectType = value;
                    OnSelectWindowTypeChange(value);
                }
            }
        }
        private DescriptionGUIDrawer _selectDrawer { get { return __selectDrawer; }
            set {
                if (__selectDrawer!=value)
                {
                    if (__selectDrawer != null)
                    {
                        __selectDrawer.OnDisable();
                    }
                    __selectDrawer = value;
                    if (__selectDrawer != null)
                    {
                        __selectDrawer.OnEnable();
                    }
                }
            }
        }
        private DescriptionGUIDrawer __selectDrawer;

        [SerializeField] private SplitView _splitView;
        [SerializeField] private WindowSelectType __windowSelectType;
        [SerializeField] private Vector2 _scroll;
        [SerializeField] private string _search;
        private bool _fold0;
        private bool _fold1;



        private const float searchTxtWith = 200;
        private const float btnWith=20;
        private const float gap = 10;
    }
    partial class MutiFrameworkWindow
    {

        private void BaseView()
        {
            GUILayout.Label("1、write custom Framework class extends Framework/UpdateFramework with Attribute(Framework)\n" +
                            "2、write custom Framework GUI extends FrameworkLineDrawer if you need\n" +
                            "3、write custom Tool GUI extends ToolLineDrawer if you need\n" +
                            "4、click update Script Button and wait for seconds", Styles.settingsHeader);
            GUILayout.Space(gap);
            if (GUILayout.Button("Update Script"))
            {
                if (!EditorApplication.isCompiling)
                {
                    MutiFrameworkWindowUtil.CreateClass();
                }
            }
        }
        private void WebView(Rect rect)
        {
            // hook to this window
            if (_webView.Hook(this))
                // do the first thing to do
                _webView.LoadURL(_url);

            // Navigation
            if (GUI.Button(new Rect(rect.x, rect.y, 25, 20), "<"))
                _webView.Back();
            if (GUI.Button(new Rect(rect.x+25, rect.y, 25, 20), ">"))
                _webView.Forward();
            if (GUI.Button(new Rect(rect.x + 50, rect.y, 25, 20), new GUIContent(EditorGUIUtility.IconContent("refresh"))))
                _webView.Reload();
            if (GUI.Button(new Rect(rect.x + 75, rect.y, 25, 20), "→"))
                _webView.LoadURL(_url);
            _url = GUI.TextField(new Rect(rect.x + 100, rect.y, rect.width - 100, 20), _url);

            // URL text field
            GUI.SetNextControlName("urlfield");
            var ev = Event.current;

            // Focus on web view if return is pressed in URL field
            if (ev.isKey && GUI.GetNameOfFocusedControl().Equals("urlfield"))
                if (ev.keyCode == KeyCode.Return)
                {
                    _webView.LoadURL(_url);
                    GUIUtility.keyboardControl = 0;
                    _webView.SetApplicationFocus(true);
                    ev.Use();
                }
            //  else if (ev.keyCode == KeyCode.A && (ev.control | ev.command))


            if (ev.type == EventType.Repaint)
            {
                // keep the browser aware with resize
                _webView.OnGUI(rect.Zoom(AnchorType.LowerCenter, -20));
            }
        }
        private void DescibtionAndBaseToolView()
        {
            {
                GUILayout.Label("Descibtion and base Tool", Styles.in_title);
                var rect = GUILayoutUtility.GetLastRect();
                _fold0 = GUI.Toggle(new Rect(rect.position, new Vector2(gap, rect.height)), _fold0, "", Styles.foldout);
                if (new Rect(rect.position, new Vector2(rect.width - searchTxtWith - btnWith * 2, rect.height)).Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
                {
                    _fold0 = !_fold0;
                    Event.current.Use();
                }

                if (_fold0)
                {
                    BaseView();
                }
            }
            {
                GUILayout.Label("Learn More with MutiFramework", Styles.in_title);
                var rect = GUILayoutUtility.GetLastRect();
                _fold1 = GUI.Toggle(new Rect(rect.position, new Vector2(gap, rect.height)), _fold1, "", Styles.foldout);
                if (new Rect(rect.position, new Vector2(rect.width - searchTxtWith - btnWith * 2, rect.height)).Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
                {
                    _fold1 = !_fold1;
                    Event.current.Use();
                }

                if (_fold1) 
                {
                    rect = GUILayoutUtility.GetLastRect();

                    rect = new Rect(0, rect.yMax, position.width, position.height - rect.height);
                    GUI.BeginClip(rect);
                    WebView(new Rect(Vector2.zero, rect.size).Zoom(AnchorType.UpperCenter,new Vector2(0,-150)));
                    GUI.EndClip();
                }
                else
                {
                    HideWebView();
                }


            }
           

        }

        private WebViewHook _webView;
        private string _url = "https://www.baidu.com";
        public void HideWebView()
        {
            if (_webView)
            {
                _webView.Detach();
            }
        }
        private void OnEnable()
        {
            this.titleContent = new GUIContent("MutiFramework");
            if (!_webView)
            {
                _webView = CreateInstance<WebViewHook>();
            }
            if (_splitView==null)
            {
                _splitView = new SplitView();
            }
            _splitView.fistPan = SplitFirstView;
            _splitView.secondPan = SplitSecondView;
            var types = GetTypes();
            _frameworks = GetFrameworksInProject(types);
            _tools = GetTools(types);
            _Collection = GetCollection();
        }
        private void OnDisable()
        {
            _selectDrawer = null;
        }
        private void OnGUI()
        { 
            GUILayout.BeginHorizontal(Styles.toolbar, GUILayout.Width(position.width));
            {
                _windowSelectType = (WindowSelectType)EditorGUILayout.EnumPopup(_windowSelectType, Styles.ToolbarDropDown);
                GUILayout.FlexibleSpace();
                _search = GUILayout.TextField(_search, Styles.searchTextField, GUILayout.MinWidth(searchTxtWith));
                if (string.IsNullOrEmpty(_search))
                {
                    GUILayout.Label("", Styles.searchCancelButtonEmpty);
                }
                else
                {
                    if (GUILayout.Button("", Styles.searchCancelButton))
                    {
                        _search = string.Empty;
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (_windowSelectType== WindowSelectType.ReadMe)
            {
                DescibtionAndBaseToolView();
            }
            else
            {
                Rect r = GUILayoutUtility.GetLastRect();
                _splitView.OnGUI(new Rect(new Vector2(0, r.yMax + 2),
                      new Vector2(position.width, position.height - r.height)));
            }

            Repaint();
        }



        void OnDestroy()
        {
            //Destroy web view
            DestroyImmediate(_webView);
        }
    }
}
#endif
