using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private Rect SourceTutorialTarget
        {
            get
            {
                if (_tutorialStep == 1) return new Rect(_uiWidth*(IsLeftHanded ? .75f : .25f)-58f, _uiHeight-160f, 116f, 116f);
                if (_tutorialStep == 3) return SourceHudLeft;
                return new Rect(0f, 0f, _uiWidth, _uiHeight);
            }
        }
        private Rect SourceTutorialCard
        {
            get
            {
                var width = Mathf.Min(340f, _uiWidth-24f);
                // Text wrapping determines the card height; positioning follows TutorialOverlay.tsx.
                var height = _tutorialStep == 1 || _tutorialStep == 2 || _tutorialStep == 4 ? 220f : 180f;
                if (_tutorialStep == 0 || _tutorialStep == 5) return new Rect(Mathf.Max(12f, (_uiWidth-width)*.5f), (_uiHeight-height)*.5f, width, height);
                var target = SourceTutorialTarget;
                var top = target.y > _uiHeight*.48f ? target.y-170f : target.yMax+14f;
                return new Rect(Mathf.Clamp(target.center.x-width*.5f,12f,_uiWidth-width-12f), Mathf.Min(_uiHeight-188f,Mathf.Max(12f,top)), width,height);
            }
        }
        private void DrawSourceTutorialSpotlight()
        {
            if (_tutorialStep == 0 || _tutorialStep == 5) return;
            var target = SourceTutorialTarget;
            var cut = new Rect(Mathf.Max(4f,target.x-8f),Mathf.Max(4f,target.y-8f),target.width+16f,target.height+16f);
            var shade = new Color(0f,0f,0f,.72f);
            SourceUiRect(new Rect(0f,0f,_uiWidth,cut.y),shade);
            SourceUiRect(new Rect(0f,cut.y,cut.x,cut.height),shade);
            SourceUiRect(new Rect(cut.xMax,cut.y,Mathf.Max(0f,_uiWidth-cut.xMax),cut.height),shade);
            SourceUiRect(new Rect(0f,cut.yMax,_uiWidth,Mathf.Max(0f,_uiHeight-cut.yMax)),shade);
            var border = new Color32(99,230,255,255);
            SourceUiRect(new Rect(cut.x,cut.y,cut.width,2f),border); SourceUiRect(new Rect(cut.x,cut.yMax-2f,cut.width,2f),border);
            SourceUiRect(new Rect(cut.x,cut.y,2f,cut.height),border); SourceUiRect(new Rect(cut.xMax-2f,cut.y,2f,cut.height),border);
        }
    }
}
