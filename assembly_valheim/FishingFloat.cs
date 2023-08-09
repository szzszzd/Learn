using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000239 RID: 569
public class FishingFloat : MonoBehaviour, IProjectile
{
	// Token: 0x06001667 RID: 5735 RVA: 0x0000C988 File Offset: 0x0000AB88
	public string GetTooltipString(int itemQuality)
	{
		return "";
	}

	// Token: 0x06001668 RID: 5736 RVA: 0x000938F0 File Offset: 0x00091AF0
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_floating = base.GetComponent<Floating>();
		this.m_nview.Register<ZDOID, bool>("RPC_Nibble", new Action<long, ZDOID, bool>(this.RPC_Nibble));
		FishingFloat.m_allInstances.Add(this);
	}

	// Token: 0x06001669 RID: 5737 RVA: 0x00093948 File Offset: 0x00091B48
	private void OnDestroy()
	{
		FishingFloat.m_allInstances.Remove(this);
	}

	// Token: 0x0600166A RID: 5738 RVA: 0x00093958 File Offset: 0x00091B58
	public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)
	{
		FishingFloat fishingFloat = FishingFloat.FindFloat(owner);
		if (fishingFloat)
		{
			ZNetScene.instance.Destroy(fishingFloat.gameObject);
		}
		long userID = owner.GetZDOID().UserID;
		this.m_nview.GetZDO().Set(ZDOVars.s_rodOwner, userID);
		this.m_nview.GetZDO().Set(ZDOVars.s_bait, ammo.m_dropPrefab.name);
		Transform rodTop = this.GetRodTop(owner);
		if (rodTop == null)
		{
			ZLog.LogWarning("Failed to find fishing rod top");
			return;
		}
		this.m_rodLine.SetPeer(owner.GetZDOID());
		this.m_lineLength = Vector3.Distance(rodTop.position, base.transform.position);
		owner.Message(MessageHud.MessageType.Center, this.m_lineLength.ToString("0m"), 0, null);
	}

	// Token: 0x0600166B RID: 5739 RVA: 0x00093A2C File Offset: 0x00091C2C
	private Character GetOwner()
	{
		if (!this.m_nview.IsValid())
		{
			return null;
		}
		long @long = this.m_nview.GetZDO().GetLong(ZDOVars.s_rodOwner, 0L);
		foreach (ZNet.PlayerInfo playerInfo in ZNet.instance.GetPlayerList())
		{
			ZDOID characterID = playerInfo.m_characterID;
			if (characterID.UserID == @long)
			{
				GameObject gameObject = ZNetScene.instance.FindInstance(playerInfo.m_characterID);
				if (gameObject == null)
				{
					return null;
				}
				return gameObject.GetComponent<Character>();
			}
		}
		return null;
	}

	// Token: 0x0600166C RID: 5740 RVA: 0x00093AE4 File Offset: 0x00091CE4
	private Transform GetRodTop(Character owner)
	{
		Transform transform = Utils.FindChild(owner.transform, "_RodTop");
		if (transform == null)
		{
			ZLog.LogWarning("Failed to find fishing rod top");
			return null;
		}
		return transform;
	}

	// Token: 0x0600166D RID: 5741 RVA: 0x00093B18 File Offset: 0x00091D18
	private void FixedUpdate()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		Character owner = this.GetOwner();
		if (!owner)
		{
			ZLog.LogWarning("Fishing rod not found, destroying fishing float");
			this.m_nview.Destroy();
			return;
		}
		Transform rodTop = this.GetRodTop(owner);
		if (!rodTop)
		{
			ZLog.LogWarning("Fishing rod not found, destroying fishing float");
			this.m_nview.Destroy();
			return;
		}
		Fish fish = this.GetCatch();
		if (owner.InAttack() || owner.IsDrawingBow())
		{
			this.ReturnBait();
			if (fish)
			{
				fish.OnHooked(null);
			}
			this.m_nview.Destroy();
			return;
		}
		float magnitude = (rodTop.transform.position - base.transform.position).magnitude;
		ItemDrop itemDrop = fish ? fish.gameObject.GetComponent<ItemDrop>() : null;
		if (!owner.HaveStamina(0f) && fish != null)
		{
			this.SetCatch(null);
			fish = null;
			this.Message("$msg_fishing_lost", true);
		}
		float skillFactor = owner.GetSkillFactor(Skills.SkillType.Fishing);
		float num = Mathf.Lerp(this.m_hookedStaminaPerSec, this.m_hookedStaminaPerSecMaxSkill, skillFactor);
		if (fish)
		{
			owner.UseStamina(num * fixedDeltaTime);
		}
		if (!fish && Utils.LengthXZ(this.m_body.velocity) > 2f)
		{
			this.TryToHook();
		}
		if (owner.IsBlocking() && owner.HaveStamina(0f))
		{
			float num2 = this.m_pullStaminaUse;
			if (fish != null)
			{
				num2 += fish.GetStaminaUse() * (float)((itemDrop == null) ? 1 : itemDrop.m_itemData.m_quality);
			}
			num2 = Mathf.Lerp(num2, num2 * this.m_pullStaminaUseMaxSkillMultiplier, skillFactor);
			owner.UseStamina(num2 * fixedDeltaTime);
			if (this.m_lineLength > magnitude - 0.2f)
			{
				float lineLength = this.m_lineLength;
				float num3 = Mathf.Lerp(this.m_pullLineSpeed, this.m_pullLineSpeedMaxSkill, skillFactor);
				if (fish && fish.IsEscaping())
				{
					num3 /= 2f;
				}
				this.m_lineLength -= fixedDeltaTime * num3;
				this.m_fishingSkillImproveTimer += fixedDeltaTime * ((fish == null) ? 1f : this.m_fishingSkillImproveHookedMultiplier);
				if (this.m_fishingSkillImproveTimer > 1f)
				{
					this.m_fishingSkillImproveTimer = 0f;
					owner.RaiseSkill(Skills.SkillType.Fishing, 1f);
				}
				this.TryToHook();
				if ((int)this.m_lineLength != (int)lineLength)
				{
					this.Message(this.m_lineLength.ToString("0m"), false);
				}
			}
			if (this.m_lineLength <= 0.5f)
			{
				if (fish)
				{
					string msg = FishingFloat.Catch(fish, owner);
					this.Message(msg, true);
					this.SetCatch(null);
					fish.OnHooked(null);
					this.m_nview.Destroy();
					return;
				}
				this.ReturnBait();
				this.m_nview.Destroy();
				return;
			}
		}
		this.m_rodLine.SetSlack((1f - Utils.LerpStep(this.m_lineLength / 2f, this.m_lineLength, magnitude)) * this.m_maxLineSlack);
		if (magnitude - this.m_lineLength > this.m_breakDistance || magnitude > this.m_maxDistance)
		{
			this.Message("$msg_fishing_linebroke", true);
			if (fish)
			{
				fish.OnHooked(null);
			}
			this.m_nview.Destroy();
			this.m_lineBreakEffect.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
			return;
		}
		if (fish)
		{
			Utils.Pull(this.m_body, fish.transform.position, 0.5f, this.m_moveForce, 0.5f, 0.3f, false, false, 1f);
		}
		Utils.Pull(this.m_body, rodTop.transform.position, this.m_lineLength, this.m_moveForce, 1f, 0.3f, false, false, 1f);
	}

	// Token: 0x0600166E RID: 5742 RVA: 0x00093F14 File Offset: 0x00092114
	public static string Catch(Fish fish, Character owner)
	{
		Humanoid humanoid = owner as Humanoid;
		ItemDrop itemDrop = fish ? fish.gameObject.GetComponent<ItemDrop>() : null;
		if (itemDrop)
		{
			itemDrop.Pickup(humanoid);
		}
		else
		{
			fish.Pickup(humanoid);
		}
		string text = "$msg_fishing_catched " + fish.GetHoverName();
		if (!fish.m_extraDrops.IsEmpty())
		{
			foreach (ItemDrop.ItemData itemData in fish.m_extraDrops.GetDropListItems())
			{
				text = text + " & " + itemData.m_shared.m_name;
				if (humanoid.GetInventory().CanAddItem(itemData.m_dropPrefab, itemData.m_stack))
				{
					ZLog.Log(string.Format("picking up {0}x {1}", itemData.m_stack, itemData.m_dropPrefab.name));
					humanoid.GetInventory().AddItem(itemData.m_dropPrefab, itemData.m_stack);
				}
				else
				{
					ZLog.Log(string.Format("no room, dropping {0}x {1}", itemData.m_stack, itemData.m_dropPrefab.name));
					UnityEngine.Object.Instantiate<GameObject>(itemData.m_dropPrefab, fish.transform.position, Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f)).GetComponent<ItemDrop>().SetStack(itemData.m_stack);
					Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$inventory_full"), 0, null);
				}
			}
		}
		return text;
	}

	// Token: 0x0600166F RID: 5743 RVA: 0x000940D0 File Offset: 0x000922D0
	private void ReturnBait()
	{
		if (this.m_baitConsumed)
		{
			return;
		}
		Character owner = this.GetOwner();
		string bait = this.GetBait();
		GameObject prefab = ZNetScene.instance.GetPrefab(bait);
		if (prefab)
		{
			Player player = owner as Player;
			if (player != null)
			{
				player.GetInventory().AddItem(prefab, 1);
			}
		}
	}

	// Token: 0x06001670 RID: 5744 RVA: 0x00094120 File Offset: 0x00092320
	private void TryToHook()
	{
		if (this.m_nibbler != null && Time.time - this.m_nibbleTime < 0.5f && this.GetCatch() == null)
		{
			this.Message("$msg_fishing_hooked", true);
			this.SetCatch(this.m_nibbler);
			this.m_nibbler = null;
		}
	}

	// Token: 0x06001671 RID: 5745 RVA: 0x0009417C File Offset: 0x0009237C
	private void SetCatch(Fish fish)
	{
		if (fish)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_sessionCatchID, fish.GetZDOID());
			this.m_hookLine.SetPeer(fish.GetZDOID());
			fish.OnHooked(this);
			this.m_baitConsumed = true;
			return;
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_sessionCatchID, ZDOID.None);
		this.m_hookLine.SetPeer(ZDOID.None);
	}

	// Token: 0x06001672 RID: 5746 RVA: 0x000941F8 File Offset: 0x000923F8
	public Fish GetCatch()
	{
		if (!this.m_nview.IsValid())
		{
			return null;
		}
		ZDOID zdoid = this.m_nview.GetZDO().GetZDOID(ZDOVars.s_sessionCatchID);
		if (!zdoid.IsNone())
		{
			GameObject gameObject = ZNetScene.instance.FindInstance(zdoid);
			if (gameObject)
			{
				return gameObject.GetComponent<Fish>();
			}
		}
		return null;
	}

	// Token: 0x06001673 RID: 5747 RVA: 0x0009424F File Offset: 0x0009244F
	public string GetBait()
	{
		if (this.m_nview == null || this.m_nview.GetZDO() == null)
		{
			return null;
		}
		return this.m_nview.GetZDO().GetString(ZDOVars.s_bait, "");
	}

	// Token: 0x06001674 RID: 5748 RVA: 0x00094288 File Offset: 0x00092488
	public bool IsInWater()
	{
		return this.m_floating.HaveLiquidLevel();
	}

	// Token: 0x06001675 RID: 5749 RVA: 0x00094295 File Offset: 0x00092495
	public void Nibble(Fish fish, bool correctBait)
	{
		this.m_nview.InvokeRPC("RPC_Nibble", new object[]
		{
			fish.GetZDOID(),
			correctBait
		});
	}

	// Token: 0x06001676 RID: 5750 RVA: 0x000942C4 File Offset: 0x000924C4
	public void RPC_Nibble(long sender, ZDOID fishID, bool correctBait)
	{
		if (Time.time - this.m_nibbleTime < 1f)
		{
			return;
		}
		if (this.GetCatch() != null)
		{
			return;
		}
		if (correctBait)
		{
			this.m_nibbleEffect.Create(base.transform.position, Quaternion.identity, base.transform, 1f, -1);
			this.m_body.AddForce(Vector3.down * this.m_nibbleForce, ForceMode.VelocityChange);
			GameObject gameObject = ZNetScene.instance.FindInstance(fishID);
			if (gameObject)
			{
				this.m_nibbler = gameObject.GetComponent<Fish>();
				this.m_nibbleTime = Time.time;
				return;
			}
		}
		else
		{
			this.m_body.AddForce(Vector3.down * this.m_nibbleForce * 0.5f, ForceMode.VelocityChange);
			this.Message("$msg_fishing_wrongbait", true);
		}
	}

	// Token: 0x06001677 RID: 5751 RVA: 0x00094399 File Offset: 0x00092599
	public static List<FishingFloat> GetAllInstances()
	{
		return FishingFloat.m_allInstances;
	}

	// Token: 0x06001678 RID: 5752 RVA: 0x000943A0 File Offset: 0x000925A0
	private static FishingFloat FindFloat(Character owner)
	{
		foreach (FishingFloat fishingFloat in FishingFloat.m_allInstances)
		{
			if (owner == fishingFloat.GetOwner())
			{
				return fishingFloat;
			}
		}
		return null;
	}

	// Token: 0x06001679 RID: 5753 RVA: 0x00094400 File Offset: 0x00092600
	public static FishingFloat FindFloat(Fish fish)
	{
		foreach (FishingFloat fishingFloat in FishingFloat.m_allInstances)
		{
			if (fishingFloat.GetCatch() == fish)
			{
				return fishingFloat;
			}
		}
		return null;
	}

	// Token: 0x0600167A RID: 5754 RVA: 0x00094460 File Offset: 0x00092660
	private void Message(string msg, bool prioritized = false)
	{
		if (!prioritized && Time.time - this.m_msgTime < 1f)
		{
			return;
		}
		this.m_msgTime = Time.time;
		Character owner = this.GetOwner();
		if (owner)
		{
			owner.Message(MessageHud.MessageType.Center, Localization.instance.Localize(msg), 0, null);
		}
	}

	// Token: 0x04001792 RID: 6034
	public float m_maxDistance = 30f;

	// Token: 0x04001793 RID: 6035
	public float m_moveForce = 10f;

	// Token: 0x04001794 RID: 6036
	public float m_pullLineSpeed = 1f;

	// Token: 0x04001795 RID: 6037
	public float m_pullLineSpeedMaxSkill = 2f;

	// Token: 0x04001796 RID: 6038
	public float m_pullStaminaUse = 10f;

	// Token: 0x04001797 RID: 6039
	public float m_pullStaminaUseMaxSkillMultiplier = 0.2f;

	// Token: 0x04001798 RID: 6040
	public float m_hookedStaminaPerSec = 1f;

	// Token: 0x04001799 RID: 6041
	public float m_hookedStaminaPerSecMaxSkill = 0.2f;

	// Token: 0x0400179A RID: 6042
	private float m_fishingSkillImproveTimer;

	// Token: 0x0400179B RID: 6043
	private float m_fishingSkillImproveHookedMultiplier = 2f;

	// Token: 0x0400179C RID: 6044
	private bool m_baitConsumed;

	// Token: 0x0400179D RID: 6045
	public float m_breakDistance = 4f;

	// Token: 0x0400179E RID: 6046
	public float m_range = 10f;

	// Token: 0x0400179F RID: 6047
	public float m_nibbleForce = 10f;

	// Token: 0x040017A0 RID: 6048
	public EffectList m_nibbleEffect = new EffectList();

	// Token: 0x040017A1 RID: 6049
	public EffectList m_lineBreakEffect = new EffectList();

	// Token: 0x040017A2 RID: 6050
	public float m_maxLineSlack = 0.3f;

	// Token: 0x040017A3 RID: 6051
	public LineConnect m_rodLine;

	// Token: 0x040017A4 RID: 6052
	public LineConnect m_hookLine;

	// Token: 0x040017A5 RID: 6053
	private ZNetView m_nview;

	// Token: 0x040017A6 RID: 6054
	private Rigidbody m_body;

	// Token: 0x040017A7 RID: 6055
	private Floating m_floating;

	// Token: 0x040017A8 RID: 6056
	private float m_lineLength;

	// Token: 0x040017A9 RID: 6057
	private float m_msgTime;

	// Token: 0x040017AA RID: 6058
	private Fish m_nibbler;

	// Token: 0x040017AB RID: 6059
	private float m_nibbleTime;

	// Token: 0x040017AC RID: 6060
	private static List<FishingFloat> m_allInstances = new List<FishingFloat>();
}
