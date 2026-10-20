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

    // ---- 빌드 실행 ----
    // 편집기 스크립트(#if UNITY_IOS 같은 조건)는 '활성 플랫폼' 기준으로 미리 컴파일돼 있어서, 전환 없이
    // BuildPlayer 를 부르면 다른 플랫폼용으로 컴파일된 후처리가 실행된다(iOS 에서 CoreHaptics 미링크 등).
    // 타깃이 다르면 먼저 전환하고, 재컴파일이 끝난 뒤 자동으로 빌드를 이어간다.
    const string PendingFileKey = "ScriptBatch.pending.file";
    const string PendingTargetKey = "ScriptBatch.pending.target";
    const string PendingRunKey = "ScriptBatch.pending.run";

    static void StartBuild(string file, BuildTarget target, bool buildAndRun)
    {
        if (string.IsNullOrEmpty(file))
        {
            UnityEngine.Debug.Log("[ScriptBatch] 빌드 취소 (저장 경로 없음)");
            return;
        }
        if (EditorUserBuildSettings.activeBuildTarget == target)
        {
            DoBuild(file, target, buildAndRun);
            return;
        }

        SessionState.SetString(PendingFileKey, file);
        SessionState.SetInt(PendingTargetKey, (int)target);
        SessionState.SetBool(PendingRunKey, buildAndRun);
        UnityEngine.Debug.Log(
            $"[ScriptBatch] 활성 플랫폼 {EditorUserBuildSettings.activeBuildTarget} → {target} 전환 후 자동으로 빌드를 이어갑니다."
        );
        bool ok = EditorUserBuildSettings.SwitchActiveBuildTargetAsync(
            BuildPipeline.GetBuildTargetGroup(target),
            target
        );
        if (!ok)
        {
            ClearPending();
            UnityEngine.Debug.LogError($"[ScriptBatch] {target} 플랫폼 전환 실패 — 해당 플랫폼 모듈이 설치돼 있는지 확인하세요.");
        }
    }

    static void ClearPending()
    {
        SessionState.EraseString(PendingFileKey);
        SessionState.EraseInt(PendingTargetKey);
        SessionState.EraseBool(PendingRunKey);
    }

    // 도메인 리로드(재컴파일) 때마다 등록. 전환이 끝나고 컴파일이 멈추면 보류 중인 빌드를 실행한다.
    [InitializeOnLoadMethod]
    static void WatchPendingBuild()
    {
        if (string.IsNullOrEmpty(SessionState.GetString(PendingFileKey, string.Empty)))
        {
            return;
        }
        EditorApplication.update += ResumePendingBuild;
    }

    static void ResumePendingBuild()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }
        string file = SessionState.GetString(PendingFileKey, string.Empty);
        if (string.IsNullOrEmpty(file))
        {
            EditorApplication.update -= ResumePendingBuild;
            return;
        }
        var target = (BuildTarget)SessionState.GetInt(PendingTargetKey, (int)BuildTarget.NoTarget);
        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            return; // 아직 전환 중
        }
        bool run = SessionState.GetBool(PendingRunKey, false);
        EditorApplication.update -= ResumePendingBuild;
        ClearPending();
        UnityEngine.Debug.Log($"[ScriptBatch] {target} 전환 완료 — 빌드 시작: {file}");
        DoBuild(file, target, run);
    }

    static void DoBuild(string file, BuildTarget target, bool buildAndRun)
    {
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

    // ---- 스토어 구분(원스토어로 빌드해야 하는데 구글 플레이로 빌드하는 실수 방지) ----
    // osSelect: 0 Android(구글 플레이), 1 iOS, 2 Windows, 3 OneStore. 0 과 3 은 패키지·버전 코드가 같아 결과물만으로는 구분이 안 된다.
    static string StoreLabel(int os) =>
        os switch { 0 => "구글 플레이", 1 => "iOS", 2 => "Windows", 3 => "원스토어", _ => "?" };

    static string StoreTag(int os) =>
        os switch { 0 => "google", 1 => "ios", 2 => "windows", 3 => "onestore", _ => "unknown" };

    static string StoreFolder(int os) =>
        os switch { 0 => "GooglePlay", 1 => "iOS", 2 => "Windows", 3 => "OneStore", _ => "" };

    static Color StoreColor(int os) =>
        os switch
        {
            0 => new Color(0.18f, 0.62f, 0.30f), // 구글 플레이: 초록
            3 => new Color(0.95f, 0.45f, 0.10f), // 원스토어: 주황
            _ => new Color(0.35f, 0.35f, 0.40f),
        };

    // 창 맨 위 색 띠: 지금 어느 스토어로 빌드되는지 한눈에
    static void DrawStoreBanner(int os)
    {
        var rect = GUILayoutUtility.GetRect(0, 36, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, StoreColor(os));
        var style = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            normal = { textColor = Color.white },
        };
        GUI.Label(rect, $"빌드 대상: {StoreLabel(os)}" + (os == 3 ? "  (ONE_STORE)" : ""), style);
        EditorGUILayout.Space(6);
    }

    // Android 빌드 버튼을 누르면 스토어를 고르는 창(원스토어 / 구글 플레이). 위 툴바에서 무엇을 골라 뒀든 여기서 정한다.
    // 고른 스토어 번호(0 구글 플레이, 3 원스토어)를 돌려준다. 취소면 -1 — 버전 코드도 올리지 않는다
    static int ChooseAndroidStore(JObject platformInfo, string ex, bool release)
    {
        string package = platformInfo?.ValueOrDefault<string>("PackageName", "") ?? "";
        string code = platformInfo != null ? platformInfo["BundleCode"]?.ToString() ?? "" : "";
        string msg =
            $"패키지: {package}\n"
            + $"버전: {PlayerSettings.bundleVersion}\n"
            + $"버전 코드: {code}\n"
            + $"파일: {ex}\n"
            + $"모드: {(release ? "release" : "debug")}";
        int picked = EditorUtility.DisplayDialogComplex(
            "어느 스토어로 빌드할까요?",
            msg,
            "원스토어",
            "취소",
            "구글 플레이"
        );
        return picked switch { 0 => 3, 2 => 0, _ => -1 };
    }

    void OnGUI()
    {
        DrawStoreBanner(option.osSelect);
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
            int os = option.osSelect;
            string ex =
                (os == 0 || os == 3) ? (option.buildExtentions == BuildExtensions.AAB ? "aab" : "apk")
                : os == 2 ? "exe"
                : "";
            // Android 면 스토어부터 고른다 → 저장 위치를 먼저 받는다. 어느 쪽이든 취소하면 버전 코드를 올리지 않고 끝낸다
            if (os == 0 || os == 3)
            {
                JObject selectedInfo = platformData != null ? platformData[option.platform] as JObject : null;
                int picked = ChooseAndroidStore(selectedInfo, ex, option.buildModeSelect == 1);
                if (picked < 0)
                {
                    UnityEngine.Debug.Log("[ScriptBatch] 빌드 취소 (스토어 선택)");
                    return;
                }
                os = option.osSelect = picked; // 정의 기호(ONE_STORE)는 아래 SettingInit → DefineChange 가 이 값으로 맞춘다
            }
            // 파일 이름·기본 폴더에 스토어를 붙인다: ../Build/OneStore/Kings.1.xx_onestore.apk
            string baseName =
                platformNames[option.platform] + "." + PlayerSettings.bundleVersion + "_" + StoreTag(os);
            string dir = "../Build/" + StoreFolder(os);
            Directory.CreateDirectory(dir);
            string file = string.IsNullOrEmpty(ex)
                ? EditorUtility.SaveFolderPanel("Choose Location of Built Game", dir, baseName)
                : EditorUtility.SaveFilePanel("Choose Location of Built Game", dir, baseName, ex);
            if (string.IsNullOrEmpty(file))
            {
                UnityEngine.Debug.Log("[ScriptBatch] 빌드 취소 (저장 경로 없음)");
                return;
            }
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
                    break;
            }
            SettingInit();
            GetWindow<ScriptBatch>().Close();

            StartBuild(file, target, buildAndRun);
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
