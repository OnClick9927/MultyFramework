using MutiFramework;
using UnityEngine;

public class ExampleFrame1Drawer : FrameworkDrawer
{
    public override string name { get { return "ExampleFrame1"; } }

    public override string version { get { return "1"; } }

    public override string author { get { return "author"; } }

    public override string describtion { get { return "describtion"; } }

    public override string assetPath { get { return "Assets"; } }
    public override void FrameworkGUI()
    {
        if (GUILayout.Button("Say Type"))
        {
            Debug.Log(GetType().FullName);
        }
    }

}
public class ExampleFrame2Drawer : FrameworkDrawer
{
    public override string name { get { return "ExampleFrame2"; } }

    public override string version { get { return "1"; } }

    public override string author { get { return "author"; } }

    public override string describtion { get { return "describtion"; } }

    public override string assetPath { get { return "Assets"; } }

    public override void FrameworkGUI()
    {
        if (GUILayout.Button("Say Type"))
        {
            Debug.Log(GetType().FullName);
        }
    }

  
}



public class ExampleToolLine : ToolDrawer
{
    public override string name { get { return "ChooseColor"; } }

    public override string version { get { return "1"; } }

    public override string author { get { return "author"; } }

    public override string describtion { get { return "we\n" +
                "can\n" +
                "choose\n" +
                "color\n" +
                "of\n" +
                "the\n" +
                "box\n"
                ;
        }
    }

    public override string assetPath { get { return "Assets"; } }

    private Color _color;


    protected override void ToolGUI()
    {
        GUILayout.Space(10);
        _color = UnityEditor.EditorGUILayout.ColorField("Box Color",_color);
        GUI.color = _color;
        GUILayout.Box("Box");
        GUI.color = Color.white;
    }
}