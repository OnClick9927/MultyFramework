using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

namespace MutiFramework
{
    partial class MutiFrameworkWindow : EditorWindow
    {
        class Styles
        {
            public static GUIStyle box = "box";
            public static GUIStyle in_title = "IN Title";
            public static GUIStyle settingsHeader = "SettingsHeader";
            public static GUIStyle header = "DD HeaderStyle";
            public static GUIStyle searchTextField = "SearchTextField";
            public static GUIStyle searchCancelButton = "SearchCancelButton";
            public static GUIStyle searchCancelButtonEmpty = "SearchCancelButtonEmpty";
        }

        [MenuItem("MutiFramework/Window")]
        static void OpenWindow()
        {
            GetWindow<MutiFrameworkWindow>();
        }

        private List<ToolLineDrawer> GetTools()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany((a) => { return a.GetTypes(); });
            return types
                .Where((type) => { return !type.IsAbstract && type.IsSubclassOf(typeof(ToolLineDrawer)); })
                .Select((type) => { return Activator.CreateInstance(type) as ToolLineDrawer; }).ToList();
        }
        private List<FrameworkLineDrawer> GetFrameworksInProject()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany((a) => { return a.GetTypes(); });
            return types
                .Where((type) => { return !type.IsAbstract && type.IsSubclassOf(typeof(FrameworkLineDrawer)); })
                .Select((type) => { return Activator.CreateInstance(type) as FrameworkLineDrawer; }).ToList();

        }
        private List<FrameworkCollectionLineDrawer> GetFrameworkCollection()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany((a) => { return a.GetTypes(); });
            return types
                .Where((type) => { return !type.IsAbstract && type.IsSubclassOf(typeof(FrameworkCollectionLineDrawer)); })
                .Select((type) => { return Activator.CreateInstance(type) as FrameworkCollectionLineDrawer; }).ToList();
        }




        private void UpdateScripts()
        {
            string flag = "//ToDo";
            string txt = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/MutiFramework/Editor/EditorFrameworks.cs").text;
            string add = "";
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany((a) => { return a.GetTypes(); });
            types
                 .Where((type) => {
                     return !type.IsAbstract && type.IsSubclassOf(typeof(Framework)) &&
                         type.IsDefined(typeof(FrameworkAttribute), false) &&
                         (type.GetCustomAttributes(typeof(FrameworkAttribute), false).First() as FrameworkAttribute).env.HasFlag(Environment.Editor);
                 })
                 .Select((type) => {
                     Framework f = Activator.CreateInstance(type) as Framework;
                     f.env = Environment.Editor;
                     return f;
                 }).ToList()
                 .ForEach((f) => {
                     add = add + "\t\tpublic static " + f.GetType() + " " + f.name + "{ get { return GetFrame(\"" + f.name + "\") as " + f.GetType() + ";}} \n";
                     //_container.Subscribe(f);
                     f.Dispose();
                 });
            int first = txt.IndexOf(flag);
            int last = txt.LastIndexOf(flag);
            string _1 = txt.Substring(0, first);
            string _2 = flag + "\n" + add;
            string _3 = txt.Substring(last, txt.Length - last);

            txt = _1 + _2 + _3;
            System.IO.File.WriteAllText(Application.dataPath + "/MutiFramework/Editor/EditorFrameworks.cs", txt);



            txt = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/MutiFramework/Frameworks.cs").text;
            add = "";
            types
                 .Where((type) => {
                     return !type.IsAbstract && type.IsSubclassOf(typeof(Framework)) &&
                         type.IsDefined(typeof(FrameworkAttribute), false) &&
                         (type.GetCustomAttributes(typeof(FrameworkAttribute), false).First() as FrameworkAttribute).env.HasFlag(Environment.Runtime);
                 })
                 .Select((type) => {
                     Framework f = Activator.CreateInstance(type) as Framework;
                     f.env = Environment.Runtime;
                     return f;
                 }).ToList()
                 .ForEach((f) => {
                     add = add + "\t\tpublic static " + f.GetType() + " " + f.name + "{ get { return GetFrame(\"" + f.name + "\") as " + f.GetType() + ";}} \n";
                     //_container.Subscribe(f);
                     f.Dispose();
                 });

            first = txt.IndexOf(flag);
            last = txt.LastIndexOf(flag);
            _1 = txt.Substring(0, first);
            _2 = flag + "\n" + add;
            _3 = txt.Substring(last, txt.Length - last);

            txt = _1 + _2 + _3;
            System.IO.File.WriteAllText(Application.dataPath + "/MutiFramework/Frameworks.cs", txt);
            AssetDatabase.Refresh();
        }

        private void DescibtionAndBaseTool()
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
                    UpdateScripts();
                }
            }
        }
        private void FrameworksInProject(string search)
        {
            for (int i = 0; i < _frameworks.Count; i++)
            {
                if (string.IsNullOrEmpty(search) || _frameworks[i].name.ToLower().Contains(search.ToLower()))
                {
                    GUILayout.BeginHorizontal(Styles.box);

                    _frameworks[i].OnGUI();
                    GUILayout.EndHorizontal();

                }
            }
        }
        private void FrameworkCollection(string search)
        {
            for (int i = 0; i < _frameworkCollection.Count; i++)
            {
                if (string.IsNullOrEmpty(search) || _frameworkCollection[i].name.ToLower().Contains(search.ToLower()))
                {
                    GUILayout.BeginHorizontal(Styles.box);

                    _frameworkCollection[i].OnGUI();
                    GUILayout.EndHorizontal();

                }
            }
        }
        private void Tools(string search)
        {
            for (int i = 0; i < _tools.Count; i++)
            {
                if (string.IsNullOrEmpty(search) || _tools[i].name.ToLower().Contains(search.ToLower()))
                {
                    GUILayout.BeginHorizontal(Styles.box);
                    _tools[i].OnGUI();
                    GUILayout.EndHorizontal();
                }
            }
        }



        private List<FrameworkLineDrawer> _frameworks;
        private List<ToolLineDrawer> _tools;
        private List<FrameworkCollectionLineDrawer> _frameworkCollection;


        [SerializeField] private bool _fold0;

        [SerializeField] private bool _fold1;
        [SerializeField] private Vector2 _scroll1;
        [SerializeField] private string _search1;

        [SerializeField] private bool _fold2;
        [SerializeField] private Vector2 _scroll2;
        [SerializeField] private string _search2;

        [SerializeField] private bool _fold3;
        [SerializeField] private string _search3;
        [SerializeField] private Vector2 _scroll3;

        private const float searchTxtWith = 200;
        private const float btnWith = 20;
        private const float gap = 10;
    }
    partial class MutiFrameworkWindow
    {
        private void OnEnable()
        {
            _frameworks = GetFrameworksInProject();
            _tools = GetTools();
            _frameworkCollection = GetFrameworkCollection();
        }

        private void OnGUI()
        {
            GUILayout.Label("MutiFramework", Styles.header,GUILayout.Width(position.width));


            //Descibtion and base Tool
            {
                GUILayout.Label("Descibtion and base Tool", Styles.in_title);
                var rect = GUILayoutUtility.GetLastRect();
                _fold0 = GUI.Toggle(new Rect(rect.position, new Vector2(gap, rect.height)), _fold0, "");
                if (new Rect(rect.position, new Vector2(rect.width - searchTxtWith - btnWith * 2, rect.height)).Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
                {
                    _fold0 = !_fold0;
                    Event.current.Use();
                }

                if (_fold0)
                {
                    DescibtionAndBaseTool();
                }
            }

            //Frameworks In Project
            {
                GUILayout.Space(gap);
                GUILayout.Label("Frameworks In Project", Styles.in_title);
                var rect = GUILayoutUtility.GetLastRect();
                _fold1 = GUI.Toggle(new Rect(rect.position, new Vector2(gap, rect.height)), _fold1, "");
                if (new Rect(rect.position, new Vector2(rect.width - searchTxtWith - btnWith * 2, rect.height)).Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
                {
                    _fold1 = !_fold1;
                    Event.current.Use();
                }
                rect.xMax -= btnWith;
                rect.x = rect.xMax - searchTxtWith;
                rect.width = searchTxtWith;
                _search1 = GUI.TextField(rect, _search1,Styles.searchTextField);
                rect.xMax += btnWith;
                rect.x = rect.xMax - btnWith;
                rect.width = btnWith;
                if (string.IsNullOrEmpty(_search1))
                {
                    GUI.Label(rect, "",Styles.searchCancelButtonEmpty);
                }
                else
                {
                    if (GUI.Button(rect,"",Styles.searchCancelButton))
                    {
                        _search1 = string.Empty;
                    }
                }

                if (_fold1)
                {
                    _scroll1 = GUILayout.BeginScrollView(_scroll1);
                    {
                        FrameworksInProject(_search1);
                        GUILayout.EndScrollView();
                    }
                }
            }

            //Framework Collection
            {
                GUILayout.Space(gap);
                GUILayout.Label("Framework Collection", Styles.in_title);
                var rect = GUILayoutUtility.GetLastRect();
                _fold2 = GUI.Toggle(new Rect(rect.position, new Vector2(gap, rect.height)), _fold2, "");
                if (new Rect(rect.position,new Vector2(rect.width- searchTxtWith-btnWith*2, rect.height)).Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
                {
                    _fold2 = !_fold2;
                    Event.current.Use();
                }
                rect.xMax -= btnWith;
                rect.x = rect.xMax - searchTxtWith;
                rect.width = searchTxtWith;
                _search2 = GUI.TextField(rect, _search2, Styles.searchTextField);
                rect.xMax += btnWith;
                rect.x = rect.xMax - btnWith;
                rect.width = btnWith;
                if (string.IsNullOrEmpty(_search2))
                {
                    GUI.Label(rect, "", Styles.searchCancelButtonEmpty);
                }
                else
                {
                    if (GUI.Button(rect, "", Styles.searchCancelButton))
                    {
                        _search2 = string.Empty;
                    }
                }
                if (_fold2)
                {
                    _scroll2 = GUILayout.BeginScrollView(_scroll2);
                    {
                        FrameworkCollection(_search2);
                        GUILayout.EndScrollView();
                    }
                }
            }

            //Tools
            {
                GUILayout.Space(gap);
                GUILayout.Label("Tools", Styles.in_title);
                var rect = GUILayoutUtility.GetLastRect();
                _fold3 = GUI.Toggle(new Rect(rect.position, new Vector2(gap, rect.height)), _fold3, "");
                if (new Rect(rect.position, new Vector2(rect.width - searchTxtWith - btnWith * 2, rect.height)).Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
                {
                    _fold3 = !_fold3;
                    Event.current.Use();
                }
                rect.xMax -= btnWith;
                rect.x = rect.xMax - searchTxtWith;
                rect.width = searchTxtWith;
                _search3 = GUI.TextField(rect, _search3, Styles.searchTextField);
                rect.xMax += btnWith;
                rect.x = rect.xMax - btnWith;
                rect.width = btnWith;
                if (string.IsNullOrEmpty(_search3))
                {
                    GUI.Label(rect, "", Styles.searchCancelButtonEmpty);
                }
                else
                {
                    if (GUI.Button(rect, "", Styles.searchCancelButton))
                    {
                        _search3 = string.Empty;
                    }
                }
                if (_fold3)
                {
                    _scroll3 = GUILayout.BeginScrollView(_scroll3);
                    {
                        Tools(_search3);
                        GUILayout.EndScrollView();
                    }
                }
            }

            GUILayout.FlexibleSpace();
            Repaint();
        }
    }
}

