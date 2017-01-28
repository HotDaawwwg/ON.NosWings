﻿////<auto-generated <- Codemaid exclusion for now (PacketIndex Order is important for maintenance)
using OpenNos.Core;
using OpenNos.Domain;

namespace OpenNos.GameObject
{
    [PacketHeader("$Promote", PassNonParseablePacket = true, Authority = AuthorityType.GameMaster)]
    public class PromotePacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public string CharacterName { get; set; }

        #endregion
    }
}