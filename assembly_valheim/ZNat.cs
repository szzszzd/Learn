using System;
using System.IO;

// Token: 0x02000164 RID: 356
public class ZNat : IDisposable
{
	// Token: 0x06000DFB RID: 3579 RVA: 0x000023E2 File Offset: 0x000005E2
	public void Dispose()
	{
	}

	// Token: 0x06000DFC RID: 3580 RVA: 0x00060900 File Offset: 0x0005EB00
	public void SetPort(int port)
	{
		if (this.m_port == port)
		{
			return;
		}
		this.m_port = port;
	}

	// Token: 0x06000DFD RID: 3581 RVA: 0x000023E2 File Offset: 0x000005E2
	public void Update(float dt)
	{
	}

	// Token: 0x06000DFE RID: 3582 RVA: 0x00060913 File Offset: 0x0005EB13
	public bool GetStatus()
	{
		return this.m_mappingOK;
	}

	// Token: 0x04001001 RID: 4097
	private FileStream m_output;

	// Token: 0x04001002 RID: 4098
	private bool m_mappingOK;

	// Token: 0x04001003 RID: 4099
	private int m_port;
}
