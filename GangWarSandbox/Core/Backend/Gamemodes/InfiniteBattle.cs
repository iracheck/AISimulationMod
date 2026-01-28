using GangWarSandbox.Gamemodes;
using GangWarSandbox.Utilities;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

using LemonUI;
using LemonUI.Menus;

namespace GangWarSandbox.Gamemodes
{
    /// <summary>
    /// Infinite Battle Gamemode is the barebones gamemode-- nothing special happens, just peds constantly spawning and running toward eachother. Capture Points work but are only there for show --> no victory conditions.
    /// </summary>
    internal class InfiniteBattleGamemode : Gamemode
    {
        // Summary:
        // 
        public InfiniteBattleGamemode() : base("Infinite Battle", "Peds will spawn forever, putting you in a battle that never ends!", 4)
        { }

        private bool isRespawning = false;
        int deathTime;
        const int TIME_TO_WAIT_AFTER_DEATH = 1500; // in ms

        public override List<NativeMenu> ConstructGamemodeMenus()
        {
            var LoadMenu = new NativeMenu("Load Save", "LOAD SAVE");

            return new List<NativeMenu> { LoadMenu }; 
        }
        
        public override void OnTickGameRunning()
        {

            Game.Player.Character.InjuryHealthThreshold = 99;

            if (Game.Player.Character.IsInjured && !isRespawning)
            {
                isRespawning = true;
                deathTime = Game.GameTime;
                Vector3 spawn = Mod.Teams[0].SpawnPoints[0];

                GTA.UI.Screen.FadeOut(250);

                // make sure they dont ACTUALLY die
                Game.Player.Character.Health = 200;
                Game.Player.IsInvincible = true;

                Function.Call(Hash.IGNORE_NEXT_RESTART, true);
                Function.Call(Hash.FORCE_GAME_STATE_PLAYING);

                Function.Call(Hash.SET_PED_TO_RAGDOLL_WITH_FALL, Game.Player.Handle, 1500, 2000, 0, Game.Player.Character.ForwardVector.X, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);
                Function.Call(Hash.NETWORK_RESURRECT_LOCAL_PLAYER, spawn.X, spawn.Y, spawn.Z, 90f, false, false, false, false, false);

                Game.Player.CanControlCharacter = false;
            }

            if (isRespawning && Game.GameTime - TIME_TO_WAIT_AFTER_DEATH > deathTime)
            {
                isRespawning = false;
                deathTime = 0;

                Game.Player.CanControlCharacter = true;


                Game.Player.Character.Health = 200;
                Game.Player.IsInvincible = false;

                GTA.UI.Screen.FadeIn(800);
            }
        }

    }
}
