using EchoSphere.Core;
internal static class RunEconomyChecks
{
    internal static int Run()
    {
        int checks=0;void Assert(bool ok,string message){checks++;if(!ok)throw new Exception("Economy: "+message);}
        var owned=new List<string>();var s=RunEconomyRules.Create(33);
        Assert(!RunEconomyRules.TakeScheduledChest(s,79.9),"no early cache");Assert(RunEconomyRules.TakeScheduledChest(s,80),"first timed cache");Assert(!RunEconomyRules.TakeScheduledChest(s,80),"cache exactly once");
        Assert(RunEconomyRules.ChestAt(9)==1720&&RunEconomyRules.ChestAt(10)==1880,"endless cache schedule");
        Assert(!RunEconomyRules.AdvanceMerchant(s,389.9)&&RunEconomyRules.AdvanceMerchant(s,390),"merchant threshold");
        Assert(!RunEconomyRules.AdvanceMerchant(s,4000),"active visit not overwritten");
        RunEconomyRules.Prepare(s,owned);Assert(s.Offers.Count==8,"eight offers");Assert(s.Offers.Select(x=>x.ArtifactId).Distinct().Count()==8,"unique stock");Assert(s.Offers.Count(x=>x.Discount)==1,"one sale");
        Assert(s.Offers.All(x=>ArtifactRewardRules.Get(x.ArtifactId).Rarity!=ArtifactRarity.Legendary),"legendary excluded");
        var rng=s.RngState;var first=s.Offers[0].ArtifactId;RunEconomyRules.Prepare(s,owned);Assert(s.RngState==rng&&s.Offers[0].ArtifactId==first,"repeat prepare stable");
        Assert(!RunEconomyRules.TryBuy(s,0,owned),"closed window rejects purchase");s.Open=true;
        Assert(!RunEconomyRules.TryBuy(s,0,owned)&&owned.Count==0,"insufficient Echo has no side effects");
        RunEconomyRules.Collect(s,5000);int balance=s.Echo,price=s.Offers[0].Price;
        Assert(RunEconomyRules.TryBuy(s,0,owned)&&s.Echo==balance-price&&owned.Contains(first),"atomic purchase");
        Assert(!RunEconomyRules.TryBuy(s,0,owned)&&s.Echo==balance-price,"double tap cannot debit again");
        var snapshot=RunSnapshotCodec.Capture(s);var restored=(RunEconomyState)RunSnapshotCodec.Restore(snapshot,typeof(RunEconomyState));
        Assert(restored.Echo==s.Echo&&restored.Offers[0].Sold&&restored.RngState==s.RngState&&restored.NextMerchant==1,"stock/wallet/RNG round trip");
        Assert(!RunEconomyRules.TryBuy(restored,0,owned),"reload cannot repurchase sold item");
        RunEconomyRules.Close(s);Assert(!s.Open&&!s.Pending,"close releases visit");Assert(RunEconomyRules.AdvanceMerchant(s,810),"next visit");RunEconomyRules.Prepare(s,owned);Assert(s.Offers.All(x=>x.ArtifactId!=first),"owned excluded next visit");
        Assert(RunEconomyRules.Price(ArtifactRarity.Common,0,false)==100&&RunEconomyRules.Price(ArtifactRarity.Common,0,true)==75&&RunEconomyRules.Price(ArtifactRarity.Common,1,false)==135,"prices");
        Assert(RunEconomyRules.MerchantAt(4)==1980&&RunEconomyRules.MerchantAt(10)==4500,"trade continues beyond one hour");
        RunEconomyRules.Collect(s,int.MaxValue);Assert(s.Echo==int.MaxValue,"wallet saturates");RunEconomyRules.Collect(s,-5);Assert(s.Echo==int.MaxValue,"negative pickup ignored");
        Assert(!RunEconomyRules.TakeScheduledChest(null,10000)&&!RunEconomyRules.AdvanceMerchant(null,10000),"legacy null never schedules");
        for(uint seed=1;seed<=100;seed++){
            var a=RunEconomyRules.Create(seed);var b=RunEconomyRules.Create(seed);RunEconomyRules.AdvanceMerchant(a,390);RunEconomyRules.AdvanceMerchant(b,390);RunEconomyRules.Prepare(a,owned);RunEconomyRules.Prepare(b,owned);
            Assert(a.Offers.Select(x=>x.ArtifactId).SequenceEqual(b.Offers.Select(x=>x.ArtifactId)),"seeded stock reproducible");
            Assert(a.RngState==b.RngState&&a.Offers.Count(x=>x.Discount)==1,"seeded state and sale");
        }
        var complete=RunEconomyRules.Create(1);RunEconomyRules.AdvanceMerchant(complete,390);RunEconomyRules.Prepare(complete,ArtifactRewardRules.All.Select(x=>x.Id).ToList());Assert(complete.Prepared&&complete.Offers.Count==0,"exhausted catalog safe");
        Console.WriteLine("Run economy: "+checks+" behavioral checks PASS");return checks;
    }
}
