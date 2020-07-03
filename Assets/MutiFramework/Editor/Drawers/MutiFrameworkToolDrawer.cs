#if MutiFramework
using UnityEngine;
using UnityEditor;

namespace MutiFramework
{
    class MutiFrameworkToolDrawer : MutiFrameworkDrawer
    {
        private const float searchTxtWith=200;
        private const float gap=10;
        private const int btnWith=20;
        private const string _describtion = "MutiFramework  version: "+ MutiFrameworkEditorTool.version+" \n" +
                                            "\n you can enjoy to use it";
        private string[] _dependences = new string[] {
            "MutiFramework"
        };

        public override string name { get { return "MutiFramework Editor Tools"; } }
        public override string version { get { return MutiFrameworkEditorTool.version; } }
        public override string author { get { return "OnClick"; } }
        public override string describtion { get { return _describtion; } }
        public override string[] dependences { get { return _dependences; } }
        public override string helpurl { get { return MutiFrameworkEditorTool.frameworkUrl; } }



        protected override void ToolGUI()
        {
            GUILayout.Label("Tips:", Styles.boldLabel);
            GUILayout.Label("1、write custom Framework class extends Framework/UpdateFramework with Attribute(Framework)\n" +
                 "2、write custom Tool GUI extends ToolDrawer if you need\n" +
                 "3、click update Script Button and wait for seconds", Styles.boldLabel);
            GUILayout.Space(gap);

            if (GUILayout.Button("Update Script"))
            {
                if (!EditorApplication.isCompiling)
                {
                    MutiFrameworkEditorTool.CreateClass();
                }
            }
        }
    }
}
#endif
