#if MutiFramework

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

#pragma  warning disable 0649
namespace MutiFramework
{
    abstract class MutiFrameworkDrawer: PanelGUIDrawer
    {
        protected static string token;
        public static class PkgConstant
        {
            public const string HOST = "https://upkg.org/api/";
           // public const string HOST = "https://pkg.tdouguo.com/api/";
           // public const string HOST = "http://127.0.0.1:5000/api/";

            // user
            public const string API_LOGIN = HOST + "login";
            public const string API_SIGNUP = HOST + "signup";

            // pkg
            public const string API_UPLOAD_PKG = HOST + "upload_pkg";
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
            public string err;

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
                        break;
                    case (int)Code.AUTHORIZATION_ERROR:
                        // todo: 重新登录
                        return false;
                    default:
                        // todo: tip msg
                        return false;
                }

                return true;
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
            public List<string> data;
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


            public List<string> GetDependences()
            {
                return dependences.Split(',').ToList();
            }

            public void AddDependences(string pkgName)
            {
                var d = GetDependences().ToList();
                d.Add(pkgName);
                dependences = string.Join(",", d);
            }

            public void RemoveDependences(string pkgName)
            {
                var d = GetDependences().ToList();
                d.Remove(pkgName);
                dependences = string.Join(",", d);
            }
        }
        public class HttpPkg
        {
            private static void GetRequest(string url, Dictionary<string, object> forms, Action<UnityWebRequest> callback, bool addToken = false)
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
                var req = UnityWebRequest.Get(newUrl);
                if (addToken)
                {
                    if (string.IsNullOrEmpty(token))
                    {
                        ShowNotification("token is Null , Please Login First");
                        return;
                    }
                    req.SetRequestHeader("token", token);
                }

              //  req.SetRequestHeader("device_info", JsonUtility.ToJson(GetDeviceInfo()));
                req.SendWebRequest();
                while (!req.isDone)
                {
                    DisplayProgressBar("Post Request", req.uri.Host, req.downloadProgress);
                }
                ClearProgressBar();
                if (!string.IsNullOrEmpty(req.error))
                {
                    DisplayDialog("Err", string.Format("GetRequest url:{0}, error:{1}", req.url, req.error));
                    return;
                }
                if (callback!=null)
                {
                    callback.Invoke(req);
                }
                req.Abort();
            }

            private static void PostRequest(string url, WWWForm forms, Action<UnityWebRequest> callback,bool addToken = false)
            {
                if (forms == null)
                {
                    forms = new WWWForm();
                }

                var req = UnityWebRequest.Post(url, forms);
                if (addToken)
                {
                    if (string.IsNullOrEmpty(token))
                    {
                        ShowNotification("token is Null , Please Login First");
                        return;
                    }
                    req.SetRequestHeader("token", token);
                }

              //  req.SetRequestHeader("device_info", JsonUtility.ToJson(GetDeviceInfo()));
                req.SendWebRequest();
                while (!req.isDone)
                {
                    DisplayProgressBar("Post Request", url, req.downloadProgress);
                }
                ClearProgressBar();

                if (!string.IsNullOrEmpty(req.error))
                {
                    DisplayDialog("Err", string.Format("GetRequest url:{0}, error:{1}", req.url, req.error));
                    return;
                }
                if (callback != null)
                {
                    callback.Invoke(req);
                }
                req.Abort();
            }

            private static void GetRequest<T>(string url, Dictionary<string, object> forms, Action<T> callback,bool addToken = false)where T : ResponseModel
            {
                GetRequest(url, forms, (req) =>
                {
                    T t = ResponseModel.Dispose<T>(url, req.downloadHandler.text);
                    if (t == null)
                    {
                        DisplayDialog("Dispose Err", req.downloadHandler.text);
                        return;
                    }
                    t.CheckCode(false);
                    if (callback != null)
                    {
                        callback.Invoke(t);
                    }
                }, addToken);
            }

            private static void PostRequest<T>(string url, WWWForm forms, Action<T> callback, bool addToken = false) where T : ResponseModel
            {
                PostRequest(url, forms, (req) =>
                {
                    T t = ResponseModel.Dispose<T>(url, req.downloadHandler.text);
                    if (t == null)
                    {
                        DisplayDialog("Dispose Err", req.downloadHandler.text);
                        return;
                    }

                    t.CheckCode(false);
                    if (callback != null)
                    {
                        callback.Invoke(t);
                    }
                }, addToken);
            }

            #region API.User

            public static void CheckToken(string token, Action<LoginModel> callback)
            {
                GetRequest<LoginModel>(PkgConstant.API_LOGIN, null, (m) =>
                {
                    m.data.token = token;
                    callback?.Invoke(m);
                }, true);
            }

            public static void Login(string email, string pwd, Action<LoginModel> callback)
            {
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddField("user_type", (int)UserType.EMAIL);
                wwwForm.AddField("email", email);
                wwwForm.AddField("password", pwd);
                PostRequest<LoginModel>(PkgConstant.API_LOGIN, wwwForm, callback);
            }

            public static void Signup(string nickName, string email, string pwd, Action<SignupModel> callback)
            {
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddField("user_type", (int)UserType.EMAIL);
                wwwForm.AddField("nick_name", nickName);
                wwwForm.AddField("email", email);
                wwwForm.AddField("password", pwd);
                PostRequest<SignupModel>(PkgConstant.API_SIGNUP, wwwForm, callback);
            }

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
                url += "&version=" + version;
                url += "&token=" + token;
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Timeout = 5000;
                WebResponse response = request.GetResponse();
                using (FileStream fs = new FileStream(downloadPath, FileMode.Create))
                using (Stream netStream = response.GetResponseStream())
                {
                    int packLength = 1024 * 20;
                    long countLength = response.ContentLength;
                    byte[] nbytes = new byte[packLength];
                    int nReadSize = 0;
                    nReadSize = netStream.Read(nbytes, 0, packLength);
                    while (nReadSize > 0)
                    {
                        fs.Write(nbytes, 0, nReadSize);
                        nReadSize = netStream.Read(nbytes, 0, packLength);
                        double dDownloadedLength = fs.Length * 1.0 / (1024 * 1024);
                        double dTotalLength = countLength * 1.0 / (1024 * 1024);
                        string tip = string.Format("已下载 {0:F}M / {1:F}M", dDownloadedLength, dTotalLength);
                        EditorUtility.DisplayProgressBar("Download pkg", tip, (float)(dDownloadedLength / dTotalLength));
                    }

                    EditorUtility.ClearProgressBar();
                    netStream.Close();
                    fs.Close();
                    onCompleted?.Invoke();
                }
            }
            #endregion
        }

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

    }
}
#endif
