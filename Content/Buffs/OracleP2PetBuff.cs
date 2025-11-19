using TheOracle.Content.Projectiles;

namespace TheOracle.Content.Buffs;

public class OracleP2PetBuff : ModBuff
{
    public override string Texture => "TheOracle/Assets/Images/Buffs/OracleP2PetBuff";

    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.vanityPet[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        bool unused = false;
        player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused, ModContent.ProjectileType<OracleP2Pet>());
    }
}
