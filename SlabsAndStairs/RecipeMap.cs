using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace SlabsAndStairs
{
    internal class RecipeMap
    {
        readonly Dictionary<string, RecipeSet> data = new();

        public void AddBlock(AssetLocation wildcard, Block block)
        {
            var set = AddMaterial(wildcard, block);
            set.block = block;
        }

        public void AddStairs(AssetLocation wildcard, Block stairs)
        {
            var set = AddMaterial(wildcard, stairs);
            set.stairs = stairs;
        }

        public void AddSlab(AssetLocation wildcard, Block slab)
        {
            var set = AddMaterial(wildcard, slab);
            set.slab = slab;
        }

        public IEnumerable<RecipeSet> Values { get => data.Values; }

        RecipeSet AddMaterial(AssetLocation wildcard, Block block)
        {
            var material = WildcardUtil.GetWildcardValue(wildcard, block.Code);

            if (!data.ContainsKey(material))
            {
                data.Add(material, new RecipeSet()
                {
                    material = material
                });
            }

            return data.Get(material);
        }
    }
}
