using System;
using System.ComponentModel;
using System.Net;

// Token: 0x02000146 RID: 326
public class ServerJoinDataDedicated : ServerJoinData
{
	// Token: 0x06000C4E RID: 3150 RVA: 0x00058CFC File Offset: 0x00056EFC
	public ServerJoinDataDedicated(string address)
	{
		string[] array = address.Split(new char[]
		{
			':'
		});
		if (array.Length < 1 || array.Length > 2)
		{
			this.m_isValid = new bool?(false);
			return;
		}
		this.SetHost(array[0]);
		ushort port;
		if (array.Length == 2 && ushort.TryParse(array[1], out port))
		{
			this.m_port = port;
		}
		else
		{
			this.m_port = 2456;
		}
		this.m_serverName = this.ToString();
	}

	// Token: 0x06000C4F RID: 3151 RVA: 0x00058D78 File Offset: 0x00056F78
	public ServerJoinDataDedicated(string host, ushort port)
	{
		if (host.Split(new char[]
		{
			':'
		}).Length != 1)
		{
			this.m_isValid = new bool?(false);
			return;
		}
		this.SetHost(host);
		this.m_port = port;
		this.m_serverName = this.ToString();
	}

	// Token: 0x06000C50 RID: 3152 RVA: 0x00058DC8 File Offset: 0x00056FC8
	public ServerJoinDataDedicated(uint host, ushort port)
	{
		this.SetHost(host);
		this.m_port = port;
		this.m_serverName = this.ToString();
	}

	// Token: 0x17000074 RID: 116
	// (get) Token: 0x06000C51 RID: 3153 RVA: 0x00058DEA File Offset: 0x00056FEA
	// (set) Token: 0x06000C52 RID: 3154 RVA: 0x00058DF2 File Offset: 0x00056FF2
	public ServerJoinDataDedicated.AddressType AddressVariant { get; private set; }

	// Token: 0x06000C53 RID: 3155 RVA: 0x00058DFC File Offset: 0x00056FFC
	public override bool IsValid()
	{
		if (this.m_isValid != null)
		{
			return this.m_isValid.Value;
		}
		if (this.m_ipString == null)
		{
			IPAddress ipaddress;
			this.m_isValid = new bool?(ServerJoinData.URLToIP(this.m_host, out ipaddress));
			if (this.m_isValid.Value)
			{
				byte[] addressBytes = ipaddress.GetAddressBytes();
				this.m_ipString = addressBytes[0].ToString();
				for (int i = 1; i < 4; i++)
				{
					this.m_ipString = this.m_ipString + "." + addressBytes[i].ToString();
				}
			}
			return this.m_isValid.Value;
		}
		ZLog.LogError("This part of the code should never run!");
		return false;
	}

	// Token: 0x06000C54 RID: 3156 RVA: 0x00058EB0 File Offset: 0x000570B0
	public void IsValidAsync(Action<bool> resultCallback)
	{
		bool result = false;
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		backgroundWorker.DoWork += delegate(object sender, DoWorkEventArgs args)
		{
			result = this.IsValid();
		};
		backgroundWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs args)
		{
			resultCallback(result);
		};
		backgroundWorker.RunWorkerAsync();
	}

	// Token: 0x06000C55 RID: 3157 RVA: 0x00058F06 File Offset: 0x00057106
	public override string GetDataName()
	{
		return "Dedicated";
	}

	// Token: 0x06000C56 RID: 3158 RVA: 0x00058F10 File Offset: 0x00057110
	public override bool Equals(object obj)
	{
		ServerJoinDataDedicated serverJoinDataDedicated = obj as ServerJoinDataDedicated;
		return serverJoinDataDedicated != null && base.Equals(obj) && this.m_host == serverJoinDataDedicated.m_host && this.m_port == serverJoinDataDedicated.m_port;
	}

	// Token: 0x06000C57 RID: 3159 RVA: 0x00058F54 File Offset: 0x00057154
	public override int GetHashCode()
	{
		return ((-468063053 * -1521134295 + base.GetHashCode()) * -1521134295 + this.m_host.GetHashCode()) * -1521134295 + this.m_port.GetHashCode();
	}

	// Token: 0x06000C58 RID: 3160 RVA: 0x00058A6F File Offset: 0x00056C6F
	public static bool operator ==(ServerJoinDataDedicated left, ServerJoinDataDedicated right)
	{
		if (left == null || right == null)
		{
			return left == null && right == null;
		}
		return left.Equals(right);
	}

	// Token: 0x06000C59 RID: 3161 RVA: 0x00058F9A File Offset: 0x0005719A
	public static bool operator !=(ServerJoinDataDedicated left, ServerJoinDataDedicated right)
	{
		return !(left == right);
	}

	// Token: 0x06000C5A RID: 3162 RVA: 0x00058FA8 File Offset: 0x000571A8
	private void SetHost(uint host)
	{
		string text = "";
		uint num = 255U;
		for (int i = 24; i >= 0; i -= 8)
		{
			text += ((num << i & host) >> i).ToString();
			if (i != 0)
			{
				text += ".";
			}
		}
		this.m_host = text;
		this.m_ipString = text;
		this.m_isValid = new bool?(true);
		this.AddressVariant = ServerJoinDataDedicated.AddressType.IP;
	}

	// Token: 0x06000C5B RID: 3163 RVA: 0x0005901C File Offset: 0x0005721C
	private void SetHost(string host)
	{
		string[] array = host.Split(new char[]
		{
			'.'
		});
		if (array.Length == 4)
		{
			byte[] array2 = new byte[4];
			bool flag = true;
			for (int i = 0; i < 4; i++)
			{
				if (!byte.TryParse(array[i], out array2[i]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				this.m_host = host;
				this.m_ipString = host;
				this.m_isValid = new bool?(true);
				this.AddressVariant = ServerJoinDataDedicated.AddressType.IP;
				return;
			}
		}
		string text = host;
		if (!host.StartsWith("http://") && !host.StartsWith("https://"))
		{
			text = "http://" + host;
		}
		if (!host.EndsWith("/"))
		{
			text += "/";
		}
		Uri uri;
		if (Uri.TryCreate(text, UriKind.Absolute, out uri))
		{
			this.m_host = host;
			this.m_isValid = null;
			this.AddressVariant = ServerJoinDataDedicated.AddressType.URL;
			return;
		}
		this.m_host = host;
		this.m_isValid = new bool?(false);
		this.AddressVariant = ServerJoinDataDedicated.AddressType.None;
	}

	// Token: 0x06000C5C RID: 3164 RVA: 0x00059117 File Offset: 0x00057317
	public string GetHost()
	{
		return this.m_host;
	}

	// Token: 0x06000C5D RID: 3165 RVA: 0x0005911F File Offset: 0x0005731F
	public string GetIPString()
	{
		if (!this.IsValid())
		{
			ZLog.LogError("Can't get IP from invalid server data");
			return null;
		}
		return this.m_ipString;
	}

	// Token: 0x06000C5E RID: 3166 RVA: 0x0005913C File Offset: 0x0005733C
	public string GetIPPortString()
	{
		return this.GetIPString() + ":" + this.m_port.ToString();
	}

	// Token: 0x06000C5F RID: 3167 RVA: 0x0005916C File Offset: 0x0005736C
	public override string ToString()
	{
		return this.GetHost() + ":" + this.m_port.ToString();
	}

	// Token: 0x17000075 RID: 117
	// (get) Token: 0x06000C60 RID: 3168 RVA: 0x0005919C File Offset: 0x0005739C
	// (set) Token: 0x06000C61 RID: 3169 RVA: 0x000591A4 File Offset: 0x000573A4
	public string m_host { get; private set; }

	// Token: 0x17000076 RID: 118
	// (get) Token: 0x06000C62 RID: 3170 RVA: 0x000591AD File Offset: 0x000573AD
	// (set) Token: 0x06000C63 RID: 3171 RVA: 0x000591B5 File Offset: 0x000573B5
	public ushort m_port { get; private set; }

	// Token: 0x04000E9E RID: 3742
	public const string typeName = "Dedicated";

	// Token: 0x04000EA0 RID: 3744
	private bool? m_isValid;

	// Token: 0x04000EA3 RID: 3747
	private string m_ipString;

	// Token: 0x02000147 RID: 327
	public enum AddressType
	{
		// Token: 0x04000EA5 RID: 3749
		None,
		// Token: 0x04000EA6 RID: 3750
		IP,
		// Token: 0x04000EA7 RID: 3751
		URL
	}
}
