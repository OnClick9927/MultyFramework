using MutiFramework;

[Framework(Environment.Editor | Environment.Runtime)]
public class IFFF : UpdateFramework
{
    public override string name => "iffff";

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
public class cfff : UpdateFramework
{
    public override string name => "cfff";

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