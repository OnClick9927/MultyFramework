using MultyFramework;

[Framework(EnvironmentType.Editor | EnvironmentType.Runtime)]
public class ExampleFrame1 : UpdateFramework
{
    public override string name => "ExampleFrame1";

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
[Framework(EnvironmentType.Runtime)]
public class ExampleFrame2 : UpdateFramework
{
    public override string name => "ExampleFrame2";

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