using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using System.IO;

namespace MutiFramework
{
    public partial class MutiFrameworkWindow : EditorWindow
    {
        class WebCollection : MutiFrameworkDrawer
        {
            private string _name;
            private string _author;


            private CollectionInfo.Version _current { get { return versions[_versionSelect]; } }
            public CollectionInfo.Version[] versions;

            public string unityVersion { get { return _current.unityVersion; } }

            public bool exist { get { return Directory.Exists(assetPath); } }

            public override string name { get { return _name; } }
            public override string version { get { return _current.version; } }
            public override string author { get { return _author; } }
            public override string describtion { get { return _current.describtion; } }
            public string assetPath { get { return _current.assetPath; } }

            public override string helpurl
            {
                get
                {
                    if (string.IsNullOrEmpty(_current.helpurl))
                        return base.helpurl;
                    return _current.helpurl;
                }
            }
            public override string[] dependences { get { return _current.dependences; } }

            public WebCollection( string name, string author, CollectionInfo.Version[] versions)
            {
                _name = name;
                this.versions = versions;
                _author = author;
                _versionSelect = 0;
            }
            private int _versionSelect;
            private string _diskversion;
            private bool _describtionFold = true;
            private bool _dependencesFold = true;
            private Vector2 _scroll;
            private string[] _versionNames;

            public override void OnEnable()
            {
                _diskversion = string.Empty;
                _versionNames = new string[versions.Length];
                for (int i = 0; i < versions.Length; i++)
                {
                    _versionNames[i] ="v "+ versions[i].version;
                }
                if (exist)
                {
                    _diskversion = MutiFrameworkEditorTool.ReadDiskVersion(assetPath);
                }
            }

            public override void OnGUI(Rect rect)
            {
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

                        using (new EditorGUI.DisabledScope(_diskversion==version))
                        {
                            if (GUILayout.Button("Install",Styles.buttonLeft))
                            {
                                InstallPakage();
                            }
                        }
                        _versionSelect = EditorGUILayout.Popup(_versionSelect, _versionNames, new GUIStyle("Popup")
                        {
                            margin = new RectOffset(2, 0, 3, 2)
                        }, GUILayout.Width(Contents.gap * 7));
                        using (new EditorGUI.DisabledScope(!exist))
                        {
                            if (GUILayout.Button("Remove"))
                            {
                                RemovePakage(assetPath);
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

                        _dependencesFold = EditorGUI.Foldout(last, _dependencesFold, "");
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
            private void InstallPakage()
            {
                MutiFrameworkEditorTool.RemovePakage(assetPath);
                string path = $"{MutiFrameworkEditorTool.rootPath}/{name}_{version}.unitypackage";
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                HttpPkg.DownloadPkg(name, version, path, () =>
                {
                    AssetDatabase.ImportPackage(path, true);
                    window.FreshInPorject();
                    OnEnable();
                });
            }
            protected virtual void RemovePakage(string path)
            {
                MutiFrameworkEditorTool.RemovePakage(path);
                window.FreshInPorject();
                OnEnable();
            }
            public static implicit operator WebCollection(CollectionInfo info)
            {
                return new WebCollection(info.name, info.author, info.versions);
            }
            public static implicit operator CollectionInfo(WebCollection drawer)
            {
                return new CollectionInfo(drawer.name, drawer.author, drawer.versions);
            }
        }
        class Styles
        {
            public static GUIStyle box = "box";
            public static GUIStyle in_title = new GUIStyle("IN Title") { fixedHeight = toolbarHeight + 5 };
            public static GUIStyle settingsHeader = "SettingsHeader";
            public static GUIStyle header = "DD HeaderStyle";
            public static GUIStyle toolbarSeachTextFieldPopup = "ToolbarSeachTextFieldPopup";
            public static GUIStyle searchTextField = new GUIStyle("ToolbarTextField")
            {
                margin = new RectOffset(0, 0, 2, 0)
            };
            public static GUIStyle searchCancelButton = "ToolbarSeachCancelButton";
            public static GUIStyle searchCancelButtonEmpty = "ToolbarSeachCancelButtonEmpty";
            public static GUIStyle foldout = "Foldout";
            public static GUIStyle toolbar = new GUIStyle("Toolbar") { fixedHeight = toolbarHeight };
            public static GUIStyle ToolbarDropDown = "ToolbarDropDown";
            public static GUIStyle selectionRect = "SelectionRect";

        }
        public enum SearchType
        {
            Name,
            Author,
            Describtion,
            Dependence
        }
        private enum WindowSelectType
        {
            ReadMe,
            InProject,
            Tools,
            WebCollection,
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
        private List<PanelGUIDrawer> GetTools(IEnumerable<Type> types)
        {
            return types
                .Where((type) => { return !type.IsAbstract && type.IsSubclassOf(typeof(ToolDrawer)); })
                .Select((type) => { return Activator.CreateInstance(type) as PanelGUIDrawer; })
                .ToList();
        }
        private List<PanelGUIDrawer> GetMutiFrameworkTools(IEnumerable<Type> types)
        {
            return types
                .Where((type) => { return !type.IsAbstract && type.IsSubclassOf(typeof(MutiFrameworkDrawer)) &&type !=typeof(WebCollection);  })
                .Select((type) => { return Activator.CreateInstance(type) as PanelGUIDrawer; })
                .ToList();
        }




        private void SplitFirstView(Rect rect)
        {
            rect = rect.Zoom(AnchorType.MiddleCenter, -5);
            GUI.BeginClip(rect);
            GUILayout.BeginArea(rect);
            switch (_windowSelectType)
            {
                case WindowSelectType.ReadMe:
                    break;
                case WindowSelectType.InProject:
                    InProjectLeftView(_search);
                    break;
                case WindowSelectType.WebCollection:
                    CollectionLeftView(_search);
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
                    break;
                case WindowSelectType.WebCollection:
                case WindowSelectType.InProject:
                case WindowSelectType.Tools:
                    if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.sceneViewState.SetAllEnabled(true);
                    if (Event.current.type== EventType.Repaint)
                    {

                    Graphics.DrawTexture(rect, _tx, _mat);
                    }

                    RightSelectView(rect);
                    break;
                default:
                    break;
            }
            GUI.EndClip();
        }

        private void LeftSelectView(string search, List<PanelGUIDrawer> guis)
        {
            if (guis == null || guis.Count == 0) return;
            for (int i = 0; i < guis.Count; i++)
            {
                var drawer = guis[i];
                bool show = string.IsNullOrEmpty(search);
                if (!show)
                {
                    switch (_searchType)
                    {
                        case SearchType.Name:
                            show = drawer.name.ToLower().Contains(search.ToLower());
                            break;
                        case SearchType.Author:
                            show =  drawer.author.ToLower().Contains(search.ToLower());
                            break;
                        case SearchType.Describtion:
                            show = drawer.describtion.ToLower().Contains(search.ToLower());
                            break;
                        case SearchType.Dependence:
                            if (drawer.dependences != null)
                            {
                                for (int j = 0; j < drawer.dependences.Length; j++)
                                {
                                    if (drawer.dependences[j].ToLower().Contains(search.ToLower()))
                                    {
                                        show = true;
                                        break;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }

                }
                if (!show) continue;
                GUILayout.BeginHorizontal(Styles.in_title);
                GUILayout.Label(drawer.name);
                GUILayout.FlexibleSpace();
                GUILayout.Label("v " + drawer.version);
                GUILayout.EndHorizontal();
                Rect rect = GUILayoutUtility.GetLastRect();
                if (_selectDrawer == drawer)
                {
                    GUI.Box(rect, "", Styles.selectionRect);
                }
                if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
                {
                    _selectDrawer = drawer;
                    Event.current.Use();
                }
                GUILayout.Label("", Styles.in_title, GUILayout.Height(0));
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
        private void CollectionLeftView(string search)
        {
            LeftSelectView(search, _collection);
        }
        private void InProjectLeftView(string search)
        {
            LeftSelectView(search, _inProject);
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
                case WindowSelectType.InProject:
                    break;
                case WindowSelectType.Tools:
                    break;
                case WindowSelectType.WebCollection:
                  //  _collection = GetCollection();
                    break;
                default:
                    break;
            }
        }


        private List<PanelGUIDrawer> _tools;
        private List<PanelGUIDrawer> _collection;
        private List<PanelGUIDrawer> _inProject;

        private WindowSelectType _windowSelectType
        {
            get { return __windowSelectType; }
            set
            {
                if (__windowSelectType != value)
                {
                    __windowSelectType = value;
                    OnSelectWindowTypeChange(value);
                }
            }
        }
        private PanelGUIDrawer _selectDrawer
        {
            get { return __selectDrawer; }
            set
            {
                if (__selectDrawer != value)
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
        private PanelGUIDrawer __selectDrawer;

        private SearchType _searchType;
        private SplitView _splitView;
        private WindowSelectType __windowSelectType;
        private string _search;

        private const float searchTxtWith = 300;
        private const float toolbarHeight = 20;
        private const float gap = 10;
    }
    partial class MutiFrameworkWindow
    {


        private WebViewHook _webView;
        private string _url;
        private void ReadMe(Rect rect)
        {
            // hook to this window
            if (_webView.Hook(this))
                // do the first thing to do
                _webView.LoadURL(_url);

            // Navigation
            if (GUI.Button(new Rect(rect.x, rect.y, 25, 20), "<"))
                _webView.Back();
            if (GUI.Button(new Rect(rect.x + 25, rect.y, 25, 20), ">"))
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
                _webView.OnGUI(rect.Zoom(AnchorType.LowerCenter, new Vector2(0, -20)));
            }
        }
        public void HideWebView()
        {
            if (_webView)
            {
                _webView.Detach();
            }
        }




        private void OnEnable()
        {
            PanelGUIDrawer.window = this;
            _url = MutiFrameworkEditorTool.frameworkUrl;
            this.titleContent = new GUIContent("MutiFramework");
            if (!_webView)
            {
                _webView = CreateInstance<WebViewHook>();
            }
            if (_splitView == null)
            {
                _splitView = new SplitView();
            }
            _splitView.fistPan = SplitFirstView;
            _splitView.secondPan = SplitSecondView;
            var types = GetTypes();
            _tools = GetMutiFrameworkTools(types).Concat(GetTools(types)).ToList();
            _tools.ForEach((f) => { f.Awake(); }); 

            if (!needReload)
            {
                _collection = _colletionInfos.ConvertAll((info) => {
                    WebCollection drawer = info;
                    return drawer as PanelGUIDrawer;
                });
                FreshInPorject();
                _collection.ForEach((f) => { f.Awake(); });
            }


            _mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/MutiFramework/Editor/Shader/Unlit_Water.mat"); 
            _tx= AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MutiFramework/Editor/Shader/Gamemap.png");
        }
        public void FreshInPorject()
        {
            _inProject = _collection.FindAll((_c) =>
            {
                return (_c as WebCollection).exist;
            });
        }
        public bool needReload { get { return _colletionInfos == null ; } }
        private List<CollectionInfo> _colletionInfos;

        public void FreshCollection(List<CollectionInfo> infos)
        {
            _collection= infos.ConvertAll((info) => {
                WebCollection drawer = info;
                return drawer as PanelGUIDrawer;
            });
            FreshInPorject();
            _collection.ForEach((f) => { f.Awake(); });
        }

        private void OnDisable()
        {
            _tools.ForEach((f) => { f.OnDestroy(); });
            _selectDrawer = null;
            if (_collection!=null)
            {
                _collection.ForEach((f) => { f.OnDestroy(); });
                _colletionInfos = _collection.ConvertAll((d) =>
                {
                    WebCollection _c = d as WebCollection;
                    CollectionInfo info = _c;
                    return info;
                });
            }
        }
        private void OnGUI()
        {
            {
                GUILayout.BeginHorizontal(Styles.toolbar, GUILayout.Width(position.width));
                _windowSelectType = (WindowSelectType)EditorGUILayout.EnumPopup(_windowSelectType, Styles.ToolbarDropDown);
                GUILayout.FlexibleSpace();

                if (_windowSelectType != WindowSelectType.ReadMe)
                {
                    _searchType = (SearchType)EditorGUILayout.EnumPopup(_searchType, Styles.toolbarSeachTextFieldPopup, GUILayout.Width(gap + 5));
                    _search = GUILayout.TextField(_search, Styles.searchTextField, GUILayout.Width(searchTxtWith));
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

                }
                GUILayout.Label(DateTime.Now.ToLongTimeString(), Styles.selectionRect);

                GUILayout.EndHorizontal();
            }
            {
                if (_windowSelectType == WindowSelectType.ReadMe)
                {
                    ReadMe(new Rect(new Vector2(0, toolbarHeight), position.size));
                }
                else
                {
                    Rect r = GUILayoutUtility.GetLastRect();
                    _splitView.OnGUI(new Rect(new Vector2(0, r.yMax),
                          new Vector2(position.width, position.height - r.height)));
                }
            }
            Repaint();
        }
        private Texture2D _tx;
        private Material _mat;
        void OnDestroy()
        {
            DestroyImmediate(_webView);
        }
    }
}
