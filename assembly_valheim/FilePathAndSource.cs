using System;

// Token: 0x020001E8 RID: 488
public struct FilePathAndSource
{
	// Token: 0x060013D3 RID: 5075 RVA: 0x00082239 File Offset: 0x00080439
	public FilePathAndSource(string path, FileHelpers.FileSource source)
	{
		this.path = path;
		this.source = source;
	}

	// Token: 0x040014B7 RID: 5303
	public string path;

	// Token: 0x040014B8 RID: 5304
	public FileHelpers.FileSource source;
}
