﻿using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000038 RID: 56
public class Tameable : MonoBehaviour, Interactable, TextReceiver
{
	// Token: 0x06000343 RID: 835 RVA: 0x00018E68 File Offset: 0x00017068
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_character = base.GetComponent<Character>();
		this.m_monsterAI = base.GetComponent<MonsterAI>();
		Character character = this.m_character;
		character.m_onDeath = (Action)Delegate.Combine(character.m_onDeath, new Action(this.OnDeath));
		MonsterAI monsterAI = this.m_monsterAI;
		monsterAI.m_onConsumedItem = (Action<ItemDrop>)Delegate.Combine(monsterAI.m_onConsumedItem, new Action<ItemDrop>(this.OnConsumedItem));
		if (this.m_nview.IsValid())
		{
			this.m_nview.Register<ZDOID, bool>("Command", new Action<long, ZDOID, bool>(this.RPC_Command));
			this.m_nview.Register<string, string>("SetName", new Action<long, string, string>(this.RPC_SetName));
			this.m_nview.Register("RPC_UnSummon", new Action<long>(this.RPC_UnSummon));
			if (this.m_saddle != null)
			{
				this.m_nview.Register("AddSaddle", new Action<long>(this.RPC_AddSaddle));
				this.m_nview.Register<bool>("SetSaddle", new Action<long, bool>(this.RPC_SetSaddle));
				this.SetSaddle(this.HaveSaddle());
			}
			base.InvokeRepeating("TamingUpdate", 3f, 3f);
		}
		if (this.m_startsTamed)
		{
			this.m_character.SetTamed(true);
		}
		if (this.m_randomStartingName.Count > 0 && this.m_nview.IsValid() && this.m_nview.GetZDO().GetString(ZDOVars.s_tamedName, "").Length == 0)
		{
			this.SetText(Localization.instance.Localize(this.m_randomStartingName[UnityEngine.Random.Range(0, this.m_randomStartingName.Count)]));
		}
	}

	// Token: 0x06000344 RID: 836 RVA: 0x0001902E File Offset: 0x0001722E
	public void Update()
	{
		this.UpdateSummon();
		this.UpdateSavedFollowTarget();
	}

	// Token: 0x06000345 RID: 837 RVA: 0x0001903C File Offset: 0x0001723C
	public string GetHoverText()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		string text = Localization.instance.Localize(this.m_character.m_name);
		if (this.m_character.IsTamed())
		{
			text += Localization.instance.Localize(" ( $hud_tame, " + this.GetStatusString() + " )");
			text += Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] $hud_pet");
			if (ZInput.InputLayout == InputLayout.Alternative1 && ZInput.IsGamepadActive())
			{
				text += Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltKeys + $KEY_Use</b></color>] $hud_rename");
			}
			else
			{
				text += Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $hud_rename");
			}
			return text;
		}
		int tameness = this.GetTameness();
		if (tameness <= 0)
		{
			text += Localization.instance.Localize(" ( $hud_wild, " + this.GetStatusString() + " )");
		}
		else
		{
			text += Localization.instance.Localize(string.Concat(new string[]
			{
				" ( $hud_tameness  ",
				tameness.ToString(),
				"%, ",
				this.GetStatusString(),
				" )"
			}));
		}
		return text;
	}

	// Token: 0x06000346 RID: 838 RVA: 0x00019175 File Offset: 0x00017375
	public string GetStatusString()
	{
		if (this.m_monsterAI.IsAlerted())
		{
			return "$hud_tamefrightened";
		}
		if (this.IsHungry())
		{
			return "$hud_tamehungry";
		}
		if (this.m_character.IsTamed())
		{
			return "$hud_tamehappy";
		}
		return "$hud_tameinprogress";
	}

	// Token: 0x06000347 RID: 839 RVA: 0x000191B0 File Offset: 0x000173B0
	public bool Interact(Humanoid user, bool hold, bool alt)
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (hold)
		{
			return false;
		}
		if (alt)
		{
			this.SetName();
			return true;
		}
		string hoverName = this.m_character.GetHoverName();
		if (!this.m_character.IsTamed())
		{
			return false;
		}
		if (Time.time - this.m_lastPetTime > 1f)
		{
			this.m_lastPetTime = Time.time;
			this.m_petEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			if (this.m_commandable)
			{
				this.Command(user, true);
			}
			else
			{
				user.Message(MessageHud.MessageType.Center, hoverName + " $hud_tamelove", 0, null);
			}
			return true;
		}
		return false;
	}

	// Token: 0x06000348 RID: 840 RVA: 0x00019268 File Offset: 0x00017468
	public string GetHoverName()
	{
		if (!this.m_character.IsTamed())
		{
			return Localization.instance.Localize(this.m_character.m_name);
		}
		string text = this.GetText().RemoveRichTextTags();
		if (text.Length > 0)
		{
			return text;
		}
		return Localization.instance.Localize(this.m_character.m_name);
	}

	// Token: 0x06000349 RID: 841 RVA: 0x000192C4 File Offset: 0x000174C4
	private void SetName()
	{
		if (!this.m_character.IsTamed())
		{
			return;
		}
		TextInput.instance.RequestText(this, "$hud_rename", 10);
	}

	// Token: 0x0600034A RID: 842 RVA: 0x000192E6 File Offset: 0x000174E6
	public string GetText()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		return this.m_nview.GetZDO().GetString(ZDOVars.s_tamedName, "");
	}

	// Token: 0x0600034B RID: 843 RVA: 0x00019315 File Offset: 0x00017515
	public void SetText(string text)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.InvokeRPC("SetName", new object[]
		{
			text,
			PrivilegeManager.GetNetworkUserId()
		});
	}

	// Token: 0x0600034C RID: 844 RVA: 0x00019348 File Offset: 0x00017548
	private void RPC_SetName(long sender, string name, string authorId)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.m_character.IsTamed())
		{
			return;
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_tamedName, name);
		this.m_nview.GetZDO().Set(ZDOVars.s_tamedNameAuthor, authorId);
	}

	// Token: 0x0600034D RID: 845 RVA: 0x000193AC File Offset: 0x000175AC
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (!(this.m_saddleItem != null) || !this.m_character.IsTamed() || !(item.m_shared.m_name == this.m_saddleItem.m_itemData.m_shared.m_name))
		{
			return false;
		}
		if (this.HaveSaddle())
		{
			user.Message(MessageHud.MessageType.Center, this.m_character.GetHoverName() + " $hud_saddle_already", 0, null);
			return true;
		}
		this.m_nview.InvokeRPC("AddSaddle", Array.Empty<object>());
		user.GetInventory().RemoveOneItem(item);
		user.Message(MessageHud.MessageType.Center, this.m_character.GetHoverName() + " $hud_saddle_ready", 0, null);
		return true;
	}

	// Token: 0x0600034E RID: 846 RVA: 0x0001947C File Offset: 0x0001767C
	private void RPC_AddSaddle(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.HaveSaddle())
		{
			return;
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_haveSaddleHash, true);
		this.m_nview.InvokeRPC(ZNetView.Everybody, "SetSaddle", new object[]
		{
			true
		});
	}

	// Token: 0x0600034F RID: 847 RVA: 0x000194DC File Offset: 0x000176DC
	public bool DropSaddle(Vector3 userPoint)
	{
		if (!this.HaveSaddle())
		{
			return false;
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_haveSaddleHash, false);
		this.m_nview.InvokeRPC(ZNetView.Everybody, "SetSaddle", new object[]
		{
			false
		});
		Vector3 flyDirection = userPoint - base.transform.position;
		this.SpawnSaddle(flyDirection);
		return true;
	}

	// Token: 0x06000350 RID: 848 RVA: 0x00019548 File Offset: 0x00017748
	private void SpawnSaddle(Vector3 flyDirection)
	{
		Rigidbody component = UnityEngine.Object.Instantiate<GameObject>(this.m_saddleItem.gameObject, base.transform.TransformPoint(this.m_dropSaddleOffset), Quaternion.identity).GetComponent<Rigidbody>();
		if (component)
		{
			Vector3 a = Vector3.up;
			if (flyDirection.magnitude > 0.1f)
			{
				flyDirection.y = 0f;
				flyDirection.Normalize();
				a += flyDirection;
			}
			component.AddForce(a * this.m_dropItemVel, ForceMode.VelocityChange);
		}
	}

	// Token: 0x06000351 RID: 849 RVA: 0x000195CB File Offset: 0x000177CB
	private bool HaveSaddle()
	{
		return !(this.m_saddle == null) && this.m_nview.IsValid() && this.m_nview.GetZDO().GetBool(ZDOVars.s_haveSaddleHash, false);
	}

	// Token: 0x06000352 RID: 850 RVA: 0x00019602 File Offset: 0x00017802
	private void RPC_SetSaddle(long sender, bool enabled)
	{
		this.SetSaddle(enabled);
	}

	// Token: 0x06000353 RID: 851 RVA: 0x0001960B File Offset: 0x0001780B
	private void SetSaddle(bool enabled)
	{
		ZLog.Log("Setting saddle:" + enabled.ToString());
		if (this.m_saddle != null)
		{
			this.m_saddle.gameObject.SetActive(enabled);
		}
	}

	// Token: 0x06000354 RID: 852 RVA: 0x00019644 File Offset: 0x00017844
	private void TamingUpdate()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_character.IsTamed())
		{
			return;
		}
		if (this.IsHungry())
		{
			return;
		}
		if (this.m_monsterAI.IsAlerted())
		{
			return;
		}
		this.m_monsterAI.SetDespawnInDay(false);
		this.m_monsterAI.SetEventCreature(false);
		this.DecreaseRemainingTime(3f);
		if (this.GetRemainingTime() <= 0f)
		{
			this.Tame();
			return;
		}
		this.m_sootheEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
	}

	// Token: 0x06000355 RID: 853 RVA: 0x000196F4 File Offset: 0x000178F4
	private void Tame()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_character.IsTamed())
		{
			return;
		}
		this.m_monsterAI.MakeTame();
		this.m_tamedEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 30f);
		if (closestPlayer)
		{
			closestPlayer.Message(MessageHud.MessageType.Center, this.m_character.m_name + " $hud_tamedone", 0, null);
		}
	}

	// Token: 0x06000356 RID: 854 RVA: 0x0001979C File Offset: 0x0001799C
	public static void TameAllInArea(Vector3 point, float radius)
	{
		foreach (Character character in Character.GetAllCharacters())
		{
			if (!character.IsPlayer())
			{
				Tameable component = character.GetComponent<Tameable>();
				if (component)
				{
					component.Tame();
				}
			}
		}
	}

	// Token: 0x06000357 RID: 855 RVA: 0x00019804 File Offset: 0x00017A04
	public void Command(Humanoid user, bool message = true)
	{
		this.m_nview.InvokeRPC("Command", new object[]
		{
			user.GetZDOID(),
			message
		});
	}

	// Token: 0x06000358 RID: 856 RVA: 0x00019834 File Offset: 0x00017A34
	private Player GetPlayer(ZDOID characterID)
	{
		GameObject gameObject = ZNetScene.instance.FindInstance(characterID);
		if (gameObject)
		{
			return gameObject.GetComponent<Player>();
		}
		return null;
	}

	// Token: 0x06000359 RID: 857 RVA: 0x00019860 File Offset: 0x00017A60
	private void RPC_Command(long sender, ZDOID characterID, bool message)
	{
		Player player = this.GetPlayer(characterID);
		if (player == null)
		{
			return;
		}
		if (this.m_monsterAI.GetFollowTarget())
		{
			this.m_monsterAI.SetFollowTarget(null);
			this.m_monsterAI.SetPatrolPoint();
			if (this.m_nview.IsOwner())
			{
				this.m_nview.GetZDO().Set(ZDOVars.s_follow, "");
			}
			if (message)
			{
				player.Message(MessageHud.MessageType.Center, this.m_character.GetHoverName() + " $hud_tamestay", 0, null);
			}
		}
		else
		{
			this.m_monsterAI.ResetPatrolPoint();
			this.m_monsterAI.SetFollowTarget(player.gameObject);
			if (this.m_nview.IsOwner())
			{
				this.m_nview.GetZDO().Set(ZDOVars.s_follow, player.GetPlayerName());
			}
			if (message)
			{
				player.Message(MessageHud.MessageType.Center, this.m_character.GetHoverName() + " $hud_tamefollow", 0, null);
			}
			int @int = this.m_nview.GetZDO().GetInt(ZDOVars.s_maxInstances, 0);
			if (@int > 0)
			{
				this.UnsummonMaxInstances(@int);
			}
		}
		this.m_unsummonTime = 0f;
	}

	// Token: 0x0600035A RID: 858 RVA: 0x0001998C File Offset: 0x00017B8C
	private void UpdateSavedFollowTarget()
	{
		if (this.m_monsterAI.GetFollowTarget() != null || !this.m_nview.IsOwner())
		{
			return;
		}
		string @string = this.m_nview.GetZDO().GetString(ZDOVars.s_follow, "");
		if (string.IsNullOrEmpty(@string))
		{
			return;
		}
		foreach (Player player in Player.GetAllPlayers())
		{
			if (player.GetPlayerName() == @string)
			{
				this.Command(player, false);
				return;
			}
		}
		if (this.m_unsummonOnOwnerLogoutSeconds > 0f)
		{
			this.m_unsummonTime += Time.fixedDeltaTime;
			if (this.m_unsummonTime > this.m_unsummonOnOwnerLogoutSeconds)
			{
				this.UnSummon();
			}
		}
	}

	// Token: 0x0600035B RID: 859 RVA: 0x00019A68 File Offset: 0x00017C68
	public bool IsHungry()
	{
		if (this.m_nview == null)
		{
			return false;
		}
		if (this.m_nview.GetZDO() == null)
		{
			return false;
		}
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong(ZDOVars.s_tameLastFeeding, 0L));
		return (ZNet.instance.GetTime() - d).TotalSeconds > (double)this.m_fedDuration;
	}

	// Token: 0x0600035C RID: 860 RVA: 0x00019AD4 File Offset: 0x00017CD4
	private void ResetFeedingTimer()
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_tameLastFeeding, ZNet.instance.GetTime().Ticks);
	}

	// Token: 0x0600035D RID: 861 RVA: 0x00019B08 File Offset: 0x00017D08
	private void OnDeath()
	{
		ZLog.Log("Valid " + this.m_nview.IsValid().ToString());
		ZLog.Log("On death " + this.HaveSaddle().ToString());
		if (this.HaveSaddle() && this.m_dropSaddleOnDeath)
		{
			ZLog.Log("Spawning saddle ");
			this.SpawnSaddle(Vector3.zero);
		}
	}

	// Token: 0x0600035E RID: 862 RVA: 0x00019B7C File Offset: 0x00017D7C
	private int GetTameness()
	{
		float remainingTime = this.GetRemainingTime();
		return (int)((1f - Mathf.Clamp01(remainingTime / this.m_tamingTime)) * 100f);
	}

	// Token: 0x0600035F RID: 863 RVA: 0x00019BAA File Offset: 0x00017DAA
	private void OnConsumedItem(ItemDrop item)
	{
		if (this.IsHungry())
		{
			this.m_sootheEffect.Create(this.m_character.GetCenterPoint(), Quaternion.identity, null, 1f, -1);
		}
		this.ResetFeedingTimer();
	}

	// Token: 0x06000360 RID: 864 RVA: 0x00019BE0 File Offset: 0x00017DE0
	private void DecreaseRemainingTime(float time)
	{
		float num = this.GetRemainingTime();
		num -= time;
		if (num < 0f)
		{
			num = 0f;
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_tameTimeLeft, num);
	}

	// Token: 0x06000361 RID: 865 RVA: 0x00019C1C File Offset: 0x00017E1C
	private float GetRemainingTime()
	{
		return this.m_nview.GetZDO().GetFloat(ZDOVars.s_tameTimeLeft, this.m_tamingTime);
	}

	// Token: 0x06000362 RID: 866 RVA: 0x00019C39 File Offset: 0x00017E39
	public bool HaveRider()
	{
		return this.m_saddle && this.m_saddle.HaveValidUser();
	}

	// Token: 0x06000363 RID: 867 RVA: 0x00019C55 File Offset: 0x00017E55
	public float GetRiderSkill()
	{
		if (this.m_saddle)
		{
			return this.m_saddle.GetRiderSkill();
		}
		return 0f;
	}

	// Token: 0x06000364 RID: 868 RVA: 0x00019C78 File Offset: 0x00017E78
	private void UpdateSummon()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_unsummonDistance > 0f && this.m_monsterAI)
		{
			GameObject followTarget = this.m_monsterAI.GetFollowTarget();
			if (followTarget && Vector3.Distance(followTarget.transform.position, base.gameObject.transform.position) > this.m_unsummonDistance)
			{
				this.UnSummon();
			}
		}
	}

	// Token: 0x06000365 RID: 869 RVA: 0x00019CFC File Offset: 0x00017EFC
	private void UnsummonMaxInstances(int maxInstances)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		GameObject followTarget = this.m_monsterAI.GetFollowTarget();
		string text;
		if (followTarget != null)
		{
			Player component = followTarget.GetComponent<Player>();
			if (component != null)
			{
				text = component.GetPlayerName();
				goto IL_3D;
			}
		}
		text = null;
		IL_3D:
		string text2 = text;
		if (text2 == null)
		{
			return;
		}
		List<Character> allCharacters = Character.GetAllCharacters();
		List<BaseAI> list = new List<BaseAI>();
		foreach (Character character in allCharacters)
		{
			if (character.m_name == this.m_character.m_name)
			{
				ZNetView component2 = character.GetComponent<ZNetView>();
				if (component2 == null)
				{
					goto IL_92;
				}
				ZDO zdo = component2.GetZDO();
				if (zdo == null)
				{
					goto IL_92;
				}
				string a2 = zdo.GetString(ZDOVars.s_follow, "");
				IL_AA:
				if (!(a2 == text2))
				{
					continue;
				}
				MonsterAI component3 = character.GetComponent<MonsterAI>();
				if (component3 != null)
				{
					list.Add(component3);
					continue;
				}
				continue;
				IL_92:
				a2 = "";
				goto IL_AA;
			}
		}
		list.Sort((BaseAI a, BaseAI b) => b.GetTimeSinceSpawned().CompareTo(a.GetTimeSinceSpawned()));
		int num = list.Count - maxInstances;
		for (int i = 0; i < num; i++)
		{
			Tameable component4 = list[i].GetComponent<Tameable>();
			if (component4 != null)
			{
				component4.UnSummon();
			}
		}
		if (num > 0 && Player.m_localPlayer)
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$hud_maxsummonsreached", 0, null);
		}
	}

	// Token: 0x06000366 RID: 870 RVA: 0x00019E78 File Offset: 0x00018078
	private void UnSummon()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.InvokeRPC(ZNetView.Everybody, "RPC_UnSummon", Array.Empty<object>());
	}

	// Token: 0x06000367 RID: 871 RVA: 0x00019EA4 File Offset: 0x000180A4
	private void RPC_UnSummon(long sender)
	{
		this.m_unSummonEffect.Create(base.gameObject.transform.position, base.gameObject.transform.rotation, null, 1f, -1);
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		ZNetScene.instance.Destroy(base.gameObject);
	}

	// Token: 0x04000335 RID: 821
	private const float m_playerMaxDistance = 15f;

	// Token: 0x04000336 RID: 822
	private const float m_tameDeltaTime = 3f;

	// Token: 0x04000337 RID: 823
	public float m_fedDuration = 30f;

	// Token: 0x04000338 RID: 824
	public float m_tamingTime = 1800f;

	// Token: 0x04000339 RID: 825
	public bool m_startsTamed;

	// Token: 0x0400033A RID: 826
	public EffectList m_tamedEffect = new EffectList();

	// Token: 0x0400033B RID: 827
	public EffectList m_sootheEffect = new EffectList();

	// Token: 0x0400033C RID: 828
	public EffectList m_petEffect = new EffectList();

	// Token: 0x0400033D RID: 829
	public bool m_commandable;

	// Token: 0x0400033E RID: 830
	public float m_unsummonDistance;

	// Token: 0x0400033F RID: 831
	public float m_unsummonOnOwnerLogoutSeconds;

	// Token: 0x04000340 RID: 832
	public EffectList m_unSummonEffect = new EffectList();

	// Token: 0x04000341 RID: 833
	public Skills.SkillType m_levelUpOwnerSkill;

	// Token: 0x04000342 RID: 834
	public float m_levelUpFactor = 1f;

	// Token: 0x04000343 RID: 835
	public ItemDrop m_saddleItem;

	// Token: 0x04000344 RID: 836
	public Sadle m_saddle;

	// Token: 0x04000345 RID: 837
	public bool m_dropSaddleOnDeath = true;

	// Token: 0x04000346 RID: 838
	public Vector3 m_dropSaddleOffset = new Vector3(0f, 1f, 0f);

	// Token: 0x04000347 RID: 839
	public float m_dropItemVel = 5f;

	// Token: 0x04000348 RID: 840
	public List<string> m_randomStartingName = new List<string>();

	// Token: 0x04000349 RID: 841
	private Character m_character;

	// Token: 0x0400034A RID: 842
	private MonsterAI m_monsterAI;

	// Token: 0x0400034B RID: 843
	private ZNetView m_nview;

	// Token: 0x0400034C RID: 844
	private float m_lastPetTime;

	// Token: 0x0400034D RID: 845
	private float m_unsummonTime;
}
