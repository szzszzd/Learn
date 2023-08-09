using System;
using System.Collections.Generic;

// Token: 0x020001A2 RID: 418
public class PlayFabMatchmakingServerData
{
	// Token: 0x060010B3 RID: 4275 RVA: 0x0006D25C File Offset: 0x0006B45C
	public override bool Equals(object obj)
	{
		PlayFabMatchmakingServerData playFabMatchmakingServerData = obj as PlayFabMatchmakingServerData;
		return playFabMatchmakingServerData != null && this.remotePlayerId == playFabMatchmakingServerData.remotePlayerId && this.serverIp == playFabMatchmakingServerData.serverIp && this.isDedicatedServer == playFabMatchmakingServerData.isDedicatedServer;
	}

	// Token: 0x060010B4 RID: 4276 RVA: 0x0006D2AC File Offset: 0x0006B4AC
	public override int GetHashCode()
	{
		return ((1416698207 * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.remotePlayerId)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.serverIp)) * -1521134295 + this.isDedicatedServer.GetHashCode();
	}

	// Token: 0x060010B5 RID: 4277 RVA: 0x0006D300 File Offset: 0x0006B500
	public override string ToString()
	{
		return string.Format("Server Name : {0}\nServer IP : {1}\nGame Version : {2}\nNetwork Version : {3}\nPlayer ID : {4}\nPlayers : {5}\nLobby ID : {6}\nNetwork ID : {7}\nJoin Code : {8}\nPlatform Restriction : {9}\nDedicated : {10}\nCommunity : {11}\nTickCreated : {12}\n", new object[]
		{
			this.serverName,
			this.serverIp,
			this.gameVersion,
			this.networkVersion,
			this.remotePlayerId,
			this.numPlayers,
			this.lobbyId,
			this.networkId,
			this.joinCode,
			this.platformRestriction,
			this.isDedicatedServer,
			this.isCommunityServer,
			this.tickCreated
		});
	}

	// Token: 0x04001188 RID: 4488
	public string serverName;

	// Token: 0x04001189 RID: 4489
	public string worldName;

	// Token: 0x0400118A RID: 4490
	public string gameVersion;

	// Token: 0x0400118B RID: 4491
	public uint networkVersion;

	// Token: 0x0400118C RID: 4492
	public string networkId = "";

	// Token: 0x0400118D RID: 4493
	public string joinCode;

	// Token: 0x0400118E RID: 4494
	public string remotePlayerId;

	// Token: 0x0400118F RID: 4495
	public string lobbyId;

	// Token: 0x04001190 RID: 4496
	public string xboxUserId = "";

	// Token: 0x04001191 RID: 4497
	public string serverIp = "";

	// Token: 0x04001192 RID: 4498
	public string platformRestriction = "None";

	// Token: 0x04001193 RID: 4499
	public bool isDedicatedServer;

	// Token: 0x04001194 RID: 4500
	public bool isCommunityServer;

	// Token: 0x04001195 RID: 4501
	public bool havePassword;

	// Token: 0x04001196 RID: 4502
	public uint numPlayers;

	// Token: 0x04001197 RID: 4503
	public long tickCreated;
}
