using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using Obeliskial_Essentials;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections;

namespace TraitMod
{
    [HarmonyPatch]
    internal class Traits
    {

        public static string[] myTraitList = { "shazixnarmasterofmeleeandranged", "shazixnarmasteroftrack" };

        public static void myDoTrait(string _trait, ref Trait __instance)
        {
            // get info you may need
            Enums.EventActivation _theEvent = Traverse.Create(__instance).Field("theEvent").GetValue<Enums.EventActivation>();
            Character _character = Traverse.Create(__instance).Field("character").GetValue<Character>();
            Character _target = Traverse.Create(__instance).Field("target").GetValue<Character>();
            int _auxInt = Traverse.Create(__instance).Field("auxInt").GetValue<int>();
            string _auxString = Traverse.Create(__instance).Field("auxString").GetValue<string>();
            CardData _castedCard = Traverse.Create(__instance).Field("castedCard").GetValue<CardData>();
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = new List<CardData>();
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            // activate traits
            if (_trait == "shazixnarmasterofmeleeandranged")
            {
                if (MatchManager.Instance != null && _castedCard != null)
                {
                    traitData = Globals.Instance.GetTraitData("shazixnarmasterofmeleeandranged");
                    if (MatchManager.Instance.activatedTraits != null && MatchManager.Instance.activatedTraits.ContainsKey("shazixnarmasterofmeleeandranged") && MatchManager.Instance.activatedTraits["shazixnarmasterofmeleeandranged"] > traitData.TimesPerTurn - 1)
                    {
                        return;
                    }
                    if ((_castedCard.GetCardTypes().Contains(Enums.CardType.Melee_Attack) || _castedCard.GetCardTypes().Contains(Enums.CardType.Ranged_Attack)) && _character.HeroData != null)
                    {
                        if (!MatchManager.Instance.activatedTraits.ContainsKey("shazixnarmasterofmeleeandranged"))
                        {
                            MatchManager.Instance.activatedTraits.Add("shazixnarmasterofmeleeandranged", 1);
                        }
                        else
                        {
                            Dictionary<string, int> activatedTraits = MatchManager.Instance.activatedTraits;
                            activatedTraits["shazixnarmasterofmeleeandranged"] = activatedTraits["shazixnarmasterofmeleeandranged"] + 1;
                        }
                        MatchManager.Instance.SetTraitInfoText();
                        // 如果是远程攻击，返还能量，上标记
                        if (_castedCard.GetCardTypes().Contains(Enums.CardType.Ranged_Attack))
                        {
                            _character.ModifyEnergy(1, true);
                            if (_character.HeroItem != null)
                            {
                                _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_Master of Melle and Ranged", "") + TextChargesLeft(MatchManager.Instance.activatedTraits["shazixnarmasterofmeleeandranged"], traitData.TimesPerTurn), Enums.CombatScrollEffectType.Trait);
                                EffectsManager.Instance.PlayEffectAC("energy", true, _character.HeroItem.CharImageT, false, 0f);
                            }
                            for (int i = 0; i < teamNpc.Length; i++)
                            {
                                if (teamNpc[i] != null && teamNpc[i].Alive)
                                {
                                    teamNpc[i].SetAuraTrait(_character, "mark", 2);
                                }
                            }
                            string text = MatchManager.Instance.CreateCardInDictionary("" , "", false);
                            CardData cardData = MatchManager.Instance.GetCardData(text);
                            while (true)
                            {
                                int randomIntRange = MatchManager.Instance.GetRandomIntRange(0, 100, "trait", "");
                                int randomIntRange2 = MatchManager.Instance.GetRandomIntRange(0, Globals.Instance.CardListByType[Enums.CardType.Melee_Attack].Count, "trait", "");
                                string id = Globals.Instance.CardListByType[Enums.CardType.Melee_Attack][randomIntRange2];
                                id = Functions.GetCardByRarity(randomIntRange, Globals.Instance.GetCardData(id, false), false);
                                text = MatchManager.Instance.CreateCardInDictionary(id, "", false);
                                cardData = MatchManager.Instance.GetCardData(text);
                                if (cardData.CardClass.ToString() != "Monster")
                                {
                                    break;
                                }
                            }
                            cardData.Vanish = true;
                            cardData.EnergyReductionPermanent = 3;
                            MatchManager.Instance.GenerateNewCard(1, text, false, Enums.CardPlace.Hand, null, null, -1, true, 0);
                            MatchManager.Instance.ItemTraitActivated(true);
                            MatchManager.Instance.CreateLogCardModification(cardData.InternalId, MatchManager.Instance.GetHero(_character.HeroIndex));
                            return;
                        }
                        // 如果是近战攻击，获得强效，抽1
                        else if (_castedCard.GetCardTypes().Contains(Enums.CardType.Melee_Attack))
                        {
                            string text = MatchManager.Instance.CreateCardInDictionary("" , "", false);
                            CardData cardData = MatchManager.Instance.GetCardData(text);
                            while (true)
                            {
                                int randomIntRange = MatchManager.Instance.GetRandomIntRange(0, 100, "trait", "");
                                int randomIntRange2 = MatchManager.Instance.GetRandomIntRange(0, Globals.Instance.CardListByType[Enums.CardType.Ranged_Attack].Count, "trait", "");
                                string id = Globals.Instance.CardListByType[Enums.CardType.Ranged_Attack][randomIntRange2];
                                id = Functions.GetCardByRarity(randomIntRange, Globals.Instance.GetCardData(id, false), false);
                                text = MatchManager.Instance.CreateCardInDictionary(id, "", false);
                                cardData = MatchManager.Instance.GetCardData(text);
                                if (cardData.CardClass.ToString() != "Monster")
                                {
                                    break;
                                }
                            }
                            cardData.Vanish = true;
                            cardData.EnergyReductionPermanent = 3;
                            MatchManager.Instance.GenerateNewCard(1, text, false, Enums.CardPlace.Hand, null, null, -1, true, 0);
                            MatchManager.Instance.ItemTraitActivated(true);
                            MatchManager.Instance.CreateLogCardModification(cardData.InternalId, MatchManager.Instance.GetHero(_character.HeroIndex));
                            return;
                        }
                    }
                }
            }
            else if (_trait == "shazixnarmasteroftrack")
            {
                if (MatchManager.Instance != null && _castedCard != null)
                {
                    traitData = Globals.Instance.GetTraitData("shazixnarmasteroftrack");
                    if (MatchManager.Instance.activatedTraits != null && MatchManager.Instance.activatedTraits.ContainsKey("shazixnarmasteroftrack") && MatchManager.Instance.activatedTraits["shazixnarmasteroftrack"] > traitData.TimesPerTurn - 1)
                    {
                        return;
                    }
                    if (_castedCard.GetCardTypes().Contains(Enums.CardType.Skill) && _character.HeroData != null)
                    {
                        if (!MatchManager.Instance.activatedTraits.ContainsKey("shazixnarmasteroftrack"))
                        {
                            MatchManager.Instance.activatedTraits.Add("shazixnarmasteroftrack", 1);
                        }
                        else
                        {
                            Dictionary<string, int> activatedTraits = MatchManager.Instance.activatedTraits;
                            activatedTraits["shazixnarmasteroftrack"] = activatedTraits["shazixnarmasteroftrack"] + 1;
                        }
                        MatchManager.Instance.SetTraitInfoText();
                        MatchManager.Instance.NewCard(1, Enums.CardFrom.Deck, "");
                        NPC[] teamNPC = MatchManager.Instance.GetTeamNPC();
                        for (int i = 0; i < teamNPC.Length; i++)
                        {
                            if (teamNPC[i] != null && teamNPC[i].Alive)
                            {
                                teamNPC[i].SetAuraTrait(_character, "sight", 2);
                            }
                        }
                        return;
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                myDoTrait(_trait, ref __instance);
                return false;
            }
            return true;
        }

        public static string TextChargesLeft(int currentCharges, int chargesTotal)
        {
            int cCharges = currentCharges;
            int cTotal = chargesTotal;
            return "<br><color=#FFF>" + cCharges.ToString() + "/" + cTotal.ToString() + "</color>";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            bool flag = false;
            bool flag2 = false;
            if (_characterCaster != null && _characterCaster.IsHero)
            {
                flag = _characterCaster.IsHero;
            }
            if (_characterTarget != null && _characterTarget.IsHero)
            {
                flag2 = true;
            }
            if (_acId == "sight")
            {
                if (_type == "set")
                {
                    if (!flag2)
                    {
                        if (__instance.TeamHaveTrait("shazixnarmasteroftrack"))
                        {
                            __result.AuraDamageIncreasedPercentPerStack = -0.2f;
                            __result.ResistModifiedPercentagePerStack = -0.2f;
                            __result.AuraDamageType = Enums.DamageType.All;
                            __result.ResistModified = Enums.DamageType.All;
                        }
                    }
                }
            }
            if (_acId == "mark")
            {
                if (_type == "set")
                {
                    if (!flag2)
                    {
                        if (__instance.TeamHaveTrait("shazixnarmarkofdeath"))
                        {
                            __result.ResistModifiedPercentagePerStack = -0.2f;
                            __result.ResistModified = Enums.DamageType.All;
                            __result.MaxMadnessCharges = 75;
                        }
                    }
                }
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "SetEvent")]
        public static void SetEventPrefix(ref Character __instance, ref Enums.EventActivation theEvent, Character target = null)
        {
            if (theEvent == Enums.EventActivation.Hitted
                    && !__instance.IsHero
                    && target.HaveTrait("shazixnarmercilesshunter")
                    && __instance.HasEffect("mark"))
            {
                // 有无情猎手天赋时，敌人带标记，被安德兰击中时承受标记层数伤害，流血增加倍率。
                float num = 2;
                num *= (1 + __instance.GetAuraCharges("bleed") * 0.01f);
                Enums.DamageType[] types = { Enums.DamageType.Slashing, Enums.DamageType.Piercing };
                Enums.DamageType damageType = types[UnityEngine.Random.Range(0, types.Length)];
                __instance.IndirectDamage(damageType, Functions.FuncRoundToInt((float)__instance.GetAuraCharges("mark") * num));
            }
        }
    }
}
