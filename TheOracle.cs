using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace TheOracle;
	
public class TheOracle : Mod
{
	public static Asset<Effect> TrailShader, FourPointGradient;
	public override void Load()
	{ 
		Asset<Effect> LoadEffect(string name) => ModContent.Request<Effect>("TheOracle/Assets/Effects/" + name, AssetRequestMode.AsyncLoad);
		TrailShader = LoadEffect("TrailShader");
		FourPointGradient = LoadEffect("FourPointGradient");
	}
}
