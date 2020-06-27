namespace MutiFramework
{
    public abstract class FrameworkLineDrawer: LineGUIDrawer
    {

        public override void OnGUI()
        {
            base.OnGUI();
            UnityEngine.GUILayout.Button("button");
        }
    } 
}

