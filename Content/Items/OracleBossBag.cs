using TheOracle.Content.NPCs;

namespace TheOracle.Content.Items;

public class OracleBossBag : ModItem
{
    public override string Texture => "TheOracle/Assets/Images/Items/OracleBossBag";

    public override bool CanRightClick() => true;

    public override void SetStaticDefaults()
    {
        ItemID.Sets.BossBag[Type] = true;

        Item.ResearchUnlockCount = 3;
    }

    public override void SetDefaults()
    {
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.Size = new(24);
        Item.rare = ItemRarityID.Purple;
        Item.expert = true;
    }

    public override void ModifyItemLoot(ItemLoot itemLoot)
    {
        //itemLoot.Add(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<MinionBossMask>(), 7));
        itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<OracleBoss>()));
    }
}