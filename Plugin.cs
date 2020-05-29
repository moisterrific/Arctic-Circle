﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TShockAPI;
using TerrariaApi.Server;
using RUDD.Dotnet;

using ItemClasses;
using TeamSetQueue;

namespace ArcticCircle
{
    public class Plugin : TerrariaPlugin
    {
        public override string Author => "";
        public override string Name => "";
        public override string Description => "";
        public override Version Version => new Version(1, 0, 0, 0);
        public Plugin(Main game) : base (game)
        {
            Instance = this;
            Hooks.Instance = new Hooks();
            Delegates.Instance = new Delegates();
        }   
        public static Plugin Instance;
        public static Ini classINI;
        public DataStore teamData;
        public override void Initialize()
        {
            teamData = new DataStore("config\\team_data");
            classINI = new Ini()
            {
                path = "config\\class_data" + Ini.ext
            };
            Delegates.Instance.Reload(new CommandArgs(string.Empty, TSPlayer.Server, null));
            ServerApi.Hooks.ServerJoin.Register(this, Hooks.Instance.OnJoin);
            ServerApi.Hooks.ServerLeave.Register(this, Hooks.Instance.OnLeave);
            ServerApi.Hooks.GameUpdate.Register(this, Hooks.Instance.ItemClassGameUpdate);
            ServerApi.Hooks.NetGetData.Register(this, Hooks.Instance.OnGetData);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {   
                ServerApi.Hooks.ServerJoin.Deregister(this, Hooks.Instance.OnJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, Hooks.Instance.OnLeave);
                ServerApi.Hooks.GameUpdate.Deregister(this, Hooks.Instance.ItemClassGameUpdate);
                ServerApi.Hooks.NetGetData.Deregister(this, Hooks.Instance.OnGetData);
            }
        }
    }
}