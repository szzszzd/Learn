using System;
using System.Collections.Generic;
using System.Threading;
using Ionic.Zlib;

// Token: 0x02000198 RID: 408
public class PlayFabZLibWorkQueue : IDisposable
{
	// Token: 0x0600107A RID: 4218 RVA: 0x0006C3A4 File Offset: 0x0006A5A4
	public PlayFabZLibWorkQueue()
	{
		PlayFabZLibWorkQueue.s_workersMutex.WaitOne();
		if (PlayFabZLibWorkQueue.s_thread == null)
		{
			PlayFabZLibWorkQueue.s_thread = new Thread(new ThreadStart(this.WorkerMain));
			PlayFabZLibWorkQueue.s_thread.Name = "PlayfabZlibThread";
			PlayFabZLibWorkQueue.s_thread.Start();
		}
		PlayFabZLibWorkQueue.s_workers.Add(this);
		PlayFabZLibWorkQueue.s_workersMutex.ReleaseMutex();
	}

	// Token: 0x0600107B RID: 4219 RVA: 0x0006C444 File Offset: 0x0006A644
	public void Compress(byte[] buffer)
	{
		this.m_buffersMutex.WaitOne();
		this.m_inCompress.Enqueue(buffer);
		this.m_buffersMutex.ReleaseMutex();
		if (PlayFabZLibWorkQueue.s_workSemaphore.CurrentCount < 1)
		{
			PlayFabZLibWorkQueue.s_workSemaphore.Release();
		}
	}

	// Token: 0x0600107C RID: 4220 RVA: 0x0006C481 File Offset: 0x0006A681
	public void Decompress(byte[] buffer)
	{
		this.m_buffersMutex.WaitOne();
		this.m_inDecompress.Enqueue(buffer);
		this.m_buffersMutex.ReleaseMutex();
		if (PlayFabZLibWorkQueue.s_workSemaphore.CurrentCount < 1)
		{
			PlayFabZLibWorkQueue.s_workSemaphore.Release();
		}
	}

	// Token: 0x0600107D RID: 4221 RVA: 0x0006C4C0 File Offset: 0x0006A6C0
	public void Poll(out List<byte[]> compressedBuffers, out List<byte[]> decompressedBuffers)
	{
		compressedBuffers = null;
		decompressedBuffers = null;
		this.m_buffersMutex.WaitOne();
		if (this.m_outCompress.Count > 0)
		{
			compressedBuffers = new List<byte[]>();
			while (this.m_outCompress.Count > 0)
			{
				compressedBuffers.Add(this.m_outCompress.Dequeue());
			}
		}
		if (this.m_outDecompress.Count > 0)
		{
			decompressedBuffers = new List<byte[]>();
			while (this.m_outDecompress.Count > 0)
			{
				decompressedBuffers.Add(this.m_outDecompress.Dequeue());
			}
		}
		this.m_buffersMutex.ReleaseMutex();
	}

	// Token: 0x0600107E RID: 4222 RVA: 0x0006C558 File Offset: 0x0006A758
	private void WorkerMain()
	{
		for (;;)
		{
			PlayFabZLibWorkQueue.s_workSemaphore.Wait();
			PlayFabZLibWorkQueue.s_workersMutex.WaitOne();
			foreach (PlayFabZLibWorkQueue playFabZLibWorkQueue in PlayFabZLibWorkQueue.s_workers)
			{
				playFabZLibWorkQueue.Execute();
			}
			PlayFabZLibWorkQueue.s_workersMutex.ReleaseMutex();
		}
	}

	// Token: 0x0600107F RID: 4223 RVA: 0x0006C5C8 File Offset: 0x0006A7C8
	private void Execute()
	{
		this.m_buffersMutex.WaitOne();
		this.DoUncompress();
		this.m_buffersMutex.ReleaseMutex();
		this.m_buffersMutex.WaitOne();
		this.DoCompress();
		this.m_buffersMutex.ReleaseMutex();
	}

	// Token: 0x06001080 RID: 4224 RVA: 0x0006C604 File Offset: 0x0006A804
	private void DoUncompress()
	{
		while (this.m_inDecompress.Count > 0)
		{
			try
			{
				byte[] payload = this.m_inDecompress.Dequeue();
				byte[] item = this.UncompressOnThisThread(payload);
				this.m_outDecompress.Enqueue(item);
			}
			catch
			{
			}
		}
	}

	// Token: 0x06001081 RID: 4225 RVA: 0x0006C658 File Offset: 0x0006A858
	private void DoCompress()
	{
		while (this.m_inCompress.Count > 0)
		{
			try
			{
				byte[] payload = this.m_inCompress.Dequeue();
				byte[] item = this.CompressOnThisThread(payload);
				this.m_outCompress.Enqueue(item);
			}
			catch
			{
			}
		}
	}

	// Token: 0x06001082 RID: 4226 RVA: 0x0006C6AC File Offset: 0x0006A8AC
	public void Dispose()
	{
		PlayFabZLibWorkQueue.s_workersMutex.WaitOne();
		PlayFabZLibWorkQueue.s_workers.Remove(this);
		PlayFabZLibWorkQueue.s_workersMutex.ReleaseMutex();
	}

	// Token: 0x06001083 RID: 4227 RVA: 0x0006C6CF File Offset: 0x0006A8CF
	internal byte[] CompressOnThisThread(byte[] payload)
	{
		return ZlibStream.CompressBuffer(payload, CompressionLevel.BestCompression);
	}

	// Token: 0x06001084 RID: 4228 RVA: 0x0006C6D9 File Offset: 0x0006A8D9
	internal byte[] UncompressOnThisThread(byte[] payload)
	{
		return ZlibStream.UncompressBuffer(payload);
	}

	// Token: 0x0400115E RID: 4446
	private static Thread s_thread;

	// Token: 0x0400115F RID: 4447
	private static bool s_moreWork;

	// Token: 0x04001160 RID: 4448
	private static readonly List<PlayFabZLibWorkQueue> s_workers = new List<PlayFabZLibWorkQueue>();

	// Token: 0x04001161 RID: 4449
	private readonly Queue<byte[]> m_inCompress = new Queue<byte[]>();

	// Token: 0x04001162 RID: 4450
	private readonly Queue<byte[]> m_outCompress = new Queue<byte[]>();

	// Token: 0x04001163 RID: 4451
	private readonly Queue<byte[]> m_inDecompress = new Queue<byte[]>();

	// Token: 0x04001164 RID: 4452
	private readonly Queue<byte[]> m_outDecompress = new Queue<byte[]>();

	// Token: 0x04001165 RID: 4453
	private static Mutex s_workersMutex = new Mutex();

	// Token: 0x04001166 RID: 4454
	private Mutex m_buffersMutex = new Mutex();

	// Token: 0x04001167 RID: 4455
	private static SemaphoreSlim s_workSemaphore = new SemaphoreSlim(0, 1);
}
