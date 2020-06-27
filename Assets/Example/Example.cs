using MutiFramework;

[Framework(Environment.Editor | Environment.Runtime)]
public class ExampleFrame1 : UpdateFramework
{
    public override string name => "ExampleFrame1";

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
[Framework(Environment.Runtime)]
public class ExampleFrame2 : UpdateFramework
{
    public override string name => "ExampleFrame2";

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