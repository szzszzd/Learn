using System;
using System.Net;
using System.Net.Sockets;

// Token: 0x02000143 RID: 323
public abstract class ServerJoinData
{
	// Token: 0x06000C31 RID: 3121 RVA: 0x0000247B File Offset: 0x0000067B
	public virtual bool IsValid()
	{
		return false;
	}

	// Token: 0x06000C32 RID: 3122 RVA: 0x0000C988 File Offset: 0x0000AB88
	public virtual string GetDataName()
	{
		return "";
	}

	// Token: 0x06000C33 RID: 3123 RVA: 0x00058A64 File Offset: 0x00056C64
	public override bool Equals(object obj)
	{
		return obj is ServerJoinData;
	}

	// Token: 0x06000C34 RID: 3124 RVA: 0x0000247B File Offset: 0x0000067B
	public override int GetHashCode()
	{
		return 0;
	}

	// Token: 0x06000C35 RID: 3125 RVA: 0x00058A6F File Offset: 0x00056C6F
	public static bool operator ==(ServerJoinData left, ServerJoinData right)
	{
		if (left == null || right == null)
		{
			return left == null && right == null;
		}
		return left.Equals(right);
	}

	// Token: 0x06000C36 RID: 3126 RVA: 0x00058A88 File Offset: 0x00056C88
	public static bool operator !=(ServerJoinData left, ServerJoinData right)
	{
		return !(left == right);
	}

	// Token: 0x06000C37 RID: 3127 RVA: 0x00058A94 File Offset: 0x00056C94
	public static bool URLToIP(string url, out IPAddress ip)
	{
		bool result;
		try
		{
			IPHostEntry hostEntry = Dns.GetHostEntry(url);
			if (hostEntry.AddressList.Length == 0)
			{
				ip = null;
				result = false;
			}
			else
			{
				ZLog.Log("Got dns entries: " + hostEntry.AddressList.Length.ToString());
				foreach (IPAddress ipaddress in hostEntry.AddressList)
				{
					if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
					{
						ip = ipaddress;
						return true;
					}
				}
				ip = null;
				result = false;
			}
		}
		catch (Exception ex)
		{
			ZLog.Log("Exception while finding ip:" + ex.ToString());
			ip = null;
			result = false;
		}
		return result;
	}

	// Token: 0x04000E99 RID: 3737
	public string m_serverName;
}
