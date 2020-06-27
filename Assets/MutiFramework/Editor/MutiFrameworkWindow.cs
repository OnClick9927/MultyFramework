using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

namespace MutiFramework
{
    partial class MutiFrameworkWindow : EditorWindow
    {

        [MenuItem("MutiFramework/Window")]
        static void OpenWindow()
        {
            GetWindow<MutiFrameworkWindow>();
        }

        private List<FrameworkGUIDrawer> GetCustomGUI()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany((a) => { return a.GetTypes(); });
            return types
                .Where((type) => { return !type.IsAbstract && type.IsSubclassOf(typeof(FrameworkGUIDrawer)); })
                .Select((type) => { return Activator.CreateInstance(type) as FrameworkGUIDrawer; }).ToList();

        }

        private List<FrameworkGUIDrawer> _customGUI;
        [SerializeField] private Vector2 _scroll;
        [SerializeField] private bool _fold;
        [SerializeField] private string _search;
        [SerializeField] private bool _fold1;
    }
    partial class MutiFrameworkWindow
    {
        private void OnEnable()
        {
            _customGUI = GetCustomGUI();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("1、write custom Framework class extends Framework/UpdateFramework with Attribute(Framework)\n" +
                             "2、write custom Framework GUI extends FrameworkGUIDrawer if you need\n"+
                             "3、click update Script Button and wait for seconds", "SettingsHeader");
            GUILayout.Space(10);

            if (GUILayout.Button("Update Script"))
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
                txt = txt.Replace(flag, add + flag);
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
                txt = txt.Replace(flag, add + flag);
                System.IO.File.WriteAllText(Application.dataPath + "/MutiFramework/Frameworks.cs", txt);
                AssetDatabase.Refresh();
            }
            GUILayout.Space(10);

            {
                GUILayout.Space(10);
                GUILayout.Label("Frameworks", "IN Title");
                var rect = GUILayoutUtility.GetLastRect();
                _fold = GUI.Toggle(new Rect(rect.position, new Vector2(10, rect.height)), _fold, "");
                rect.x = rect.xMax - 300;
                rect.width = 300;
                _search = GUI.TextField(rect, _search);

                if (_fold)
                {
                    _scroll = GUILayout.BeginScrollView(_scroll);
                    {
                        for (int i = 0; i < _customGUI.Count; i++)
                        {
                            if (string.IsNullOrEmpty(_search) || _customGUI[i].name.ToLower().Contains(_search.ToLower()))
                            {
                                GUILayout.BeginHorizontal("box");

                                _customGUI[i].OnGUI();
                                GUILayout.EndHorizontal();

                            }
                        }
                        GUILayout.EndScrollView();
                    }
                }

            }
            {
                GUILayout.Space(10);
                GUILayout.Label("Tools", "IN Title");
                var rect = GUILayoutUtility.GetLastRect();
                _fold1 = GUI.Toggle(new Rect(rect.position, new Vector2(10, rect.height)), _fold1, "");
                if (_fold1)
                {

                }
            }

             
            Repaint();
        }
        private void OnDisable()
        {

        }
    }
}

