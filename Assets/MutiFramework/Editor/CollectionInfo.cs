#if MutiFramework
namespace MutiFramework
{
    /// <summary>
    /// 网上的条目
    /// </summary>
    public class CollectionInfo
    {
        private string _name;
        private string _version;
        private string _author;
        private string _describtion;
        private string _assetPath;
        private string[] _dependences;
        private string _helpurl;
        private string _unityVersion;
        private string _downloadUrl;

        public CollectionInfo(string name, string version, string author, string describtion, string assetPath, string[] dependences, string helpurl, string unityVersion, string downloadUrl)
        {
            _name = name;
            _version = version;
            _author = author;
            _describtion = describtion;
            _assetPath = assetPath;
            _dependences = dependences;
            _helpurl = helpurl;
            _unityVersion = unityVersion;
            _downloadUrl = downloadUrl;
        }

        /// <summary>
        /// unity版本
        /// </summary>
        public string unityVersion { get { return _unityVersion; } }
        /// <summary>
        /// 名字
        /// </summary>
        public string name { get { return _name; } }
        /// <summary>
        /// 版本
        /// </summary>
        public string version { get { return _version; } }
        /// <summary>
        /// 作者
        /// </summary>
        public string author { get { return _author; } }
        /// <summary>
        /// 描述
        /// </summary>
        public string describtion { get { return _describtion; } }
        /// <summary>
        /// 下载到的编辑器路径
        /// </summary>
        public string assetPath { get { return _assetPath; } }
        /// <summary>
        /// 帮助链接
        /// </summary>
        public string helpurl { get { return _helpurl; } }
        /// <summary>
        /// 依赖项
        /// </summary>
        public string[] dependences { get { return _dependences; } }
        /// <summary>
        /// 下载路径
        /// </summary>
        public string downloadUrl { get { return _downloadUrl; } }

    }
}

#endif
