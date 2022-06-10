using System;
using System.IO;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace AutoHostSave
{
    public class AutoHostSaveMod : Mod
    {
        private bool _loggedInviteCode;
        private ModConfig _modConfig;
        private int _wait;
        private bool _loaded;

        public override void Entry(IModHelper helper)
        {
            _modConfig = Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.Saved += OnSaved;

            _wait = _modConfig.TicksBeforeLoad;
            Helper.Events.GameLoop.UpdateTicked += WaitUntilReady;

            Helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
        }

        private void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Game1.options.enableServer || Game1.server == null || _loggedInviteCode ||
                Game1.server.getInviteCode() == null) return;

            Monitor.Log("Invite Code: " + Game1.server.getInviteCode(), LogLevel.Info);
            _loggedInviteCode = true;
        }

        private void WaitUntilReady(object sender, UpdateTickedEventArgs e)
        {
            if (_loaded && Game1.activeClickableMenu is not TitleMenu) return;

            if (_wait <= 0)
                LoadLastGameOrStartNewGame();
            else
                _wait -= 1;
        }

        private void OnSaved(object sender, SavedEventArgs e)
        {
            _modConfig.LastSave = Constants.SaveFolderName;
            Helper.WriteConfig(_modConfig);
        }

        private Vector2 GetDefaultSpawnPosition()
        {
            var bed = Utility.PointToVector2(((FarmHouse)Game1.player.currentLocation).getBedSpot()) * Game1.tileSize;
            bed.X -= Game1.tileSize;
            return bed;
        }

        private void InitializeCharacter()
        {
            Game1.player.Name = _modConfig.CharacterName;
            Game1.whichFarm = _modConfig.FarmType;
            Game1.player.farmName.Value = _modConfig.FarmName;
            Game1.player.favoriteThing.Value = _modConfig.FavoriteThing;
            Game1.player.displayName = Game1.player.Name;
            Game1.player.changeAccessory(_modConfig.Accessory);
            Game1.player.changeEyeColor(_modConfig.EyeColor);
            Game1.player.changeGender(_modConfig.Male);
            Game1.player.changeHairColor(_modConfig.HairColor);
            Game1.player.changeHairStyle(_modConfig.HairStyle);
            Game1.player.changePants(_modConfig.PantsColor);
            Game1.player.changeShirt(_modConfig.ShirtType);
            Game1.player.changeSkinColor(_modConfig.SkinColor);
            Game1.player.catPerson = _modConfig.CatPerson;
            Game1.player.whichPetBreed = _modConfig.PetBreed;
        }

        private void StartFirstDay()
        {
            Game1.startingCabins = _modConfig.Cabins;
            Game1.multiplayerMode = 2;
            Game1.loadForNewGame();
            Game1.player.Position = GetDefaultSpawnPosition();
            Game1.player.isInBed.Value = true;
            Game1.saveOnNewDay = true;

            // Speed up new day
            Game1.currentMinigame = null;
            Game1.newDay = false;
            Game1.newDaySync = new NewDaySynchronizer();
            Game1.nonWarpFade = false;

            Game1.player.currentEyes = 1;
            Game1.player.blinkTimer = 0;
            Game1.player.CanMove = true;
            Game1.pauseTime = 0.0f;

            if (Game1.activeClickableMenu == null || Game1.dialogueUp)
                return;
            Game1.activeClickableMenu.emergencyShutDown();
            Game1.exitActiveMenu();

            Game1.eventUp = false;
            Game1.eventOver = false;
            Game1.farmEvent = null;
            Game1.dayOfMonth = 0;
            Game1.setGameMode(3);
            Game1.NewDay(600f);
            if (Game1.currentLocation.currentEvent == null) return;
            Game1.currentLocation.currentEvent.cleanup();
            Game1.currentLocation.currentEvent = null;
        }

        private void LoadLastGameOrStartNewGame()
        {
            if (_modConfig.LastSave != null)
            {
                var file = _modConfig.LastSave;
                var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "StardewValley", "Saves", file);

                if (!Directory.Exists(savePath))
                {
                    Monitor.Log("Attempted to load a save that doesn't seem to exist. Loading a new game.",
                        LogLevel.Warn);
                    InitializeCharacter();
                    StartFirstDay();
                    _loaded = true;
                    return;
                }

                try
                {
                    Monitor.Log("Loading Save: " + file, LogLevel.Info);
                    SaveGame.Load(file);
                    if (Game1.activeClickableMenu is TitleMenu m) m.exitThisMenu(false);
                    _loaded = true;
                }
                catch (Exception ex)
                {
                    Monitor.Log("Load Failed", LogLevel.Error);
                    Monitor.Log(ex.Message);
                }
            }
            else
            {
                InitializeCharacter();
                StartFirstDay();
                _loaded = true;
            }
        }
    }
}