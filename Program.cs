#region

using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Humanizer
{
    public class Program
    {
        public static Menu Menu;
        public static float LastMove;
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static List<string> SpellList = new List<string> { "Q", "W", "E", "R" };
        public static List<float> LastCast = new List<float> { 0, 0, 0, 0 };

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("Humanizer", "Humanizer", true);

            var spells = Menu.AddSubMenu(new Menu("Spells", "Spells"));

            for (var i = 0; i <= 3; i++)
            {
                var spell = SpellList[i];
                var menu = spells.AddSubMenu(new Menu(spell, spell));
                menu.AddItem(new MenuItem("Enabled" + i, "Delay " + spell, true).SetValue(true));
                menu.AddItem(new MenuItem("MinDelay" + i, "Minimum Delay", true).SetValue(new Slider(80)));
                menu.AddItem(new MenuItem("MaxDelay" + i, "Maximum Delay", true).SetValue(new Slider(200, 100, 400)));
            }

            var move = Menu.AddSubMenu(new Menu("Movement", "Movement"));
            move.AddItem(new MenuItem("MovementEnabled", "Enabled").SetValue(true));
            move.AddItem(new MenuItem("MinDelay", "Minimum Delay")).SetValue(new Slider(80));
            move.AddItem(new MenuItem("MaxDelay", "Maximum Delay")).SetValue(new Slider(80));

            Menu.AddToMainMenu();

            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            var spell = (int) args.Slot;
            var senderValid = sender != null && sender.Owner != null && sender.Owner.IsMe;

            if (!senderValid || !args.Slot.IsMainSpell() || !Menu.Item("Enabled" + spell, true).IsActive())
            {
                return;
            }

            var min = Menu.Item("MinDelay" + spell, true).GetValue<Slider>().Value;
            var max = Menu.Item("MaxDelay" + spell, true).GetValue<Slider>().Value;
            var delay = min > max ? min : WeightedRandom.Next(min, max);

            if (Utils.TickCount - LastCast[spell] < delay)
            {
                args.Process = false;
                return;
            }

            LastCast[spell] = Utils.TickCount;
        }

        private static void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            var senderValid = sender != null && sender.IsValid && sender.IsMe;

            if (!senderValid || args.Order != GameObjectOrder.MoveTo || !Menu.Item("MovementEnabled").IsActive())
            {
                return;
            }
            var min = Menu.Item("MinDelay").GetValue<Slider>().Value;
            var max = Menu.Item("MaxDelay").GetValue<Slider>().Value;
            var delay = min > max ? min : WeightedRandom.Next(min, max);

            if (Utils.TickCount - LastMove < delay)
            {
                args.Process = false;
                return;
            }

            LastMove = Utils.TickCount;
        }
    }
}
