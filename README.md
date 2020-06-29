# Describtion
The starter of some frameworks   
The Goal : Use multiple frames at the same time

# Framework Structure
* Editor
  * EditorFrameworks（The Frameworks in Editor mode，The Editor Entrance of Framework that in project which supports Editor mode）
  * Window
    * FrameworkLineDrawer （The Editorwindow line of Frameworks that in project  ）
    * ToolLineDrawer（The Editorwindow line of tools that in project）
    * FrameworkCollectionLineDrawer（Coming soon   |  Frameworks  download /cleaar/update  ）
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

 ### Extend MutiFramework by your codes
 ``` csharp
 [Framework(Environment.Editor | Environment.Runtime)]    //which mode your Framework suport
public class ExampleFrame1 : UpdateFramework
{
    public override string name => "ExampleFrame1";       // your Framework name

    public override int priority => 8;            

    public override void Dispose()
    {
    }

    public override void Startup()
    {
    }

    public override void Update()
    {
    }
}
 ```
  ### Extend MutiFramework EditorWindow Tools by your codes
 ``` csharp
public class ExampleToolLine : ToolLineDrawer
{
    private Color _color;
    public override string name => "ChooseColor";
    public override void OnGUI()
    {
        base.OnGUI();
        GUI.color = _color;
        _color= UnityEditor.EditorGUILayout.ColorField(_color);
        GUI.color = Color.white;
    }
}
 ```
   ### Extend MutiFramework EditorWindow Enterance of your EditorWindows by your codes
 ``` csharp
public class ExampleFrame1Line : FrameworkLineDrawer
{
    public override string name { get { return "ExampleFrame1"; } }
}
 ```
 #### Result of EditorWindow Extension by codes
 ![result](http://file.liangxiegame.com/8c6a9545-307d-4539-8ade-f3e2d1d46a62.png)
 
# Framework to be accessed
[IFramework(OnClick)](https://github.com/OnClick9927/IFramework)

# Frameworks that have been accessed


  

