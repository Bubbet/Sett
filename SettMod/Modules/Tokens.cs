﻿using R2API;

namespace SettMod.Modules
{
    internal static class Tokens
    {
        public const string bossSubtitle = "The Boss";
        public const string characterLore = "A leader of Ionia's growing criminal underworld, Sett rose to prominence in the wake of the war with Noxus. Though he began as a humble challenger in the fighting pits of Navori, he quickly gained notoriety for his savage strength, and his ability to take seemingly endless amounts of punishment. Now, having climbed through the ranks of local combatants, Sett has muscled to the top, reigning over the pits he once fought in.";
        public const string characterName = "<color=#ffa700>Sett</color>";
        public const string characterOutro = "..and so he left, with newfound might to honor.";
        public const string characterOutroFailure = "..and so he returned, infallible bastion truly immortalized.";
        public const string characterSubtitle = "The Boss";

        internal static void AddTokens()
        {
            LanguageAPI.Add("SETT_NAME", characterName);
            LanguageAPI.Add("SETT_DESCRIPTION", Modules.StaticValues.descriptionText);
            LanguageAPI.Add("SETT_SUBTITLE", characterSubtitle);
            LanguageAPI.Add("SETT_LORE", characterLore);
            LanguageAPI.Add("SETT_OUTRO_FLAVOR", characterOutro);
            LanguageAPI.Add("SETT_OUTRO_FAILURE", characterOutroFailure);
        }
    }
}