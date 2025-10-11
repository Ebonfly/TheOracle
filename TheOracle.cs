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
    public static TheOracle Instance;

    public TheOracle()
    {
        MusicSkipsVolumeRemap = true;
        Instance = this;
    }
}