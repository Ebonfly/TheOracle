using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TheOracle.Common.Utils;

namespace TheOracle.Content.NPCs;

public class OracleBoss : ModNPC
{
    public override string Texture => "TheOracle/Assets/Images/NPCs/OracleBoss";
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 2;
        NPCID.Sets.TrailCacheLength[Type] = 10;
        NPCID.Sets.TrailingMode[Type] = 3;
    }
    
    public override void SetDefaults()
    {
        NPC.Size = new Vector2(202, 304);
        NPC.lifeMax = 150000;
        NPC.defense = 50;
        NPC.aiStyle = -1;
        NPC.boss = true;
        NPC.noGravity = NPC.noTileCollide = NPC.lavaImmune = NPC.buffImmune[BuffID.Confused] = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.knockBackResist = 0f;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D tex = Assets.NPCs.OracleBoss.Value;
        Texture2D glow = Assets.NPCs.OracleBoss_Glow.Value;
        
        Texture2D crystalTex = Assets.NPCs.OracleCrystal.Value;
        Texture2D orbTex = Assets.NPCs.OracleOrbs.Value;

        Vector2 mainOrigin = new Vector2(202, 306) * 0.5f;
        Effect lightingEffect = TheOracle.FourPointGradient.Value;
        Vector4 LightingColor(Vector2 offset) => new Vector4(Lighting.GetSubLight(NPC.Center + offset.RotatedBy(NPC.rotation)), 1);
        lightingEffect.CurrentTechnique.Passes[0].Apply();
        lightingEffect.Parameters["colorTL"].SetValue(LightingColor(-mainOrigin));
        lightingEffect.Parameters["colorTR"].SetValue(LightingColor(new Vector2(mainOrigin.X, -mainOrigin.Y)));
        lightingEffect.Parameters["colorBL"].SetValue(LightingColor(new Vector2(-mainOrigin.X, mainOrigin.Y)));
        lightingEffect.Parameters["colorBR"].SetValue(mainOrigin);
        spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, mainOrigin, NPC.scale, SpriteEffects.None, 0);
        spriteBatch.Reload(effect: null);
        return false;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frame.Width = 202;
        NPC.frame.Height = 306;
    }

    public Vector2 CrystalPosition;
    public float CrystalRotation;

    private const int OrbAmount = 4;
    public float[] OrbRotation = new float[OrbAmount];
    public Vector2[] OrbPosition = new Vector2[OrbAmount];
    public ref float AIState => ref NPC.ai[0];
    public ref float AITimer => ref NPC.ai[1];
    public ref float AITimer2 => ref NPC.ai[2];
    public ref float AITimer3 => ref NPC.ai[3];
}