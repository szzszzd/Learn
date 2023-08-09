using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200028B RID: 651
public class Ship : MonoBehaviour
{
	// Token: 0x060018D8 RID: 6360 RVA: 0x000A5700 File Offset: 0x000A3900
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		WearNTear component = base.GetComponent<WearNTear>();
		if (component)
		{
			WearNTear wearNTear = component;
			wearNTear.m_onDestroyed = (Action)Delegate.Combine(wearNTear.m_onDestroyed, new Action(this.OnDestroyed));
		}
		if (this.m_nview.GetZDO() == null)
		{
			base.enabled = false;
		}
		this.m_body.maxDepenetrationVelocity = 2f;
		Heightmap.ForceGenerateAll();
		this.m_sailCloth = this.m_sailObject.GetComponentInChildren<Cloth>();
	}

	// Token: 0x060018D9 RID: 6361 RVA: 0x000A5790 File Offset: 0x000A3990
	private void OnEnable()
	{
		Ship.Instances.Add(this);
	}

	// Token: 0x060018DA RID: 6362 RVA: 0x000A579D File Offset: 0x000A399D
	private void OnDisable()
	{
		Ship.Instances.Remove(this);
	}

	// Token: 0x060018DB RID: 6363 RVA: 0x000A57AB File Offset: 0x000A39AB
	public bool CanBeRemoved()
	{
		return this.m_players.Count == 0;
	}

	// Token: 0x060018DC RID: 6364 RVA: 0x000A57BC File Offset: 0x000A39BC
	private void Start()
	{
		this.m_nview.Register("Stop", new Action<long>(this.RPC_Stop));
		this.m_nview.Register("Forward", new Action<long>(this.RPC_Forward));
		this.m_nview.Register("Backward", new Action<long>(this.RPC_Backward));
		this.m_nview.Register<float>("Rudder", new Action<long, float>(this.RPC_Rudder));
		base.InvokeRepeating("UpdateOwner", 2f, 2f);
	}

	// Token: 0x060018DD RID: 6365 RVA: 0x000A5850 File Offset: 0x000A3A50
	private void PrintStats()
	{
		if (this.m_players.Count == 0)
		{
			return;
		}
		ZLog.Log("Vel:" + this.m_body.velocity.magnitude.ToString("0.0"));
	}

	// Token: 0x060018DE RID: 6366 RVA: 0x000A589C File Offset: 0x000A3A9C
	public void ApplyControlls(Vector3 dir)
	{
		bool flag = (double)dir.z > 0.5;
		bool flag2 = (double)dir.z < -0.5;
		if (flag && !this.m_forwardPressed)
		{
			this.Forward();
		}
		if (flag2 && !this.m_backwardPressed)
		{
			this.Backward();
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		float num = Mathf.Lerp(0.5f, 1f, Mathf.Abs(this.m_rudderValue));
		this.m_rudder = dir.x * num;
		this.m_rudderValue += this.m_rudder * this.m_rudderSpeed * fixedDeltaTime;
		this.m_rudderValue = Mathf.Clamp(this.m_rudderValue, -1f, 1f);
		if (Time.time - this.m_sendRudderTime > 0.2f)
		{
			this.m_sendRudderTime = Time.time;
			this.m_nview.InvokeRPC("Rudder", new object[]
			{
				this.m_rudderValue
			});
		}
		this.m_forwardPressed = flag;
		this.m_backwardPressed = flag2;
	}

	// Token: 0x060018DF RID: 6367 RVA: 0x000A59A7 File Offset: 0x000A3BA7
	public void Forward()
	{
		this.m_nview.InvokeRPC("Forward", Array.Empty<object>());
	}

	// Token: 0x060018E0 RID: 6368 RVA: 0x000A59BE File Offset: 0x000A3BBE
	public void Backward()
	{
		this.m_nview.InvokeRPC("Backward", Array.Empty<object>());
	}

	// Token: 0x060018E1 RID: 6369 RVA: 0x000A59D5 File Offset: 0x000A3BD5
	public void Rudder(float rudder)
	{
		this.m_nview.Invoke("Rudder", rudder);
	}

	// Token: 0x060018E2 RID: 6370 RVA: 0x000A59E8 File Offset: 0x000A3BE8
	private void RPC_Rudder(long sender, float value)
	{
		this.m_rudderValue = value;
	}

	// Token: 0x060018E3 RID: 6371 RVA: 0x000A59F1 File Offset: 0x000A3BF1
	public void Stop()
	{
		this.m_nview.InvokeRPC("Stop", Array.Empty<object>());
	}

	// Token: 0x060018E4 RID: 6372 RVA: 0x000A5A08 File Offset: 0x000A3C08
	private void RPC_Stop(long sender)
	{
		this.m_speed = Ship.Speed.Stop;
	}

	// Token: 0x060018E5 RID: 6373 RVA: 0x000A5A14 File Offset: 0x000A3C14
	private void RPC_Forward(long sender)
	{
		switch (this.m_speed)
		{
		case Ship.Speed.Stop:
			this.m_speed = Ship.Speed.Slow;
			return;
		case Ship.Speed.Back:
			this.m_speed = Ship.Speed.Stop;
			break;
		case Ship.Speed.Slow:
			this.m_speed = Ship.Speed.Half;
			return;
		case Ship.Speed.Half:
			this.m_speed = Ship.Speed.Full;
			return;
		case Ship.Speed.Full:
			break;
		default:
			return;
		}
	}

	// Token: 0x060018E6 RID: 6374 RVA: 0x000A5A64 File Offset: 0x000A3C64
	private void RPC_Backward(long sender)
	{
		switch (this.m_speed)
		{
		case Ship.Speed.Stop:
			this.m_speed = Ship.Speed.Back;
			return;
		case Ship.Speed.Back:
			break;
		case Ship.Speed.Slow:
			this.m_speed = Ship.Speed.Stop;
			return;
		case Ship.Speed.Half:
			this.m_speed = Ship.Speed.Slow;
			return;
		case Ship.Speed.Full:
			this.m_speed = Ship.Speed.Half;
			break;
		default:
			return;
		}
	}

	// Token: 0x060018E7 RID: 6375 RVA: 0x000A5AB4 File Offset: 0x000A3CB4
	public void CustomFixedUpdate()
	{
		bool flag = this.HaveControllingPlayer();
		this.UpdateControlls(Time.fixedDeltaTime);
		this.UpdateSail(Time.fixedDeltaTime);
		this.UpdateRudder(Time.fixedDeltaTime, flag);
		if (this.m_nview && !this.m_nview.IsOwner())
		{
			return;
		}
		this.UpdateUpsideDmg(Time.fixedDeltaTime);
		if (this.m_players.Count == 0)
		{
			this.m_speed = Ship.Speed.Stop;
			this.m_rudderValue = 0f;
		}
		if (!flag && (this.m_speed == Ship.Speed.Slow || this.m_speed == Ship.Speed.Back))
		{
			this.m_speed = Ship.Speed.Stop;
		}
		Vector3 worldCenterOfMass = this.m_body.worldCenterOfMass;
		Vector3 vector = this.m_floatCollider.transform.position + this.m_floatCollider.transform.forward * this.m_floatCollider.size.z / 2f;
		Vector3 vector2 = this.m_floatCollider.transform.position - this.m_floatCollider.transform.forward * this.m_floatCollider.size.z / 2f;
		Vector3 vector3 = this.m_floatCollider.transform.position - this.m_floatCollider.transform.right * this.m_floatCollider.size.x / 2f;
		Vector3 vector4 = this.m_floatCollider.transform.position + this.m_floatCollider.transform.right * this.m_floatCollider.size.x / 2f;
		float waterLevel = Floating.GetWaterLevel(worldCenterOfMass, ref this.m_previousCenter);
		float waterLevel2 = Floating.GetWaterLevel(vector3, ref this.m_previousLeft);
		float waterLevel3 = Floating.GetWaterLevel(vector4, ref this.m_previousRight);
		float waterLevel4 = Floating.GetWaterLevel(vector, ref this.m_previousForward);
		float waterLevel5 = Floating.GetWaterLevel(vector2, ref this.m_previousBack);
		float num = (waterLevel + waterLevel2 + waterLevel3 + waterLevel4 + waterLevel5) / 5f;
		float num2 = worldCenterOfMass.y - num - this.m_waterLevelOffset;
		if (num2 > this.m_disableLevel)
		{
			return;
		}
		this.m_body.WakeUp();
		this.UpdateWaterForce(num2, Time.fixedDeltaTime);
		ref Vector3 ptr = new Vector3(vector3.x, waterLevel2, vector3.z);
		Vector3 vector5 = new Vector3(vector4.x, waterLevel3, vector4.z);
		ref Vector3 ptr2 = new Vector3(vector.x, waterLevel4, vector.z);
		Vector3 vector6 = new Vector3(vector2.x, waterLevel5, vector2.z);
		float fixedDeltaTime = Time.fixedDeltaTime;
		float d = fixedDeltaTime * 50f;
		float num3 = Mathf.Clamp01(Mathf.Abs(num2) / this.m_forceDistance);
		Vector3 a = Vector3.up * this.m_force * num3;
		this.m_body.AddForceAtPosition(a * d, worldCenterOfMass, ForceMode.VelocityChange);
		float num4 = Vector3.Dot(this.m_body.velocity, base.transform.forward);
		float num5 = Vector3.Dot(this.m_body.velocity, base.transform.right);
		Vector3 vector7 = this.m_body.velocity;
		float value = vector7.y * vector7.y * Mathf.Sign(vector7.y) * this.m_damping * num3;
		float value2 = num4 * num4 * Mathf.Sign(num4) * this.m_dampingForward * num3;
		float value3 = num5 * num5 * Mathf.Sign(num5) * this.m_dampingSideway * num3;
		vector7.y -= Mathf.Clamp(value, -1f, 1f);
		vector7 -= base.transform.forward * Mathf.Clamp(value2, -1f, 1f);
		vector7 -= base.transform.right * Mathf.Clamp(value3, -1f, 1f);
		if (vector7.magnitude > this.m_body.velocity.magnitude)
		{
			vector7 = vector7.normalized * this.m_body.velocity.magnitude;
		}
		if (this.m_players.Count == 0)
		{
			vector7.x *= 0.1f;
			vector7.z *= 0.1f;
		}
		this.m_body.velocity = vector7;
		this.m_body.angularVelocity = this.m_body.angularVelocity - this.m_body.angularVelocity * this.m_angularDamping * num3;
		float num6 = 0.15f;
		float num7 = 0.5f;
		float num8 = Mathf.Clamp((ptr2.y - vector.y) * num6, -num7, num7);
		float num9 = Mathf.Clamp((vector6.y - vector2.y) * num6, -num7, num7);
		float num10 = Mathf.Clamp((ptr.y - vector3.y) * num6, -num7, num7);
		float num11 = Mathf.Clamp((vector5.y - vector4.y) * num6, -num7, num7);
		num8 = Mathf.Sign(num8) * Mathf.Abs(Mathf.Pow(num8, 2f));
		num9 = Mathf.Sign(num9) * Mathf.Abs(Mathf.Pow(num9, 2f));
		num10 = Mathf.Sign(num10) * Mathf.Abs(Mathf.Pow(num10, 2f));
		num11 = Mathf.Sign(num11) * Mathf.Abs(Mathf.Pow(num11, 2f));
		this.m_body.AddForceAtPosition(Vector3.up * num8 * d, vector, ForceMode.VelocityChange);
		this.m_body.AddForceAtPosition(Vector3.up * num9 * d, vector2, ForceMode.VelocityChange);
		this.m_body.AddForceAtPosition(Vector3.up * num10 * d, vector3, ForceMode.VelocityChange);
		this.m_body.AddForceAtPosition(Vector3.up * num11 * d, vector4, ForceMode.VelocityChange);
		float sailSize = 0f;
		if (this.m_speed == Ship.Speed.Full)
		{
			sailSize = 1f;
		}
		else if (this.m_speed == Ship.Speed.Half)
		{
			sailSize = 0.5f;
		}
		Vector3 sailForce = this.GetSailForce(sailSize, fixedDeltaTime);
		Vector3 position = worldCenterOfMass + base.transform.up * this.m_sailForceOffset;
		this.m_body.AddForceAtPosition(sailForce, position, ForceMode.VelocityChange);
		Vector3 position2 = base.transform.position + base.transform.forward * this.m_stearForceOffset;
		float d2 = num4 * this.m_stearVelForceFactor;
		this.m_body.AddForceAtPosition(base.transform.right * d2 * -this.m_rudderValue * fixedDeltaTime, position2, ForceMode.VelocityChange);
		Vector3 a2 = Vector3.zero;
		Ship.Speed speed = this.m_speed;
		if (speed != Ship.Speed.Back)
		{
			if (speed == Ship.Speed.Slow)
			{
				a2 += base.transform.forward * this.m_backwardForce * (1f - Mathf.Abs(this.m_rudderValue));
			}
		}
		else
		{
			a2 += -base.transform.forward * this.m_backwardForce * (1f - Mathf.Abs(this.m_rudderValue));
		}
		if (this.m_speed == Ship.Speed.Back || this.m_speed == Ship.Speed.Slow)
		{
			float d3 = (float)((this.m_speed == Ship.Speed.Back) ? -1 : 1);
			a2 += base.transform.right * this.m_stearForce * -this.m_rudderValue * d3;
		}
		this.m_body.AddForceAtPosition(a2 * fixedDeltaTime, position2, ForceMode.VelocityChange);
		this.ApplyEdgeForce(Time.fixedDeltaTime);
	}

	// Token: 0x060018E8 RID: 6376 RVA: 0x000A62A0 File Offset: 0x000A44A0
	private void UpdateUpsideDmg(float dt)
	{
		if (base.transform.up.y >= 0f)
		{
			return;
		}
		this.m_upsideDownDmgTimer += dt;
		if (this.m_upsideDownDmgTimer <= this.m_upsideDownDmgInterval)
		{
			return;
		}
		this.m_upsideDownDmgTimer = 0f;
		IDestructible component = base.GetComponent<IDestructible>();
		if (component == null)
		{
			return;
		}
		HitData hitData = new HitData();
		hitData.m_damage.m_blunt = this.m_upsideDownDmg;
		hitData.m_point = base.transform.position;
		hitData.m_dir = Vector3.up;
		component.Damage(hitData);
	}

	// Token: 0x060018E9 RID: 6377 RVA: 0x000A6334 File Offset: 0x000A4534
	private Vector3 GetSailForce(float sailSize, float dt)
	{
		Vector3 windDir = EnvMan.instance.GetWindDir();
		float windIntensity = EnvMan.instance.GetWindIntensity();
		float num = Mathf.Lerp(0.25f, 1f, windIntensity);
		float num2 = this.GetWindAngleFactor();
		num2 *= num;
		Vector3 target = Vector3.Normalize(windDir + base.transform.forward) * num2 * this.m_sailForceFactor * sailSize;
		this.m_sailForce = Vector3.SmoothDamp(this.m_sailForce, target, ref this.m_windChangeVelocity, 1f, 99f);
		return this.m_sailForce;
	}

	// Token: 0x060018EA RID: 6378 RVA: 0x000A63C8 File Offset: 0x000A45C8
	public float GetWindAngleFactor()
	{
		float num = Vector3.Dot(EnvMan.instance.GetWindDir(), -base.transform.forward);
		float num2 = Mathf.Lerp(0.7f, 1f, 1f - Mathf.Abs(num));
		float num3 = 1f - Utils.LerpStep(0.75f, 0.8f, num);
		return num2 * num3;
	}

	// Token: 0x060018EB RID: 6379 RVA: 0x000A642C File Offset: 0x000A462C
	private void UpdateWaterForce(float depth, float dt)
	{
		if (this.m_lastDepth == -9999f)
		{
			this.m_lastDepth = depth;
			return;
		}
		float num = depth - this.m_lastDepth;
		this.m_lastDepth = depth;
		float num2 = num / dt;
		if (num2 > 0f)
		{
			return;
		}
		if (Mathf.Abs(num2) > this.m_minWaterImpactForce && Time.time - this.m_lastWaterImpactTime > this.m_minWaterImpactInterval)
		{
			this.m_lastWaterImpactTime = Time.time;
			this.m_waterImpactEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			if (this.m_players.Count > 0)
			{
				IDestructible component = base.GetComponent<IDestructible>();
				if (component != null)
				{
					HitData hitData = new HitData();
					hitData.m_damage.m_blunt = this.m_waterImpactDamage;
					hitData.m_point = base.transform.position;
					hitData.m_dir = Vector3.up;
					component.Damage(hitData);
				}
			}
		}
	}

	// Token: 0x060018EC RID: 6380 RVA: 0x000A6518 File Offset: 0x000A4718
	private void ApplyEdgeForce(float dt)
	{
		float magnitude = base.transform.position.magnitude;
		float num = 10420f;
		if (magnitude > num)
		{
			Vector3 a = Vector3.Normalize(base.transform.position);
			float d = Utils.LerpStep(num, 10500f, magnitude) * 8f;
			Vector3 a2 = a * d;
			this.m_body.AddForce(a2 * dt, ForceMode.VelocityChange);
		}
	}

	// Token: 0x060018ED RID: 6381 RVA: 0x000A6584 File Offset: 0x000A4784
	private void UpdateControlls(float dt)
	{
		if (this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_forward, (int)this.m_speed, false);
			this.m_nview.GetZDO().Set(ZDOVars.s_rudder, this.m_rudderValue);
			return;
		}
		this.m_speed = (Ship.Speed)this.m_nview.GetZDO().GetInt(ZDOVars.s_forward, 0);
		if (Time.time - this.m_sendRudderTime > 1f)
		{
			this.m_rudderValue = this.m_nview.GetZDO().GetFloat(ZDOVars.s_rudder, 0f);
		}
	}

	// Token: 0x060018EE RID: 6382 RVA: 0x000A6625 File Offset: 0x000A4825
	public bool IsSailUp()
	{
		return this.m_speed == Ship.Speed.Half || this.m_speed == Ship.Speed.Full;
	}

	// Token: 0x060018EF RID: 6383 RVA: 0x000A663C File Offset: 0x000A483C
	private void UpdateSail(float dt)
	{
		this.UpdateSailSize(dt);
		Vector3 vector = EnvMan.instance.GetWindDir();
		vector = Vector3.Cross(Vector3.Cross(vector, base.transform.up), base.transform.up);
		if (this.m_speed == Ship.Speed.Full || this.m_speed == Ship.Speed.Half)
		{
			float t = 0.5f + Vector3.Dot(base.transform.forward, vector) * 0.5f;
			Quaternion to = Quaternion.LookRotation(-Vector3.Lerp(vector, Vector3.Normalize(vector - base.transform.forward), t), base.transform.up);
			this.m_mastObject.transform.rotation = Quaternion.RotateTowards(this.m_mastObject.transform.rotation, to, 30f * dt);
			return;
		}
		if (this.m_speed == Ship.Speed.Back)
		{
			Quaternion from = Quaternion.LookRotation(-base.transform.forward, base.transform.up);
			Quaternion to2 = Quaternion.LookRotation(-vector, base.transform.up);
			to2 = Quaternion.RotateTowards(from, to2, 80f);
			this.m_mastObject.transform.rotation = Quaternion.RotateTowards(this.m_mastObject.transform.rotation, to2, 30f * dt);
		}
	}

	// Token: 0x060018F0 RID: 6384 RVA: 0x000A6788 File Offset: 0x000A4988
	private void UpdateRudder(float dt, bool haveControllingPlayer)
	{
		if (!this.m_rudderObject)
		{
			return;
		}
		Quaternion quaternion = Quaternion.Euler(0f, this.m_rudderRotationMax * -this.m_rudderValue, 0f);
		if (haveControllingPlayer)
		{
			if (this.m_speed == Ship.Speed.Slow)
			{
				this.m_rudderPaddleTimer += dt;
				quaternion *= Quaternion.Euler(0f, Mathf.Sin(this.m_rudderPaddleTimer * 6f) * 20f, 0f);
			}
			else if (this.m_speed == Ship.Speed.Back)
			{
				this.m_rudderPaddleTimer += dt;
				quaternion *= Quaternion.Euler(0f, Mathf.Sin(this.m_rudderPaddleTimer * -3f) * 40f, 0f);
			}
		}
		this.m_rudderObject.transform.localRotation = Quaternion.Slerp(this.m_rudderObject.transform.localRotation, quaternion, 0.5f);
	}

	// Token: 0x060018F1 RID: 6385 RVA: 0x000A687C File Offset: 0x000A4A7C
	private void UpdateSailSize(float dt)
	{
		float num = 0f;
		switch (this.m_speed)
		{
		case Ship.Speed.Stop:
			num = 0.1f;
			break;
		case Ship.Speed.Back:
			num = 0.1f;
			break;
		case Ship.Speed.Slow:
			num = 0.1f;
			break;
		case Ship.Speed.Half:
			num = 0.5f;
			break;
		case Ship.Speed.Full:
			num = 1f;
			break;
		}
		Vector3 localScale = this.m_sailObject.transform.localScale;
		bool flag = Mathf.Abs(localScale.y - num) < 0.01f;
		if (!flag)
		{
			localScale.y = Mathf.MoveTowards(localScale.y, num, dt);
			this.m_sailObject.transform.localScale = localScale;
		}
		if (this.m_sailCloth)
		{
			if (this.m_speed == Ship.Speed.Stop || this.m_speed == Ship.Speed.Slow || this.m_speed == Ship.Speed.Back)
			{
				if (flag && this.m_sailCloth.enabled)
				{
					this.m_sailCloth.enabled = false;
				}
			}
			else if (flag)
			{
				if (!this.m_sailWasInPosition)
				{
					this.m_sailCloth.enabled = false;
					this.m_sailCloth.enabled = true;
				}
			}
			else
			{
				this.m_sailCloth.enabled = true;
			}
		}
		this.m_sailWasInPosition = flag;
	}

	// Token: 0x060018F2 RID: 6386 RVA: 0x000A69A4 File Offset: 0x000A4BA4
	private void UpdateOwner()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (Player.m_localPlayer == null)
		{
			return;
		}
		if (this.m_players.Count > 0 && !this.IsPlayerInBoat(Player.m_localPlayer))
		{
			long owner = this.m_players[0].GetOwner();
			this.m_nview.GetZDO().SetOwner(owner);
			ZLog.Log("Changing ship owner to " + owner.ToString());
		}
	}

	// Token: 0x060018F3 RID: 6387 RVA: 0x000A6A30 File Offset: 0x000A4C30
	private void OnTriggerEnter(Collider collider)
	{
		Player component = collider.GetComponent<Player>();
		if (component)
		{
			this.m_players.Add(component);
			ZLog.Log("Player onboard, total onboard " + this.m_players.Count.ToString());
			if (component == Player.m_localPlayer)
			{
				Ship.s_currentShips.Add(this);
			}
		}
		Character component2 = collider.GetComponent<Character>();
		if (component2)
		{
			Character character = component2;
			int inNumShipVolumes = character.InNumShipVolumes;
			character.InNumShipVolumes = inNumShipVolumes + 1;
		}
	}

	// Token: 0x060018F4 RID: 6388 RVA: 0x000A6AB4 File Offset: 0x000A4CB4
	private void OnTriggerExit(Collider collider)
	{
		Player component = collider.GetComponent<Player>();
		if (component)
		{
			this.m_players.Remove(component);
			ZLog.Log("Player over board, players left " + this.m_players.Count.ToString());
			if (component == Player.m_localPlayer)
			{
				Ship.s_currentShips.Remove(this);
			}
		}
		Character component2 = collider.GetComponent<Character>();
		if (component2)
		{
			Character character = component2;
			int inNumShipVolumes = character.InNumShipVolumes;
			character.InNumShipVolumes = inNumShipVolumes - 1;
		}
	}

	// Token: 0x060018F5 RID: 6389 RVA: 0x000A6B38 File Offset: 0x000A4D38
	public bool IsPlayerInBoat(long playerID)
	{
		using (List<Player>.Enumerator enumerator = this.m_players.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.GetPlayerID() == playerID)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060018F6 RID: 6390 RVA: 0x000A6B94 File Offset: 0x000A4D94
	public bool IsPlayerInBoat(Player player)
	{
		return this.m_players.Contains(player);
	}

	// Token: 0x060018F7 RID: 6391 RVA: 0x000A6BA2 File Offset: 0x000A4DA2
	public bool HasPlayerOnboard()
	{
		return this.m_players.Count > 0;
	}

	// Token: 0x060018F8 RID: 6392 RVA: 0x000A6BB4 File Offset: 0x000A4DB4
	private void OnDestroyed()
	{
		if (this.m_nview.IsValid() && this.m_nview.IsOwner())
		{
			Gogan.LogEvent("Game", "ShipDestroyed", base.gameObject.name, 0L);
		}
		Ship.s_currentShips.Remove(this);
	}

	// Token: 0x060018F9 RID: 6393 RVA: 0x000A6C04 File Offset: 0x000A4E04
	public bool IsWindControllActive()
	{
		using (List<Player>.Enumerator enumerator = this.m_players.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.GetSEMan().HaveStatusAttribute(StatusEffect.StatusAttribute.SailingPower))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060018FA RID: 6394 RVA: 0x000A6C64 File Offset: 0x000A4E64
	public static Ship GetLocalShip()
	{
		if (Ship.s_currentShips.Count != 0)
		{
			return Ship.s_currentShips[Ship.s_currentShips.Count - 1];
		}
		return null;
	}

	// Token: 0x060018FB RID: 6395 RVA: 0x000A6C8A File Offset: 0x000A4E8A
	private bool HaveControllingPlayer()
	{
		return this.m_players.Count != 0 && this.m_shipControlls.HaveValidUser();
	}

	// Token: 0x060018FC RID: 6396 RVA: 0x000A6CA6 File Offset: 0x000A4EA6
	public bool IsOwner()
	{
		return this.m_nview.IsValid() && this.m_nview.IsOwner();
	}

	// Token: 0x060018FD RID: 6397 RVA: 0x000A6CC2 File Offset: 0x000A4EC2
	public float GetSpeed()
	{
		return Vector3.Dot(this.m_body.velocity, base.transform.forward);
	}

	// Token: 0x060018FE RID: 6398 RVA: 0x000A6CDF File Offset: 0x000A4EDF
	public Ship.Speed GetSpeedSetting()
	{
		return this.m_speed;
	}

	// Token: 0x060018FF RID: 6399 RVA: 0x000A6CE7 File Offset: 0x000A4EE7
	public float GetRudder()
	{
		return this.m_rudder;
	}

	// Token: 0x06001900 RID: 6400 RVA: 0x000A6CEF File Offset: 0x000A4EEF
	public float GetRudderValue()
	{
		return this.m_rudderValue;
	}

	// Token: 0x06001901 RID: 6401 RVA: 0x000A6CF8 File Offset: 0x000A4EF8
	public float GetShipYawAngle()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return 0f;
		}
		return -Utils.YawFromDirection(mainCamera.transform.InverseTransformDirection(base.transform.forward));
	}

	// Token: 0x06001902 RID: 6402 RVA: 0x000A6D38 File Offset: 0x000A4F38
	public float GetWindAngle()
	{
		Vector3 windDir = EnvMan.instance.GetWindDir();
		return -Utils.YawFromDirection(base.transform.InverseTransformDirection(windDir));
	}

	// Token: 0x06001903 RID: 6403 RVA: 0x000A6D64 File Offset: 0x000A4F64
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position + base.transform.forward * this.m_stearForceOffset, 0.25f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(base.transform.position + base.transform.up * this.m_sailForceOffset, 0.25f);
	}

	// Token: 0x170000F2 RID: 242
	// (get) Token: 0x06001904 RID: 6404 RVA: 0x000A6DE5 File Offset: 0x000A4FE5
	public static List<Ship> Instances { get; } = new List<Ship>();

	// Token: 0x04001ACD RID: 6861
	private bool m_forwardPressed;

	// Token: 0x04001ACE RID: 6862
	private bool m_backwardPressed;

	// Token: 0x04001ACF RID: 6863
	private float m_sendRudderTime;

	// Token: 0x04001AD0 RID: 6864
	[Header("Objects")]
	public GameObject m_sailObject;

	// Token: 0x04001AD1 RID: 6865
	public GameObject m_mastObject;

	// Token: 0x04001AD2 RID: 6866
	public GameObject m_rudderObject;

	// Token: 0x04001AD3 RID: 6867
	public ShipControlls m_shipControlls;

	// Token: 0x04001AD4 RID: 6868
	public Transform m_controlGuiPos;

	// Token: 0x04001AD5 RID: 6869
	[Header("Misc")]
	public BoxCollider m_floatCollider;

	// Token: 0x04001AD6 RID: 6870
	public float m_waterLevelOffset;

	// Token: 0x04001AD7 RID: 6871
	public float m_forceDistance = 1f;

	// Token: 0x04001AD8 RID: 6872
	public float m_force = 0.5f;

	// Token: 0x04001AD9 RID: 6873
	public float m_damping = 0.05f;

	// Token: 0x04001ADA RID: 6874
	public float m_dampingSideway = 0.05f;

	// Token: 0x04001ADB RID: 6875
	public float m_dampingForward = 0.01f;

	// Token: 0x04001ADC RID: 6876
	public float m_angularDamping = 0.01f;

	// Token: 0x04001ADD RID: 6877
	public float m_disableLevel = -0.5f;

	// Token: 0x04001ADE RID: 6878
	public float m_sailForceOffset;

	// Token: 0x04001ADF RID: 6879
	public float m_sailForceFactor = 0.1f;

	// Token: 0x04001AE0 RID: 6880
	public float m_rudderSpeed = 0.5f;

	// Token: 0x04001AE1 RID: 6881
	public float m_stearForceOffset = -10f;

	// Token: 0x04001AE2 RID: 6882
	public float m_stearForce = 0.5f;

	// Token: 0x04001AE3 RID: 6883
	public float m_stearVelForceFactor = 0.1f;

	// Token: 0x04001AE4 RID: 6884
	public float m_backwardForce = 50f;

	// Token: 0x04001AE5 RID: 6885
	public float m_rudderRotationMax = 30f;

	// Token: 0x04001AE6 RID: 6886
	public float m_minWaterImpactForce = 2.5f;

	// Token: 0x04001AE7 RID: 6887
	public float m_minWaterImpactInterval = 2f;

	// Token: 0x04001AE8 RID: 6888
	public float m_waterImpactDamage = 10f;

	// Token: 0x04001AE9 RID: 6889
	public float m_upsideDownDmgInterval = 1f;

	// Token: 0x04001AEA RID: 6890
	public float m_upsideDownDmg = 20f;

	// Token: 0x04001AEB RID: 6891
	public EffectList m_waterImpactEffect = new EffectList();

	// Token: 0x04001AEC RID: 6892
	private bool m_sailWasInPosition;

	// Token: 0x04001AED RID: 6893
	private Vector3 m_windChangeVelocity = Vector3.zero;

	// Token: 0x04001AEE RID: 6894
	private Ship.Speed m_speed;

	// Token: 0x04001AEF RID: 6895
	private float m_rudder;

	// Token: 0x04001AF0 RID: 6896
	private float m_rudderValue;

	// Token: 0x04001AF1 RID: 6897
	private Vector3 m_sailForce = Vector3.zero;

	// Token: 0x04001AF2 RID: 6898
	private readonly List<Player> m_players = new List<Player>();

	// Token: 0x04001AF3 RID: 6899
	private WaterVolume m_previousCenter;

	// Token: 0x04001AF4 RID: 6900
	private WaterVolume m_previousLeft;

	// Token: 0x04001AF5 RID: 6901
	private WaterVolume m_previousRight;

	// Token: 0x04001AF6 RID: 6902
	private WaterVolume m_previousForward;

	// Token: 0x04001AF7 RID: 6903
	private WaterVolume m_previousBack;

	// Token: 0x04001AF8 RID: 6904
	private static readonly List<Ship> s_currentShips = new List<Ship>();

	// Token: 0x04001AF9 RID: 6905
	private Rigidbody m_body;

	// Token: 0x04001AFA RID: 6906
	private ZNetView m_nview;

	// Token: 0x04001AFB RID: 6907
	private Cloth m_sailCloth;

	// Token: 0x04001AFC RID: 6908
	private float m_lastDepth = -9999f;

	// Token: 0x04001AFD RID: 6909
	private float m_lastWaterImpactTime;

	// Token: 0x04001AFE RID: 6910
	private float m_upsideDownDmgTimer;

	// Token: 0x04001AFF RID: 6911
	private float m_rudderPaddleTimer;

	// Token: 0x0200028C RID: 652
	public enum Speed
	{
		// Token: 0x04001B02 RID: 6914
		Stop,
		// Token: 0x04001B03 RID: 6915
		Back,
		// Token: 0x04001B04 RID: 6916
		Slow,
		// Token: 0x04001B05 RID: 6917
		Half,
		// Token: 0x04001B06 RID: 6918
		Full
	}
}
