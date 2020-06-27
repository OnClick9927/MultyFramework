using UnityEngine;

namespace MutiFramework
{
    public abstract class LineGUIDrawer
    {
        public abstract string name { get; }
        public virtual string tooltip { get; }
        public virtual void OnGUI()
        {
            GUILayout.Label(new GUIContent(name, tooltip), "boldlabel", GUILayout.Width(150));
         
        }
    }

}

