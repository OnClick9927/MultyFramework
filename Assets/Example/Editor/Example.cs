using MutiFramework;
using UnityEngine;
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

    private Color _color=Color.white;


    protected override void ToolGUI()
    {
        GUILayout.Space(10);
        _color = UnityEditor.EditorGUILayout.ColorField("Box Color",_color);
        GUI.color = _color;
        GUILayout.Box("Box",GUILayout.Width(100),GUILayout.Height(100));
        GUI.color = Color.white;
    }
}