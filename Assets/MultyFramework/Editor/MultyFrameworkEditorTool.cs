using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable 0414
#pragma warning disable 0649
namespace MultyFramework
{
    class MultyFrameworkEditorTool
    {
        #region upkg
        public static class PkgConstant
        {
            public const string HOST = "https://api.upkg.net/v1/";

            public const string API_LOGIN = HOST + "login";
            public const string API_SIGNUP = HOST + "signup";

            public const string API_VERIFY_TOKEN = HOST + "verify_token";
            public const string API_CHANGE_PASSWORD = HOST + "change_password";
            public const string API_FORGE_PASSWORD_REQUEST = HOST + "forge_password_request";
            public const string API_FORGE_PASSWORD = HOST + "forge_password";

            // pkg
            public const string API_UPLOAD_PKG = HOST + "upload_pkg";
            public const string API_UPDATE_PKG = HOST + "update_pkg";
            public const string API_DELETE_PKG = HOST + "delete_pkg";
            public const string API_PKG_INFO = HOST + "pkg_info";
            public const string API_PKG_INFOS = HOST + "pkg_infos";
            public const string API_DOWNLOAD_PKG = HOST + "download_pkg";
            public const string API_PKG_INFO_LIST = HOST + "pkg_info_list";

            public static string PKG_PERMISSIONS_PRIVATE = ((int)PkgPermissions.PRIVATE).ToString();
            public static string PKG_PERMISSIONS_PUBLIC = ((int)PkgPermissions.PUBLIC).ToString();
        }

        public enum Code
        {
            OK = 200,

            ERROR_400 = 400,
            ERROR_404 = 404,
            ERROR_500 = 500,

            USER_LOGIN_FAIL = 1201, //登录失败或登录失效
            USER_SIGNUP_FAIL = 1202, //注册失败
            USER_PWD_ERR = 1203, //密码错误
            USER_NULL = 1204, //没有找到该用户
            USER_EXIST = 1205, //已经存在用户

            DB_ERROR = 4001, //数据库错误
            PARAM_ERROR = 4101, //请求参数错误
            AUTHORIZATION_ERROR = 4201, //认证授权错误
            UNKNOWN_ERROR = 4301, //未知错误

            NOT_SUPPORT_GET = 4302, //不支持GET请求,请使用POSE

            PKG_EXIST = 10001, // 包已存在
            PKG_NO_PERMISSION = 10002, //用户无权限操作
            PKG_NO_EXIST = 10003, // 没有找到该包
        }

        public enum UserType
        {
            NONE = 0,
            EMAIL = 1,
            PHONE = 2,
            WX_QUICK = 3,
        }

        public enum PkgPermissions
        {
            PUBLIC = 0,
            PRIVATE = 1,
        }

        [Serializable]
        public class ResponseModel
        {
            private string url; // 请求url
            private string text; // 响应文本

            public int code;

            public string msg;
            //public string err;

            public static T Dispose<T>(string url, string text) where T : ResponseModel
            {
                if (string.IsNullOrEmpty(text))
                    return null;
                var responseModel = JsonUtility.FromJson<T>(text);
                responseModel.text = text;
                responseModel.url = url;
                if (responseModel != null)
                {
                    ShowNotification("Success");
                }
                return responseModel;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="disposeLogic">是否根据code自动处理逻辑</param>
            /// <returns></returns>
            public bool CheckCode(bool disposeLogic = true)
            {
                switch (code)
                {
                    case (int)Code.OK:
                        return true;
                    default:
                        DisplayDialog("Err " + code, this.msg + "\n" + url);
                        return false;
                }
            }
        }

        [Serializable]
        public class LoginModel : ResponseModel
        {
            public UserInfo data;
        }

        [Serializable]
        public class SignupModel : ResponseModel
        {
            public UserInfo data;
        }

        [Serializable]
        public class PkgInfoModel : ResponseModel
        {
            public PkgInfo data;
        }

        [Serializable]
        public class PkgInfosModel : ResponseModel
        {
            public List<PkgInfo> data;
        }

        [Serializable]
        public class PkgInfoListModel : ResponseModel
        {
            public List<Data> data = null;

            [Serializable]
            public class Data
            {
                public string pkg_name;
                public string last_version;
                public string last_time;
            }
        }

        [Serializable]
        public class UserInfo
        {
            public string token;
            public string nick_name;
            public string email;
        }


        [Serializable]
        public class PkgInfo
        {
            // todo:不写默认值是因为返回的结构也是使用此结构体 如果有默认值会造成失败时候也有数据了
            public string pkg_name;
            public string version;
            public string permissions;
            public string dependences;
            public string author;
            public string describtion;
            public string help_url;
            public string pkg_path;
            public string unity_version;
            public string create_time;
            public string share_url;
            public string share_pwd;

            public List<string> GetDependences()
            {
                return dependences.Split(',').ToList();
            }

            public void AddDependences(string pkgName)
            {
                var d = GetDependences().ToList();
                d.Add(pkgName);
                dependences = string.Join(",", d.ToArray());
            }

            public void RemoveDependences(string pkgName)
            {
                var d = GetDependences().ToList();
                d.Remove(pkgName);
                dependences = string.Join(",", d.ToArray());
            }
        }

      

        public class HttpPkg
        {
            private abstract class Request
            {
                public readonly string url;
                private readonly Action<UnityWebRequest> _callback;
                protected readonly bool addToken;
                protected UnityWebRequest request;

                public float progress
                {
                    get { return request.downloadProgress; }
                }

                public bool isDone
                {
                    get { return request.isDone; }
                }
                public string error { get { return request.error; } }
                protected Request(string url, Action<UnityWebRequest> callback, bool addToken = false)
                {
                    this.url = url;
                    this._callback = callback;
                    this.addToken = addToken;
                }

                public void Start()
                {
                    request.SendWebRequest();
                }

                public void Compelete()
                {
                    ClearProgressBar();
                    if (!string.IsNullOrEmpty(request.error))
                    {
                        ShowNotification(request.error);
                        return;
                    }

                    if (_callback != null)
                    {
                        ShowNotification("Request Sucess");
                        _callback.Invoke(request);
                    }

                    request.Abort();
                    request.Dispose();
                }
                public static long GetTimeStamp(bool bflag = true)
                {
                    TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    long ret;
                    if (bflag)
                        ret = Convert.ToInt64(ts.TotalSeconds);
                    else
                        ret = Convert.ToInt64(ts.TotalMilliseconds);
                    return ret;
                }
            }

            private class Request_Get : Request
            {
                public Request_Get(string url, Action<UnityWebRequest> callback, Dictionary<string, object> forms,
                    bool addToken = false) : base(url, callback, addToken)
                {
                    string newUrl = url;

                    if (forms != null && forms.Count > 0)
                    {
                        newUrl += "?";
                        foreach (var item in forms)
                        {
                            newUrl += string.Format("{0}={1}", item.Key, item.Value) + "&";
                        }

                      //  newUrl = newUrl.Substring(0, newUrl.Length - 1);
                        newUrl +=  GetTimeStamp();

                    }
                    else
                    {
                        newUrl += "?" + GetTimeStamp();

                    }
                    //Debug.LogError(newUrl);
                    request = UnityWebRequest.Get(newUrl);
                    if (addToken)
                    {
                        if (string.IsNullOrEmpty(token))
                        {
                            ShowNotification("token is Null , Please Login First");
                            return;
                        }

                        request.SetRequestHeader("token", token);
                    }
                }
            }

            private class Request_Post : Request
            {
                public Request_Post(string url, Action<UnityWebRequest> callback, WWWForm forms, bool addToken = false)
                    : base(url, callback, addToken)
                {
                    if (forms == null)
                    {
                        forms = new WWWForm();
                    }
                    url += "?" + GetTimeStamp();
                   // Debug.LogError(url);

                    request = UnityWebRequest.Post(url, forms);
                    if (addToken)
                    {
                        if (string.IsNullOrEmpty(token))
                        {
                            ShowNotification("token is Null , Please Login First");
                            return;
                        }

                        request.SetRequestHeader("token", token);
                    }
                }
            }

            private const int maxRequest = 20;
            private static Queue<Request> _waitRequests;
            private static List<Request> _requests;

            private static void Update()
            {
                if (_waitRequests.Count <= 0 && _requests.Count <= 0) return;
                while (_requests.Count < maxRequest)
                {
                    if (_waitRequests.Count > 0)
                    {
                        var _req = _waitRequests.Dequeue();
                        _req.Start();
                        _requests.Add(_req);
                    }
                    else
                    {
                        break;
                    }
                }

                for (int i = _requests.Count - 1; i >= 0; i--)
                {
                    var _req = _requests[i];
                    if (_req.isDone)
                    {
                        _req.Compelete();
                        _requests.Remove(_req);
                        //  break;
                    }
                    else
                    {
                        DisplayProgressBar("Post Request", _req.url, _req.progress);
                    }
                }
            }

            private static void Run(Request request)
            {
                if (_waitRequests == null)
                {
                    _waitRequests = new Queue<Request>();
                    _requests = new List<Request>();
                    EditorApplication.update += Update;
                }

                _waitRequests.Enqueue(request);
            }


            private static void GetRequest(string url, Dictionary<string, object> forms,
                Action<UnityWebRequest> callback, bool addToken = false)
            {
                Run(new Request_Get(url, callback, forms, addToken));
            }

            private static void PostRequest(string url, WWWForm forms, Action<UnityWebRequest> callback,
                bool addToken = false)
            {
                Run(new Request_Post(url, callback, forms, addToken));
            }

            private static void GetRequest<T>(string url, Dictionary<string, object> forms, Action<T> callback,
                bool addToken = false) where T : ResponseModel
            {
                GetRequest(url, forms, (req) =>
                {
                    T t = ResponseModel.Dispose<T>(url, req.downloadHandler.text);
                    if (t == null)
                    {
                        DisplayDialog("Dispose Err", req.downloadHandler.text);
                        return;
                    }

                    bool bo = t.CheckCode(false);
                    if (bo && callback != null)
                    {
                        callback.Invoke(t);
                    }
                }, addToken);
            }

            private static void PostRequest<T>(string url, WWWForm forms, Action<T> callback, bool addToken = false)
                where T : ResponseModel
            {
                PostRequest(url, forms, (req) =>
                {
                    T t = ResponseModel.Dispose<T>(url, req.downloadHandler.text);
                    if (t == null)
                    {
                        DisplayDialog("Dispose Err", req.downloadHandler.text);
                        return;
                    }

                    bool bo = t.CheckCode(false);

                    if (bo && callback != null)
                    {
                        callback.Invoke(t);
                    }
                }, addToken);
            }

            #region API.User

            public static void CheckToken(string token, Action<LoginModel> callback)
            {
                PostRequest<LoginModel>(PkgConstant.API_VERIFY_TOKEN, null, (m) =>
                {
                   // m.data.token = token;
                    if (callback != null)
                    {
                        callback(m);
                    }
                }, true);
            }

            public static void ChangePassword(string pwd, Action<ResponseModel> callback)
            {
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddField("password", pwd);
                PostRequest<ResponseModel>(PkgConstant.API_CHANGE_PASSWORD, wwwForm, callback, true);
            }

            #region Email

            public static void LoginFormEmail(string email, string pwd, Action<LoginModel> callback)
            {
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddField("user_type", ((int)UserType.EMAIL).ToString());
                wwwForm.AddField("email", email);
                wwwForm.AddField("password", pwd);
                PostRequest<LoginModel>(PkgConstant.API_LOGIN, wwwForm, callback);
            }

            public static void SignupFormEmail(string nickName, string email, string pwd, Action<SignupModel> callback)
            {
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddField("user_type", ((int)UserType.EMAIL).ToString());
                wwwForm.AddField("nick_name", nickName);
                wwwForm.AddField("email", email);
                wwwForm.AddField("password", pwd);
                PostRequest<SignupModel>(PkgConstant.API_SIGNUP, wwwForm, callback);
            }

            public static void ForgePasswordRequestFormEmail(string email, Action<ResponseModel> callback)
            {
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddField("user_type", ((int)UserType.EMAIL).ToString());
                wwwForm.AddField("email", email);
                PostRequest<ResponseModel>(PkgConstant.API_FORGE_PASSWORD_REQUEST, wwwForm, callback, false);
            }

            public static void ForgePasswordFormEmail(string email, string pwd, string code,
                Action<ResponseModel> callback)
            {
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddField("user_type", ((int)UserType.EMAIL).ToString());
                wwwForm.AddField("password", pwd);
                wwwForm.AddField("email", email);
                wwwForm.AddField("code", code);
                PostRequest<ResponseModel>(PkgConstant.API_FORGE_PASSWORD, wwwForm, callback, false);
            }

            #endregion

            #endregion

            #region API.Pkg

            public static void UploadPkg(PkgInfo uploadForm, byte[] fileByte, Action<ResponseModel> callback)
            {
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddBinaryData("file", fileByte);
                wwwForm.AddField("pkg_name", uploadForm.pkg_name);
                wwwForm.AddField("version", uploadForm.version);
                wwwForm.AddField("unity_version", uploadForm.unity_version);
                wwwForm.AddField("pkg_path", uploadForm.pkg_path);
                wwwForm.AddField("author", uploadForm.author);
                wwwForm.AddField("describtion", uploadForm.describtion);
                wwwForm.AddField("help_url", uploadForm.help_url);
                wwwForm.AddField("dependences", uploadForm.dependences);
                wwwForm.AddField("permissions", uploadForm.permissions);
                PostRequest<ResponseModel>(PkgConstant.API_UPLOAD_PKG, wwwForm, callback, true);
            }

            public static void DeletePkg(string pkgName, string version, Action<ResponseModel> callback)
            {
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddField("pkg_name", pkgName);
                wwwForm.AddField("version", version);
                PostRequest<ResponseModel>(PkgConstant.API_DELETE_PKG, wwwForm, callback, true);
            }


            /// <summary>
            /// 获取包信息
            /// </summary>
            /// <param name="pkgName">包名</param>
            /// <param name="version">版本为空则获取最新, 赋值则获取指定版本的</param>
            public static void GetPkgInfo(string pkgName, string version, Action<PkgInfoModel> callback)
            {
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddField("pkg_name", pkgName);
                if (!string.IsNullOrEmpty(version))
                    wwwForm.AddField("version", version);
                PostRequest<PkgInfoModel>(PkgConstant.API_PKG_INFO, wwwForm, callback, true);
            }

            /// <summary>
            /// 获取当前包所有版本信息
            /// </summary>
            /// <param name="pkgName">包名</param>
            public static void GetPkgInfos(string pkgName, Action<PkgInfosModel> callback)
            {
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddField("pkg_name", pkgName);
                PostRequest<PkgInfosModel>(PkgConstant.API_PKG_INFOS, wwwForm, callback, true);
            }


            /// <summary>
            /// 获取所有公开的包名称列表
            /// </summary>
            /// <param name="pkgName">包名</param>
            /// <param name="version">版本为空则获取最新, 赋值则获取指定版本的</param>
            public static void GetPkgInfoList(Action<PkgInfoListModel> callback)
            {
                PostRequest<PkgInfoListModel>(PkgConstant.API_PKG_INFO_LIST, null, callback, true);
            }

            public static void DownloadPkg(string pkgName, string version, string downloadPath, Action onCompleted)
            {
                string url = PkgConstant.API_DOWNLOAD_PKG;
                url += "?pkg_name=" + pkgName;
                if (!string.IsNullOrEmpty(version))
                    url += "&version=" + version;
                url += "&token=" + token;
                GetRequest(url, null, (req) =>
                {
                    File.WriteAllBytes(downloadPath, req.downloadHandler.data);
                    if (onCompleted != null)
                    {
                        onCompleted();
                    }
                });
            }

            #endregion
        }
        #endregion



        private const string userJsonName = "version.json";
        private static Encoding _encoding = Encoding.UTF8;
        public const string baidu = "https://www.baidu.com/";
        public const string frameworkUrl = "https://upkg.org";
        public const string version="0.0.0.1";
        public const string framewokName = "MultyFramework";
        private static MultyFrameworkWindow _window;
        public static event Action onPackagesChange;

        private static string userjsonPath
        {
            get { return MultyFrameworkEditorTool.rootPath + "/user.json"; }
        }
        private static string pkgjsonPath
        {
            get { return MultyFrameworkEditorTool.rootPath + "/pkgs.json"; }
        }
        private static string pkgversionjsonPath
        {
            get
            {
                string path = MultyFrameworkEditorTool.rootPath + "/pkgversion";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;

            }
        }
        private static string pkgPath
        {
            get
            {
                string path = MultyFrameworkEditorTool.rootPath + "/pkgs";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;

            }
        }
        private static string localPkgPath
        {
            get
            {
                string path = MultyFrameworkEditorTool.rootPath + "/localpkgs";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;

            }
        }
        public static MultyFrameworkWindow window
        {
            get
            {
                if (_window == null)
                {
                    Open();
                }
                return _window;
            }
            set {
                _window = value;
            }
        }
        public static string rootPath
        {
            get
            {
                string path = Path.Combine(Application.persistentDataPath+"/../",framewokName+"Memory");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    File.SetAttributes(path, FileAttributes.Hidden);
                }
                return path;
            }
        }
        public static string token
        {
            get { return window.multyDrawersInfo.token; }
        }
        public static bool login
        {
            get { return window.multyDrawersInfo.login; }
        }
        public static MultyFrameworkDrawersInfo.UserJson _userJson
        {
            get { return window.multyDrawersInfo.userJson; }
        }



        public class ForgetPasswordInfo
        {
            public string email;
            public string code;
            public string newPsd;
        }
        public class LoginInfo
        {
            public string email;
            public string password;
            public bool see;
        }
        public class RegisterInfo
        {
            public string email;
            public string password;
            public string nick_name;
        }
        public class UploadInfo
        {
            public string unityVersion
            {
                get { return Application.unityVersion; }
            }

            public bool isPublic = true;
            public string name = "pkg name";
            public string version = "0.0.0.1";
            public string author = "author";
            public string describtion = "No Describtion ";
            public string assetPath = "Assets";
            public string helpurl = frameworkUrl;
            public List<string> dependences = new List<string>();
        }

        [MenuItem(framewokName + "/Open Memory")]
        static void OpenMemory()
        {
            EditorUtility.OpenWithDefaultApp(rootPath);
        }
        [MenuItem(framewokName + "/Clear Memory")]
        static void ClearMemory()
        {
            Directory.Delete(rootPath, true);
        }

        [MenuItem(framewokName + "/Update To Newest")]
        static void UpdateFramework()
        {
            UpdatePakage(framewokName);
        }
        [MenuItem(framewokName + "/Window")]
        static void Open()
        {
            var ws = Resources.FindObjectsOfTypeAll<MultyFrameworkWindow>();
            if (ws != null && ws.Length > 0) return;
            _window = EditorWindow.GetWindow<MultyFrameworkWindow>();
            window.url = frameworkUrl;
            window.webview = ScriptableObject.CreateInstance<WebViewHook>();
            if (window.webview.Hook(window))
                window.titleContent = new GUIContent("MultyFramework");
            Init();
        }




        public static void Init()
        {
            window.multyDrawersInfo = new MultyFrameworkDrawersInfo();
            Func<MultyFrameworkDrawersInfo.UserJson> check = () => {
                if (File.Exists(userjsonPath))
                {
                    return JsonUtility.FromJson<MultyFrameworkDrawersInfo.UserJson>(File.ReadAllText(userjsonPath,_encoding));
                }
                return new MultyFrameworkDrawersInfo.UserJson();
            };
            window.multyDrawersInfo.userJson = check();
            LoginWithToken();
        }

        private static void FreshWebCollection()
        {
            window.multyDrawersInfo.selfInfos.Clear();
            window.multyDrawersInfo.infos.Clear();

            HttpPkg.GetPkgInfoList((m) =>
            {
                var datas = m.data;
                List<PkgInfoListModel.Data> localDatas = new List<PkgInfoListModel.Data>();
                if (File.Exists(pkgjsonPath))
                {
                    localDatas = JsonUtility.FromJson<PkgInfoListModel>(File.ReadAllText(pkgjsonPath)).data;
                }
                File.WriteAllText(pkgjsonPath, JsonUtility.ToJson(m, true));

                for (int i = 0; i < datas.Count; i++)
                {
                    var _data = datas[i];
                    var _tmp = localDatas.Find((d) =>
                    {
                        return d.pkg_name == _data.pkg_name && d.last_time == _data.last_time && d.last_version == _data.last_version;
                    });
                    localDatas.RemoveAll((d) => {
                        return d.pkg_name == _data.pkg_name;
                    });

                    if (_tmp != null)
                    {
                        string path = Path.Combine(pkgversionjsonPath, _data.pkg_name + ".json");
                        WebCollectionInfo info = JsonUtility.FromJson<WebCollectionInfo>(File.ReadAllText(path));
                        TryFinish(info, localDatas, datas);
                    }
                    else
                    {
                        HttpPkg.GetPkgInfos(_data.pkg_name,
                            (model) =>
                            {
                                WebCollectionInfo info = new WebCollectionInfo()
                                {
                                    name = model.data[0].pkg_name,
                                    author = model.data[0].author,
                                };
                                WebCollectionInfo.Version[] versions = new WebCollectionInfo.Version[model.data.Count];
                                for (int j = 0; j < model.data.Count; j++)
                                {
                                    versions[j] = new WebCollectionInfo.Version()
                                    {
                                        version = model.data[j].version,
                                        describtion = model.data[j].describtion,
                                        dependences = model.data[j].GetDependences().ToArray(),
                                        helpurl = model.data[j].help_url,
                                        unityVersion = model.data[j].unity_version,
                                        assetPath = model.data[j].pkg_path,
                                    };
                                }
                                info.versions = versions;
                                File.WriteAllText(Path.Combine(pkgversionjsonPath, info.name + ".json"), JsonUtility.ToJson(info, true));
                                TryFinish(info, localDatas, datas);
                            });
                    }
                }
            });
        }

        private static void TryFinish(WebCollectionInfo info, List<PkgInfoListModel.Data> localDatas, List<PkgInfoListModel.Data> datas)
        {
            window.multyDrawersInfo.infos.Add(info);

            if (window.multyDrawersInfo.infos.Count == datas.Count)
            {
                localDatas.RemoveAll((_d) => {
                    string _path = Path.Combine(pkgversionjsonPath, _d.pkg_name + ".json");
                    if (File.Exists(_path))
                    {
                        File.Delete(_path);
                    }
                    return true;
                });


                if (!string.IsNullOrEmpty(window.multyDrawersInfo.userJson.name))
                {
                    for (int j = 0; j < window.multyDrawersInfo.infos.Count; j++)
                    {
                        if (window.multyDrawersInfo.infos[j].author ==
                            window.multyDrawersInfo.userJson.name)
                        {
                            window.multyDrawersInfo.selfInfos.Add(window.multyDrawersInfo.infos[j]);
                        }
                    }
                }
                window.multyDrawersInfo.FreshDrawers();
            }
        }

        public static void ClearUserJson()
        {
            window.multyDrawersInfo.userJson = new MultyFrameworkDrawersInfo.UserJson();
            window.multyDrawersInfo.login = false;
            if (File.Exists(userjsonPath))
            {
                File.Delete(userjsonPath);
            }

            window.multyDrawersInfo.infos.Clear();
            window.multyDrawersInfo.selfInfos.Clear();
            window.multyDrawersInfo.FreshDrawers();
        }


        public static void TryLogin(LoginInfo info)
        {
            window.multyDrawersInfo.login = false;
            HttpPkg.LoginFormEmail(info.email, info.password, (model) =>
            {
                window.multyDrawersInfo.login = true;
                WriteUserJson(info.email, model.data.token, model.data.nick_name);
                FreshWebCollection();
            });
        }
        public static void LoginWithToken()
        {
            window.multyDrawersInfo.login = false;
            HttpPkg.CheckToken(
                token
                , (model) =>
                {
                    window.multyDrawersInfo.login = true;
                    WriteUserJson(model.data.email, model.data.token, model.data.nick_name);
                    FreshWebCollection();
                });
        }

        private static void WriteUserJson(string email, string token, string name)
        {
            window.multyDrawersInfo.userJson = new MultyFrameworkDrawersInfo.UserJson()
            {
                email = email,
                token = token,
                name = name
            };
            File.WriteAllText(userjsonPath, JsonUtility.ToJson(window.multyDrawersInfo.userJson, true), _encoding);
        }

        public static void Signup(RegisterInfo info)
        {
            HttpPkg.SignupFormEmail(info.nick_name, info.email, info.password, (model) =>
            {
                WriteUserJson(info.email, model.data.token, info.nick_name);
                LoginWithToken();
            });
        }


        public static void ForgetEmailPassword(ForgetPasswordInfo info)
        {
            HttpPkg.ForgePasswordRequestFormEmail(info.email, (model) => { });
        }
        public static void ChangeEmailPassword(ForgetPasswordInfo info)
        {
            HttpPkg.ForgePasswordFormEmail(info.email, info.newPsd, info.code, (model) => {
                ClearUserJson();
            });
        }



        internal static void UpdatePakage(string name)
        {
            string path = string.Format("{0}/{1}_{2}.unitypackage", pkgPath, name, "_lasted");
            HttpPkg.DownloadPkg(name, null, path, () => {
                AssetDatabase.ImportPackage(path, true);
            });
        }
        public static void UploadPackage(UploadInfo uploadInfo)
        {
            // MultyFrameworkEditorTool.CreateVersionJson(uploadInfo.assetPath, uploadInfo);
            AssetDatabase.ExportPackage(uploadInfo.assetPath, uploadInfo.name + ".unitypackage",
                ExportPackageOptions.Recurse);
            byte[] bytes = File.ReadAllBytes("Assets/../" + uploadInfo.name + ".unitypackage");
            PkgInfo form = new PkgInfo()
            {
                unity_version = uploadInfo.unityVersion,
                author = uploadInfo.author,
                pkg_path = uploadInfo.assetPath,
                pkg_name = uploadInfo.name,
                version = uploadInfo.version,
                dependences = "",
                permissions = uploadInfo.isPublic
                    ? PkgConstant.PKG_PERMISSIONS_PUBLIC
                    : PkgConstant.PKG_PERMISSIONS_PRIVATE,
                help_url = uploadInfo.helpurl,
                describtion = uploadInfo.describtion,
            };
            for (int i = 0; i < uploadInfo.dependences.Count; i++)
            {
                form.AddDependences(uploadInfo.dependences[i]);
            }

            HttpPkg.UploadPkg(form, bytes, (m) =>
            {
                string source = "Assets/../" + uploadInfo.name + ".unitypackage";
                string dest = Path.Combine(localPkgPath, uploadInfo.name + "_" + uploadInfo.version + ".unitypackage");
                if (File.Exists(dest))
                {
                    File.Delete(dest);
                }
                File.Move(source,dest);
                FreshWebCollection();
            });
        }
        public static void DeletePackage(string name, string version)
        {
            HttpPkg.DeletePkg(name, version, (m) =>
            {
                FreshWebCollection();
            });
        }
        public static void InstallPackage(string name, string version)
        {
            string path = string.Format("{0}/{1}_{2}.unitypackage", pkgPath, name, version);
            HttpPkg.DownloadPkg(name, version, path, () =>
            {
                AssetDatabase.ImportPackage(path, true);
                window.multyDrawersInfo.FreshInPorject();
                if (onPackagesChange != null)
                {
                    onPackagesChange();
                }
            });
        }
        public static void RemovePakageFromAssets(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
            window.multyDrawersInfo.FreshInPorject();
            if (onPackagesChange != null)
            {
                onPackagesChange();
            }
        }












        public static void CreateClass()
        {

            string flag = "//ToDo";
            string editorPath = "Assets/MultyFramework/Editor/EditorFrameworks.cs";
            string rtPath = "Assets/MultyFramework/Frameworks.cs";
            Encoding utf8 = Encoding.UTF8;
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany((a) => { return a.GetTypes(); });
            EditorUtility.DisplayProgressBar("Fresh Scripts", "", 0);

            string txt = File.ReadAllText(editorPath, utf8);
            string add = "";
            types
                 .Where((type) => {
                     return !type.IsAbstract && type.IsSubclassOf(typeof(Framework)) &&
                         type.IsDefined(typeof(FrameworkAttribute), false) &&
                          ((type.GetCustomAttributes(typeof(FrameworkAttribute), false).First() as FrameworkAttribute).env & EnvironmentType.Editor) != 0;
                 })
                 .Select((type) => {
                     Framework f = Activator.CreateInstance(type) as Framework;
                     return f;
                 }).ToList()
                 .ForEach((f) => {
                     add = add + "\t\tpublic static " + f.GetType() + " " + f.name + "{ get { return GetFramework(\"" + f.name + "\") as " + f.GetType() + ";}} \n";
                     //_container.Subscribe(f);
                     f.Dispose();
                 });
            int first = txt.IndexOf(flag);
            int last = txt.LastIndexOf(flag);
            string _1 = txt.Substring(0, first);
            string _2 = flag + "\n" + add;
            string _3 = txt.Substring(last, txt.Length - last);

            txt = _1 + _2 + _3;
            File.WriteAllText(editorPath, txt.Replace("\r\n", "\n"), utf8);
            EditorUtility.DisplayProgressBar("Fresh Scripts", "", 0.5f);



            txt = File.ReadAllText(rtPath, utf8);
            add = "";
            types
                 .Where((type) => {
                     return !type.IsAbstract && type.IsSubclassOf(typeof(Framework)) &&
                         type.IsDefined(typeof(FrameworkAttribute), false) &&
                        ((type.GetCustomAttributes(typeof(FrameworkAttribute), false).First() as FrameworkAttribute).env & EnvironmentType.Runtime) != 0;
                 })
                 .Select((type) => {
                     Framework f = Activator.CreateInstance(type) as Framework;
                     return f;
                 }).ToList()
                 .ForEach((f) => {
                     add = add + "\t\tpublic static " + f.GetType() + " " + f.name + "{ get { return GetFramework(\"" + f.name + "\") as " + f.GetType() + ";}} \n";
                     //_container.Subscribe(f);
                     f.Dispose();
                 });

            first = txt.IndexOf(flag);
            last = txt.LastIndexOf(flag);
            _1 = txt.Substring(0, first);
            _2 = flag + "\n" + add;
            _3 = txt.Substring(last, txt.Length - last);

            txt = _1 + _2 + _3;
            File.WriteAllText(rtPath, txt.Replace("\r\n", "\n"), utf8);
            EditorUtility.DisplayProgressBar("Fresh Scripts", "", 0.8f);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();

        }
        public static void ShowNotification(string message)
        {
            window.ShowNotification(new GUIContent(message));
        }
        public static void DisplayProgressBar(string title, string info, float progress)
        {
            EditorUtility.DisplayProgressBar(title, info, progress);
        }
        public static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }
        protected static bool DisplayDialog(string title, string info, string ok = "ok", string cancel = "cancel")
        {
            return EditorUtility.DisplayDialog(title, info, ok, cancel);
        }
    }
}
