using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder;
using ChampionFocusSpells;

namespace Dawnsbury.Mods.Classes.Champion
{
    public class DawnsburyChampion
    {
        public static Trait ChampionTrait;
        public static Trait ChampionDeityTrait;
        public static Trait ChampionDomainTrait;
        public static Trait ChampionFeatureTrait;

        public static FeatName DomainFire;
        public static FeatName DomainTravel;
        public static FeatName DomainCold;
        public static FeatName DomainLuck;
        public static FeatName DomainDestruction;
        public static FeatName DomainSun;
        public static FeatName DomainDeath;
        public static FeatName DomainHealing;
        public static FeatName DomainMoon;
        public static FeatName DomainUndeath;

        public static List<Feat>? ChampionDomains;
        public static List<Feat>? ChampionDeities;

        [DawnsburyDaysModMainMethod]
        public static void LoadMod()
        {
            ChampionSpells.LoadSpells();
            ChampionTrait = ModManager.RegisterTrait("Champion", new TraitProperties("Champion", true, relevantForShortBlock: false) { IsClassTrait = true });
            ChampionDeityTrait = ModManager.RegisterTrait("ChampionDeity", new TraitProperties("Champion", true, relevantForShortBlock: false) { IsClassTrait = false });
            ChampionDomainTrait = ModManager.RegisterTrait("ChampionDomain", new TraitProperties("Champion", true, relevantForShortBlock: false) { IsClassTrait = false });
            ChampionFeatureTrait = ModManager.RegisterTrait("ChampionFeature", new TraitProperties("Champion", true, relevantForShortBlock: false) { IsClassTrait = false });

            DawnsburyChampionFeats.LoadFeats();


            LoadChampionDomains();

            ChampionDomains = LoadChampionDomains().ToList();
            ChampionDomains.ForEach(domain =>
            {
                ModManager.AddFeat(domain);
            });

            List<Feat> ChampionDeities = LoadChampionDeities().ToList();
            ChampionDeities.ForEach(deity =>
            {
                ModManager.AddFeat(deity);
            });

            ModManager.AddFeat(GenerateClassSelectionFeat());

        }
        private static Feat GenerateClassSelectionFeat()
        {

            LimitedAbilityBoost classBoosts = new LimitedAbilityBoost(Ability.Strength, Ability.Dexterity);
            Trait[] classTrained = new[] { Trait.Perception, Trait.Reflex, Trait.Unarmed, Trait.Simple, Trait.Martial,
                Trait.UnarmoredDefense, Trait.LightArmor, Trait.MediumArmor, Trait.HeavyArmor, Trait.Divine };
            Trait[] classExpert = new[] { Trait.Fortitude, Trait.Will };
            string flavorText = "You are an emissary of a deity, a devoted servant who has taken up a weighty mantle, and you adhere to a code that holds you apart from those around you. While champions exist for every alignment, as a champion of good, you provide certainty and hope to the innocent. You have powerful defenses that you share freely with your allies and innocent bystanders, as well as holy power you use to end the threat of evil. Your devotion even attracts the attention of holy spirits who aid you on your journey.";
            string rulesText =
                "{b}Deity and Cause:{/b} Champions are divine servants of a deity. Choose a deity to follow; your alignment must be one allowed for followers of your deity. Your cause determines your champion's reaction and grants you a devotion spell."
                + "\n\n{b}Deific Weapon:{/b} You zealously bear your deity's favored weapon. If it's an unarmed attack with a {b}d4{/b} damage die or a simple weapon, increase the damage die by one step. (You gain the {b}Deadly Simplicity{/b} feat.)"
                + "\n\n{b}Champion's Reaction:{/b} Your cause gives you a special reaction."
                + "\n\n{b}Devotion Spells{/b}: Your deity's power grants you special divine spells called devotion spells, which are a type of focus spell. It costs 1 Focus Point to cast a focus spell, and you start with a focus pool of 1 Focus Point."
                + "\n\n{b}Shield Block:{/b} You gain the Shield Block general feat, a reaction that lets you reduce damage with your shield.";

            var championClass = new ClassSelectionFeat(FeatName.CustomFeat, flavorText, ChampionTrait, classBoosts, 10,
                classTrained, classExpert, 2, rulesText, DawnsburyChampionCauses()).WithCustomName("Champion")
                .WithOnSheet(sheet =>
                {
                    CalculatedCharacterSheetValues sheet2 = sheet;
                    sheet2.SetProficiency(Trait.Spell, Proficiency.Trained);
                    sheet2.GrantFeat(FeatName.ShieldBlock);
                    sheet2.GrantFeat(FeatName.DeadlySimplicity);
                    sheet2.AddSelectionOption(new SingleFeatSelectionOption("level1ChampionFeat", "Champion feat", 1, (feat) => feat is TrueFeat && feat.HasTrait(ChampionTrait)));
                    sheet2.AddSelectionOption(new SingleFeatSelectionOption("championDeity", "Deity", 1, (feat) =>
                    {
                        return feat is ChampionDeitySelectionFeat;
                    })); ;
                    DeitySelectionFeat test = sheet.Deity;

                    if (!(test == null))
                    {
                        sheet2.AddSelectionOption(new SingleFeatSelectionOption("championDomain", "Domain: " + test.ToString(), 1, (feat) =>
                        {
                            return feat is ChampionDeitySelectionFeat;
                        })); ;
                    }

                    sheet2.AddAtLevel(3, delegate
                    {
                        sheet2.AddSelectionOption(new SingleFeatSelectionOption("divineAllySelection", "Divine Ally", 3, (feat) => (feat.FeatName == DawnsburyChampionFeats.BladeAllyFeatName || feat.FeatName == DawnsburyChampionFeats.ShieldAllyFeatName)));
                    });
                });

            return championClass;
        }
        public static IEnumerable<Feat> LoadChampionDomains()
        {
            FeatName domainName;

            domainName = ModManager.RegisterFeatName("ChampionDomainFire", "Fire");
            DomainFire = domainName;
            yield return CreateChampionDomain(domainName, "You control flame.", SpellId.FireRay);

            domainName = ModManager.RegisterFeatName("ChampionDomainTravel", "Travel");
            DomainTravel = domainName;
            yield return CreateChampionDomain(domainName, "You have power over movement and journeys.", SpellId.AgileFeet);

            domainName = ModManager.RegisterFeatName("ChampionDomainCold", "Cold");
            DomainCold = domainName;
            yield return CreateChampionDomain(domainName, "You control ice, snow, and freezing temperatures.", SpellId.WinterBolt);

            domainName = ModManager.RegisterFeatName("ChampionDomainLuck", "Luck");
            DomainLuck = domainName;
            yield return CreateChampionDomain(domainName, "You're unnaturally lucky and keep out of harm's way.", SpellId.BitOfLuck);

            domainName = ModManager.RegisterFeatName("ChampionDomainDestruction", "Destruction");
            DomainDestruction = domainName;
            yield return CreateChampionDomain(domainName, "You are a conduit for divine devastation.", SpellId.CryOfDestruction);

            domainName = ModManager.RegisterFeatName("ChampionDomainSun", "Sun");
            DomainSun = domainName;
            yield return CreateChampionDomain(domainName, "You harness the power of the sun and other light sources, and punish undead.", SpellId.DazzlingFlash);

            domainName = ModManager.RegisterFeatName("ChampionDomainDeath", "Death");
            DomainDeath = domainName;
            yield return CreateChampionDomain(domainName, "You have the power to end lives and destroy undead.", SpellId.DeathsCall);

            domainName = ModManager.RegisterFeatName("ChampionDomainHealing", "Healing");
            DomainHealing = domainName;
            yield return CreateChampionDomain(domainName, "Your healing magic is particularly potent.", SpellId.HealersBlessing);

            domainName = ModManager.RegisterFeatName("ChampionDomainMoon", "Moon");
            DomainMoon = domainName;
            yield return CreateChampionDomain(domainName, "You command powers associated with the moon.", SpellId.Moonbeam);

            domainName = ModManager.RegisterFeatName("ChampionDomainUndeath", "Undeath");
            DomainUndeath = domainName;
            yield return CreateChampionDomain(domainName, "Your magic carries close ties to the undead.", SpellId.TouchOfUndeath);

        }

        private static Feat CreateChampionDomain(FeatName domainName, string flavorText, SpellId grantedDomainSpell)
        {
            Spell spell = AllSpells.CreateModernSpell(grantedDomainSpell, null, 0, inCombat: false, new SpellInformation
            {
                ClassOfOrigin = ChampionTrait
            });
            return new Feat(domainName, flavorText, "You gain access to the {i}" + spell.Name.ToLower() + "{/i} domain focus spell.", new List<Trait> { ChampionDomainTrait, Trait.Homebrew }, null).WithRulesBlockForSpell(grantedDomainSpell).WithIllustration(spell.Illustration).WithOnSheet(delegate (CalculatedCharacterSheetValues sheet)
            {
                sheet.AddFocusSpellAndFocusPoint(ChampionTrait, Ability.Charisma, grantedDomainSpell);
            });
        }

        private static IEnumerable<Feat> LoadChampionDeities()
        {
            yield return new ChampionDeitySelectionFeat("Deity: The Oracle", "The Oracle is the most widely worshipped and revered deity on Our Point of Light, especially among the civilized folk. Well-aware that they are floating on a small insignificant plane through an endless void, the inhabitants of the plane put their faith in the Oracle, hoping that she would guide them and protect them from incomprehensible threats.\n\nAnd the Oracle, unlike most other deities, doesn't speak with her followers through signs or riddles. Instead, she maintains an open channel of verbal communication at her Grand Temple, though the temple's archclerics only rarely grant permission to ordinary folk to petition the Oracle.", "{b}• Edicts{/b} Care for each other, enjoy your life, protect civilization\n{b}• Anathema{/b} Travel into the Void, summon or ally yourself with demons", NineCornerAlignmentExtensions.All(), new FeatName[2]
            {
                    FeatName.HealingFont,
                    FeatName.HarmfulFont
            }, new FeatName[4]
            {
                    DomainHealing,
                    DomainTravel,
                    DomainLuck,
                    DomainSun
            }, ItemName.Morningstar, new SpellId[1] { SpellId.Invisibility });
            yield return new ChampionDeitySelectionFeat("Deity: The Blooming Flower", "The only deity to have granted any spells during the Time of the Broken Sun, the Blooming Flower is perhaps responsible for the survival of civilization on Our Point of Light. She was instrumental in allowing the civilized folk to beat back the undead and summon food to carry us though the long night. Ever since then, she has become associated with sun and light and is respected for her role in the survival of Our Point of Light even by her enemies.\n\nIn spring, during the Day of Blossoms, she makes all plants on the plane blossom at the same time, covering every field, meadow and forest in a beautiful spectacle of bright colors. This causes winter to end and ushers in a one-week celebration during which all combat is not just forbidden, but impossible.", "{b}• Edicts{/b} Grow yourself, protect and enjoy nature, destroy the undead\n{b}• Anathema{/b} Break the stillness of forests, steal food, desecrate ancient sites", NineCornerAlignmentExtensions.All().Except(new NineCornerAlignment[3]
            {
                    NineCornerAlignment.NeutralEvil,
                    NineCornerAlignment.ChaoticEvil,
                    NineCornerAlignment.LawfulEvil
            }).ToArray(), new FeatName[1] { FeatName.HealingFont }, new FeatName[4]
            {
                    DomainSun,
                    DomainMoon,
                    DomainLuck,
                    DomainHealing
            }, ItemName.Shortbow, new SpellId[1] { SpellId.ColorSpray });
            yield return new ChampionDeitySelectionFeat("Deity: The Thundering Tsunami", "A chaotic force of destruction, the Thundering Tsunami is best known for the Nights where Ocean Walks, unpredictable events that happen once in a generation when waves wash over our seaside settlements, and leave behind hundreds of destructive water elementals that wreak further havoc before they're killed or dry out and perish.\n\nDespite this destruction and despite being the most worshipped by evil water cults, the Thundering Tsunami is not evil herself and scholars believe that her destructive nights are a necessary component of the world's lifecycle, a release valve for pressure which would otherwise necessarily cause the plane itself to self-destruct.", "{b}• Edicts{/b} Build durable structures, walk at night, learn to swim\n{b}• Anathema{/b} Dive deep underwater, live on hills or inland, approach the edges of the world", NineCornerAlignmentExtensions.All().Except(new NineCornerAlignment[3]
            {
                    NineCornerAlignment.LawfulGood,
                    NineCornerAlignment.LawfulNeutral,
                    NineCornerAlignment.LawfulEvil
            }).ToArray(), new FeatName[2]
            {
                    FeatName.HealingFont,
                    FeatName.HarmfulFont
            }, new FeatName[4]
            {
                    DomainMoon,
                    DomainCold,
                    DomainTravel,
                    DomainDestruction
            }, ItemName.Warhammer, new SpellId[1] { SpellId.HideousLaughter });
            yield return new ChampionDeitySelectionFeat("Deity: The Unquenchable Inferno", "The Unquenchable Inferno is the eternal fire that burns within the plane itself. The Inferno never answers any {i}commune{/i} rituals or other divinations, but each time a fire elemental dies, it releases a memory. This could be a single sentence or an hour-long recitation, and depending on the age and power of the elemental, it could be trivial minutiae of the elemental's life or important discussions on the nature of the cosmos.\n\nThe Keeper-Monks of the Ring of Fire consider fire to be the living memory of this plane and are funding expeditions to capture, kill and listen to elder fire elementals everywhere so that more fundamental truths may be revealed and shared.", "{b}• Edicts{/b} be prepared, battle your enemies, learn Ignan\n{b}• Anathema{/b} burn books, gain fire immunity", NineCornerAlignmentExtensions.All(), new FeatName[2]
            {
                    FeatName.HealingFont,
                    FeatName.HarmfulFont
            }, new FeatName[4]
            {
                    DomainSun,
                    DomainFire,
                    DomainDestruction,
                    DomainDeath
            }, ItemName.Earthbreaker, new SpellId[1] { SpellId.BurningHands });
            yield return new ChampionDeitySelectionFeat("Deity: The Cerulean Sky", "Perhaps the most calm deity of them all, the Cerulean Sky manifests as the dome that shields us from the dangers of the Void during the day, but shows us the beauty of the Points of Light at night. It's the Cerulean Sky who connects leylines, draws water into clouds, and suffuses the land with both positive and negative energies, balancing the plane. She is the guardian of inanimate forces.\n\nBut perhaps because of her cold detachment and incomprehensible logic, over time she paradoxically became to be viewed more as the goddess of the night than of the daytime sky.", "{b}• Edicts{/b} Contemplate the sky, explore the world, fly\n{b}• Anathema{/b} Stay inside, create smoke, delve underground", NineCornerAlignmentExtensions.All(), new FeatName[2]
            {
                    FeatName.HealingFont,
                    FeatName.HarmfulFont
            }, new FeatName[4]
            {
                    DomainMoon,
                    DomainCold,
                    DomainTravel,
                    DomainUndeath
            }, ItemName.Falchion, new SpellId[1] { SpellId.ShockingGrasp });
        }

        private static List<Feat> DawnsburyChampionCauses()
        {
            var liberator = new ChampionCauseFeat("Liberator",
                "You're commited to defending the freedom of others.", "You gain the {b}Liberating Step{/b} champion's reaction and the " + "{b}{link:" + ChampionSpells.LayOnHandsId.ToString() + "} Lay on Hands{/link}{/b}" + " devotion spell.",
                new List<Trait>(), new List<Feat>()).WithOnSheet(sheet =>
                {
                    sheet.AddFocusSpellAndFocusPoint(ChampionTrait, Ability.Charisma, ChampionSpells.LayOnHandsId);
                    sheet.GrantFeat(DawnsburyChampionFeats.LiberatingStepFeatName);
                });

            var paladin = new ChampionCauseFeat("Paladin",
                "You're honorable, forthright, and committed to pushing back the forces of cruelty.", "You gain the {b}Retributive Strike{/b} champion's reaction and the " + "{b}{link:" + ChampionSpells.LayOnHandsId.ToString() + "} Lay on Hands{/link}{/b}" + " devotion spell.",
                new List<Trait>(), new List<Feat>()).WithOnSheet(sheet =>
                {
                    sheet.AddFocusSpellAndFocusPoint(ChampionTrait, Ability.Charisma, ChampionSpells.LayOnHandsId);
                    sheet.GrantFeat(DawnsburyChampionFeats.RetributiveStrikeFeatName);
                });

            var redeemer = new ChampionCauseFeat("Redeemer",
                "You're full of kindness and forgiveness.", "You gain the {b}Glimpse of Redemption{/b} champion's reaction and the " + "{b}{link:" + ChampionSpells.LayOnHandsId.ToString() + "} Lay on Hands{/link}{/b}" + " devotion spell.",
                new List<Trait>(), new List<Feat>()).WithOnSheet(sheet =>
                {
                    sheet.AddFocusSpellAndFocusPoint(ChampionTrait, Ability.Charisma, ChampionSpells.LayOnHandsId);
                    sheet.GrantFeat(DawnsburyChampionFeats.GlimpseOfRedemptionFeatName);
                });

            var causes = new List<Feat>() { liberator, paladin, redeemer };
            return causes;
        }

    }

}