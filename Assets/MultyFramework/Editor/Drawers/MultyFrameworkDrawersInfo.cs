using System;
using System.Collections.Generic;


#pragma  warning disable 0649
namespace MultyFramework
{
    [Serializable]
    public class MultyFrameworkDrawersInfo
    {
        [Serializable]
        public class UserJson
        {
            public string email;
            public string token;
            public string name;
        }
        public string token { get { return userJson.token; } }
        public UserJson userJson;
        public bool login;
        public List<WebCollectionInfo> infos=new List<WebCollectionInfo>();
        public List<WebCollectionInfo> selfInfos=new List<WebCollectionInfo>();



        public List<PanelGUIDrawer> collection=new List<PanelGUIDrawer>();
        public List<PanelGUIDrawer> inProject=new List<PanelGUIDrawer>();

        public void FreshDrawers()
        {
            collection.Clear();
            inProject.Clear();
            infos.ForEach((info) =>
            {
                WebCollection drawer = info;
                drawer.OnEnable();
                if (drawer.exist)
                {
                    inProject.Add(drawer);
                }
                collection.Add(drawer);
            });
            FreshInPorject();
        }
        public void FreshInPorject()
        {
            inProject = collection.FindAll((_c) =>
            {
                return (_c as WebCollection).exist;
            });
        }
    }
}

