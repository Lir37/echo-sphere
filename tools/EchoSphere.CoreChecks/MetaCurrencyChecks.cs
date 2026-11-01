using EchoSphere.Core;
internal static class MetaCurrencyChecks
{
 internal static int Run(){int count=0;void A(bool ok,string why){count++;if(!ok)throw new Exception("Meta currency: "+why);}
  var wallet=MetaCurrencyRules.MigrateGold(12345);A(wallet.Credits==12345&&wallet.ResonanceShards==0&&wallet.ArchiveData==0,"gold migrates1:1 without invented other balances");
  A(MetaCurrencyRules.MigrateGold(-1).Credits==0,"negative legacy clamped");
  A(!MetaCurrencyRules.TrySpend(wallet,CurrencyId.Echo,1),"run Echo is never spent from meta wallet");
  A(MetaCurrencyRules.TryGrantOnce(wallet,"first",7,3,2),"one atomic multi-currency grant");A(!MetaCurrencyRules.TryGrantOnce(wallet,"first",7,3,2)&&wallet.Credits==12352&&wallet.ResonanceShards==3&&wallet.ArchiveData==2,"receipt prevents duplicate all balances");
  A(!MetaCurrencyRules.TrySpend(wallet,CurrencyId.ResonanceShards,4)&&wallet.ResonanceShards==3,"no overdraft");A(!MetaCurrencyRules.TrySpend(wallet,CurrencyId.Credits,0)&&!MetaCurrencyRules.TrySpend(wallet,CurrencyId.Credits,-1),"no invalid cost");
  var restored=(MetaWallet)RunSnapshotCodec.Restore(RunSnapshotCodec.Capture(wallet),typeof(MetaWallet));A(MetaCurrencyRules.HasReceipt(restored,"first")&&!MetaCurrencyRules.TryGrantOnce(restored,"first",7,3,2),"save receipt keeps grant exactly once");
  A(!MetaCurrencyRules.Valid(new MetaWallet())&&!MetaCurrencyRules.Valid(null),"missing schema is not valid empty wallet");
  for(uint seed=1;seed<=200;seed++){
   var rng=new SeededRng(seed);var state=MetaCurrencyRules.MigrateGold(1000);long[] expected={1000,0,0};
   for(int i=0;i<100;i++){
    int kind=rng.NextInt(3),amount=1+rng.NextInt(50);var id=(CurrencyId)(kind+1);
    if(rng.NextFloat()<.55f){int cr=kind==0?amount:0,sh=kind==1?amount:0,da=kind==2?amount:0;string receipt=seed+":"+i;A(MetaCurrencyRules.TryGrantOnce(state,receipt,cr,sh,da),"seeded grant");A(!MetaCurrencyRules.TryGrantOnce(state,receipt,cr,sh,da),"seeded duplicate rejection");expected[kind]+=amount;}
    else{bool affordable=expected[kind]>=amount;A(MetaCurrencyRules.TrySpend(state,id,amount)==affordable,"seeded spend");if(affordable)expected[kind]-=amount;}
    for(int c=0;c<3;c++)A(MetaCurrencyRules.Balance(state,(CurrencyId)(c+1))==expected[c],"independent accounting");
    if(i%20==0)state=(MetaWallet)RunSnapshotCodec.Restore(RunSnapshotCodec.Capture(state),typeof(MetaWallet));
   }
  }
  A(MetaCurrencyRules.CharacterUnlockShards(0)==0&&MetaCurrencyRules.CharacterUnlockShards(1500)==10&&MetaCurrencyRules.CharacterUnlockShards(2500)==17,"authored unlock prices");
  A(MetaCurrencyRules.MasteryResearchShards(1)==4&&MetaCurrencyRules.MasteryResearchShards(9)==20&&MetaCurrencyRules.MasteryResearchShards(10)==0,"optional mastery prices and cap");
  A(MetaCurrencyRules.RunShards(3,true)==6&&MetaCurrencyRules.RunShards(1,false)==1&&MetaCurrencyRules.RunShards(0,false)==0,"boss/stabilization income");
  A(MetaCurrencyRules.RunShards(int.MaxValue,true)==int.MaxValue,"reward overflow safe");
  foreach(var def in CharacterRules.All){int price=MetaCurrencyRules.CharacterUnlockShards(def.UnlockCost);A(def.UnlockCost==0?price==0:price>0,"all character prices defined");Console.WriteLine("Meta unlock "+CharacterRules.ToSourceId(def.Id)+": "+price+" Shards ("+(price==0?0:(price+5)/6)+" six-Shard clears)");}
  wallet=MetaCurrencyRules.MigrateGold(500);A(MetaCurrencyRules.TryPurchaseOnce(wallet,"purchase:one",CurrencyId.Credits,500)&&wallet.Credits==0,"purchase and receipt atomic");
  A(MetaCurrencyRules.TryPurchaseOnce(wallet,"purchase:one",CurrencyId.Credits,500)&&wallet.Credits==0,"interrupted entitlement can finish without paying twice");
  A(!MetaCurrencyRules.TryPurchaseOnce(wallet,"purchase:two",CurrencyId.Credits,1)&&!MetaCurrencyRules.HasReceipt(wallet,"purchase:two"),"unaffordable cannot create entitlement");
  wallet=MetaCurrencyRules.MigrateGold(int.MaxValue);A(MetaCurrencyRules.TryGrantOnce(wallet,"saturate",int.MaxValue,int.MaxValue,int.MaxValue)&&wallet.Credits==int.MaxValue,"saturating grants");
  A(!MetaCurrencyRules.TryGrantOnce(wallet,"negative",-1,0,0)&&!MetaCurrencyRules.HasReceipt(wallet,"negative"),"invalid grant does not consume receipt");
  Console.WriteLine("Meta currencies: "+count+" checks PASS");return count;
 }
}
