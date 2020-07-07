🏷[English](https://github.com/OnClick9927/MutiFramework/blob/master/README.md) 🏷 [中文](https://github.com/OnClick9927/MutiFramework/blob/master/%E8%AF%BB%E6%88%91.md)
# Describtion
The starter of some frameworks   
The Goal : Use multiple frames at the same time
# 🥚Coming 
* download/ update/upload codes
# Framework Structure
* Editor
  * EditorFrameworks（The Frameworks in Editor mode，The Editor Entrance of Framework that in project which supports Editor mode）
  * Window
    * ToolDrawer（The Editorwindow view of tools that in project）
* Runtime
  * Frameworks（The Frameworks in runtime Mode，The Editor Entrance of Framework that in project which supports runtime mode）
  * Framwork/UpdateFramework（extends this class when you import a Framework and you can use it）
  
# Standard Code Example
``` csharp
class StandardExampleClass
{
    private const string fieldConstPrivate = "const_private" ;
    private static string fieldConstPrivate = "static_private" ;
    private string _fieldPrivate="_private";

    public const string fieldConstPublic = "const_public" ;
    public static string fieldStaticPublic = "static_public" ;
    public string fieldPublic= "public";

    private string _propertyPrivate { get { return "_property" ;} }
    private static string propertyStaticPrivate { get { return "_property" ;} }
    public string propertyPublic { get { return "_property" ;} }
    public static string propertyStaticPublic { get { return "_property" ;} }

    private void PrivateFunction()
    {
    }

    public void PublicFunction()
    {
    }
}
```


# How To Use
 ###  Do Not Forget Add Macro definition MutiFramework
![result](http://file.liangxiegame.com/4578b0b9-2975-4a0e-b89a-26c1be7ddf65.png)
 ### Extend MutiFramework by your codes
 ``` csharp
 [Framework(Environment.Editor | Environment.Runtime)]    //which mode your Framework suport
public class ExampleFrame1 : UpdateFramework
{
    public override string name => "ExampleFrame1";       // your Framework name

    public override int priority => 8;            

    protected override void OnDispose()
    {    
    }

    protected override void OnStartup()
    {   
    }

    protected override void OnUpdate()
    {   
    }
}
 ```
  ### Extend MutiFramework EditorWindow Tools by your codes
 ``` csharp
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
 ```

 
 #### Result of EditorWindow Extension by codes
 ![result](http://file.liangxiegame.com/8d019686-a36b-4930-89ea-8b7c469863bb.png)

# Framework to be accessed
🥚[IFramework(OnClick)](https://github.com/OnClick9927/IFramework)

# Frameworks that have been accessed


  

