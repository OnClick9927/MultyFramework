using System;
namespace MultyFramework
{
    /// <summary>
    /// 网上的条目
    /// </summary>
    [Serializable]
    public class WebCollectionInfo
    {
        [Serializable]
        public class Version
        {
            public string version;
            public string describtion;
            public string unityVersion;
            public string[] dependences;
            public string assetPath;
            public string helpurl;
        }
        /// <summary>
        /// 名字
        /// </summary>
        public string name;
        /// <summary>
        /// 作者
        /// </summary>
        public string author;
        /// <summary>
        /// 下载到的编辑器路径
        /// </summary>

        /// <summary>
        /// 版本信息
        /// </summary>
        public Version[] versions;

        public WebCollectionInfo() { }
        public WebCollectionInfo(string name, string author, Version[] versions)
        {
            this.name = name;
            this.author = author;
            this.versions = versions;
        }
    }
}


