﻿#region dependencies
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;
#endregion

namespace kTalon2
{
    internal class Program
    {
        private const string Champion = "Talon";
        private static readonly List<Spell> Spellist = new List<Spell>();
        private static Spell _q, _w, _e, _r;
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Menu _config;
        private static Items.Item _tmt, _rah;
        private static SpellSlot _igniteSlot;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != Champion)
                return;
            #region Skillshots

            _igniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            _tmt = new Items.Item(3077, 400f); // tiamat
            _rah = new Items.Item(3074, 400f); // hydra

            _q = new Spell(SpellSlot.Q, 250f);
            _w = new Spell(SpellSlot.W, 600f);
            _e = new Spell(SpellSlot.E, 700f);
            _r = new Spell(SpellSlot.R, 500f);
            

            // fine tune of spells~


            _w.SetSkillshot(5f, 0f, 902f, false, Prediction.SkillshotType.SkillshotCone);
            _r.SetSkillshot(5f, 650f, 650f, false, Prediction.SkillshotType.SkillshotCircle);
            Spellist.AddRange(new[] { _q, _w, _e, _r });

            #endregion

            #region Menu
            // Menu 
            _config = new Menu(Player.ChampionName, Player.ChampionName, true);
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));


            // Combo
            
            // Harrass
            _config.AddSubMenu(new Menu("Harras", "harras"));
            _config.SubMenu("harras").AddItem(new MenuItem("QonPlayer", "Use Q").SetValue(true));
            _config.SubMenu("harras").AddItem(new MenuItem("WonPlayer", "Use W").SetValue(true));
            _config.SubMenu("harras").AddItem(new MenuItem("EonPlayer", "Use E").SetValue(false));

            // Lane Clear
            _config.AddSubMenu(new Menu("Lane Clear", "laneclear"));
            _config.SubMenu("laneclear").AddItem(new MenuItem("QonCreep", "use Q").SetValue(true));
            _config.SubMenu("laneclear").AddItem(new MenuItem("WonCreep", "use W").SetValue(true));

            // Last Hit
            _config.AddSubMenu(new Menu("Last H1t", "lasthit"));
            _config.SubMenu("lasthit").AddItem(new MenuItem("QkillCreep", "Use Q to FARM").SetValue(false));
            _config.SubMenu("lasthit").AddItem(new MenuItem("ManatoCreep", "> Mana Percent to Farm").SetValue(new Slider(30,0,100)));

            // KS
            _config.AddSubMenu(new Menu("KS", "ks"));

            // Drawning
            _config.AddSubMenu(new Menu("Drawning", "drawning"));

            _config.AddToMainMenu(); // add everything
            #endregion

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat("kTalon2 Loaded :}");
            
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Utility.DrawCircle(Player.Position, _w.Range, Color.Blue);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
            if (_orbwalker.ActiveMode.ToString() == "Combo")
            {
                
            }
            if (_orbwalker.ActiveMode.ToString() == "Mixed")
            {
                Mixed();
            }

            if (_orbwalker.ActiveMode.ToString() == "LaneClear")
            {
                Clear();
            }
            if (_orbwalker.ActiveMode.ToString() == "LastHit")
            {
                LastHit();
            }

        }
        #region LaneClear
        private static void Clear()
        {
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _w.Range, MinionTypes.All,
                MinionTeam.Enemy, MinionOrderTypes.MaxHealth); // not ideal at ALL need to make a MEC to calc mobs around to use W not only 1 target when have >1~

            if (mobs.Count > 0)
            {
                if (_config.SubMenu("laneclear").Item("WonCreep").GetValue<bool>() && _w.IsReady())
                    _w.Cast(mobs[0]);
                if (_config.SubMenu("laneclear").Item("QonCreep").GetValue<bool>() && _w.IsReady())
                    _q.Cast(mobs[0]);
            }
        }
        #endregion

        #region Harass
        private static void Mixed()
        {
            var target = SimpleTs.GetTarget(_w.Range, SimpleTs.DamageType.Physical);
            if (_config.SubMenu("harras").Item("WonPlayer").GetValue<bool>() && _w.IsReady())
            {
                _w.CastOnUnit(target, false);
            }
            if (_config.SubMenu("harras").Item("EonPlayer").GetValue<bool>() && _e.IsReady())
            {
                _e.Cast(target);
            }
            if (_config.SubMenu("harras").Item("QonPlayer").GetValue<bool>() && _q.IsReady())
            {
                _q.Cast(target);
            }
            if (_tmt.IsReady())
            {
                _tmt.Cast(target);
            }
            if (_rah.IsReady())
            {
                _rah.Cast(target);
            }

        }
        #endregion

        #region Last Hit

        private static void LastHit()
        {
            var mana = (Player.Mana / Player.MaxMana).ToString("N");
            var manatocast = _config.SubMenu("lasthit").Item("ManatoCreep").GetValue<Slider>().Value;
            Game.PrintChat(mana + " / " + manatocast); // getting values test ~
        }
        #endregion
    }
}
