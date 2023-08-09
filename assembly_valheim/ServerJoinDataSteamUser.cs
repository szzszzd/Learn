using System;
using Steamworks;

// Token: 0x02000144 RID: 324
public class ServerJoinDataSteamUser : ServerJoinData
{
	// Token: 0x06000C39 RID: 3129 RVA: 0x00058B3C File Offset: 0x00056D3C
	public ServerJoinDataSteamUser(ulong joinUserID)
	{
		this.m_joinUserID = new CSteamID(joinUserID);
		this.m_serverName = this.ToString();
	}

	// Token: 0x06000C3A RID: 3130 RVA: 0x00058B5C File Offset: 0x00056D5C
	public ServerJoinDataSteamUser(CSteamID joinUserID)
	{
		this.m_joinUserID = joinUserID;
		this.m_serverName = this.ToString();
	}

	// Token: 0x06000C3B RID: 3131 RVA: 0x00058B78 File Offset: 0x00056D78
	public override bool IsValid()
	{
		return this.m_joinUserID.IsValid();
	}

	// Token: 0x06000C3C RID: 3132 RVA: 0x00058B93 File Offset: 0x00056D93
	public override string GetDataName()
	{
		return "Steam user";
	}

	// Token: 0x06000C3D RID: 3133 RVA: 0x00058B9C File Offset: 0x00056D9C
	public override bool Equals(object obj)
	{
		ServerJoinDataSteamUser serverJoinDataSteamUser = obj as ServerJoinDataSteamUser;
		return serverJoinDataSteamUser != null && base.Equals(obj) && this.m_joinUserID.Equals(serverJoinDataSteamUser.m_joinUserID);
	}

	// Token: 0x06000C3E RID: 3134 RVA: 0x00058BD4 File Offset: 0x00056DD4
	public override int GetHashCode()
	{
		return (-995281327 * -1521134295 + base.GetHashCode()) * -1521134295 + this.m_joinUserID.GetHashCode();
	}

	// Token: 0x06000C3F RID: 3135 RVA: 0x00058A6F File Offset: 0x00056C6F
	public static bool operator ==(ServerJoinDataSteamUser left, ServerJoinDataSteamUser right)
	{
		if (left == null || right == null)
		{
			return left == null && right == null;
		}
		return left.Equals(right);
	}

	// Token: 0x06000C40 RID: 3136 RVA: 0x00058C0E File Offset: 0x00056E0E
	public static bool operator !=(ServerJoinDataSteamUser left, ServerJoinDataSteamUser right)
	{
		return !(left == right);
	}

	// Token: 0x06000C41 RID: 3137 RVA: 0x00058C1C File Offset: 0x00056E1C
	public override string ToString()
	{
		return this.m_joinUserID.ToString();
	}

	// Token: 0x17000072 RID: 114
	// (get) Token: 0x06000C42 RID: 3138 RVA: 0x00058C3D File Offset: 0x00056E3D
	// (set) Token: 0x06000C43 RID: 3139 RVA: 0x00058C45 File Offset: 0x00056E45
	public CSteamID m_joinUserID { get; private set; }

	// Token: 0x04000E9A RID: 3738
	public const string typeName = "Steam user";
}
