using EchoSphere.Core;
using UnityEngine;
namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        public float RuneAttackSpeedMultiplier=>RuneRules.AttackSpeed(_runEconomy?.Runes);
        public bool ConsumeRuneEcho(bool successful)=>RuneRules.ConsumeEcho(_runEconomy?.Runes,successful);
        private void ActivateCampaignRune(string type,Vector2 position)
        {
            if(_runEconomy.Runes==null)_runEconomy.Runes=new RuneRunState();
            if(!RuneRules.Activate(_runEconomy.Runes,type,_regionEndlessMode||_convergenceRun,position.x,position.y))return;
            float scale=SpherePlacementRules.SourcePixelsToWorld;
            switch(type){
                // Player model currently has no timed negative statuses to cleanse.
                // Invulnerability is applied without removing beneficial ability timers.
                case "phase":_playerCore.GrantInvulnerability(.8f);break;
                case "purge":
                    // Reuse the existing level-scaled radius attack, ordinary enemies only.
                    for(int i=_enemies.Count-1;i>=0;i--){var enemy=_enemies[i];if(enemy!=null&&enemy.IsAlive&&!enemy.IsBoss&&!enemy.IsElite&&Vector2.Distance(enemy.transform.position,position)<=240*scale)enemy.ReceiveDamage(45+_level*4);}
                    break;
                case "resonance":AddResonanceCharge(60);break;
                case "hunt":
                    EnemyAgent2D target=null;float nearest=float.PositiveInfinity;
                    foreach(var enemy in _enemies){if(enemy==null||!enemy.IsAlive||(!enemy.IsElite&&!enemy.IsBoss))continue;float d=Vector2.Distance(enemy.transform.position,position);if(d>=nearest)continue;nearest=d;target=enemy;}
                    if(target!=null)target.ApplyRuneHuntMark(8);break;
            }
            SpawnImpact(position,RuneAgent2D.GetRuneColor(type));
            string text=RuneRules.Name(type,IsRussianUi);
            if(type=="runic_cell")text+=IsRussianUi?" · +5 КРЕДИТОВ К ИТОГУ":" · +5 CREDITS AT RUN END";
            ShowTimedCombatMessage(text,1.4f);
        }
        private void TickCampaignRunes(float dt)
        {
            if(IsGameplayPaused||_gameOver)return;
            var state=_runEconomy?.Runes;if(state==null)return;
            RuneRules.Tick(state,dt);
            if(state.Gravity<=0)return;
            Vector2 center=new Vector2(state.GravityX,state.GravityY);float scale=SpherePlacementRules.SourcePixelsToWorld;
            foreach(var enemy in _enemies){if(enemy==null||!enemy.IsAlive)continue;Vector2 p=enemy.transform.position;
                if((p-center).sqrMagnitude>240*scale*240*scale)continue;
                enemy.transform.position=Vector2.MoveTowards(p,center,90*scale*dt);RefreshEnemyQuery(enemy);
            }
        }
        private void DrawNativeRuneZone()
        {
            var state=_runEconomy?.Runes;if(state==null||state.Gravity<=0)return;
            var point=NativeSourcePixels(new Vector2(state.GravityX,state.GravityY));
            _worldSurface.Save();_worldSurface.Translate(point.x,point.y);_worldSurface.SetOpacity(Mathf.Min(1,state.Gravity));
            _nativeWorldFeedback.Boundary(232,0,new Color(.65f,.48f,1f,.65f));
            _nativeWorldPickableViews.DrawRune(_worldSurface,0,0,"gravity",RuneAgent2D.GetRuneColor("gravity"),_runTime);_worldSurface.Restore();
        }
    }
}
