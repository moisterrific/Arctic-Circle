using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TShockAPI;
using TerrariaApi.Server;
using Terraria.Server;
using RUDD;
using RUDD.Dotnet;

using ArcticCircle;
using static ArcticCircle.Utils;

namespace ArcticCircle
{
    public class Delegates
    {
        public Delegates()
        {
            Instance = this;
        }
        public static Delegates Instance;
        public bool removeClass, canChoose = true;
        public bool[] hasChosenClass = new bool[256];
        public bool freeJoin;
        public bool kickOnSwitch;
        public bool kickOnLeave;
        public bool teamSpawn;
        public bool overflow;
        public bool autoAssignGroup;
        public Vector2[] teamSpawns = new Vector2[6];
        public static string[] Teams
        {
            get { return new string[] { "None", "Red Team", "Green Team", "Blue Team", "Yellow Team", "Pink Team" }; }
        }
        public string[] itemSet = new string[4];
        public int total;
        public static string[] Informal
        {
            get { return new string[] { "none", "red", "green", "blue", "yellow", "pink" }; }
        }
        public string redTeam = "red", greenTeam = "green", blueTeam = "blue", yellowTeam = "yellow", pinkTeam = "pink";
        public string[] Groups
        {
            get { return new string[] { "none", redTeam, greenTeam, blueTeam, yellowTeam, pinkTeam }; }
        }
        public const string Roster = "Roster";
        public const string Key = "names";
        public Block spawn;
        public Block setting;

        #region Classes
        public void ChooseClass(CommandArgs e)
        {
            bool canBypass = e.Player.HasPermission("classes.admin.bypass");

            if (!canChoose && !canBypass)
            {
                e.Player.SendErrorMessage("Class selection has currently been disabled.");
                return;
            }
            if (!TShockAPI.TShock.ServerSideCharacterConfig.Enabled)
            {
                e.Player.SendErrorMessage("SSC is not enabled, therefore class choosing is also not enabled.");
                return;
            }

            string classes = "";
            for (int i = 0; i < Utils.ClassID.Array.Length; i++)
            {
                classes += Utils.ClassID.Array[i] + ", ";
            }
            classes = classes.TrimEnd(new char[] { ',', ' ' });

            if (e.Message.Contains(" "))
            {
                string param = e.Message.Substring(e.Message.IndexOf(" ") + 1).ToLower().Trim(' ');
                if (/*Plugin.Instance.teamData.GetBlock(userName).GetValue("class") != "0"*/ hasChosenClass[e.Player.Index] && !canBypass)
                {
                    e.Player.SendErrorMessage("The character class designation has already occurred.");
                    return;
                }
                if (Utils.ClassSet(param) == -1)
                {
                    e.Player.SendErrorMessage("There is no such class. Try '/chooseclass [c/FFFF00:'" + classes.TrimEnd(' ') + "'] instead.");
                    return;
                }

                Utils.ResetPlayer(e.Player);
                int index = Utils.ClassSet(param);
                if (index < 0)
                {
                    e.Player.SendErrorMessage("No such class was found. Try '/chooseclass [c/FFFF00:'" + classes.TrimEnd(' ') + "'] instead.");
                    return;
                }
                if (itemSet[index].Length > 0)
                {
                    if (index >= 0)
                    {
                        string[] array = itemSet[index].Trim(' ').Split(',');
                        for (int j = 0; j < array.Length; j++)
                        {
                            #region Works | good formatting
                            /*
                            for (int n = 0; n < array[j].Length; n++)
                            {
                                if (array[j].Substring(n, 1) == "s")
                                {
                                    int.TryParse(array[j].Substring(n + 1), out type);
                                    int.TryParse(array[j].Substring(0, n), out stack);
                                    e.Player.GiveItem(type, stack);
                                    continue;
                                }
                                else if (array[j].Substring(n, 1) == "p")
                                {
                                    int.TryParse(array[j].Substring(n + 1), out type);
                                    int.TryParse(array[j].Substring(0, n), out prefix);
                                    e.Player.GiveItem(type, 1, prefix);
                                    continue;
                                }
                            }
                            int.TryParse(array[j], out type);
                            e.Player.GiveItem(type, 1);*/
                            #endregion
                            #region Tried & works | bad formatting
                            if (int.TryParse(array[j], out int type))
                            {
                                var data = Plugin.Instance.item_data;
                                var list = TShock.Utils.GetItemByIdOrName(type.ToString());
                                Block block = new Block()
                                {
                                    active = false
                                };
                                if (list.Count > 0)
                                {
                                    Item item = list[0];
                                    if (data.BlockExists(item.Name))
                                    {
                                        block = data.GetBlock(item.Name);
                                        block.active = true;
                                    }
                                }
                                int stack = j + 1;
                                if (stack < array.Length)
                                {
                                    if (array[stack].StartsWith("s"))
                                    {
                                        j++;
                                        if (int.TryParse(array[stack].Substring(1), out stack))
                                        {
                                            if (block.active)
                                            {
                                                ItemGet(new CommandArgs("giveitem", e.Player, new List<string>() { type.ToString(), stack.ToString(), 0.ToString() }));
                                            }
                                            else e.Player.GiveItem(type, stack);
                                            continue;
                                        }
                                        else
                                        {
                                            if (block.active)
                                            {
                                                ItemGet(new CommandArgs("giveitem", e.Player, new List<string>() { type.ToString(), 1.ToString(), 0.ToString() }));
                                            }
                                            e.Player.GiveItem(type, 1);
                                            continue;
                                        }
                                    }
                                }
                                int prefix = j + 1;
                                if (prefix < array.Length)
                                {
                                    if (array[prefix].StartsWith("p"))
                                    {
                                        j++;
                                        if (int.TryParse(array[prefix].Substring(1), out prefix))
                                        {
                                            if (block.active)
                                            {
                                                ItemGet(new CommandArgs("giveitem", e.Player, new List<string>() { type.ToString(), 1.ToString(), prefix.ToString() }));
                                            }
                                            else e.Player.GiveItem(type, 1, prefix);
                                            continue;
                                        }
                                        else
                                        {
                                            if (block.active)
                                            {
                                                ItemGet(new CommandArgs("giveitem", e.Player, new List<string>() { type.ToString(), 1.ToString(), 0.ToString() }));
                                            }
                                            else e.Player.GiveItem(type, 1);
                                            continue;
                                        }
                                    }
                                }
                                if (block.active)
                                {
                                    ItemGet(new CommandArgs("giveitem", e.Player, new List<string>() { type.ToString(), 1.ToString(), 0.ToString() }));
                                }
                                else e.Player.GiveItem(type, 1);
                            }
                            #endregion
                        }
                    }
                }
                hasChosenClass[e.Player.Index] = true;
                e.Player.SetBuff(BuffID.Webbed, 60, true);
                e.Player.SendSuccessMessage(Utils.ClassID.Array[Utils.ClassSet(param)] + " class chosen!");
                return;
            }
            e.Player.SendErrorMessage("Try '/chooseclass [c/FFFF00:'" + classes.TrimEnd(' ') + "'] instead.");
        }

        public void AddClass(CommandArgs e)
        {
            TSPlayer tsPlayer = e.Player;
            Player player = tsPlayer.TPlayer;
            if (e.Parameters.Count == 0)
            {
                tsPlayer.SendErrorMessage("Invalid syntax! Proper syntax: /addclass <class name>");
                return;
            }

            string className = e.Parameters[0];

            // Iterate through the player's inventory and make a list of all the items to add to the class.
            List<ClassItem> classItems = new List<ClassItem>();
            for (int i = 0; i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots; i++)
            {
                Item item = null;
                if (i < NetItem.InventorySlots) // Main inventory slots.
                {
                    item = player.inventory[i];
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots) // Armor and Accessory slots
                {
                    int index = i - NetItem.InventorySlots;
                    item = player.armor[index];
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots) // Dye Slots
                {
                    var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots);
                    item = player.dye[index];
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots) // Misc equip slots
                {
                    var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots);
                    item = player.miscEquips[index];
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots) // Misc dye slots
                {
                    var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots);
                    item = player.miscDyes[index];
                }

                if (item.netID == 0)
                {
                    continue;
                }

                ClassItem classItem = new ClassItem()
                {
                    id = item.netID,
                    stack = item.stack,
                    prefix = item.prefix
                };
                classItems.Add(classItem);
            }

            // Write to the class config file.
            string classItemsConfig = "";
            foreach (ClassItem classItem in classItems)
            {
                classItemsConfig += classItem.id + ",";
                if (classItem.stack > 1)
                {
                    classItemsConfig += string.Format("s{0},", classItem.stack);
                }
                else if (classItem.prefix > 0)
                {
                    classItemsConfig += string.Format("p{0},", classItem.prefix);
                }
            }
            classItemsConfig = classItemsConfig.TrimEnd(new char[] { ',' });

            Plugin.classINI.AddSetting(className, classItemsConfig);

            tsPlayer.SendSuccessMessage("The " + className + " class was successfully added! Please use /reload or restart the server to be able to use the class.");
        }
        #endregion

        public void Reload(CommandArgs e)
        {
            #region Team Set V2
            if (!Directory.Exists("config"))
                Directory.CreateDirectory("config");
 
            Ini ini = new Ini()
            {
                setting = new string[] { "playersperteam", "kickonswitch", "teamfreejoin", "kickonleave", "enableteamspawn", "teamoverflow", "autogroupassign", Informal[0], Informal[1], Informal[2], Informal[3], Informal[4], Informal[5] },
                path = "config\\team_data" + Ini.ext
            };
            total = 0;
            if (!File.Exists(ini.path))
                ini.WriteFile(new object[] { 4, false, false, false, false, false, false, "0:0", "0:0", "0:0", "0:0", "0:0", "0:0" });

            string t = string.Empty;
            string kick = string.Empty;
            string free = string.Empty;
            string leave = string.Empty;
            string tspawn = string.Empty;
            string overf = string.Empty;
            string auto = string.Empty;
            string  none = string.Empty,
                    red = string.Empty, 
                    green = string.Empty, 
                    blue = string.Empty, 
                    yellow = string.Empty, 
                    pink = string.Empty;
            var file = ini.ReadFile();
            if (file.Length > 0)
            {
                Ini.TryParse(file[0], out t);
                Ini.TryParse(file[1], out kick);
                Ini.TryParse(file[2], out free);
                Ini.TryParse(file[3], out leave);
                Ini.TryParse(file[4], out tspawn);
                Ini.TryParse(file[5], out overf);
                Ini.TryParse(file[6], out auto);
                Ini.TryParse(file[7], out none);
                Ini.TryParse(file[8], out red);
                Ini.TryParse(file[9], out green);
                Ini.TryParse(file[10], out blue);
                Ini.TryParse(file[11], out yellow);
                Ini.TryParse(file[12], out pink);
            }
            bool.TryParse(kick, out kickOnSwitch);
            int.TryParse(t, out total);
            bool.TryParse(free, out freeJoin);
            bool.TryParse(leave, out kickOnLeave);
            bool.TryParse(tspawn, out teamSpawn);
            bool.TryParse(overf, out overflow);
            bool.TryParse(auto, out autoAssignGroup);

            int.TryParse(none.Split(':')[0], out int noneX);
            int.TryParse(none.Split(':')[1], out int noneY);
            int.TryParse(red.Split(':')[0], out int redX);
            int.TryParse(red.Split(':')[1], out int redY);
            int.TryParse(green.Split(':')[0], out int greenX);
            int.TryParse(green.Split(':')[1], out int greenY);
            int.TryParse(blue.Split(':')[0], out int blueX);
            int.TryParse(blue.Split(':')[1], out int blueY);
            int.TryParse(yellow.Split(':')[0], out int yellowX);
            int.TryParse(yellow.Split(':')[1], out int yellowY);
            int.TryParse(pink.Split(':')[0], out int pinkX);
            int.TryParse(pink.Split(':')[1], out int pinkY);
            teamSpawns[0] = new Vector2(noneX, noneY);
            teamSpawns[1] = new Vector2(redX, redY);
            teamSpawns[2] = new Vector2(greenX, greenY);
            teamSpawns[3] = new Vector2(blueX, blueY);
            teamSpawns[4] = new Vector2(yellowX, yellowY);
            teamSpawns[5] = new Vector2(pinkX, pinkY);
            total = Math.Max(total, 2);
            string[] Slots = new string[total];
            for (int i = 0; i < total; i++)
                Slots[i] = "players" + (i + 1);
            
            foreach (string team in Teams)
            {
                if (!Plugin.Instance.teamData.BlockExists(team))
                    Plugin.Instance.teamData.NewBlock(Slots, team);
                else
                {
                    Block block;
                    if ((block = Plugin.Instance.teamData.GetBlock(team)).Contents.Length < total)
                    {
                        for (int i = 0; i < total; i++)
                        {
                            if (!block.Keys()[i].Contains(i.ToString()))
                                block.AddItem("players" + i, "0");
                        }
                    }
                }
            }
            string[] keys = Informal;
            if (!Plugin.Instance.teamData.BlockExists("groups"))
            {
                setting = Plugin.Instance.teamData.NewBlock(keys, "groups");
                for (int i = 0; i < Groups.Length; i++)
                {
                    setting.WriteValue(keys[i], Groups[i]);
                }
            }
            else
            {
                setting = Plugin.Instance.teamData.GetBlock("groups");
                for (int i = 0; i < Groups.Length; i++)
                {
                    setting.WriteValue(keys[i], Groups[i]);
                }
            }
            if (!Plugin.Instance.teamData.BlockExists("spawns"))
            {
                spawn = Plugin.Instance.teamData.NewBlock(keys, "spawns");
            }
            else 
            {
                spawn = Plugin.Instance.teamData.GetBlock("spawns");
            }
            e.Player.SendSuccessMessage("[TeamSet] settings reloaded.");
            #endregion

            #region Item Classes
            if (!File.Exists(Plugin.classINI.path))
            {
                Plugin.classINI.WriteFile(null);            
            }
            string[] array = Plugin.classINI.ReadFile();
            
            //string choose = "";
            //Plugin.classINI.TryParse(array[0], out choose);
            //bool.TryParse(choose, out canChoose);
            
            if (array.Length == 0)
                return;
            itemSet = new string[array.Length];
            Utils.ClassID.Array = new string[itemSet.Length];
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Contains('='))
                {
                    Ini.TryParse(array[i], out itemSet[i]);
                    Utils.ClassID.Array[i] = array[i].Substring(0, array[i].IndexOf('='));
                }
            }
            if (e.TPlayer == TShockAPI.TSPlayer.Server.TPlayer)
                Console.WriteLine("[PlayerClasses] Successfully reloaded the Plugin.classINI.");
            else e.Player.SendSuccessMessage("[c/FF0000:PlayerClasses] Successfully reloaded the Plugin.classINI.");
            #endregion
        }

        public void Start(CommandArgs e)
        {
            void error()
            {
                e.Player.SendErrorMessage("Try using [c/FFFF00:/match class <# of seconds>] to set a countdown for players to choose a class.");
            }
            if (e.Message.Contains(" "))
            {
                string sub = e.Message.Substring(e.Message.IndexOf(" ") + 1);
                if (sub.StartsWith("class") && sub.Contains(" "))
                {
                    int.TryParse(sub.Substring(sub.IndexOf(" ") + 1), out Hooks.ticks);
                    Hooks.ticks = Math.Max(Hooks.ticks, 60);
                    Hooks.preMatchChoose = true;
                    TShockAPI.TSPlayer.All.SendInfoMessage("You have [c/FF00FF:" + Hooks.ticks + " seconds] to choose a class before one is auto-assigned to you");
                }
                else
                {
                    error();
                }
            }
            else
            {
                error();
            }
        }
       
        public void ResetAll(CommandArgs e)
        {
            string list = " ";
            for (int i = 0; i < hasChosenClass.Length; i++)
            {
                hasChosenClass[i] = false;
            }
            foreach (TSPlayer p in TShock.Players)
            {
                if (p != null && p.Active)
                    list += p.Name + " ";
            }
            e.Player.SendSuccessMessage("The users:" + list + "have had their classes removed.");
        }

        public void ResetOption(CommandArgs e)
        {
            if (e.Parameters.Count < 1)
            {
                e.Player.SendErrorMessage("Try '/resetopt <user name>' instead.");
                return;
            }

            string userName = e.Parameters[0];
            TSPlayer player = Util.FindPlayer(userName);
            if (player == null)
            {
                e.Player.SendErrorMessage("The player was not found!");
                return;
            }
            hasChosenClass[Util.FindPlayer(userName).Index] = false;
            e.Player.SendSuccessMessage(player.Name + " has had their class removed.");
        }

        #region Team Set
        public void KickAll(CommandArgs e)
        {
            string list = " ";
            foreach (TSPlayer p in TShock.Players)
            {
                if (p != null && p.Active)
                {
                    Utils.SetTeam(p.Index, 0);
                    list += p.Name + " ";
                }
            }
            e.Player.SendSuccessMessage(string.Concat("Members [c/FFFF00:", list, "] ", "have been removed from their teams."));
        }
        public void TeleportTeam(CommandArgs e)
        {
            if (e.Message.Contains(" "))
            {
                string sub = e.Message.Substring(e.Message.IndexOf(" ") + 1);
                if (sub.StartsWith("all"))
                {
                    foreach (TSPlayer player in TShock.Players)
                    {
                        if (player != null && player.Active)
                        {
                            var v2 = teamSpawns[Utils.GetPlayerTeam(player.Name)];
                            player.Teleport(v2.X * 16, v2.Y * 16);
                            player.SendInfoMessage(e.Player.Name + " has teleported you to your team spawn.");
                        }
                    }
                    e.Player.SendSuccessMessage("All players have been teleported to their team spawns.");
                }
                else if (sub.StartsWith("team"))
                {
                    string team = sub.Substring(sub.IndexOf(" "));
                    int index = Utils.GetTeamIndex(team);
                    foreach (TSPlayer player in TShock.Players)
                    {
                        if (player != null && player.Active)
                        {
                            var v2 = teamSpawns[index];
                            player.Teleport(v2.X * 16, v2.Y * 16);
                            player.SendInfoMessage(e.Player.Name + " has teleported you to your team spawn.");
                        }
                    }
                    e.Player.SendSuccessMessage("Team " + team + " has been teleported to their team spawns.");
                }
                else
                {
                    string userName = sub;
                    int index = 0;
                    foreach (TSPlayer player in TShock.Players)
                    {
                        if (player != null && player.Active && player.Name == userName)
                        {
                            index = player.Team;
                            var v2 = teamSpawns[player.Team];
                            player.Teleport(v2.X * 16, v2.Y * 16);
                            player.SendInfoMessage(e.Player.Name + " has teleported you to your team spawn.");
                            break;
                        }
                    }
                    e.Player.SendSuccessMessage(userName + " has been teleported to team " + Teams[index] + "'s spawn.");
                }
            }
        }
        public void AutoSort(CommandArgs e)
        {
            if (e.Message.Contains(" ") && e.Message.Contains(","))
            {
                string[] user = Plugin.Instance.teamData.GetBlock(Roster).GetValue(Key).Split(';');
                
                int[] num = new int[Teams.Length];
                for (int k = 0; k < num.Length; k++)
                    num[k] = -1;

                string[] sub = e.Message.Substring(e.Message.IndexOf(" ") + 1).Split(',');
                for (int i = 0; i < sub.Length; i++)
                {
                    int index = int.Parse(sub[i]);
                    num[index] = Utils.TeamCount(index);
                }
                
                for (int n = 0; n < user.Length; n++)
                {
                    int index = 0;
                    int min = total;
                    for (int j = 0; j < num.Length; j++)
                    {
                        if (num[j] == -1)
                            continue;
                        if (num[j] < min)
                        {
                            min = num[j];
                            index = j;
                        }
                    }
                    TSPlayer player = null;
                    const int None = 0;
                    foreach (TSPlayer p in TShock.Players)
                    {
                        if (p != null && p.Active && p.Name == user[n] && p.Team == None)
                        {
                            player = p;
                            break;
                        }
                    }
                    if (player != null && index != 0)
                    {
                        num[index]++;
                        JoinTeam(new CommandArgs("jointeam " + Informal[index], player, null));
                        e.Player.SendInfoMessage(player.Name + " sent to " + Teams[index]);
                    }
                }
            }
        }
 
        public void MakeDataBase(CommandArgs e)
        {
            if (e.Message.Contains(" "))
            {
                string sub = e.Message.Substring(e.Message.IndexOf(" ") + 1);
                if (sub.StartsWith("reset"))
                {
                    Plugin.Instance.teamData.Dispose(false);
                    e.Player.SendSuccessMessage("The database has been cleared. Please run [c/FF0000:/database init <max # per team>.]");
                    return;
                }
                if (sub.StartsWith("init"))
                {
                    string[] Slots = new string[] {};
                    void num(int count)
                    {
                        total = Math.Max(count, 2);
                        Slots = new string[total];
                        for (int i = 0; i < total; i++)
                            Slots[i] = "players" + (i + 1);
                        e.Player.SendSuccessMessage("Max spots per team has been set to: [c/FFFF00: " + total + "].");
                    }
                    if (!sub.Contains(" "))
                    {
                        num(total);
                    }
                    else if (int.TryParse(sub.Substring(sub.IndexOf(" ") + 1), out int t))
                    {
                        num(t);
                    }
                    else
                    {
                        e.Player.SendErrorMessage("Specify total max players per team: [c/FFFF00:/database init <#>], or leave the # out which defaults to config data.");
                        return;
                    }
                    foreach (string team in Teams)
                    {
                        if (!Plugin.Instance.teamData.BlockExists(team))
                            Plugin.Instance.teamData.NewBlock(Slots, team);
                        else
                        {
                            Block block;
                            if ((block = Plugin.Instance.teamData.GetBlock(team)).Contents.Length < total)
                            {
                                for (int i = 0; i < total; i++)
                                {
                                    if (!block.Keys()[i].Contains(i.ToString()))
                                        block.AddItem("players" + i, "0");
                                }
                            }
                        }
                    }
                    string[] keys = Informal;
                    if (!Plugin.Instance.teamData.BlockExists("groups"))
                    {
                        setting = Plugin.Instance.teamData.NewBlock(keys, "groups");
                        for (int i = 0; i < Groups.Length; i++)
                        {
                            setting.WriteValue(keys[i], Groups[i]);
                        }
                    }
                    else
                    {
                        setting = Plugin.Instance.teamData.GetBlock("groups");
                        for (int i = 0; i < Groups.Length; i++)
                        {
                            setting.WriteValue(keys[i], Groups[i]);
                        }
                    }
                    if (!Plugin.Instance.teamData.BlockExists("spawns"))
                    {
                        spawn = Plugin.Instance.teamData.NewBlock(keys, "spawns");
                    }
                    else 
                    {
                        spawn = Plugin.Instance.teamData.GetBlock("spawns");
                    }
                    e.Player.SendSuccessMessage("Database initializing complete.");
                }
            }
        }
        public void TeamSpawn(CommandArgs e)
        {
            if (teamSpawn)
            {
                Utils.TeamTeleport(e.Player.Name, e.Player.Index);
            }
            else e.Player.SendInfoMessage("Team spawn points are disabled.");
        }
        
        public void SetSpawn(CommandArgs e)
        {
            Vector2 v2 = new Vector2((float)Math.Round(e.TPlayer.position.X, 0), (float)Math.Round(e.TPlayer.position.Y, 0));
            if (e.Message.Contains(" "))
            {
                string team = e.Message.Substring(e.Message.IndexOf(" ") + 1).ToLower();
                for (int i = 0; i < Informal.Length; i++)
                {
                    if (Informal[i] == team)
                    {
                        spawn.WriteValue(team, string.Concat(v2.X, "x", v2.Y));
                        break;
                    }
                    if (i == Informal.Length - 1)
                    {
                        e.Player.SendErrorMessage(string.Concat(team, " is not an existing team. Only the name of the color of the team is required."));
                        return;
                    }
                }
                e.Player.SendSuccessMessage(string.Format("{0} team spawn set at {1}X {2}Y.", team, v2.X, v2.Y));
            }
            else
            {
                e.Player.SendErrorMessage("The command format is /settspawn <team color>.");
            }
        }
        public void MakeGroups(CommandArgs e)
        {
            if (e.Message.ToLower().Contains("teamset"))
            {
                string cmd = e.Message.Substring(7);
                if (cmd.Contains("red"))
                {
                    redTeam = cmd.Substring(cmd.LastIndexOf(" ") + 1);
                    e.Player.SendSuccessMessage("Red's group: " + redTeam);
                }
                else if (cmd.Contains("green"))
                {
                    greenTeam = cmd.Substring(cmd.LastIndexOf(" ") + 1);
                    e.Player.SendSuccessMessage("Green's group: " + greenTeam);
                }
                else if (cmd.Contains("blue"))
                {
                    blueTeam = cmd.Substring(cmd.LastIndexOf(" ") + 1);
                    e.Player.SendSuccessMessage("Blue's group: " + blueTeam);
                }
                else if (cmd.Contains("yellow"))
                {
                    yellowTeam = cmd.Substring(cmd.LastIndexOf(" ") + 1);
                    e.Player.SendSuccessMessage("Yellow's group: " + yellowTeam);
                }
                else if (cmd.Contains("pink"))
                {
                    pinkTeam = cmd.Substring(cmd.LastIndexOf(" ") + 1);
                    e.Player.SendSuccessMessage("Pink's group: " + pinkTeam);
                }
                else
                {
                    e.Player.SendInfoMessage("/teamset [team color] [group name]");
                }
                for (int i = 1; i < Groups.Length; i++)
                    setting.WriteValue(Informal[i], Groups[i]);
                return;
            }
            var manage = TShock.Groups;
            if (manage.GroupExists("default"))
            {
                manage.GetGroupByName("default").SetPermission(new List<string>() { "teamset.join" });
                manage.GetGroupByName("default").ChatColor = "200,200,200";
                manage.GetGroupByName("default").Prefix = "[i:1] ";
            }
            if (!manage.GroupExists("team"))
            {
                manage.AddGroup("team", "default", "", "255,255,255");
                Console.WriteLine("The group 'team' has been made.");
            }
            for (int i = 1; i < Teams.Length; i++)
            {
                if (!TShock.Groups.GroupExists(Groups[i]))
                {
                    TShock.Groups.AddGroup(Groups[i], "team", "", "255,255,255");
                    switch (i)
                    {
                        case 1:
                            manage.GetGroupByName(Groups[i]).Prefix = "[i:1526] ";
                            manage.GetGroupByName(Groups[i]).ChatColor = "200,000,000";
                            break;
                        case 2:
                            manage.GetGroupByName(Groups[i]).Prefix = "[i:1525] ";
                            manage.GetGroupByName(Groups[i]).ChatColor = "000,200,050";
                            break;
                        case 3:
                            manage.GetGroupByName(Groups[i]).Prefix = "[i:1524] ";
                            manage.GetGroupByName(Groups[i]).ChatColor = "100,100,200";
                            break;
                        case 4:
                            manage.GetGroupByName(Groups[i]).Prefix = "[i:1523] ";
                            manage.GetGroupByName(Groups[i]).ChatColor = "200,150,000";
                            break;
                        case 5:
                            manage.GetGroupByName(Groups[i]).Prefix = "[i:1522] ";
                            manage.GetGroupByName(Groups[i]).ChatColor = "200,000,150";
                            break;
                    }
                    Console.WriteLine("The group '", Groups[i], "' has been made.");
                }
            }
            string msg;
            Console.WriteLine(msg = "The permissions, group colors, and chat prefixes have not been completely set up and will need to be done manually, though each team group has been parented to group 'team'.");
            e.Player.SendSuccessMessage(msg);
        }
        public void PlaceTeam(CommandArgs e)
        {
            string cmd;
            if (e.Message.ToLower().Contains("placeteam"))
            {
                if ((cmd = e.Message.ToLower()).Length > 9 && e.Message.Contains(" "))
                {
                    for (int i = 0; i < Main.player.Length; i++)
                    {
                        var player = Main.player[i];
                        if (player.active)
                        {
                            string name = player.name.ToLower();
                            if (cmd.ToLower().Contains(name))
                            {
                                string preserveCase = cmd.Substring(cmd.IndexOf(" ") + 1, name.Length);
                                string sub = cmd.Substring(cmd.IndexOf(" ") + 1, name.Length).ToLower();
                                if (sub == name)
                                {
                                    string team = cmd.Substring(cmd.LastIndexOf(" ") + 1).ToLower();
                                    int t = Utils.GetTeamIndex(team);
                                    if (t > 0 || int.TryParse(team, out t))
                                    {
                                        int get;
                                        if ((get = Utils.GetPlayerTeam(name)) == 0)
                                        {
                                            if (Utils.SetPlayerTeam(name, t))
                                            {
                                                e.Player.SendSuccessMessage(string.Concat(preserveCase, " is now on team ", Teams[t], "."));
                                                string set = Groups[t].ToLower();
                                                if (TShock.Groups.GroupExists(set) && autoAssignGroup)
                                                {
                                                    e.Player.Group = TShock.Groups.GetGroupByName(set);
                                                    Console.WriteLine(string.Concat(e.Player.Name, " has been set to group ", set, "!"));
                                                }
                                            }
                                            else
                                            {
                                                e.Player.SendErrorMessage(string.Concat(Teams[t], " might be already full."));
                                            }
                                        }
                                        else 
                                        {
                                            e.Player.SendErrorMessage(string.Concat(preserveCase, " is already on ", Teams[get], ". Using /removeteam [name] will remove the player from their team."));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (e.Message.Contains("removeteam"))
            {
                if ((cmd = e.Message).Length > 10 && e.Message.Contains(" "))
                {
                    string name;
                    if (Utils.RemoveFromTeam(name = cmd.Substring(cmd.IndexOf(" ") + 1)))
                    {
                        e.Player.SendSuccessMessage(string.Concat(name, " has been removed from their team."));
                        string set = "default";
                        if (TShock.Groups.GroupExists(set) && autoAssignGroup)
                        {
                            e.Player.Group = TShock.Groups.GetGroupByName(set);
                            Console.WriteLine(string.Concat(e.Player.Name, " has been set to group ", set, "!"));
                        }
                    }
                    else
                    {
                        e.Player.SendErrorMessage(string.Concat(name, " might not be on a team, or their is no player by this name."));
                    }
                }
                else
                {
                    e.Player.SendErrorMessage(string.Concat("Try /removeteam [name]."));
                }
            }
        }
        public void JoinTeam(CommandArgs e)
        {
            if (e.Message.StartsWith("team"))	
            {	
                Utils.SetTeam(e.Player.Index, Utils.GetPlayerTeam(e.Player.Name));	
                e.Player.SendErrorMessage("Your previous team designation has been kept. Use [c/FFFF00:/jointeam <color|index>] instead.");	
                return;	
            }
            bool success = false;
            for (int i = 0; i < Teams.Length; i++)
            {
                string t = Teams[i];
                string cmd = e.Message.Substring(e.Message.IndexOf(" ") + 1);
                int.TryParse(cmd, out int index);
                if (Utils.GetPlayerTeam(e.Player.Name) == 0 || freeJoin)
                {
                    if (t.ToLower().Contains(cmd.ToLower()))
                        success = Utils.SetPlayerTeam(e.Player.Name, Utils.GetTeamIndex(cmd));
                    else if (index != 0 && index == Utils.GetTeamIndex(t))
                    {
                        success = Utils.SetPlayerTeam(e.Player.Name, index);
                    }
                    if (success)
                    {
                        if (!e.Message.StartsWith("team"))
                            e.Player.SendSuccessMessage(string.Concat("Joining ", t, " has succeeded."));
                        string set = Groups[i];
                        if (TShock.Groups.GroupExists(set) && autoAssignGroup)
                        {
                            e.Player.Group = TShock.Groups.GetGroupByName(set);
                            Console.WriteLine(string.Concat(e.Player.Name, " has been set to group ", set, "!"));
                        }
                        return;
                    }
                }
            }
            e.Player.SendErrorMessage(string.Concat("Chances are you are already on a team or this team's roster is full."));
        }
        #endregion

        #region Item Tweak
        public static string[] Parameters = new string[] { "width", "height", "damage", "crit", "knockback", "prefix", "reusedelay", "shoot", "shootspeed", "useammo", "usetime", "autoreuse", "ammo", "scale" };
        public const int Width = 0, Height = 1, Damage = 2, Crit = 3, KB = 4, Prefix = 5, ReuseDelay = 6, Shoot = 7, ShootSpeed = 8, UseAmmo = 9, UseTime = 10, AutoReuse = 11, Ammo = 12, Scale = 13;
        public void ItemRestore(CommandArgs e)
        {
            var param = e.Parameters;
            if (param.Count == 0)
            {
                e.Player.SendErrorMessage("There was a parameter error. Try: [c/FFFF00:/resetitem <item name | ID>] to restore default values.");
                return;
            }
            var list = TShock.Utils.GetItemByIdOrName(param[0]);
            var data = Plugin.Instance.item_data;
            if (list.Count == 0)
            {
                e.Player.SendErrorMessage("Item " + param[0] + " not found.");
                return;
            }
            Item item = list[0];

            if (data.BlockExists(item.Name))
            {
                Block block = data.GetBlock(item.Name);

                block.WriteValue(Parameters[Width].TrimEnd(':', '0'), item.width.ToString());
                block.WriteValue(Parameters[Height].TrimEnd(':', '0'), item.height.ToString());
                block.WriteValue(Parameters[Damage].TrimEnd(':', '0'), item.damage.ToString());
                block.WriteValue(Parameters[Crit].TrimEnd(':', '0'), item.crit.ToString());
                block.WriteValue(Parameters[KB].TrimEnd(':', '0'), item.knockBack.ToString());
                block.WriteValue(Parameters[Prefix].TrimEnd(':', '0'), item.prefix.ToString());
                block.WriteValue(Parameters[ReuseDelay].TrimEnd(':', '0'), item.reuseDelay.ToString());
                block.WriteValue(Parameters[Shoot].TrimEnd(':', '0'), item.shoot.ToString());
                block.WriteValue(Parameters[ShootSpeed].TrimEnd(':', '0'), item.shootSpeed.ToString());
                block.WriteValue(Parameters[UseAmmo].TrimEnd(':', '0'), item.useAmmo.ToString());
                block.WriteValue(Parameters[UseTime].TrimEnd(':', '0'), item.useTime.ToString());
                block.WriteValue(Parameters[AutoReuse].TrimEnd(':', '0'), item.autoReuse.ToString());
                block.WriteValue(Parameters[Ammo].TrimEnd(':', '0'), item.ammo.ToString());
                block.WriteValue(Parameters[Scale].TrimEnd(':', '0'), item.scale.ToString());
                  
                e.Player.SendSuccessMessage(item.Name + " has been restored to default values.");
                return;
            }
            else
            {
                e.Player.SendErrorMessage(item.Name + " is not stored. Use [c/FFFF00:/tweak <item name | ID>] to add it.");
            }
        }
        public void ItemTweak(CommandArgs e)
        {
            var param = e.Parameters;
            string List = " ";
                for (int i = 0; i < Parameters.Length; i++)
                    List += Parameters[i] + ", ";
            
            if (param.Count == 0)
            {
                e.Player.SendErrorMessage("There was a parameter error. Try: [c/FFFF00:/tweak <item name | ID>] to store it.");
                return;
            }

            var list = TShock.Utils.GetItemByIdOrName(param[0]);
            var data = Plugin.Instance.item_data;
            if (list.Count == 0)
            {
                e.Player.SendErrorMessage("Item " + param[0] + " not found.");
                return;
            }
            Item item = list[0];
            
            Block block;
            if (!data.BlockExists(item.Name))
            {
                block = data.NewBlock(Parameters, item.Name);

                block.WriteValue(Parameters[Width].TrimEnd(':', '0'), item.width.ToString());
                block.WriteValue(Parameters[Height].TrimEnd(':', '0'), item.height.ToString());
                block.WriteValue(Parameters[Damage].TrimEnd(':', '0'), item.damage.ToString());
                block.WriteValue(Parameters[Crit].TrimEnd(':', '0'), item.crit.ToString());
                block.WriteValue(Parameters[KB].TrimEnd(':', '0'), item.knockBack.ToString());
                block.WriteValue(Parameters[Prefix].TrimEnd(':', '0'), item.prefix.ToString());
                block.WriteValue(Parameters[ReuseDelay].TrimEnd(':', '0'), item.reuseDelay.ToString());
                block.WriteValue(Parameters[Shoot].TrimEnd(':', '0'), item.shoot.ToString());
                block.WriteValue(Parameters[ShootSpeed].TrimEnd(':', '0'), item.shootSpeed.ToString());
                block.WriteValue(Parameters[UseAmmo].TrimEnd(':', '0'), item.useAmmo.ToString());
                block.WriteValue(Parameters[UseTime].TrimEnd(':', '0'), item.useTime.ToString());
                block.WriteValue(Parameters[AutoReuse].TrimEnd(':', '0'), item.autoReuse.ToString());
                block.WriteValue(Parameters[Ammo].TrimEnd(':', '0'), item.ammo.ToString());
                block.WriteValue(Parameters[Scale].TrimEnd(':', '0'), item.scale.ToString());
                  
                e.Player.SendSuccessMessage(item.Name + " has been stored. Use [c/FFFF00:/tweak] again to change its attributes.");
                return;
            }

            if (param.Count < 3)
            {
                e.Player.SendErrorMessage("There was a parameter error. Try: [c/FFFF00:/tweak <item name | ID> <parameter> <#>]\n" +
                        "Available parameters:[c/FFFF00:" + List.Replace(":", "").Replace("0", "").TrimEnd(',', ' ') +  "].");
                return;
            }
            
            if (!List.Contains(param[1].ToLower()))
            {
                e.Player.SendErrorMessage(param[1] + " was an invalid parameter.\n" +
                        "Available parameters:[c/FFFF00:" + List.Replace(":", "").Replace("0", "").TrimEnd(',', ' ') +  "].");
                return;
            }
            
            block = data.GetBlock(item.Name);
            block.WriteValue(param[1], param[2]);
            
            e.Player.SendSuccessMessage(string.Format("{0}'s {1} attribute changed to {2}.", item.Name, param[1], param[2]));
        }
        public void ItemGet(CommandArgs e)
        {
            var param = e.Parameters;

            if (param.Count == 0 || param.Count < 3)
            {
                e.Player.SendErrorMessage("Not enough information. Use [c/FFFF00:/giveitem <item name | ID> <stack #> <prefix #>].");
                return;
            }

            string List = " ";
                for (int i = 0; i < Parameters.Length; i++)
                    List += Parameters[i] + ", ";

            var itemList = TShock.Utils.GetItemByIdOrName(param[0]);

            if (itemList.Count == 0)
            {
                e.Player.SendErrorMessage("Item " + param[0] + " not found.");
                return;
            }
            Item getItem = itemList[0];

            var data = Plugin.Instance.item_data;
            Block block;
            if (!data.BlockExists(getItem.Name))
            {
                e.Player.SendErrorMessage("Item " + param[0] + " has no reference stored. [c/FFFF00:First add it using /tweak].");
                return;
            }
            if (param.Count < 2)
            {
                e.Player.SendErrorMessage("Not enough information. Use [c/FFFF00:/giveitem <iten name | ID> <stack #>]"); //+
                        /*"Available parameters:[c/FFFF00:" + List.Replace(":", "").Replace("0", "").TrimEnd(',', ' ') +  "].");*/
                return;
            }
            else
            {
                if (!int.TryParse(param[1], out int stack))
                    stack = 1;

                block = data.GetBlock(getItem.Name);

                byte.TryParse(block.GetValue(param[2]), out byte prefix);
                int index = Item.NewItem(e.TPlayer.position, new Microsoft.Xna.Framework.Vector2(32, 48), getItem.type, stack, false, prefix);

                Item item = Main.item[index];
                //if (param.Contains(Parameters[Damage]))
                    int.TryParse(block.GetValue(Parameters[Damage].TrimEnd(':', '0')), out item.damage);
                //if (param.Contains(Parameters[Crit]))
                    int.TryParse(block.GetValue(Parameters[Crit].TrimEnd(':', '0')), out item.crit);
                //if (param.Contains(Parameters[KB])) 
                    float.TryParse(block.GetValue(Parameters[KB].TrimEnd(':', '0')), out item.knockBack);
                //if (param.Contains(Parameters[Prefix]))
                    byte.TryParse(block.GetValue(Parameters[Prefix].TrimEnd(':', '0')), out item.prefix);
                //if (param.Contains(Parameters[ReuseDelay]))
                    int.TryParse(block.GetValue(Parameters[ReuseDelay].TrimEnd(':', '0')), out item.reuseDelay);
                //if (param.Contains(Parameters[Shoot]))
                    int.TryParse(block.GetValue(Parameters[Shoot].TrimEnd(':', '0')), out item.shoot);
                //if (param.Contains(Parameters[ShootSpeed]))
                    float.TryParse(block.GetValue(Parameters[ShootSpeed].TrimEnd(':', '0')), out item.shootSpeed);
                //if (param.Contains(Parameters[UseAmmo]))
                    int.TryParse(block.GetValue(Parameters[UseAmmo].TrimEnd(':', '0')), out item.useAmmo);
                //if (param.Contains(Parameters[UseTime]))
                    int.TryParse(block.GetValue(Parameters[UseTime].TrimEnd(':', '0')), out item.useTime);
                //if (param.Contains(Parameters[Width]))
                    int.TryParse(block.GetValue(Parameters[Width].TrimEnd(':', '0')), out item.width);
                //if (param.Contains(Parameters[Height]))
                    int.TryParse(block.GetValue(Parameters[Height].TrimEnd(':', '0')), out item.height);
                //if (param.Contains(Parameters[AutoReuse]))
                    bool.TryParse(block.GetValue(Parameters[AutoReuse].TrimEnd(':', '0')), out item.autoReuse);
                //if (param.Contains(Parameters[Ammo]))
                    int.TryParse(block.GetValue(Parameters[Ammo].TrimEnd(':', '0')), out item.ammo);
                //if (param.Contains(Parameters[Scale]))
                    float.TryParse(block.GetValue(Parameters[Scale].TrimEnd(':', '0')), out item.scale);

                TSPlayer.All.SendData(PacketTypes.TweakItem, "", index, 255, 63);
            }
        }
        #endregion


        #region Safe Regions v2
        public const string Heading = "Regions";
        public const int Max = 256;
        public Dictionary<string,bool> pvpRules = new Dictionary<string, bool>(Max);
        public void Setup(CommandArgs e)
        {
            var regionData = Plugin.Instance.regionData;
            if (e.Message.StartsWith("load"))
            {
                int count = 0;
                var regions = TShock.Regions.Regions;
                for (int i = 0; i < Max; i++)
                {
                    if (i >= regions.Count)
                        break;
                    if (!pvpRules.Keys.Contains(regions[i].Name.ToLower()))
                    {
                        count++;
                        pvpRules.Add(regions[i].Name.ToLower(), false);
                    }
                }
                e.Player.SendSuccessMessage(count + " regions loaded into the database.");
            }
            if (e.Message.StartsWith("region") && e.Message.Contains("define"))
            {
                pvpRules.Add(e.Message.Substring(14).ToLower(), false);
                e.Player.SendSuccessMessage("Region " + e.Message.Substring(14) + " has been logged into the database.");
                return;
            }
            if (e.Message.StartsWith("sr") && e.Message.Contains(" "))
            {
                Block block = regionData.GetBlock(Heading);
                string sub = e.Message.Substring(e.Message.IndexOf(" ") + 1).ToLower();
                if (sub.Contains(" "))
                {
                    string arg = sub.Substring(sub.LastIndexOf(" ") + 1);
                    sub = sub.Substring(0, sub.IndexOf(" "));
                    var region = TShock.Regions.Regions.Where(t => t.Name.ToLower() == arg.ToLower()).ToArray();
                    if (region.Length > 0)
                    {
                        if (sub == "pvp")
                        {
                            pvpRules[arg] = !pvpRules[arg];
                            block.WriteValue(Utils.GetKey(block, arg), region[0].Name.ToLower(), pvpRules[arg]);
                            e.Player.SendSuccessMessage(pvpRules[arg] ? arg + " region is now PvP enabled." : arg + " region has PvP turned off.");
                            return;
                        }
                    }
                    e.Player.SendErrorMessage("There was an error in region name input.");
                    return;
                }
                if (sub.ToLower() == "help")
                {
                    e.Player.SendErrorMessage("Flips the PvP status of the region: [c/FFFF00:/sr pvp <region name>]");
                    return;
                }
                e.Player.SendErrorMessage("Flips the PvP status of the region: [c/FFFF00:/sr pvp <region name>]");
            }
            else
            {
                e.Player.SendErrorMessage("Flips the PvP status of the region: [c/FFFF00:/sr pvp <region name>]");
            }
        }
        #endregion
    }
}