using System;
using UnityEngine;

// Token: 0x0200002E RID: 46
public class Sadle : MonoBehaviour, Interactable, Hoverable, IDoodadController
{
	// Token: 0x060002F9 RID: 761 RVA: 0x00017440 File Offset: 0x00015640
	private void Awake()
	{
		this.m_character = base.gameObject.GetComponentInParent<Character>();
		this.m_nview = this.m_character.GetComponent<ZNetView>();
		this.m_tambable = this.m_character.GetComponent<Tameable>();
		this.m_monsterAI = this.m_character.GetComponent<MonsterAI>();
		this.m_nview.Register<long>("RequestControl", new Action<long, long>(this.RPC_RequestControl));
		this.m_nview.Register<long>("ReleaseControl", new Action<long, long>(this.RPC_ReleaseControl));
		this.m_nview.Register<bool>("RequestRespons", new Action<long, bool>(this.RPC_RequestRespons));
		this.m_nview.Register<Vector3>("RemoveSaddle", new Action<long, Vector3>(this.RPC_RemoveSaddle));
		this.m_nview.Register<Vector3, int, float>("Controls", new Action<long, Vector3, int, float>(this.RPC_Controls));
	}

	// Token: 0x060002FA RID: 762 RVA: 0x0001751D File Offset: 0x0001571D
	public bool IsValid()
	{
		return this;
	}

	// Token: 0x060002FB RID: 763 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x060002FC RID: 764 RVA: 0x00017528 File Offset: 0x00015728
	private void FixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (!this.m_character.IsTamed())
		{
			return;
		}
		if (this.IsLocalUser())
		{
			this.UpdateRidingSkill(Time.fixedDeltaTime);
		}
		if (this.m_nview.IsOwner())
		{
			float fixedDeltaTime = Time.fixedDeltaTime;
			this.UpdateStamina(fixedDeltaTime);
			this.UpdateDrown(fixedDeltaTime);
		}
	}

	// Token: 0x060002FD RID: 765 RVA: 0x00017588 File Offset: 0x00015788
	private void UpdateDrown(float dt)
	{
		if (this.m_character.IsSwimming() && !this.m_character.IsOnGround() && !this.HaveStamina(0f))
		{
			this.m_drownDamageTimer += dt;
			if (this.m_drownDamageTimer > 1f)
			{
				this.m_drownDamageTimer = 0f;
				float damage = Mathf.Ceil(this.m_character.GetMaxHealth() / 20f);
				HitData hitData = new HitData();
				hitData.m_damage.m_damage = damage;
				hitData.m_point = this.m_character.GetCenterPoint();
				hitData.m_dir = Vector3.down;
				hitData.m_pushForce = 10f;
				this.m_character.Damage(hitData);
				Vector3 position = base.transform.position;
				position.y = this.m_character.GetLiquidLevel();
				this.m_drownEffects.Create(position, base.transform.rotation, null, 1f, -1);
			}
		}
	}

	// Token: 0x060002FE RID: 766 RVA: 0x00017688 File Offset: 0x00015888
	public bool UpdateRiding(float dt)
	{
		if (!base.isActiveAndEnabled)
		{
			return false;
		}
		if (!this.m_character.IsTamed())
		{
			return false;
		}
		if (!this.HaveValidUser())
		{
			return false;
		}
		if (this.m_speed == Sadle.Speed.Stop || this.m_controlDir.magnitude == 0f)
		{
			return false;
		}
		if (this.m_speed == Sadle.Speed.Walk || this.m_speed == Sadle.Speed.Run)
		{
			if (this.m_speed == Sadle.Speed.Run && !this.HaveStamina(0f))
			{
				this.m_speed = Sadle.Speed.Walk;
			}
			this.m_monsterAI.MoveTowards(this.m_controlDir, this.m_speed == Sadle.Speed.Run);
			float riderSkill = this.GetRiderSkill();
			float num = Mathf.Lerp(1f, 0.5f, riderSkill);
			if (this.m_character.IsSwimming())
			{
				this.UseStamina(this.m_swimStaminaDrain * num * dt);
			}
			else if (this.m_speed == Sadle.Speed.Run)
			{
				this.UseStamina(this.m_runStaminaDrain * num * dt);
			}
		}
		else if (this.m_speed == Sadle.Speed.Turn)
		{
			this.m_monsterAI.StopMoving();
			this.m_character.SetRun(false);
			this.m_monsterAI.LookTowards(this.m_controlDir);
		}
		this.m_monsterAI.ResetRandomMovement();
		return true;
	}

	// Token: 0x060002FF RID: 767 RVA: 0x000177B0 File Offset: 0x000159B0
	public string GetHoverText()
	{
		if (!this.InUseDistance(Player.m_localPlayer))
		{
			return Localization.instance.Localize("<color=gray>$piece_toofar</color>");
		}
		string text = Localization.instance.Localize(this.m_hoverText);
		text += Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
		if (ZInput.InputLayout == InputLayout.Alternative1 && ZInput.IsGamepadActive())
		{
			text += Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltKeys + $KEY_Use</b></color>] $hud_saddle_remove");
		}
		else
		{
			text += Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $hud_saddle_remove");
		}
		return text;
	}

	// Token: 0x06000300 RID: 768 RVA: 0x0001783F File Offset: 0x00015A3F
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_hoverText);
	}

	// Token: 0x06000301 RID: 769 RVA: 0x00017854 File Offset: 0x00015A54
	public bool Interact(Humanoid character, bool repeat, bool alt)
	{
		if (repeat)
		{
			return false;
		}
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (!this.InUseDistance(character))
		{
			return false;
		}
		if (!this.m_character.IsTamed())
		{
			return false;
		}
		Player player = character as Player;
		if (player == null)
		{
			return false;
		}
		if (alt)
		{
			this.m_nview.InvokeRPC("RemoveSaddle", new object[]
			{
				character.transform.position
			});
			return true;
		}
		this.m_nview.InvokeRPC("RequestControl", new object[]
		{
			player.GetZDOID().UserID
		});
		return false;
	}

	// Token: 0x06000302 RID: 770 RVA: 0x000178FC File Offset: 0x00015AFC
	public Character GetCharacter()
	{
		return this.m_character;
	}

	// Token: 0x06000303 RID: 771 RVA: 0x00017904 File Offset: 0x00015B04
	public Tameable GetTameable()
	{
		return this.m_tambable;
	}

	// Token: 0x06000304 RID: 772 RVA: 0x0001790C File Offset: 0x00015B0C
	public void ApplyControlls(Vector3 moveDir, Vector3 lookDir, bool run, bool autoRun, bool block)
	{
		if (Player.m_localPlayer == null)
		{
			return;
		}
		float skillFactor = Player.m_localPlayer.GetSkills().GetSkillFactor(Skills.SkillType.Ride);
		Sadle.Speed speed = Sadle.Speed.NoChange;
		Vector3 vector = Vector3.zero;
		if (block || (double)moveDir.z > 0.5 || run)
		{
			Vector3 vector2 = lookDir;
			vector2.y = 0f;
			vector2.Normalize();
			vector = vector2;
		}
		if (run)
		{
			speed = Sadle.Speed.Run;
		}
		else if ((double)moveDir.z > 0.5)
		{
			speed = Sadle.Speed.Walk;
		}
		else if ((double)moveDir.z < -0.5)
		{
			speed = Sadle.Speed.Stop;
		}
		else if (block)
		{
			speed = Sadle.Speed.Turn;
		}
		this.m_nview.InvokeRPC("Controls", new object[]
		{
			vector,
			(int)speed,
			skillFactor
		});
	}

	// Token: 0x06000305 RID: 773 RVA: 0x000179E0 File Offset: 0x00015BE0
	private void RPC_Controls(long sender, Vector3 rideDir, int rideSpeed, float skill)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.m_rideSkill = skill;
		if (rideDir != Vector3.zero)
		{
			this.m_controlDir = rideDir;
		}
		if (rideSpeed == 4)
		{
			if (this.m_speed == Sadle.Speed.Turn)
			{
				this.m_speed = Sadle.Speed.Stop;
			}
			return;
		}
		if (rideSpeed == 3 && (this.m_speed == Sadle.Speed.Walk || this.m_speed == Sadle.Speed.Run))
		{
			return;
		}
		this.m_speed = (Sadle.Speed)rideSpeed;
	}

	// Token: 0x06000306 RID: 774 RVA: 0x00017A4C File Offset: 0x00015C4C
	private void UpdateRidingSkill(float dt)
	{
		this.m_raiseSkillTimer += dt;
		if (this.m_raiseSkillTimer > 1f)
		{
			this.m_raiseSkillTimer = 0f;
			if (this.m_speed == Sadle.Speed.Run)
			{
				Player.m_localPlayer.RaiseSkill(Skills.SkillType.Ride, 1f);
			}
		}
	}

	// Token: 0x06000307 RID: 775 RVA: 0x00017A99 File Offset: 0x00015C99
	private void ResetControlls()
	{
		this.m_controlDir = Vector3.zero;
		this.m_speed = Sadle.Speed.Stop;
		this.m_rideSkill = 0f;
	}

	// Token: 0x06000308 RID: 776 RVA: 0x000178FC File Offset: 0x00015AFC
	public Component GetControlledComponent()
	{
		return this.m_character;
	}

	// Token: 0x06000309 RID: 777 RVA: 0x00017AB8 File Offset: 0x00015CB8
	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	// Token: 0x0600030A RID: 778 RVA: 0x00017AC5 File Offset: 0x00015CC5
	private void RPC_RemoveSaddle(long sender, Vector3 userPoint)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.HaveValidUser())
		{
			return;
		}
		this.m_tambable.DropSaddle(userPoint);
	}

	// Token: 0x0600030B RID: 779 RVA: 0x00017AEC File Offset: 0x00015CEC
	private void RPC_RequestControl(long sender, long playerID)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.GetUser() == playerID || !this.HaveValidUser())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_user, playerID);
			this.ResetControlls();
			this.m_nview.InvokeRPC(sender, "RequestRespons", new object[]
			{
				true
			});
			this.m_nview.GetZDO().SetOwner(sender);
			return;
		}
		this.m_nview.InvokeRPC(sender, "RequestRespons", new object[]
		{
			false
		});
	}

	// Token: 0x0600030C RID: 780 RVA: 0x00017B88 File Offset: 0x00015D88
	public bool HaveValidUser()
	{
		long user = this.GetUser();
		if (user == 0L)
		{
			return false;
		}
		foreach (ZDO zdo in ZNet.instance.GetAllCharacterZDOS())
		{
			if (zdo.m_uid.UserID == user)
			{
				return Vector3.Distance(zdo.GetPosition(), base.transform.position) < this.m_maxUseRange;
			}
		}
		return false;
	}

	// Token: 0x0600030D RID: 781 RVA: 0x00017C18 File Offset: 0x00015E18
	private void RPC_ReleaseControl(long sender, long playerID)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.GetUser() == playerID)
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_user, 0L);
			this.ResetControlls();
		}
	}

	// Token: 0x0600030E RID: 782 RVA: 0x00017C50 File Offset: 0x00015E50
	private void RPC_RequestRespons(long sender, bool granted)
	{
		if (!Player.m_localPlayer)
		{
			return;
		}
		if (granted)
		{
			Player.m_localPlayer.StartDoodadControl(this);
			if (this.m_attachPoint != null)
			{
				Player.m_localPlayer.AttachStart(this.m_attachPoint, this.m_character.gameObject, false, false, false, this.m_attachAnimation, this.m_detachOffset);
				return;
			}
		}
		else
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_inuse", 0, null);
		}
	}

	// Token: 0x0600030F RID: 783 RVA: 0x00017CC4 File Offset: 0x00015EC4
	public void OnUseStop(Player player)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.InvokeRPC("ReleaseControl", new object[]
		{
			player.GetZDOID().UserID
		});
		if (this.m_attachPoint != null)
		{
			player.AttachStop();
		}
	}

	// Token: 0x06000310 RID: 784 RVA: 0x00017D20 File Offset: 0x00015F20
	private bool IsLocalUser()
	{
		if (!Player.m_localPlayer)
		{
			return false;
		}
		long user = this.GetUser();
		return user != 0L && user == Player.m_localPlayer.GetPlayerID();
	}

	// Token: 0x06000311 RID: 785 RVA: 0x00017D54 File Offset: 0x00015F54
	private long GetUser()
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return 0L;
		}
		return this.m_nview.GetZDO().GetLong(ZDOVars.s_user, 0L);
	}

	// Token: 0x06000312 RID: 786 RVA: 0x00017D8B File Offset: 0x00015F8B
	private bool InUseDistance(Humanoid human)
	{
		return Vector3.Distance(human.transform.position, this.m_attachPoint.position) < this.m_maxUseRange;
	}

	// Token: 0x06000313 RID: 787 RVA: 0x00017DB0 File Offset: 0x00015FB0
	private void UseStamina(float v)
	{
		if (v == 0f)
		{
			return;
		}
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		float num = this.GetStamina();
		num -= v;
		if (num < 0f)
		{
			num = 0f;
		}
		this.SetStamina(num);
		this.m_staminaRegenTimer = 1f;
	}

	// Token: 0x06000314 RID: 788 RVA: 0x00017E0C File Offset: 0x0001600C
	private bool HaveStamina(float amount = 0f)
	{
		return this.m_nview.IsValid() && this.GetStamina() > amount;
	}

	// Token: 0x06000315 RID: 789 RVA: 0x00017E28 File Offset: 0x00016028
	public float GetStamina()
	{
		if (this.m_nview == null)
		{
			return 0f;
		}
		if (this.m_nview.GetZDO() == null)
		{
			return 0f;
		}
		return this.m_nview.GetZDO().GetFloat(ZDOVars.s_stamina, this.GetMaxStamina());
	}

	// Token: 0x06000316 RID: 790 RVA: 0x00017E77 File Offset: 0x00016077
	private void SetStamina(float stamina)
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_stamina, stamina);
	}

	// Token: 0x06000317 RID: 791 RVA: 0x00017E8F File Offset: 0x0001608F
	public float GetMaxStamina()
	{
		return this.m_maxStamina;
	}

	// Token: 0x06000318 RID: 792 RVA: 0x00017E98 File Offset: 0x00016098
	private void UpdateStamina(float dt)
	{
		this.m_staminaRegenTimer -= dt;
		if (this.m_staminaRegenTimer > 0f)
		{
			return;
		}
		if (this.m_character.InAttack() || this.m_character.IsSwimming())
		{
			return;
		}
		float num = this.GetStamina();
		float maxStamina = this.GetMaxStamina();
		if (num < maxStamina || num > maxStamina)
		{
			float num2 = this.m_tambable.IsHungry() ? this.m_staminaRegenHungry : this.m_staminaRegen;
			float num3 = num2 + (1f - num / maxStamina) * num2;
			num += num3 * dt;
			if (num > maxStamina)
			{
				num = maxStamina;
			}
			this.SetStamina(num);
		}
	}

	// Token: 0x06000319 RID: 793 RVA: 0x00017F2F File Offset: 0x0001612F
	public float GetRiderSkill()
	{
		return this.m_rideSkill;
	}

	// Token: 0x040002D5 RID: 725
	public string m_hoverText = "";

	// Token: 0x040002D6 RID: 726
	public float m_maxUseRange = 10f;

	// Token: 0x040002D7 RID: 727
	public Transform m_attachPoint;

	// Token: 0x040002D8 RID: 728
	public Vector3 m_detachOffset = new Vector3(0f, 0.5f, 0f);

	// Token: 0x040002D9 RID: 729
	public string m_attachAnimation = "attach_chair";

	// Token: 0x040002DA RID: 730
	public float m_maxStamina = 100f;

	// Token: 0x040002DB RID: 731
	public float m_runStaminaDrain = 10f;

	// Token: 0x040002DC RID: 732
	public float m_swimStaminaDrain = 10f;

	// Token: 0x040002DD RID: 733
	public float m_staminaRegen = 10f;

	// Token: 0x040002DE RID: 734
	public float m_staminaRegenHungry = 10f;

	// Token: 0x040002DF RID: 735
	public EffectList m_drownEffects = new EffectList();

	// Token: 0x040002E0 RID: 736
	private const float m_staminaRegenDelay = 1f;

	// Token: 0x040002E1 RID: 737
	private Vector3 m_controlDir;

	// Token: 0x040002E2 RID: 738
	private Sadle.Speed m_speed;

	// Token: 0x040002E3 RID: 739
	private float m_rideSkill;

	// Token: 0x040002E4 RID: 740
	private float m_staminaRegenTimer;

	// Token: 0x040002E5 RID: 741
	private float m_drownDamageTimer;

	// Token: 0x040002E6 RID: 742
	private float m_raiseSkillTimer;

	// Token: 0x040002E7 RID: 743
	private Character m_character;

	// Token: 0x040002E8 RID: 744
	private ZNetView m_nview;

	// Token: 0x040002E9 RID: 745
	private Tameable m_tambable;

	// Token: 0x040002EA RID: 746
	private MonsterAI m_monsterAI;

	// Token: 0x0200002F RID: 47
	private enum Speed
	{
		// Token: 0x040002EC RID: 748
		Stop,
		// Token: 0x040002ED RID: 749
		Walk,
		// Token: 0x040002EE RID: 750
		Run,
		// Token: 0x040002EF RID: 751
		Turn,
		// Token: 0x040002F0 RID: 752
		NoChange
	}
}
