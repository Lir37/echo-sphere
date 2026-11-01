using EchoSphere.Core;
internal static class RegionalEnemyChecks {
 internal static int Run(){int n=0;void A(bool x,string m){n++;if(!x)throw new Exception(m);}
 string[] roles={"grunt","ranged","sniper","tank_guard","anchor","healer","disruptor","phase","swarmer","charger","bomber","leech","corruptor","splitter","scavenger"};
 for(int region=0;region<6;region++)foreach(var role in roles){var p=RegionalEnemyRules.Get(region,role);
 A(p.Range>=p.Minimum&&p.Minimum>=0,"range ordering");A(p.Pellets>=1&&p.Pellets<=3,"bounded volley");
 if(p.Range>0){A(p.Windup>=.5f&&p.Interval>p.Windup,"dodgeable windup");A(p.Speed>0,"projectile speed");A(RegionalEnemyRules.Radial(p,p.Range+1)==1,"approach");A(RegionalEnemyRules.Radial(p,(p.Range+p.Minimum)/2)==0,"hold band");A(RegionalEnemyRules.Radial(p,1)==0,"never retreat");}
 A(p.Range<=110,"inside base standard sphere range130");
 if(role=="grunt"||role=="charger"||role=="tank_guard"||role=="healer")A(p.Range==0,"melee and support roles do not shoot");
 for(int d=0;d<700;d++)A(RegionalEnemyRules.Radial(p,d)>=0,"never flee at any distance");
 for(int i=0;i<=100;i++){float remaining=p.Windup*i/100;float t=RegionalEnemyRules.Telegraph(remaining,p.Windup);A(float.IsFinite(t)&&t>=0&&t<=1,"pose finite");}
 }
 A(RegionalEnemyRules.Get(0,"grunt").Motion==RegionalMotion.Chase,"basin melee");A(RegionalEnemyRules.Get(2,"charger").Motion==RegionalMotion.Lunge,"brood pounce");A(RegionalEnemyRules.Get(3,"ranged").Motion==RegionalMotion.Hold,"crystal emplacement");A(RegionalEnemyRules.Get(4,"ranged").Shot=="arc","storm discharge");A(RegionalEnemyRules.Get(5,"grunt").Motion==RegionalMotion.Flank,"void flank");
 return n;}
}
