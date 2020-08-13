using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

namespace MultyFramework
{
    public partial class MultyFrameworkWindow : EditorWindow
    {
        class Styles
        {
            public static GUIStyle box = "box"; 
            public static GUIStyle in_title = new GUIStyle("IN Title") { fixedHeight = toolbarHeight + 5 };
            public static GUIStyle toolbarSeachTextFieldPopup = "ToolbarSeachTextFieldPopup";
            public static GUIStyle searchTextField = new GUIStyle("ToolbarTextField")
            {
                margin = new RectOffset(0, 0, 2, 0)
            };
            public static GUIStyle searchCancelButton = "ToolbarSeachCancelButton";
            public static GUIStyle searchCancelButtonEmpty = "ToolbarSeachCancelButtonEmpty";
            public static GUIStyle foldout = "Foldout";
            public static GUIStyle toolbar = new GUIStyle("Toolbar") { fixedHeight = toolbarHeight };
            public static GUIStyle toolbarDropDown = "ToolbarDropDown";
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
            Tools,
            WebCollection,
            InProject,
            ReadMe,
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
                            if (EditorWindow.focusedWindow!=null)
                            {
                                EditorWindow.focusedWindow.Repaint();
                            }
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
        private List<PanelGUIDrawer> GetMultyFrameworkTools(IEnumerable<Type> types)
        {
            return types
                .Where((type) => { return !type.IsAbstract && type.IsSubclassOf(typeof(MultyFrameworkDrawer)) && type != typeof(WebCollection); })
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
        public float secondwidth;

        private void SplitSecondView(Rect rect)
        {
            secondwidth = rect.width;
            GUI.BeginClip(rect);
            rect.Set(0, 0, rect.width, rect.height);
            rect = rect.Zoom(AnchorType.UpperCenter, -20);
            switch (_windowSelectType)
            {
                case WindowSelectType.ReadMe:
                    break;
                case WindowSelectType.WebCollection:
                case WindowSelectType.InProject:
                case WindowSelectType.Tools:

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
                            show = drawer.author.ToLower().Contains(search.ToLower());
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
            LeftSelectView(search, multyDrawersInfo.collection);
        }
        private void InProjectLeftView(string search)
        {
            LeftSelectView(search, multyDrawersInfo.inProject);
        }

        private void OnSelectWindowTypeChange(WindowSelectType value)
        {
            _selectDrawer = null;
            switch (value)
            {
                case WindowSelectType.ReadMe:
                    webview.LoadURL(url);
                    break;
                case WindowSelectType.InProject:
                case WindowSelectType.Tools:
                case WindowSelectType.WebCollection:
                    HideWebView();
                    break;
                default:
                    break;
            }
        }




        private List<PanelGUIDrawer> _tools;

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

        public MultyFrameworkDrawersInfo multyDrawersInfo = new MultyFrameworkDrawersInfo();

        private PanelGUIDrawer __selectDrawer;

        private SearchType _searchType;
        private SplitView _splitView;
        private WindowSelectType __windowSelectType;
        private string _search;

        private const float searchTxtWith = 300;
        private const float toolbarHeight = 20;
        private const float gap = 10;
    }
    partial class MultyFrameworkWindow
    {


        public WebViewHook webview;
        public string url;
        private void ReadMe(Rect rect)
        {
            if (GUI.Button(new Rect(rect.x, rect.y, 25, 20), "<"))
                webview.Back();
            if (GUI.Button(new Rect(rect.x + 25, rect.y, 25, 20), ">"))
                webview.Forward();
            if (GUI.Button(new Rect(rect.x + 50, rect.y, 25, 20), EditorGUIUtility.IconContent("refresh")))
                webview.Reload();
            if (GUI.Button(new Rect(rect.x + 75, rect.y, 25, 20), "→"))
                webview.LoadURL(url);
            url = GUI.TextField(new Rect(rect.x + 100, rect.y, rect.width - 100, 20), url);
            if (Event.current.type == EventType.Repaint)
            {
                webview.OnGUI(rect.Zoom(AnchorType.LowerCenter, new Vector2(0, -20))); 
            }
        }
        public void HideWebView()
        {
            if (webview)
            {
                webview.Detach();
            }
        }




        private void OnEnable()
        {
            MultyFrameworkEditorTool.window = this;
            if (_splitView == null)
            {
                _splitView = new SplitView();
            }
            _splitView.fistPan = SplitFirstView;
            _splitView.secondPan = SplitSecondView;
            var types = GetTypes();
            _tools = GetMultyFrameworkTools(types).Concat(GetTools(types)).ToList();
            _tools.ForEach((f) => { f.Awake(); });
            multyDrawersInfo.FreshDrawers();
        }

        private void OnDisable()
        {
            _tools.ForEach((f) => { f.OnDestroy(); });
            _selectDrawer = null;
        }
        private void OnGUI()
        {
            {
                GUILayout.BeginHorizontal(Styles.toolbar, GUILayout.Width(position.width));
                _windowSelectType = (WindowSelectType)EditorGUILayout.EnumPopup(_windowSelectType, Styles.toolbarDropDown);
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
                    Rect r = GUILayoutUtility.GetLastRect();

                    ReadMe(new Rect(new Rect(new Vector2(0, r.yMax),
                          new Vector2(position.width, position.height - r.height))));
                }
                else
                {
                    Rect r = GUILayoutUtility.GetLastRect();
                    _splitView.OnGUI(new Rect(new Vector2(0, r.yMax),
                          new Vector2(position.width, position.height - r.height)));
                }
            }
           // Repaint();
        }

        void OnDestroy()
        {
            DestroyImmediate(webview);
            MultyFrameworkEditorTool.window = null;
        }
    }
}
