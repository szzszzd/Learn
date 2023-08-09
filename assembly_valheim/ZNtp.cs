using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// Token: 0x02000178 RID: 376
public class ZNtp : IDisposable
{
	// Token: 0x1700009C RID: 156
	// (get) Token: 0x06000EE7 RID: 3815 RVA: 0x00065531 File Offset: 0x00063731
	public static ZNtp instance
	{
		get
		{
			return ZNtp.m_instance;
		}
	}

	// Token: 0x06000EE8 RID: 3816 RVA: 0x00065538 File Offset: 0x00063738
	public ZNtp()
	{
		ZNtp.m_instance = this;
		this.m_ntpTime = DateTime.UtcNow;
		this.m_ntpThread = new Thread(new ThreadStart(this.NtpThread));
		this.m_ntpThread.Start();
	}

	// Token: 0x06000EE9 RID: 3817 RVA: 0x0006558C File Offset: 0x0006378C
	public void Dispose()
	{
		if (this.m_ntpThread != null)
		{
			ZLog.Log("Stoping ntp thread");
			this.m_lock.WaitOne();
			this.m_stop = true;
			this.m_ntpThread.Abort();
			this.m_lock.ReleaseMutex();
			this.m_ntpThread = null;
		}
		if (this.m_lock != null)
		{
			this.m_lock.Close();
			this.m_lock = null;
		}
	}

	// Token: 0x06000EEA RID: 3818 RVA: 0x000655F5 File Offset: 0x000637F5
	public bool GetStatus()
	{
		return this.m_status;
	}

	// Token: 0x06000EEB RID: 3819 RVA: 0x000655FD File Offset: 0x000637FD
	public void Update(float dt)
	{
		this.m_lock.WaitOne();
		this.m_ntpTime = this.m_ntpTime.AddSeconds((double)dt);
		this.m_lock.ReleaseMutex();
	}

	// Token: 0x06000EEC RID: 3820 RVA: 0x0006562C File Offset: 0x0006382C
	private void NtpThread()
	{
		while (!this.m_stop)
		{
			DateTime ntpTime;
			if (this.GetNetworkTime("pool.ntp.org", out ntpTime))
			{
				this.m_status = true;
				this.m_lock.WaitOne();
				this.m_ntpTime = ntpTime;
				this.m_lock.ReleaseMutex();
			}
			else
			{
				this.m_status = false;
			}
			Thread.Sleep(60000);
		}
	}

	// Token: 0x06000EED RID: 3821 RVA: 0x0006568A File Offset: 0x0006388A
	public DateTime GetTime()
	{
		return this.m_ntpTime;
	}

	// Token: 0x06000EEE RID: 3822 RVA: 0x00065694 File Offset: 0x00063894
	private bool GetNetworkTime(string ntpServer, out DateTime time)
	{
		byte[] array = new byte[48];
		array[0] = 27;
		IPAddress[] addressList;
		try
		{
			addressList = Dns.GetHostEntry(ntpServer).AddressList;
			if (addressList.Length == 0)
			{
				ZLog.Log("Dns lookup failed");
				time = DateTime.UtcNow;
				return false;
			}
		}
		catch
		{
			ZLog.Log("Failed ntp dns lookup");
			time = DateTime.UtcNow;
			return false;
		}
		IPEndPoint remoteEP = new IPEndPoint(addressList[0], 123);
		Socket socket = null;
		try
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.ReceiveTimeout = 3000;
			socket.SendTimeout = 3000;
			socket.Connect(remoteEP);
			if (!socket.Connected)
			{
				ZLog.Log("Failed to connect to ntp");
				time = DateTime.UtcNow;
				socket.Close();
				return false;
			}
			socket.Send(array);
			socket.Receive(array);
			socket.Shutdown(SocketShutdown.Both);
			socket.Close();
		}
		catch
		{
			if (socket != null)
			{
				socket.Close();
			}
			time = DateTime.UtcNow;
			return false;
		}
		ulong num = (ulong)array[40] << 24 | (ulong)array[41] << 16 | (ulong)array[42] << 8 | (ulong)array[43];
		ulong num2 = (ulong)array[44] << 24 | (ulong)array[45] << 16 | (ulong)array[46] << 8 | (ulong)array[47];
		ulong num3 = num * 1000UL + num2 * 1000UL / 4294967296UL;
		time = new DateTime(1900, 1, 1).AddMilliseconds((double)num3);
		return true;
	}

	// Token: 0x0400107F RID: 4223
	private static ZNtp m_instance;

	// Token: 0x04001080 RID: 4224
	private DateTime m_ntpTime;

	// Token: 0x04001081 RID: 4225
	private bool m_status;

	// Token: 0x04001082 RID: 4226
	private bool m_stop;

	// Token: 0x04001083 RID: 4227
	private Thread m_ntpThread;

	// Token: 0x04001084 RID: 4228
	private Mutex m_lock = new Mutex();
}
