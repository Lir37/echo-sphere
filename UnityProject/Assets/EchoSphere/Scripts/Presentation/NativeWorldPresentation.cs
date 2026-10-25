using UnityEngine;

namespace EchoSphere.Presentation
{
    // Scene-owned visual resources. Runtime supplies typed state and paint order;
    // Unity scene destruction releases every view's meshes, fonts and materials.
    [DisallowMultipleComponent]
    public sealed class NativeWorldPresentation : MonoBehaviour
    {
        private NativeWorldCharacterIndicators _CharacterIndicators;
        internal NativeWorldCharacterIndicators CharacterIndicators=>_CharacterIndicators??=new NativeWorldCharacterIndicators();
        private NativeWorldFeedback _Feedback;
        internal NativeWorldFeedback Feedback=>_Feedback??=new NativeWorldFeedback();
        private NativeWorldNetwork _Network;
        internal NativeWorldNetwork Network=>_Network??=new NativeWorldNetwork();
        private NativeWorldRegion _Region;
        internal NativeWorldRegion Region=>_Region??=new NativeWorldRegion();
        private NativeWorldSphereFamilies _SphereFamilies;
        internal NativeWorldSphereFamilies SphereFamilies=>_SphereFamilies??=new NativeWorldSphereFamilies();
        private NativeWorldField _Field;
        internal NativeWorldField Field=>_Field??=new NativeWorldField();
        private NativeWorldAmbient _Ambient;
        internal NativeWorldAmbient Ambient=>_Ambient??=new NativeWorldAmbient();
        private NativeWorldPickups _Pickups;
        internal NativeWorldPickups Pickups=>_Pickups??=new NativeWorldPickups();
        private NativeWorldMinions _Minions;
        internal NativeWorldMinions Minions=>_Minions??=new NativeWorldMinions();
        private NativeWorldEnergyBolts _EnergyBolts;
        internal NativeWorldEnergyBolts EnergyBolts=>_EnergyBolts??=new NativeWorldEnergyBolts();
        private NativeWorldSprites _Sprites;
        internal NativeWorldSprites Sprites=>_Sprites??=new NativeWorldSprites();
        private NativeWorldProjectiles _Projectiles;
        internal NativeWorldProjectiles Projectiles=>_Projectiles??=new NativeWorldProjectiles();
        private NativeWorldCreatures _Creatures;
        internal NativeWorldCreatures Creatures=>_Creatures??=new NativeWorldCreatures();
        private NativeWorldStandardSphere _StandardSphere;
        internal NativeWorldStandardSphere StandardSphere=>_StandardSphere??=new NativeWorldStandardSphere();
        private NativeWorldPickableViews _PickableViews;
        internal NativeWorldPickableViews PickableViews=>_PickableViews??=new NativeWorldPickableViews();
        private NativeWorldPlayer _Player;
        internal NativeWorldPlayer Player=>_Player??=new NativeWorldPlayer();
        internal void BeginFrame(NativeWorldSurface surface)
        {
            Feedback.BeginFrame(surface);
            _=Field;_=Ambient;
            Pickups.BeginFrame();Minions.BeginFrame();EnergyBolts.BeginFrame();Sprites.BeginFrame();
            Projectiles.BeginFrame();Creatures.BeginFrame();StandardSphere.BeginFrame();PickableViews.BeginFrame();
            Player.BeginFrame();SphereFamilies.BeginFrame();CharacterIndicators.BeginFrame();Network.BeginFrame();
        }
        internal void ReleaseResources()
        {
            _CharacterIndicators?.Dispose();_CharacterIndicators=null;
            _Feedback?.Dispose();_Feedback=null;
            _Network?.Dispose();_Network=null;
            _Region?.Dispose();_Region=null;
            _SphereFamilies?.Dispose();_SphereFamilies=null;
            _Field?.Dispose();_Field=null;
            _Ambient?.Dispose();_Ambient=null;
            _Pickups?.Dispose();_Pickups=null;
            _Minions?.Dispose();_Minions=null;
            _EnergyBolts?.Dispose();_EnergyBolts=null;
            _Sprites?.Dispose();_Sprites=null;
            _Projectiles?.Dispose();_Projectiles=null;
            _Creatures?.Dispose();_Creatures=null;
            _StandardSphere?.Dispose();_StandardSphere=null;
            _PickableViews?.Dispose();_PickableViews=null;
            _Player?.Dispose();_Player=null;
        }
        private void OnDestroy()=>ReleaseResources();
    }
}
