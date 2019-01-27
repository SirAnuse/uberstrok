﻿using System;
using System.Text;
using UberStrok.Core.Common;

namespace UberStrok.Core.Views
{
    [Serializable]
    public class LoadoutView
    {
        public LoadoutView()
        {
            Type = AvatarType.LutzRavinoff;
            SkinColor = "#FFFFFF";
        }

        public LoadoutView(int loadoutId, int backpack, int boots, int cmid, int face, int functionalItem1, int functionalItem2, int functionalItem3, int gloves, int head, int lowerBody, int meleeWeapon, int quickItem1, int quickItem2, int quickItem3, AvatarType type, int upperBody, int weapon1, int weapon1Mod1, int weapon1Mod2, int weapon1Mod3, int weapon2, int weapon2Mod1, int weapon2Mod2, int weapon2Mod3, int weapon3, int weapon3Mod1, int weapon3Mod2, int weapon3Mod3, int webbing, string skinColor)
        {
            Backpack = backpack;
            Boots = boots;
            Cmid = cmid;
            Face = face;
            FunctionalItem1 = functionalItem1;
            FunctionalItem2 = functionalItem2;
            FunctionalItem3 = functionalItem3;
            Gloves = gloves;
            Head = head;
            LoadoutId = loadoutId;
            LowerBody = lowerBody;
            MeleeWeapon = meleeWeapon;
            QuickItem1 = quickItem1;
            QuickItem2 = quickItem2;
            QuickItem3 = quickItem3;
            Type = type;
            UpperBody = upperBody;
            Weapon1 = weapon1;
            Weapon1Mod1 = weapon1Mod1;
            Weapon1Mod2 = weapon1Mod2;
            Weapon1Mod3 = weapon1Mod3;
            Weapon2 = weapon2;
            Weapon2Mod1 = weapon2Mod1;
            Weapon2Mod2 = weapon2Mod2;
            Weapon2Mod3 = weapon2Mod3;
            Weapon3 = weapon3;
            Weapon3Mod1 = weapon3Mod1;
            Weapon3Mod2 = weapon3Mod2;
            Weapon3Mod3 = weapon3Mod3;
            Webbing = webbing;
            if (skinColor[0] != '#')
                SkinColor = $"#{skinColor}";
            else
                SkinColor = skinColor;

        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("[LoadoutView: [Backpack: ");
            builder.Append(Backpack);
            builder.Append("][Boots: ");
            builder.Append(Boots);
            builder.Append("][Cmid: ");
            builder.Append(Cmid);
            builder.Append("][Face: ");
            builder.Append(Face);
            builder.Append("][FunctionalItem1: ");
            builder.Append(FunctionalItem1);
            builder.Append("][FunctionalItem2: ");
            builder.Append(FunctionalItem2);
            builder.Append("][FunctionalItem3: ");
            builder.Append(FunctionalItem3);
            builder.Append("][Gloves: ");
            builder.Append(Gloves);
            builder.Append("][Head: ");
            builder.Append(Head);
            builder.Append("][LoadoutId: ");
            builder.Append(LoadoutId);
            builder.Append("][LowerBody: ");
            builder.Append(LowerBody);
            builder.Append("][MeleeWeapon: ");
            builder.Append(MeleeWeapon);
            builder.Append("][QuickItem1: ");
            builder.Append(QuickItem1);
            builder.Append("][QuickItem2: ");
            builder.Append(QuickItem2);
            builder.Append("][QuickItem3: ");
            builder.Append(QuickItem3);
            builder.Append("][Type: ");
            builder.Append(Type);
            builder.Append("][UpperBody: ");
            builder.Append(UpperBody);
            builder.Append("][Weapon1: ");
            builder.Append(Weapon1);
            builder.Append("][Weapon1Mod1: ");
            builder.Append(Weapon1Mod1);
            builder.Append("][Weapon1Mod2: ");
            builder.Append(Weapon1Mod2);
            builder.Append("][Weapon1Mod3: ");
            builder.Append(Weapon1Mod3);
            builder.Append("][Weapon2: ");
            builder.Append(Weapon2);
            builder.Append("][Weapon2Mod1: ");
            builder.Append(Weapon2Mod1);
            builder.Append("][Weapon2Mod2: ");
            builder.Append(Weapon2Mod2);
            builder.Append("][Weapon2Mod3: ");
            builder.Append(Weapon2Mod3);
            builder.Append("][Weapon3: ");
            builder.Append(Weapon3);
            builder.Append("][Weapon3Mod1: ");
            builder.Append(Weapon3Mod1);
            builder.Append("][Weapon3Mod2: ");
            builder.Append(Weapon3Mod2);
            builder.Append("][Weapon3Mod3: ");
            builder.Append(Weapon3Mod3);
            builder.Append("][Webbing: ");
            builder.Append(Webbing);
            builder.Append("][SkinColor: ");
            builder.Append(SkinColor);
            builder.Append("]]");
            return builder.ToString();
        }

        public int Backpack { get; set; }
        public int Boots { get; set; }
        public int Cmid { get; set; }
        public int Face { get; set; }
        public int FunctionalItem1{ get; set; }
        public int FunctionalItem2 { get; set; }
        public int FunctionalItem3 { get; set; }
        public int Gloves { get; set; }
        public int Head { get; set; }
        public int LoadoutId { get; set; }
        public int LowerBody { get; set; }
        public int MeleeWeapon { get; set; }
        public int QuickItem1 { get; set; }
        public int QuickItem2 { get; set; }
        public int QuickItem3 { get; set; }
        public string SkinColor { get; set; }
        public AvatarType Type { get; set; }
        public int UpperBody { get; set; }
        public int Weapon1 { get; set; }
        public int Weapon1Mod1 { get; set; }
        public int Weapon1Mod2 { get; set; }
        public int Weapon1Mod3 { get; set; }
        public int Weapon2 { get; set; }
        public int Weapon2Mod1 { get; set; }
        public int Weapon2Mod2 { get; set; }
        public int Weapon2Mod3 { get; set; }
        public int Weapon3 { get; set; }
        public int Weapon3Mod1 { get; set; }
        public int Weapon3Mod2 { get; set; }
        public int Weapon3Mod3 { get; set; }
        public int Webbing { get; set; }
    }
}
