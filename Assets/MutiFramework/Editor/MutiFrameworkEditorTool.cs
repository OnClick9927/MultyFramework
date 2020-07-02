#if MutiFramework
using UnityEngine;
using UnityEditor;

namespace MutiFramework
{
    class MutiFrameworkEditorTool : ToolDrawer
    {
        private const float searchTxtWith=200;
        private const float gap=10;
        private const int btnWith=20;
        private const string _version = "0.0.1";
        private const string _describtion = "MutiFramework  version: "+ _version+" \n" +
                                            "\n you can enjoy to use it";
        private string[] _dependences = new string[] {
            "MutiFramework"
        };

        public override string name { get { return "MutiFramework Editor Tool"; } }
        public override string version { get { return _version; } }
        public override string author { get { return "OnClick"; } }
        public override string describtion { get { return _describtion; } }
        public override string assetPath { get { return "Assets"; } }
        public override string[] dependences { get { return _dependences; } }
        public override string helpurl { get { return MutiFrameworkWindowUtil.frameworkUrl; } }

        protected override void RemoveTool(string path)
        {
            ShowNotification("This is Tool of MutiFramework\n " +
                                "Can not Remove This");
        }

        protected override void ToolGUI()
        {
            GUILayout.Label("Tips:", Styles.boldLabel);
            GUILayout.Label("1、write custom Framework class extends Framework/UpdateFramework with Attribute(Framework)\n" +
                 "2、write custom Framework GUI extends FrameworkDrawer if you need\n" +
                 "3、write custom Tool GUI extends ToolDrawer if you need\n" +
                 "4、click update Script Button and wait for seconds", Styles.boldLabel);
            GUILayout.Space(gap);
            if (GUILayout.Button("Update Script"))
            {
                if (!EditorApplication.isCompiling)
                {
                    MutiFrameworkWindowUtil.CreateClass();
                }
            }
        }
    }
}
#endif
