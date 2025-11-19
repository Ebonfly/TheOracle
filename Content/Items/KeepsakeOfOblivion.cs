using TheOracle.Content.Buffs;
using TheOracle.Content.Projectiles;

namespace TheOracle.Content.Items;

public class KeepsakeOfOblivion : ModItem
{
    public override string Texture => "TheOracle/Assets/Images/Items/KeepsakeOfOblivion";

    public override void SetDefaults()
    {
        Item.DefaultToVanitypet(ModContent.ProjectileType<OracleP2Pet>(), ModContent.BuffType<OracleP2PetBuff>());
        Item.Size = new(16);
        Item.rare = ItemRarityID.Master;
        Item.master = true;
        Item.value = Item.sellPrice(0, 0, 5, 0);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        player.AddBuff(Item.buffType, 2);

        return false;
    }
}
