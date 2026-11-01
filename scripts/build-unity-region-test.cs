using System;using System.IO;using System.Linq;using UnityEditor;using UnityEditor.Build.Reporting;using UnityEngine;
public static class RegionTestBuild {
 public static object Main(){
 if(EditorApplication.isPlaying)throw new Exception("Stop Play before build");
 string path=Path.GetFullPath("Builds/Android/ECHO-SPHERE-regions-test-2026-11-02.apk");Directory.CreateDirectory(Path.GetDirectoryName(path));
 var options=new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=path,target=BuildTarget.Android,options=BuildOptions.None,extraScriptingDefines=new[]{"ECHO_REGION_TEST_ACCESS"}};
 if(options.scenes.Length==0)throw new Exception("No enabled scenes");var report=BuildPipeline.BuildPlayer(options);
 var result=new Result{status=report.summary.result.ToString(),path=path,bytes=(long)report.summary.totalSize};File.WriteAllText("Library/region-test-build.json",JsonUtility.ToJson(result,true));if(report.summary.result!=BuildResult.Succeeded)throw new Exception("APK build failed: "+report.summary.result);return result;
 }
 [Serializable]public class Result{public string status,path;public long bytes;}
}
