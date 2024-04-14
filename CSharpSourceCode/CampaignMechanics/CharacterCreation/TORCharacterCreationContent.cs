using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.Conversation.Tags;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TOR_Core.AbilitySystem.Spells;
using TOR_Core.CampaignMechanics.Religion;
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;
using TOR_Core.Extensions.ExtendedInfoSystem;
using TOR_Core.Utilities;

namespace TOR_Core.CampaignMechanics.CharacterCreation
{
    public class TORCharacterCreationContent : CharacterCreationContentBase
    {
        private List<CharacterCreationOption> _options;
        private readonly int _maxStageNumber = 3;
        private bool _isFemale = false;
        private int _originalRace = 0;

        public TORCharacterCreationContent()
        {
            try
            {
                var path = TORPaths.TORCoreModuleExtendedDataPath + "tor_cc_options.xml";
                XmlSerializer ser = new XmlSerializer(typeof(List<CharacterCreationOption>));
                _options = ser.Deserialize(File.OpenRead(path)) as List<CharacterCreationOption>;
            }
            catch (Exception)
            {
                TORCommon.Log("Failed to open tor_cc_options.xml for character creation.", NLog.LogLevel.Error);
                throw;
            }
            ExtendedInfoManager.Instance.ClearInfo(Hero.MainHero);
        }

        public override TextObject ReviewPageDescription => new TextObject("{=tor_cc_finished_info_str}You prepare to enter The Old World! Here is your character. Click finish if you are ready, or go back to make changes.", null);

        public override IEnumerable<Type> CharacterCreationStages
        {
            get
            {
                yield return typeof(CharacterCreationCultureStage);
                yield return typeof(CharacterCreationFaceGeneratorStage);
                yield return typeof(CharacterCreationGenericStage);
                yield return typeof(CharacterCreationOptionsStage);
                yield return typeof(CharacterCreationBannerEditorStage);
                yield return typeof(CharacterCreationClanNamingStage);
                yield return typeof(CharacterCreationReviewStage);
                yield break;
            }
        }

        protected override void OnInitialized(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation characterCreation)
        {
            AddMenus(characterCreation);
        }

        private void AddMenus(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation characterCreation)
        {
            //stages
            CharacterCreationMenu stage1Menu = new CharacterCreationMenu(new TextObject("{=tor_cc_origin_summary_str}Origin", null), new TextObject("{=tor_cc_origin_text_str}Choose your family's background...", null), new CharacterCreationOnInit(OnMenuInit), CharacterCreationMenu.MenuTypes.MultipleChoice);
            CharacterCreationMenu stage2Menu = new CharacterCreationMenu(new TextObject("{=tor_cc_growth_summary_str}Growth", null), new TextObject("{=tor_cc_growth_text_str}Teenage years...", null), new CharacterCreationOnInit(OnMenuInit), CharacterCreationMenu.MenuTypes.MultipleChoice);
            CharacterCreationMenu stage3Menu = new CharacterCreationMenu(new TextObject("{=tor_cc_profession_summary_str}Profession", null), new TextObject("{=tor_cc_profession_text_str}Your starting profession...", null), new CharacterCreationOnInit(OnMenuInit), CharacterCreationMenu.MenuTypes.MultipleChoice);

            for (int i = 1; i <= _maxStageNumber; i++)
            {
                List<string> cultures = new List<string>();
                _options.ForEach(x =>
                {
                    if (x.StageNumber == i && !cultures.Contains(x.Culture))
                    {
                        cultures.Add(x.Culture);
                    }
                });
                foreach (var culture in cultures)
                {
                    CharacterCreationCategory category = new CharacterCreationCategory();
                    switch (i)
                    {
                        case 1:
                            category = stage1Menu.AddMenuCategory(delegate ()
                            {
                                return GetSelectedCulture().StringId == culture;
                            });
                            break;
                        case 2:
                            category = stage2Menu.AddMenuCategory(delegate ()
                            {
                                return GetSelectedCulture().StringId == culture;
                            });
                            break;
                        case 3:
                            category = stage3Menu.AddMenuCategory(delegate ()
                            {
                                return GetSelectedCulture().StringId == culture;
                            });
                            break;
                        default:
                            break;
                    }

                    var relevantOptions = _options.FindAll(x => x.StageNumber == i && x.Culture.Equals(culture));
                    foreach (var option in relevantOptions)
                    {
                        var effectedSkills = new MBList<SkillObject>();
                        foreach (var skillId in option.SkillsToIncrease)
                        {
                            effectedSkills.Add(Skills.All.FirstOrDefault(x => x.StringId == skillId));
                        }
                        CharacterAttribute attribute = Attributes.All.Where(x => x.StringId == option.AttributeToIncrease.ToLower()).FirstOrDefault();
                        category.AddCategoryOption(new TextObject(option.OptionText), effectedSkills, attribute, FocusToAdd, SkillLevelToAdd, AttributeLevelToAdd, null, delegate (TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation charInfo)
                        {
                            OnOptionSelected(charInfo, option.Id);
                        },
                        delegate (TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation charInfo)
                        {
                            OnOptionFinalize(charInfo, option.Id);
                        },
                        new TextObject(option.OptionFlavourText));
                    }
                }
            }

            characterCreation.AddNewMenu(stage1Menu);
            characterCreation.AddNewMenu(stage2Menu);
            characterCreation.AddNewMenu(stage3Menu);
        }

        protected override void OnApplyCulture()
        {
            _originalRace = CharacterObject.PlayerCharacter.Race;
        }

        private void OnMenuInit(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation charInfo)
        {
            charInfo.IsPlayerAlone = true;
            charInfo.HasSecondaryCharacter = false;
            charInfo.ClearFaceGenMounts();
            _isFemale = CharacterObject.PlayerCharacter.IsFemale;
            _originalRace = CharacterObject.PlayerCharacter.Race;
            //if(Debugger.IsAttached) _originalRace = CharacterObject.PlayerCharacter.Race; //This is to allow becoming different races by selecting them at character creation for development purposes.
        }

        private void OnOptionSelected(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation charInfo, string optionId)
        {
            var selectedOption = _options.Find(x => x.Id == optionId);
            charInfo.ClearFaceGenPrefab();
            var race = _originalRace;
            Hero.MainHero.UpdatePlayerGender(_isFemale);
            
            
            
            if (IsVampireCharacterCreationID (optionId))
            {
                race = FaceGen.GetRaceOrDefault("vampire");
            }
            else if(IsDamselCharacterCreationID(optionId) && !CharacterObject.PlayerCharacter.IsFemale)
            {
                Hero.MainHero.UpdatePlayerGender(true);
            }
            else if(IsKnightErrantCharacterCreationID(optionId) && CharacterObject.PlayerCharacter.IsFemale)
            {
                Hero.MainHero.UpdatePlayerGender(false);
            }
            UpdateVisuals(race, charInfo);
            UpdateEquipment(selectedOption, charInfo);
            
        }

        private void UpdateVisuals(int race, TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation charInfo)
        {
            List<FaceGenChar> list = new List<FaceGenChar>();
            BodyProperties bodyProperties = CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment, -1);
            list.Add(new FaceGenChar(bodyProperties, race, new Equipment(), CharacterObject.PlayerCharacter.IsFemale));
            charInfo.ChangeFaceGenChars(list);
            CharacterObject.PlayerCharacter.Race = race;
        }
        private void UpdateEquipment(CharacterCreationOption selectedOption, TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation charInfo)
        {
            List<Equipment> list = new List<Equipment>();
            Equipment equipment = null;
            try
            {
                equipment = MBObjectManager.Instance.GetObject<MBEquipmentRoster>(selectedOption.EquipmentSetId).DefaultEquipment;
                if (equipment == null) MBObjectManager.Instance.CreateObject<MBEquipmentRoster>(selectedOption.EquipmentSetId);
            }
            catch (NullReferenceException)
            {
                TORCommon.Log("Attempted to read characterobject " + selectedOption.EquipmentSetId + " in Character Creation, but no such entry exists in XML. Falling back to default.", NLog.LogLevel.Error);
                throw;
            }
            if (equipment != null)
            {
                if (equipment.IsValid && !equipment.IsEmpty())
                {
                    list.Add(equipment);
                    charInfo.ChangeCharactersEquipment(list);
                    PlayerStartEquipment = equipment;
                    CharacterObject.PlayerCharacter.Equipment.FillFrom(PlayerStartEquipment);
                }
            }
        }

        private void OnOptionFinalize(TaleWorlds.CampaignSystem.CharacterCreationContent.CharacterCreation charInfo, string id)
        {
            Hero.MainHero.AddAttribute("AbilityUser");
            Hero.MainHero.AddAttribute("CanPlaceArtillery");

            if (Hero.MainHero.Culture.StringId == "mousillon")
            {
                Hero.MainHero.AddReligiousInfluence(ReligionObject.All.FirstOrDefault(x => x.StringId == "cult_of_nagash"), 60, false);
            }
            
            if (IsMagicianCharacterCreationID (id) || IsDamselCharacterCreationID (id))
            {
                Hero.MainHero.AddAttribute("SpellCaster");
                Hero.MainHero.AddAbility("Dart");
                Hero.MainHero.AddKnownLore("MinorMagic");
                var skill = Hero.MainHero.GetSkillValue(TORSkills.SpellCraft);
                Hero.MainHero.HeroDeveloper.SetInitialSkillLevel(TORSkills.SpellCraft, Math.Max(skill, 25));
                Hero.MainHero.HeroDeveloper.AddPerk(TORPerks.SpellCraft.EntrySpells);
            }

            if (IsWitchHunterCharacterCreationID(id))
            {
                Hero.MainHero.AddCareer(TORCareers.WitchHunter);
            }
            
            if (IsKnightErrantCharacterCreationID(id))
            {
                Hero.MainHero.AddCareer(TORCareers.GrailKnight);
            }

            if (IsKnightOfMousillonCharacterCreationId(id))
            {
                Hero.MainHero.AddCareer(TORCareers.BlackGrailKnight);
            }
            
            if(IsDamselCharacterCreationID (id))
            {
                Hero.MainHero.AddAttribute("Priest");
                Hero.MainHero.AddCareer(TORCareers.GrailDamsel);
                var skill = Hero.MainHero.GetSkillValue(TORSkills.Faith);
                Hero.MainHero.HeroDeveloper.SetInitialSkillLevel(TORSkills.Faith, Math.Max(skill, 25));
                var knight = MBObjectManager.Instance.GetObject<CharacterObject>("tor_br_realm_knight");
                Hero.MainHero.AddAbility("AuraOfTheLady");
                Hero.MainHero.PartyBelongedTo.Party.AddMember(knight, 1, 0);
            }
            
            if (IsPriestAcolyteCharacterCreationID(id))
            {
                Hero.MainHero.AddAttribute("Priest");
            }
            else if (IsNecromancerCharacterCreationID (id))
            {
                Hero.MainHero.AddAttribute("SpellCaster");
                Hero.MainHero.AddAttribute("Necromancer");
                Hero.MainHero.AddAbility("SummonSkeleton");
                Hero.MainHero.AddKnownLore("MinorMagic");
                Hero.MainHero.AddKnownLore("Necromancy");
                var skill = Hero.MainHero.GetSkillValue(TORSkills.SpellCraft);
                Hero.MainHero.HeroDeveloper.SetInitialSkillLevel(TORSkills.SpellCraft, Math.Max(skill, 25));
                Hero.MainHero.HeroDeveloper.AddPerk(TORPerks.SpellCraft.EntrySpells);
                Hero.MainHero.AddCareer(TORCareers.Necromancer);
                Hero.MainHero.AddReligiousInfluence(ReligionObject.All.FirstOrDefault(x => x.StringId == "cult_of_nagash"), 25);
            }
            else if (IsVampireCharacterCreationID (id))
            {
                Hero.MainHero.AddAttribute("Vampire");
                Hero.MainHero.AddAttribute("Necromancer");
                Hero.MainHero.AddReligiousInfluence(ReligionObject.All.FirstOrDefault(x => x.StringId == "cult_of_nagash"), 60);
            }

            if (Hero.MainHero.GetCareer() == null)
            {
                Hero.MainHero.AddCareer(TORCareers.Mercenary);
            }
        }

        public override void OnCharacterCreationFinalized()
        {
            CultureObject culture = CharacterObject.PlayerCharacter.Culture;
            Hero.MainHero.AddCultureSpecificCustomResource(0);
            Vec2 position2D = default(Vec2);

            switch (culture.StringId)
            {
                case "empire":
                    position2D = new Vec2(1450.97f, 991.37f);
                    break;
                case "khuzait":
                    position2D = new Vec2(1617.54f, 969.70f);
                    break;
                case "vlandia":
                    position2D = new Vec2(998.96f, 830.02f);
                    break;
                case "mousillon":
                    position2D = new Vec2(932.531f, 1049.944f);
                    break;
                default:
                    position2D = new Vec2(1420.97f, 981.37f);
                    break;
            }
            MobileParty.MainParty.Position2D = position2D;
            MapState mapState;
            if ((mapState = (GameStateManager.Current.ActiveState as MapState)) != null)
            {
                mapState.Handler.ResetCamera(true, true);
                mapState.Handler.TeleportCameraToMainParty();
            }
            SetHeroAge(25);
            if (Hero.MainHero.IsSpellCaster()) PromptChooseLore();
            if (Hero.MainHero.IsVampire()) PromptChooseBloodline();
            if (Hero.MainHero.Culture.StringId == "empire" && Hero.MainHero.IsPriest()) PromptChoosePriesthood();
            
        }

        protected void SetHeroAge(float age)
        {
            Hero.MainHero.SetBirthDay(CampaignTime.YearsFromNow(-age));
        }

        private bool IsKnightOfMousillonCharacterCreationId(string characterCreationOptionID) =>  characterCreationOptionID == "option56";
        private bool IsWitchHunterCharacterCreationID(string characterCreationOptionID) =>  characterCreationOptionID == "option14";
        private bool IsKnightErrantCharacterCreationID(string characterCreationOptionID) =>  characterCreationOptionID == "option41";
        private bool IsVampireCharacterCreationID(string characterCreationOptionID) => characterCreationOptionID == "option26" || characterCreationOptionID == "option57";
        private bool IsMagicianCharacterCreationID(string characterCreationOptionID) => characterCreationOptionID == "option12";
        private bool IsDamselCharacterCreationID(string characterCreationOptionID) => characterCreationOptionID == "option42";
        private bool IsPriestAcolyteCharacterCreationID(string characterCreationOptionID) => characterCreationOptionID == "option13";
        private bool IsNecromancerCharacterCreationID(string characterCreationOptionID) => characterCreationOptionID == "option27" || characterCreationOptionID == "option58";

        private void PromptChooseLore()
        {
            List<InquiryElement> list = new List<InquiryElement>();
            var lores = LoreObject.GetAll();
            foreach (var item in lores)
            {
                if (item.ID != "MinorMagic" && !item.DisabledForCultures.Contains(CharacterObject.PlayerCharacter.Culture.StringId) && !Hero.MainHero.GetExtendedInfo().HasKnownLore(item.ID)&&!item.IsRestrictedToVampires) list.Add(new InquiryElement(item, item.Name, null)) ;
            }

            if (list.IsEmpty()) return;
            
            var inquirydata = new MultiSelectionInquiryData("Choose Lore", "Choose a lore to specialize in.", list, false, 1, 1, "Confirm", "Cancel", OnChooseLore, OnCancel);
            MBInformationManager.ShowMultiSelectionInquiry(inquirydata, true);
        }

        private void OnChooseLore(List<InquiryElement> obj)
        {
            var choice = obj[0].Identifier as LoreObject;
            var info = Hero.MainHero.GetExtendedInfo();
            if (choice != null)
            {
                Hero.MainHero.AddKnownLore(choice.ID);
                if (info.SpellCastingLevel < SpellCastingLevel.Entry) Hero.MainHero.SetSpellCastingLevel(SpellCastingLevel.Entry);
                MBInformationManager.AddQuickInformation(new TextObject("Successfully learned lore: " + choice.Name), 0, CharacterObject.PlayerCharacter);
            }
            InformationManager.HideInquiry();
        }

        private void OnCancel(List<InquiryElement> obj)
        {
            MBInformationManager.AddQuickInformation(new TextObject("You MUST choose."));
        }

        private void PromptChooseBloodline()
        {
            List<InquiryElement> list = new List<InquiryElement>();
            list.Add(new InquiryElement("generic_vampire", "Von Carstein Vampire", null));
            list.Add(new InquiryElement("blood_knight", "Blood Knight", null));
            list.Add(new InquiryElement("necrarch", "Necrarch", null));
            var inquirydata = new MultiSelectionInquiryData("Choose Bloodline", "Choose your vampiric bloodline.", list, false, 1, 1, "Confirm", "Cancel", OnChooseBloodline, OnCancel);
            MBInformationManager.ShowMultiSelectionInquiry(inquirydata, true);
        }
        
        private void PromptChoosePriesthood()
        {
            List<InquiryElement> list = new List<InquiryElement>();
            list.Add(new InquiryElement("WarriorPriest", "Sigmar", null));
            list.Add(new InquiryElement("WarriorPriestUlric", "Ulric", null));
            var inquirydata = new MultiSelectionInquiryData("Choose God", "You are a priest of the Empire. Choose the God you are devoted to.", list, false, 1, 1, "Confirm", "Cancel", OnChoosePriesthood, OnCancel);
            MBInformationManager.ShowMultiSelectionInquiry(inquirydata, true);
        }

        private void OnChoosePriesthood(List<InquiryElement> obj)
        {
            var choice = obj[0].Identifier as string;
            if(choice == "WarriorPriest")
            {
                Hero.MainHero.AddCareer(TORCareers.WarriorPriest);
                Hero.MainHero.AddReligiousInfluence(ReligionObject.All.FirstOrDefault(x => x.StringId == "cult_of_sigmar"), 60);
            }

            if (choice == "WarriorPriestUlric")
            {
                Hero.MainHero.AddCareer(TORCareers.WarriorPriestUlric);
                Hero.MainHero.AddReligiousInfluence(ReligionObject.All.FirstOrDefault(x => x.StringId == "cult_of_ulric"), 60);
            }
            
            var skill = Hero.MainHero.GetSkillValue(TORSkills.Faith);
            Hero.MainHero.HeroDeveloper.SetInitialSkillLevel(TORSkills.Faith, Math.Max(skill, 25));
            Hero.MainHero.HeroDeveloper.AddPerk(TORPerks.Faith.NovicePrayers);
        }
        private void OnChooseBloodline(List<InquiryElement> obj)
        {
            var choice = obj[0].Identifier as string;
            if(choice == "generic_vampire")
            {
                Hero.MainHero.AddAttribute("SpellCaster");
                Hero.MainHero.AddAbility("NagashGaze");
                Hero.MainHero.AddKnownLore("MinorMagic");
                Hero.MainHero.AddKnownLore("Necromancy");
                var skill = Hero.MainHero.GetSkillValue(TORSkills.SpellCraft);
                Hero.MainHero.HeroDeveloper.SetInitialSkillLevel(TORSkills.SpellCraft, Math.Max(skill, 25));
                Hero.MainHero.HeroDeveloper.AddPerk(TORPerks.SpellCraft.EntrySpells);
                MBInformationManager.AddQuickInformation(new TextObject("Successfully learned Necromancy"), 0, CharacterObject.PlayerCharacter);
                Hero.MainHero.AddCareer(TORCareers.MinorVampire);
            }

            if (choice == "blood_knight")
            {
                Hero.MainHero.AddCareer(TORCareers.BloodKnight);
            }

            if (choice == "necrarch")
            {
                Hero.MainHero.AddAttribute("SpellCaster");
                Hero.MainHero.AddAbility("NagashGaze");
                Hero.MainHero.AddKnownLore("MinorMagic");
                Hero.MainHero.AddKnownLore("Necromancy");
                Hero.MainHero.AddCareer(TORCareers.Necrarch);
                var skill = Hero.MainHero.GetSkillValue(TORSkills.SpellCraft);
                Hero.MainHero.HeroDeveloper.SetInitialSkillLevel(TORSkills.SpellCraft, Math.Max(skill, 25));
                Hero.MainHero.HeroDeveloper.AddPerk(TORPerks.SpellCraft.EntrySpells);
                MBInformationManager.AddQuickInformation(new TextObject("Successfully learned Necromancy"), 0, CharacterObject.PlayerCharacter);
            }
        }
    }
}
