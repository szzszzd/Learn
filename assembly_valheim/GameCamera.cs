using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

// Token: 0x020001CC RID: 460
public class GameCamera : MonoBehaviour
{
	// Token: 0x170000C4 RID: 196
	// (get) Token: 0x060012D2 RID: 4818 RVA: 0x0007BAEA File Offset: 0x00079CEA
	public static GameCamera instance
	{
		get
		{
			return GameCamera.m_instance;
		}
	}

	// Token: 0x060012D3 RID: 4819 RVA: 0x0007BAF1 File Offset: 0x00079CF1
	private void Awake()
	{
		GameCamera.m_instance = this;
		this.m_camera = base.GetComponent<Camera>();
		this.m_listner = base.GetComponentInChildren<AudioListener>();
		this.m_camera.depthTextureMode = DepthTextureMode.DepthNormals;
		this.ApplySettings();
		if (!Application.isEditor)
		{
			this.m_mouseCapture = true;
		}
	}

	// Token: 0x060012D4 RID: 4820 RVA: 0x0007BB31 File Offset: 0x00079D31
	private void OnDestroy()
	{
		if (GameCamera.m_instance == this)
		{
			GameCamera.m_instance = null;
		}
	}

	// Token: 0x060012D5 RID: 4821 RVA: 0x0007BB46 File Offset: 0x00079D46
	public void ApplySettings()
	{
		this.m_cameraShakeEnabled = (PlayerPrefs.GetInt("CameraShake", 1) == 1);
		this.m_shipCameraTilt = (PlayerPrefs.GetInt("ShipCameraTilt", 1) == 1);
	}

	// Token: 0x060012D6 RID: 4822 RVA: 0x0007BB78 File Offset: 0x00079D78
	private void LateUpdate()
	{
		float deltaTime = Time.deltaTime;
		if (Input.GetKeyDown(KeyCode.F11) || (this.m_freeFly && Input.GetKeyDown(KeyCode.Mouse1)))
		{
			GameCamera.ScreenShot();
		}
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			this.UpdateBaseOffset(localPlayer, deltaTime);
		}
		this.UpdateMouseCapture();
		this.UpdateCamera(Time.unscaledDeltaTime);
		this.UpdateListner();
	}

	// Token: 0x060012D7 RID: 4823 RVA: 0x0007BBE0 File Offset: 0x00079DE0
	private void UpdateMouseCapture()
	{
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F1))
		{
			this.m_mouseCapture = !this.m_mouseCapture;
		}
		if (this.m_mouseCapture && !InventoryGui.IsVisible() && !TextInput.IsVisible() && !Menu.IsVisible() && !Minimap.IsOpen() && !StoreGui.IsVisible() && !Hud.IsPieceSelectionVisible())
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			return;
		}
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = ZInput.IsMouseActive();
	}

	// Token: 0x060012D8 RID: 4824 RVA: 0x0007BC64 File Offset: 0x00079E64
	public static void ScreenShot()
	{
		DateTime now = DateTime.Now;
		Directory.CreateDirectory(Utils.GetSaveDataPath(FileHelpers.FileSource.Local) + "/screenshots");
		string text = now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
		string text2 = now.ToString("yyyy-MM-dd");
		string text3 = string.Concat(new string[]
		{
			Utils.GetSaveDataPath(FileHelpers.FileSource.Local),
			"/screenshots/screenshot_",
			text2,
			"_",
			text,
			".png"
		});
		if (File.Exists(text3))
		{
			return;
		}
		ScreenCapture.CaptureScreenshot(text3);
		ZLog.Log("Screenshot saved:" + text3);
	}

	// Token: 0x060012D9 RID: 4825 RVA: 0x0007BD34 File Offset: 0x00079F34
	private void UpdateListner()
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer && !this.m_freeFly)
		{
			this.m_listner.transform.position = localPlayer.m_eye.position;
			return;
		}
		this.m_listner.transform.localPosition = Vector3.zero;
	}

	// Token: 0x060012DA RID: 4826 RVA: 0x0007BD88 File Offset: 0x00079F88
	private void UpdateCamera(float dt)
	{
		if (this.m_freeFly)
		{
			this.UpdateFreeFly(dt);
			this.UpdateCameraShake(dt);
			this.<UpdateCamera>g__debugCamera|9_0(Input.GetAxis("Mouse ScrollWheel"));
			return;
		}
		this.m_camera.fieldOfView = this.m_fov;
		this.m_skyCamera.fieldOfView = this.m_fov;
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			if ((!Chat.instance || !Chat.instance.HasFocus()) && !global::Console.IsVisible() && !InventoryGui.IsVisible() && !StoreGui.IsVisible() && !Menu.IsVisible() && !Minimap.IsOpen() && !localPlayer.InCutscene() && !localPlayer.InPlaceMode())
			{
				float minDistance = this.m_minDistance;
				float num = Input.GetAxis("Mouse ScrollWheel");
				if (Player.m_debugMode)
				{
					num = this.<UpdateCamera>g__debugCamera|9_0(num);
				}
				this.m_distance -= num * this.m_zoomSens;
				if (ZInput.GetButton("JoyAltKeys"))
				{
					if (ZInput.GetButton("JoyCamZoomIn"))
					{
						this.m_distance += -this.m_zoomSens * dt;
					}
					else if (ZInput.GetButton("JoyCamZoomOut"))
					{
						this.m_distance += this.m_zoomSens * dt;
					}
				}
				float max = (localPlayer.GetControlledShip() != null) ? this.m_maxDistanceBoat : this.m_maxDistance;
				this.m_distance = Mathf.Clamp(this.m_distance, minDistance, max);
			}
			if (localPlayer.IsDead() && localPlayer.GetRagdoll())
			{
				Vector3 averageBodyPosition = localPlayer.GetRagdoll().GetAverageBodyPosition();
				base.transform.LookAt(averageBodyPosition);
			}
			else
			{
				Vector3 position;
				Quaternion rotation;
				this.GetCameraPosition(dt, out position, out rotation);
				base.transform.position = position;
				base.transform.rotation = rotation;
			}
			this.UpdateCameraShake(dt);
		}
	}

	// Token: 0x060012DB RID: 4827 RVA: 0x0007BF58 File Offset: 0x0007A158
	private void GetCameraPosition(float dt, out Vector3 pos, out Quaternion rot)
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null)
		{
			pos = base.transform.position;
			rot = base.transform.rotation;
			return;
		}
		Vector3 vector = this.GetOffsetedEyePos();
		float num = this.m_distance;
		if (localPlayer.InIntro())
		{
			vector = localPlayer.transform.position;
			num = this.m_flyingDistance;
		}
		Vector3 vector2 = -localPlayer.m_eye.transform.forward;
		if (this.m_smoothYTilt && !localPlayer.InIntro())
		{
			num = Mathf.Lerp(num, 1.5f, Utils.SmoothStep(0f, -0.5f, vector2.y));
		}
		Vector3 vector3 = vector + vector2 * num;
		this.CollideRay2(localPlayer.m_eye.position, vector, ref vector3);
		this.UpdateNearClipping(vector, vector3, dt);
		float liquidLevel = Floating.GetLiquidLevel(vector3, 1f, LiquidType.All);
		if (vector3.y < liquidLevel + this.m_minWaterDistance)
		{
			vector3.y = liquidLevel + this.m_minWaterDistance;
			this.m_waterClipping = true;
		}
		else
		{
			this.m_waterClipping = false;
		}
		pos = vector3;
		rot = localPlayer.m_eye.transform.rotation;
		if (this.m_shipCameraTilt)
		{
			this.ApplyCameraTilt(localPlayer, dt, ref rot);
		}
	}

	// Token: 0x060012DC RID: 4828 RVA: 0x0007C0A8 File Offset: 0x0007A2A8
	private void ApplyCameraTilt(Player player, float dt, ref Quaternion rot)
	{
		if (player.InIntro())
		{
			return;
		}
		Ship standingOnShip = player.GetStandingOnShip();
		float num = Mathf.Clamp01((this.m_distance - this.m_minDistance) / (this.m_maxDistanceBoat - this.m_minDistance));
		num = Mathf.Pow(num, 2f);
		float smoothTime = Mathf.Lerp(this.m_tiltSmoothnessShipMin, this.m_tiltSmoothnessShipMax, num);
		Vector3 up = Vector3.up;
		if (standingOnShip != null && standingOnShip.transform.up.y > 0f)
		{
			up = standingOnShip.transform.up;
		}
		else if (player.IsAttached())
		{
			up = player.GetVisual().transform.up;
		}
		Vector3 forward = player.m_eye.transform.forward;
		Vector3 target = Vector3.Lerp(up, Vector3.up, num * 0.5f);
		this.m_smoothedCameraUp = Vector3.SmoothDamp(this.m_smoothedCameraUp, target, ref this.m_smoothedCameraUpVel, smoothTime, 99f, dt);
		rot = Quaternion.LookRotation(forward, this.m_smoothedCameraUp);
	}

	// Token: 0x060012DD RID: 4829 RVA: 0x0007C1AC File Offset: 0x0007A3AC
	private void UpdateNearClipping(Vector3 eyePos, Vector3 camPos, float dt)
	{
		float num = this.m_nearClipPlaneMax;
		Vector3 normalized = (camPos - eyePos).normalized;
		if (this.m_waterClipping || Physics.CheckSphere(camPos - normalized * this.m_nearClipPlaneMax, this.m_nearClipPlaneMax, this.m_blockCameraMask))
		{
			num = this.m_nearClipPlaneMin;
		}
		if (this.m_camera.nearClipPlane != num)
		{
			this.m_camera.nearClipPlane = num;
		}
	}

	// Token: 0x060012DE RID: 4830 RVA: 0x0007C224 File Offset: 0x0007A424
	private void CollideRay2(Vector3 eyePos, Vector3 offsetedEyePos, ref Vector3 end)
	{
		float num;
		if (this.RayTestPoint(eyePos, offsetedEyePos, (end - offsetedEyePos).normalized, Vector3.Distance(eyePos, end), out num))
		{
			float t = Utils.LerpStep(0.5f, 2f, num);
			Vector3 a = eyePos + (end - eyePos).normalized * num;
			Vector3 b = offsetedEyePos + (end - offsetedEyePos).normalized * num;
			end = Vector3.Lerp(a, b, t);
		}
	}

	// Token: 0x060012DF RID: 4831 RVA: 0x0007C2C0 File Offset: 0x0007A4C0
	private bool RayTestPoint(Vector3 point, Vector3 offsetedPoint, Vector3 dir, float maxDist, out float distance)
	{
		bool result = false;
		distance = maxDist;
		RaycastHit raycastHit;
		if (Physics.SphereCast(offsetedPoint, this.m_raycastWidth, dir, out raycastHit, maxDist, this.m_blockCameraMask))
		{
			distance = raycastHit.distance;
			result = true;
		}
		offsetedPoint + dir * distance;
		if (Physics.SphereCast(point, this.m_raycastWidth, dir, out raycastHit, maxDist, this.m_blockCameraMask))
		{
			if (raycastHit.distance < distance)
			{
				distance = raycastHit.distance;
			}
			result = true;
		}
		if (Physics.Raycast(point, dir, out raycastHit, maxDist, this.m_blockCameraMask))
		{
			float num = raycastHit.distance - this.m_nearClipPlaneMin;
			if (num < distance)
			{
				distance = num;
			}
			result = true;
		}
		return result;
	}

	// Token: 0x060012E0 RID: 4832 RVA: 0x0007C378 File Offset: 0x0007A578
	private bool RayTestPoint(Vector3 point, Vector3 dir, float maxDist, out Vector3 hitPoint)
	{
		RaycastHit raycastHit;
		if (Physics.SphereCast(point, 0.2f, dir, out raycastHit, maxDist, this.m_blockCameraMask))
		{
			hitPoint = point + dir * raycastHit.distance;
			return true;
		}
		if (Physics.Raycast(point, dir, out raycastHit, maxDist, this.m_blockCameraMask))
		{
			hitPoint = point + dir * (raycastHit.distance - 0.05f);
			return true;
		}
		hitPoint = Vector3.zero;
		return false;
	}

	// Token: 0x060012E1 RID: 4833 RVA: 0x0007C404 File Offset: 0x0007A604
	private void UpdateFreeFly(float dt)
	{
		if (global::Console.IsVisible())
		{
			return;
		}
		Vector2 zero = Vector2.zero;
		zero.x = Input.GetAxis("Mouse X");
		zero.y = Input.GetAxis("Mouse Y");
		zero.x += ZInput.GetJoyRightStickX() * 110f * dt;
		zero.y += -ZInput.GetJoyRightStickY() * 110f * dt;
		this.m_freeFlyYaw += zero.x;
		this.m_freeFlyPitch -= zero.y;
		if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			this.m_freeFlySpeed *= 0.8f;
		}
		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			this.m_freeFlySpeed *= 1.2f;
		}
		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			this.m_freeFlySpeed *= 1.2f;
		}
		if (ZInput.GetButton("JoyTabLeft"))
		{
			this.m_camera.fieldOfView = Mathf.Max(this.m_freeFlyMinFov, this.m_camera.fieldOfView - dt * 20f);
		}
		if (ZInput.GetButton("JoyTabRight"))
		{
			this.m_camera.fieldOfView = Mathf.Min(this.m_freeFlyMaxFov, this.m_camera.fieldOfView + dt * 20f);
		}
		this.m_skyCamera.fieldOfView = this.m_camera.fieldOfView;
		if (ZInput.GetButton("JoyButtonY"))
		{
			this.m_freeFlySpeed += this.m_freeFlySpeed * 0.1f * dt * 10f;
		}
		if (ZInput.GetButton("JoyButtonX"))
		{
			this.m_freeFlySpeed -= this.m_freeFlySpeed * 0.1f * dt * 10f;
		}
		this.m_freeFlySpeed = Mathf.Clamp(this.m_freeFlySpeed, 1f, 1000f);
		if (ZInput.GetButtonDown("JoyLStick") || ZInput.GetButtonDown("SecondaryAttack"))
		{
			if (this.m_freeFlyLockon)
			{
				this.m_freeFlyLockon = null;
			}
			else
			{
				int mask = LayerMask.GetMask(new string[]
				{
					"Default",
					"static_solid",
					"terrain",
					"vehicle",
					"character",
					"piece",
					"character_net",
					"viewblock"
				});
				RaycastHit raycastHit;
				if (Physics.Raycast(base.transform.position, base.transform.forward, out raycastHit, 10000f, mask))
				{
					this.m_freeFlyLockon = raycastHit.collider.transform;
					this.m_freeFlyLockonOffset = this.m_freeFlyLockon.InverseTransformPoint(base.transform.position);
				}
			}
		}
		Vector3 vector = Vector3.zero;
		if (ZInput.GetButton("Left"))
		{
			vector -= Vector3.right;
		}
		if (ZInput.GetButton("Right"))
		{
			vector += Vector3.right;
		}
		if (ZInput.GetButton("Forward"))
		{
			vector += Vector3.forward;
		}
		if (ZInput.GetButton("Backward"))
		{
			vector -= Vector3.forward;
		}
		if (ZInput.GetButton("Jump"))
		{
			vector += Vector3.up;
		}
		if (ZInput.GetButton("Crouch"))
		{
			vector -= Vector3.up;
		}
		vector += Vector3.up * ZInput.GetJoyRTrigger();
		vector -= Vector3.up * ZInput.GetJoyLTrigger();
		vector += Vector3.right * ZInput.GetJoyLeftStickX(false);
		vector += -Vector3.forward * ZInput.GetJoyLeftStickY(true);
		if (ZInput.GetButtonDown("JoyButtonB") || ZInput.GetButtonDown("Block"))
		{
			this.m_freeFlySavedVel = vector;
		}
		float magnitude = this.m_freeFlySavedVel.magnitude;
		if (magnitude > 0.001f)
		{
			vector += this.m_freeFlySavedVel;
			if (vector.magnitude > magnitude)
			{
				vector = vector.normalized * magnitude;
			}
		}
		if (vector.magnitude > 1f)
		{
			vector.Normalize();
		}
		vector = base.transform.TransformVector(vector);
		vector *= this.m_freeFlySpeed;
		if (this.m_freeFlySmooth <= 0f)
		{
			this.m_freeFlyVel = vector;
		}
		else
		{
			this.m_freeFlyVel = Vector3.SmoothDamp(this.m_freeFlyVel, vector, ref this.m_freeFlyAcc, this.m_freeFlySmooth, 99f, dt);
		}
		if (this.m_freeFlyLockon)
		{
			this.m_freeFlyLockonOffset += this.m_freeFlyLockon.InverseTransformVector(this.m_freeFlyVel * dt);
			base.transform.position = this.m_freeFlyLockon.TransformPoint(this.m_freeFlyLockonOffset);
		}
		else
		{
			base.transform.position = base.transform.position + this.m_freeFlyVel * dt;
		}
		Quaternion quaternion = Quaternion.Euler(0f, this.m_freeFlyYaw, 0f) * Quaternion.Euler(this.m_freeFlyPitch, 0f, 0f);
		if (this.m_freeFlyLockon)
		{
			quaternion = this.m_freeFlyLockon.rotation * quaternion;
		}
		if ((ZInput.GetButtonDown("JoyRStick") && !ZInput.GetButton("JoyAltKeys")) || ZInput.GetButtonDown("Attack"))
		{
			if (this.m_freeFlyTarget)
			{
				this.m_freeFlyTarget = null;
			}
			else
			{
				int mask2 = LayerMask.GetMask(new string[]
				{
					"Default",
					"static_solid",
					"terrain",
					"vehicle",
					"character",
					"piece",
					"character_net",
					"viewblock"
				});
				RaycastHit raycastHit2;
				if (Physics.Raycast(base.transform.position, base.transform.forward, out raycastHit2, 10000f, mask2))
				{
					this.m_freeFlyTarget = raycastHit2.collider.transform;
					this.m_freeFlyTargetOffset = this.m_freeFlyTarget.InverseTransformPoint(raycastHit2.point);
				}
			}
		}
		if (this.m_freeFlyTarget)
		{
			quaternion = Quaternion.LookRotation((this.m_freeFlyTarget.TransformPoint(this.m_freeFlyTargetOffset) - base.transform.position).normalized, Vector3.up);
		}
		if (this.m_freeFlySmooth <= 0f)
		{
			base.transform.rotation = quaternion;
			return;
		}
		Quaternion rotation = Utils.SmoothDamp(base.transform.rotation, quaternion, ref this.m_freeFlyRef, this.m_freeFlySmooth, 9999f, dt);
		base.transform.rotation = rotation;
	}

	// Token: 0x060012E2 RID: 4834 RVA: 0x0007CAB8 File Offset: 0x0007ACB8
	private void UpdateCameraShake(float dt)
	{
		this.m_shakeIntensity -= dt;
		if (this.m_shakeIntensity <= 0f)
		{
			this.m_shakeIntensity = 0f;
			return;
		}
		float num = this.m_shakeIntensity * this.m_shakeIntensity * this.m_shakeIntensity;
		this.m_shakeTimer += dt * Mathf.Clamp01(this.m_shakeIntensity) * this.m_shakeFreq;
		Quaternion rhs = Quaternion.Euler(Mathf.Sin(this.m_shakeTimer) * num * this.m_shakeMovement, Mathf.Cos(this.m_shakeTimer * 0.9f) * num * this.m_shakeMovement, 0f);
		base.transform.rotation = base.transform.rotation * rhs;
	}

	// Token: 0x060012E3 RID: 4835 RVA: 0x0007CB78 File Offset: 0x0007AD78
	public void AddShake(Vector3 point, float range, float strength, bool continous)
	{
		if (!this.m_cameraShakeEnabled)
		{
			return;
		}
		float num = Vector3.Distance(point, base.transform.position);
		if (num > range)
		{
			return;
		}
		num = Mathf.Max(1f, num);
		float num2 = 1f - num / range;
		float num3 = strength * num2;
		if (num3 < this.m_shakeIntensity)
		{
			return;
		}
		this.m_shakeIntensity = num3;
		if (continous)
		{
			this.m_shakeTimer = Time.time * Mathf.Clamp01(strength) * this.m_shakeFreq;
			return;
		}
		this.m_shakeTimer = Time.time * Mathf.Clamp01(this.m_shakeIntensity) * this.m_shakeFreq;
	}

	// Token: 0x060012E4 RID: 4836 RVA: 0x0007CC0C File Offset: 0x0007AE0C
	private float RayTest(Vector3 point, Vector3 dir, float maxDist)
	{
		RaycastHit raycastHit;
		if (Physics.SphereCast(point, 0.2f, dir, out raycastHit, maxDist, this.m_blockCameraMask))
		{
			return raycastHit.distance;
		}
		return maxDist;
	}

	// Token: 0x060012E5 RID: 4837 RVA: 0x0007CC40 File Offset: 0x0007AE40
	private Vector3 GetCameraBaseOffset(Player player)
	{
		if (player.InBed())
		{
			return player.GetHeadPoint() - player.transform.position;
		}
		if (player.IsAttached() || player.IsSitting())
		{
			return player.GetHeadPoint() + Vector3.up * 0.3f - player.transform.position;
		}
		return player.m_eye.transform.position - player.transform.position;
	}

	// Token: 0x060012E6 RID: 4838 RVA: 0x0007CCC8 File Offset: 0x0007AEC8
	private void UpdateBaseOffset(Player player, float dt)
	{
		Vector3 cameraBaseOffset = this.GetCameraBaseOffset(player);
		this.m_currentBaseOffset = Vector3.SmoothDamp(this.m_currentBaseOffset, cameraBaseOffset, ref this.m_offsetBaseVel, 0.5f, 999f, dt);
		if (Vector3.Distance(this.m_playerPos, player.transform.position) > 20f)
		{
			this.m_playerPos = player.transform.position;
		}
		this.m_playerPos = Vector3.SmoothDamp(this.m_playerPos, player.transform.position, ref this.m_playerVel, this.m_smoothness, 999f, dt);
	}

	// Token: 0x060012E7 RID: 4839 RVA: 0x0007CD5C File Offset: 0x0007AF5C
	private Vector3 GetOffsetedEyePos()
	{
		Player localPlayer = Player.m_localPlayer;
		if (!localPlayer)
		{
			return base.transform.position;
		}
		if (localPlayer.GetStandingOnShip() != null || localPlayer.IsAttached())
		{
			return localPlayer.transform.position + this.m_currentBaseOffset + this.GetCameraOffset(localPlayer);
		}
		return this.m_playerPos + this.m_currentBaseOffset + this.GetCameraOffset(localPlayer);
	}

	// Token: 0x060012E8 RID: 4840 RVA: 0x0007CDDC File Offset: 0x0007AFDC
	private Vector3 GetCameraOffset(Player player)
	{
		if (this.m_distance <= 0f)
		{
			return player.m_eye.transform.TransformVector(this.m_fpsOffset);
		}
		if (player.InBed())
		{
			return Vector3.zero;
		}
		Vector3 vector = player.UseMeleeCamera() ? this.m_3rdCombatOffset : this.m_3rdOffset;
		return player.m_eye.transform.TransformVector(vector);
	}

	// Token: 0x060012E9 RID: 4841 RVA: 0x0007CE43 File Offset: 0x0007B043
	public void ToggleFreeFly()
	{
		this.m_freeFly = !this.m_freeFly;
	}

	// Token: 0x060012EA RID: 4842 RVA: 0x0007CE54 File Offset: 0x0007B054
	public void SetFreeFlySmoothness(float smooth)
	{
		this.m_freeFlySmooth = Mathf.Clamp(smooth, 0f, 1f);
	}

	// Token: 0x060012EB RID: 4843 RVA: 0x0007CE6C File Offset: 0x0007B06C
	public float GetFreeFlySmoothness()
	{
		return this.m_freeFlySmooth;
	}

	// Token: 0x060012EC RID: 4844 RVA: 0x0007CE74 File Offset: 0x0007B074
	public static bool InFreeFly()
	{
		return GameCamera.m_instance && GameCamera.m_instance.m_freeFly;
	}

	// Token: 0x060012EE RID: 4846 RVA: 0x0007D034 File Offset: 0x0007B234
	[CompilerGenerated]
	private float <UpdateCamera>g__debugCamera|9_0(float scroll)
	{
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.C) && !global::Console.IsVisible())
		{
			float axis = Input.GetAxis("Mouse Y");
			EnvMan.instance.m_debugTimeOfDay = true;
			EnvMan.instance.m_debugTime = (EnvMan.instance.m_debugTime + axis * 0.005f) % 1f;
			if (EnvMan.instance.m_debugTime < 0f)
			{
				EnvMan.instance.m_debugTime += 1f;
			}
			float axis2 = Input.GetAxis("Mouse X");
			this.m_fov += axis2 * 1f;
			this.m_fov = Mathf.Clamp(this.m_fov, 0.5f, 165f);
			this.m_camera.fieldOfView = this.m_fov;
			this.m_skyCamera.fieldOfView = this.m_fov;
			if (Player.m_localPlayer && Player.m_localPlayer.IsDebugFlying())
			{
				if (scroll > 0f)
				{
					Character.m_debugFlySpeed = (int)Mathf.Clamp((float)Character.m_debugFlySpeed * 1.1f, (float)(Character.m_debugFlySpeed + 1), 300f);
				}
				else if (scroll < 0f && Character.m_debugFlySpeed > 1)
				{
					Character.m_debugFlySpeed = (int)Mathf.Min((float)Character.m_debugFlySpeed * 0.9f, (float)(Character.m_debugFlySpeed - 1));
				}
			}
			scroll = 0f;
		}
		return scroll;
	}

	// Token: 0x04001395 RID: 5013
	private Vector3 m_playerPos = Vector3.zero;

	// Token: 0x04001396 RID: 5014
	private Vector3 m_currentBaseOffset = Vector3.zero;

	// Token: 0x04001397 RID: 5015
	private Vector3 m_offsetBaseVel = Vector3.zero;

	// Token: 0x04001398 RID: 5016
	private Vector3 m_playerVel = Vector3.zero;

	// Token: 0x04001399 RID: 5017
	public Vector3 m_3rdOffset = Vector3.zero;

	// Token: 0x0400139A RID: 5018
	public Vector3 m_3rdCombatOffset = Vector3.zero;

	// Token: 0x0400139B RID: 5019
	public Vector3 m_fpsOffset = Vector3.zero;

	// Token: 0x0400139C RID: 5020
	public float m_flyingDistance = 15f;

	// Token: 0x0400139D RID: 5021
	public LayerMask m_blockCameraMask;

	// Token: 0x0400139E RID: 5022
	public float m_minDistance;

	// Token: 0x0400139F RID: 5023
	public float m_maxDistance = 6f;

	// Token: 0x040013A0 RID: 5024
	public float m_maxDistanceBoat = 6f;

	// Token: 0x040013A1 RID: 5025
	public float m_raycastWidth = 0.35f;

	// Token: 0x040013A2 RID: 5026
	public bool m_smoothYTilt;

	// Token: 0x040013A3 RID: 5027
	public float m_zoomSens = 10f;

	// Token: 0x040013A4 RID: 5028
	public float m_inventoryOffset = 0.1f;

	// Token: 0x040013A5 RID: 5029
	public float m_nearClipPlaneMin = 0.1f;

	// Token: 0x040013A6 RID: 5030
	public float m_nearClipPlaneMax = 0.5f;

	// Token: 0x040013A7 RID: 5031
	public float m_fov = 65f;

	// Token: 0x040013A8 RID: 5032
	public float m_freeFlyMinFov = 5f;

	// Token: 0x040013A9 RID: 5033
	public float m_freeFlyMaxFov = 120f;

	// Token: 0x040013AA RID: 5034
	public float m_tiltSmoothnessShipMin = 0.1f;

	// Token: 0x040013AB RID: 5035
	public float m_tiltSmoothnessShipMax = 0.5f;

	// Token: 0x040013AC RID: 5036
	public float m_shakeFreq = 10f;

	// Token: 0x040013AD RID: 5037
	public float m_shakeMovement = 1f;

	// Token: 0x040013AE RID: 5038
	public float m_smoothness = 0.1f;

	// Token: 0x040013AF RID: 5039
	public float m_minWaterDistance = 0.3f;

	// Token: 0x040013B0 RID: 5040
	public Camera m_skyCamera;

	// Token: 0x040013B1 RID: 5041
	private float m_distance = 4f;

	// Token: 0x040013B2 RID: 5042
	private bool m_freeFly;

	// Token: 0x040013B3 RID: 5043
	private float m_shakeIntensity;

	// Token: 0x040013B4 RID: 5044
	private float m_shakeTimer;

	// Token: 0x040013B5 RID: 5045
	private bool m_cameraShakeEnabled = true;

	// Token: 0x040013B6 RID: 5046
	private bool m_mouseCapture;

	// Token: 0x040013B7 RID: 5047
	private Quaternion m_freeFlyRef = Quaternion.identity;

	// Token: 0x040013B8 RID: 5048
	private float m_freeFlyYaw;

	// Token: 0x040013B9 RID: 5049
	private float m_freeFlyPitch;

	// Token: 0x040013BA RID: 5050
	private float m_freeFlySpeed = 20f;

	// Token: 0x040013BB RID: 5051
	private float m_freeFlySmooth;

	// Token: 0x040013BC RID: 5052
	private Vector3 m_freeFlySavedVel = Vector3.zero;

	// Token: 0x040013BD RID: 5053
	private Transform m_freeFlyTarget;

	// Token: 0x040013BE RID: 5054
	private Vector3 m_freeFlyTargetOffset = Vector3.zero;

	// Token: 0x040013BF RID: 5055
	private Transform m_freeFlyLockon;

	// Token: 0x040013C0 RID: 5056
	private Vector3 m_freeFlyLockonOffset = Vector3.zero;

	// Token: 0x040013C1 RID: 5057
	private Vector3 m_freeFlyVel = Vector3.zero;

	// Token: 0x040013C2 RID: 5058
	private Vector3 m_freeFlyAcc = Vector3.zero;

	// Token: 0x040013C3 RID: 5059
	private Vector3 m_freeFlyTurnVel = Vector3.zero;

	// Token: 0x040013C4 RID: 5060
	private bool m_shipCameraTilt = true;

	// Token: 0x040013C5 RID: 5061
	private Vector3 m_smoothedCameraUp = Vector3.up;

	// Token: 0x040013C6 RID: 5062
	private Vector3 m_smoothedCameraUpVel = Vector3.zero;

	// Token: 0x040013C7 RID: 5063
	private AudioListener m_listner;

	// Token: 0x040013C8 RID: 5064
	private Camera m_camera;

	// Token: 0x040013C9 RID: 5065
	private bool m_waterClipping;

	// Token: 0x040013CA RID: 5066
	private static GameCamera m_instance;
}
