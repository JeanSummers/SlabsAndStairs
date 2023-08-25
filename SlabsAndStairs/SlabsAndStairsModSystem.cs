using System;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SlabsAndStairs
{
    public class SlabsAndStairsModSystem : ModSystem
    {
        Config config;
        ICoreServerAPI api;

        public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Server;

        public override double ExecuteOrder() => 1;

        public override void AssetsLoaded(ICoreAPI api)
        {
            this.api = (ICoreServerAPI)api;

            config = new(api, Mod.Info.ModID);

            var watch = System.Diagnostics.Stopwatch.StartNew();
            var count = api.World.GridRecipes.Count;
            api.Logger.Debug($"XXX1 {count}");

            CreateRecipes();

            watch.Stop();
            api.Logger.Debug($"XXX2 {api.World.GridRecipes.Count}(+{api.World.GridRecipes.Count - count}) {watch.ElapsedMilliseconds}ms");
        }

        void CreateRecipes()
        {
            foreach (var set in config.GetAssetSets())
            {
                if (set.isWildcard)
                {
                    CreateWildcardRecipe(set); 
                } else
                {
                    CreateSimpleRecipe(set);
                }
            }
        }

        void CreateWildcardRecipe(AssetSet asset)
        {
            var recipeMap = new RecipeMap();

            foreach (var block in FindBlocks(asset.block))
            {
                recipeMap.AddBlock(asset.block, block);
            }

            foreach (var slab in FindBlocks(asset.slab))
            {
                recipeMap.AddSlab(asset.slab, slab);
            }

            foreach (var stairs in FindBlocks(asset.stairs))
            {
                recipeMap.AddStairs(asset.stairs, stairs);
            }

            foreach (var set in recipeMap.Values)
            {
                CreateRecipeFromSet(set);
            }
        }

        static readonly Block[] empty = Array.Empty<Block>();

        Block[] FindBlocks(AssetLocation block)
        {
            if (block == null) return empty;
            var found = api.World.SearchBlocks(block);
            if (found == null || found.Length == 0) return empty;

            return found;
        }

        void CreateSimpleRecipe(AssetSet set)
        {
            CreateRecipeFromSet(new RecipeSet()
            {
                block = ResolveBlock(set.block),
                slab = ResolveBlock(set.slab),
                stairs = ResolveBlock(set.stairs),
            });
        }

        Block ResolveBlock(AssetLocation code) => code != null ? api.World.GetBlock(code) : null;

        void CreateRecipeFromSet(RecipeSet set)
        {
            var hasBlock = set.block != null;
            var hasSlab = set.slab != null;
            var hasStairs = set.stairs != null;

            if (hasBlock && hasSlab)
            {
                CreateRecipe("XX", set.block, set.slab, 4, set);
                CreateRecipe("X,X", set.slab, set.block, 1, set);
            }
            if (hasBlock && hasStairs)
            {
                CreateRecipe("_X,XX", set.block, set.stairs, 4, set);
                CreateRecipe("XX,XX", set.stairs, set.block, 3, set);
            }
            if (hasStairs && hasSlab)
            {
                CreateRecipe("_X,XX", set.slab, set.stairs, 2, set);
                CreateRecipe("XX", set.stairs, set.slab, 3, set);
            }
        }

        void CreateRecipe(string pattern, Block input, Block output, int amount, RecipeSet set)
        {
            var rows = pattern.Split(',');
            GridRecipe recipe = new()
            {
                Name = set.RecipeName(config, input, output),
                RecipeGroup = 143,
                IngredientPattern = pattern,
                Ingredients = new()
                {
                    ["X"] = new()
                    {
                        Type = EnumItemClass.Block,
                        Code = input.Code,
                    }
                },
                Width = rows[0].Length,
                Height = rows.Length,
                Output = new()
                {
                    Type = EnumItemClass.Block,
                    Code = output.Code,
                    Quantity = amount
                }
            };
            recipe.ResolveIngredients(api.World);
            api.RegisterCraftingRecipe(recipe);
        }

    }
}
