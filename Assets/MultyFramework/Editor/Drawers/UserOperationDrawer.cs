using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

namespace MultyFramework
{
    class UserOperationDrawer : MultyFrameworkDrawer
    {
        private class UserJson
        {
            public string email;
            public string token;
            public string name;
        }
        private class LoginInfo
        {
            public string email;
            public string password;
            public bool see;
        }
        private class RegisterInfo
        {
            public string email;
            public string password;
            public string nick_name;
        }

        private enum UserOperation
        {
            register, Login, ForgetPassword, Upload
        }
        private UserOperation _userOperation;

        private const string _describtion = "MultyFramework  version: " + MultyFrameworkEditorTool.version + " \n" +
                                            "\n you can enjoy to use it";
        private string[] _dependences = new string[] {
            "MultyFramework"
        };

        public override string name { get { return "MultyFramework User Operation"; } }
        public override string version { get { return MultyFrameworkEditorTool.version; } }
        public override string author { get { return "OnClick"; } }
        public override string describtion { get { return _describtion; } }
       
        public override string[] dependences { get { return _dependences; } }
        public override string helpurl { get { return MultyFrameworkEditorTool.frameworkUrl; } }
        private string _userjsonPath { get { return MultyFrameworkEditorTool.rootPath + "/user.json"; } }
        private Encoding _encoding = Encoding.UTF8;
        private LoginInfo _loginInfo;
        private RegisterInfo _registerInfo;
        private MultyFrameworkEditorTool.UploadInfo _uploadInfo;
        private UserJson _userJson;
        private bool _login;

        protected override void ToolGUI()
        {
            _userOperation = (UserOperation)GUILayout.Toolbar((int)_userOperation, Enum.GetNames(typeof(UserOperation)),GUILayout.Height(Contents.gap*2.5f));
            GUILayout.Space(Contents.gap/2);
            switch (_userOperation)
            {
                case UserOperation.register:
                    RegisterGUI();
                    break;
                case UserOperation.Login:
                    LoginGUI();
                    break;
                case UserOperation.ForgetPassword:
                    break;
                case UserOperation.Upload:
                    GUI.enabled = _login;
                    UploadGUI();
                    GUI.enabled = true;
                    break;
                default:
                    break;
            }
            GUILayout.Space(Contents.gap );
        }

        private void LoginGUI()
        {
            if (_login)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Logout"))
                {
                    _login = false;
                    _userJson = new UserJson();
                    if (File.Exists(_userjsonPath))
                    {
                        File.Delete(_userjsonPath);
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(Contents.gap);

                GUILayout.Label("Self Web Packages", Styles.in_title);
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.xMin = rect.xMax - 20;
                rect.width = 20;
                
                if (GUI.Button(rect,Contents.refresh))
                {
                    FreshWebCollection();
                }
                for (int i = 0; i < _selfInfos.Count; i++)
                {
                    for (int j = 0; j < _selfInfos[i].versions.Length; j++)
                    {
                        GUILayout.BeginHorizontal(Styles.box);
                        GUILayout.Label(_selfInfos[i].name);
                        GUILayout.Label(_selfInfos[i].versions[j].unityVersion);
                        GUILayout.Label(_selfInfos[i].versions[j].version);
                        if (GUILayout.Button("",Styles.minus,GUILayout.Width(Contents.gap*2)))
                        {
                            HttpPkg.DeletePkg(_selfInfos[i].name, _selfInfos[i].versions[j].version, (m) => {
                                ShowNotification("Delete Sucess");
                            });
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                {
                    GUILayout.BeginHorizontal();
                    _loginInfo.email = EditorGUILayout.TextField("Email", _loginInfo.email);
                    GUILayout.Space(Contents.gap + 2);
                    GUILayout.EndHorizontal();
                }
                {
                    GUILayout.BeginHorizontal();
                    if (!_loginInfo.see)
                    {
                        _loginInfo.password = EditorGUILayout.TextField("Password", _loginInfo.password);
                    }
                    else
                    {
                        _loginInfo.password = EditorGUILayout.PasswordField("Password", _loginInfo.password);
                    }
                    _loginInfo.see = GUILayout.Toggle(_loginInfo.see, "", Styles.in_LockButton);
                    GUILayout.EndHorizontal();

                }
                {
                    GUILayout.Space(Contents.gap);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(Contents.Go, GUILayout.Width(Contents.gap * 5)))
                    {
                        if (string.IsNullOrEmpty(_loginInfo.email))
                        {
                            ShowNotification("Err: email is null");
                            return;
                        }
                        if (string.IsNullOrEmpty(_loginInfo.password))
                        {
                            ShowNotification("Err: password is null");
                            return;
                        }
                        HttpPkg.Login(_loginInfo.email, _loginInfo.password,(model)=> {
                            _login = true;
                            _userJson = new UserJson()
                            {
                                email = _loginInfo.email,
                                token = model.data.token,
                                name=model.data.nick_name
                            };
                            File.WriteAllText(_userjsonPath, JsonUtility.ToJson(_userJson, true), _encoding);
                            FreshWebCollection();
                        });
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }

        private void RegisterGUI()
        {
            _registerInfo.email = EditorGUILayout.TextField("Email", _registerInfo.email);
            _registerInfo.nick_name = EditorGUILayout.TextField("Name", _registerInfo.nick_name);
            _registerInfo.password = EditorGUILayout.TextField("Password", _registerInfo.password);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Contents.Go,GUILayout.Width(Contents.gap*5)))
            {
                if (string.IsNullOrEmpty(_registerInfo.email))
                {
                    ShowNotification("Err: email is null");
                    return;
                }
                if (string.IsNullOrEmpty(_registerInfo.password))
                {
                    ShowNotification("Err: password is null");
                    return;
                }
                if (string.IsNullOrEmpty(_registerInfo.nick_name))
                {
                    ShowNotification("Err: Name is null");
                    return;
                }
                HttpPkg.Signup(_registerInfo.nick_name, _registerInfo.email, _registerInfo.password,(model)=> {
                    _userJson = new UserJson()
                    {
                        email = _registerInfo.email,
                        token = model.data.token,
                        name = _registerInfo.nick_name
                    };
                    token = model.data.token;
                    File.WriteAllText(_userjsonPath, JsonUtility.ToJson(_userJson, true), _encoding);
                    ShowNotification("Success");
                    LoginWithToken();
                });
            }
            GUILayout.EndHorizontal();
        }

        private void UploadGUI()
        {
            _uploadInfo.author = _userJson.name;
            GUILayout.Label("Upload:", Styles.boldLabel);
            EditorGUILayout.LabelField("Unity Version", _uploadInfo.unityVersion);
            EditorGUILayout.LabelField("Author", _uploadInfo.author);
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("AssetPath", _uploadInfo.assetPath);
                if (GUILayout.Button("Select",GUILayout.Width(50)))
                {
                    _uploadInfo.assetPath = EditorUtility.OpenFolderPanel("Choose Your Code Root Path", "Assets", "");
                    if (!_uploadInfo.assetPath.Contains("Assets"))
                        _uploadInfo.assetPath = string.Empty;
                    else
                    {
                        int index = _uploadInfo.assetPath.IndexOf("Assets");
                        _uploadInfo.assetPath = _uploadInfo.assetPath.Remove(0, index);
                    }
                }
                GUILayout.EndHorizontal();
            }
            _uploadInfo.name = EditorGUILayout.TextField("Name", _uploadInfo.name);
            _uploadInfo.version = EditorGUILayout.TextField("Version", _uploadInfo.version);
            _uploadInfo.isPublic = EditorGUILayout.Toggle("Public", _uploadInfo.isPublic);
            _uploadInfo.helpurl = EditorGUILayout.TextField("Helpurl", _uploadInfo.helpurl);


            GUILayout.Label("Dependences", Styles.boldLabel);
            for (int i = 0; i < _uploadInfo.dependences.Count; i++)
            {
                GUILayout.BeginHorizontal();
                _uploadInfo.dependences[i] = EditorGUILayout.TextField(_uploadInfo.dependences[i]);
                if (GUILayout.Button("",Styles.minus,GUILayout.Width(18)))
                {
                    _uploadInfo.dependences.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("", Styles.plus))
            {
                _uploadInfo.dependences.Add("");
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Describtion");
            _uploadInfo.describtion = EditorGUILayout.TextArea(_uploadInfo.describtion, GUILayout.MinHeight(100));



            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Contents.Go, GUILayout.Width(Contents.gap * 5)))
                {
                    MultyFrameworkEditorTool.CreateVersionJson(_uploadInfo.assetPath, _uploadInfo);
                    AssetDatabase.ExportPackage(_uploadInfo.assetPath, _uploadInfo.name + ".unitypackage", ExportPackageOptions.Recurse);
                    byte[] bytes = File.ReadAllBytes("Assets/../" + _uploadInfo.name + ".unitypackage");
                    PkgInfo form = new PkgInfo()
                    {
                        unity_version = _uploadInfo.unityVersion,
                        author = _uploadInfo.author,
                        pkg_path = _uploadInfo.assetPath,
                        pkg_name = _uploadInfo.name,
                        version = _uploadInfo.version,
                        permissions = _uploadInfo.isPublic ? PkgConstant.PKG_PERMISSIONS_PUBLIC : PkgConstant.PKG_PERMISSIONS_PRIVATE,
                        help_url = _uploadInfo.helpurl,
                        describtion = _uploadInfo.describtion,
                    };
                    for (int i = 0; i < _uploadInfo.dependences.Count; i++)
                    {
                        form.AddDependences(_uploadInfo.dependences[i]);
                    }
                    HttpPkg.UploadPkg(form, bytes, (m) => {
                        File.Delete("Assets/../" + _uploadInfo.name + ".unitypackage");
                        ShowNotification("Success");
                        FreshWebCollection();
                    });
                }
                GUILayout.EndHorizontal();

            }
        }


        public override void Awake()
        {
            _loginInfo = new LoginInfo();
            _registerInfo = new RegisterInfo();
            _uploadInfo = new MultyFrameworkEditorTool.UploadInfo();
            _login = false;
            if (File.Exists(_userjsonPath))
            {
                _userJson = JsonUtility.FromJson<UserJson>(File.ReadAllText(_userjsonPath));
                token = _userJson.token;
                LoginWithToken();
            }
            else
            {
                _userJson = new UserJson();
                token = _userJson.token;
                if (window.needReload)
                {
                    FreshWebCollection();
                }
            }
        }

        private void LoginWithToken()
        {
            HttpPkg.CheckToken(
                _userJson.token
                , (model) =>
                {
                    _login = true;
                    if (window.needReload)
                    {
                        FreshWebCollection();
                    }
                });
        }
        private void FreshWebCollection()
        {
            HttpPkg.GetPkgInfoList((m) => {
                var names = m.data;
                for (int i = 0; i < names.Count; i++)
                {
                    HttpPkg.GetPkgInfos(names[i],  
                        (model) => {
                            CollectionInfo info = new CollectionInfo()
                            {
                                name= names[i],
                                author = model.data[0].author,
                            };
                            CollectionInfo.Version[] versions = new CollectionInfo.Version[model.data.Count];
                            for (int j = 0; j < model.data.Count; j++)
                            {
                                versions[j] = new CollectionInfo.Version()
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
                            CollectInfos(info, i, names.Count);
                        });
                }
            });
        }
        private List<CollectionInfo> _selfInfos = new List<CollectionInfo>();
        private List<CollectionInfo> _infos = new List<CollectionInfo>();
        private void CollectInfos(CollectionInfo info,int index,int count)
        {
            if (index==0)
            {
                _infos.Clear();
            }
            _infos.Add(info);
            if (index==count-1)
            {
                if (_login)
                {
                    _selfInfos.Clear();
                    for (int i = 0; i < _infos.Count; i++)
                    {
                        if (_infos[i].author == _userJson.name)
                        {
                            _selfInfos.Add(_infos[i]);
                        }
                    }
                }

                window.FreshCollection(_infos);
            }
        }
    }
}
