﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using NC_Reactor_Planner.Properties;

namespace NC_Reactor_Planner
{
    public static class Palette
    {
        public static BlockTypes selectedType = BlockTypes.Air;
        public static ReactorGridCell selectedBlock;

        public static Dictionary<Block, BlockTypes> blocks;
        public static Dictionary<string, Block> blockPalette;
        //public static List<Block> miscBlocks = new List<Block>();
        public static List<Cooler> coolers;
        //public static List<Moderator> moderators = new List<Moderator>();
        public static Dictionary<string, Bitmap> textures;

        public static Point3D dummyPosition = new Point3D(-1, -1, -1);

        public static void Load()
        {
            if(textures == null)
                LoadTextures();
            LoadPalette();
        }

        private static void LoadTextures()
        {
            textures = new Dictionary<string, Bitmap>();
            textures.Add("Air", Resources.Air);

            textures.Add("Water", Resources.Water);
            textures.Add("Iron", Resources.Iron);
            textures.Add("Redstone", Resources.Redstone);
            textures.Add("Quartz", Resources.Quartz);
            textures.Add("Obsidian", Resources.Obsidian);
            textures.Add("Glowstone", Resources.Glowstone);
            textures.Add("Lapis", Resources.Lapis);
            textures.Add("Gold", Resources.Gold);
            textures.Add("Prismarine", Resources.Prismarine);
            textures.Add("Diamond", Resources.Diamond);
            textures.Add("Emerald", Resources.Emerald);
            textures.Add("Copper", Resources.Copper);
            textures.Add("Tin", Resources.Tin);
            textures.Add("Lead", Resources.Lead);
            textures.Add("Bronze", Resources.Bronze);
            textures.Add("Boron", Resources.Boron);
            textures.Add("Magnesium", Resources.Magnesium);
            textures.Add("Helium", Resources.Helium);
            textures.Add("Enderium", Resources.Enderium);
            textures.Add("Cryotheum", Resources.Cryotheum);

            textures.Add("Graphite", Resources.Graphite);
            textures.Add("Beryllium", Resources.Beryllium);

            textures.Add("FuelCell", Resources.FuelCell);

        }

        public static void LoadPalette(bool Active = false)
        {
            blocks = new Dictionary<Block, BlockTypes>();
            blockPalette = new Dictionary<string, Block>();
            coolers = new List<Cooler>();

            PopulateBlocks(Active);

            PopulateBlockPalette();

            ReloadValuesFromConfig();
        }

        public static void ReloadValuesFromConfig()
        {
            foreach (KeyValuePair<Block, BlockTypes> blockEntry in blocks)
                blockEntry.Key.ReloadValuesFromConfig();

            foreach (KeyValuePair<string, Block> blockEntry in blockPalette)
                blockEntry.Value.ReloadValuesFromConfig();
        }

        private static void PopulateBlockPalette()
        {
            //PopulateCoolers();

            blockPalette.Add("Air", new Block("Air", BlockTypes.Air, textures["Air"], dummyPosition));
            blockPalette.Add("FuelCell", new FuelCell("FuelCell", textures["FuelCell"], dummyPosition));

            foreach (Cooler cooler in coolers)
            {
                blockPalette.Add(cooler.DisplayName, cooler);
            }

            blockPalette.Add("Beryllium", new Moderator("Beryllium", ModeratorTypes.Beryllium, textures["Beryllium"], dummyPosition));
            blockPalette.Add("Graphite", new Moderator("Graphite", ModeratorTypes.Graphite, textures["Graphite"], dummyPosition));
        }

        private static void PopulateBlocks(bool Active = false)
        {
            PopulateCoolers(Active);

            blocks.Add(new Block("Air", BlockTypes.Air, textures["Air"], dummyPosition), BlockTypes.Air);
            blocks.Add(new FuelCell("FuelCell", textures["FuelCell"], dummyPosition), BlockTypes.FuelCell);

            foreach (Cooler cooler in coolers)
            {
                blocks.Add(cooler, BlockTypes.Cooler);
            }

            blocks.Add(new Moderator("Beryllium", ModeratorTypes.Beryllium, textures["Beryllium"], dummyPosition), BlockTypes.Moderator);
            blocks.Add(new Moderator("Graphite", ModeratorTypes.Graphite, textures["Graphite"], dummyPosition), BlockTypes.Moderator);
        }

        private static void PopulateCoolers(bool Active = false)
        {
            foreach (KeyValuePair<string, CoolerValues> coolerEntry in Configuration.Coolers)
            {
                CoolerValues cv = coolerEntry.Value;
                CoolerTypes parsedType;
                if (Enum.TryParse(coolerEntry.Key, out parsedType))
                    coolers.Add(new Cooler(coolerEntry.Key, textures[coolerEntry.Key], parsedType, cv.HeatActive, cv.HeatPassive, cv.Requirements, dummyPosition, Active));
                else
                    throw new ArgumentException("Unexpected cooler type in config!");
            }
        }

        public static Block BlockToPlace(Block previousBlock)
        {
            switch (selectedType)
            {
                case BlockTypes.Air:
                    return new Block("Air", BlockTypes.Air, textures["Air"], previousBlock.Position);
                case BlockTypes.Cooler:
                    return new Cooler((Cooler)selectedBlock.block, previousBlock.Position);
                case BlockTypes.Moderator:
                    return new Moderator((Moderator)selectedBlock.block, previousBlock.Position);
                case BlockTypes.FuelCell:
                    return new FuelCell((FuelCell)selectedBlock.block, previousBlock.Position);
                default:
                    return new Block("Air", BlockTypes.Air, textures["Air"], previousBlock.Position);
            }

        }

        public static bool PlacingSameBlock(Block block, MouseButtons placementMethod)
        {
            string blockToPlace = "Null";
            switch (placementMethod)
            {
                case MouseButtons.Left:
                    blockToPlace = selectedBlock.block.DisplayName;
                    break;
                case MouseButtons.None:
                    break;
                case MouseButtons.Right:
                    blockToPlace = "Air";
                    break;
                case MouseButtons.Middle:
                    blockToPlace = "FuelCell";
                    break;
                case MouseButtons.XButton1:
                    break;
                case MouseButtons.XButton2:
                    break;
                default:
                    break;
            }
            if (block.DisplayName == blockToPlace)
                if ((selectedBlock.block is Cooler selCooler) && block is Cooler placedCooler)
                {
                    if (selCooler.Active & placedCooler.Active)
                        return true;
                }
                else
                    return true;


            return false;
        }

        public static Cooler GetCooler(string displayName)
        {
            foreach (Cooler cooler in coolers)
            {
                if (cooler.DisplayName == displayName)
                    return cooler;
            }
            //return new Cooler(coolers.First(), dummyPosition);
            throw new ArgumentException("No such cooler! Looked for: " + displayName);
        }
    }

    public enum BlockTypes
    {
        Air,
        Cooler,
        Moderator,
        FuelCell,
        Casing,
    }
}
