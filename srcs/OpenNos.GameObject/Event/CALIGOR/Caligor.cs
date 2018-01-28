﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Event.CALIGOR
{
    public static class Caligor
    {
        #region Properties

        public static int AngelDamage { get; set; }

        public static int DemonDamage { get; set; }

        public static MapInstance CaligorMapInstance { get; set; }

        public static MapInstance EntryMap { get; set; }

        public static MapMonster RaidBoss { get; set; }

        public static short RaidTime { get; set; }

        public static bool IsLocked { get; set; }

        #endregion


        #region Methods

        public static async Task GenerateCaligor()
        {
            RaidTime = 3600;
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CALIGOR_REALM_OPEN"), 0));

            CaligorMapInstance =
                ServerManager.Instance.GenerateMapInstance(154, MapInstanceType.CaligorInstance, new InstanceBag());
            ServerManager.Instance.Act4Maps.Add(CaligorMapInstance);

            EntryMap = ServerManager.Instance.Act4Maps.FirstOrDefault(m => m.Map.MapId == 153);

            if (EntryMap == null)
            {
                return;
            }

            EntryMap?.CreatePortal(new Portal
            {
                SourceMapId = 153,
                SourceX = 70,
                SourceY = 159,
                DestinationMapId = 0,
                DestinationX = 70,
                DestinationY = 159,
                DestinationMapInstanceId = CaligorMapInstance.MapInstanceId,
                Type = -1
            });
            EntryMap?.CreatePortal(new Portal
            {
                SourceMapId = 153,
                SourceX = 110,
                SourceY = 159,
                DestinationMapId = 0,
                DestinationX = 110,
                DestinationY = 159,
                DestinationMapInstanceId = CaligorMapInstance.MapInstanceId,
                Type = -1
            });
            //TODO: Add the top map portal

            RaidBoss = CaligorMapInstance.Monsters.FirstOrDefault(s => s.Monster.NpcMonsterVNum == 2305);

            if (RaidBoss == null)
            {
                EndRaid();
                return;
            }

            RaidBoss.IsBoss = true;
            RaidBoss?.BattleEntity.OnDeathEvents.Add(new EventContainer(CaligorMapInstance, EventActionType.SCRIPTEND, (byte)1));

            while (RaidTime > 0)
            {
                RefreshState();
                RaidTime -= 5;
                await Task.Delay(5000);
            }
            EndRaid();
        }

        public static void EndRaid()
        {
            ServerManager.Instance.Act4Maps.Remove(CaligorMapInstance);
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CALIGOR_REALM_CLOSED"), 0));
            TeleportPlayers();
            ServerManager.Instance.StartedEvents.Remove(EventType.CALIGOR);
            EventHelper.Instance.RunEvent(new EventContainer(CaligorMapInstance, EventActionType.DISPOSEMAP, null));
        }

        public static void LockEntry()
        {
            foreach (var portal in EntryMap.Portals.Where(p => p.DestinationMapInstanceId == CaligorMapInstance.MapInstanceId))
            {
                EntryMap.Portals.Remove(portal);
                EntryMap.Broadcast(portal.GenerateGp());
            }
            IsLocked = true;
            ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CALIGOR_REALM_LOCKED"), 0));
        }

        public static void RefreshState()
        {
            CaligorMapInstance.Broadcast($"ch_dm {RaidBoss.MaxHp} {AngelDamage} {DemonDamage} {RaidTime}");

            if (AngelDamage + DemonDamage > RaidBoss.MaxHp / 2 && !IsLocked)
            {
                LockEntry();
            }
        }

        public static void TeleportPlayers()
        {
            foreach (var character in CaligorMapInstance.Sessions)
            {
                // Teleport everyone back to the raidmap
                ServerManager.Instance.ChangeMapInstance(character.Character.CharacterId, EntryMap.MapInstanceId, character.Character.MapX, character.Character.MapY);
            }
        }

        #endregion
    }
}