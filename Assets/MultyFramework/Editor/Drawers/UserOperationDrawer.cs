﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

namespace MultyFramework
{
    class UserOperationDrawer : MultyFrameworkDrawer
    {

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

        private LoginInfo _loginInfo;
        private RegisterInfo _registerInfo;
        private UploadInfo _uploadInfo;



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
                    ClearUserJson();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(Contents.gap);

                GUILayout.Label("Self Web Packages", Styles.in_title);
                var _selfInfos = window.multyDrawersInfo.selfInfos;
                for (int i = 0; i < _selfInfos.Count; i++)
                {
                    for (int j = 0; j < _selfInfos[i].versions.Length; j++)
                    {
                        GUILayout.BeginHorizontal(Styles.box);
                        GUILayout.Label(_selfInfos[i].name);
                        GUILayout.Label(_selfInfos[i].versions[j].unityVersion);
                        GUILayout.Label(_selfInfos[i].versions[j].version);
                        if (GUILayout.Button("", Styles.minus, GUILayout.Width(Contents.gap * 2)))
                        {
                            HttpPkg.DeletePkg(_selfInfos[i].name, _selfInfos[i].versions[j].version, (m) =>
                            {
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
                        TryLogin(_loginInfo.email, _loginInfo.password);
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
                Signup(_registerInfo.nick_name, _registerInfo.email, _registerInfo.password);
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
                    UploadPkg(_uploadInfo);
                }
                GUILayout.EndHorizontal();

            }
        }


        public override void Awake()
        {
            _loginInfo = new LoginInfo();
            _registerInfo = new RegisterInfo();
            _uploadInfo = new UploadInfo();
        }

      

    }
}
