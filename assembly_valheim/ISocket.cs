using System;

// Token: 0x02000133 RID: 307
public interface ISocket
{
	// Token: 0x06000BE7 RID: 3047
	bool IsConnected();

	// Token: 0x06000BE8 RID: 3048
	void Send(ZPackage pkg);

	// Token: 0x06000BE9 RID: 3049
	ZPackage Recv();

	// Token: 0x06000BEA RID: 3050
	int GetSendQueueSize();

	// Token: 0x06000BEB RID: 3051
	int GetCurrentSendRate();

	// Token: 0x06000BEC RID: 3052
	bool IsHost();

	// Token: 0x06000BED RID: 3053
	void Dispose();

	// Token: 0x06000BEE RID: 3054
	bool GotNewData();

	// Token: 0x06000BEF RID: 3055
	void Close();

	// Token: 0x06000BF0 RID: 3056
	string GetEndPointString();

	// Token: 0x06000BF1 RID: 3057
	void GetAndResetStats(out int totalSent, out int totalRecv);

	// Token: 0x06000BF2 RID: 3058
	void GetConnectionQuality(out float localQuality, out float remoteQuality, out int ping, out float outByteSec, out float inByteSec);

	// Token: 0x06000BF3 RID: 3059
	ISocket Accept();

	// Token: 0x06000BF4 RID: 3060
	int GetHostPort();

	// Token: 0x06000BF5 RID: 3061
	bool Flush();

	// Token: 0x06000BF6 RID: 3062
	string GetHostName();

	// Token: 0x06000BF7 RID: 3063
	void VersionMatch();
}
