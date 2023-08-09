using System;

// Token: 0x02000132 RID: 306
public interface ISerializableParameter
{
	// Token: 0x06000BE5 RID: 3045
	void Serialize(ref ZPackage pkg);

	// Token: 0x06000BE6 RID: 3046
	void Deserialize(ref ZPackage pkg);
}
