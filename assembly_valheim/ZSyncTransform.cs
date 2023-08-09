using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200018E RID: 398
public class ZSyncTransform : MonoBehaviour
{
	// Token: 0x06001034 RID: 4148 RVA: 0x0006AF2C File Offset: 0x0006912C
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_projectile = base.GetComponent<Projectile>();
		this.m_character = base.GetComponent<Character>();
		if (this.m_nview.GetZDO() == null)
		{
			base.enabled = false;
			return;
		}
		if (this.m_body)
		{
			this.m_isKinematicBody = this.m_body.isKinematic;
			this.m_useGravity = this.m_body.useGravity;
		}
		this.m_wasOwner = this.m_nview.GetZDO().IsOwner();
	}

	// Token: 0x06001035 RID: 4149 RVA: 0x0006AFC3 File Offset: 0x000691C3
	private void OnEnable()
	{
		ZSyncTransform.Instances.Add(this);
	}

	// Token: 0x06001036 RID: 4150 RVA: 0x0006AFD0 File Offset: 0x000691D0
	private void OnDisable()
	{
		ZSyncTransform.Instances.Remove(this);
	}

	// Token: 0x06001037 RID: 4151 RVA: 0x0006AFDE File Offset: 0x000691DE
	private Vector3 GetVelocity()
	{
		if (this.m_body != null)
		{
			return this.m_body.velocity;
		}
		if (this.m_projectile != null)
		{
			return this.m_projectile.GetVelocity();
		}
		return Vector3.zero;
	}

	// Token: 0x06001038 RID: 4152 RVA: 0x0006B019 File Offset: 0x00069219
	private Vector3 GetPosition()
	{
		if (!this.m_body)
		{
			return base.transform.position;
		}
		return this.m_body.position;
	}

	// Token: 0x06001039 RID: 4153 RVA: 0x0006B040 File Offset: 0x00069240
	private void OwnerSync()
	{
		ZDO zdo = this.m_nview.GetZDO();
		bool flag = zdo.IsOwner();
		bool flag2 = !this.m_wasOwner && flag;
		this.m_wasOwner = flag;
		if (!flag)
		{
			return;
		}
		if (flag2)
		{
			if (this.m_syncPosition)
			{
				base.transform.position = zdo.GetPosition();
			}
			if (this.m_syncRotation)
			{
				base.transform.rotation = zdo.GetRotation();
			}
			if (this.m_syncBodyVelocity && this.m_body)
			{
				this.m_body.velocity = Vector3.zero;
				this.m_body.angularVelocity = Vector3.zero;
				this.m_body.Sleep();
			}
		}
		if (base.transform.position.y < -5000f)
		{
			if (this.m_body)
			{
				this.m_body.velocity = Vector3.zero;
			}
			ZLog.Log("Object fell out of world:" + base.gameObject.name);
			float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
			Vector3 position = base.transform.position;
			position.y = groundHeight + 1f;
			base.transform.position = position;
			return;
		}
		if (this.m_syncPosition)
		{
			zdo.SetPosition(this.GetPosition());
			zdo.Set(ZDOVars.s_velHash, this.GetVelocity());
			if (this.m_characterParentSync)
			{
				if (this.GetRelativePosition(zdo, out this.m_tempParent, out this.m_tempAttachJoint, out this.m_tempRelativePos, out this.m_tempRelativeRot, out this.m_tempRelativeVel))
				{
					zdo.SetConnection(ZDOExtraData.ConnectionType.SyncTransform, this.m_tempParent);
					zdo.Set(ZDOVars.s_attachJointHash, this.m_tempAttachJoint);
					zdo.Set(ZDOVars.s_relPosHash, this.m_tempRelativePos);
					zdo.Set(ZDOVars.s_relRotHash, this.m_tempRelativeRot);
					zdo.Set(ZDOVars.s_velHash, this.m_tempRelativeVel);
				}
				else
				{
					zdo.UpdateConnection(ZDOExtraData.ConnectionType.SyncTransform, ZDOID.None);
					zdo.Set(ZDOVars.s_attachJointHash, "");
				}
			}
		}
		if (this.m_syncRotation && base.transform.hasChanged)
		{
			Quaternion rotation = this.m_body ? this.m_body.rotation : base.transform.rotation;
			zdo.SetRotation(rotation);
		}
		if (this.m_syncScale && base.transform.hasChanged)
		{
			if (Mathf.Approximately(base.transform.localScale.x, base.transform.localScale.y) && Mathf.Approximately(base.transform.localScale.x, base.transform.localScale.z))
			{
				zdo.RemoveVec3(ZDOVars.s_scaleHash);
				zdo.Set(ZDOVars.s_scaleScalarHash, base.transform.localScale.x);
			}
			else
			{
				zdo.RemoveFloat(ZDOVars.s_scaleScalarHash);
				zdo.Set(ZDOVars.s_scaleHash, base.transform.localScale);
			}
		}
		if (this.m_body)
		{
			if (this.m_syncBodyVelocity)
			{
				this.m_nview.GetZDO().Set(ZDOVars.s_bodyVelHash, this.m_body.velocity);
				this.m_nview.GetZDO().Set(ZDOVars.s_bodyAVelHash, this.m_body.angularVelocity);
			}
			this.m_body.useGravity = this.m_useGravity;
		}
		base.transform.hasChanged = false;
	}

	// Token: 0x0600103A RID: 4154 RVA: 0x0006B3AC File Offset: 0x000695AC
	private bool GetRelativePosition(ZDO zdo, out ZDOID parent, out string attachJoint, out Vector3 relativePos, out Quaternion relativeRot, out Vector3 relativeVel)
	{
		if (this.m_character)
		{
			return this.m_character.GetRelativePosition(out parent, out attachJoint, out relativePos, out relativeRot, out relativeVel);
		}
		if (base.transform.parent)
		{
			ZNetView znetView = base.transform.parent ? base.transform.parent.GetComponent<ZNetView>() : null;
			if (znetView && znetView.IsValid())
			{
				parent = znetView.GetZDO().m_uid;
				attachJoint = "";
				relativePos = base.transform.localPosition;
				relativeRot = base.transform.localRotation;
				relativeVel = Vector3.zero;
				return true;
			}
		}
		parent = ZDOID.None;
		attachJoint = "";
		relativePos = Vector3.zero;
		relativeRot = Quaternion.identity;
		relativeVel = Vector3.zero;
		return false;
	}

	// Token: 0x0600103B RID: 4155 RVA: 0x0006B4A8 File Offset: 0x000696A8
	private void SyncPosition(ZDO zdo, float dt, out bool usedLocalRotation)
	{
		usedLocalRotation = false;
		if (this.m_characterParentSync && zdo.HasOwner())
		{
			ZDOID connectionZDOID = zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.SyncTransform);
			if (!connectionZDOID.IsNone())
			{
				GameObject gameObject = ZNetScene.instance.FindInstance(connectionZDOID);
				if (gameObject)
				{
					ZSyncTransform component = gameObject.GetComponent<ZSyncTransform>();
					if (component)
					{
						component.ClientSync(dt);
					}
					string @string = zdo.GetString(ZDOVars.s_attachJointHash, "");
					Vector3 vector = zdo.GetVec3(ZDOVars.s_relPosHash, Vector3.zero);
					Quaternion quaternion = zdo.GetQuaternion(ZDOVars.s_relRotHash, Quaternion.identity);
					Vector3 vec = zdo.GetVec3(ZDOVars.s_velHash, Vector3.zero);
					if (zdo.DataRevision != this.m_posRevision)
					{
						this.m_posRevision = zdo.DataRevision;
						this.m_targetPosTimer = 0f;
					}
					if (@string.Length > 0)
					{
						Transform transform = Utils.FindChild(gameObject.transform, @string);
						if (transform)
						{
							base.transform.position = transform.position;
						}
					}
					else
					{
						this.m_targetPosTimer += dt;
						this.m_targetPosTimer = Mathf.Min(this.m_targetPosTimer, 2f);
						vector += vec * this.m_targetPosTimer;
						if (!this.m_haveTempRelPos)
						{
							this.m_haveTempRelPos = true;
							this.m_tempRelPos = vector;
						}
						if (Vector3.Distance(this.m_tempRelPos, vector) > 0.001f)
						{
							this.m_tempRelPos = Vector3.Lerp(this.m_tempRelPos, vector, 0.2f);
							vector = this.m_tempRelPos;
						}
						Vector3 vector2 = gameObject.transform.TransformPoint(vector);
						if (Vector3.Distance(base.transform.position, vector2) > 0.001f)
						{
							base.transform.position = vector2;
						}
					}
					Quaternion a = Quaternion.Inverse(gameObject.transform.rotation) * base.transform.rotation;
					if (Quaternion.Angle(a, quaternion) > 0.001f)
					{
						Quaternion rhs = Quaternion.Slerp(a, quaternion, 0.5f);
						base.transform.rotation = gameObject.transform.rotation * rhs;
					}
					usedLocalRotation = true;
					return;
				}
			}
		}
		this.m_haveTempRelPos = false;
		Vector3 vector3 = zdo.GetPosition();
		if (zdo.DataRevision != this.m_posRevision)
		{
			this.m_posRevision = zdo.DataRevision;
			this.m_targetPosTimer = 0f;
		}
		if (zdo.HasOwner())
		{
			this.m_targetPosTimer += dt;
			this.m_targetPosTimer = Mathf.Min(this.m_targetPosTimer, 2f);
			Vector3 vec2 = zdo.GetVec3(ZDOVars.s_velHash, Vector3.zero);
			vector3 += vec2 * this.m_targetPosTimer;
		}
		float num = Vector3.Distance(base.transform.position, vector3);
		if (num > 0.001f)
		{
			base.transform.position = ((num < 5f) ? Vector3.Lerp(base.transform.position, vector3, 0.2f) : vector3);
		}
	}

	// Token: 0x0600103C RID: 4156 RVA: 0x0006B7A8 File Offset: 0x000699A8
	private void ClientSync(float dt)
	{
		ZDO zdo = this.m_nview.GetZDO();
		if (zdo.IsOwner())
		{
			return;
		}
		int frameCount = Time.frameCount;
		if (this.m_lastUpdateFrame == frameCount)
		{
			return;
		}
		this.m_lastUpdateFrame = frameCount;
		if (this.m_isKinematicBody)
		{
			if (this.m_syncPosition)
			{
				Vector3 vector = zdo.GetPosition();
				if (Vector3.Distance(this.m_body.position, vector) > 5f)
				{
					this.m_body.position = vector;
				}
				else
				{
					if (Vector3.Distance(this.m_body.position, vector) > 0.01f)
					{
						vector = Vector3.Lerp(this.m_body.position, vector, 0.2f);
					}
					this.m_body.MovePosition(vector);
				}
			}
			if (this.m_syncRotation)
			{
				Quaternion rotation = zdo.GetRotation();
				if (Quaternion.Angle(this.m_body.rotation, rotation) > 45f)
				{
					this.m_body.rotation = rotation;
				}
				else
				{
					this.m_body.MoveRotation(rotation);
				}
			}
		}
		else
		{
			bool flag = false;
			if (this.m_syncPosition)
			{
				this.SyncPosition(zdo, dt, out flag);
			}
			if (this.m_syncRotation && !flag)
			{
				Quaternion rotation2 = zdo.GetRotation();
				if (Quaternion.Angle(base.transform.rotation, rotation2) > 0.001f)
				{
					base.transform.rotation = Quaternion.Slerp(base.transform.rotation, rotation2, 0.5f);
				}
			}
			if (this.m_body)
			{
				this.m_body.useGravity = false;
				if (this.m_syncBodyVelocity && this.m_nview.HasOwner())
				{
					Vector3 vec = zdo.GetVec3(ZDOVars.s_bodyVelHash, Vector3.zero);
					Vector3 vec2 = zdo.GetVec3(ZDOVars.s_bodyAVelHash, Vector3.zero);
					if (vec.magnitude > 0.01f || vec2.magnitude > 0.01f)
					{
						this.m_body.velocity = vec;
						this.m_body.angularVelocity = vec2;
					}
					else
					{
						this.m_body.Sleep();
					}
				}
				else if (!this.m_body.IsSleeping())
				{
					this.m_body.velocity = Vector3.zero;
					this.m_body.angularVelocity = Vector3.zero;
					this.m_body.Sleep();
				}
			}
		}
		if (this.m_syncScale)
		{
			Vector3 vec3 = zdo.GetVec3(ZDOVars.s_scaleHash, Vector3.zero);
			if (vec3 != Vector3.zero)
			{
				base.transform.localScale = vec3;
				return;
			}
			float @float = zdo.GetFloat(ZDOVars.s_scaleScalarHash, base.transform.localScale.x);
			if (!base.transform.localScale.x.Equals(@float))
			{
				base.transform.localScale = new Vector3(@float, @float, @float);
			}
		}
	}

	// Token: 0x0600103D RID: 4157 RVA: 0x0006BA64 File Offset: 0x00069C64
	public void CustomFixedUpdate(float fixedDeltaTime)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.ClientSync(fixedDeltaTime);
	}

	// Token: 0x0600103E RID: 4158 RVA: 0x0006BA7B File Offset: 0x00069C7B
	public void CustomLateUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.OwnerSync();
	}

	// Token: 0x0600103F RID: 4159 RVA: 0x0006BA7B File Offset: 0x00069C7B
	public void SyncNow()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.OwnerSync();
	}

	// Token: 0x170000A1 RID: 161
	// (get) Token: 0x06001040 RID: 4160 RVA: 0x0006BA91 File Offset: 0x00069C91
	public static List<ZSyncTransform> Instances { get; } = new List<ZSyncTransform>();

	// Token: 0x04001121 RID: 4385
	public bool m_syncPosition = true;

	// Token: 0x04001122 RID: 4386
	public bool m_syncRotation = true;

	// Token: 0x04001123 RID: 4387
	public bool m_syncScale;

	// Token: 0x04001124 RID: 4388
	public bool m_syncBodyVelocity;

	// Token: 0x04001125 RID: 4389
	public bool m_characterParentSync;

	// Token: 0x04001126 RID: 4390
	private const float m_smoothnessPos = 0.2f;

	// Token: 0x04001127 RID: 4391
	private const float m_smoothnessRot = 0.5f;

	// Token: 0x04001128 RID: 4392
	private bool m_isKinematicBody;

	// Token: 0x04001129 RID: 4393
	private bool m_useGravity = true;

	// Token: 0x0400112A RID: 4394
	private Vector3 m_tempRelPos;

	// Token: 0x0400112B RID: 4395
	private bool m_haveTempRelPos;

	// Token: 0x0400112C RID: 4396
	private float m_targetPosTimer;

	// Token: 0x0400112D RID: 4397
	private uint m_posRevision = uint.MaxValue;

	// Token: 0x0400112E RID: 4398
	private int m_lastUpdateFrame = -1;

	// Token: 0x0400112F RID: 4399
	private bool m_wasOwner;

	// Token: 0x04001130 RID: 4400
	private ZNetView m_nview;

	// Token: 0x04001131 RID: 4401
	private Rigidbody m_body;

	// Token: 0x04001132 RID: 4402
	private Projectile m_projectile;

	// Token: 0x04001133 RID: 4403
	private Character m_character;

	// Token: 0x04001134 RID: 4404
	private ZDOID m_tempParent;

	// Token: 0x04001135 RID: 4405
	private string m_tempAttachJoint;

	// Token: 0x04001136 RID: 4406
	private Vector3 m_tempRelativePos;

	// Token: 0x04001137 RID: 4407
	private Quaternion m_tempRelativeRot;

	// Token: 0x04001138 RID: 4408
	private Vector3 m_tempRelativeVel;
}
