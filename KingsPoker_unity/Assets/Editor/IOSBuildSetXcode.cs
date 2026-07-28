using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class IOSBuildSetXcode
{
    [PostProcessBuild]
    public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            // Get plist
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            // Get root
            PlistElementDict rootDict = plist.root;

            // Change value of CFBundleVersion in Xcode plist
            //카카오 앱 키
            var buildKey = "KAKAO_APP_KEY";
            var kakaoAppKey = "3f69cffeb163f567f3b1c122c74d6b03";

            rootDict.SetString(buildKey, kakaoAppKey);

            var bgModes = rootDict.CreateArray("LSApplicationQueriesSchemes");
            // kakao+카카오 앱 키
            bgModes.AddString($"kakao{kakaoAppKey}");
            bgModes.AddString("kakaokompassauth");
            bgModes.AddString("storykompassauth");

            //앱네임 셋팅
            rootDict.SetString("CFBundleDisplayName", Application.productName);
            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            // URL 스키마 추가

            var array = plist.root.CreateArray("CFBundleURLTypes");
            var urlDict = array.AddDict();

            urlDict.SetString("CFBundleURLName", "kakaoLogin");
            var urlInnerArray = urlDict.CreateArray("CFBundleURLSchemes");
            // kakao+카카오 앱 키
            urlInnerArray.AddString($"kakao{kakaoAppKey}");
            // naver 스키마
            urlDict = array.AddDict();
            urlDict.SetString("CFBundleURLName", "naverlogin");
            urlInnerArray = urlDict.CreateArray("CFBundleURLSchemes");
            urlInnerArray.AddString("naveroauthlogin");

            File.WriteAllText(plistPath, plist.WriteToString());

            // Write to file
            //------------------------------------plist 파일 복사 및 프로젝트에 추가---------------
            // Go get pbxproj file
            string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";

            // PBXProject class represents a project build settings file,
            // here is how to read that in.
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);

            string projectGuid = proj.ProjectGuid();

            // This is the Xcode target in the generated project
            string targetGuid = proj.GetUnityMainTargetGuid();

            // Copy plist from the project folder to the build folder
            FileUtil.CopyFileOrDirectory(
                "./Assets/Plugins/iOS/GoogleSignIn/GoogleService-Info-GoogleSignin.plist",
                pathToBuiltProject + "/GoogleService-Info-GoogleSignin.plist"
            );
            proj.AddFileToBuild(
                targetGuid,
                proj.AddFile(
                    "GoogleService-Info-GoogleSignin.plist",
                    "GoogleService-Info-GoogleSignin.plist"
                )
            );

            // Product_name_app이 내 유니티  app name으로 설정 안되는 현상 해결
            proj.SetBuildProperty(projectGuid, "PRODUCT_NAME_APP", Application.productName);

            proj.WriteToFile(projPath);
            // Write PBXProject object back to the file
            EditUnityAppController(pathToBuiltProject);

            // Push Notifications capability + Background Modes(Remote notifications) 자동 추가
            // Apple 로그인 후처리(SignInWithApplePostprocessor)와 같은 Entitlements.entitlements 파일을 쓰므로 병합된다
            string pbxProjectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            ProjectCapabilityManager capabilityManager = new ProjectCapabilityManager(
                pbxProjectPath,
                "Entitlements.entitlements",
                null,
                proj.GetUnityMainTargetGuid()
            );
            // 개발 빌드면 aps-environment=development, 아니면 production (스토어 배포 시 재서명에서 production 적용)
            capabilityManager.AddPushNotifications(EditorUserBuildSettings.development);
            capabilityManager.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
            capabilityManager.WriteToFile();


            //string pbxProjectContent = File.ReadAllText(pbxProjectPath);
            //pbxProjectContent = pbxProjectContent.Replace(
            //    "PRODUCT_NAME_APP = ProductName;", "PRODUCT_NAME_APP = KlassicPoker;");
            //File.WriteAllText(pbxProjectPath, pbxProjectContent);
        }
    }

    static void EditUnityAppController(string pathToBuiltProject)
    {
        //Edit UnityAppController.mm
        XClass UnityAppController = new XClass(
            pathToBuiltProject + "/Classes/UnityAppController.mm"
        );
        //Refer to the header file of the third-party SDK
        string below =
            "- (BOOL)application:(UIApplication*)app openURL:(NSURL*)url options:(NSDictionary<NSString*, id>*)options\n{\n";
        string text =
            "if ([KOSession isKakaoAccountLoginCallback:url]) {return [KOSession handleOpenURL:url];}\n";
        UnityAppController.WriteBelow(below, text);
        below = "- (void)applicationDidBecomeActive:(UIApplication*)application\n{\n";
        text = "[KOSession handleDidBecomeActive];\n";
        UnityAppController.WriteBelow(below, text);
        UnityAppController.WriteFirst("#import <KakaoOpenSDK/KakaoOpenSDK.h>");
        //add code:  [[ThirdSDK sharedInstance] showSplash:@"appkey" withWindow:self.window blockid:@"blockid"]; return YES;
    }
}

public partial class XClass : System.IDisposable
{
    private string filePath;

    public XClass(string fPath)
    {
        filePath = fPath;
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogError(filePath + "The file does not exist under the path!");
            return;
        }
    }

    public void Replace(string oldStr, string newStr, string method = "")
    {
        if (!File.Exists(filePath))
        {
            return;
        }
        bool getMethod = false;
        string[] codes = File.ReadAllLines(filePath);
        for (int i = 0; i < codes.Length; i++)
        {
            string str = codes[i].ToString();
            if (string.IsNullOrEmpty(method))
            {
                if (str.Contains(oldStr))
                    codes.SetValue(newStr, i);
            }
            else
            {
                if (!getMethod)
                {
                    getMethod = str.Contains(method);
                }
                if (!getMethod)
                    continue;
                if (str.Contains(oldStr))
                {
                    codes.SetValue(newStr, i);
                    break;
                }
            }
        }
        File.WriteAllLines(filePath, codes);
    }

    public void WriteBelow(string below, string text)
    {
        StreamReader streamReader = new StreamReader(filePath);
        string text_all = streamReader.ReadToEnd();
        streamReader.Close();
        int beginIndex = text_all.IndexOf(below);
        if (beginIndex == -1)
        {
            return;
        }
        int endIndex = text_all.LastIndexOf("\n", beginIndex + below.Length);
        text_all =
            text_all.Substring(0, endIndex) + "\n" + text + "\n" + text_all.Substring(endIndex);
        StreamWriter streamWriter = new StreamWriter(filePath);
        streamWriter.Write(text_all);
        streamWriter.Close();
    }

    public void WriteFirst(string text)
    {
        StreamReader streamReader = new StreamReader(filePath);
        string text_all = streamReader.ReadToEnd();
        streamReader.Close();

        text_all = text + "\n" + text_all;
        StreamWriter streamWriter = new StreamWriter(filePath);
        streamWriter.Write(text_all);
        streamWriter.Close();
    }

    public void Dispose() { }
}
