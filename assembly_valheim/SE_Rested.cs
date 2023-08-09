using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000060 RID: 96
public class SE_Rested : SE_Stats
{
	// Token: 0x060004F8 RID: 1272 RVA: 0x0002868C File Offset: 0x0002688C
	public override void Setup(Character character)
	{
		base.Setup(character);
		this.UpdateTTL();
		Player player = this.m_character as Player;
		this.m_character.Message(MessageHud.MessageType.Center, "$se_rested_start ($se_rested_comfort:" + player.GetComfortLevel().ToString() + ")", 0, null);
	}

	// Token: 0x060004F9 RID: 1273 RVA: 0x000286DD File Offset: 0x000268DD
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		this.m_timeSinceComfortUpdate -= dt;
	}

	// Token: 0x060004FA RID: 1274 RVA: 0x000286F4 File Offset: 0x000268F4
	public override void ResetTime()
	{
		this.UpdateTTL();
	}

	// Token: 0x060004FB RID: 1275 RVA: 0x000286FC File Offset: 0x000268FC
	private void UpdateTTL()
	{
		Player player = this.m_character as Player;
		float num = this.m_baseTTL + (float)(player.GetComfortLevel() - 1) * this.m_TTLPerComfortLevel;
		float num2 = this.m_ttl - this.m_time;
		if (num > num2)
		{
			this.m_ttl = num;
			this.m_time = 0f;
		}
	}

	// Token: 0x060004FC RID: 1276 RVA: 0x00028754 File Offset: 0x00026954
	private static int PieceComfortSort(Piece x, Piece y)
	{
		if (x.m_comfortGroup != y.m_comfortGroup)
		{
			return x.m_comfortGroup.CompareTo(y.m_comfortGroup);
		}
		float num = (float)x.GetComfort();
		float num2 = (float)y.GetComfort();
		if (num != num2)
		{
			return num2.CompareTo(num);
		}
		return y.m_name.CompareTo(x.m_name);
	}

	// Token: 0x060004FD RID: 1277 RVA: 0x000287BA File Offset: 0x000269BA
	public static int CalculateComfortLevel(Player player)
	{
		return SE_Rested.CalculateComfortLevel(player.InShelter(), player.transform.position);
	}

	// Token: 0x060004FE RID: 1278 RVA: 0x000287D4 File Offset: 0x000269D4
	public static int CalculateComfortLevel(bool inShelter, Vector3 position)
	{
		int num = 1;
		if (inShelter)
		{
			num++;
			List<Piece> nearbyComfortPieces = SE_Rested.GetNearbyComfortPieces(position);
			nearbyComfortPieces.Sort(new Comparison<Piece>(SE_Rested.PieceComfortSort));
			int i = 0;
			while (i < nearbyComfortPieces.Count)
			{
				Piece piece = nearbyComfortPieces[i];
				if (i <= 0)
				{
					goto IL_68;
				}
				Piece piece2 = nearbyComfortPieces[i - 1];
				if ((piece.m_comfortGroup == Piece.ComfortGroup.None || piece.m_comfortGroup != piece2.m_comfortGroup) && !(piece.m_name == piece2.m_name))
				{
					goto IL_68;
				}
				IL_71:
				i++;
				continue;
				IL_68:
				num += piece.GetComfort();
				goto IL_71;
			}
		}
		return num;
	}

	// Token: 0x060004FF RID: 1279 RVA: 0x00028860 File Offset: 0x00026A60
	private static List<Piece> GetNearbyComfortPieces(Vector3 point)
	{
		SE_Rested.s_tempPieces.Clear();
		Piece.GetAllComfortPiecesInRadius(point, 10f, SE_Rested.s_tempPieces);
		return SE_Rested.s_tempPieces;
	}

	// Token: 0x040005D7 RID: 1495
	[Header("__SE_Rested__")]
	public float m_baseTTL = 300f;

	// Token: 0x040005D8 RID: 1496
	public float m_TTLPerComfortLevel = 60f;

	// Token: 0x040005D9 RID: 1497
	private const float c_ComfortRadius = 10f;

	// Token: 0x040005DA RID: 1498
	private float m_timeSinceComfortUpdate;

	// Token: 0x040005DB RID: 1499
	private static readonly List<Piece> s_tempPieces = new List<Piece>();
}
