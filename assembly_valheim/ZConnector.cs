using System;
using System.Net;
using System.Net.Sockets;

// Token: 0x0200014F RID: 335
public class ZConnector : IDisposable
{
	// Token: 0x06000C9C RID: 3228 RVA: 0x00059B6C File Offset: 0x00057D6C
	public ZConnector(string host, int port)
	{
		this.m_host = host;
		this.m_port = port;
		ZLog.Log("Zconnect " + host + " " + port.ToString());
		Dns.BeginGetHostEntry(host, new AsyncCallback(this.OnHostLookupDone), null);
	}

	// Token: 0x06000C9D RID: 3229 RVA: 0x00059BBD File Offset: 0x00057DBD
	public void Dispose()
	{
		this.Close();
	}

	// Token: 0x06000C9E RID: 3230 RVA: 0x00059BC8 File Offset: 0x00057DC8
	private void Close()
	{
		if (this.m_socket != null)
		{
			try
			{
				if (this.m_socket.Connected)
				{
					this.m_socket.Shutdown(SocketShutdown.Both);
				}
			}
			catch (Exception ex)
			{
				string str = "Some excepetion when shuting down ZConnector socket, ignoring:";
				Exception ex2 = ex;
				ZLog.Log(str + ((ex2 != null) ? ex2.ToString() : null));
			}
			this.m_socket.Close();
			this.m_socket = null;
		}
		this.m_abort = true;
	}

	// Token: 0x06000C9F RID: 3231 RVA: 0x00059C40 File Offset: 0x00057E40
	public bool IsPeer(string host, int port)
	{
		return this.m_host == host && this.m_port == port;
	}

	// Token: 0x06000CA0 RID: 3232 RVA: 0x00059C5C File Offset: 0x00057E5C
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
			ZLog.Log("ZConnector - result completed");
			return true;
		}
		this.m_timer += dt;
		if (this.m_timer > ZConnector.m_timeout)
		{
			ZLog.Log("ZConnector - timeout");
			this.Close();
			return true;
		}
		return false;
	}

	// Token: 0x06000CA1 RID: 3233 RVA: 0x00059CE0 File Offset: 0x00057EE0
	public ZSocket Complete()
	{
		if (this.m_socket != null && this.m_socket.Connected)
		{
			ZSocket result = new ZSocket(this.m_socket, this.m_host);
			this.m_socket = null;
			return result;
		}
		this.Close();
		return null;
	}

	// Token: 0x06000CA2 RID: 3234 RVA: 0x00059D17 File Offset: 0x00057F17
	public bool CompareEndPoint(IPEndPoint endpoint)
	{
		return this.m_endPoint.Equals(endpoint);
	}

	// Token: 0x06000CA3 RID: 3235 RVA: 0x00059D28 File Offset: 0x00057F28
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
		ZLog.Log("Host lookup done , addresses: " + iphostEntry.AddressList.Length.ToString());
		foreach (IPAddress ipaddress in iphostEntry.AddressList)
		{
			string str = " ";
			IPAddress ipaddress2 = ipaddress;
			ZLog.Log(str + ((ipaddress2 != null) ? ipaddress2.ToString() : null));
		}
		this.m_socket = ZSocket.CreateSocket();
		this.m_result = this.m_socket.BeginConnect(iphostEntry.AddressList, this.m_port, null, null);
	}

	// Token: 0x06000CA4 RID: 3236 RVA: 0x00059DE8 File Offset: 0x00057FE8
	public string GetEndPointString()
	{
		return this.m_host + ":" + this.m_port.ToString();
	}

	// Token: 0x06000CA5 RID: 3237 RVA: 0x00059E05 File Offset: 0x00058005
	public string GetHostName()
	{
		return this.m_host;
	}

	// Token: 0x06000CA6 RID: 3238 RVA: 0x00059E0D File Offset: 0x0005800D
	public int GetHostPort()
	{
		return this.m_port;
	}

	// Token: 0x04000ED1 RID: 3793
	private Socket m_socket;

	// Token: 0x04000ED2 RID: 3794
	private IAsyncResult m_result;

	// Token: 0x04000ED3 RID: 3795
	private IPEndPoint m_endPoint;

	// Token: 0x04000ED4 RID: 3796
	private string m_host;

	// Token: 0x04000ED5 RID: 3797
	private int m_port;

	// Token: 0x04000ED6 RID: 3798
	private bool m_dnsError;

	// Token: 0x04000ED7 RID: 3799
	private bool m_abort;

	// Token: 0x04000ED8 RID: 3800
	private float m_timer;

	// Token: 0x04000ED9 RID: 3801
	private static float m_timeout = 5f;
}
