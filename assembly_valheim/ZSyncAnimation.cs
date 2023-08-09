using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200018D RID: 397
public class ZSyncAnimation : MonoBehaviour
{
	// Token: 0x06001021 RID: 4129 RVA: 0x0006A950 File Offset: 0x00068B50
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_animator = base.GetComponentInChildren<Animator>();
		this.m_animator.logWarnings = false;
		this.m_nview.Register<string>("SetTrigger", new Action<long, string>(this.RPC_SetTrigger));
		this.m_boolHashes = new int[this.m_syncBools.Count];
		this.m_boolDefaults = new bool[this.m_syncBools.Count];
		for (int i = 0; i < this.m_syncBools.Count; i++)
		{
			this.m_boolHashes[i] = ZSyncAnimation.GetHash(this.m_syncBools[i]);
			this.m_boolDefaults[i] = this.m_animator.GetBool(this.m_boolHashes[i]);
		}
		this.m_floatHashes = new int[this.m_syncFloats.Count];
		this.m_floatDefaults = new float[this.m_syncFloats.Count];
		for (int j = 0; j < this.m_syncFloats.Count; j++)
		{
			this.m_floatHashes[j] = ZSyncAnimation.GetHash(this.m_syncFloats[j]);
			this.m_floatDefaults[j] = this.m_animator.GetFloat(this.m_floatHashes[j]);
		}
		this.m_intHashes = new int[this.m_syncInts.Count];
		this.m_intDefaults = new int[this.m_syncInts.Count];
		for (int k = 0; k < this.m_syncInts.Count; k++)
		{
			this.m_intHashes[k] = ZSyncAnimation.GetHash(this.m_syncInts[k]);
			this.m_intDefaults[k] = this.m_animator.GetInteger(this.m_intHashes[k]);
		}
		if (this.m_nview.GetZDO() == null)
		{
			base.enabled = false;
			return;
		}
		this.SyncParameters();
	}

	// Token: 0x06001022 RID: 4130 RVA: 0x0006AB1A File Offset: 0x00068D1A
	private void OnEnable()
	{
		ZSyncAnimation.Instances.Add(this);
	}

	// Token: 0x06001023 RID: 4131 RVA: 0x0006AB27 File Offset: 0x00068D27
	private void OnDisable()
	{
		ZSyncAnimation.Instances.Remove(this);
	}

	// Token: 0x06001024 RID: 4132 RVA: 0x0006AB35 File Offset: 0x00068D35
	public static int GetHash(string name)
	{
		return Animator.StringToHash(name);
	}

	// Token: 0x06001025 RID: 4133 RVA: 0x0006AB3D File Offset: 0x00068D3D
	public void CustomFixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.SyncParameters();
	}

	// Token: 0x06001026 RID: 4134 RVA: 0x0006AB54 File Offset: 0x00068D54
	private void SyncParameters()
	{
		ZDO zdo = this.m_nview.GetZDO();
		if (this.m_nview.IsOwner())
		{
			zdo.Set(ZSyncAnimation.s_animSpeedID, this.m_animator.speed);
			return;
		}
		for (int i = 0; i < this.m_boolHashes.Length; i++)
		{
			int num = this.m_boolHashes[i];
			bool @bool = zdo.GetBool(438569 + num, this.m_boolDefaults[i]);
			this.m_animator.SetBool(num, @bool);
		}
		for (int j = 0; j < this.m_floatHashes.Length; j++)
		{
			int num2 = this.m_floatHashes[j];
			float @float = zdo.GetFloat(438569 + num2, this.m_floatDefaults[j]);
			if (this.m_smoothCharacterSpeeds && (num2 == ZSyncAnimation.s_forwardSpeedID || num2 == ZSyncAnimation.s_sidewaySpeedID))
			{
				this.m_animator.SetFloat(num2, @float, 0.2f, Time.fixedDeltaTime);
			}
			else
			{
				this.m_animator.SetFloat(num2, @float);
			}
		}
		for (int k = 0; k < this.m_intHashes.Length; k++)
		{
			int num3 = this.m_intHashes[k];
			int @int = zdo.GetInt(438569 + num3, this.m_intDefaults[k]);
			this.m_animator.SetInteger(num3, @int);
		}
		float float2 = zdo.GetFloat(ZSyncAnimation.s_animSpeedID, 1f);
		this.m_animator.speed = float2;
	}

	// Token: 0x06001027 RID: 4135 RVA: 0x0006ACB8 File Offset: 0x00068EB8
	public void SetTrigger(string name)
	{
		this.m_nview.InvokeRPC(ZNetView.Everybody, "SetTrigger", new object[]
		{
			name
		});
	}

	// Token: 0x06001028 RID: 4136 RVA: 0x0006ACDC File Offset: 0x00068EDC
	public void SetBool(string name, bool value)
	{
		int hash = ZSyncAnimation.GetHash(name);
		this.SetBool(hash, value);
	}

	// Token: 0x06001029 RID: 4137 RVA: 0x0006ACF8 File Offset: 0x00068EF8
	public void SetBool(int hash, bool value)
	{
		if (this.m_animator.GetBool(hash) == value)
		{
			return;
		}
		this.m_animator.SetBool(hash, value);
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(438569 + hash, value);
		}
	}

	// Token: 0x0600102A RID: 4138 RVA: 0x0006AD54 File Offset: 0x00068F54
	public void SetFloat(string name, float value)
	{
		int hash = ZSyncAnimation.GetHash(name);
		this.SetFloat(hash, value);
	}

	// Token: 0x0600102B RID: 4139 RVA: 0x0006AD70 File Offset: 0x00068F70
	public void SetFloat(int hash, float value)
	{
		if (Mathf.Abs(this.m_animator.GetFloat(hash) - value) < 0.01f)
		{
			return;
		}
		if (this.m_smoothCharacterSpeeds && (hash == ZSyncAnimation.s_forwardSpeedID || hash == ZSyncAnimation.s_sidewaySpeedID))
		{
			this.m_animator.SetFloat(hash, value, 0.2f, Time.fixedDeltaTime);
		}
		else
		{
			this.m_animator.SetFloat(hash, value);
		}
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(438569 + hash, value);
		}
	}

	// Token: 0x0600102C RID: 4140 RVA: 0x0006AE08 File Offset: 0x00069008
	public void SetInt(string name, int value)
	{
		int hash = ZSyncAnimation.GetHash(name);
		this.SetInt(hash, value);
	}

	// Token: 0x0600102D RID: 4141 RVA: 0x0006AE24 File Offset: 0x00069024
	public void SetInt(int hash, int value)
	{
		if (this.m_animator.GetInteger(hash) == value)
		{
			return;
		}
		this.m_animator.SetInteger(hash, value);
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(438569 + hash, value, false);
		}
	}

	// Token: 0x0600102E RID: 4142 RVA: 0x0006AE81 File Offset: 0x00069081
	private void RPC_SetTrigger(long sender, string name)
	{
		this.m_animator.SetTrigger(name);
	}

	// Token: 0x0600102F RID: 4143 RVA: 0x0006AE8F File Offset: 0x0006908F
	public void SetSpeed(float speed)
	{
		this.m_animator.speed = speed;
	}

	// Token: 0x06001030 RID: 4144 RVA: 0x0006AE9D File Offset: 0x0006909D
	public bool IsOwner()
	{
		return this.m_nview.IsValid() && this.m_nview.IsOwner();
	}

	// Token: 0x170000A0 RID: 160
	// (get) Token: 0x06001031 RID: 4145 RVA: 0x0006AEB9 File Offset: 0x000690B9
	public static List<ZSyncAnimation> Instances { get; } = new List<ZSyncAnimation>();

	// Token: 0x04001110 RID: 4368
	private ZNetView m_nview;

	// Token: 0x04001111 RID: 4369
	private Animator m_animator;

	// Token: 0x04001112 RID: 4370
	public List<string> m_syncBools = new List<string>();

	// Token: 0x04001113 RID: 4371
	public List<string> m_syncFloats = new List<string>();

	// Token: 0x04001114 RID: 4372
	public List<string> m_syncInts = new List<string>();

	// Token: 0x04001115 RID: 4373
	public bool m_smoothCharacterSpeeds = true;

	// Token: 0x04001116 RID: 4374
	private static readonly int s_forwardSpeedID = ZSyncAnimation.GetHash("forward_speed");

	// Token: 0x04001117 RID: 4375
	private static readonly int s_sidewaySpeedID = ZSyncAnimation.GetHash("sideway_speed");

	// Token: 0x04001118 RID: 4376
	private static readonly int s_animSpeedID = ZSyncAnimation.GetHash("anim_speed");

	// Token: 0x04001119 RID: 4377
	private int[] m_boolHashes;

	// Token: 0x0400111A RID: 4378
	private bool[] m_boolDefaults;

	// Token: 0x0400111B RID: 4379
	private int[] m_floatHashes;

	// Token: 0x0400111C RID: 4380
	private float[] m_floatDefaults;

	// Token: 0x0400111D RID: 4381
	private int[] m_intHashes;

	// Token: 0x0400111E RID: 4382
	private int[] m_intDefaults;

	// Token: 0x0400111F RID: 4383
	private const int m_zdoSalt = 438569;
}
