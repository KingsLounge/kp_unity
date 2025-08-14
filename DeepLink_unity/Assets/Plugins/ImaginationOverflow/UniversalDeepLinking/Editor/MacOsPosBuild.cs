using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ImaginationOverflow.UniversalDeepLinking.Editor.Xcode;
using ImaginationOverflow.UniversalDeepLinking.Editor.Xcode.PBX;
using UnityEditor;

using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ImaginationOverflow.UniversalDeepLinking.Editor
{
    public class MacOsPosBuild : IPosBuilder
    {
        const string FrameworkName = "UniversalDeepLink.framework";

        private bool IsXcodeBuild(string path)
        {
            return path.EndsWith(".xcodeproj");
        }
        public void PostBuildProcess(AppLinkingConfiguration configuration, string pathToBuiltProject)
        {

#if UDL_DEBUG
            Debug.Log("UDL: path is " + pathToBuiltProject + " OS " + SystemInfo.operatingSystem + " Is Mac " + IsBuiltOnMac());
#endif
            //
            //  Unity sometimes doesn't include the .app in the pathToBuiltProject
            //
            if (Directory.Exists(pathToBuiltProject) == false)
                pathToBuiltProject += ".app";

            AddDeepLinks(configuration, pathToBuiltProject);

            if (IsXcodeBuild(pathToBuiltProject))
            {
                ExtractFrameworkAndAddItToXcode(pathToBuiltProject);
            }

            else
            {
                CopyUniversalDeepLinkFramework(pathToBuiltProject);


                if (IsBuiltOnMac())
                {
                    InjectFrameworkInGameApp(pathToBuiltProject);
                }
                else
                    AddScriptsToIncludeFramework(pathToBuiltProject);
            }



        }

        private void ExtractFrameworkAndAddItToXcode(string pathToBuiltProject)
        {
            //
            //  Extracts framework
            //
            const string frameworksDir = "Frameworks";
            var pathToExport = Directory.GetParent(GetMacOsProjectPath(pathToBuiltProject)).FullName;
            var pathToLib = Path.Combine(Application.dataPath, EditorHelpers.PluginPath + "/libs/Standalone/UniversalDeepLink.framework.zip");


            ShellHelper.ShellRequest unzip = ShellHelper.ProcessFileCommand("unzip", string.Format("-o \"{0}\" -d \"{1}\"", pathToLib, pathToExport));

            unzip.OnDone += () =>
            {
                string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                PBXProject project = new PBXProject();
                project.ReadFromString(File.ReadAllText(projectPath));

                //
                //  Adds framework to pbx
                //
                var frameworkInProject = Path.Combine(Application.productName, frameworksDir);
                frameworkInProject = Path.Combine(frameworkInProject, FrameworkName);
                var prodGuid = project.TargetGuidByName(Application.productName);

                var myFramework = project.AddFile(FrameworkName, frameworkInProject, PBXSourceTree.Source);
                project.AddFileToBuild(prodGuid, myFramework);
                PBXProjectExtensions.AddFileToEmbedLibraries(project, prodGuid, myFramework);

                File.WriteAllText(projectPath, project.WriteToString());

            };


        }


        private void AddScriptsToIncludeFramework(string pathToBuiltProject)
        {
            var parent = Directory.GetParent(pathToBuiltProject);

            var dir = Directory.CreateDirectory(Path.Combine(parent.FullName, "UniversalDeepLinkingScripts"));

            File.Copy(Path.Combine(Application.dataPath, EditorHelpers.PluginPath + "/libs/Tools/optool"), Path.Combine(dir.FullName, "optool"), true);
            File.Copy(Path.Combine(Application.dataPath, EditorHelpers.PluginPath + "/libs/Standalone/UniversalDeepLink.framework.zip"), Path.Combine(dir.FullName, "UniversalDeepLink.framework.zip"), true);

            var appDir = new DirectoryInfo(pathToBuiltProject).Name.Replace(".app", string.Empty);

            //var parentDir = Path.Combine(pathToBuiltProject, "Contents/MacOS/");

            //var gameExec = Directory.GetFiles(parentDir).First();

            //gameExec = Path.GetFileName(gameExec);

            using (TextWriter fileTW = new StreamWriter(Path.Combine(dir.FullName, "setup.sh")))
            {
                fileTW.NewLine = "\n";
                fileTW.WriteLine("#!/bin/bash");
                fileTW.WriteLine("rm -r ../{0}.app/Contents/Frameworks/UniversalDeepLink.framework", appDir);
                fileTW.WriteLine("unzip -o  UniversalDeepLink.framework.zip -d ../{0}.app/Contents/Frameworks/", appDir);
                fileTW.WriteLine("./optool install -c load -p \"@executable_path/../Frameworks/UniversalDeepLink.framework/Versions/A/UniversalDeepLink\" -t ../{0}.app/Contents/MacOS/{0}", appDir);
            }

            Debug.Log("Deep Linking not configured for Mac! Since you are building on Windows we are unable to completely configure your game to use the Universal Deeplinking Plugin.\nWe created a folder named UniversalDeepLinkingScripts by your deliverable with a script that you need to run in order to fully use the plugin. Check https://universaldeeplinking.imaginationoverflow.com/docs/GettingStarted/#building-for-macos-on-windows for more info");

        }

        private PlistDocument GetPlistFile(string pathToBuiltProject, out string plistPath)
        {

            var plist = new PlistDocument();

            plistPath = pathToBuiltProject + "/Contents/Info.plist";

            if (IsXcodeBuild(pathToBuiltProject))
            {
                plistPath = Path.Combine(GetMacOsProjectPath(pathToBuiltProject), "Info.plist");
            }


            plist.ReadFromString(File.ReadAllText(plistPath));
            return plist;
        }

        private string GetMacOsProjectPath(string pathToBuiltProject)
        {
            var directory = new DirectoryInfo(pathToBuiltProject);
            var parentDir = directory.Parent.FullName;

            return Path.Combine(parentDir, Application.productName);
        }

        private void AddDeepLinks(AppLinkingConfiguration configuration, string pathToBuiltProject)
        {
#if UDL_DEBUG

            Debug.Log("UDL: Adding deep links");
#endif
            string plistPath;
            var plist = GetPlistFile(pathToBuiltProject, out plistPath);


            var rootDict = plist.root;

            var bgModes = rootDict.CreateArray("CFBundleURLTypes");

            foreach (var deepLinkingProtocol in configuration.GetPlatformDeepLinkingProtocols(SupportedPlatforms.OSX, true))
            {
                var dict = bgModes.AddDict();

                dict.SetString("CFBundleTypeRole", "Viewer");

                dict.SetString("CFBundleURLIconFile", "Logo");

                dict.SetString("CFBundleURLName", Application.identifier);

                dict.CreateArray("CFBundleURLSchemes").AddString(deepLinkingProtocol.Scheme);
            }


            File.WriteAllText(plistPath, plist.WriteToString());

        }

        private void CopyUniversalDeepLinkFramework(string pathToBuiltProject)
        {
            var frameworkFolderPath = pathToBuiltProject + "/Contents/Frameworks";

            var universalDeepLinkFrameworkPath = Path.Combine(Application.dataPath,
                EditorHelpers.PluginPath + "/libs/Standalone/" + FrameworkName);



            if (IsBuiltOnMac())
                MacCopy(universalDeepLinkFrameworkPath, frameworkFolderPath);
            else
            {
                WindowsCopy(universalDeepLinkFrameworkPath, frameworkFolderPath);

            }


        }

        private static bool IsBuiltOnMac()
        {
            return Application.platform == RuntimePlatform.OSXEditor || SystemInfo.operatingSystem.ToLower().Contains("mac");
        }

        private static void MacCopy(string universalDeepLinkFrameworkPath, string frameworkFolderPath)
        {
            var toolFile = "cp";

            var copyArguments = "-a"
                                + " "
                                + @"""" + universalDeepLinkFrameworkPath + @""""
                                + "  "
                                + @"""" + frameworkFolderPath + @"""";

            ShellHelper.ShellRequest req = ShellHelper.ProcessFileCommand(toolFile, copyArguments);

            req.OnLog += delegate (int arg1, string arg2) { Debug.Log(arg2); };

            req.OnDone += delegate () { Debug.Log("Copy UniversalDeepLink Framawork Completed"); };

            req.OnError += delegate () { Debug.Log("Error on UniversalDeepLink Copy Framework"); };
        }

        private static void WindowsCopy(string universalDeepLinkFrameworkPath, string frameworkFolderPath)
        {
            frameworkFolderPath = Path.Combine(frameworkFolderPath, FrameworkName);
            try
            {
                Directory.CreateDirectory(frameworkFolderPath);
            }
            catch (Exception e)
            {
                Debug.Log("UDL Mac build Error " + e.ToString());
            }

            var toolFile = "xcopy";

            var copyArguments = @"""" + universalDeepLinkFrameworkPath + @""""
                                + "  "
                                + @"""" + frameworkFolderPath + @""""
                                + @" /E ";


            Process foo = new Process();
            foo.StartInfo = new ProcessStartInfo(toolFile, copyArguments);
            foo.Start();

            foo.WaitForExit();
            //ShellHelper.ShellRequest req = ShellHelper.ProcessFileCommand(toolFile, copyArguments);
        }

        private string GetGameName(string pathToBuiltProject)
        {
            var pathsplit = pathToBuiltProject.Split('/');

            var gameName = pathsplit.ToList().Find((p) => p.Contains(".app"));

            if (!string.IsNullOrEmpty(gameName))
            {
                return gameName.Replace(".app", "");
            }

            return null;
        }

        private static bool _injectionComplete = false;
        private void InjectFrameworkInGameApp(string pathToBuiltProject)
        {
            _injectionComplete = false;
#if UDL_DEBUG
            Debug.Log("UDL: Injecting Lib");
#endif
            var workDirectory = Path.Combine(Application.dataPath,
                EditorHelpers.PluginPath + "/libs/Tools/");

            var optoolPath = Path.Combine(workDirectory, "optool");

            var executablePath = "\"@executable_path/../Frameworks/UniversalDeepLink.framework/Versions/A/UniversalDeepLink\"";

            var parentDir = Path.Combine(pathToBuiltProject, "Contents/MacOS/");

            var gameName = GetGameName(pathToBuiltProject).ToLower();

            var gameExec = Directory.GetFiles(parentDir).FirstOrDefault(f => f.ToLower().EndsWith(gameName));

            if (gameExec == null)
            {
                //
                //  Failed to find game exec with same name, try to get one that is a directory
                //

                var directories = Directory.GetDirectories(parentDir);

                if (directories.Length == 0)
                {
                    //
                    //  If no dirs found, try to get a file without any extension
                    //
                    gameExec = Directory.GetFiles(parentDir).FirstOrDefault(f =>
                        f.Substring(f.LastIndexOf(Path.DirectorySeparatorChar)).Contains(".") == false);
                }
                else
                    gameExec = directories.First();
                    

                Debug.LogErrorFormat("Failed to find macos executable!!!\nTrying <<<<{0}>>>>", gameExec);
            }

            var gameAppPath = @"""" + gameExec + @"""";

            string opToolCmd = "install -c load -p " + executablePath + " -t " + gameAppPath;

            ShellHelper.ShellRequest chmod = ShellHelper.ProcessFileCommand("chmod", "+x \"" + optoolPath + "\"");

            //var appDir = new DirectoryInfo(pathToBuiltProject).Name.Replace(".app", string.Empty);
            var pathToExport = Path.Combine(pathToBuiltProject, "Contents/Frameworks/");
            var pathToRemove = Path.Combine(pathToExport, "UniversalDeepLink.framework");
            var pathToLib = Path.Combine(Application.dataPath, EditorHelpers.PluginPath + "/libs/Standalone/UniversalDeepLink.framework.zip");

#if UDL_DEBUG
            Debug.Log("UDL: chmod +x \"" + optoolPath + "\"");
#endif
            chmod.OnDone += () =>
            {
                ShellHelper.ShellRequest delete = ShellHelper.ProcessFileCommand("rm", " -r \"" + pathToRemove + "\"");
                delete.OnDone += () =>
                {
#if UDL_DEBUG
                    Debug.Log("UDL: unzip -o " + string.Format("-o \"{0}\" -d \"{1}\"", pathToLib, pathToExport));
#endif

                    ShellHelper.ShellRequest unzip = ShellHelper.ProcessFileCommand("unzip", string.Format("-o \"{0}\" -d \"{1}\"", pathToLib, pathToExport));

                    unzip.OnDone += () =>
                    {

                        RunOptool(optoolPath, opToolCmd);
                    };



                    unzip.OnError += () =>
                    {
                        Debug.Log("Error unzipping " + pathToRemove);
                    };
                };

                delete.OnError += () =>
                {
                    Debug.Log("Error deleting " + pathToRemove);
                };
            };

            //
            //  Wait until all commands execute
            //
            int maxSecsWait = 10;
            for (int i = 0; i < maxSecsWait && _injectionComplete == false; i++)
            {
                Thread.Sleep(1000);
                ShellHelper.Step();
            }

        }


        private static void RunOptool(string optoolPath, string opToolCmd)
        {
#if UDL_DEBUG
            Debug.Log("UDL: optool | " + optoolPath + "  " + opToolCmd);
#endif

            ShellHelper.ShellRequest req = ShellHelper.ProcessFileCommand(optoolPath, opToolCmd);
            req.OnLog += delegate (int arg1, string arg2) { Debug.Log(arg2); };

            req.OnDone += delegate ()
            {
                Debug.Log("Successfully injected framework on Build");
                _injectionComplete = true;
            };

            req.OnError += delegate () { Debug.Log("Error injecting framework on Build"); };
        }
    }

}