using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Humanizer;
using Dawnsbury.Modding;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Audio;
using Microsoft.Xna.Framework.Graphics;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using ChampionFocusSpells;
using Dawnsbury.Display.Illustrations;

namespace Dawnsbury.Mods.Classes.Champion
{
    public class DawnsburyChampionFeats
    {
        public static FeatName LiberatingStepFeatName;
        public static FeatName GlimpseOfRedemptionFeatName;
        public static FeatName AgileShieldGripFeatName;
        public static FeatName RetributiveStrikeFeatName;
        public static FeatName BladeAllyFeatName;
        public static FeatName ShieldAllyFeatName;
        public static FeatName DesperatePrayerFeatName;
        public static FeatName DivineGraceFeatName;
        public static FeatName DeitysDomainFeatName;
        public static FeatName DevotedGuardianFeatName;
        public static FeatName ResilientMindFeatName;
        public static FeatName SunBladeFeatName;
        public static FeatName AcceleratingTouchFeatName;
        public static FeatName AuraOfCourageFeatName;

        public static void LoadFeats()
        {
            ModManager.AddFeat(ChampionAgileShieldGrip());
            ModManager.AddFeat(DeitysDomain());
            ModManager.AddFeat(LiberatingStep());
            ModManager.AddFeat(GlimpseOfRedemption());
            ModManager.AddFeat(RetributiveStrike());
            ModManager.AddFeat(BladeAlly());
            ModManager.AddFeat(ShieldAlly());
            ModManager.AddFeat(DivineGrace());
            ModManager.AddFeat(DesperatePrayer());
            ModManager.AddFeat(DevotedGuardian());
            ModManager.AddFeat(ResilientMind());
            ModManager.AddFeat(SunBlade());
            ModManager.AddFeat(AcceleratingTouch());
            ModManager.AddFeat(AuraOfCourage());
        }
        public static Feat ChampionAgileShieldGrip()
        {
            String name = "Agile Shield Grip";
            int level = 1;
            String rulesText = "You can change your grip on the shield, lowering its damage die to a {b}d4{/b}.\n\nAs long as the weapon damage die is a d4, your Strikes with the shield gain the {b}agile{\b} weapon trait.\n\nYou can use Agile Shield Grip again to switch to a normal grip, returning the damage to the usual amount and removing the agile trait.";
            String flavorText = "You change your shield grip and perform rapid shield attacks.";
            AgileShieldGripFeatName = ModManager.RegisterFeatName("agileShieldGrip", name);
            Feat feat = new TrueFeat(AgileShieldGripFeatName, level, flavorText, rulesText, new[] { DawnsburyChampion.ChampionTrait, Trait.Fighter, Trait.Mod }, null)
                .WithActionCost(1).WithOnCreature((creature) =>
                {
                    creature.AddQEffect(new QEffect(ExpirationCondition.Never)
                    {
                        ProvideMainAction = (qfSelf) =>
                        {
                            CombatAction agileShieldGripAction = new CombatAction(creature, IllustrationName.ShieldSpell, "Agile Shield Grip", new Trait[] { }
                            , "Make shield strikes {b}Agile{/b}", Target.Self()).WithEffectOnSelf(creature =>
                            {
                                IReadOnlyList<QEffect> activeEffects = creature.QEffects;

                                QEffect agileGripEffect = activeEffects.FirstOrDefault(effect =>
                                {
                                    return effect.Name == "Agile Shield Grip";
                                });

                                if (!(agileGripEffect == null))
                                {
                                    agileGripEffect.ExpiresAt = ExpirationCondition.Immediately;
                                    creature.HeldItems.ForEach(heldItem =>
                                    {
                                        if (heldItem.HasTrait(Trait.Shield) && heldItem.HasTrait(Trait.Agile))
                                        {
                                            {
                                                heldItem.Traits.Remove(Trait.Agile);
                                            }
                                        }
                                    });
                                }
                                else
                                {

                                    creature.HeldItems.ForEach(heldItem =>
                                        {
                                            if (heldItem.HasTrait(Trait.Shield))
                                            {
                                                heldItem.Traits.Add(Trait.Agile);
                                            }
                                        });

                                    creature.AddQEffect(new QEffect("Agile Shield Grip", "Your strikes with a shield do {b}d4{/b} damage and are {b}agile.{/b}", ExpirationCondition.Never, creature, IllustrationName.ShieldSpell)
                                    {
                                        CountsAsABuff = true,
                                        YouDealDamageWithStrike = (QEffect qeffect, CombatAction attack, DiceFormula originalDiceFormula, Creature target) =>
                                        {
                                            Item bestHandwraps = StrikeRules.GetBestHandwraps(creature);
                                            Item weapon = attack.Item;
                                            List<Trait> weaponTraits = weapon.Traits;
                                            if (!weaponTraits.Contains(Trait.Shield))
                                            {
                                                return originalDiceFormula;
                                            }

                                            //Check for handwraps to determine number of damage dice
                                            string dieCount = originalDiceFormula.ToString().Substring(0, originalDiceFormula.ToString().IndexOf('d'));
                                            if (!(bestHandwraps == null) && bestHandwraps.WeaponProperties!.DamageDieCount == 2)
                                            {
                                                dieCount = "2";
                                            }

                                            string diceFormula = originalDiceFormula.Roll().Item2.Split(" ")[0];
                                            //string dieCount = diceFormula.Split("d")[0].Replace("{b}", "").Replace("{/b}", "");

                                            DiceFormula newFormula = DiceFormula.FromText(dieCount + "d4", weapon.Name);
                                            newFormula = newFormula.Add(DiceFormula.FromText(creature.Abilities.Strength.ToString(), "Strength"));
                                            return newFormula;
                                        }
                                    });
                                };
                            });
                            return new ActionPossibility(agileShieldGripAction);
                        }
                    }); ;
                }).WithOnCreature(delegate (CalculatedCharacterSheetValues sheet, Creature creature)
                {
                    Item bestHandwraps = StrikeRules.GetBestHandwraps(creature);
                    creature.AddQEffect(new QEffect("Agile Shield Grip: Handwraps Bonus", "Your shield strikes gain the benefits of your Handwraps of Mighty Blows.", ExpirationCondition.Never, creature)
                    {
                        YouDealDamageWithStrike = (QEffect qeffect, CombatAction attack, DiceFormula originalDiceFormula, Creature target) =>
                        {
                            Item weapon = attack.Item;
                            List<Trait> weaponTraits = weapon.Traits;
                            if (!weaponTraits.Contains(Trait.Shield))
                            {
                                return originalDiceFormula;
                            }

                            string diceFormula = originalDiceFormula.Roll().Item2.Split(" ")[0].Replace("{b}", "").Replace("{/b}", "");
                            string dieCount = diceFormula.Split("d")[0];
                            string dieDamage = diceFormula.Split("d")[1];

                            DiceFormula newFormula = DiceFormula.FromText(dieCount + "d" + dieDamage, weapon.Name);
                            newFormula = newFormula.Add(DiceFormula.FromText(creature.Abilities.Strength.ToString(), "Strength"));
                            return newFormula;
                        }
                    });
                });
            return feat;
        }

        public static Feat DeitysDomain()
        {
            DeitysDomainFeatName = ModManager.RegisterFeatName("deitysDomain", "Deity's Domain");
            Feat deitysDomain = new TrueFeat(DeitysDomainFeatName, 1, "You embody an aspect of your deity.", "Choose one of your deity's domains. You gain the domain's initial domain spell as a devotion spell.", new Trait[2] { DawnsburyChampion.ChampionTrait, Trait.Mod }).WithOnSheet(delegate (CalculatedCharacterSheetValues sheet)
            {
                CalculatedCharacterSheetValues sheet2 = sheet;

                sheet.AddSelectionOption(new SingleFeatSelectionOption("Domain", "Domain", -1, delegate (Feat ft)
                {
                    DeitySelectionFeat deity = sheet.Deity;

                    return (deity == null) ? (((ChampionDeitySelectionFeat)sheet.AllFeats.FirstOrDefault((Feat ft) => ft is ChampionDeitySelectionFeat))?.AllowedDomains.Contains(ft.FeatName) ?? false) : (deity?.AllowedDomains.Contains(ft.FeatName) ?? false);
                }));
            });

            return deitysDomain;
        }

        public static QEffect ChampionReactionApplied(Creature source)
        {
            QEffect qf = new QEffect("ChampionReactionApplied", "", ExpirationCondition.ExpiresAtEndOfSourcesTurn, source);
            return qf;
        }

        public static Feat LiberatingStep()
        {
            string name = "Liberating Step";
            int level = 1;
            string flavorText = "You free an ally from restraint.";
            string rulesText = "";

            LiberatingStepFeatName = ModManager.RegisterFeatName("liberatingStep", name);

            Feat feat = new TrueFeat(LiberatingStepFeatName, level, flavorText, rulesText, new Trait[0], null).WithActionCost(-2).WithOnCreature(delegate (CalculatedCharacterSheetValues sheet, Creature creature)
            {
                CombatAction LiberatingStepAction = new CombatAction(creature, IllustrationName.FleetStep, "Liberating Step {icon:Reaction}", new Trait[2] { DawnsburyChampion.ChampionFeatureTrait, Trait.Mod }, "You free an ally from restraint and protect them from harm.", Target.RangedFriend(3)).WithActionCost(-2);
            }).WithPermanentQEffect("You protect your allies from harm and free them from restraints.", qf =>
            {

            }).WithOnCreature((creature) =>
            {
                CombatAction LiberatingStepAction = new CombatAction(creature, IllustrationName.FleetStep, "Liberating Step", new Trait[0], "", Target.RangedFriend(3));
                Creature target = null;

                QEffect LiberatingStepEffect = new QEffect("LiberatingStep", " ", ExpirationCondition.Never, creature)
                {
                    YouAreDealtDamage = async delegate (QEffect qf, Creature attacker, DamageStuff damageStuff, Creature defender)
                    {
                        if (qf.Owner == defender)
                        {
                            return null;
                        }

                        else if (attacker == null || attacker.Occupies == null)
                        {
                            return null;
                        }

                        else if (qf.Source.DistanceTo(defender) > 3 || qf.Source.DistanceTo(attacker) > 3 || qf.Source == defender)
                        {
                            return null;
                        }

                        target = defender;
                        bool reactionUsed = await qf.Owner.Battle.AskToUseReaction(qf.Source, "{b}" + defender.Name + "{/b} is about to take damage.\nUse {b}" + qf.Source.Name + "'s Liberating Step?{/b}");

                        if (reactionUsed && !defender.QEffects.Any(effect => effect.Name == "ChampionReactionApplied"))

                        {
                            defender.Battle.Log("{b}" + qf.Source.Name + "{/b} used {b}Liberating Step{/b}.");
                            qf.Tag = true;

                            if (!defender.QEffects.Any(effect => effect.Name == "ChampionReactionApplied") && defender.HP > 0)
                            {
                                target.AddQEffect(ChampionReactionApplied(qf.Source));
                                return new ChampionsReactionModification(2 + qf.Source.Level, "Liberating Step", defender.WeaknessAndResistance.Resistances);
                            }
                        }

                        return null;
                    },
                    Tag = false,
                    AfterYouAreTargeted = async (QEffect qf, CombatAction action) =>
                    {
                        bool reactionUsed = (bool)qf.Tag;

                        if (reactionUsed)
                        {
                            await target.StrideAsync("Choose where to step", true, true, allowCancel: true);
                            qf.Tag = false;
                        }
                    }
                };

                //Add Liberating Step checker to all allies
                creature.AddQEffect(new QEffect(ExpirationCondition.Ephemeral)
                {
                    WhenExpires = qf =>
                    {
                        foreach (Creature ally in creature.Battle.AllCreatures.Where((Creature cr) => !cr.EnemyOf(creature)).ToList())
                        {
                            ally.AddQEffect(LiberatingStepEffect);
                        }
                    }
                });
            });

            return feat;
        }

        public static Feat GlimpseOfRedemption()
        {
            string name = "Glimpse of Redemption";
            int level = 1;
            string flavorText = "Your foe hesitates under the weight of sin as visions of redemption play in ther mind's eye.";
            string rulesText = "";

            GlimpseOfRedemptionFeatName = ModManager.RegisterFeatName("glimpseOfRedemption", name);

            Feat feat = new TrueFeat(GlimpseOfRedemptionFeatName, level, flavorText, rulesText, new Trait[2] { DawnsburyChampion.ChampionFeatureTrait, Trait.Mod }, null).WithActionCost(-2).WithOnCreature(delegate (CalculatedCharacterSheetValues sheet, Creature creature)
            {
                CombatAction GlimpseOfRedemptionAction = new CombatAction(creature, IllustrationName.SearingLight, "GlimpseOfRedemption", new Trait[] { }, "Your foe hesitates under the weight of sin as visions of redemption play in ther mind's eye.", Target.RangedFriend(3)).WithActionCost(-2);
            }).WithPermanentQEffect("Your foe hesitates under the weight of sin as visions of redemption play in ther mind's eye.", qf =>
            {

            }).WithOnCreature((creature) =>
            {
                CombatAction GlimpseOfRedemptionAction = new CombatAction(creature, IllustrationName.FleetStep, "Glimpse of Redemption", new Trait[0], "", Target.RangedFriend(3));
                Creature target = null;

                QEffect GlimpseOfRedemptionEffect = new QEffect("GlimpseOfRedemption", " ", ExpirationCondition.Never, creature)
                {
                    YouAreDealtDamage = async delegate (QEffect qf, Creature attacker, DamageStuff damageStuff, Creature defender)
                    {
                        if (qf.Owner == defender)
                        {
                            return null;
                        }

                        else if (attacker == null || attacker.Occupies == null)
                        {
                            return null;
                        }

                        else if (qf.Source.DistanceTo(defender) > 3 || qf.Source.DistanceTo(attacker) > 3 || qf.Source == defender)
                        {
                            return null;
                        }
                        target = defender;
                        bool reactionUsed = await qf.Owner.Battle.AskToUseReaction(qf.Source, "{b}" + defender.Name + "{/b} is about to take damage.\nUse {b}" + qf.Source.Name + "'s Glimpse of Redemption?{/b}");

                        if (reactionUsed)
                        {
                            QEffect enfeeble = QEffect.Enfeebled(2).WithExpirationAtEndOfOwnerTurn();
                            enfeeble.CannotExpireThisTurn = true;

                            attacker.AddQEffect(enfeeble);

                            if (!defender.QEffects.Any(effect => effect.Name == "ChampionReactionApplied"))
                            {
                                target.AddQEffect(ChampionReactionApplied(qf.Source));
                                return new ChampionsReactionModification(2 + qf.Source.Level, "Glimpse of Redemption", defender.WeaknessAndResistance.Resistances);
                            }
                        }

                        return null;
                    }
                };

                //Add Glimpse of Redemption checker to all allies
                creature.AddQEffect(new QEffect(ExpirationCondition.Ephemeral)
                {
                    WhenExpires = qf =>
                    {
                        foreach (Creature ally in creature.Battle.AllCreatures.Where((Creature cr) => !cr.EnemyOf(creature)).ToList())
                        {
                            ally.AddQEffect(GlimpseOfRedemptionEffect);
                        }
                    }
                });
            });

            return feat;
        }

        public static Feat RetributiveStrike()
        {
            string name = "Retributive Strike";
            int level = 1;
            string flavorText = "You protect your ally and strike your foe.";
            string rulesText = "";

            RetributiveStrikeFeatName = ModManager.RegisterFeatName("retributiveStrike", name);

            Feat feat = new TrueFeat(RetributiveStrikeFeatName, level, flavorText, rulesText, new Trait[2] { DawnsburyChampion.ChampionFeatureTrait, Trait.Mod }, null).WithActionCost(-2).WithOnCreature(delegate (CalculatedCharacterSheetValues sheet, Creature creature)
            {
                CombatAction RetributiveStrikeAction = new CombatAction(creature, IllustrationName.MagicWeapon, "Retributive Strike {icon:Reaction}", new Trait[] { }, "You protect your ally and strike your foe.", Target.RangedFriend(3)).WithActionCost(-2);
            }).WithPermanentQEffect("You protect your ally and strike your foe.", qf =>
            {

            }).WithOnCreature((creature) =>
            {
                Creature target = null;

                QEffect RetributiveStrikeEffect = new QEffect("RetributiveStrike", " ", ExpirationCondition.Never, creature, IllustrationName.AcidArrow)
                {
                    YouAreDealtDamage = async delegate (QEffect qf, Creature attacker, DamageStuff damageStuff, Creature defender)
                    {
                        if (qf.Owner == defender)
                        {
                            return null;
                        }

                        if (attacker == null || attacker.Occupies == null)
                        {
                            return null;
                        }

                        target = defender;

                        if (!attacker.EnemyOf(creature) || qf.Source.DistanceTo(defender) > 3 || qf.Source.DistanceTo(attacker) > 3 || qf.Source == defender)
                        {
                            return null;
                        }

                        bool reactionUsed = await qf.Owner.Battle.AskToUseReaction(qf.Source, "{b}" + defender.Name + "{/b} is about to take damage.\nUse {b}" + qf.Source.Name + "'s Retributive Strike?{/b}");

                        if (reactionUsed)
                        {
                            qf.Tag = true;
                            defender.Battle.Log("{b}" + qf.Source.Name + "{/b} used {b}Retributive Strike{/b}.");

                            if (!defender.QEffects.Any(effect => effect.Name == "ChampionReactionApplied"))
                            {
                                target.AddQEffect(ChampionReactionApplied(qf.Source));
                                return new ChampionsReactionModification(2 + qf.Source.Level, "Retributive Strike", defender.WeaknessAndResistance.Resistances);
                            }
                        }

                        return null;
                    },
                    AfterYouAreTargeted = async delegate (QEffect qf, CombatAction action)
                    {
                        Creature attacker = action.Owner;

                        if ((bool)qf.Tag == false)
                        {
                            return;
                        }

                        else if (creature.DistanceTo(attacker) <= 1 && creature.PrimaryWeapon.HasTrait(Trait.Melee) && attacker.EnemyOf(creature) && creature.HP > 0)
                        {
                            bool confirmStrike = await qf.Owner.Battle.AskForConfirmation(creature, IllustrationName.MagicWeapon, "Attack {b}" + attacker.Name + "{/b} with {b}Retributive Strike{/b}?", "Retributive Strike{icon:Reaction}");
                            await creature.MakeStrike(attacker, creature.PrimaryWeapon, 0);
                        }

                        qf.Tag = false;
                    },
                    Tag = false
                };

                //Add Retributive Strike checker to all allies
                creature.AddQEffect(new QEffect(ExpirationCondition.Ephemeral)
                {
                    WhenExpires = qf =>
                    {
                        foreach (Creature ally in creature.Battle.AllCreatures.Where((Creature cr) => !cr.EnemyOf(creature)).ToList())
                        {
                            ally.AddQEffect(RetributiveStrikeEffect);
                        }
                    }
                });
            });

            return feat;
        }

        public static Feat BladeAlly()
        {
            string name = "Blade Ally";
            int level = 3;
            string flavorText = "You gain a divine ally enhancing your Strikes.";
            string rulesText = "A spirit of battle dwells within your armaments. Your melee Strikes deal {b}1d6 Positive damage{/b} to undead. On a critical hit, undead are also {b}Enfeebled 1{/b} until the end of your next turn.";

            BladeAllyFeatName = ModManager.RegisterFeatName("bladeAlly", name);

            Feat feat = new TrueFeat(BladeAllyFeatName, level, flavorText, rulesText, new Trait[2] { DawnsburyChampion.ChampionFeatureTrait, Trait.Mod }, null).WithOnCreature(delegate (CalculatedCharacterSheetValues sheet, Creature creature) { });
            feat.WithOnCreature((creature) =>
            {
                creature.AddQEffect(new QEffect(ExpirationCondition.Never)
                {
                    AddExtraKindedDamageOnStrike = (CombatAction action, Creature target) =>
                    {
                        if (action.HasTrait(Trait.Melee) && target.HasTrait(Trait.Undead) && (action.CheckResult == CheckResult.Success || action.CheckResult == CheckResult.CriticalSuccess))
                        {
                            KindedDamage damage = new KindedDamage(DiceFormula.FromText("1d6", "Blade Ally"), DamageKind.Positive);
                            return damage;
                        }
                        return null;
                    },
                    AfterYouDealDamage = async (Creature creature, CombatAction action, Creature defender) =>
                    {
                        if (action.HasTrait(Trait.Melee) && action.HasTrait(Trait.Strike) && action.CheckResult == CheckResult.CriticalSuccess && defender.HasTrait(Trait.Undead))
                        {
                            QEffect enfeebleEffect = QEffect.Enfeebled(1).WithExpirationAtStartOfSourcesTurn(defender, 1);
                            enfeebleEffect.Source = defender;
                            enfeebleEffect.CannotExpireThisTurn = true;
                            defender.AddQEffect(enfeebleEffect);
                        }
                    }
                });
            }).WithPermanentQEffect("Your melee Strikes deal positive damage to undead.", qf =>
            {

            });

            return feat;
        }

        public static Feat ShieldAlly()
        {
            string name = "Shield Ally";
            int level = 3;
            string flavorText = "You gain a divine ally enhancing your shield.";
            string rulesText = "A spirit of protection dwells within your shield. In your hands, the shield's Hardness increases by 2.";

            ShieldAllyFeatName = ModManager.RegisterFeatName("shieldAlly", name);

            Feat feat = new TrueFeat(ShieldAllyFeatName, level, flavorText, rulesText, new Trait[2] { DawnsburyChampion.ChampionFeatureTrait, Trait.Mod }, null).WithOnCreature(delegate (CalculatedCharacterSheetValues sheet, Creature creature) { });
            feat.WithOnCreature((creature) =>
            {
                creature.AddQEffect(new QEffect(ExpirationCondition.Never)
                {
                    StateCheck = async delegate (QEffect qf)
                    {
                        List<Item> items = creature.HeldItems.Concat(creature.CarriedItems).ToList();
                        foreach (Item item in items)
                        {
                            if (item.HasTrait(Trait.Shield) && !item.Name.Contains(" (Shield Ally"))
                            {
                                item.Hardness = item.Hardness += 2;
                                item.Name = item.Name + " (Shield Ally)";
                            }
                        }
                    }
                });


                creature.AddQEffect(new QEffect(ExpirationCondition.Ephemeral)
                {
                    WhenExpires = qf =>
                    {
                        creature.Battle.Log(DivineGraceFeatName.ToString());

                        foreach (Creature ally in creature.Battle.AllCreatures.Where((Creature cr) => !cr.EnemyOf(creature)).ToList())
                        {
                            if (!ally.HasFeat(ShieldAllyFeatName))
                            {
                                QEffect shieldAllyRemover = new QEffect("shieldAllyRemover", "", ExpirationCondition.Never, ally);
                                shieldAllyRemover.StateCheck = async delegate (QEffect qf)
                                {
                                    List<Item> items = ally.HeldItems.Concat(ally.CarriedItems).ToList();
                                    foreach (Item item in items)
                                    {
                                        if (item.HasTrait(Trait.Shield) && item.Name.Contains(" (Shield Ally"))
                                        {
                                            item.Hardness = item.Hardness -= 2;
                                            item.Name = item.Name.Replace(" (Shield Ally)", "");
                                        }
                                    }
                                };

                                ally.AddQEffect(shieldAllyRemover);
                            }

                        }
                    }
                });

            }
            ).WithPermanentQEffect("Your shield gains +2 hardness.", qf =>
            {
            }
            );

            return feat;
        }

        public static Feat DevotedGuardian()
        {
            string name = "Devoted Guardian";
            int level = 2;
            string flavorText = "You adopt a wide stance, ready to defend both yourself and your chosen ward.";
            string rulesText = "{b}Requirements: {/b} Your shield is raised.\n{b}Frequency: {b} Once per round\nSelect one adjacent creature. As long as your shield is raised and the creature remains adjacent to you, the creature gains a {b}+1 circumstance bonus to their AC{/b}, or a +2 circumstance bonus if the shield you raised was a tower shield.";

            DevotedGuardianFeatName = ModManager.RegisterFeatName("devotedGuardian", name);
            Feat feat = new TrueFeat(DevotedGuardianFeatName, level, flavorText, rulesText, new Trait[2] { DawnsburyChampion.ChampionTrait, Trait.Mod }, null).WithActionCost(1).WithOnCreature(delegate (CalculatedCharacterSheetValues sheet, Creature creature) { });
            feat.WithOnCreature((creature) =>
            {
                creature.AddQEffect(new QEffect(ExpirationCondition.Never)
                {
                    StateCheck = async (QEffect qf) =>
                    {
                        if (creature.QEffects.Any(effect => effect.Id == QEffectId.RaisingAShield) && ((bool)qf.Tag == false))
                        {
                            creature.Battle.Log(creature.Actions.ActionHistoryThisTurn.LastOrDefault().ActionId.ToString());
                            QEffect devotedGuardianProvider = new QEffect(ExpirationCondition.Ephemeral);
                            CombatAction devotedGuardianAction = new CombatAction(creature, IllustrationName.Protection, "Devoted Guardian", new Trait[] { }, "You protect your chosen ward with your raised shield.", Target.AdjacentFriend()).WithNoSaveFor((action, cr) => true);
                            devotedGuardianAction.WithEffectOnEachTarget(async (action, caster, target, result) =>
                            {
                                qf.Tag = true;
                                List<Item> heldItems = caster.HeldItems.ToList();
                                int buffValue = 1;
                                if (heldItems.Any(item => item.ItemName == ItemName.TowerShield))
                                {
                                    buffValue = 2;
                                }
                                QEffect devotedGuardianBuff = new QEffect("Devoted Guardian", "", ExpirationCondition.Never, caster);
                                devotedGuardianBuff.CountsAsABuff = true;
                                devotedGuardianBuff.Illustration = IllustrationName.Protection;
                                devotedGuardianBuff.Description = "Protected by " + caster.Name + ". Gaining a {b} +" + buffValue.ToString() + " circumstance bonus to AC{/b} while within 5 feet of " + caster.Name + ".";
                                devotedGuardianBuff.CannotExpireThisTurn = true;
                                devotedGuardianBuff.StateCheck = async (QEffect qf) =>
                                {
                                    if (caster.DistanceTo(target) <= 1 && caster.QEffects.Any(effect => effect.Id == QEffectId.RaisingAShield))
                                    {
                                        target.AddQEffect(new QEffect(ExpirationCondition.Ephemeral)
                                        {
                                            BonusToDefenses = (QEffect _, CombatAction _, Defense defense) =>
                                            {
                                                if (defense == Defense.AC && caster.DistanceTo(target) <= 1 && caster.QEffects.Any(effect => effect.Id == QEffectId.RaisingAShield))
                                                {
                                                    return new Bonus(buffValue, BonusType.Circumstance, "Devoted Guardian");
                                                }
                                                return null;
                                            }
                                        });
                                    }

                                };
                                target.AddQEffect(devotedGuardianBuff);
                            }).WithSoundEffect(SfxName.ShieldSpell);

                            devotedGuardianProvider.ProvideMainAction = (qf) =>
                            {
                                return new ActionPossibility(devotedGuardianAction);
                            };

                            creature.AddQEffect(devotedGuardianProvider);

                        }
                    },
                    StartOfYourTurn = async (QEffect qf, Creature creature) =>
                    {
                        //Tag keeps track of whether Devoted Guardian was already used this turn
                        qf.Tag = false;
                    },
                    StartOfCombat = async (QEffect qf) =>
                    {
                        qf.Tag = false;
                    }
                });
            });

            return feat;
        }

        public static Feat DesperatePrayer()
        {
            string name = "Desperate Prayer";
            int level = 1;
            string flavorText = "You call out to your deity in a plea for their aid.";
            string rulesText = "You instantly recover 1 Focus Point.";

            DesperatePrayerFeatName = ModManager.RegisterFeatName("desperatePrayer", name);

            Feat feat = new TrueFeat(DesperatePrayerFeatName, level, flavorText, rulesText, new Trait[2] { DawnsburyChampion.ChampionTrait, Trait.Mod }, null).WithActionCost(0).WithOnCreature(delegate (CalculatedCharacterSheetValues sheet, Creature creature) { });
            feat.WithOnCreature((creature) =>
            {
                creature.AddQEffect(new QEffect(ExpirationCondition.Never)
                {
                    StartOfYourTurn = async delegate (QEffect qf, Creature creature)
                    {
                        if (creature.Spellcasting.FocusPoints == 0 && !(creature.PersistentUsedUpResources.UsedUpActions.Contains("desperatePrayer")))
                        {
                            {
                                bool desperatePrayer = await creature.Battle.AskForConfirmation(creature, IllustrationName.Heroism, creature.Name + " is out of focus points. Use " + "{b}Desperate Prayer{/b}" + " to recover one focus point?", "Desperate Prayer");
                                {
                                    creature.PersistentUsedUpResources.UsedUpActions.Add("desperatePrayer");
                                    Sfxs.Play(SfxName.Bless);
                                    creature.Battle.Log(creature.Name + "'s prayer is answered.");
                                    creature.Spellcasting.FocusPoints = 1;
                                }
                            }
                        }
                    }
                });
            }).WithPermanentQEffect("Call to your deity to recover 1 Focus Point.", qf =>
            {

            });

            return feat;
        }

        public static Feat DivineGrace()
        {
            string name = "Divine Grace";
            int level = 2;
            string flavorText = "You call upon your deity's grace.";
            string rulesText = "{b}Trigger:{/b} You attempt a save against a spell, before you roll.\nYou gain a +2 circumstance bonus to the save.";

            DivineGraceFeatName = ModManager.RegisterFeatName("divineGrace", name);

            Feat feat = new TrueFeat(DivineGraceFeatName, level, flavorText, rulesText, new Trait[2] { DawnsburyChampion.ChampionTrait, Trait.Mod }, null).WithActionCost(-2).WithOnCreature(delegate (CalculatedCharacterSheetValues sheet, Creature creature) { });
            feat.WithOnCreature((creature) =>
            {
                creature.AddQEffect(new QEffect(ExpirationCondition.Never)
                {
                    BeforeYourSavingThrow = async delegate (QEffect qf, CombatAction action, Creature creature)
                    {
                        if (action.SpellcastingSource != null)
                        {
                            creature.Battle.AskToUseReaction(creature, "You are about to roll a saving throw against a spell. Use Divine Grace?");
                            creature.AddQEffect(new QEffect(ExpirationCondition.Ephemeral)
                            {
                                BonusToDefenses = (QEffect qf, CombatAction action, Defense defense) => new Bonus(2, BonusType.Circumstance, "Divine Grace")
                            });
                        }
                    }
                });
            }).WithPermanentQEffect("Gain a {b}+2 circumstance bonus{/b} to your saving throw against a spell.", qf =>
            {

            });

            return feat;
        }

        public static Feat ResilientMind()
        {
            string name = "Resilient Mind";
            int level = 2;
            string flavorText = "You're firm in your convictions and have girded your mind against outside infuence.";
            string rulesText = "You gain a {b}+1 circumstance bonus{b} to saves against mental effects. This bonus increases to {b}+2{/b} against mental effects originating from undead.";

            ResilientMindFeatName = ModManager.RegisterFeatName("resilientMind", name);

            Feat feat = new TrueFeat(ResilientMindFeatName, level, flavorText, rulesText, new Trait[2] { DawnsburyChampion.ChampionTrait, Trait.Mod }, null).WithOnCreature(delegate (CalculatedCharacterSheetValues sheet, Creature creature) { });
            feat.WithPermanentQEffect("You gain a circumstance bonus to saves against mental effects.", qf =>
            {
                qf.BonusToDefenses = (QEffect _, CombatAction action, Defense defense) =>
                {
                    if (!(action == null) && action.HasTrait(Trait.Mental) && defense == Defense.AC)
                    {
                        int bonusValue = 1;
                        if (action.Owner.HasTrait(Trait.Undead))
                        {
                            bonusValue = 2;
                        }
                        return new Bonus(bonusValue, BonusType.Circumstance, "Resilient Mind", true);
                    }
                    return null;
                };
            });
            return feat;
        }

        public static Feat SunBlade()
        {
            string name = "Sun Blade";
            int level = 4;
            string flavorText = "You can unleash burning sunlight from your sword or spear.";
            string rulesText = "You gain the sun blade devotion spell. Increase the number of Focus Points in your focus pool by 1.";

            SunBladeFeatName = ModManager.RegisterFeatName("sunBlade", name);
            Feat feat = new TrueFeat(SunBladeFeatName, level, flavorText, rulesText, new Trait[2] { DawnsburyChampion.ChampionTrait, Trait.Mod }, null).WithOnCreature(delegate (CalculatedCharacterSheetValues sheet, Creature creature) { })
            .WithOnSheet(delegate (CalculatedCharacterSheetValues sheet)
            {
                sheet.AddFocusSpellAndFocusPoint(DawnsburyChampion.ChampionTrait, Ability.Charisma, ChampionSpells.SunBladeId);
            });

            return feat;
        }

        public static Feat AcceleratingTouch()
        {
            string name = "Accelerating Touch";
            int level = 4;
            string flavorText = "Your healing energies are infused with bounding energy.";
            string rulesText = "A creature that recovers Hit Points from your lay on hands gains a +10-foot status bonus to its Speed until the end of its next turn.";

            AcceleratingTouchFeatName = ModManager.RegisterFeatName("acceleratingTouch", name);
            Feat feat = new TrueFeat(AcceleratingTouchFeatName, level, flavorText, rulesText, new Trait[2] { DawnsburyChampion.ChampionTrait, Trait.Mod }, null).WithOnCreature(delegate (CalculatedCharacterSheetValues sheet, Creature creature) { });
            feat.WithPermanentQEffect("Creatures healed by your {b}Lay on Hands{/b} gain a {b}+10-foot status bonus to speed.{/b}", qf =>
            {
            });
            return feat;
        }

        public static Feat AuraOfCourage()
        {
            string name = "Aura of Courage";
            int level = 4;
            string flavorText = "You stand strong in the face of danger and inspire your allies to do the same.";
            string rulesText = "Whenever you become {b}frightened{/b}, reduce the condition value by 1 (to a minimum of 0). At the end of your turn when you would reduce your frightened condition value by 1, you also reduce the value by 1 for all allies within 15 feet.";

            AuraOfCourageFeatName = ModManager.RegisterFeatName("auraOfCourage", name);
            Feat feat = new TrueFeat(AuraOfCourageFeatName, 4, flavorText, rulesText, new Trait[2] { DawnsburyChampion.ChampionTrait, Trait.Mod });
            feat.WithOnCreature(delegate (Creature creature)
            {
                creature.AddQEffect(new QEffect("Aura of Courage", "Whenever you become {b}frightened{/b}, reduce the condition value by 1 (to a minimum of 0). At the end of your turn when you would reduce your frightened condition value by 1, you also reduce the value by 1 for all allies within 15 feet.", ExpirationCondition.Never, creature, (Illustration)IllustrationName.None)
                {
                    YouAcquireQEffect = delegate (QEffect self, QEffect newQEffect)
                    {
                        if (newQEffect.Id == QEffectId.Frightened)
                        {
                            if (self.Owner.HasEffect(QEffectId.DirgeOfDoomFrightenedSustainer))
                            {
                                newQEffect.Value = Math.Max(1, newQEffect.Value - 1);
                            }
                            else
                            {
                                newQEffect.Value--;
                            }

                            if (newQEffect.Value <= 0)
                            {
                                return null;
                            }
                        }

                        return newQEffect;
                    },
                    EndOfYourTurn = async delegate (QEffect qf, Creature creature)
                    {
                        foreach (Creature ally in creature.Battle.AllCreatures.Where((Creature cr) => !cr.EnemyOf(creature)).ToList())
                        {
                            if (ally.DistanceTo(creature) <= 3)
                            {
                                foreach (QEffect effect in ally.QEffects)
                                {
                                    if (effect.Id == QEffectId.Frightened)
                                    {
                                        if (effect.Id == QEffectId.Frightened)
                                        {
                                            effect.Value--;
                                        }

                                        if (effect.Value <= 0 && effect.Owner.HasEffect(QEffectId.DirgeOfDoomFrightenedSustainer))
                                        {
                                            effect.Value = 1;
                                        }

                                        else if (effect.Value <= 0)
                                        {
                                            effect.ExpiresAt = ExpirationCondition.Immediately;
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }).WithPermanentQEffect("You and your allies recover quicker from {b}frightened{/b}.", qf =>
            {

            });

            return feat;
        }
    }

    public class ChampionDeitySelectionFeat : Feat
    {
        public NineCornerAlignment[] AllowedAlignments { get; }

        public FeatName[] AllowedFonts { get; }

        public FeatName[] AllowedDomains { get; }

        public ItemName FavoredWeapon { get; }

        public ChampionDeitySelectionFeat(string deityName, string flavorText, string edictsAndAnathema, NineCornerAlignment[] allowedAlignments, FeatName[] allowedFonts, FeatName[] allowedDomains, ItemName favoredWeapon, SpellId[] extraSpells)
            : base(FeatName.CustomFeat, flavorText, ComposeDeityRulesText(edictsAndAnathema, allowedAlignments, allowedFonts, allowedDomains, favoredWeapon, extraSpells), new List<Trait> { }, null)
        {
            AllowedAlignments = allowedAlignments;
            AllowedFonts = allowedFonts;
            AllowedDomains = allowedDomains;
            FavoredWeapon = favoredWeapon;
            WithPrerequisite((CalculatedCharacterSheetValues values) => AllowedAlignments.Contains(values.NineCornerAlignment), "You don't meet the alignment restriction of this deity.");
            WithCustomName(deityName);
            WithOnSheet(delegate
            {
            });
        }

        private static string ComposeDeityRulesText(string edictsAndAnathema, NineCornerAlignment[] allowedAlignments, FeatName[] allowedFonts, FeatName[] allowedDomains, ItemName favoredWeapon, SpellId[] extraSpells)
        {
            List<Feat> domains = DawnsburyChampion.ChampionDomains;
            return edictsAndAnathema + "\n{b}• Allowed alignments{/b} " + Alignments.Describe(allowedAlignments) +
                "\n{b}• Allowed domains{/b} " + string.Join(", ", allowedDomains.Select((FeatName d) => domains.FirstOrDefault(item => item.FeatName == d)?.Name.ToLower())) +
                "\n{b}• Favored weapon{/b} " + EnumHumanizeExtensions.Humanize((Enum)favoredWeapon).ToLower();
        }
    }

    public class ChampionCauseFeat : Feat
    {
        public ChampionCauseFeat(string name, string flavorText, string rulesText, List<Trait> traits, List<Feat> subfeats) : base(FeatName.CustomFeat, flavorText, rulesText, traits, subfeats = null)
        {
            this.WithCustomName(name);
        }
    }
}