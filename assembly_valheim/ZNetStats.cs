using System;
using UnityEngine;

// Token: 0x0200016D RID: 365
public class ZNetStats
{
	// Token: 0x06000EB4 RID: 3764 RVA: 0x00064DA6 File Offset: 0x00062FA6
	internal void IncRecvBytes(int count)
	{
		this.m_recvBytes += count;
	}

	// Token: 0x06000EB5 RID: 3765 RVA: 0x00064DB6 File Offset: 0x00062FB6
	internal void IncSentBytes(int count)
	{
		this.m_sentBytes += count;
	}

	// Token: 0x06000EB6 RID: 3766 RVA: 0x00064DC6 File Offset: 0x00062FC6
	public void GetAndResetStats(out int totalSent, out int totalRecv)
	{
		totalSent = this.m_sentBytes;
		totalRecv = this.m_recvBytes;
		this.m_sentBytes = 0;
		this.m_statSentBytes = 0;
		this.m_recvBytes = 0;
		this.m_statRecvBytes = 0;
		this.m_statStart = Time.time;
	}

	// Token: 0x06000EB7 RID: 3767 RVA: 0x00064E00 File Offset: 0x00063000
	public void GetConnectionQuality(out float localQuality, out float remoteQuality, out int ping, out float outByteSec, out float inByteSec)
	{
		float num = Time.time - this.m_statStart;
		if (num >= 1f)
		{
			this.m_sendRate = ((float)(this.m_sentBytes - this.m_statSentBytes) / num * 2f + this.m_sendRate) / 3f;
			this.m_recvRate = ((float)(this.m_recvBytes - this.m_statRecvBytes) / num * 2f + this.m_recvRate) / 3f;
			this.m_statSentBytes = this.m_sentBytes;
			this.m_statRecvBytes = this.m_recvBytes;
			this.m_statStart = Time.time;
		}
		localQuality = 0f;
		remoteQuality = 0f;
		ping = 0;
		outByteSec = this.m_sendRate;
		inByteSec = this.m_recvRate;
	}

	// Token: 0x04001065 RID: 4197
	private int m_recvBytes;

	// Token: 0x04001066 RID: 4198
	private int m_statRecvBytes;

	// Token: 0x04001067 RID: 4199
	private int m_sentBytes;

	// Token: 0x04001068 RID: 4200
	private int m_statSentBytes;

	// Token: 0x04001069 RID: 4201
	private float m_recvRate;

	// Token: 0x0400106A RID: 4202
	private float m_sendRate;

	// Token: 0x0400106B RID: 4203
	private float m_statStart = Time.time;
}
