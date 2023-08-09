using System;

// Token: 0x0200000F RID: 15
public class Emote : Attribute
{
	// Token: 0x0600013E RID: 318 RVA: 0x00008B38 File Offset: 0x00006D38
	public static void DoEmote(Emotes emote)
	{
		Emote attributeOfType = emote.GetAttributeOfType<Emote>();
		if (Player.m_localPlayer && Player.m_localPlayer.StartEmote(emote.ToString().ToLower(), attributeOfType == null || attributeOfType.OneShot) && attributeOfType != null && attributeOfType.FaceLookDirection)
		{
			Player.m_localPlayer.FaceLookDirection();
		}
	}

	// Token: 0x0400012F RID: 303
	public bool OneShot = true;

	// Token: 0x04000130 RID: 304
	public bool FaceLookDirection;
}
