using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// Token: 0x02000187 RID: 391
public class ZSocket2 : ZNetStats, IDisposable, ISocket
{
	// Token: 0x06000F84 RID: 3972 RVA: 0x000678E9 File Offset: 0x00065AE9
	public ZSocket2()
	{
	}

	// Token: 0x06000F85 RID: 3973 RVA: 0x00067929 File Offset: 0x00065B29
	public static TcpClient CreateSocket()
	{
		TcpClient tcpClient = new TcpClient(AddressFamily.InterNetwork);
		ZSocket2.ConfigureSocket(tcpClient);
		return tcpClient;
	}

	// Token: 0x06000F86 RID: 3974 RVA: 0x00067937 File Offset: 0x00065B37
	private static void ConfigureSocket(TcpClient socket)
	{
		socket.NoDelay = true;
		socket.SendBufferSize = 2048;
	}

	// Token: 0x06000F87 RID: 3975 RVA: 0x0006794C File Offset: 0x00065B4C
	public ZSocket2(TcpClient socket, string originalHostName = null)
	{
		this.m_socket = socket;
		this.m_originalHostName = originalHostName;
		try
		{
			this.m_endpoint = (this.m_socket.Client.RemoteEndPoint as IPEndPoint);
		}
		catch
		{
			this.Close();
			return;
		}
		this.BeginReceive();
	}

	// Token: 0x06000F88 RID: 3976 RVA: 0x000679E4 File Offset: 0x00065BE4
	public void Dispose()
	{
		this.Close();
		this.m_mutex.Close();
		this.m_sendMutex.Close();
		this.m_recvBuffer = null;
	}

	// Token: 0x06000F89 RID: 3977 RVA: 0x00067A0C File Offset: 0x00065C0C
	public void Close()
	{
		ZLog.Log("Closing socket " + this.GetEndPointString());
		if (this.m_listner != null)
		{
			this.m_listner.Stop();
			this.m_listner = null;
		}
		if (this.m_socket != null)
		{
			this.m_socket.Close();
			this.m_socket = null;
		}
		this.m_endpoint = null;
	}

	// Token: 0x06000F8A RID: 3978 RVA: 0x000670EC File Offset: 0x000652EC
	public static IPEndPoint GetEndPoint(string host, int port)
	{
		return new IPEndPoint(Dns.GetHostEntry(host).AddressList[0], port);
	}

	// Token: 0x06000F8B RID: 3979 RVA: 0x00067A69 File Offset: 0x00065C69
	public bool StartHost(int port)
	{
		if (this.m_listner != null)
		{
			this.m_listner.Stop();
			this.m_listner = null;
		}
		if (!this.BindSocket(port, port + 10))
		{
			ZLog.LogWarning("Failed to bind socket");
			return false;
		}
		return true;
	}

	// Token: 0x06000F8C RID: 3980 RVA: 0x00067AA0 File Offset: 0x00065CA0
	private bool BindSocket(int startPort, int endPort)
	{
		for (int i = startPort; i <= endPort; i++)
		{
			try
			{
				this.m_listner = new TcpListener(IPAddress.Any, i);
				this.m_listner.Start();
				this.m_listenPort = i;
				ZLog.Log("Bound socket port " + i.ToString());
				return true;
			}
			catch
			{
				ZLog.Log("Failed to bind port:" + i.ToString());
				this.m_listner = null;
			}
		}
		return false;
	}

	// Token: 0x06000F8D RID: 3981 RVA: 0x00067B2C File Offset: 0x00065D2C
	private void BeginReceive()
	{
		this.m_recvSizeOffset = 0;
		this.m_socket.GetStream().BeginRead(this.m_recvSizeBuffer, 0, this.m_recvSizeBuffer.Length, new AsyncCallback(this.PkgSizeReceived), this.m_socket);
	}

	// Token: 0x06000F8E RID: 3982 RVA: 0x00067B68 File Offset: 0x00065D68
	private void PkgSizeReceived(IAsyncResult res)
	{
		if (this.m_socket == null || !this.m_socket.Connected)
		{
			ZLog.LogWarning("PkgSizeReceived socket closed");
			this.Close();
			return;
		}
		int num;
		try
		{
			num = this.m_socket.GetStream().EndRead(res);
		}
		catch (Exception ex)
		{
			ZLog.LogWarning("PkgSizeReceived exception " + ex.ToString());
			this.Close();
			return;
		}
		if (num == 0)
		{
			ZLog.LogWarning("PkgSizeReceived Got 0 bytes data,closing socket");
			this.Close();
			return;
		}
		this.m_gotData = true;
		this.m_recvSizeOffset += num;
		if (this.m_recvSizeOffset < this.m_recvSizeBuffer.Length)
		{
			int count = this.m_recvSizeBuffer.Length - this.m_recvOffset;
			this.m_socket.GetStream().BeginRead(this.m_recvSizeBuffer, this.m_recvSizeOffset, count, new AsyncCallback(this.PkgSizeReceived), this.m_socket);
			return;
		}
		int num2 = BitConverter.ToInt32(this.m_recvSizeBuffer, 0);
		if (num2 == 0 || num2 > 10485760)
		{
			ZLog.LogError("PkgSizeReceived Invalid pkg size " + num2.ToString());
			return;
		}
		this.m_lastRecvPkgSize = num2;
		this.m_recvOffset = 0;
		this.m_lastRecvPkgSize = num2;
		if (this.m_recvBuffer == null)
		{
			this.m_recvBuffer = new byte[ZSocket2.m_maxRecvBuffer];
		}
		this.m_socket.GetStream().BeginRead(this.m_recvBuffer, this.m_recvOffset, this.m_lastRecvPkgSize, new AsyncCallback(this.PkgReceived), this.m_socket);
	}

	// Token: 0x06000F8F RID: 3983 RVA: 0x00067CEC File Offset: 0x00065EEC
	private void PkgReceived(IAsyncResult res)
	{
		int num;
		try
		{
			num = this.m_socket.GetStream().EndRead(res);
		}
		catch (Exception ex)
		{
			ZLog.Log("PkgReceived error " + ex.ToString());
			this.Close();
			return;
		}
		if (num == 0)
		{
			ZLog.LogWarning("PkgReceived: Got 0 bytes data,closing socket");
			this.Close();
			return;
		}
		this.m_gotData = true;
		this.m_totalRecv += num;
		this.m_recvOffset += num;
		base.IncRecvBytes(num);
		if (this.m_recvOffset < this.m_lastRecvPkgSize)
		{
			int count = this.m_lastRecvPkgSize - this.m_recvOffset;
			if (this.m_recvBuffer == null)
			{
				this.m_recvBuffer = new byte[ZSocket2.m_maxRecvBuffer];
			}
			this.m_socket.GetStream().BeginRead(this.m_recvBuffer, this.m_recvOffset, count, new AsyncCallback(this.PkgReceived), this.m_socket);
			return;
		}
		ZPackage item = new ZPackage(this.m_recvBuffer, this.m_lastRecvPkgSize);
		this.m_mutex.WaitOne();
		this.m_pkgQueue.Enqueue(item);
		this.m_mutex.ReleaseMutex();
		this.BeginReceive();
	}

	// Token: 0x06000F90 RID: 3984 RVA: 0x00067E1C File Offset: 0x0006601C
	public ISocket Accept()
	{
		if (this.m_listner == null)
		{
			return null;
		}
		if (!this.m_listner.Pending())
		{
			return null;
		}
		TcpClient socket = this.m_listner.AcceptTcpClient();
		ZSocket2.ConfigureSocket(socket);
		return new ZSocket2(socket, null);
	}

	// Token: 0x06000F91 RID: 3985 RVA: 0x00067E4E File Offset: 0x0006604E
	public bool IsConnected()
	{
		return this.m_socket != null && this.m_socket.Connected;
	}

	// Token: 0x06000F92 RID: 3986 RVA: 0x00067E68 File Offset: 0x00066068
	public void Send(ZPackage pkg)
	{
		if (pkg.Size() == 0)
		{
			return;
		}
		if (this.m_socket == null || !this.m_socket.Connected)
		{
			return;
		}
		byte[] array = pkg.GetArray();
		byte[] bytes = BitConverter.GetBytes(array.Length);
		byte[] array2 = new byte[array.Length + bytes.Length];
		bytes.CopyTo(array2, 0);
		array.CopyTo(array2, 4);
		base.IncSentBytes(array.Length);
		this.m_sendMutex.WaitOne();
		if (!this.m_isSending)
		{
			if (array2.Length > 10485760)
			{
				ZLog.LogError("Too big data package: " + array2.Length.ToString());
			}
			try
			{
				this.m_totalSent += array2.Length;
				this.m_socket.GetStream().BeginWrite(array2, 0, array2.Length, new AsyncCallback(this.PkgSent), this.m_socket);
				this.m_isSending = true;
				goto IL_105;
			}
			catch (Exception ex)
			{
				string str = "Handled exception in ZSocket:Send:";
				Exception ex2 = ex;
				ZLog.Log(str + ((ex2 != null) ? ex2.ToString() : null));
				this.Close();
				goto IL_105;
			}
		}
		this.m_sendQueue.Enqueue(array2);
		IL_105:
		this.m_sendMutex.ReleaseMutex();
	}

	// Token: 0x06000F93 RID: 3987 RVA: 0x00067F98 File Offset: 0x00066198
	private void PkgSent(IAsyncResult res)
	{
		try
		{
			this.m_socket.GetStream().EndWrite(res);
		}
		catch (Exception ex)
		{
			ZLog.Log("PkgSent error " + ex.ToString());
			this.Close();
			return;
		}
		this.m_sendMutex.WaitOne();
		if (this.m_sendQueue.Count > 0 && this.IsConnected())
		{
			byte[] array = this.m_sendQueue.Dequeue();
			try
			{
				this.m_totalSent += array.Length;
				this.m_socket.GetStream().BeginWrite(array, 0, array.Length, new AsyncCallback(this.PkgSent), this.m_socket);
				goto IL_CF;
			}
			catch (Exception ex2)
			{
				string str = "Handled exception in pkgsent:";
				Exception ex3 = ex2;
				ZLog.Log(str + ((ex3 != null) ? ex3.ToString() : null));
				this.m_isSending = false;
				this.Close();
				goto IL_CF;
			}
		}
		this.m_isSending = false;
		IL_CF:
		this.m_sendMutex.ReleaseMutex();
	}

	// Token: 0x06000F94 RID: 3988 RVA: 0x0006809C File Offset: 0x0006629C
	public ZPackage Recv()
	{
		if (this.m_socket == null)
		{
			return null;
		}
		if (this.m_pkgQueue.Count == 0)
		{
			return null;
		}
		ZPackage result = null;
		this.m_mutex.WaitOne();
		if (this.m_pkgQueue.Count > 0)
		{
			result = this.m_pkgQueue.Dequeue();
		}
		this.m_mutex.ReleaseMutex();
		return result;
	}

	// Token: 0x06000F95 RID: 3989 RVA: 0x000680F6 File Offset: 0x000662F6
	public string GetEndPointString()
	{
		if (this.m_endpoint != null)
		{
			return this.m_endpoint.ToString();
		}
		return "None";
	}

	// Token: 0x06000F96 RID: 3990 RVA: 0x00068111 File Offset: 0x00066311
	public string GetHostName()
	{
		if (this.m_endpoint != null)
		{
			return this.m_endpoint.Address.ToString();
		}
		return "None";
	}

	// Token: 0x06000F97 RID: 3991 RVA: 0x00068131 File Offset: 0x00066331
	public IPEndPoint GetEndPoint()
	{
		return this.m_endpoint;
	}

	// Token: 0x06000F98 RID: 3992 RVA: 0x0006813C File Offset: 0x0006633C
	public bool IsPeer(string host, int port)
	{
		if (!this.IsConnected())
		{
			return false;
		}
		if (this.m_endpoint == null)
		{
			return false;
		}
		IPEndPoint endpoint = this.m_endpoint;
		return (endpoint.Address.ToString() == host && endpoint.Port == port) || (this.m_originalHostName != null && this.m_originalHostName == host && endpoint.Port == port);
	}

	// Token: 0x06000F99 RID: 3993 RVA: 0x000681A4 File Offset: 0x000663A4
	public bool IsHost()
	{
		return this.m_listenPort != 0;
	}

	// Token: 0x06000F9A RID: 3994 RVA: 0x000681AF File Offset: 0x000663AF
	public int GetHostPort()
	{
		return this.m_listenPort;
	}

	// Token: 0x06000F9B RID: 3995 RVA: 0x000681B8 File Offset: 0x000663B8
	public int GetSendQueueSize()
	{
		if (!this.IsConnected())
		{
			return 0;
		}
		this.m_sendMutex.WaitOne();
		int num = 0;
		foreach (byte[] array in this.m_sendQueue)
		{
			num += array.Length;
		}
		this.m_sendMutex.ReleaseMutex();
		return num;
	}

	// Token: 0x06000F9C RID: 3996 RVA: 0x00068230 File Offset: 0x00066430
	public bool IsSending()
	{
		return this.m_isSending || this.m_sendQueue.Count > 0;
	}

	// Token: 0x06000F9D RID: 3997 RVA: 0x0006824A File Offset: 0x0006644A
	public bool GotNewData()
	{
		bool gotData = this.m_gotData;
		this.m_gotData = false;
		return gotData;
	}

	// Token: 0x06000F9E RID: 3998 RVA: 0x0000290F File Offset: 0x00000B0F
	public bool Flush()
	{
		return true;
	}

	// Token: 0x06000F9F RID: 3999 RVA: 0x0000247B File Offset: 0x0000067B
	public int GetCurrentSendRate()
	{
		return 0;
	}

	// Token: 0x06000FA0 RID: 4000 RVA: 0x0000247B File Offset: 0x0000067B
	public int GetAverageSendRate()
	{
		return 0;
	}

	// Token: 0x06000FA1 RID: 4001 RVA: 0x000023E2 File Offset: 0x000005E2
	public void VersionMatch()
	{
	}

	// Token: 0x040010BC RID: 4284
	private TcpListener m_listner;

	// Token: 0x040010BD RID: 4285
	private TcpClient m_socket;

	// Token: 0x040010BE RID: 4286
	private Mutex m_mutex = new Mutex();

	// Token: 0x040010BF RID: 4287
	private Mutex m_sendMutex = new Mutex();

	// Token: 0x040010C0 RID: 4288
	private static int m_maxRecvBuffer = 10485760;

	// Token: 0x040010C1 RID: 4289
	private int m_recvOffset;

	// Token: 0x040010C2 RID: 4290
	private byte[] m_recvBuffer;

	// Token: 0x040010C3 RID: 4291
	private int m_recvSizeOffset;

	// Token: 0x040010C4 RID: 4292
	private byte[] m_recvSizeBuffer = new byte[4];

	// Token: 0x040010C5 RID: 4293
	private Queue<ZPackage> m_pkgQueue = new Queue<ZPackage>();

	// Token: 0x040010C6 RID: 4294
	private bool m_isSending;

	// Token: 0x040010C7 RID: 4295
	private Queue<byte[]> m_sendQueue = new Queue<byte[]>();

	// Token: 0x040010C8 RID: 4296
	private IPEndPoint m_endpoint;

	// Token: 0x040010C9 RID: 4297
	private string m_originalHostName;

	// Token: 0x040010CA RID: 4298
	private int m_listenPort;

	// Token: 0x040010CB RID: 4299
	private int m_lastRecvPkgSize;

	// Token: 0x040010CC RID: 4300
	private int m_totalSent;

	// Token: 0x040010CD RID: 4301
	private int m_totalRecv;

	// Token: 0x040010CE RID: 4302
	private bool m_gotData;
}
