using System;

// Token: 0x02000145 RID: 325
public class ServerJoinDataPlayFabUser : ServerJoinData
{
	// Token: 0x06000C44 RID: 3140 RVA: 0x00058C4E File Offset: 0x00056E4E
	public ServerJoinDataPlayFabUser(string remotePlayerId)
	{
		this.m_remotePlayerId = remotePlayerId;
		this.m_serverName = this.ToString();
	}

	// Token: 0x06000C45 RID: 3141 RVA: 0x00058C69 File Offset: 0x00056E69
	public override bool IsValid()
	{
		return this.m_remotePlayerId != null;
	}

	// Token: 0x06000C46 RID: 3142 RVA: 0x00058C74 File Offset: 0x00056E74
	public override string GetDataName()
	{
		return "PlayFab user";
	}

	// Token: 0x06000C47 RID: 3143 RVA: 0x00058C7C File Offset: 0x00056E7C
	public override bool Equals(object obj)
	{
		ServerJoinDataPlayFabUser serverJoinDataPlayFabUser = obj as ServerJoinDataPlayFabUser;
		return serverJoinDataPlayFabUser != null && base.Equals(obj) && this.ToString() == serverJoinDataPlayFabUser.ToString();
	}

	// Token: 0x06000C48 RID: 3144 RVA: 0x00058CAF File Offset: 0x00056EAF
	public override int GetHashCode()
	{
		return (1688301347 * -1521134295 + base.GetHashCode()) * -1521134295 + this.ToString().GetHashCode();
	}

	// Token: 0x06000C49 RID: 3145 RVA: 0x00058A6F File Offset: 0x00056C6F
	public static bool operator ==(ServerJoinDataPlayFabUser left, ServerJoinDataPlayFabUser right)
	{
		if (left == null || right == null)
		{
			return left == null && right == null;
		}
		return left.Equals(right);
	}

	// Token: 0x06000C4A RID: 3146 RVA: 0x00058CD5 File Offset: 0x00056ED5
	public static bool operator !=(ServerJoinDataPlayFabUser left, ServerJoinDataPlayFabUser right)
	{
		return !(left == right);
	}

	// Token: 0x06000C4B RID: 3147 RVA: 0x00058CE1 File Offset: 0x00056EE1
	public override string ToString()
	{
		return this.m_remotePlayerId;
	}

	// Token: 0x17000073 RID: 115
	// (get) Token: 0x06000C4C RID: 3148 RVA: 0x00058CE9 File Offset: 0x00056EE9
	// (set) Token: 0x06000C4D RID: 3149 RVA: 0x00058CF1 File Offset: 0x00056EF1
	public string m_remotePlayerId { get; private set; }

	// Token: 0x04000E9C RID: 3740
	public const string typeName = "PlayFab user";
}
