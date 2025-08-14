using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class ScriptBatch : EditorWindow
{
    delegate void BuildFunction(bool buildAndRun);
    private static ScriptBatch buildOptionWindow;

    JArray platformData = null;

    public static void BuildGameToWindow()
    {
        // Get filename.
        string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
        if (!string.IsNullOrEmpty(path))
        {
            string[] levels =
            {
                "Assets/Scenes/Start.unity",
                "Assets/Scenes/Login_Astar.unity",
                "Assets/Scenes/Lobby.unity",
                "Assets/Scenes/slotMachine.unity",
                "Assets/Scenes/Holdem 1.unity"
            };

            // Build player.
            BuildPipeline.BuildPlayer(
                levels,
                path + "/BuiltGame.exe",
                BuildTarget.StandaloneWindows,
                BuildOptions.None
            );

            // Copy a file from the project folder to the build folder, alongside the built game.
            Replace("Assets/Editor/ScriptBatch.cs", path + "/ScriptBatch.cs");

            // Run the game (Process class from System.Diagnostics).
            Process proc = new Process();
            proc.StartInfo.FileName = path + "BuiltGame.exe";
            proc.Start();
        }
    }

    public static void BuildGameToAndroid()
    {
        // Get filename.

        //string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "D:/Build/홀덩", "");
        string file = EditorUtility.SaveFilePanel(
            "Choose Location of Built Game",
            "../Build/Android",
            "jecpotHoldem",
            "apk"
        );
        GUILayout.Label("Scenes to include in build:", EditorStyles.boldLabel);

        var levels = EditorBuildSettings.scenes; //{"Assets/Scenes/Start.unity", "Assets/Scenes/Login_Astar.unity","Assets/Scenes/Lobby.unity","Assets/Scenes/slotMachine.unity","Assets/Scenes/Holdem 1.unity"};
        if (!string.IsNullOrEmpty(file))
        {
            var now = DateTime.Now;
            // Build player.
            BuildPipeline.BuildPlayer(levels, file, BuildTarget.Android, BuildOptions.None);
            var webPath = string.Format(
                "Z:/web/jackpot/build/Astar_{0}{1:00}{2:00}.apk",
                now.Year,
                now.Month,
                now.Day
            );
            Replace(file, webPath);
            // Copy a file from the project folder to the build folder, alongside the built game.
            //FileUtil.Replace("Assets/Editor/ScriptBatch.cs", path + "/ScriptBatch.cs");

            // Run the game (Process class from System.Diagnostics).

            Process.Start(System.IO.Path.GetDirectoryName(file));
            Process.Start(System.IO.Path.GetDirectoryName(webPath));
        }
    }

    [MenuItem("MyTools/Build")]
    public static void BuildWindow()
    {
        if (!buildOptionWindow)
        {
            buildOptionWindow = GetWindow<ScriptBatch>();
        }

        option = LoadOptions();
        buildOptionWindow.Show();
    }

    static BuildOption option = LoadOptions();
    List<string> platformNames = new List<string>();

    void OnGUI()
    {
        EditorGUILayout.LabelField("Build Platform");
        option.osSelect = GUILayout.Toolbar(
            option.osSelect,
            new string[] { "Android", "IOS", "Window", "OneStore" },
            EditorStyles.toggleGroup
        );

        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Build Mode");
        option.buildModeSelect = GUILayout.Toolbar(
            option.buildModeSelect,
            new string[] { "debug", "release" },
            EditorStyles.toggleGroup
        );
        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Build Extensions");
        option.buildExtentions = (BuildExtensions)
            GUILayout.Toolbar(
                (int)option.buildExtentions,
                new string[] { "apk", "aab" },
                EditorStyles.toggleGroup
            );

        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Scripting Define Symbols");
        EditorGUI.BeginChangeCheck();
        option.define = EditorGUILayout.TextField(option.define);

        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("BuildVersion");
        option.version = EditorGUILayout.TextArea(option.version);

        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Build Platform");
        string buildDataPath = Application.dataPath + "/Editor/BuildData.json";
        JObject buildData = null;
        //JArray platformData = null;
        if (File.Exists(buildDataPath))
        {
            string str = File.ReadAllText(buildDataPath);
            buildData = JObject.Parse(str);
            platformNames.Clear();

            platformData = buildData.GetValue("platform") as JArray;
            for (int i = 0; i < platformData.Count; i++)
            {
                JObject obj = platformData[i] as JObject;
                platformNames.Add(obj.GetValue("key").ToString());
            }
            EditorGUI.BeginChangeCheck();

            option.platform = GUILayout.SelectionGrid(
                option.platform,
                platformNames.ToArray(),
                4,
                EditorStyles.toggleGroup
            );
            string lastBuild = platformData[option.platform]["LastBuildDate"].ToString();
            string buildTime =
                (DateTime.Now.Year.ToString())
                + (DateTime.Now.Month.ToString("D2"))
                + (DateTime.Now.Day.ToString("D2"));
            if (buildTime != lastBuild)
            {
                platformData[option.platform]["BuildNumber"] = 1;
            }
            string buildNumber = ((int)platformData[option.platform]["BuildNumber"]).ToString("D2");
            PlayerSettings.bundleVersion = string.Format(
                "{0}.{1}{2:00}",
                option.version,
                buildTime,
                buildNumber
            ); // option.version + '.' + buildTime + '.' + buildNumber;
            switch (option.osSelect)
            {
                case 0:
                    PlayerSettings.Android.bundleVersionCode = (int)
                        platformData[option.platform]["BundleCode"];
                    break;

                case 1:
                    PlayerSettings.iOS.buildNumber = platformData[option.platform]
                        ["BundleCode"]
                        .ToString();
                    break;
                case 2:
                    break;
                case 3:
                    PlayerSettings.Android.bundleVersionCode = (int)
                        platformData[option.platform]["BundleCode"];
                    break;
            }
            GUILayout.Button("refresh");
            if (EditorGUI.EndChangeCheck())
            {
                DefineChange();
                SaveOptions();
                var data = (platformData[option.platform] as JObject);
                string path = data["IconPath"].ToObject<string>();
                var texture = new Texture2D(0, 0);
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                var iconSizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.iOS);
                Texture2D[] textures = { texture };
                //textures[0] = texture;
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, textures);
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, textures);
                PlayerSettings.productName = data["ProductName"].ToObject<string>();
                PlayerSettings.SetApplicationIdentifier(
                    BuildTargetGroup.Android,
                    data.ValueOrDefault<string>("PackageName", "")
                );
                PlayerSettings.SetApplicationIdentifier(
                    BuildTargetGroup.iOS,
                    data.ValueOrDefault<string>(
                        "IOSPackageName",
                        PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)
                    )
                );
                List<EditorBuildSettingsScene> levels = new List<EditorBuildSettingsScene>();
                JArray scenes = data.GetValue("scenes") as JArray;
                for (int i = 0; i < scenes.Count; i++)
                {
                    levels.Add(new EditorBuildSettingsScene(scenes[i].ToString(), true));
                }
                EditorBuildSettings.scenes = levels.ToArray();

                JArray copy = (platformData[option.platform] as JObject).ValueOrDefault<JArray>(
                    "copy",
                    new JArray()
                );
                string[] delete = (
                    platformData[option.platform] as JObject
                ).ValueOrDefault<string[]>("delete", new string[0]);
                ReplaceWithJArray(copy);
                DeleteWithJArray(delete);
                SettginOptionsConstants(data);
                Delete("Assets/StreamingAssets/google-services-desktop.json");
                Firebase.Editor.GenerateXmlFromGoogleServicesJson.ForceJsonUpdate();
            }
        }
        else
        {
            EditorGUILayout.LabelField("Your has not 'BuildData.json' please make it.");
        }
        bool saveButton = GUILayout.Button("save");
        if (saveButton)
        {
            SaveOptions();
        }

        BuildFunction build = delegate(bool buildAndRun)
        {
            // PlayerSettings.SplashScreen.show = false;
            BuildTarget target = BuildTarget.Android;
            string ex = "";
            switch (option.osSelect)
            {
                case 0:
                case 3:
                    target = BuildTarget.Android;
                    bool aabBuild = option.buildExtentions == BuildExtensions.AAB;
                    EditorUserBuildSettings.buildAppBundle = aabBuild;
                    if (platformData != null)
                    {
                        JObject platformInfo = platformData[option.platform] as JObject;
                        PlayerSettings.Android.keystoreName = platformInfo
                            .GetValue("KeystoreName")
                            .ToString();
                        PlayerSettings.Android.keystorePass = platformInfo
                            .GetValue("KeystorePass")
                            .ToString();
                        PlayerSettings.Android.keyaliasName = platformInfo
                            .GetValue("KeyaliasName")
                            .ToString();
                        PlayerSettings.Android.keyaliasPass = platformInfo
                            .GetValue("KeyaliasPass")
                            .ToString();
                        PlayerSettings.Android.bundleVersionCode = (int)platformInfo["BundleCode"];
                        JArray copy =
                            (platformData[option.platform] as JObject).GetValue("copy") as JArray;
                        string[] delete = (
                            platformData[option.platform] as JObject
                        ).ValueOrDefault<string[]>("delete", new string[0]);
                        ReplaceWithJArray(copy, true);
                        DeleteWithJArray(delete);

                        platformInfo["BundleCode"] = PlayerSettings.Android.bundleVersionCode + 1;
                        platformInfo["BuildNumber"] = (int)platformInfo["BuildNumber"] + 1;
                        string buildTime =
                            (DateTime.Now.Year.ToString())
                            + (DateTime.Now.Month.ToString("D2"))
                            + (DateTime.Now.Day.ToString("D2"));
                        platformInfo["LastBuildDate"] = buildTime;
                        PlayerSettings.Android.targetArchitectures =
                            UnityEditor.AndroidArchitecture.ARM64
                            | UnityEditor.AndroidArchitecture.ARMv7;
                        File.WriteAllText(
                            buildDataPath,
                            buildData.ToString(),
                            System.Text.Encoding.UTF8
                        );
                        UnityEngine.Debug.Log(PlayerSettings.bundleVersion);
                        UnityEngine.Debug.Log(PlayerSettings.Android.bundleVersionCode);
                    }
                    ex = aabBuild ? "aab" : "apk";
                    break;
                case 1:
                    {
                        target = BuildTarget.iOS;
                        JObject platformInfo = platformData[option.platform] as JObject;
                        platformInfo["BundleCode"] = int.Parse(PlayerSettings.iOS.buildNumber) + 1;
                        platformInfo["BuildNumber"] = (int)platformInfo["BuildNumber"] + 1;
                        string buildTime =
                            (DateTime.Now.Year.ToString().Substring(2, 2))
                            + (DateTime.Now.Month.ToString("D2"))
                            + (DateTime.Now.Day.ToString("D2"));
                        platformInfo["LastBuildDate"] = buildTime;
                        File.WriteAllText(
                            buildDataPath,
                            buildData.ToString(),
                            System.Text.Encoding.UTF8
                        );
                        UnityEngine.Debug.Log(PlayerSettings.bundleVersion);
                        UnityEngine.Debug.Log(PlayerSettings.iOS.buildNumber);
                    }
                    break;
                case 2:
                    target = BuildTarget.StandaloneWindows;
                    ex = "exe";
                    break;
            }
            string file;
            if (string.IsNullOrEmpty(ex))
            {
                file = EditorUtility.SaveFolderPanel(
                    "Choose Location of Built Game",
                    "../Build",
                    platformNames[option.platform] + "." + PlayerSettings.bundleVersion
                );
            }
            else
            {
                file = EditorUtility.SaveFilePanel(
                    "Choose Location of Built Game",
                    "../Build",
                    platformNames[option.platform] + "." + PlayerSettings.bundleVersion,
                    ex
                );
            }
            SettingInit();
            GetWindow<ScriptBatch>().Close();

            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, file, target, BuildOptions.None);
            EditorApplication.Beep();
            Process.Start(System.IO.Path.GetDirectoryName(file));
            if (buildAndRun)
            {
                if (target == BuildTarget.Android)
                {
                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "CMD.exe";

                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardInput = true;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardError = true;

                    process.EnableRaisingEvents = false;
                    process.StartInfo = startInfo;
                    process.Start(); //프로세스 시작
                    process.StandardInput.Write(
                        EditorPrefs.GetString("AndroidSdkRoot")
                            + "/platform-tools/adb.exe  install -r "
                            + file
                            + Environment.NewLine
                    );
                    process.StandardInput.Close();

                    string result = process.StandardOutput.ReadToEnd(); //실행결과를 standard output으로 받아와 string값에 저장
                    string error = process.StandardError.ReadToEnd(); //오류유무를 standard output으로 받아와 string값에 저장
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.Append("[ Result Info ]\r\n"); //출력
                    sb.Append(result);
                    sb.Append("\r\n");
                    sb.Append("[ Error Info ]\r\n");
                    sb.Append(error);

                    UnityEngine.Debug.Log(sb.ToString());

                    process.WaitForExit();
                    process.Close();
                }
                else if (target == BuildTarget.StandaloneWindows)
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = file;
                    proc.Start();
                }
            }
        };
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("");

        bool buildButton = GUILayout.Button("build");
        bool buildAndRunButton = GUILayout.Button("build and run");

        if (EditorGUI.EndChangeCheck())
        {
            if (buildButton)
            {
                build(false);
            }
            if (buildAndRunButton)
            {
                build(true);
            }
        }
    }

    public void SettginOptionsConstants(JObject data)
    {
        var path = "Assets/Constants/LinkOptionConstant.cs";
        var linkFileString =
            $"public static class LinkOptionConstant\n"
            + $"{{\n"
            + $"\tpublic const string projectName = \"{data.ValueOrDefault<string>("key", "")}\";\n"
            + $"\tpublic const string naverClientId = \"{data.ValueOrDefault<string>("naverClientId", "")}\";\n"
            + $"\tpublic const string naverClientSecret = \"{data.ValueOrDefault<string>("naverClientSecret", "")}\";\n"
            + $"\tpublic const string naverClientName = \"{data.ValueOrDefault<string>("naverClientName", "")}\";\n"
            + $"\tpublic const string storageUrl = \"{data.ValueOrDefault<string>("storageUrl", "")}\";\n"
            + $"\tpublic const string configPath = \"{data.ValueOrDefault<string>("configPath", "/config/serverConfig.json")}\";\n"
            + $"\tpublic const string webClientId = \"{data.ValueOrDefault<string>("webClientId", "")}\";\n"
            + $"\tpublic const bool addressables = {data.ValueOrDefault<bool>("addressables", true).ToString().ToLower()};\n"
            + $"}}";
        if (!Directory.Exists(Path.GetDirectoryName("Assets/Constants/LinkOptionConstant.cs")))
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName("Assets/Constants/LinkOptionConstant.cs")
            );
        }
        File.WriteAllText(path, linkFileString, System.Text.Encoding.UTF8);
    }

    public void ReplaceWithJArray(JArray copy, bool onlyBeforeBuild = false)
    {
        for (int i = 0; i < copy.Count; i++)
        {
            JObject copyData = copy[i] as JObject;
            bool useToMeta = false;
            bool beforeBuild = false;
            string from = copyData.GetValue("from").ToString();
            string to = copyData.GetValue("to").ToString();
            JToken temp;
            JToken beforeBuildToken;
            if (copyData.TryGetValue("beforeBuild", out beforeBuildToken))
            {
                beforeBuild = (bool)beforeBuildToken;
            }
            if (onlyBeforeBuild && !beforeBuild)
                continue;
            if (copyData.TryGetValue("useToMeta", out temp))
            {
                useToMeta = (bool)temp;
            }
            if (useToMeta)
            {
                string center = from + "___2";
                Replace(from, center);
                string[] deleteList = Directory.GetFiles(
                    center,
                    "*.meta",
                    SearchOption.AllDirectories
                );
                foreach (string f in deleteList)
                {
                    File.Delete(f);
                }
                string[] copyList = Directory.GetFiles(to, "*.meta", SearchOption.AllDirectories);
                foreach (string f in copyList)
                {
                    File.Copy(f, f.Replace(to, center));
                }
                Replace(center, to, true);
            }
            else
            {
                Replace(from, to);
            }
        }
    }

    public void DeleteWithJArray(string[] delete, bool onlyBeforeBuild = false)
    {
        for (int i = 0; i < delete.Length; i++)
        {
            Delete(delete[i]);
        }
    }

    private void DefineChange()
    {
        string define = string.Empty;
        if (option.osSelect == 3)
        {
            define = "ONE_STORE";
        }

        if (define.Length > 0)
        {
            define += ",";
            define += option.define;
            if (define[define.Length - 1] != ';')
            {
                define += ';';
            }
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            option.targetGroup,
            define + platformNames[option.platform]
        );
        JObject obj = platformData[option.platform] as JObject;

        PlayerSettings.useAnimatedAutorotation = true;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        // 일단 모두 on한 다음에 꺼야 에러가 나지 않는다.
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = true;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;

        Boolean t = obj.GetValue("allowedAutorotateToPortrait").ToObject<Boolean>();
        PlayerSettings.allowedAutorotateToPortrait = t;
        t = obj.GetValue("allowedAutorotateToPortraitUpsideDown").ToObject<Boolean>();
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = t;
        t = obj.GetValue("allowedAutorotateToLandscapeLeft").ToObject<Boolean>();
        PlayerSettings.allowedAutorotateToLandscapeLeft = t;
        t = obj.GetValue("allowedAutorotateToLandscapeRight").ToObject<Boolean>();
        PlayerSettings.allowedAutorotateToLandscapeRight = t;
    }

    private static void Replace(string before, string after, bool deleteBefore = false)
    {
        string rootPath = Application.dataPath;
        string[] temp = rootPath.Split('/');
        temp[temp.Length - 1] = "";
        rootPath = string.Join("/", temp);
        string path = rootPath + before;
        if (File.Exists(path) || Directory.Exists(path))
        {
            if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                FileUtil.ReplaceDirectory(before, after);
            }
            else
            {
                FileUtil.ReplaceFile(before, after);
            }
            if (deleteBefore)
            {
                FileUtil.DeleteFileOrDirectory(before);
            }
        }
        else
        {
            UnityEngine.Debug.LogError($"파일 또는 폴더가 존재하지 않음\n{path}");
        }
    }

    private static void Delete(string fileName)
    {
        string rootPath = Application.dataPath;
        string[] temp = rootPath.Split('/');
        temp[temp.Length - 1] = "";
        rootPath = string.Join("/", temp);
        string path = rootPath + fileName;

        if (File.Exists(path) || Directory.Exists(path))
        {
            File.Delete(path);
        }
        else
        {
            UnityEngine.Debug.Log($"{path} 파일이 존재하지 않습니다");
        }
    }

    private void SettingInit()
    {
        switch (option.osSelect)
        {
            case 0:
                option.buildTarget = BuildTarget.Android;
                option.targetGroup = BuildTargetGroup.Android;
                break;
            case 1:
                option.buildTarget = BuildTarget.iOS;
                option.targetGroup = BuildTargetGroup.iOS;
                break;
            case 2:
                option.buildTarget = BuildTarget.StandaloneWindows;
                option.targetGroup = BuildTargetGroup.Standalone;
                break;
            case 3:
                option.buildTarget = BuildTarget.Android;
                option.targetGroup = BuildTargetGroup.Android;
                break;
        }

        EditorUserBuildSettings.development = option.buildModeSelect == 0;
        DefineChange();
        SaveOptions();
    }

    private void SaveOptions()
    {
        string path = string.Format("./Assets/Editor/options");
        FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        BinaryFormatter bf = new BinaryFormatter();

        bf.Serialize(fs, option);
        fs.Close();
    }

    private static BuildOption LoadOptions()
    {
        BuildOption op = null;
        string path = string.Format("./Assets/Editor/options");
        FileInfo fileInfo = new FileInfo(path);
        if (fileInfo.Exists)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryFormatter bf = new BinaryFormatter();
            op = (BuildOption)bf.Deserialize(fs);

            fs.Close();
        }
        else
        {
            op = new BuildOption();
        }
        return op;
    }

    private void OnDestroy()
    {
        SettingInit();
    }
}

[Serializable]
public class BuildOption
{
    public int osSelect;
    public int buildModeSelect;
    public int platform = 0;
    public string define = "";
    public string version;
    public BuildExtensions buildExtentions = BuildExtensions.APK;
    public BuildTarget buildTarget = BuildTarget.Android;
    public BuildTargetGroup targetGroup = BuildTargetGroup.Android;
    public string keystorName;
    public string keystorePass;
    public string keyalialnName;
    public string keyaliasPass;
}

public enum BuildExtensions
{
    APK = 0,
    AAB = 1
}
