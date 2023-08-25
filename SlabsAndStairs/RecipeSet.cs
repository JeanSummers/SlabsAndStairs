using Vintagestory.API.Common;

namespace SlabsAndStairs
{
    internal class RecipeSet
    {
        public string material;
        public Block block;
        public Block slab;
        public Block stairs;

        public AssetLocation RecipeName(Config config, Block from, Block to)
        {
            var materialString = material == null ? null : $"-{material}";
            return new(config.ModId, $"{Category(from)}-to-{Category(to)}{materialString}-{from.Id}-{to.Id}");
        }

        static string Category(Block block)
        {
            return block.Code.Path.Split('-')[0];
        }
    }
}
