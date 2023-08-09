using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// Token: 0x02000186 RID: 390
public class ZSocket : IDisposable
{
	// Token: 0x06000F68 RID: 3944 RVA: 0x00066F58 File Offset: 0x00065158
	public ZSocket()
	{
		this.m_socket = ZSocket.CreateSocket();
	}

	// Token: 0x06000F69 RID: 3945 RVA: 0x00066FB9 File Offset: 0x000651B9
	public static Socket CreateSocket()
	{
		return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
		{
			NoDelay = true
		};
	}

	// Token: 0x06000F6A RID: 3946 RVA: 0x00066FCC File Offset: 0x000651CC
	public ZSocket(Socket socket, string originalHostName = null)
	{
		this.m_socket = socket;
		this.m_originalHostName = originalHostName;
		try
		{
			this.m_endpoint = (this.m_socket.RemoteEndPoint as IPEndPoint);
		}
		catch
		{
			this.Close();
			return;
		}
		this.BeginReceive();
	}

	// Token: 0x06000F6B RID: 3947 RVA: 0x00067068 File Offset: 0x00065268
	public void Dispose()
	{
		this.Close();
		this.m_mutex.Close();
		this.m_sendMutex.Close();
		this.m_recvBuffer = null;
	}

	// Token: 0x06000F6C RID: 3948 RVA: 0x00067090 File Offset: 0x00065290
	public void Close()
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
			catch (Exception)
			{
			}
			this.m_socket.Close();
		}
		this.m_socket = null;
		this.m_endpoint = null;
	}

	// Token: 0x06000F6D RID: 3949 RVA: 0x000670EC File Offset: 0x000652EC
	public static IPEndPoint GetEndPoint(string host, int port)
	{
		return new IPEndPoint(Dns.GetHostEntry(host).AddressList[0], port);
	}

	// Token: 0x06000F6E RID: 3950 RVA: 0x00067104 File Offset: 0x00065304
	public bool Connect(string host, int port)
	{
		ZLog.Log("Connecting to " + host + " : " + port.ToString());
		IPEndPoint endPoint = ZSocket.GetEndPoint(host, port);
		this.m_socket.BeginConnect(endPoint, null, null).AsyncWaitHandle.WaitOne(3000, true);
		if (!this.m_socket.Connected)
		{
			return false;
		}
		try
		{
			this.m_endpoint = (this.m_socket.RemoteEndPoint as IPEndPoint);
		}
		catch
		{
			this.Close();
			return false;
		}
		this.BeginReceive();
		ZLog.Log(" connected");
		return true;
	}

	// Token: 0x06000F6F RID: 3951 RVA: 0x000671AC File Offset: 0x000653AC
	public bool StartHost(int port)
	{
		if (this.m_listenPort != 0)
		{
			this.Close();
		}
		if (!this.BindSocket(this.m_socket, IPAddress.Any, port, port + 10))
		{
			ZLog.LogWarning("Failed to bind socket");
			return false;
		}
		this.m_socket.Listen(100);
		this.m_socket.BeginAccept(new AsyncCallback(this.AcceptCallback), this.m_socket);
		return true;
	}

	// Token: 0x06000F70 RID: 3952 RVA: 0x00067218 File Offset: 0x00065418
	private bool BindSocket(Socket socket, IPAddress ipAddress, int startPort, int endPort)
	{
		for (int i = startPort; i <= endPort; i++)
		{
			try
			{
				IPEndPoint localEP = new IPEndPoint(ipAddress, i);
				this.m_socket.Bind(localEP);
				this.m_listenPort = i;
				ZLog.Log("Bound socket port " + i.ToString());
				return true;
			}
			catch
			{
				ZLog.Log("Failed to bind port:" + i.ToString());
			}
		}
		return false;
	}

	// Token: 0x06000F71 RID: 3953 RVA: 0x00067294 File Offset: 0x00065494
	private void BeginReceive()
	{
		this.m_socket.BeginReceive(this.m_recvSizeBuffer, 0, this.m_recvSizeBuffer.Length, SocketFlags.None, new AsyncCallback(this.PkgSizeReceived), this.m_socket);
	}

	// Token: 0x06000F72 RID: 3954 RVA: 0x000672C4 File Offset: 0x000654C4
	private void PkgSizeReceived(IAsyncResult res)
	{
		int num;
		try
		{
			num = this.m_socket.EndReceive(res);
		}
		catch (Exception)
		{
			this.Disconnect();
			return;
		}
		this.m_totalRecv += num;
		if (num != 4)
		{
			this.Disconnect();
			return;
		}
		int num2 = BitConverter.ToInt32(this.m_recvSizeBuffer, 0);
		if (num2 == 0 || num2 > 10485760)
		{
			ZLog.LogError("Invalid pkg size " + num2.ToString());
			return;
		}
		this.m_lastRecvPkgSize = num2;
		this.m_recvOffset = 0;
		this.m_lastRecvPkgSize = num2;
		if (this.m_recvBuffer == null)
		{
			this.m_recvBuffer = new byte[ZSocket.m_maxRecvBuffer];
		}
		this.m_socket.BeginReceive(this.m_recvBuffer, this.m_recvOffset, this.m_lastRecvPkgSize, SocketFlags.None, new AsyncCallback(this.PkgReceived), this.m_socket);
	}

	// Token: 0x06000F73 RID: 3955 RVA: 0x000673A4 File Offset: 0x000655A4
	private void Disconnect()
	{
		if (this.m_socket != null)
		{
			try
			{
				this.m_socket.Disconnect(true);
			}
			catch
			{
			}
		}
	}

	// Token: 0x06000F74 RID: 3956 RVA: 0x000673DC File Offset: 0x000655DC
	private void PkgReceived(IAsyncResult res)
	{
		int num;
		try
		{
			num = this.m_socket.EndReceive(res);
		}
		catch (Exception)
		{
			this.Disconnect();
			return;
		}
		this.m_totalRecv += num;
		this.m_recvOffset += num;
		if (this.m_recvOffset < this.m_lastRecvPkgSize)
		{
			int size = this.m_lastRecvPkgSize - this.m_recvOffset;
			if (this.m_recvBuffer == null)
			{
				this.m_recvBuffer = new byte[ZSocket.m_maxRecvBuffer];
			}
			this.m_socket.BeginReceive(this.m_recvBuffer, this.m_recvOffset, size, SocketFlags.None, new AsyncCallback(this.PkgReceived), this.m_socket);
			return;
		}
		ZPackage item = new ZPackage(this.m_recvBuffer, this.m_lastRecvPkgSize);
		this.m_mutex.WaitOne();
		this.m_pkgQueue.Enqueue(item);
		this.m_mutex.ReleaseMutex();
		this.BeginReceive();
	}

	// Token: 0x06000F75 RID: 3957 RVA: 0x000674CC File Offset: 0x000656CC
	private void AcceptCallback(IAsyncResult res)
	{
		Socket item;
		try
		{
			item = this.m_socket.EndAccept(res);
		}
		catch
		{
			this.Disconnect();
			return;
		}
		this.m_mutex.WaitOne();
		this.m_newConnections.Enqueue(item);
		this.m_mutex.ReleaseMutex();
		this.m_socket.BeginAccept(new AsyncCallback(this.AcceptCallback), this.m_socket);
	}

	// Token: 0x06000F76 RID: 3958 RVA: 0x00067544 File Offset: 0x00065744
	public ZSocket Accept()
	{
		if (this.m_newConnections.Count == 0)
		{
			return null;
		}
		Socket socket = null;
		this.m_mutex.WaitOne();
		if (this.m_newConnections.Count > 0)
		{
			socket = this.m_newConnections.Dequeue();
		}
		this.m_mutex.ReleaseMutex();
		if (socket != null)
		{
			return new ZSocket(socket, null);
		}
		return null;
	}

	// Token: 0x06000F77 RID: 3959 RVA: 0x0006759F File Offset: 0x0006579F
	public bool IsConnected()
	{
		return this.m_socket != null && this.m_socket.Connected;
	}

	// Token: 0x06000F78 RID: 3960 RVA: 0x000675B8 File Offset: 0x000657B8
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
		this.m_sendMutex.WaitOne();
		if (!this.m_isSending)
		{
			if (array.Length > 10485760)
			{
				ZLog.LogError("Too big data package: " + array.Length.ToString());
			}
			try
			{
				this.m_totalSent += bytes.Length;
				this.m_socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(this.PkgSent), null);
				this.m_isSending = true;
				this.m_sendQueue.Enqueue(array);
				goto IL_EC;
			}
			catch (Exception ex)
			{
				string str = "Handled exception in ZSocket:Send:";
				Exception ex2 = ex;
				ZLog.Log(str + ((ex2 != null) ? ex2.ToString() : null));
				this.Disconnect();
				goto IL_EC;
			}
		}
		this.m_sendQueue.Enqueue(bytes);
		this.m_sendQueue.Enqueue(array);
		IL_EC:
		this.m_sendMutex.ReleaseMutex();
	}

	// Token: 0x06000F79 RID: 3961 RVA: 0x000676CC File Offset: 0x000658CC
	private void PkgSent(IAsyncResult res)
	{
		this.m_sendMutex.WaitOne();
		if (this.m_sendQueue.Count > 0 && this.IsConnected())
		{
			byte[] array = this.m_sendQueue.Dequeue();
			try
			{
				this.m_totalSent += array.Length;
				this.m_socket.BeginSend(array, 0, array.Length, SocketFlags.None, new AsyncCallback(this.PkgSent), null);
				goto IL_92;
			}
			catch (Exception ex)
			{
				string str = "Handled exception in pkgsent:";
				Exception ex2 = ex;
				ZLog.Log(str + ((ex2 != null) ? ex2.ToString() : null));
				this.m_isSending = false;
				this.Disconnect();
				goto IL_92;
			}
		}
		this.m_isSending = false;
		IL_92:
		this.m_sendMutex.ReleaseMutex();
	}

	// Token: 0x06000F7A RID: 3962 RVA: 0x00067788 File Offset: 0x00065988
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

	// Token: 0x06000F7B RID: 3963 RVA: 0x000677E2 File Offset: 0x000659E2
	public string GetEndPointString()
	{
		if (this.m_endpoint != null)
		{
			return this.m_endpoint.ToString();
		}
		return "None";
	}

	// Token: 0x06000F7C RID: 3964 RVA: 0x000677FD File Offset: 0x000659FD
	public string GetEndPointHost()
	{
		if (this.m_endpoint != null)
		{
			return this.m_endpoint.Address.ToString();
		}
		return "None";
	}

	// Token: 0x06000F7D RID: 3965 RVA: 0x0006781D File Offset: 0x00065A1D
	public IPEndPoint GetEndPoint()
	{
		return this.m_endpoint;
	}

	// Token: 0x06000F7E RID: 3966 RVA: 0x00067828 File Offset: 0x00065A28
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

	// Token: 0x06000F7F RID: 3967 RVA: 0x00067890 File Offset: 0x00065A90
	public bool IsHost()
	{
		return this.m_listenPort != 0;
	}

	// Token: 0x06000F80 RID: 3968 RVA: 0x0006789B File Offset: 0x00065A9B
	public int GetHostPort()
	{
		return this.m_listenPort;
	}

	// Token: 0x06000F81 RID: 3969 RVA: 0x000678A3 File Offset: 0x00065AA3
	public bool IsSending()
	{
		return this.m_isSending || this.m_sendQueue.Count > 0;
	}

	// Token: 0x06000F82 RID: 3970 RVA: 0x000678BD File Offset: 0x00065ABD
	public void GetAndResetStats(out int totalSent, out int totalRecv)
	{
		totalSent = this.m_totalSent;
		totalRecv = this.m_totalRecv;
		this.m_totalSent = 0;
		this.m_totalRecv = 0;
	}

	// Token: 0x040010AB RID: 4267
	private Socket m_socket;

	// Token: 0x040010AC RID: 4268
	private Mutex m_mutex = new Mutex();

	// Token: 0x040010AD RID: 4269
	private Mutex m_sendMutex = new Mutex();

	// Token: 0x040010AE RID: 4270
	private Queue<Socket> m_newConnections = new Queue<Socket>();

	// Token: 0x040010AF RID: 4271
	private static int m_maxRecvBuffer = 10485760;

	// Token: 0x040010B0 RID: 4272
	private int m_recvOffset;

	// Token: 0x040010B1 RID: 4273
	private byte[] m_recvBuffer;

	// Token: 0x040010B2 RID: 4274
	private byte[] m_recvSizeBuffer = new byte[4];

	// Token: 0x040010B3 RID: 4275
	private Queue<ZPackage> m_pkgQueue = new Queue<ZPackage>();

	// Token: 0x040010B4 RID: 4276
	private bool m_isSending;

	// Token: 0x040010B5 RID: 4277
	private Queue<byte[]> m_sendQueue = new Queue<byte[]>();

	// Token: 0x040010B6 RID: 4278
	private IPEndPoint m_endpoint;

	// Token: 0x040010B7 RID: 4279
	private string m_originalHostName;

	// Token: 0x040010B8 RID: 4280
	private int m_listenPort;

	// Token: 0x040010B9 RID: 4281
	private int m_lastRecvPkgSize;

	// Token: 0x040010BA RID: 4282
	private int m_totalSent;

	// Token: 0x040010BB RID: 4283
	private int m_totalRecv;
}
