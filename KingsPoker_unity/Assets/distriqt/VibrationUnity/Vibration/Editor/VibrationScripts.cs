using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class VibrationScripts : ScriptableObject
{

    [PostProcessBuild]
    public static void OnPostProcess(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget != BuildTarget.iOS && buildTarget != BuildTarget.tvOS)
        {
            return;
        }
#if UNITY_IOS

		var projectPath = PBXProject.GetPBXProjectPath(buildPath);
		var project = new PBXProject();
		project.ReadFromFile(projectPath);

        project.AddFrameworkToProject(project.GetUnityMainTargetGuid(), "CoreHaptics.framework", false);
        project.AddFrameworkToProject(project.GetUnityFrameworkTargetGuid(), "CoreHaptics.framework", false);

        project.WriteToFile(projectPath);
#endif

    }

}


