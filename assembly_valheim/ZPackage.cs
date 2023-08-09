using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

// Token: 0x02000179 RID: 377
public class ZPackage
{
	// Token: 0x06000EEF RID: 3823 RVA: 0x00065830 File Offset: 0x00063A30
	public ZPackage()
	{
		this.m_writer = new BinaryWriter(this.m_stream);
		this.m_reader = new BinaryReader(this.m_stream);
	}

	// Token: 0x06000EF0 RID: 3824 RVA: 0x00065868 File Offset: 0x00063A68
	public ZPackage(string base64String)
	{
		this.m_writer = new BinaryWriter(this.m_stream);
		this.m_reader = new BinaryReader(this.m_stream);
		if (string.IsNullOrEmpty(base64String))
		{
			return;
		}
		byte[] array = Convert.FromBase64String(base64String);
		this.m_stream.Write(array, 0, array.Length);
		this.m_stream.Position = 0L;
	}

	// Token: 0x06000EF1 RID: 3825 RVA: 0x000658D8 File Offset: 0x00063AD8
	public ZPackage(byte[] data)
	{
		this.m_writer = new BinaryWriter(this.m_stream);
		this.m_reader = new BinaryReader(this.m_stream);
		this.m_stream.Write(data, 0, data.Length);
		this.m_stream.Position = 0L;
	}

	// Token: 0x06000EF2 RID: 3826 RVA: 0x00065938 File Offset: 0x00063B38
	public ZPackage(byte[] data, int dataSize)
	{
		this.m_writer = new BinaryWriter(this.m_stream);
		this.m_reader = new BinaryReader(this.m_stream);
		this.m_stream.Write(data, 0, dataSize);
		this.m_stream.Position = 0L;
	}

	// Token: 0x06000EF3 RID: 3827 RVA: 0x00065993 File Offset: 0x00063B93
	public void SetReader(BinaryReader reader)
	{
		this.m_reader = reader;
	}

	// Token: 0x06000EF4 RID: 3828 RVA: 0x0006599C File Offset: 0x00063B9C
	public void SetWriter(BinaryWriter writer)
	{
		this.m_writer = writer;
	}

	// Token: 0x06000EF5 RID: 3829 RVA: 0x000659A5 File Offset: 0x00063BA5
	public void Load(byte[] data)
	{
		this.Clear();
		this.m_stream.Write(data, 0, data.Length);
		this.m_stream.Position = 0L;
	}

	// Token: 0x06000EF6 RID: 3830 RVA: 0x000659CC File Offset: 0x00063BCC
	public void Write(ZPackage pkg)
	{
		byte[] array = pkg.GetArray();
		this.m_writer.Write(array.Length);
		this.m_writer.Write(array);
	}

	// Token: 0x06000EF7 RID: 3831 RVA: 0x000659FC File Offset: 0x00063BFC
	public void WriteCompressed(ZPackage pkg)
	{
		byte[] array = Utils.Compress(pkg.GetArray());
		this.m_writer.Write(array.Length);
		this.m_writer.Write(array);
	}

	// Token: 0x06000EF8 RID: 3832 RVA: 0x00065A2F File Offset: 0x00063C2F
	public void Write(byte[] array)
	{
		this.m_writer.Write(array.Length);
		this.m_writer.Write(array);
	}

	// Token: 0x06000EF9 RID: 3833 RVA: 0x00065A4B File Offset: 0x00063C4B
	public void Write(byte data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x06000EFA RID: 3834 RVA: 0x00065A59 File Offset: 0x00063C59
	public void Write(sbyte data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x06000EFB RID: 3835 RVA: 0x00065A67 File Offset: 0x00063C67
	public void Write(char data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x06000EFC RID: 3836 RVA: 0x00065A75 File Offset: 0x00063C75
	public void Write(bool data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x06000EFD RID: 3837 RVA: 0x00065A83 File Offset: 0x00063C83
	public void Write(int data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x06000EFE RID: 3838 RVA: 0x00065A91 File Offset: 0x00063C91
	public void Write(uint data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x06000EFF RID: 3839 RVA: 0x00065A9F File Offset: 0x00063C9F
	public void Write(short data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x06000F00 RID: 3840 RVA: 0x00065AAD File Offset: 0x00063CAD
	public void Write(ushort data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x06000F01 RID: 3841 RVA: 0x00065ABB File Offset: 0x00063CBB
	public void Write(long data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x06000F02 RID: 3842 RVA: 0x00065AC9 File Offset: 0x00063CC9
	public void Write(ulong data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x06000F03 RID: 3843 RVA: 0x00065AD7 File Offset: 0x00063CD7
	public void Write(float data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x06000F04 RID: 3844 RVA: 0x00065AE5 File Offset: 0x00063CE5
	public void Write(double data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x06000F05 RID: 3845 RVA: 0x00065AF3 File Offset: 0x00063CF3
	public void Write(string data)
	{
		this.m_writer.Write(data);
	}

	// Token: 0x06000F06 RID: 3846 RVA: 0x00065B01 File Offset: 0x00063D01
	public void Write(ZDOID id)
	{
		this.m_writer.Write(id.UserID);
		this.m_writer.Write(id.ID);
	}

	// Token: 0x06000F07 RID: 3847 RVA: 0x00065B27 File Offset: 0x00063D27
	public void Write(Vector3 v3)
	{
		this.m_writer.Write(v3.x);
		this.m_writer.Write(v3.y);
		this.m_writer.Write(v3.z);
	}

	// Token: 0x06000F08 RID: 3848 RVA: 0x00065B5C File Offset: 0x00063D5C
	public void Write(Vector2i v2)
	{
		this.m_writer.Write(v2.x);
		this.m_writer.Write(v2.y);
	}

	// Token: 0x06000F09 RID: 3849 RVA: 0x00065B80 File Offset: 0x00063D80
	public void Write(Vector2s v2)
	{
		this.m_writer.Write(v2.x);
		this.m_writer.Write(v2.y);
	}

	// Token: 0x06000F0A RID: 3850 RVA: 0x00065BA4 File Offset: 0x00063DA4
	public void Write(Quaternion q)
	{
		this.m_writer.Write(q.x);
		this.m_writer.Write(q.y);
		this.m_writer.Write(q.z);
		this.m_writer.Write(q.w);
	}

	// Token: 0x06000F0B RID: 3851 RVA: 0x00065BF5 File Offset: 0x00063DF5
	public ZDOID ReadZDOID()
	{
		return new ZDOID(this.m_reader.ReadInt64(), this.m_reader.ReadUInt32());
	}

	// Token: 0x06000F0C RID: 3852 RVA: 0x00065C12 File Offset: 0x00063E12
	public bool ReadBool()
	{
		return this.m_reader.ReadBoolean();
	}

	// Token: 0x06000F0D RID: 3853 RVA: 0x00065C1F File Offset: 0x00063E1F
	public char ReadChar()
	{
		return this.m_reader.ReadChar();
	}

	// Token: 0x06000F0E RID: 3854 RVA: 0x00065C2C File Offset: 0x00063E2C
	public byte ReadByte()
	{
		return this.m_reader.ReadByte();
	}

	// Token: 0x06000F0F RID: 3855 RVA: 0x00065C39 File Offset: 0x00063E39
	public sbyte ReadSByte()
	{
		return this.m_reader.ReadSByte();
	}

	// Token: 0x06000F10 RID: 3856 RVA: 0x00065C46 File Offset: 0x00063E46
	public short ReadShort()
	{
		return this.m_reader.ReadInt16();
	}

	// Token: 0x06000F11 RID: 3857 RVA: 0x00065C53 File Offset: 0x00063E53
	public ushort ReadUShort()
	{
		return this.m_reader.ReadUInt16();
	}

	// Token: 0x06000F12 RID: 3858 RVA: 0x00065C60 File Offset: 0x00063E60
	public int ReadInt()
	{
		return this.m_reader.ReadInt32();
	}

	// Token: 0x06000F13 RID: 3859 RVA: 0x00065C6D File Offset: 0x00063E6D
	public uint ReadUInt()
	{
		return this.m_reader.ReadUInt32();
	}

	// Token: 0x06000F14 RID: 3860 RVA: 0x00065C7A File Offset: 0x00063E7A
	public long ReadLong()
	{
		return this.m_reader.ReadInt64();
	}

	// Token: 0x06000F15 RID: 3861 RVA: 0x00065C87 File Offset: 0x00063E87
	public ulong ReadULong()
	{
		return this.m_reader.ReadUInt64();
	}

	// Token: 0x06000F16 RID: 3862 RVA: 0x00065C94 File Offset: 0x00063E94
	public float ReadSingle()
	{
		return this.m_reader.ReadSingle();
	}

	// Token: 0x06000F17 RID: 3863 RVA: 0x00065CA1 File Offset: 0x00063EA1
	public double ReadDouble()
	{
		return this.m_reader.ReadDouble();
	}

	// Token: 0x06000F18 RID: 3864 RVA: 0x00065CAE File Offset: 0x00063EAE
	public string ReadString()
	{
		return this.m_reader.ReadString();
	}

	// Token: 0x06000F19 RID: 3865 RVA: 0x00065CBC File Offset: 0x00063EBC
	public Vector3 ReadVector3()
	{
		return new Vector3
		{
			x = this.m_reader.ReadSingle(),
			y = this.m_reader.ReadSingle(),
			z = this.m_reader.ReadSingle()
		};
	}

	// Token: 0x06000F1A RID: 3866 RVA: 0x00065D08 File Offset: 0x00063F08
	public Vector2i ReadVector2i()
	{
		return new Vector2i
		{
			x = this.m_reader.ReadInt32(),
			y = this.m_reader.ReadInt32()
		};
	}

	// Token: 0x06000F1B RID: 3867 RVA: 0x00065D44 File Offset: 0x00063F44
	public Vector2s ReadVector2s()
	{
		return new Vector2s
		{
			x = this.m_reader.ReadInt16(),
			y = this.m_reader.ReadInt16()
		};
	}

	// Token: 0x06000F1C RID: 3868 RVA: 0x00065D80 File Offset: 0x00063F80
	public Quaternion ReadQuaternion()
	{
		return new Quaternion
		{
			x = this.m_reader.ReadSingle(),
			y = this.m_reader.ReadSingle(),
			z = this.m_reader.ReadSingle(),
			w = this.m_reader.ReadSingle()
		};
	}

	// Token: 0x06000F1D RID: 3869 RVA: 0x00065DE0 File Offset: 0x00063FE0
	public ZPackage ReadCompressedPackage()
	{
		int count = this.m_reader.ReadInt32();
		return new ZPackage(Utils.Decompress(this.m_reader.ReadBytes(count)));
	}

	// Token: 0x06000F1E RID: 3870 RVA: 0x00065E10 File Offset: 0x00064010
	public ZPackage ReadPackage()
	{
		int count = this.m_reader.ReadInt32();
		return new ZPackage(this.m_reader.ReadBytes(count));
	}

	// Token: 0x06000F1F RID: 3871 RVA: 0x00065E3C File Offset: 0x0006403C
	public void ReadPackage(ref ZPackage pkg)
	{
		int count = this.m_reader.ReadInt32();
		byte[] array = this.m_reader.ReadBytes(count);
		pkg.Clear();
		pkg.m_stream.Write(array, 0, array.Length);
		pkg.m_stream.Position = 0L;
	}

	// Token: 0x06000F20 RID: 3872 RVA: 0x00065E88 File Offset: 0x00064088
	public byte[] ReadByteArray()
	{
		int count = this.m_reader.ReadInt32();
		return this.m_reader.ReadBytes(count);
	}

	// Token: 0x06000F21 RID: 3873 RVA: 0x00065EAD File Offset: 0x000640AD
	public string GetBase64()
	{
		return Convert.ToBase64String(this.GetArray());
	}

	// Token: 0x06000F22 RID: 3874 RVA: 0x00065EBA File Offset: 0x000640BA
	public byte[] GetArray()
	{
		this.m_writer.Flush();
		this.m_stream.Flush();
		return this.m_stream.ToArray();
	}

	// Token: 0x06000F23 RID: 3875 RVA: 0x00065EDD File Offset: 0x000640DD
	public void SetPos(int pos)
	{
		this.m_stream.Position = (long)pos;
	}

	// Token: 0x06000F24 RID: 3876 RVA: 0x00065EEC File Offset: 0x000640EC
	public int GetPos()
	{
		return (int)this.m_stream.Position;
	}

	// Token: 0x06000F25 RID: 3877 RVA: 0x00065EFA File Offset: 0x000640FA
	public int Size()
	{
		this.m_writer.Flush();
		this.m_stream.Flush();
		return (int)this.m_stream.Length;
	}

	// Token: 0x06000F26 RID: 3878 RVA: 0x00065F1E File Offset: 0x0006411E
	public void Clear()
	{
		this.m_writer.Flush();
		this.m_stream.SetLength(0L);
		this.m_stream.Position = 0L;
	}

	// Token: 0x06000F27 RID: 3879 RVA: 0x00065F48 File Offset: 0x00064148
	public byte[] GenerateHash()
	{
		byte[] array = this.GetArray();
		return SHA512.Create().ComputeHash(array);
	}

	// Token: 0x04001085 RID: 4229
	private MemoryStream m_stream = new MemoryStream();

	// Token: 0x04001086 RID: 4230
	private BinaryWriter m_writer;

	// Token: 0x04001087 RID: 4231
	private BinaryReader m_reader;
}
