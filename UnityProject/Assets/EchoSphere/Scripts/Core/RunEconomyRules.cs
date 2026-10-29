using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    [Serializable] public sealed class MerchantOffer
    {
        public string ArtifactId;
        public int Price;
        public bool Sold, Discount;
    }
    [Serializable] public sealed class RunEconomyState
    {
        public int Version = 1, Echo, NextChest, NextMerchant;
        public uint RngState;
        public bool Pending, Prepared, Open;
        public double ArrivedAt;
        public List<MerchantOffer> Offers = new List<MerchantOffer>();
    }
    // New-run economy only. A null state deliberately preserves legacy runs.
    public static class RunEconomyRules
    {
        private static readonly int[] ChestTimes = {80,190,340,530,690,880,1090,1310,1520,1720};
        private static readonly int[] MerchantTimes = {390,810,1230,1560};
        public static RunEconomyState Create(uint runSeed) => new RunEconomyState {RngState=runSeed ^ 0xA7316B45u};
        public static double ChestAt(int index) => index < 0 ? double.PositiveInfinity : 1800d*(index/ChestTimes.Length)+ChestTimes[index%ChestTimes.Length];
        public static double MerchantAt(int index) => index < 0 ? double.PositiveInfinity : index < 4 ? MerchantTimes[index] : 1560d+420d*(index-3);
        public static bool TakeScheduledChest(RunEconomyState state,double time)
        {
            if(state==null||double.IsNaN(time)||time<ChestAt(state.NextChest)||state.NextChest==int.MaxValue)return false;
            state.NextChest++;return true;
        }
        public static bool AdvanceMerchant(RunEconomyState state,double time)
        {
            if(state==null||state.Pending||time<MerchantAt(state.NextMerchant)||double.IsNaN(time)||state.NextMerchant==int.MaxValue)return false;
            state.ArrivedAt=MerchantAt(state.NextMerchant++);state.Pending=true;state.Prepared=false;state.Open=false;state.Offers.Clear();return true;
        }
        public static void Collect(RunEconomyState state,int baseExperience)
        {if(state!=null&&baseExperience>0)state.Echo=(int)Math.Min(int.MaxValue,(long)state.Echo+baseExperience);}
        public static int Price(ArtifactRarity rarity,int visit,bool discount)
        {
            double value=rarity==ArtifactRarity.Common?100:rarity==ArtifactRarity.Rare?180:rarity==ArtifactRarity.Epic?300:500;
            value*=Math.Pow(1.35,Math.Max(0,visit));if(discount)value*=.75;
            return (int)Math.Min(int.MaxValue,Math.Ceiling(value));
        }
        public static void Prepare(RunEconomyState state,IList<string> owned)
        {
            if(state==null||!state.Pending||state.Prepared)return;
            var rng=new SeededRng(1);rng.RestoreState(state.RngState);
            var pools=new List<ArtifactRewardDefinition>[4];for(int i=0;i<4;i++)pools[i]=new List<ArtifactRewardDefinition>();
            foreach(var item in ArtifactRewardRules.All){int r=(int)item.Rarity;if(r<0||r>3||owned.Contains(item.Id))continue;pools[r].Add(item);}
            var weights=new[]{30,35,25,10};state.Offers.Clear();
            for(int slot=0;slot<8;slot++){
                int total=0;for(int r=0;r<4;r++)if(pools[r].Count>0)total+=weights[r];if(total==0)break;
                double roll=rng.NextFloat()*total;int chosen=0;
                for(int r=0;r<4;r++){if(pools[r].Count==0)continue;chosen=r;roll-=weights[r];if(roll<0)break;}
                var pool=pools[chosen];int at=rng.NextInt(pool.Count);var item=pool[at];pool.RemoveAt(at);
                state.Offers.Add(new MerchantOffer {ArtifactId=item.Id,Price=Price(item.Rarity,state.NextMerchant-1,false)});
            }
            if(state.Offers.Count>0){var sale=state.Offers[rng.NextInt(state.Offers.Count)];sale.Discount=true;sale.Price=Price(ArtifactRewardRules.Get(sale.ArtifactId).Rarity,state.NextMerchant-1,true);}
            state.RngState=rng.State;state.Prepared=true;
        }
        public static bool CanBuy(RunEconomyState state,int slot,IList<string> owned)
        {
            if(state==null||!state.Pending||!state.Prepared||!state.Open||slot<0||slot>=state.Offers.Count)return false;
            var offer=state.Offers[slot];var item=ArtifactRewardRules.Get(offer.ArtifactId);
            return item!=null&&item.Rarity!=ArtifactRarity.Legendary&&!offer.Sold&&offer.Price>0&&state.Echo>=offer.Price&&!owned.Contains(offer.ArtifactId);
        }
        // Callers apply the standard artifact acquisition effects, then persist the whole run.
        public static bool TryBuy(RunEconomyState state,int slot,IList<string> owned)
        {
            if(!CanBuy(state,slot,owned))return false;
            var offer=state.Offers[slot];owned.Add(offer.ArtifactId);state.Echo-=offer.Price;offer.Sold=true;return true;
        }
        public static void Close(RunEconomyState state){if(state==null)return;state.Open=false;state.Pending=false;}
    }
}
