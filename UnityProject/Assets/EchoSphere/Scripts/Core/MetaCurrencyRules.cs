using System;
using System.Collections.Generic;
namespace EchoSphere.Core
{
    public enum CurrencyId { Echo, Credits, ResonanceShards, ArchiveData }
    [Serializable] public sealed class MetaWallet
    {
        // No initializer: a missing schema version in JSON must not masquerade
        // as a valid empty wallet and erase the migrated gold balance.
        public int Version, Credits, ResonanceShards, ArchiveData;
        public List<string> Receipts;
    }
    public static class MetaCurrencyRules
    {
        // Authored initial economy, approved2026-11-01. No exchange between currencies.
        public const int NaturalDiscoveryData=1, LoreResearchCost=3, StabilizationShards=3;
        public static int CharacterUnlockShards(int legacyUnlockCost)=>(int)Math.Min(int.MaxValue,Math.Max(0,((long)legacyUnlockCost+149)/150));
        public static int MasteryResearchShards(int currentLevel)=>currentLevel<1||currentLevel>=10?0:2*(currentLevel+1);
        public static int RunShards(int bosses,bool stabilized)=>(int)Math.Min(int.MaxValue,Math.Max(0L,bosses)+(stabilized?StabilizationShards:0));
        public static MetaWallet MigrateGold(int gold)=>new MetaWallet{Version=1,Credits=Math.Max(0,gold),Receipts=new List<string>()};
        public static bool Valid(MetaWallet wallet)=>wallet!=null&&wallet.Version==1&&wallet.Credits>=0&&wallet.ResonanceShards>=0&&wallet.ArchiveData>=0&&wallet.Receipts!=null;
        public static bool IsPersistent(CurrencyId id)=>id==CurrencyId.Credits||id==CurrencyId.ResonanceShards||id==CurrencyId.ArchiveData;
        public static int Balance(MetaWallet wallet,CurrencyId id)
        {if(!Valid(wallet)||!IsPersistent(id))return 0;return id==CurrencyId.Credits?wallet.Credits:id==CurrencyId.ResonanceShards?wallet.ResonanceShards:wallet.ArchiveData;}
        private static void Set(MetaWallet wallet,CurrencyId id,int value)
        {if(id==CurrencyId.Credits)wallet.Credits=value;else if(id==CurrencyId.ResonanceShards)wallet.ResonanceShards=value;else wallet.ArchiveData=value;}
        public static bool TrySpend(MetaWallet wallet,CurrencyId id,int amount)
        {if(!Valid(wallet)||!IsPersistent(id)||amount<=0||Balance(wallet,id)<amount)return false;Set(wallet,id,Balance(wallet,id)-amount);return true;}
        public static bool TryPurchaseOnce(MetaWallet wallet,string receipt,CurrencyId id,int cost)
        {
            if(!Valid(wallet)||string.IsNullOrEmpty(receipt)||!IsPersistent(id)||cost<=0)return false;
            if(HasReceipt(wallet,receipt))return true;
            if(!TrySpend(wallet,id,cost))return false;
            wallet.Receipts.Add(receipt);return true;
        }
        public static bool HasReceipt(MetaWallet wallet,string receipt)=>Valid(wallet)&&!string.IsNullOrEmpty(receipt)&&wallet.Receipts.Contains(receipt);
        // Receipt and all three balances belong to one persisted envelope.
        public static bool TryGrantOnce(MetaWallet wallet,string receipt,int credits,int shards,int data)
        {
            if(!Valid(wallet)||string.IsNullOrEmpty(receipt)||credits<0||shards<0||data<0||HasReceipt(wallet,receipt))return false;
            wallet.Credits=(int)Math.Min(int.MaxValue,(long)wallet.Credits+credits);
            wallet.ResonanceShards=(int)Math.Min(int.MaxValue,(long)wallet.ResonanceShards+shards);
            wallet.ArchiveData=(int)Math.Min(int.MaxValue,(long)wallet.ArchiveData+data);
            wallet.Receipts.Add(receipt);return true;
        }
        public static string Name(CurrencyId id,bool ru)
        {switch(id){case CurrencyId.Echo:return ru?"Эхо":"Echo";case CurrencyId.Credits:return ru?"Кредиты":"Credits";case CurrencyId.ResonanceShards:return ru?"Осколки резонанса":"Resonance Shards";case CurrencyId.ArchiveData:return ru?"Данные архива":"Archive Data";default:throw new ArgumentOutOfRangeException(nameof(id));}}
        public static string Purpose(CurrencyId id,bool ru)
        {switch(id){case CurrencyId.Echo:return ru?"Ресурс забега для торговца; не переносится между забегами.":"Run resource for the merchant; not carried between runs.";case CurrencyId.Credits:return ru?"Награда за забег. Постоянные улучшения в магазине.":"Run reward. Permanent shop upgrades.";case CurrencyId.ResonanceShards:return ru?"За босса +1, за стабилизацию +"+StabilizationShards+". Открытие персонажей и необязательное ускорение мастерства; обычный рост от XP сохраняется.":"+1 per Boss, +"+StabilizationShards+" for stabilization. Character unlocks and optional mastery acceleration; normal XP progression remains available.";case CurrencyId.ArchiveData:return ru?"За новую находку +"+NaturalDiscoveryData+". Исследование региональной записи стоит "+LoreResearchCost+" и не приносит новых данных.":"New discovery +"+NaturalDiscoveryData+". Regional lore research costs "+LoreResearchCost+" and does not grant new Data.";default:throw new ArgumentOutOfRangeException(nameof(id));}}
    }
}
