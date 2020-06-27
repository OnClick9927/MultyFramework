using UnityEngine;

namespace MutiFramework
{
    public abstract class FrameworkGUIDrawer
    {
        public abstract string name { get; }

        public virtual void OnGUI()
        {
            GUILayout.Label(name, "boldlabel", GUILayout.Width(100));
            GUILayout.Button(name);

        }
    }


}

