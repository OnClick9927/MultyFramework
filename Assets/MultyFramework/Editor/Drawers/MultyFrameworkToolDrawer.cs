using UnityEngine;
using UnityEditor;

namespace MultyFramework
{
    class MultyFrameworkToolDrawer : MultyFrameworkDrawer
    {
        private const float searchTxtWith=200;
        private const float gap=10;
        private const int btnWith=20;
        private const string _describtion = "MultyFramework  version: "+ MultyFrameworkEditorTool.version+" \n" +
                                            "\n you can enjoy to use it";
        private string[] _dependences = new string[] {
            "MultyFramework"
        };

        public override string name { get { return "MultyFramework Editor Tools"; } }
        public override string version { get { return MultyFrameworkEditorTool.version; } }
        public override string author { get { return "OnClick"; } }
        public override string describtion { get { return _describtion; } }
        public override string[] dependences { get { return _dependences; } }
        public override string helpurl { get { return MultyFrameworkEditorTool.frameworkUrl; } }



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
                    MultyFrameworkEditorTool.CreateClass();
                }
            }
        }
    }
}

