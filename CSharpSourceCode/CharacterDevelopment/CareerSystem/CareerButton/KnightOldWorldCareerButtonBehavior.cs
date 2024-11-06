using System.Collections.Generic;
using System.Linq;
using Ink.Parsed;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TOR_Core.CampaignMechanics.CustomResources;
using TOR_Core.CampaignMechanics.Religion;
using TOR_Core.Extensions;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TOR_Core.Utilities;

namespace TOR_Core.CharacterDevelopment.CareerSystem.CareerButton;

public class KnightOldWorldCareerButtonBehavior(CareerObject career) : CareerButtonBehaviorBase(career)
{
    private const int MINIMUMLEVELFORSEAL = 5;

    private string _secularSealIcon = "CareerSystem\\aqshy";
    private string _sigmarSealIcon = "CareerSystem\\aqshy";
    private string _taalSealIcon = "CareerSystem\\ghyran";
    private string _ulricSealIcon = "CareerSystem\\azyr";
    private string _shallyaSealIcon = "CareerSystem\\ghyran";
    private string _manaanSealIcon = "CareerSystem\\manaan";
    private CharacterObject _setCharacter;

    public override void ButtonClickedEvent(CharacterObject characterObject, bool isPrisoner = false)
    {
        _setCharacter = characterObject;
        
        var seals = new List<KnightPuritySeal>();

        var secular = false;

        var dominantReligion = Hero.MainHero.GetDominantReligion();
        if(dominantReligion == null || Hero.MainHero.GetDevotionLevelForReligion(dominantReligion) < DevotionLevel.Fanatic )
        {
            seals = GetSecularSeals();
            secular = true;
        }
        else
        {
            seals = GetTemplarPuritySeals();
        }
        
        var inquiryElements = new List<InquiryElement>() ;
        for (var i = 0; i < seals.Count; i++)
        {
            var seal = seals[i];
            var icon = seal.SealIcon;

            var price = seal.Price;

            var displayName = seal.Name;
            var displayDescription = seal.Description;

            if (price > Hero.MainHero.GetCultureSpecificCustomResourceValue())
            {
                continue;
            }

            if (!secular)
            {
                if (Hero.MainHero.GetDominantReligion().StringId != seal.DeityCultId)
                {
                 continue;   
                }
            }
            var inquiryElement = new InquiryElement(seal,displayName.ToString(),null,true,displayDescription.ToString());
            
            inquiryElements.Add(inquiryElement);
        }

        var count = 1;

        if (Hero.MainHero.HasCareerChoice("PathOfGloryPassive4"))
        {
            count = 2;
        }
        
        var inquirydata = new MultiSelectionInquiryData("Choose purity Seal.", "Empower your Knight units with new powerful seals", inquiryElements,
            true, 1, count, "Accept", "Cancel", OnSelectedOption, OnCancel, "", false);
        MBInformationManager.ShowMultiSelectionInquiry(inquirydata);
    }

    private void OnCancel(List<InquiryElement> obj)
    {
        
    }

    private void OnSelectedOption(List<InquiryElement> elements)
    {
        var seals =new List<KnightPuritySeal>();
        foreach (var elem in elements)
        {
            var seal =  elem.Identifier as KnightPuritySeal;
            seals.Add(seal);
        }
      
        
        var partyExtendedInfo = ExtendedInfoManager.Instance.GetPartyInfoFor(Hero.MainHero.PartyBelongedTo.StringId);

        var currentSeals = GetCurrentActiveSeals(_setCharacter);
        if (currentSeals!=null&& !currentSeals.IsEmpty())
        {
            foreach (var elem in currentSeals)
            {
                partyExtendedInfo.RemoveTroopAttribute(_setCharacter.StringId,elem.SealId);
            }
            
        }

        foreach (var seal in seals)
        {
            partyExtendedInfo.AddTroopAttribute(_setCharacter, seal.SealId);
        }
        
        if (PartyVMExtension.ViewModelInstance != null) PartyVMExtension.ViewModelInstance.RefreshValues();

    }
    
    private List<KnightPuritySeal> GetCurrentActiveSeals(CharacterObject setCharacter)
    {
        if (setCharacter == null) return null;
        var partyExtendedInfo = ExtendedInfoManager.Instance.GetPartyInfoFor(Hero.MainHero.PartyBelongedTo.StringId);

        if (partyExtendedInfo.TroopAttributes.TryGetValue(setCharacter.StringId, out var attributes))
            if (attributes.Count > 0)
            {
                var availableSeals = GetSecularSeals();
                
                availableSeals.AddRange(GetTemplarPuritySeals());

                var seals = new List<KnightPuritySeal>();

                foreach (var seal in availableSeals)
                {
                    foreach (var attribute in attributes)
                    {
                        if (seal.SealId == attribute)
                        {
                            seals.Add(seal);
                        }
                    }
                }

                return seals;
            }

        return null;
    }

    public List<KnightPuritySeal> GetAllPuritySeals()
    {
        var list = new List<KnightPuritySeal>();
        list.AddRange(GetSecularSeals());
        list.AddRange(GetTemplarPuritySeals());

        return list;

    }

    private  List<KnightPuritySeal> GetSecularSeals()
    {
        return new List<KnightPuritySeal>()
        {
            new("SecularSeal1",  "test", null, 10, _secularSealIcon),
            new("SecularSeal2" ,null, null, 10, _secularSealIcon),
            new("SecularSeal3", "test", null, 10, _secularSealIcon),
        };
    }
    private List<KnightPuritySeal> GetTemplarPuritySeals()
    {
        return new List<KnightPuritySeal>()
        {

            
            new("SigmarSeal1","test","cult_of_sigmar",10, _sigmarSealIcon),
            new("SigmarSeal2",null,"cult_of_sigmar",10,_sigmarSealIcon),
            new("SigmarSeal3","test","cult_of_sigmar",10,_sigmarSealIcon),
            
            new("UlricSeal1","test","cult_of_ulric",10, _ulricSealIcon),
            new("UlricSeal2","test","cult_of_ulric",10, _ulricSealIcon),
            new("UlricSeal3","test","cult_of_ulric",10, _ulricSealIcon),
            
            new("TaalSeal1","test","cult_of_taal",10, _taalSealIcon),
            new("TaalSeal2",null,"cult_of_taal",10, _taalSealIcon),
            new("TaalSeal3","test","cult_of_taal",10,_taalSealIcon),
            
            new("ManaanSeal1","test","cult_of_manaan",10, _manaanSealIcon),
            new("ManaanSeal2",null,"cult_of_manaan",10, _manaanSealIcon),
            new("ManaanSeal3","test","cult_of_manaan",10, _manaanSealIcon),
            
            new("ShallyaSeal1","test","cult_of_shallya",10, _shallyaSealIcon),
            new("ShallyaSeal2","test","cult_of_shallya",10, _shallyaSealIcon),
            new("ShallyaSeal3","test","cult_of_shallya",10,_shallyaSealIcon),
        };
    }

    public override bool ShouldButtonBeVisible(CharacterObject characterObject, bool isPrisoner = false)
    {
        return characterObject.IsKnightUnit() && characterObject.Culture.StringId != TORConstants.Cultures.BRETONNIA && characterObject.Race == 0;
    }

    public override bool ShouldButtonBeActive(CharacterObject characterObject, out TextObject displayText, bool isPrisoner = false)
    {
        displayText = new TextObject("Add a purity Seal to the Knight");
        
        var currentSeals = GetCurrentActiveSeals(characterObject);

        if (currentSeals != null && !currentSeals.IsEmpty())
        {
            displayText = new TextObject("");
            foreach (var seal in currentSeals)
            {
                var text = displayText.ToString(); 
                text += seal.Description;
                text += "\n";
                displayText = new TextObject(text);
            }
           
            return true;
        }
        
        

        var devotion = Hero.MainHero.GetDominantReligion();
        
        if (devotion!=null && Hero.MainHero.GetDevotionLevelForReligion(Hero.MainHero.GetDominantReligion()) < DevotionLevel.Fanatic && !Hero.MainHero.HasCareerChoice("SecularOrdersPassive3"))
        {
            displayText = new TextObject("You are not religious enough to do that");
            
            return false;
        }
        
        if (characterObject.Tier <  MINIMUMLEVELFORSEAL)
        {
            GameTexts.SetVariable("MINIMUMSEALLEVEL",MINIMUMLEVELFORSEAL);
            displayText = new TextObject("Unit tier is not high enough. Minimum tier is {MINIMUMSEALLEVEL}.");
            return false;
        }
        return true;
    }
}

public class KnightPuritySeal()
{
    public KnightPuritySeal(string sealId, string weaponEffectId,string deityCultId, int price, string sealIcon) : this()
    {
        this.Name = GameTexts.TryGetText("TORKnightPuritySealName", out var nameText,sealId ) ? nameText : new TextObject(sealId);
        this.Description = GameTexts.TryGetText("TORKnightPuritySealDescription", out var descriptionText, sealId ) ? descriptionText : new TextObject("No description found");

        
        
        this.WeaponEffect = weaponEffectId;
        this.Price = price;
        this.DeityCultId = deityCultId;
        this.SealId = sealId;
        this.SealIcon = sealIcon;
    }
    
    public TextObject Name = new TextObject("");
    public TextObject Description = new TextObject("");
    public string SealId;
    public string WeaponEffect;
    public int Price;
    public string DeityCultId;
    public string SealIcon;
}