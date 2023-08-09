using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// Token: 0x0200014D RID: 333
public class ZBroastcast : IDisposable
{
	// Token: 0x17000084 RID: 132
	// (get) Token: 0x06000C90 RID: 3216 RVA: 0x000597A5 File Offset: 0x000579A5
	public static ZBroastcast instance
	{
		get
		{
			return ZBroastcast.m_instance;
		}
	}

	// Token: 0x06000C91 RID: 3217 RVA: 0x000597AC File Offset: 0x000579AC
	public static void Initialize()
	{
		if (ZBroastcast.m_instance == null)
		{
			ZBroastcast.m_instance = new ZBroastcast();
		}
	}

	// Token: 0x06000C92 RID: 3218 RVA: 0x000597C0 File Offset: 0x000579C0
	private ZBroastcast()
	{
		ZLog.Log("opening zbroadcast");
		this.m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		this.m_socket.EnableBroadcast = true;
		try
		{
			this.m_listner = new UdpClient(6542);
			this.m_listner.EnableBroadcast = true;
			this.m_listner.BeginReceive(new AsyncCallback(this.GotPackage), null);
		}
		catch (Exception ex)
		{
			this.m_listner = null;
			ZLog.Log("Error creating zbroadcast socket " + ex.ToString());
		}
	}

	// Token: 0x06000C93 RID: 3219 RVA: 0x00059874 File Offset: 0x00057A74
	public void SetServerPort(int port)
	{
		this.m_myPort = port;
	}

	// Token: 0x06000C94 RID: 3220 RVA: 0x00059880 File Offset: 0x00057A80
	public void Dispose()
	{
		ZLog.Log("Clozing zbroadcast");
		if (this.m_listner != null)
		{
			this.m_listner.Close();
		}
		this.m_socket.Close();
		this.m_lock.Close();
		if (ZBroastcast.m_instance == this)
		{
			ZBroastcast.m_instance = null;
		}
	}

	// Token: 0x06000C95 RID: 3221 RVA: 0x000598CE File Offset: 0x00057ACE
	public void Update(float dt)
	{
		this.m_timer -= dt;
		if (this.m_timer <= 0f)
		{
			this.m_timer = 5f;
			if (this.m_myPort != 0)
			{
				this.Ping();
			}
		}
		this.TimeoutHosts(dt);
	}

	// Token: 0x06000C96 RID: 3222 RVA: 0x0005990C File Offset: 0x00057B0C
	private void GotPackage(IAsyncResult ar)
	{
		IPEndPoint ipendPoint = new IPEndPoint(0L, 0);
		byte[] array;
		try
		{
			array = this.m_listner.EndReceive(ar, ref ipendPoint);
		}
		catch (ObjectDisposedException)
		{
			return;
		}
		if (array.Length < 5)
		{
			return;
		}
		ZPackage zpackage = new ZPackage(array);
		if (zpackage.ReadChar() != 'F')
		{
			return;
		}
		if (zpackage.ReadChar() != 'E')
		{
			return;
		}
		if (zpackage.ReadChar() != 'J')
		{
			return;
		}
		if (zpackage.ReadChar() != 'D')
		{
			return;
		}
		int port = zpackage.ReadInt();
		this.m_lock.WaitOne();
		this.AddHost(ipendPoint.Address.ToString(), port);
		this.m_lock.ReleaseMutex();
		this.m_listner.BeginReceive(new AsyncCallback(this.GotPackage), null);
	}

	// Token: 0x06000C97 RID: 3223 RVA: 0x000599CC File Offset: 0x00057BCC
	private void Ping()
	{
		IPEndPoint remoteEP = new IPEndPoint(IPAddress.Broadcast, 6542);
		ZPackage zpackage = new ZPackage();
		zpackage.Write('F');
		zpackage.Write('E');
		zpackage.Write('J');
		zpackage.Write('D');
		zpackage.Write(this.m_myPort);
		this.m_socket.SendTo(zpackage.GetArray(), remoteEP);
	}

	// Token: 0x06000C98 RID: 3224 RVA: 0x00059A30 File Offset: 0x00057C30
	private void AddHost(string host, int port)
	{
		foreach (ZBroastcast.HostData hostData in this.m_hosts)
		{
			if (hostData.m_port == port && hostData.m_host == host)
			{
				hostData.m_timeout = 0f;
				return;
			}
		}
		ZBroastcast.HostData hostData2 = new ZBroastcast.HostData();
		hostData2.m_host = host;
		hostData2.m_port = port;
		hostData2.m_timeout = 0f;
		this.m_hosts.Add(hostData2);
	}

	// Token: 0x06000C99 RID: 3225 RVA: 0x00059ACC File Offset: 0x00057CCC
	private void TimeoutHosts(float dt)
	{
		this.m_lock.WaitOne();
		foreach (ZBroastcast.HostData hostData in this.m_hosts)
		{
			hostData.m_timeout += dt;
			if (hostData.m_timeout > 10f)
			{
				this.m_hosts.Remove(hostData);
				return;
			}
		}
		this.m_lock.ReleaseMutex();
	}

	// Token: 0x06000C9A RID: 3226 RVA: 0x00059B5C File Offset: 0x00057D5C
	public void GetHostList(List<ZBroastcast.HostData> hosts)
	{
		hosts.AddRange(this.m_hosts);
	}

	// Token: 0x04000EC4 RID: 3780
	private List<ZBroastcast.HostData> m_hosts = new List<ZBroastcast.HostData>();

	// Token: 0x04000EC5 RID: 3781
	private static ZBroastcast m_instance;

	// Token: 0x04000EC6 RID: 3782
	private const int m_port = 6542;

	// Token: 0x04000EC7 RID: 3783
	private const float m_pingInterval = 5f;

	// Token: 0x04000EC8 RID: 3784
	private const float m_hostTimeout = 10f;

	// Token: 0x04000EC9 RID: 3785
	private float m_timer;

	// Token: 0x04000ECA RID: 3786
	private int m_myPort;

	// Token: 0x04000ECB RID: 3787
	private Socket m_socket;

	// Token: 0x04000ECC RID: 3788
	private UdpClient m_listner;

	// Token: 0x04000ECD RID: 3789
	private Mutex m_lock = new Mutex();

	// Token: 0x0200014E RID: 334
	public class HostData
	{
		// Token: 0x04000ECE RID: 3790
		public string m_host;

		// Token: 0x04000ECF RID: 3791
		public int m_port;

		// Token: 0x04000ED0 RID: 3792
		public float m_timeout;
	}
}
