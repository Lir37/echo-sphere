using System;
using EchoSphere.Core;
using UnityEngine;
namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private const string MetaWalletPreferenceKey="echosphere_meta_wallet_v1";
        private const string MetaWalletBackupPreferenceKey="echosphere_meta_wallet_backup_v1";
        private string _runMetaReceipt;
        private MetaWallet _metaWallet;
        private string _metaWalletRaw;
        private string _metaEntitlementsRaw;
        private static MetaWallet ParseMetaWallet(string raw)
        {try{var value=JsonUtility.FromJson<MetaWallet>(raw);return MetaCurrencyRules.Valid(value)?value:null;}catch(ArgumentException){return null;}}
        private MetaWallet ReadMetaWallet()
        {
            string raw=PlayerPrefs.GetString(MetaWalletPreferenceKey,"");
            if(_metaWallet!=null&&raw==_metaWalletRaw)return _metaWallet;
            if(!PlayerPrefs.HasKey(MetaWalletPreferenceKey)){
                var migrated=MetaCurrencyRules.MigrateGold(PlayerPrefs.GetInt(GoldPreferenceKey,0));
                WriteMetaWallet(migrated);return _metaWallet;
            }
            var loaded=ParseMetaWallet(raw);
            if(loaded==null){
                loaded=ParseMetaWallet(PlayerPrefs.GetString(MetaWalletBackupPreferenceKey,""));
                if(loaded==null)throw new InvalidOperationException("Meta wallet is unreadable; saved data retained. Refusing to reset balances or repeat gold migration.");
                // Preserve the corrupt primary for diagnosis before recovering.
                PlayerPrefs.SetString("echosphere_meta_wallet_recovery",raw);WriteMetaWallet(loaded);return _metaWallet;
            }
            _metaWallet=loaded;_metaWalletRaw=raw;return loaded;
        }
        private void WriteMetaWallet(MetaWallet wallet)
        {
            if(!MetaCurrencyRules.Valid(wallet))throw new ArgumentException("Invalid meta wallet");
            string raw=JsonUtility.ToJson(wallet);
            PlayerPrefs.SetString(MetaWalletPreferenceKey,raw);
            PlayerPrefs.SetString(MetaWalletBackupPreferenceKey,raw);
            // Mirror for existing source adapters; never ADD it again once
            // a wallet exists. The JSON envelope is authoritative after migration.
            PlayerPrefs.SetInt(GoldPreferenceKey,wallet.Credits);PlayerPrefs.Save();
            _metaWallet=wallet;_metaWalletRaw=raw;
        }
        private int GetMetaBalance(CurrencyId id)
        {var wallet=ReadMetaWallet();ReconcileMetaEntitlements(wallet);return MetaCurrencyRules.Balance(wallet,id);}
        private void ReconcileMetaEntitlements(MetaWallet wallet)
        {
            if(_metaEntitlementsRaw==_metaWalletRaw)return;_metaEntitlementsRaw=_metaWalletRaw;
            bool shop=false,characters=false,mastery=false,lore=false;
            foreach(var receipt in wallet.Receipts){if(receipt==null)continue;if(receipt.StartsWith("purchase:shop:",StringComparison.Ordinal))shop=true;if(receipt.StartsWith("purchase:character:",StringComparison.Ordinal))characters=true;if(receipt.StartsWith("purchase:mastery:",StringComparison.Ordinal))mastery=true;if(receipt.StartsWith("purchase:lore:",StringComparison.Ordinal))lore=true;}
            // An already-paid receipt proves ownership even if the corresponding
            // old-format profile write was interrupted. Existing progress wins.
            if(shop){EnsureShopSaveLoaded();bool changed=false;foreach(string id in PersistentUpgradeRules.Ids){int level=_shopSave.upgrades.Get(id),max=PersistentUpgradeRules.GetMaxLevel(id);
                for(int target=level+1;target<=max;target++)if(MetaCurrencyRules.HasReceipt(wallet,"purchase:shop:"+id+":"+target)){level=target;changed=true;}_shopSave.upgrades.Set(id,level);
            }if(changed)SaveShopState();}
            if(characters||mastery){EnsureCharacterProfilesLoaded();bool changed=false;foreach(var definition in CharacterRules.All){var profile=FindCharacterProfile(definition.Id);if(profile==null)continue;
                if(!profile.unlocked&&MetaCurrencyRules.HasReceipt(wallet,"purchase:character:"+profile.id)){profile.unlocked=true;changed=true;}
                if(mastery)for(int level=2;level<=10;level++)if(level>profile.masteryLevel&&MetaCurrencyRules.HasReceipt(wallet,"purchase:mastery:"+profile.id+":"+level)){profile.unlocked=true;RestoreLegacyPurchasedMastery(profile,level);changed=true;}
            }if(changed)SaveCharacterProfiles();}
            if(lore){EnsureKnowledgeLoaded();bool changed=false;foreach(var entry in CampaignRegionLore.All)if(MetaCurrencyRules.HasReceipt(wallet,"purchase:lore:"+entry.Id))changed|=_knownKnowledgeIds.Add(entry.Id);
                if(changed){PlayerPrefs.SetString("echosphere_knowledge_v1",JsonUtility.ToJson(new KnowledgeSave{ids=new System.Collections.Generic.List<string>(_knownKnowledgeIds)}));PlayerPrefs.Save();}}
        }
        private bool TryBuyMeta(string receipt,CurrencyId id,int amount)
        {var wallet=ReadMetaWallet();if(!MetaCurrencyRules.TryPurchaseOnce(wallet,receipt,id,amount))return false;WriteMetaWallet(wallet);return true;}
        private bool GrantMetaOnce(string receipt,int credits,int shards,int data)
        {var wallet=ReadMetaWallet();if(!MetaCurrencyRules.TryGrantOnce(wallet,receipt,credits,shards,data))return false;WriteMetaWallet(wallet);return true;}
        private void ClearMetaWallet()
        {PlayerPrefs.DeleteKey(MetaWalletPreferenceKey);PlayerPrefs.DeleteKey(MetaWalletBackupPreferenceKey);PlayerPrefs.DeleteKey("echosphere_meta_wallet_recovery");_metaWallet=null;_metaWalletRaw=null;_metaEntitlementsRaw=null;}
        private string MetaBalanceLabel(CurrencyId id)=>MetaCurrencyRules.Name(id,IsRussianUi)+": "+GetMetaBalance(id);
    }
}
