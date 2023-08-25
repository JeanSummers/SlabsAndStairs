using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;

namespace SlabsAndStairs
{

    internal class Config
    {
        readonly ConfigData data;
        public readonly string ModId;

        public Config(ICoreAPI api, string modid)
        {
            ModId = modid;

            try
            {
                data = api.LoadModConfig<ConfigData>($"{modid}.json");
            }
            catch (Exception e)
            {
                api.Logger.Error(e);
            }

            data ??= api.Assets.Get<ConfigData>(new(modid, "config/default-config.json"));
        }

        public List<AssetSet> GetAssetSets()
        {
            if (data == null)
            {
                return new();
            }

            var result = new List<AssetSet>();

            foreach (var recipe in data.recipes)
            {
                var isWildcard = AllHaveWildcards(recipe);
                if (isWildcard == null) continue;

                result.Add(new()
                {
                    isWildcard = (bool)isWildcard,
                    block = AssetOrNull(recipe.block),
                    slab = AssetOrNull(recipe.slab),
                    stairs = AssetOrNull(recipe.stairs)
                });
            }

            return result;
        }

        static bool? AllHaveWildcards(ConfigRecipe r)
        {
            // This is trippy o_o
            // Basically we filter combinations of conditions
            // all null -> skip
            // some true and some false -> skip
            // some true and any null -> true
            // some false and any null -> false

            var bl = r?.block?.Contains('*');
            var sl = r?.slab?.Contains('*');
            var st = r?.stairs?.Contains('*');

            int t = 0;
            if (bl == true) t++;
            if (sl == true) t++;
            if (st == true) t++;

            int f = 0;
            if (bl == false) f++;
            if (sl == false) f++;
            if (st == false) f++;

            if (f == t) return null;
            if (f == 0) return true;
            if (t == 0) return false;
            return null;
        }

        static AssetLocation AssetOrNull(string code) => code == null ? null : new AssetLocation(code);
    }
}
