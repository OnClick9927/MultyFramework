using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0414
#pragma warning disable 0649
namespace MultyFramework
{
    public partial class MultyFrameworkDrawer
    {
        public static class PkgConstant
        {
            // public const string HOST = "https://upkg.org/api/";
            public const string HOST = "https://test-api.upkg.org/v1/";
            // public const string HOST = "https://api.upkg.org/v1/";

            // user
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
                        DisplayDialog("Err " + code, this.msg);
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
                        _callback.Invoke(request);
                    }

                    request.Abort();
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

                        newUrl = newUrl.Substring(0, newUrl.Length - 1);
                    }

                    request = UnityWebRequest.Get(newUrl);
                    if (addToken)
                    {
                        if (string.IsNullOrEmpty(_token))
                        {
                            ShowNotification("token is Null , Please Login First");
                            return;
                        }

                        request.SetRequestHeader("token", _token);
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

                    request = UnityWebRequest.Post(url, forms);
                    if (addToken)
                    {
                        if (string.IsNullOrEmpty(_token))
                        {
                            ShowNotification("token is Null , Please Login First");
                            return;
                        }

                        request.SetRequestHeader("token", _token);
                    }
                }
            }

            private const int maxRequest = 5;
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
                        break;
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
                    m.data.token = token;
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
                GetRequest<PkgInfoListModel>(PkgConstant.API_PKG_INFO_LIST, null, callback, true);
            }

            public static void DownloadPkg(string pkgName, string version, string downloadPath, Action onCompleted)
            {
                string url = PkgConstant.API_DOWNLOAD_PKG;
                url += "?pkg_name=" + pkgName;
                url += "&version=" + version;
                url += "&token=" + _token;

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

    }
    public abstract partial class MultyFrameworkDrawer : PanelGUIDrawer
    {
       

        private static Encoding _encoding = Encoding.UTF8;

        private bool _describtionFold = true;
        private bool _dependencesFold = true;
        private Vector2 _scroll;

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            GUILayout.BeginArea(rect);
            {
                _scroll = GUILayout.BeginScrollView(_scroll);
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(name, Styles.header);
                    if (GUILayout.Button(Contents.help, Styles.controlLabel))
                    {
                        Help.BrowseURL(helpurl);
                    }

                    GUILayout.Space(Contents.gap);
                    GUILayout.Label(Application.unityVersion);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }

                GUILayout.Label("Version  " + version, Styles.boldLabel);
                GUILayout.Label("Author " + author, Styles.boldLabel);

                {
                    GUILayout.Label("Dependences", Styles.in_title);
                    Rect last = GUILayoutUtility.GetLastRect();
                    last.width -= Contents.gap;
                    last.xMin += Contents.gap;
                    if (Event.current.type == EventType.MouseUp && last.Contains(Event.current.mousePosition))
                    {
                        _dependencesFold = !_dependencesFold;
                        Event.current.Use();
                    }

                    last.xMin -= Contents.gap;
                    last.width = Contents.gap;

                    _dependencesFold = UnityEditor.EditorGUI.Foldout(last, _dependencesFold, "");
                    if (_dependencesFold)
                    {
                        if (dependences != null)
                        {
                            for (int i = 0; i < dependences.Length; i++)
                            {
                                GUILayout.Label(dependences[i]);
                            }
                        }
                    }
                }
                {
                    GUILayout.Label("Describtion ", Styles.in_title);
                    Rect last = GUILayoutUtility.GetLastRect();
                    last.width -= Contents.gap;
                    last.xMin += Contents.gap;
                    if (Event.current.type == EventType.MouseUp && last.Contains(Event.current.mousePosition))
                    {
                        _describtionFold = !_describtionFold;
                        Event.current.Use();
                    }

                    last.xMin -= Contents.gap;
                    last.width = Contents.gap;
                    _describtionFold = EditorGUI.Foldout(last, _describtionFold, "");
                    if (_describtionFold)
                    {
                        GUILayout.Label(describtion);
                    }
                }
                GUILayout.Label("", Styles.in_title, GUILayout.Height(0));
                ToolGUI();
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        protected virtual void ToolGUI()
        {
        }




        protected class ForgetPasswordInfo
        {
            public string email;
            public string code;
            public string newPsd;
        }
        protected class LoginInfo
        {
            public string email;
            public string password;
            public bool see;
        }

        protected class RegisterInfo
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
            public string helpurl = MultyFrameworkEditorTool.baidu;
            public List<string> dependences = new List<string>();
        }


        private static string _userjsonPath
        {
            get { return MultyFrameworkEditorTool.rootPath + "/user.json"; }
        }
        private static string _pkgjsonPath
        {
            get { return MultyFrameworkEditorTool.rootPath + "/pkgs.json"; }
        }
        private static string _pkgversionjsonPath
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

        protected static bool _login
        {
            get { return window.multyDrawersInfo.login; }
        }

        protected static string _token
        {
            get { return window.multyDrawersInfo.token; }
        }

        protected static MultyFrameworkDrawersInfo.UserJson _userJson
        {
            get { return window.multyDrawersInfo.userJson; }
        }


        public static void Init()
        {
            window.multyDrawersInfo = new MultyFrameworkDrawersInfo();
            window.multyDrawersInfo.userJson = CheckUserJson();
            LoginWithToken();
        }





        private static MultyFrameworkDrawersInfo.UserJson CheckUserJson()
        {
            if (File.Exists(_userjsonPath))
            {
                return JsonUtility.FromJson<MultyFrameworkDrawersInfo.UserJson>(File.ReadAllText(_userjsonPath,
                    _encoding));
            }

            return new MultyFrameworkDrawersInfo.UserJson();
        }

        private static void FreshWebCollection()
        {
            window.multyDrawersInfo.selfInfos.Clear();
            window.multyDrawersInfo.infos.Clear();

            HttpPkg.GetPkgInfoList((m) =>
            {
                var datas = m.data;
                List<PkgInfoListModel.Data> localDatas = new List<PkgInfoListModel.Data>();
                if (File.Exists(_pkgjsonPath))
                {
                    localDatas = JsonUtility.FromJson<PkgInfoListModel>(File.ReadAllText(_pkgjsonPath)).data;
                }
                File.WriteAllText(_pkgjsonPath, JsonUtility.ToJson(m,true));

                for (int i = 0; i < datas.Count; i++)
                {
                    var _data = datas[i];
                    var _tmp = localDatas.Find((d) =>
                    {
                        return d.pkg_name == _data.pkg_name && d.last_time == _data.last_time && d.last_version == _data.last_version;
                    });
                    if (_tmp != null)
                    {
                        localDatas.Remove(_tmp);
                        string path = Path.Combine(_pkgversionjsonPath, _data.pkg_name + ".json");
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
                                File.WriteAllText(Path.Combine(_pkgversionjsonPath, info.name + ".json"), JsonUtility.ToJson(info,true));
                                TryFinish(info, localDatas, datas);
                            });
                    }
                }
            });
        }
        private static void TryFinish(WebCollectionInfo info , List<PkgInfoListModel.Data> localDatas, List<PkgInfoListModel.Data> datas)
        {
            window.multyDrawersInfo.infos.Add(info);

            if (window.multyDrawersInfo.infos.Count == datas.Count)
            {
                localDatas.RemoveAll((_d) => {
                    string _path = Path.Combine(_pkgversionjsonPath, _d.pkg_name + ".json");
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

        protected static void ClearUserJson()
        {
            window.multyDrawersInfo.userJson = new MultyFrameworkDrawersInfo.UserJson();
            window.multyDrawersInfo.login = false;
            if (File.Exists(_userjsonPath))
            {
                File.Delete(_userjsonPath);
            }

            window.multyDrawersInfo.infos.Clear();
            window.multyDrawersInfo.selfInfos.Clear();
            window.multyDrawersInfo.FreshDrawers();
        }


        protected static void TryLogin(LoginInfo info)
        {
            HttpPkg.LoginFormEmail(info.email, info.password, (model) =>
            {
                WriteUserJson(info.email, model.data.token, model.data.nick_name);
                FreshWebCollection();
            });
        }

        private static void WriteUserJson(string email, string token, string name, bool login = true)
        {
            window.multyDrawersInfo.login = login;
            window.multyDrawersInfo.userJson = new MultyFrameworkDrawersInfo.UserJson()
            {
                email = email,
                token = token,
                name = name
            };

            File.WriteAllText(_userjsonPath, JsonUtility.ToJson(window.multyDrawersInfo.userJson, true), _encoding);
        }

        protected static void Signup(RegisterInfo info)
        {
            HttpPkg.SignupFormEmail(info.nick_name, info.email, info.password, (model) =>
            {
                WriteUserJson(info.email, model.data.token, info.nick_name, false);
                ShowNotification("Success");
                 LoginWithToken();
            });
        }

        private static void LoginWithToken()
        {
            window.multyDrawersInfo.login = false;
            HttpPkg.CheckToken(
                _token
                , (model) =>
                {
                    window.multyDrawersInfo.login = true;
                    FreshWebCollection();
                });
        }

        protected static void ForgetEmailPassword(ForgetPasswordInfo info)
        {
            HttpPkg.ForgePasswordRequestFormEmail(info.email, (model) => { });
        }
        protected static void ChangeEmailPassword(ForgetPasswordInfo info)
        {
            HttpPkg.ForgePasswordFormEmail(info.email, info.newPsd,info.code,(model) => {
                ShowNotification("Success");
                ClearUserJson();
            });
        }
        protected static void UploadPkg(UploadInfo uploadInfo)
        {
            MultyFrameworkEditorTool.CreateVersionJson(uploadInfo.assetPath, uploadInfo);
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
                File.Delete("Assets/../" + uploadInfo.name + ".unitypackage");
                ShowNotification("Success");
                FreshWebCollection();
            });
        }
    }
}