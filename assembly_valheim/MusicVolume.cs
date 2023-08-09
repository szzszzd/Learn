using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000270 RID: 624
public class MusicVolume : MonoBehaviour
{
	// Token: 0x060017F6 RID: 6134 RVA: 0x0009F8BC File Offset: 0x0009DABC
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview)
		{
			this.m_PlayCount = this.m_nview.GetZDO().GetInt(ZDOVars.s_plays, 0);
			this.m_nview.Register("RPC_PlayMusic", new Action<long>(this.RPC_PlayMusic));
		}
		if (this.m_addRadiusFromLocation)
		{
			Location componentInParent = base.GetComponentInParent<Location>();
			if (componentInParent != null)
			{
				this.m_radius += componentInParent.GetMaxRadius();
			}
		}
		if (this.m_fadeByProximity)
		{
			MusicVolume.m_proximityMusicVolumes.Add(this);
		}
	}

	// Token: 0x060017F7 RID: 6135 RVA: 0x0009F952 File Offset: 0x0009DB52
	private void OnDestroy()
	{
		MusicVolume.m_proximityMusicVolumes.Remove(this);
	}

	// Token: 0x060017F8 RID: 6136 RVA: 0x0009F960 File Offset: 0x0009DB60
	private void RPC_PlayMusic(long sender)
	{
		bool flag = Vector3.Distance(Player.m_localPlayer.transform.position, base.transform.position) < this.m_radius + this.m_surroundingPlayersAdditionalRadius;
		if (flag)
		{
			this.PlayMusic();
		}
		if (this.m_nview && this.m_nview.IsValid() && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_plays, flag ? this.m_PlayCount : (this.m_PlayCount + 1), false);
		}
	}

	// Token: 0x060017F9 RID: 6137 RVA: 0x0009F9F8 File Offset: 0x0009DBF8
	private void PlayMusic()
	{
		ZLog.Log("MusicLocation '" + base.name + "' Playing Music: " + this.m_musicName);
		this.m_PlayCount++;
		MusicMan.instance.LocationMusic(this.m_musicName);
		if (this.m_loopMusic)
		{
			this.m_isLooping = true;
		}
	}

	// Token: 0x060017FA RID: 6138 RVA: 0x0009FA54 File Offset: 0x0009DC54
	private void Update()
	{
		if (Player.m_localPlayer == null)
		{
			return;
		}
		if (this.m_fadeByProximity)
		{
			return;
		}
		if (DateTime.Now > this.m_lastEnterCheck + TimeSpan.FromSeconds(1.0))
		{
			this.m_lastEnterCheck = DateTime.Now;
			if (this.IsInside(Player.m_localPlayer.transform.position, false))
			{
				if (!this.m_lastWasInside)
				{
					this.m_lastWasInside = (this.m_lastWasInsideWide = true);
					this.OnEnter();
				}
			}
			else
			{
				if (this.m_lastWasInside)
				{
					this.m_lastWasInside = false;
					this.OnExit();
				}
				if (this.m_lastWasInsideWide && !this.IsInside(Player.m_localPlayer.transform.position, true))
				{
					this.m_lastWasInsideWide = false;
					this.OnExitWide();
				}
			}
		}
		if (this.m_isLooping && this.m_lastWasInside && !string.IsNullOrEmpty(this.m_musicName))
		{
			MusicMan.instance.LocationMusic(this.m_musicName);
		}
	}

	// Token: 0x060017FB RID: 6139 RVA: 0x0009FB54 File Offset: 0x0009DD54
	private void OnEnter()
	{
		ZLog.Log("MusicLocation.OnEnter: " + base.name);
		if (!string.IsNullOrEmpty(this.m_musicName) && (this.m_maxPlaysPerActivation == 0 || this.m_PlayCount < this.m_maxPlaysPerActivation) && UnityEngine.Random.Range(0f, 1f) <= this.m_musicChance && (this.m_musicCanRepeat || MusicMan.instance.m_lastLocationMusic != this.m_musicName))
		{
			if (this.m_nview)
			{
				this.m_nview.InvokeRPC(ZNetView.Everybody, "RPC_PlayMusic", Array.Empty<object>());
				return;
			}
			this.PlayMusic();
		}
	}

	// Token: 0x060017FC RID: 6140 RVA: 0x0009FBFD File Offset: 0x0009DDFD
	private void OnExit()
	{
		ZLog.Log("MusicLocation.OnExit: " + base.name);
	}

	// Token: 0x060017FD RID: 6141 RVA: 0x0009FC14 File Offset: 0x0009DE14
	private void OnExitWide()
	{
		ZLog.Log("MusicLocation.OnExitWide: " + base.name);
		if (MusicMan.instance.m_lastLocationMusic == this.m_musicName && (this.m_stopMusicOnExit || this.m_loopMusic))
		{
			MusicMan.instance.LocationMusic(null);
		}
		this.m_isLooping = false;
	}

	// Token: 0x060017FE RID: 6142 RVA: 0x0009FC70 File Offset: 0x0009DE70
	public bool IsInside(Vector3 point, bool checkOuter = false)
	{
		if (this.IsBox())
		{
			if (!checkOuter)
			{
				return this.GetInnerBounds().Contains(point);
			}
			return this.GetOuterBounds().Contains(point);
		}
		else
		{
			float num = Vector3.Distance(base.transform.position, point);
			if (checkOuter)
			{
				return num < this.m_radius + this.m_outerRadiusExtra;
			}
			return num < this.m_radius;
		}
	}

	// Token: 0x060017FF RID: 6143 RVA: 0x0009FCD8 File Offset: 0x0009DED8
	private void OnDrawGizmos()
	{
		if (!this.IsBox())
		{
			Gizmos.color = new Color(0.6f, 0.8f, 0.8f, 0.5f);
			Gizmos.DrawWireSphere(base.transform.position, this.m_radius);
			Gizmos.color = new Color(0.6f, 0.8f, 0.8f, 0.25f);
			Gizmos.DrawWireSphere(base.transform.position, this.m_radius + this.m_outerRadiusExtra);
			return;
		}
		Gizmos.color = new Color(0.6f, 0.8f, 0.8f, 0.5f);
		Gizmos.DrawWireCube(this.GetInnerBounds().center, this.GetBox().size);
		Gizmos.color = new Color(0.6f, 0.8f, 0.8f, 0.25f);
		Gizmos.DrawWireCube(this.GetOuterBounds().center, this.GetOuterBounds().size);
	}

	// Token: 0x06001800 RID: 6144 RVA: 0x0009FDDC File Offset: 0x0009DFDC
	private bool IsBox()
	{
		return this.GetBox().size.x != 0f;
	}

	// Token: 0x06001801 RID: 6145 RVA: 0x0009FE06 File Offset: 0x0009E006
	private Bounds GetBox()
	{
		if (!this.m_sizeFromRoom)
		{
			return this.m_boundsInner;
		}
		return new Bounds(Vector3.zero, this.m_sizeFromRoom.m_size);
	}

	// Token: 0x06001802 RID: 6146 RVA: 0x0009FE38 File Offset: 0x0009E038
	private Bounds GetInnerBounds()
	{
		Bounds box = this.GetBox();
		return new Bounds(box.center + base.transform.position, box.size);
	}

	// Token: 0x06001803 RID: 6147 RVA: 0x0009FE70 File Offset: 0x0009E070
	private Bounds GetOuterBounds()
	{
		Bounds box = this.GetBox();
		return new Bounds(box.center + base.transform.position, box.size + new Vector3(this.m_outerRadiusExtra, this.m_outerRadiusExtra, this.m_outerRadiusExtra));
	}

	// Token: 0x06001804 RID: 6148 RVA: 0x0009FEC4 File Offset: 0x0009E0C4
	private float MinBoundDimension()
	{
		Bounds box = this.GetBox();
		if (box.size.x < box.size.y && box.size.x < box.size.z)
		{
			return box.size.x;
		}
		if (box.size.y >= box.size.z)
		{
			return box.size.z;
		}
		return box.size.y;
	}

	// Token: 0x06001805 RID: 6149 RVA: 0x0009FF4C File Offset: 0x0009E14C
	public static float UpdateProximityVolumes(AudioSource musicSource)
	{
		if (!Player.m_localPlayer)
		{
			return 1f;
		}
		float num = 0f;
		if (MusicVolume.m_lastProximityVolume != null && MusicVolume.m_lastProximityVolume.GetInnerBounds().Contains(Player.m_localPlayer.transform.position))
		{
			num = 1f;
		}
		else
		{
			MusicVolume.m_lastProximityVolume = null;
			MusicVolume.m_close.Clear();
			foreach (MusicVolume musicVolume in MusicVolume.m_proximityMusicVolumes)
			{
				if (musicVolume && musicVolume.IsInside(Player.m_localPlayer.transform.position, true))
				{
					MusicVolume.m_close.Add(musicVolume);
				}
			}
			if (MusicVolume.m_close.Count == 0)
			{
				MusicMan.instance.LocationMusic(null);
				return 1f;
			}
			foreach (MusicVolume musicVolume2 in MusicVolume.m_close)
			{
				if (musicVolume2.IsInside(Player.m_localPlayer.transform.position, false))
				{
					MusicVolume.m_lastProximityVolume = musicVolume2;
					num = 1f;
				}
			}
			if (num == 0f)
			{
				MusicVolume musicVolume3 = null;
				foreach (MusicVolume musicVolume4 in MusicVolume.m_close)
				{
					float num2;
					float num3;
					if (musicVolume4.IsBox())
					{
						num2 = Vector3.Distance(musicVolume4.GetInnerBounds().ClosestPoint(Player.m_localPlayer.transform.position), Player.m_localPlayer.transform.position);
						num3 = musicVolume4.m_outerRadiusExtra - num2;
					}
					else
					{
						float num4 = Vector3.Distance(musicVolume4.transform.position, Player.m_localPlayer.transform.position);
						num2 = num4 - musicVolume4.m_radius;
						num3 = musicVolume4.m_radius + musicVolume4.m_outerRadiusExtra - num4;
					}
					musicVolume4.m_proximity = 1f - Math.Min(1f, num2 / (num2 + num3));
					if (musicVolume3 == null || musicVolume4.m_proximity > musicVolume3.m_proximity)
					{
						musicVolume3 = musicVolume4;
					}
				}
				MusicVolume.m_lastProximityVolume = musicVolume3;
				num = musicVolume3.m_proximity;
			}
		}
		MusicMan.instance.LocationMusic(MusicVolume.m_lastProximityVolume.m_musicName);
		return num;
	}

	// Token: 0x04001973 RID: 6515
	private ZNetView m_nview;

	// Token: 0x04001974 RID: 6516
	public static List<MusicVolume> m_proximityMusicVolumes = new List<MusicVolume>();

	// Token: 0x04001975 RID: 6517
	private static MusicVolume m_lastProximityVolume;

	// Token: 0x04001976 RID: 6518
	private static List<MusicVolume> m_close = new List<MusicVolume>();

	// Token: 0x04001977 RID: 6519
	public bool m_addRadiusFromLocation = true;

	// Token: 0x04001978 RID: 6520
	public float m_radius = 10f;

	// Token: 0x04001979 RID: 6521
	public float m_outerRadiusExtra = 0.5f;

	// Token: 0x0400197A RID: 6522
	public float m_surroundingPlayersAdditionalRadius = 50f;

	// Token: 0x0400197B RID: 6523
	public Bounds m_boundsInner;

	// Token: 0x0400197C RID: 6524
	[global::Tooltip("Takes dimension from the room it's a part of and sets bounds to it's size.")]
	public Room m_sizeFromRoom;

	// Token: 0x0400197D RID: 6525
	[Header("Music")]
	public string m_musicName = "";

	// Token: 0x0400197E RID: 6526
	public float m_musicChance = 0.7f;

	// Token: 0x0400197F RID: 6527
	[global::Tooltip("If the music can play again before playing a different location music first.")]
	public bool m_musicCanRepeat = true;

	// Token: 0x04001980 RID: 6528
	public bool m_loopMusic;

	// Token: 0x04001981 RID: 6529
	public bool m_stopMusicOnExit;

	// Token: 0x04001982 RID: 6530
	public int m_maxPlaysPerActivation;

	// Token: 0x04001983 RID: 6531
	[global::Tooltip("Makes the music fade by distance between inner/outer bounds. With this enabled loop, repeat, stoponexit, chance, etc is ignored.")]
	public bool m_fadeByProximity;

	// Token: 0x04001984 RID: 6532
	[HideInInspector]
	public int m_PlayCount;

	// Token: 0x04001985 RID: 6533
	private DateTime m_lastEnterCheck;

	// Token: 0x04001986 RID: 6534
	private bool m_lastWasInside;

	// Token: 0x04001987 RID: 6535
	private bool m_lastWasInsideWide;

	// Token: 0x04001988 RID: 6536
	private bool m_isLooping;

	// Token: 0x04001989 RID: 6537
	private float m_proximity;
}
