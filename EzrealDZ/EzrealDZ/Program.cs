using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace EzrealDZ
{
    class Program
    {
        public static string ChampName = "Ezreal";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static Items.Item DFG;

        public static Menu DZ;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q, 1150);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, float.MaxValue);

            Q.SetSkillshot(0.5f, 80f, 1200f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.5f, 80f, 1200f, false, SkillshotType.SkillshotLine);

            //Base menu
            DZ = new Menu("DZ" + ChampName, ChampName, true);

            //Orbwalker and menu
            DZ.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(DZ.SubMenu("Orbwalker"));

            //Target selector and menu
            var ts = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(ts);
            DZ.AddSubMenu(ts);

            //Combo menu
            DZ.AddSubMenu(new Menu("Combo", "Combo"));
            DZ.SubMenu("Combo").AddItem(new MenuItem("useQ", "Use Q?").SetValue(true));
            DZ.SubMenu("Combo").AddItem(new MenuItem("useW", "Use W?").SetValue(true));
            DZ.SubMenu("Combo").AddItem(new MenuItem("useE", "Use E?").SetValue(true));
            DZ.SubMenu("Combo").AddItem(new MenuItem("useR", "Use R?").SetValue(true));
            DZ.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Farming menu
            DZ.AddSubMenu(new Menu("Farming", "Farming"));
            DZ.SubMenu("Farming").AddItem(new MenuItem("useQF", "Use Q?").SetValue(true));
            DZ.SubMenu("Farming").AddItem(new MenuItem("FreezeActive", "Freeze lane").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //Exploits
            DZ.AddItem(new MenuItem("NFE", "No-Face Exploit").SetValue(true));

            //Make the menu visible
            DZ.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw; // Add onDraw
            Game.OnGameUpdate += Game_OnGameUpdate; // adds OnGameUpdate (Same as onTick in bol)

            Game.PrintChat(ChampName + " loaded!. Carry them son ;)");
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (DZ.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (DZ.Item("FreezeActive").GetValue<KeyBind>().Active)
            {
                Farm();
            }
        }

        static void Drawing_OnDraw(EventArgs args) // Spell range circles
        {
            Utility.DrawCircle(Player.Position, Q.Range, Color.Blue);
            Utility.DrawCircle(Player.Position, W.Range, Color.Blue);
            Utility.DrawCircle(Player.Position, E.Range, Color.Blue);
        }

        public static void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;
            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            var useQ = DZ.Item("useQF").GetValue<bool>();

            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion) * 1000 / 1200)) < DamageLib.getDmg(minion, DamageLib.SpellType.Q) - 10)
                    {
                        Q.Cast(minion, true);
                        return;
                    }
                }
            }
        }

        public static void Combo()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;

            if (target.IsValidTarget(Q.Range) && Q.IsReady())
            {
                Q.Cast(target, DZ.Item("NFE").GetValue<bool>());
            }

            if (target.IsValidTarget(W.Range) && W.IsReady())
            {
                W.Cast(target, DZ.Item("NFE").GetValue<bool>());
            }

            if (target.IsValidTarget(E.Range) && E.IsReady())
            {
                E.Cast(target, DZ.Item("NFE").GetValue<bool>());
            }

            if (target.IsValidTarget(R.Range) && R.IsReady())
            {
                R.Cast(target, DZ.Item("NFE").GetValue<bool>());
            }
        }
    }
}