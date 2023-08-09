using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000006 RID: 6
public class CharacterAnimEvent : MonoBehaviour
{
	// Token: 0x060000F9 RID: 249 RVA: 0x00006D54 File Offset: 0x00004F54
	private void Awake()
	{
		this.m_character = base.GetComponentInParent<Character>();
		this.m_nview = this.m_character.GetComponent<ZNetView>();
		this.m_animator = base.GetComponent<Animator>();
		this.m_monsterAI = this.m_character.GetComponent<MonsterAI>();
		this.m_visEquipment = this.m_character.GetComponent<VisEquipment>();
		this.m_footStep = this.m_character.GetComponent<FootStep>();
		this.m_head = this.m_animator.GetBoneTransform(HumanBodyBones.Head);
		this.m_headLookDir = this.m_character.transform.forward;
		if (CharacterAnimEvent.s_ikGroundMask == 0)
		{
			CharacterAnimEvent.s_ikGroundMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"terrain",
				"vehicle"
			});
		}
	}

	// Token: 0x060000FA RID: 250 RVA: 0x00006E2D File Offset: 0x0000502D
	private void OnEnable()
	{
		CharacterAnimEvent.Instances.Add(this);
	}

	// Token: 0x060000FB RID: 251 RVA: 0x00006E3A File Offset: 0x0000503A
	private void OnDisable()
	{
		CharacterAnimEvent.Instances.Remove(this);
	}

	// Token: 0x060000FC RID: 252 RVA: 0x00006E48 File Offset: 0x00005048
	private void OnAnimatorMove()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.m_character.AddRootMotion(this.m_animator.deltaPosition);
	}

	// Token: 0x060000FD RID: 253 RVA: 0x00006E7C File Offset: 0x0000507C
	public void CustomFixedUpdate()
	{
		if (this.m_character == null)
		{
			return;
		}
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (!this.m_character.InAttack() && !this.m_character.InMinorAction() && !this.m_character.InEmote() && this.m_character.CanMove())
		{
			this.m_animator.speed = 1f;
		}
		this.UpdateFreezeFrame(Time.fixedDeltaTime);
	}

	// Token: 0x060000FE RID: 254 RVA: 0x00006EF5 File Offset: 0x000050F5
	public bool CanChain()
	{
		return this.m_chain;
	}

	// Token: 0x060000FF RID: 255 RVA: 0x00006F00 File Offset: 0x00005100
	public void FreezeFrame(float delay)
	{
		if (delay <= 0f)
		{
			return;
		}
		if (this.m_pauseTimer > 0f)
		{
			this.m_pauseTimer = delay;
			return;
		}
		this.m_pauseTimer = delay;
		this.m_pauseSpeed = this.m_animator.speed;
		this.m_animator.speed = 0.0001f;
		if (this.m_pauseSpeed <= 0.01f)
		{
			this.m_pauseSpeed = 1f;
		}
	}

	// Token: 0x06000100 RID: 256 RVA: 0x00006F6C File Offset: 0x0000516C
	private void UpdateFreezeFrame(float dt)
	{
		if (this.m_pauseTimer > 0f)
		{
			this.m_pauseTimer -= dt;
			if (this.m_pauseTimer <= 0f)
			{
				this.m_animator.speed = this.m_pauseSpeed;
			}
		}
		if (this.m_animator.speed < 0.01f && this.m_pauseTimer <= 0f)
		{
			this.m_animator.speed = 1f;
		}
	}

	// Token: 0x06000101 RID: 257 RVA: 0x00006FE1 File Offset: 0x000051E1
	public void Speed(float speedScale)
	{
		this.m_animator.speed = speedScale;
	}

	// Token: 0x06000102 RID: 258 RVA: 0x00006FEF File Offset: 0x000051EF
	public void Chain()
	{
		this.m_chain = true;
	}

	// Token: 0x06000103 RID: 259 RVA: 0x00006FF8 File Offset: 0x000051F8
	public void ResetChain()
	{
		this.m_chain = false;
	}

	// Token: 0x06000104 RID: 260 RVA: 0x00007004 File Offset: 0x00005204
	public void FootStep(AnimationEvent e)
	{
		if ((double)e.animatorClipInfo.weight < 0.33)
		{
			return;
		}
		if (this.m_footStep)
		{
			if (e.stringParameter.Length > 0)
			{
				this.m_footStep.OnFoot(e.stringParameter);
				return;
			}
			this.m_footStep.OnFoot();
		}
	}

	// Token: 0x06000105 RID: 261 RVA: 0x00007064 File Offset: 0x00005264
	public void Hit()
	{
		this.m_character.OnAttackTrigger();
	}

	// Token: 0x06000106 RID: 262 RVA: 0x00007064 File Offset: 0x00005264
	public void OnAttackTrigger()
	{
		this.m_character.OnAttackTrigger();
	}

	// Token: 0x06000107 RID: 263 RVA: 0x00007071 File Offset: 0x00005271
	public void Jump()
	{
		this.m_character.Jump(true);
	}

	// Token: 0x06000108 RID: 264 RVA: 0x0000707F File Offset: 0x0000527F
	public void Land()
	{
		if (this.m_character.IsFlying())
		{
			this.m_character.Land();
		}
	}

	// Token: 0x06000109 RID: 265 RVA: 0x00007099 File Offset: 0x00005299
	public void TakeOff()
	{
		if (!this.m_character.IsFlying())
		{
			this.m_character.TakeOff();
		}
	}

	// Token: 0x0600010A RID: 266 RVA: 0x000070B3 File Offset: 0x000052B3
	public void Stop(AnimationEvent e)
	{
		this.m_character.OnStopMoving();
	}

	// Token: 0x0600010B RID: 267 RVA: 0x000070C0 File Offset: 0x000052C0
	public void DodgeMortal()
	{
		Player player = this.m_character as Player;
		if (player)
		{
			player.OnDodgeMortal();
		}
	}

	// Token: 0x0600010C RID: 268 RVA: 0x000070E7 File Offset: 0x000052E7
	public void TrailOn()
	{
		if (this.m_visEquipment)
		{
			this.m_visEquipment.SetWeaponTrails(true);
		}
		this.m_character.OnWeaponTrailStart();
	}

	// Token: 0x0600010D RID: 269 RVA: 0x0000710D File Offset: 0x0000530D
	public void TrailOff()
	{
		if (this.m_visEquipment)
		{
			this.m_visEquipment.SetWeaponTrails(false);
		}
	}

	// Token: 0x0600010E RID: 270 RVA: 0x00007128 File Offset: 0x00005328
	public void GPower()
	{
		Player player = this.m_character as Player;
		if (player)
		{
			player.ActivateGuardianPower();
		}
	}

	// Token: 0x0600010F RID: 271 RVA: 0x00007150 File Offset: 0x00005350
	private void OnAnimatorIK(int layerIndex)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.UpdateLookat();
		this.UpdateFootIK();
	}

	// Token: 0x06000110 RID: 272 RVA: 0x0000716C File Offset: 0x0000536C
	public void CustomLateUpdate()
	{
		this.UpdateHeadRotation(Time.deltaTime);
		if (this.m_femaleHack)
		{
			Character character = this.m_character;
			float num = (this.m_visEquipment.GetModelIndex() == 1) ? this.m_femaleOffset : this.m_maleOffset;
			Vector3 localPosition = this.m_leftShoulder.localPosition;
			localPosition.x = -num;
			this.m_leftShoulder.localPosition = localPosition;
			Vector3 localPosition2 = this.m_rightShoulder.localPosition;
			localPosition2.x = num;
			this.m_rightShoulder.localPosition = localPosition2;
		}
	}

	// Token: 0x06000111 RID: 273 RVA: 0x000071F4 File Offset: 0x000053F4
	private void UpdateLookat()
	{
		if (this.m_headRotation && this.m_head)
		{
			float target = this.m_lookWeight;
			if (this.m_headLookDir != Vector3.zero)
			{
				this.m_animator.SetLookAtPosition(this.m_head.position + this.m_headLookDir * 10f);
			}
			if (this.m_character.InAttack() || (!this.m_character.IsPlayer() && !this.m_character.CanMove()))
			{
				target = 0f;
			}
			this.m_lookAtWeight = Mathf.MoveTowards(this.m_lookAtWeight, target, Time.deltaTime);
			float bodyWeight = this.m_character.IsAttached() ? 0f : this.m_bodyLookWeight;
			this.m_animator.SetLookAtWeight(this.m_lookAtWeight, bodyWeight, this.m_headLookWeight, this.m_eyeLookWeight, this.m_lookClamp);
		}
	}

	// Token: 0x06000112 RID: 274 RVA: 0x000072E4 File Offset: 0x000054E4
	private void UpdateFootIK()
	{
		if (!this.m_footIK)
		{
			return;
		}
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		if (Vector3.Distance(base.transform.position, mainCamera.transform.position) > 64f)
		{
			return;
		}
		if ((this.m_character.IsFlying() && !this.m_character.IsOnGround()) || (this.m_character.IsSwimming() && !this.m_character.IsOnGround()) || this.m_character.IsSitting())
		{
			for (int i = 0; i < this.m_feets.Length; i++)
			{
				CharacterAnimEvent.Foot foot = this.m_feets[i];
				this.m_animator.SetIKPositionWeight(foot.m_ikHandle, 0f);
				this.m_animator.SetIKRotationWeight(foot.m_ikHandle, 0f);
			}
			return;
		}
		bool flag = this.m_character.IsSitting();
		float deltaTime = Time.deltaTime;
		for (int j = 0; j < this.m_feets.Length; j++)
		{
			CharacterAnimEvent.Foot foot2 = this.m_feets[j];
			Vector3 position = foot2.m_transform.position;
			AvatarIKGoal ikHandle = foot2.m_ikHandle;
			float num = this.m_useFeetValues ? foot2.m_footDownMax : this.m_footDownMax;
			float d = this.m_useFeetValues ? foot2.m_footOffset : this.m_footOffset;
			float num2 = this.m_useFeetValues ? foot2.m_footStepHeight : this.m_footStepHeight;
			float num3 = this.m_useFeetValues ? foot2.m_stabalizeDistance : this.m_stabalizeDistance;
			if (flag)
			{
				num2 /= 4f;
			}
			Vector3 vector = base.transform.InverseTransformPoint(position - base.transform.up * d);
			float target = 1f - Mathf.Clamp01(vector.y / num);
			foot2.m_ikWeight = Mathf.MoveTowards(foot2.m_ikWeight, target, deltaTime * 10f);
			this.m_animator.SetIKPositionWeight(ikHandle, foot2.m_ikWeight);
			this.m_animator.SetIKRotationWeight(ikHandle, foot2.m_ikWeight * 0.5f);
			if (foot2.m_ikWeight > 0f)
			{
				RaycastHit raycastHit;
				if (Physics.Raycast(position + Vector3.up * num2, Vector3.down, out raycastHit, num2 * 4f, CharacterAnimEvent.s_ikGroundMask))
				{
					Vector3 vector2 = raycastHit.point + Vector3.up * d;
					Vector3 plantNormal = raycastHit.normal;
					if (num3 > 0f)
					{
						if (foot2.m_ikWeight >= 1f)
						{
							if (!foot2.m_isPlanted)
							{
								foot2.m_plantPosition = vector2;
								foot2.m_plantNormal = plantNormal;
								foot2.m_isPlanted = true;
							}
							else if (Vector3.Distance(foot2.m_plantPosition, vector2) > num3)
							{
								foot2.m_isPlanted = false;
							}
							else
							{
								vector2 = foot2.m_plantPosition;
								plantNormal = foot2.m_plantNormal;
							}
						}
						else
						{
							foot2.m_isPlanted = false;
						}
					}
					this.m_animator.SetIKPosition(ikHandle, vector2);
					Quaternion goalRotation = Quaternion.LookRotation(Vector3.Cross(this.m_animator.GetIKRotation(ikHandle) * Vector3.right, raycastHit.normal), raycastHit.normal);
					this.m_animator.SetIKRotation(ikHandle, goalRotation);
				}
				else
				{
					foot2.m_ikWeight = Mathf.MoveTowards(foot2.m_ikWeight, 0f, deltaTime * 4f);
					this.m_animator.SetIKPositionWeight(ikHandle, foot2.m_ikWeight);
					this.m_animator.SetIKRotationWeight(ikHandle, foot2.m_ikWeight * 0.5f);
				}
			}
		}
	}

	// Token: 0x06000113 RID: 275 RVA: 0x0000767C File Offset: 0x0000587C
	private void UpdateHeadRotation(float dt)
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_headRotation && this.m_head)
		{
			Vector3 lookFromPos = this.GetLookFromPos();
			Vector3 vector = Vector3.zero;
			if (this.m_nview.IsOwner())
			{
				if (this.m_monsterAI != null)
				{
					Character targetCreature = this.m_monsterAI.GetTargetCreature();
					if (targetCreature != null)
					{
						vector = targetCreature.GetEyePoint();
					}
				}
				else
				{
					vector = lookFromPos + this.m_character.GetLookDir() * 100f;
				}
				if (this.m_lookAt != null)
				{
					vector = this.m_lookAt.position;
				}
				this.m_sendTimer += Time.deltaTime;
				if (this.m_sendTimer > 0.2f)
				{
					this.m_sendTimer = 0f;
					this.m_nview.GetZDO().Set(ZDOVars.s_lookTarget, vector);
				}
			}
			else
			{
				vector = this.m_nview.GetZDO().GetVec3(ZDOVars.s_lookTarget, Vector3.zero);
			}
			if (vector != Vector3.zero)
			{
				Vector3 b = Vector3.Normalize(vector - lookFromPos);
				this.m_headLookDir = Vector3.Lerp(this.m_headLookDir, b, 0.1f);
				return;
			}
			this.m_headLookDir = this.m_character.transform.forward;
		}
	}

	// Token: 0x06000114 RID: 276 RVA: 0x000077E4 File Offset: 0x000059E4
	private Vector3 GetLookFromPos()
	{
		if (this.m_eyes != null && this.m_eyes.Length != 0)
		{
			Vector3 a = Vector3.zero;
			foreach (Transform transform in this.m_eyes)
			{
				a += transform.position;
			}
			return a / (float)this.m_eyes.Length;
		}
		return this.m_head.position;
	}

	// Token: 0x06000115 RID: 277 RVA: 0x0000784C File Offset: 0x00005A4C
	public void FindJoints()
	{
		ZLog.Log("Finding joints");
		List<Transform> list = new List<Transform>();
		Transform transform = Utils.FindChild(base.transform, "LeftEye");
		Transform transform2 = Utils.FindChild(base.transform, "RightEye");
		if (transform)
		{
			list.Add(transform);
		}
		if (transform2)
		{
			list.Add(transform2);
		}
		this.m_eyes = list.ToArray();
		Transform transform3 = Utils.FindChild(base.transform, "LeftFootFront");
		Transform transform4 = Utils.FindChild(base.transform, "RightFootFront");
		Transform transform5 = Utils.FindChild(base.transform, "LeftFoot");
		if (transform5 == null)
		{
			transform5 = Utils.FindChild(base.transform, "LeftFootBack");
		}
		if (transform5 == null)
		{
			transform5 = Utils.FindChild(base.transform, "l_foot");
		}
		if (transform5 == null)
		{
			transform5 = Utils.FindChild(base.transform, "Foot.l");
		}
		if (transform5 == null)
		{
			transform5 = Utils.FindChild(base.transform, "foot.l");
		}
		Transform transform6 = Utils.FindChild(base.transform, "RightFoot");
		if (transform6 == null)
		{
			transform6 = Utils.FindChild(base.transform, "RightFootBack");
		}
		if (transform6 == null)
		{
			transform6 = Utils.FindChild(base.transform, "r_foot");
		}
		if (transform6 == null)
		{
			transform6 = Utils.FindChild(base.transform, "Foot.r");
		}
		if (transform6 == null)
		{
			transform6 = Utils.FindChild(base.transform, "foot.r");
		}
		List<CharacterAnimEvent.Foot> list2 = new List<CharacterAnimEvent.Foot>();
		if (transform3)
		{
			list2.Add(new CharacterAnimEvent.Foot(transform3, AvatarIKGoal.LeftHand));
		}
		if (transform4)
		{
			list2.Add(new CharacterAnimEvent.Foot(transform4, AvatarIKGoal.RightHand));
		}
		if (transform5)
		{
			list2.Add(new CharacterAnimEvent.Foot(transform5, AvatarIKGoal.LeftFoot));
		}
		if (transform6)
		{
			list2.Add(new CharacterAnimEvent.Foot(transform6, AvatarIKGoal.RightFoot));
		}
		this.m_feets = list2.ToArray();
	}

	// Token: 0x06000116 RID: 278 RVA: 0x00007A40 File Offset: 0x00005C40
	private void OnDrawGizmosSelected()
	{
		if (this.m_footIK)
		{
			foreach (CharacterAnimEvent.Foot foot in this.m_feets)
			{
				float d = this.m_useFeetValues ? foot.m_footDownMax : this.m_footDownMax;
				float d2 = this.m_useFeetValues ? foot.m_footOffset : this.m_footOffset;
				float d3 = this.m_useFeetValues ? foot.m_footStepHeight : this.m_footStepHeight;
				float num = this.m_useFeetValues ? foot.m_stabalizeDistance : this.m_stabalizeDistance;
				Vector3 vector = foot.m_transform.position - base.transform.up * d2;
				Gizmos.color = ((vector.y > base.transform.position.y) ? Color.red : Color.white);
				Gizmos.DrawWireSphere(vector, 0.1f);
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireCube(new Vector3(vector.x, base.transform.position.y, vector.z) + Vector3.up * d, new Vector3(1f, 0.01f, 1f));
				Gizmos.color = Color.red;
				Gizmos.DrawLine(vector, vector + Vector3.up * d3);
				if (num > 0f)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawWireSphere(vector, num);
					Gizmos.matrix = Matrix4x4.identity;
				}
				if (foot.m_isPlanted)
				{
					Gizmos.color = Color.yellow;
					Gizmos.DrawWireCube(vector, new Vector3(0.4f, 0.3f, 0.4f));
				}
			}
		}
	}

	// Token: 0x17000003 RID: 3
	// (get) Token: 0x06000117 RID: 279 RVA: 0x00007BFD File Offset: 0x00005DFD
	public static List<CharacterAnimEvent> Instances { get; } = new List<CharacterAnimEvent>();

	// Token: 0x040000CA RID: 202
	[Header("Foot IK")]
	public bool m_footIK;

	// Token: 0x040000CB RID: 203
	public float m_footDownMax = 0.4f;

	// Token: 0x040000CC RID: 204
	public float m_footOffset = 0.1f;

	// Token: 0x040000CD RID: 205
	public float m_footStepHeight = 1f;

	// Token: 0x040000CE RID: 206
	public float m_stabalizeDistance;

	// Token: 0x040000CF RID: 207
	public bool m_useFeetValues;

	// Token: 0x040000D0 RID: 208
	public CharacterAnimEvent.Foot[] m_feets = Array.Empty<CharacterAnimEvent.Foot>();

	// Token: 0x040000D1 RID: 209
	[Header("Head/eye rotation")]
	public bool m_headRotation = true;

	// Token: 0x040000D2 RID: 210
	public Transform[] m_eyes;

	// Token: 0x040000D3 RID: 211
	public float m_lookWeight = 0.5f;

	// Token: 0x040000D4 RID: 212
	public float m_bodyLookWeight = 0.1f;

	// Token: 0x040000D5 RID: 213
	public float m_headLookWeight = 1f;

	// Token: 0x040000D6 RID: 214
	public float m_eyeLookWeight;

	// Token: 0x040000D7 RID: 215
	public float m_lookClamp = 0.5f;

	// Token: 0x040000D8 RID: 216
	private const float m_headRotationSmoothness = 0.1f;

	// Token: 0x040000D9 RID: 217
	public Transform m_lookAt;

	// Token: 0x040000DA RID: 218
	[Header("Player Female hack")]
	public bool m_femaleHack;

	// Token: 0x040000DB RID: 219
	public Transform m_leftShoulder;

	// Token: 0x040000DC RID: 220
	public Transform m_rightShoulder;

	// Token: 0x040000DD RID: 221
	public float m_femaleOffset = 0.0004f;

	// Token: 0x040000DE RID: 222
	public float m_maleOffset = 0.0007651657f;

	// Token: 0x040000DF RID: 223
	private Character m_character;

	// Token: 0x040000E0 RID: 224
	private Animator m_animator;

	// Token: 0x040000E1 RID: 225
	private ZNetView m_nview;

	// Token: 0x040000E2 RID: 226
	private MonsterAI m_monsterAI;

	// Token: 0x040000E3 RID: 227
	private VisEquipment m_visEquipment;

	// Token: 0x040000E4 RID: 228
	private FootStep m_footStep;

	// Token: 0x040000E5 RID: 229
	private float m_pauseTimer;

	// Token: 0x040000E6 RID: 230
	private float m_pauseSpeed = 1f;

	// Token: 0x040000E7 RID: 231
	private float m_sendTimer;

	// Token: 0x040000E8 RID: 232
	private Vector3 m_headLookDir;

	// Token: 0x040000E9 RID: 233
	private float m_lookAtWeight;

	// Token: 0x040000EA RID: 234
	private Transform m_head;

	// Token: 0x040000EB RID: 235
	private bool m_chain;

	// Token: 0x040000EC RID: 236
	private static int s_ikGroundMask = 0;

	// Token: 0x02000007 RID: 7
	[Serializable]
	public class Foot
	{
		// Token: 0x0600011A RID: 282 RVA: 0x00007CAC File Offset: 0x00005EAC
		public Foot(Transform t, AvatarIKGoal handle)
		{
			this.m_transform = t;
			this.m_ikHandle = handle;
			this.m_ikWeight = 0f;
		}

		// Token: 0x040000EE RID: 238
		public Transform m_transform;

		// Token: 0x040000EF RID: 239
		public AvatarIKGoal m_ikHandle;

		// Token: 0x040000F0 RID: 240
		public float m_footDownMax = 0.4f;

		// Token: 0x040000F1 RID: 241
		public float m_footOffset = 0.1f;

		// Token: 0x040000F2 RID: 242
		public float m_footStepHeight = 1f;

		// Token: 0x040000F3 RID: 243
		public float m_stabalizeDistance;

		// Token: 0x040000F4 RID: 244
		[NonSerialized]
		public float m_ikWeight;

		// Token: 0x040000F5 RID: 245
		[NonSerialized]
		public Vector3 m_plantPosition = Vector3.zero;

		// Token: 0x040000F6 RID: 246
		[NonSerialized]
		public Vector3 m_plantNormal = Vector3.up;

		// Token: 0x040000F7 RID: 247
		[NonSerialized]
		public bool m_isPlanted;
	}
}
