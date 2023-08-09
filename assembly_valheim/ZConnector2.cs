using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

// Token: 0x02000150 RID: 336
public class ZConnector2 : IDisposable
{
	// Token: 0x06000CA8 RID: 3240 RVA: 0x00059E21 File Offset: 0x00058021
	public ZConnector2(string host, int port)
	{
		this.m_host = host;
		this.m_port = port;
		Dns.BeginGetHostEntry(host, new AsyncCallback(this.OnHostLookupDone), null);
	}

	// Token: 0x06000CA9 RID: 3241 RVA: 0x00059E4B File Offset: 0x0005804B
	public void Dispose()
	{
		this.Close();
	}

	// Token: 0x06000CAA RID: 3242 RVA: 0x00059E53 File Offset: 0x00058053
	private void Close()
	{
		if (this.m_socket != null)
		{
			this.m_socket.Close();
			this.m_socket = null;
		}
		this.m_abort = true;
	}

	// Token: 0x06000CAB RID: 3243 RVA: 0x00059E76 File Offset: 0x00058076
	public bool IsPeer(string host, int port)
	{
		return this.m_host == host && this.m_port == port;
	}

	// Token: 0x06000CAC RID: 3244 RVA: 0x00059E94 File Offset: 0x00058094
	public bool UpdateStatus(float dt, bool logErrors = false)
	{
		if (this.m_abort)
		{
			ZLog.Log("ZConnector - Abort");
			return true;
		}
		if (this.m_dnsError)
		{
			ZLog.Log("ZConnector - dns error");
			return true;
		}
		if (this.m_result != null && this.m_result.IsCompleted)
		{
			return true;
		}
		this.m_timer += dt;
		if (this.m_timer > ZConnector2.m_timeout)
		{
			this.Close();
			return true;
		}
		return false;
	}

	// Token: 0x06000CAD RID: 3245 RVA: 0x00059F04 File Offset: 0x00058104
	public ZSocket2 Complete()
	{
		if (this.m_socket != null && this.m_socket.Connected)
		{
			ZSocket2 result = new ZSocket2(this.m_socket, this.m_host);
			this.m_socket = null;
			return result;
		}
		this.Close();
		return null;
	}

	// Token: 0x06000CAE RID: 3246 RVA: 0x00059F3B File Offset: 0x0005813B
	public bool CompareEndPoint(IPEndPoint endpoint)
	{
		return this.m_endPoint.Equals(endpoint);
	}

	// Token: 0x06000CAF RID: 3247 RVA: 0x00059F4C File Offset: 0x0005814C
	private void OnHostLookupDone(IAsyncResult res)
	{
		IPHostEntry iphostEntry = Dns.EndGetHostEntry(res);
		if (this.m_abort)
		{
			ZLog.Log("Host lookup abort");
			return;
		}
		if (iphostEntry.AddressList.Length == 0)
		{
			this.m_dnsError = true;
			ZLog.Log("Host lookup adress list empty");
			return;
		}
		iphostEntry.AddressList = this.KeepInetAddrs(iphostEntry.AddressList);
		this.m_socket = ZSocket2.CreateSocket();
		this.m_result = this.m_socket.BeginConnect(iphostEntry.AddressList, this.m_port, null, null);
	}

	// Token: 0x06000CB0 RID: 3248 RVA: 0x00059FCC File Offset: 0x000581CC
	private IPAddress[] KeepInetAddrs(IPAddress[] inetAddrs)
	{
		List<IPAddress> list = new List<IPAddress>();
		foreach (IPAddress ipaddress in inetAddrs)
		{
			if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
			{
				list.Add(ipaddress);
			}
		}
		return list.ToArray();
	}

	// Token: 0x06000CB1 RID: 3249 RVA: 0x0005A009 File Offset: 0x00058209
	public string GetEndPointString()
	{
		return this.m_host + ":" + this.m_port.ToString();
	}

	// Token: 0x06000CB2 RID: 3250 RVA: 0x0005A026 File Offset: 0x00058226
	public string GetHostName()
	{
		return this.m_host;
	}

	// Token: 0x06000CB3 RID: 3251 RVA: 0x0005A02E File Offset: 0x0005822E
	public int GetHostPort()
	{
		return this.m_port;
	}

	// Token: 0x04000EDA RID: 3802
	private TcpClient m_socket;

	// Token: 0x04000EDB RID: 3803
	private IAsyncResult m_result;

	// Token: 0x04000EDC RID: 3804
	private IPEndPoint m_endPoint;

	// Token: 0x04000EDD RID: 3805
	private string m_host;

	// Token: 0x04000EDE RID: 3806
	private int m_port;

	// Token: 0x04000EDF RID: 3807
	private bool m_dnsError;

	// Token: 0x04000EE0 RID: 3808
	private bool m_abort;

	// Token: 0x04000EE1 RID: 3809
	private float m_timer;

	// Token: 0x04000EE2 RID: 3810
	private static float m_timeout = 5f;
}
