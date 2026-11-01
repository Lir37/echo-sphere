using System;
namespace EchoSphere.Core
{
    [Serializable] public sealed class RuneRunState
    {
        public float Overdrive, Harvest, Fortify, Gravity;
        public float GravityX, GravityY;
        public bool EchoReady;
        public int CellsCollected;
    }
    // Blueprint section22. Unspecified numeric tuning is explicitly authored:
    // gravity4s/240px/90px per second; Runic Cell5meta currency,10% special pool.
    public static class RuneRules
    {
        public const float PickupLifetime=22f, EliteDropChance=.35f;
        public const int CellValue=5;
        private static readonly string[] Types={"overdrive","phase","harvest","purge","resonance","fortify","hunt","echo","gravity","runic_cell"};
        public static string Pick(float roll,bool specialMode)
        {int count=specialMode?10:9;return Types[Math.Min(count-1,Math.Max(0,(int)(roll*count)))];}
        public static float DropChance(bool boss,bool elite)=>boss?1f:elite?EliteDropChance:0f;
        public static bool Activate(RuneRunState s,string type,bool specialMode,float x,float y)
        {
            if(s==null)return false;
            switch(type){
                case "overdrive":s.Overdrive=8;break;
                case "phase":case "purge":case "resonance":case "hunt":break;
                case "harvest":s.Harvest=5;break;
                case "fortify":s.Fortify=8;break;
                case "echo":s.EchoReady=true;break;
                case "gravity":s.Gravity=4;s.GravityX=x;s.GravityY=y;break;
                case "runic_cell":if(!specialMode)return false;s.CellsCollected=(int)Math.Min(int.MaxValue/CellValue,Math.Max(0L,(long)s.CellsCollected)+1);break;
                default:return false;
            }
            return true;
        }
        public static void Tick(RuneRunState s,float dt)
        {if(s==null||dt<=0||float.IsNaN(dt)||float.IsInfinity(dt))return;s.Overdrive=Math.Max(0,s.Overdrive-dt);s.Harvest=Math.Max(0,s.Harvest-dt);s.Fortify=Math.Max(0,s.Fortify-dt);s.Gravity=Math.Max(0,s.Gravity-dt);}
        public static float AttackSpeed(RuneRunState s)=>s!=null&&s.Overdrive>0?1.35f:1f;
        public static float Magnet(RuneRunState s)=>s!=null&&s.Harvest>0?2f:1f;
        public static float DamageTaken(RuneRunState s)=>s!=null&&s.Fortify>0?.65f:1f;
        public static bool ConsumeEcho(RuneRunState s,bool successful)
        {if(s==null||!s.EchoReady||!successful)return false;s.EchoReady=false;return true;}
        public static int CellReward(RuneRunState s)=>s==null?0:(int)Math.Min(int.MaxValue,Math.Max(0L,(long)s.CellsCollected)*CellValue);
        public static string Description(string id,bool ru)
        {switch(id){
            case "overdrive":return ru?"Скорость атаки всей сети +35% на 8 с. Повторный подбор обновляет время.":"Network attack speed +35% for 8 s. Picking another refreshes the duration.";
            case "phase":return ru?"Неуязвимость на 0,8 с.":"Invulnerability for 0.8 s.";
            case "harvest":return ru?"Радиус притяжения опыта ×2 на 5 с.":"XP attraction radius ×2 for 5 s.";
            case "purge":return ru?"45 + 4 × уровень урона обычным врагам в радиусе 240. Элиты и боссы не затрагиваются.":"45 + 4 × level damage to ordinary enemies within 240. Elites and bosses are excluded.";
            case "resonance":return ru?"Мгновенно даёт 60 резонанса с действующими модификаторами заряда.":"Instantly adds 60 Resonance, using existing charge modifiers.";
            case "fortify":return ru?"Получаемый урон ×0,65 на 8 с; щиты не добавляются.":"Damage taken ×0.65 for 8 s; grants no shield charges.";
            case "hunt":return ru?"Помечает ближайшую элиту или босса на 8 с. Метка поддерживает действующие бонусы Охотника.":"Marks the nearest Elite or Boss for 8 s, supporting the Hunter’s existing mark bonuses.";
            case "echo":return ru?"Повторяет следующую успешную атаку сферы. Пустая атака заряд не расходует; заряды не складываются.":"Repeats the next successful Sphere attack. Empty attacks do not consume it; charges do not stack.";
            case "gravity":return ru?"На 4 с создаёт зону радиусом 240; тянет врагов к центру со скоростью 90 в секунду.":"Creates a 240-radius pull zone for 4 s; pulls enemies toward its center at 90 per second.";
            case "runic_cell":return ru?"+5 кредитов в конце забега. Выпадает только в Endless и Convergence.":"+5 Credits at run end. Drops only in Endless and Convergence.";
            default:return "";
        }}
        public static int Count=>Types.Length;
        public static string TypeAt(int index)=>Types[index];
        public static string Name(string id,bool ru)
        {switch(id){
            case "overdrive":return ru?"РУНА РАЗГОНА":"OVERDRIVE RUNE";
            case "phase":return ru?"РУНА ФАЗЫ":"PHASE RUNE";
            case "harvest":return ru?"РУНА СБОРА":"HARVEST RUNE";
            case "purge":return ru?"РУНА ОЧИЩЕНИЯ":"PURGE RUNE";
            case "resonance":return ru?"РУНА РЕЗОНАНСА":"RESONANCE RUNE";
            case "fortify":return ru?"РУНА УКРЕПЛЕНИЯ":"FORTIFY RUNE";
            case "hunt":return ru?"РУНА ОХОТЫ":"HUNT RUNE";
            case "echo":return ru?"РУНА ЭХА":"ECHO RUNE";
            case "gravity":return ru?"РУНА ГРАВИТАЦИИ":"GRAVITY RUNE";
            case "runic_cell":return ru?"РУНИЧЕСКАЯ ЯЧЕЙКА":"RUNIC CELL";
            default:return ru?"РУНА":"RUNE";
        }}
    }
}
