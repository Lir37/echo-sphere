using System.Collections.Generic;
using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeRunResultView _nativeRunResult;
        private void DisposeNativeRunResult(){_nativeRunResult?.Dispose();_nativeRunResult=null;}
        private void PrepareNativeRunResult(VisualCssNode root)
        {
            if(!_gameOver){DisposeNativeRunResult();return;}
            VisualCssNode overlay=null;
            void Find(VisualCssNode node){if(overlay==null&&node.HasClass("bg-black/80")&&node.HasClass("absolute")&&node.HasClass("inset-0")&&node.HasClass("z-50"))overlay=node;foreach(var child in node.Children)Find(child);}
            Find(root);if(!(overlay?.Native is SourceUiFrame frame)){DisposeNativeRunResult();return;}
            if(_nativeRunResult==null||_nativeRunResult.Root.parent!=frame.Content){DisposeNativeRunResult();_nativeRunResult=new NativeRunResultView(frame.Content,ReadNativeRunResultState(),_sourceUi,ExitNativeRunResult);}
            bool wasReady=_nativeRunResult.Ready;_nativeRunResult.Prepare(SourceDevicePixelRatio);
            if(_nativeRunResult.Ready){foreach(var child in overlay.Children)if(child.Native is UnityEngine.UIElements.VisualElement element)element.style.visibility=UnityEngine.UIElements.Visibility.Hidden;if(!wasReady)_sourceUi.Dirty=true;}
        }
        private V ProjectNativeRunResult(V tree)
        {
            if(!_gameOver||_nativeRunResult==null||!_nativeRunResult.Ready)return tree;
            void Visit(V value){if(value.Raw is VisualDom.Node node){var classes=(node.Properties["className"].Text??"").Split(' ');bool Has(string c)=>System.Array.IndexOf(classes,c)>=0;if(Has("bg-black/80")&&Has("absolute")&&Has("inset-0")&&Has("z-50")){node.Children.Clear();return;}foreach(var child in node.Children)Visit(child);}else if(value.Raw is List<V>)foreach(var child in value.Items)Visit(child);}
            Visit(tree);return tree;
        }
        private NativeRunResultView.State ReadNativeRunResultState()
        {
            string lang=IsRussianUi?"ru":"en";string T(string key)=>Visual_i18n.Scope["translations"][lang][key].Text;
            var rows=new List<NativeRunResultView.Row> {
                new NativeRunResultView.Row(T("survived"),Mathf.FloorToInt(_runTime)+" "+T("seconds")),
                new NativeRunResultView.Row(T("wave"),_wave.ToString()),new NativeRunResultView.Row(IsRussianUi?"УБИЙСТВА":"KILLS",_kills.ToString()),new NativeRunResultView.Row(T("goldEarned"),_runGoldEarned.ToString())};
            // Keep the actual original result's default Basin accessors, even
            // when another region was selected. Do not invent a behavior fix.
            if(_regionStabilized){rows.Add(new NativeRunResultView.Row(IsRussianUi?"ИСПЫТАНИЯ":"CHALLENGES",GetCompletedRegionChallenges(EchoRegionId.ResonanceBasin).Length+"/3"));rows.Add(new NativeRunResultView.Row(IsRussianUi?"БЕСКОНЕЧНЫЙ":"ENDLESS",IsRegionEndlessUnlocked(EchoRegionId.ResonanceBasin)?(IsRussianUi?"ОТКРЫТ":"UNLOCKED"):(IsRussianUi?"ЗАКРЫТ":"LOCKED")));rows.Add(new NativeRunResultView.Row(IsRussianUi?"МАСТЕРСТВО":"MASTERY",_regionMasteryChallengesCompleted+"/3"));}
            if(_sourceResultRank>0&&_sourceResultRank<=10)rows.Add(new NativeRunResultView.Row(T("rank"),"#"+_sourceResultRank));
            return new NativeRunResultView.State {Title=_regionStabilized?(IsRussianUi?"РЕГИОН СТАБИЛИЗИРОВАН":"REGION STABILIZED"):T("gameOver"),Clear=_regionStabilized,NewRecord=_sourceResultNewRecord,Record=T("newRecord"),Return=T("return"),Rows=rows};
        }
        private void ExitNativeRunResult(){ClearRunSnapshot();PlayerPrefs.SetInt("echo-sphere:open_region_select",1);PlayerPrefs.Save();SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);}
    }
}
