using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000020 RID: 32
public class Player : Humanoid
{
	// Token: 0x060001BE RID: 446 RVA: 0x0000C564 File Offset: 0x0000A764
	protected override void Awake()
	{
		base.Awake();
		Player.s_players.Add(this);
		this.m_skills = base.GetComponent<Skills>();
		this.SetupAwake();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_placeRayMask = LayerMask.GetMask(new string[]
		{
			"Default",
			"static_solid",
			"Default_small",
			"piece",
			"piece_nonsolid",
			"terrain",
			"vehicle"
		});
		this.m_placeWaterRayMask = LayerMask.GetMask(new string[]
		{
			"Default",
			"static_solid",
			"Default_small",
			"piece",
			"piece_nonsolid",
			"terrain",
			"Water",
			"vehicle"
		});
		this.m_removeRayMask = LayerMask.GetMask(new string[]
		{
			"Default",
			"static_solid",
			"Default_small",
			"piece",
			"piece_nonsolid",
			"terrain",
			"vehicle"
		});
		this.m_interactMask = LayerMask.GetMask(new string[]
		{
			"item",
			"piece",
			"piece_nonsolid",
			"Default",
			"static_solid",
			"Default_small",
			"character",
			"character_net",
			"terrain",
			"vehicle"
		});
		this.m_autoPickupMask = LayerMask.GetMask(new string[]
		{
			"item"
		});
		Inventory inventory = this.m_inventory;
		inventory.m_onChanged = (Action)Delegate.Combine(inventory.m_onChanged, new Action(this.OnInventoryChanged));
		if (Player.s_attackMask == 0)
		{
			Player.s_attackMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"piece_nonsolid",
				"terrain",
				"character",
				"character_net",
				"character_ghost",
				"hitbox",
				"character_noenv",
				"vehicle"
			});
		}
		this.m_nview.Register("OnDeath", new Action<long>(this.RPC_OnDeath));
		if (this.m_nview.IsOwner())
		{
			this.m_nview.Register<int, string, int>("Message", new Action<long, int, string, int>(this.RPC_Message));
			this.m_nview.Register<bool, bool>("OnTargeted", new Action<long, bool, bool>(this.RPC_OnTargeted));
			this.m_nview.Register<float>("UseStamina", new Action<long, float>(this.RPC_UseStamina));
			if (MusicMan.instance)
			{
				MusicMan.instance.TriggerMusic("Wakeup");
			}
			this.UpdateKnownRecipesList();
			this.UpdateAvailablePiecesList();
			this.SetupPlacementGhost();
		}
		this.m_placeRotation = UnityEngine.Random.Range(0, 16);
		float f = UnityEngine.Random.Range(0f, 6.2831855f);
		base.SetLookDir(new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f)), 0f);
		this.FaceLookDirection();
	}

	// Token: 0x060001BF RID: 447 RVA: 0x0000C895 File Offset: 0x0000AA95
	protected override void OnEnable()
	{
		base.OnEnable();
	}

	// Token: 0x060001C0 RID: 448 RVA: 0x0000C89D File Offset: 0x0000AA9D
	protected override void OnDisable()
	{
		base.OnDisable();
	}

	// Token: 0x060001C1 RID: 449 RVA: 0x0000C8A5 File Offset: 0x0000AAA5
	public void SetLocalPlayer()
	{
		if (Player.m_localPlayer == this)
		{
			return;
		}
		Player.m_localPlayer = this;
		ZNet.instance.SetReferencePosition(base.transform.position);
		EnvMan.instance.SetForceEnvironment("");
	}

	// Token: 0x060001C2 RID: 450 RVA: 0x0000C8E0 File Offset: 0x0000AAE0
	public void SetPlayerID(long playerID, string name)
	{
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		if (this.GetPlayerID() != 0L)
		{
			return;
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_playerID, playerID);
		this.m_nview.GetZDO().Set(ZDOVars.s_playerName, name);
	}

	// Token: 0x060001C3 RID: 451 RVA: 0x0000C930 File Offset: 0x0000AB30
	public long GetPlayerID()
	{
		if (!this.m_nview.IsValid())
		{
			return 0L;
		}
		return this.m_nview.GetZDO().GetLong(ZDOVars.s_playerID, 0L);
	}

	// Token: 0x060001C4 RID: 452 RVA: 0x0000C959 File Offset: 0x0000AB59
	public string GetPlayerName()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		return this.m_nview.GetZDO().GetString(ZDOVars.s_playerName, "...");
	}

	// Token: 0x060001C5 RID: 453 RVA: 0x0000C988 File Offset: 0x0000AB88
	public override string GetHoverText()
	{
		return "";
	}

	// Token: 0x060001C6 RID: 454 RVA: 0x0000C98F File Offset: 0x0000AB8F
	public override string GetHoverName()
	{
		return this.GetPlayerName();
	}

	// Token: 0x060001C7 RID: 455 RVA: 0x0000C997 File Offset: 0x0000AB97
	protected override void Start()
	{
		base.Start();
	}

	// Token: 0x060001C8 RID: 456 RVA: 0x0000C9A0 File Offset: 0x0000ABA0
	protected override void OnDestroy()
	{
		ZDO zdo = this.m_nview.GetZDO();
		if (zdo != null && ZNet.instance != null)
		{
			ZLog.LogWarning(string.Concat(new string[]
			{
				"Player destroyed sec:",
				zdo.GetSector().ToString(),
				"  pos:",
				base.transform.position.ToString(),
				"  zdopos:",
				zdo.GetPosition().ToString(),
				"  ref ",
				ZNet.instance.GetReferencePosition().ToString()
			}));
		}
		if (this.m_placementGhost)
		{
			UnityEngine.Object.Destroy(this.m_placementGhost);
			this.m_placementGhost = null;
		}
		base.OnDestroy();
		Player.s_players.Remove(this);
		if (Player.m_localPlayer == this)
		{
			ZLog.LogWarning("Local player destroyed");
			Player.m_localPlayer = null;
		}
	}

	// Token: 0x060001C9 RID: 457 RVA: 0x0000CAB4 File Offset: 0x0000ACB4
	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		this.UpdateAwake(fixedDeltaTime);
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.UpdateTargeted(fixedDeltaTime);
		if (this.m_nview.IsOwner())
		{
			if (Player.m_localPlayer != this)
			{
				ZLog.Log("Destroying old local player");
				ZNetScene.instance.Destroy(base.gameObject);
				return;
			}
			if (this.IsDead())
			{
				return;
			}
			this.UpdateActionQueue(fixedDeltaTime);
			this.PlayerAttackInput(fixedDeltaTime);
			this.UpdateAttach();
			this.UpdateDoodadControls(fixedDeltaTime);
			this.UpdateCrouch(fixedDeltaTime);
			this.UpdateDodge(fixedDeltaTime);
			this.UpdateCover(fixedDeltaTime);
			this.UpdateStations(fixedDeltaTime);
			this.UpdateGuardianPower(fixedDeltaTime);
			this.UpdateBaseValue(fixedDeltaTime);
			this.UpdateStats(fixedDeltaTime);
			this.UpdateTeleport(fixedDeltaTime);
			this.AutoPickup(fixedDeltaTime);
			this.EdgeOfWorldKill(fixedDeltaTime);
			this.UpdateBiome(fixedDeltaTime);
			this.UpdateStealth(fixedDeltaTime);
			if (GameCamera.instance && Vector3.Distance(GameCamera.instance.transform.position, base.transform.position) < 2f)
			{
				base.SetVisible(false);
			}
			AudioMan.instance.SetIndoor(this.InShelter());
		}
	}

	// Token: 0x060001CA RID: 458 RVA: 0x0000CBDC File Offset: 0x0000ADDC
	private void Update()
	{
		bool flag = InventoryGui.IsVisible();
		if (ZInput.InputLayout != InputLayout.Default && ZInput.IsGamepadActive() && !flag && (ZInput.GetButtonUp("JoyAltPlace") && ZInput.GetButton("JoyAltKeys")))
		{
			this.m_altPlace = !this.m_altPlace;
			if (MessageHud.instance != null)
			{
				string str = Localization.instance.Localize("$hud_altplacement");
				string str2 = this.m_altPlace ? Localization.instance.Localize("$hud_on") : Localization.instance.Localize("$hud_off");
				MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, str + " " + str2, 0, null);
			}
		}
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		bool flag2 = this.TakeInput();
		this.UpdateHover();
		if (flag2)
		{
			if (Player.m_debugMode && global::Console.instance.IsCheatsEnabled())
			{
				if (Input.GetKeyDown(KeyCode.Z))
				{
					this.ToggleDebugFly();
				}
				if (Input.GetKeyDown(KeyCode.B))
				{
					this.ToggleNoPlacementCost();
				}
				if (Input.GetKeyDown(KeyCode.K))
				{
					global::Console.instance.TryRunCommand("killenemies", false, false);
				}
				if (Input.GetKeyDown(KeyCode.L))
				{
					global::Console.instance.TryRunCommand("removedrops", false, false);
				}
			}
			bool alt = (ZInput.InputLayout == InputLayout.Alternative1 && ZInput.IsGamepadActive()) ? ZInput.GetButton("JoyAltKeys") : (ZInput.GetButton("AltPlace") || ZInput.GetButton("JoyAltPlace"));
			if (ZInput.GetButtonDown("Use") || ZInput.GetButtonDown("JoyUse"))
			{
				if (this.m_hovering)
				{
					this.Interact(this.m_hovering, false, alt);
				}
				else if (this.m_doodadController != null)
				{
					this.StopDoodadControl();
				}
			}
			else if ((ZInput.GetButton("Use") || ZInput.GetButton("JoyUse")) && this.m_hovering)
			{
				this.Interact(this.m_hovering, true, alt);
			}
			if ((ZInput.InputLayout != InputLayout.Default && ZInput.IsGamepadActive()) ? (!this.InPlaceMode() && ZInput.GetButtonDown("JoyHide") && !ZInput.GetButton("JoyAltKeys")) : (ZInput.GetButtonDown("Hide") || (ZInput.GetButtonDown("JoyHide") && !ZInput.GetButton("JoyAltKeys"))))
			{
				if (base.GetRightItem() != null || base.GetLeftItem() != null)
				{
					if (!this.InAttack() && !this.InDodge())
					{
						base.HideHandItems();
					}
				}
				else if ((!base.IsSwimming() || base.IsOnGround()) && !this.InDodge())
				{
					base.ShowHandItems();
				}
			}
			if (ZInput.GetButtonDown("ToggleWalk"))
			{
				base.SetWalk(!base.GetWalk());
				if (base.GetWalk())
				{
					this.Message(MessageHud.MessageType.TopLeft, "$msg_walk $hud_on", 0, null);
				}
				else
				{
					this.Message(MessageHud.MessageType.TopLeft, "$msg_walk $hud_off", 0, null);
				}
			}
			if (ZInput.GetButtonDown("Sit") || (!this.InPlaceMode() && ZInput.GetButtonDown("JoySit")))
			{
				if (this.InEmote() && this.IsSitting())
				{
					this.StopEmote();
				}
				else
				{
					this.StartEmote("sit", false);
				}
			}
			bool flag3 = ZInput.IsGamepadActive() && !ZInput.GetButton("JoyAltKeys");
			bool flag4 = ZInput.InputLayout == InputLayout.Default && ZInput.GetButtonDown("JoyGP");
			bool flag5 = ZInput.InputLayout == InputLayout.Alternative1 && ZInput.GetButton("JoyLStick") && ZInput.GetButton("JoyRStick");
			if (ZInput.GetButtonDown("GP") || (flag3 && (flag4 || flag5)))
			{
				this.StartGuardianPower();
			}
			bool flag6 = ZInput.GetButtonDown("JoyAutoPickup") && ZInput.GetButton("JoyAltKeys");
			if (ZInput.GetButtonDown("AutoPickup") || flag6)
			{
				this.m_enableAutoPickup = !this.m_enableAutoPickup;
				this.Message(MessageHud.MessageType.TopLeft, "$hud_autopickup:" + (this.m_enableAutoPickup ? "$hud_on" : "$hud_off"), 0, null);
			}
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				this.UseHotbarItem(1);
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				this.UseHotbarItem(2);
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				this.UseHotbarItem(3);
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				this.UseHotbarItem(4);
			}
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				this.UseHotbarItem(5);
			}
			if (Input.GetKeyDown(KeyCode.Alpha6))
			{
				this.UseHotbarItem(6);
			}
			if (Input.GetKeyDown(KeyCode.Alpha7))
			{
				this.UseHotbarItem(7);
			}
			if (Input.GetKeyDown(KeyCode.Alpha8))
			{
				this.UseHotbarItem(8);
			}
		}
		this.UpdatePlacement(flag2, Time.deltaTime);
	}

	// Token: 0x060001CB RID: 459 RVA: 0x0000D064 File Offset: 0x0000B264
	private void UpdatePlacement(bool takeInput, float dt)
	{
		this.UpdateWearNTearHover();
		if (!this.InPlaceMode())
		{
			if (this.m_placementGhost)
			{
				this.m_placementGhost.SetActive(false);
			}
			return;
		}
		if (!takeInput)
		{
			return;
		}
		this.UpdateBuildGuiInput();
		if (Hud.IsPieceSelectionVisible())
		{
			return;
		}
		ItemDrop.ItemData rightItem = base.GetRightItem();
		if (ZInput.GetButtonDown("Remove") || ZInput.GetButtonDown("JoyRemove"))
		{
			this.m_removePressedTime = Time.time;
		}
		if (Time.time - this.m_removePressedTime < 0.2f && rightItem.m_shared.m_buildPieces.m_canRemovePieces && Time.time - this.m_lastToolUseTime > this.m_removeDelay)
		{
			this.m_removePressedTime = -9999f;
			if (this.HaveStamina(rightItem.m_shared.m_attack.m_attackStamina))
			{
				if (this.RemovePiece())
				{
					this.m_lastToolUseTime = Time.time;
					base.AddNoise(50f);
					this.UseStamina(rightItem.m_shared.m_attack.m_attackStamina);
					if (rightItem.m_shared.m_useDurability)
					{
						rightItem.m_durability -= rightItem.m_shared.m_useDurabilityDrain;
					}
				}
			}
			else
			{
				Hud.instance.StaminaBarEmptyFlash();
			}
		}
		Piece selectedPiece = this.m_buildPieces.GetSelectedPiece();
		if (selectedPiece != null)
		{
			if (ZInput.GetButtonDown("Attack") || ZInput.GetButtonDown("JoyPlace"))
			{
				this.m_placePressedTime = Time.time;
			}
			if (Time.time - this.m_placePressedTime < 0.2f && Time.time - this.m_lastToolUseTime > this.m_placeDelay)
			{
				this.m_placePressedTime = -9999f;
				if (this.HaveStamina(rightItem.m_shared.m_attack.m_attackStamina))
				{
					if (selectedPiece.m_repairPiece)
					{
						this.Repair(rightItem, selectedPiece);
					}
					else if (this.m_placementGhost != null)
					{
						if (this.m_noPlacementCost || this.HaveRequirements(selectedPiece, Player.RequirementMode.CanBuild))
						{
							if (this.PlacePiece(selectedPiece))
							{
								this.m_lastToolUseTime = Time.time;
								this.ConsumeResources(selectedPiece.m_resources, 0, -1);
								this.UseStamina(rightItem.m_shared.m_attack.m_attackStamina);
								if (rightItem.m_shared.m_useDurability)
								{
									rightItem.m_durability -= rightItem.m_shared.m_useDurabilityDrain;
								}
							}
						}
						else
						{
							this.Message(MessageHud.MessageType.Center, "$msg_missingrequirement", 0, null);
						}
					}
				}
				else
				{
					Hud.instance.StaminaBarEmptyFlash();
				}
			}
		}
		if (this.m_placementGhost != null)
		{
			IPieceMarker component = this.m_placementGhost.gameObject.GetComponent<IPieceMarker>();
			if (component != null)
			{
				component.ShowBuildMarker();
			}
		}
		Piece hoveringPiece = this.GetHoveringPiece();
		if (hoveringPiece)
		{
			IPieceMarker component2 = hoveringPiece.gameObject.GetComponent<IPieceMarker>();
			if (component2 != null)
			{
				component2.ShowHoverMarker();
			}
		}
		if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			this.m_placeRotation--;
		}
		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			this.m_placeRotation++;
		}
		float num = ZInput.GetJoyRightStickX();
		bool flag = ZInput.GetButton("JoyRotate") && Mathf.Abs(num) > 0.5f;
		if (ZInput.IsGamepadActive() && ZInput.InputLayout == InputLayout.Alternative1)
		{
			flag = (ZInput.GetButton("JoyRotate") || ZInput.GetButton("JoyRotateRight"));
			num = (ZInput.GetButton("JoyRotate") ? 0.5f : -0.5f);
		}
		if (flag)
		{
			if (this.m_rotatePieceTimer == 0f)
			{
				if (num < 0f)
				{
					this.m_placeRotation++;
				}
				else
				{
					this.m_placeRotation--;
				}
			}
			else if (this.m_rotatePieceTimer > 0.25f)
			{
				if (num < 0f)
				{
					this.m_placeRotation++;
				}
				else
				{
					this.m_placeRotation--;
				}
				this.m_rotatePieceTimer = 0.17f;
			}
			this.m_rotatePieceTimer += dt;
			return;
		}
		this.m_rotatePieceTimer = 0f;
	}

	// Token: 0x060001CC RID: 460 RVA: 0x0000D470 File Offset: 0x0000B670
	private void UpdateBuildGuiInputAlternative1()
	{
		if (!Hud.IsPieceSelectionVisible() && ZInput.GetButtonDown("JoyBuildMenu"))
		{
			for (int i = 0; i < this.m_buildPieces.m_selectedPiece.Length; i++)
			{
				this.m_buildPieces.m_lastSelectedPiece[i] = this.m_buildPieces.m_selectedPiece[i];
			}
			Hud.instance.TogglePieceSelection();
			return;
		}
		if (Hud.IsPieceSelectionVisible())
		{
			if (ZInput.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyButtonB"))
			{
				for (int j = 0; j < this.m_buildPieces.m_selectedPiece.Length; j++)
				{
					this.m_buildPieces.m_selectedPiece[j] = this.m_buildPieces.m_lastSelectedPiece[j];
				}
				Hud.HidePieceSelection();
				this.SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyButtonA"))
			{
				Hud.HidePieceSelection();
			}
			if (ZInput.GetButtonDown("JoyTabLeft") || ZInput.GetButtonDown("TabLeft") || ZInput.GetAxis("Mouse ScrollWheel") > 0f)
			{
				this.m_buildPieces.PrevCategory();
				this.UpdateAvailablePiecesList();
			}
			if (ZInput.GetButtonDown("JoyTabRight") || ZInput.GetButtonDown("TabRight") || ZInput.GetAxis("Mouse ScrollWheel") < 0f)
			{
				this.m_buildPieces.NextCategory();
				this.UpdateAvailablePiecesList();
			}
			if (ZInput.GetButtonDown("JoyLStickLeft") || ZInput.GetButtonDown("JoyDPadLeft"))
			{
				this.m_buildPieces.LeftPiece();
				this.SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickRight") || ZInput.GetButtonDown("JoyDPadRight"))
			{
				this.m_buildPieces.RightPiece();
				this.SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyDPadUp"))
			{
				this.m_buildPieces.UpPiece();
				this.SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickDown") || ZInput.GetButtonDown("JoyDPadDown"))
			{
				this.m_buildPieces.DownPiece();
				this.SetupPlacementGhost();
			}
		}
	}

	// Token: 0x060001CD RID: 461 RVA: 0x0000D664 File Offset: 0x0000B864
	private void UpdateBuildGuiInput()
	{
		if (ZInput.InputLayout == InputLayout.Alternative1 && ZInput.IsGamepadActive())
		{
			this.UpdateBuildGuiInputAlternative1();
			return;
		}
		if (Hud.instance.IsQuickPieceSelectEnabled())
		{
			if (!Hud.IsPieceSelectionVisible() && ZInput.GetButtonDown("BuildMenu"))
			{
				Hud.instance.TogglePieceSelection();
			}
		}
		else if (ZInput.GetButtonDown("BuildMenu"))
		{
			Hud.instance.TogglePieceSelection();
		}
		if (ZInput.GetButtonDown("JoyUse"))
		{
			Hud.instance.TogglePieceSelection();
		}
		if (Hud.IsPieceSelectionVisible())
		{
			if (Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyButtonB"))
			{
				Hud.HidePieceSelection();
			}
			if (ZInput.GetButtonDown("JoyTabLeft") || ZInput.GetButtonDown("TabLeft") || Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				this.m_buildPieces.PrevCategory();
				this.UpdateAvailablePiecesList();
			}
			if (ZInput.GetButtonDown("JoyTabRight") || ZInput.GetButtonDown("TabRight") || Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				this.m_buildPieces.NextCategory();
				this.UpdateAvailablePiecesList();
			}
			if (ZInput.GetButtonDown("JoyLStickLeft") || ZInput.GetButtonDown("JoyDPadLeft"))
			{
				this.m_buildPieces.LeftPiece();
				this.SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickRight") || ZInput.GetButtonDown("JoyDPadRight"))
			{
				this.m_buildPieces.RightPiece();
				this.SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyDPadUp"))
			{
				this.m_buildPieces.UpPiece();
				this.SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickDown") || ZInput.GetButtonDown("JoyDPadDown"))
			{
				this.m_buildPieces.DownPiece();
				this.SetupPlacementGhost();
			}
		}
	}

	// Token: 0x060001CE RID: 462 RVA: 0x0000D81A File Offset: 0x0000BA1A
	public void SetSelectedPiece(Vector2Int p)
	{
		if (this.m_buildPieces && this.m_buildPieces.GetSelectedIndex() != p)
		{
			this.m_buildPieces.SetSelected(p);
			this.SetupPlacementGhost();
		}
	}

	// Token: 0x060001CF RID: 463 RVA: 0x0000D84E File Offset: 0x0000BA4E
	public Piece GetPiece(Vector2Int p)
	{
		if (!(this.m_buildPieces != null))
		{
			return null;
		}
		return this.m_buildPieces.GetPiece(p);
	}

	// Token: 0x060001D0 RID: 464 RVA: 0x0000D86C File Offset: 0x0000BA6C
	public bool IsPieceAvailable(Piece piece)
	{
		return this.m_buildPieces != null && this.m_buildPieces.IsPieceAvailable(piece);
	}

	// Token: 0x060001D1 RID: 465 RVA: 0x0000D88A File Offset: 0x0000BA8A
	public Piece GetSelectedPiece()
	{
		if (!(this.m_buildPieces != null))
		{
			return null;
		}
		return this.m_buildPieces.GetSelectedPiece();
	}

	// Token: 0x060001D2 RID: 466 RVA: 0x0000D8A7 File Offset: 0x0000BAA7
	private void LateUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.UpdateEmote();
		if (this.m_nview.IsOwner())
		{
			ZNet.instance.SetReferencePosition(base.transform.position);
			this.UpdatePlacementGhost(false);
		}
	}

	// Token: 0x060001D3 RID: 467 RVA: 0x0000D8E8 File Offset: 0x0000BAE8
	private void SetupAwake()
	{
		if (this.m_nview.GetZDO() == null)
		{
			this.m_animator.SetBool("wakeup", false);
			return;
		}
		bool @bool = this.m_nview.GetZDO().GetBool(ZDOVars.s_wakeup, true);
		this.m_animator.SetBool("wakeup", @bool);
		if (@bool)
		{
			this.m_wakeupTimer = 0f;
		}
	}

	// Token: 0x060001D4 RID: 468 RVA: 0x0000D94C File Offset: 0x0000BB4C
	private void UpdateAwake(float dt)
	{
		if (this.m_wakeupTimer >= 0f)
		{
			this.m_wakeupTimer += dt;
			if (this.m_wakeupTimer > 1f)
			{
				this.m_wakeupTimer = -1f;
				this.m_animator.SetBool("wakeup", false);
				if (this.m_nview.IsOwner())
				{
					this.m_nview.GetZDO().Set(ZDOVars.s_wakeup, false);
				}
			}
		}
	}

	// Token: 0x060001D5 RID: 469 RVA: 0x0000D9C0 File Offset: 0x0000BBC0
	private void EdgeOfWorldKill(float dt)
	{
		if (this.IsDead())
		{
			return;
		}
		float num = Utils.DistanceXZ(Vector3.zero, base.transform.position);
		float num2 = 10420f;
		if (num > num2 && (base.IsSwimming() || base.transform.position.y < ZoneSystem.instance.m_waterLevel))
		{
			Vector3 a = Vector3.Normalize(base.transform.position);
			float d = Utils.LerpStep(num2, 10500f, num) * 10f;
			this.m_body.MovePosition(this.m_body.position + a * d * dt);
		}
		if (num > num2 && base.transform.position.y < ZoneSystem.instance.m_waterLevel - 40f)
		{
			HitData hitData = new HitData();
			hitData.m_damage.m_damage = 99999f;
			base.Damage(hitData);
		}
	}

	// Token: 0x060001D6 RID: 470 RVA: 0x0000DAAC File Offset: 0x0000BCAC
	private void AutoPickup(float dt)
	{
		if (this.IsTeleporting())
		{
			return;
		}
		if (!this.m_enableAutoPickup)
		{
			return;
		}
		Vector3 vector = base.transform.position + Vector3.up;
		foreach (Collider collider in Physics.OverlapSphere(vector, this.m_autoPickupRange, this.m_autoPickupMask))
		{
			if (collider.attachedRigidbody)
			{
				ItemDrop component = collider.attachedRigidbody.GetComponent<ItemDrop>();
				if (!(component == null) && component.m_autoPickup && !this.HaveUniqueKey(component.m_itemData.m_shared.m_name) && component.GetComponent<ZNetView>().IsValid())
				{
					if (!component.CanPickup(true))
					{
						component.RequestOwn();
					}
					else if (!component.InTar())
					{
						component.Load();
						if (this.m_inventory.CanAddItem(component.m_itemData, -1) && component.m_itemData.GetWeight() + this.m_inventory.GetTotalWeight() <= this.GetMaxCarryWeight())
						{
							float num = Vector3.Distance(component.transform.position, vector);
							if (num <= this.m_autoPickupRange)
							{
								if (num < 0.3f)
								{
									base.Pickup(component.gameObject, true, true);
								}
								else
								{
									Vector3 a = Vector3.Normalize(vector - component.transform.position);
									float d = 15f;
									component.transform.position = component.transform.position + a * d * dt;
								}
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x060001D7 RID: 471 RVA: 0x0000DC58 File Offset: 0x0000BE58
	private void PlayerAttackInput(float dt)
	{
		if (this.InPlaceMode())
		{
			return;
		}
		ItemDrop.ItemData currentWeapon = base.GetCurrentWeapon();
		this.UpdateWeaponLoading(currentWeapon, dt);
		if (currentWeapon != null && currentWeapon.m_shared.m_attack.m_bowDraw)
		{
			this.UpdateAttackBowDraw(currentWeapon, dt);
		}
		else
		{
			if (this.m_attack)
			{
				this.m_queuedAttackTimer = 0.5f;
				this.m_queuedSecondAttackTimer = 0f;
			}
			if (this.m_secondaryAttack)
			{
				this.m_queuedSecondAttackTimer = 0.5f;
				this.m_queuedAttackTimer = 0f;
			}
			this.m_queuedAttackTimer -= Time.fixedDeltaTime;
			this.m_queuedSecondAttackTimer -= Time.fixedDeltaTime;
			if ((this.m_queuedAttackTimer > 0f || this.m_attackHold) && this.StartAttack(null, false))
			{
				this.m_queuedAttackTimer = 0f;
			}
			if ((this.m_queuedSecondAttackTimer > 0f || this.m_secondaryAttackHold) && this.StartAttack(null, true))
			{
				this.m_queuedSecondAttackTimer = 0f;
			}
		}
		if (this.m_currentAttack != null && this.m_currentAttack.m_loopingAttack && !(this.m_currentAttackIsSecondary ? this.m_secondaryAttackHold : this.m_attackHold))
		{
			this.m_currentAttack.Abort();
		}
	}

	// Token: 0x060001D8 RID: 472 RVA: 0x0000DD8C File Offset: 0x0000BF8C
	private void UpdateWeaponLoading(ItemDrop.ItemData weapon, float dt)
	{
		if (weapon == null || !weapon.m_shared.m_attack.m_requiresReload)
		{
			this.SetWeaponLoaded(null);
			return;
		}
		if (this.m_weaponLoaded == weapon)
		{
			return;
		}
		if (weapon.m_shared.m_attack.m_requiresReload && !this.IsReloadActionQueued())
		{
			this.QueueReloadAction();
		}
	}

	// Token: 0x060001D9 RID: 473 RVA: 0x0000DDE0 File Offset: 0x0000BFE0
	private void CancelReloadAction()
	{
		foreach (Player.MinorActionData minorActionData in this.m_actionQueue)
		{
			if (minorActionData.m_type == Player.MinorActionData.ActionType.Reload)
			{
				this.m_actionQueue.Remove(minorActionData);
				break;
			}
		}
	}

	// Token: 0x060001DA RID: 474 RVA: 0x0000DE44 File Offset: 0x0000C044
	public override void ResetLoadedWeapon()
	{
		this.SetWeaponLoaded(null);
		foreach (Player.MinorActionData minorActionData in this.m_actionQueue)
		{
			if (minorActionData.m_type == Player.MinorActionData.ActionType.Reload)
			{
				this.m_actionQueue.Remove(minorActionData);
				break;
			}
		}
	}

	// Token: 0x060001DB RID: 475 RVA: 0x0000DEB0 File Offset: 0x0000C0B0
	private void SetWeaponLoaded(ItemDrop.ItemData weapon)
	{
		if (weapon == this.m_weaponLoaded)
		{
			return;
		}
		this.m_weaponLoaded = weapon;
		this.m_nview.GetZDO().Set(ZDOVars.s_weaponLoaded, weapon != null);
	}

	// Token: 0x060001DC RID: 476 RVA: 0x0000DEDC File Offset: 0x0000C0DC
	public override bool IsWeaponLoaded()
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (!this.m_nview.IsOwner())
		{
			return this.m_nview.GetZDO().GetBool(ZDOVars.s_weaponLoaded, false);
		}
		return this.m_weaponLoaded != null;
	}

	// Token: 0x060001DD RID: 477 RVA: 0x0000DF1C File Offset: 0x0000C11C
	private void UpdateAttackBowDraw(ItemDrop.ItemData weapon, float dt)
	{
		if (this.m_blocking || this.InMinorAction() || this.IsAttached())
		{
			this.m_attackDrawTime = -1f;
			if (!string.IsNullOrEmpty(weapon.m_shared.m_attack.m_drawAnimationState))
			{
				this.m_zanim.SetBool(weapon.m_shared.m_attack.m_drawAnimationState, false);
			}
			return;
		}
		float num = weapon.GetDrawStaminaDrain();
		if ((double)base.GetAttackDrawPercentage() >= 1.0)
		{
			num *= 0.5f;
		}
		bool flag = num <= 0f || this.HaveStamina(0f);
		if (this.m_attackDrawTime < 0f)
		{
			if (!this.m_attackHold)
			{
				this.m_attackDrawTime = 0f;
				return;
			}
		}
		else
		{
			if (this.m_attackHold && flag && this.m_attackDrawTime >= 0f)
			{
				if (this.m_attackDrawTime == 0f)
				{
					if (!weapon.m_shared.m_attack.StartDraw(this, weapon))
					{
						this.m_attackDrawTime = -1f;
						return;
					}
					weapon.m_shared.m_holdStartEffect.Create(base.transform.position, Quaternion.identity, base.transform, 1f, -1);
				}
				this.m_attackDrawTime += Time.fixedDeltaTime;
				if (!string.IsNullOrEmpty(weapon.m_shared.m_attack.m_drawAnimationState))
				{
					this.m_zanim.SetBool(weapon.m_shared.m_attack.m_drawAnimationState, true);
				}
				this.UseStamina(num * dt);
				return;
			}
			if (this.m_attackDrawTime > 0f)
			{
				if (flag)
				{
					this.StartAttack(null, false);
				}
				if (!string.IsNullOrEmpty(weapon.m_shared.m_attack.m_drawAnimationState))
				{
					this.m_zanim.SetBool(weapon.m_shared.m_attack.m_drawAnimationState, false);
				}
				this.m_attackDrawTime = 0f;
			}
		}
	}

	// Token: 0x060001DE RID: 478 RVA: 0x0000E0F9 File Offset: 0x0000C2F9
	protected override bool HaveQueuedChain()
	{
		return (this.m_queuedAttackTimer > 0f || this.m_attackHold) && base.GetCurrentWeapon() != null && this.m_currentAttack != null && this.m_currentAttack.CanStartChainAttack();
	}

	// Token: 0x060001DF RID: 479 RVA: 0x0000E130 File Offset: 0x0000C330
	private void UpdateBaseValue(float dt)
	{
		this.m_baseValueUpdateTimer += dt;
		if (this.m_baseValueUpdateTimer > 2f)
		{
			this.m_baseValueUpdateTimer = 0f;
			this.m_baseValue = EffectArea.GetBaseValue(base.transform.position, 20f);
			this.m_nview.GetZDO().Set(ZDOVars.s_baseValue, this.m_baseValue, false);
			this.m_comfortLevel = SE_Rested.CalculateComfortLevel(this);
		}
	}

	// Token: 0x060001E0 RID: 480 RVA: 0x0000E1A6 File Offset: 0x0000C3A6
	public int GetComfortLevel()
	{
		if (this.m_nview == null)
		{
			return 0;
		}
		return this.m_comfortLevel;
	}

	// Token: 0x060001E1 RID: 481 RVA: 0x0000E1BE File Offset: 0x0000C3BE
	public int GetBaseValue()
	{
		if (!this.m_nview.IsValid())
		{
			return 0;
		}
		if (this.m_nview.IsOwner())
		{
			return this.m_baseValue;
		}
		return this.m_nview.GetZDO().GetInt(ZDOVars.s_baseValue, 0);
	}

	// Token: 0x060001E2 RID: 482 RVA: 0x0000E1F9 File Offset: 0x0000C3F9
	public bool IsSafeInHome()
	{
		return this.m_safeInHome;
	}

	// Token: 0x060001E3 RID: 483 RVA: 0x0000E204 File Offset: 0x0000C404
	private void UpdateBiome(float dt)
	{
		if (this.InIntro())
		{
			return;
		}
		this.m_biomeTimer += dt;
		if (this.m_biomeTimer > 1f)
		{
			this.m_biomeTimer = 0f;
			Heightmap.Biome biome = Heightmap.FindBiome(base.transform.position);
			if (this.m_currentBiome != biome)
			{
				this.m_currentBiome = biome;
				this.AddKnownBiome(biome);
			}
		}
	}

	// Token: 0x060001E4 RID: 484 RVA: 0x0000E268 File Offset: 0x0000C468
	public Heightmap.Biome GetCurrentBiome()
	{
		return this.m_currentBiome;
	}

	// Token: 0x060001E5 RID: 485 RVA: 0x0000E270 File Offset: 0x0000C470
	public override void RaiseSkill(Skills.SkillType skill, float value = 1f)
	{
		if (skill == Skills.SkillType.None)
		{
			return;
		}
		float num = 1f;
		this.m_seman.ModifyRaiseSkill(skill, ref num);
		value *= num;
		this.m_skills.RaiseSkill(skill, value);
	}

	// Token: 0x060001E6 RID: 486 RVA: 0x0000E2A8 File Offset: 0x0000C4A8
	private void UpdateStats(float dt)
	{
		if (this.InIntro() || this.IsTeleporting())
		{
			return;
		}
		this.m_timeSinceDeath += dt;
		this.UpdateMovementModifier();
		this.UpdateFood(dt, false);
		bool flag = this.IsEncumbered();
		float maxStamina = this.GetMaxStamina();
		float num = 1f;
		if (this.IsBlocking())
		{
			num *= 0.8f;
		}
		if ((base.IsSwimming() && !base.IsOnGround()) || this.InAttack() || this.InDodge() || this.m_wallRunning || flag)
		{
			num = 0f;
		}
		float num2 = (this.m_staminaRegen + (1f - this.m_stamina / maxStamina) * this.m_staminaRegen * this.m_staminaRegenTimeMultiplier) * num;
		float num3 = 1f;
		this.m_seman.ModifyStaminaRegen(ref num3);
		num2 *= num3;
		this.m_staminaRegenTimer -= dt;
		if (this.m_stamina < maxStamina && this.m_staminaRegenTimer <= 0f)
		{
			this.m_stamina = Mathf.Min(maxStamina, this.m_stamina + num2 * dt);
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_stamina, this.m_stamina);
		float maxEitr = this.GetMaxEitr();
		float num4 = 1f;
		if (this.IsBlocking())
		{
			num4 *= 0.8f;
		}
		if (this.InAttack() || this.InDodge())
		{
			num4 = 0f;
		}
		float num5 = (this.m_eiterRegen + (1f - this.m_eitr / maxEitr) * this.m_eiterRegen) * num4;
		float num6 = 1f;
		this.m_seman.ModifyEitrRegen(ref num6);
		num6 += this.GetEquipmentEitrRegenModifier();
		num5 *= num6;
		this.m_eitrRegenTimer -= dt;
		if (this.m_eitr < maxEitr && this.m_eitrRegenTimer <= 0f)
		{
			this.m_eitr = Mathf.Min(maxEitr, this.m_eitr + num5 * dt);
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_eitr, this.m_eitr);
		if (flag)
		{
			if (this.m_moveDir.magnitude > 0.1f)
			{
				this.UseStamina(this.m_encumberedStaminaDrain * dt);
			}
			this.m_seman.AddStatusEffect(Player.s_statusEffectEncumbered, false, 0, 0f);
			this.ShowTutorial("encumbered", false);
		}
		else
		{
			this.m_seman.RemoveStatusEffect(Player.s_statusEffectEncumbered, false);
		}
		if (!this.HardDeath())
		{
			this.m_seman.AddStatusEffect(Player.s_statusEffectSoftDeath, false, 0, 0f);
		}
		else
		{
			this.m_seman.RemoveStatusEffect(Player.s_statusEffectSoftDeath, false);
		}
		this.UpdateEnvStatusEffects(dt);
	}

	// Token: 0x060001E7 RID: 487 RVA: 0x0000E53C File Offset: 0x0000C73C
	public float GetEquipmentEitrRegenModifier()
	{
		float num = 0f;
		if (this.m_chestItem != null)
		{
			num += this.m_chestItem.m_shared.m_eitrRegenModifier;
		}
		if (this.m_legItem != null)
		{
			num += this.m_legItem.m_shared.m_eitrRegenModifier;
		}
		if (this.m_helmetItem != null)
		{
			num += this.m_helmetItem.m_shared.m_eitrRegenModifier;
		}
		if (this.m_shoulderItem != null)
		{
			num += this.m_shoulderItem.m_shared.m_eitrRegenModifier;
		}
		if (this.m_leftItem != null)
		{
			num += this.m_leftItem.m_shared.m_eitrRegenModifier;
		}
		if (this.m_rightItem != null)
		{
			num += this.m_rightItem.m_shared.m_eitrRegenModifier;
		}
		if (this.m_utilityItem != null)
		{
			num += this.m_utilityItem.m_shared.m_eitrRegenModifier;
		}
		return num;
	}

	// Token: 0x060001E8 RID: 488 RVA: 0x0000E610 File Offset: 0x0000C810
	private void UpdateEnvStatusEffects(float dt)
	{
		this.m_nearFireTimer += dt;
		HitData.DamageModifiers damageModifiers = base.GetDamageModifiers(null);
		bool flag = this.m_nearFireTimer < 0.25f;
		bool flag2 = this.m_seman.HaveStatusEffect("Burning");
		bool flag3 = this.InShelter();
		HitData.DamageModifier modifier = damageModifiers.GetModifier(HitData.DamageType.Frost);
		bool flag4 = EnvMan.instance.IsFreezing();
		bool flag5 = EnvMan.instance.IsCold();
		bool flag6 = EnvMan.instance.IsWet();
		bool flag7 = this.IsSensed();
		bool flag8 = this.m_seman.HaveStatusEffect("Wet");
		bool flag9 = this.IsSitting();
		bool flag10 = EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.WarmCozyArea, 1f);
		bool flag11 = flag4 && !flag && !flag3;
		bool flag12 = (flag5 && !flag) || (flag4 && flag && !flag3) || (flag4 && !flag && flag3);
		if (modifier == HitData.DamageModifier.Resistant || modifier == HitData.DamageModifier.VeryResistant || flag10)
		{
			flag11 = false;
			flag12 = false;
		}
		if (flag6 && !this.m_underRoof)
		{
			this.m_seman.AddStatusEffect(Player.s_statusEffectWet, true, 0, 0f);
		}
		if (flag3)
		{
			this.m_seman.AddStatusEffect(Player.s_statusEffectShelter, false, 0, 0f);
		}
		else
		{
			this.m_seman.RemoveStatusEffect(Player.s_statusEffectShelter, false);
		}
		if (flag)
		{
			this.m_seman.AddStatusEffect(Player.s_statusEffectCampFire, false, 0, 0f);
		}
		else
		{
			this.m_seman.RemoveStatusEffect(Player.s_statusEffectCampFire, false);
		}
		bool flag13 = !flag7 && (flag9 || flag3) && !flag12 && !flag11 && (!flag8 || flag10) && !flag2 && flag;
		if (flag13)
		{
			this.m_seman.AddStatusEffect(Player.s_statusEffectResting, false, 0, 0f);
		}
		else
		{
			this.m_seman.RemoveStatusEffect(Player.s_statusEffectResting, false);
		}
		this.m_safeInHome = (flag13 && flag3 && (float)this.GetBaseValue() >= 1f);
		if (flag11)
		{
			if (!this.m_seman.RemoveStatusEffect(Player.s_statusEffectCold, true))
			{
				this.m_seman.AddStatusEffect(Player.s_statusEffectFreezing, false, 0, 0f);
				return;
			}
		}
		else if (flag12)
		{
			if (!this.m_seman.RemoveStatusEffect(Player.s_statusEffectFreezing, true) && this.m_seman.AddStatusEffect(Player.s_statusEffectCold, false, 0, 0f))
			{
				this.ShowTutorial("cold", false);
				return;
			}
		}
		else
		{
			this.m_seman.RemoveStatusEffect(Player.s_statusEffectCold, false);
			this.m_seman.RemoveStatusEffect(Player.s_statusEffectFreezing, false);
		}
	}

	// Token: 0x060001E9 RID: 489 RVA: 0x0000E89C File Offset: 0x0000CA9C
	private bool CanEat(ItemDrop.ItemData item, bool showMessages)
	{
		foreach (Player.Food food in this.m_foods)
		{
			if (food.m_item.m_shared.m_name == item.m_shared.m_name)
			{
				if (food.CanEatAgain())
				{
					return true;
				}
				this.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_nomore", new string[]
				{
					item.m_shared.m_name
				}), 0, null);
				return false;
			}
		}
		using (List<Player.Food>.Enumerator enumerator = this.m_foods.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.CanEatAgain())
				{
					return true;
				}
			}
		}
		if (this.m_foods.Count >= 3)
		{
			this.Message(MessageHud.MessageType.Center, "$msg_isfull", 0, null);
			return false;
		}
		return true;
	}

	// Token: 0x060001EA RID: 490 RVA: 0x0000E9B0 File Offset: 0x0000CBB0
	private Player.Food GetMostDepletedFood()
	{
		Player.Food food = null;
		foreach (Player.Food food2 in this.m_foods)
		{
			if (food2.CanEatAgain() && (food == null || food2.m_time < food.m_time))
			{
				food = food2;
			}
		}
		return food;
	}

	// Token: 0x060001EB RID: 491 RVA: 0x0000EA1C File Offset: 0x0000CC1C
	public void ClearFood()
	{
		this.m_foods.Clear();
	}

	// Token: 0x060001EC RID: 492 RVA: 0x0000EA29 File Offset: 0x0000CC29
	public bool RemoveOneFood()
	{
		if (this.m_foods.Count == 0)
		{
			return false;
		}
		this.m_foods.RemoveAt(UnityEngine.Random.Range(0, this.m_foods.Count));
		return true;
	}

	// Token: 0x060001ED RID: 493 RVA: 0x0000EA58 File Offset: 0x0000CC58
	private bool EatFood(ItemDrop.ItemData item)
	{
		if (!this.CanEat(item, false))
		{
			return false;
		}
		string text = "";
		if (item.m_shared.m_food > 0f)
		{
			text = text + " +" + item.m_shared.m_food.ToString() + " $item_food_health ";
		}
		if (item.m_shared.m_foodStamina > 0f)
		{
			text = text + " +" + item.m_shared.m_foodStamina.ToString() + " $item_food_stamina ";
		}
		if (item.m_shared.m_foodEitr > 0f)
		{
			text = text + " +" + item.m_shared.m_foodEitr.ToString() + " $item_food_eitr ";
		}
		this.Message(MessageHud.MessageType.Center, text, 0, null);
		foreach (Player.Food food in this.m_foods)
		{
			if (food.m_item.m_shared.m_name == item.m_shared.m_name)
			{
				if (food.CanEatAgain())
				{
					food.m_time = item.m_shared.m_foodBurnTime;
					food.m_health = item.m_shared.m_food;
					food.m_stamina = item.m_shared.m_foodStamina;
					food.m_eitr = item.m_shared.m_foodEitr;
					this.UpdateFood(0f, true);
					return true;
				}
				return false;
			}
		}
		if (this.m_foods.Count < 3)
		{
			Player.Food food2 = new Player.Food();
			food2.m_name = item.m_dropPrefab.name;
			food2.m_item = item;
			food2.m_time = item.m_shared.m_foodBurnTime;
			food2.m_health = item.m_shared.m_food;
			food2.m_stamina = item.m_shared.m_foodStamina;
			food2.m_eitr = item.m_shared.m_foodEitr;
			this.m_foods.Add(food2);
			this.UpdateFood(0f, true);
			return true;
		}
		Player.Food mostDepletedFood = this.GetMostDepletedFood();
		if (mostDepletedFood != null)
		{
			mostDepletedFood.m_name = item.m_dropPrefab.name;
			mostDepletedFood.m_item = item;
			mostDepletedFood.m_time = item.m_shared.m_foodBurnTime;
			mostDepletedFood.m_health = item.m_shared.m_food;
			mostDepletedFood.m_stamina = item.m_shared.m_foodStamina;
			this.UpdateFood(0f, true);
			return true;
		}
		return false;
	}

	// Token: 0x060001EE RID: 494 RVA: 0x0000ECE8 File Offset: 0x0000CEE8
	private void UpdateFood(float dt, bool forceUpdate)
	{
		this.m_foodUpdateTimer += dt;
		if (this.m_foodUpdateTimer >= 1f || forceUpdate)
		{
			this.m_foodUpdateTimer -= 1f;
			foreach (Player.Food food in this.m_foods)
			{
				food.m_time -= 1f;
				float num = Mathf.Clamp01(food.m_time / food.m_item.m_shared.m_foodBurnTime);
				num = Mathf.Pow(num, 0.3f);
				food.m_health = food.m_item.m_shared.m_food * num;
				food.m_stamina = food.m_item.m_shared.m_foodStamina * num;
				food.m_eitr = food.m_item.m_shared.m_foodEitr * num;
				if (food.m_time <= 0f)
				{
					this.Message(MessageHud.MessageType.Center, "$msg_food_done", 0, null);
					this.m_foods.Remove(food);
					break;
				}
			}
			float health;
			float stamina;
			float num2;
			this.GetTotalFoodValue(out health, out stamina, out num2);
			this.SetMaxHealth(health, true);
			this.SetMaxStamina(stamina, true);
			this.SetMaxEitr(num2, true);
			if (num2 > 0f)
			{
				this.ShowTutorial("eitr", false);
			}
		}
		if (!forceUpdate)
		{
			this.m_foodRegenTimer += dt;
			if (this.m_foodRegenTimer >= 10f)
			{
				this.m_foodRegenTimer = 0f;
				float num3 = 0f;
				foreach (Player.Food food2 in this.m_foods)
				{
					num3 += food2.m_item.m_shared.m_foodRegen;
				}
				if (num3 > 0f)
				{
					float num4 = 1f;
					this.m_seman.ModifyHealthRegen(ref num4);
					num3 *= num4;
					base.Heal(num3, true);
				}
			}
		}
	}

	// Token: 0x060001EF RID: 495 RVA: 0x0000EF20 File Offset: 0x0000D120
	private void GetTotalFoodValue(out float hp, out float stamina, out float eitr)
	{
		hp = this.m_baseHP;
		stamina = this.m_baseStamina;
		eitr = 0f;
		foreach (Player.Food food in this.m_foods)
		{
			hp += food.m_health;
			stamina += food.m_stamina;
			eitr += food.m_eitr;
		}
	}

	// Token: 0x060001F0 RID: 496 RVA: 0x0000EFA4 File Offset: 0x0000D1A4
	public float GetBaseFoodHP()
	{
		return this.m_baseHP;
	}

	// Token: 0x060001F1 RID: 497 RVA: 0x0000EFAC File Offset: 0x0000D1AC
	public List<Player.Food> GetFoods()
	{
		return this.m_foods;
	}

	// Token: 0x060001F2 RID: 498 RVA: 0x0000EFB4 File Offset: 0x0000D1B4
	public void OnSpawned()
	{
		this.m_spawnEffects.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		if (this.m_firstSpawn)
		{
			if (this.m_valkyrie != null)
			{
				UnityEngine.Object.Instantiate<GameObject>(this.m_valkyrie, base.transform.position, Quaternion.identity);
			}
			this.m_firstSpawn = false;
		}
	}

	// Token: 0x060001F3 RID: 499 RVA: 0x0000F020 File Offset: 0x0000D220
	protected override bool CheckRun(Vector3 moveDir, float dt)
	{
		if (!base.CheckRun(moveDir, dt))
		{
			return false;
		}
		bool flag = this.HaveStamina(0f);
		float skillFactor = this.m_skills.GetSkillFactor(Skills.SkillType.Run);
		float num = Mathf.Lerp(1f, 0.5f, skillFactor);
		float num2 = this.m_runStaminaDrain * num;
		this.m_seman.ModifyRunStaminaDrain(num2, ref num2);
		this.UseStamina(dt * num2);
		if (this.HaveStamina(0f))
		{
			this.m_runSkillImproveTimer += dt;
			if (this.m_runSkillImproveTimer > 1f)
			{
				this.m_runSkillImproveTimer = 0f;
				this.RaiseSkill(Skills.SkillType.Run, 1f);
			}
			this.ClearActionQueue();
			return true;
		}
		if (flag)
		{
			Hud.instance.StaminaBarEmptyFlash();
		}
		return false;
	}

	// Token: 0x060001F4 RID: 500 RVA: 0x0000F0DC File Offset: 0x0000D2DC
	private void UpdateMovementModifier()
	{
		this.m_equipmentMovementModifier = 0f;
		if (this.m_rightItem != null)
		{
			this.m_equipmentMovementModifier += this.m_rightItem.m_shared.m_movementModifier;
		}
		if (this.m_leftItem != null)
		{
			this.m_equipmentMovementModifier += this.m_leftItem.m_shared.m_movementModifier;
		}
		if (this.m_chestItem != null)
		{
			this.m_equipmentMovementModifier += this.m_chestItem.m_shared.m_movementModifier;
		}
		if (this.m_legItem != null)
		{
			this.m_equipmentMovementModifier += this.m_legItem.m_shared.m_movementModifier;
		}
		if (this.m_helmetItem != null)
		{
			this.m_equipmentMovementModifier += this.m_helmetItem.m_shared.m_movementModifier;
		}
		if (this.m_shoulderItem != null)
		{
			this.m_equipmentMovementModifier += this.m_shoulderItem.m_shared.m_movementModifier;
		}
		if (this.m_utilityItem != null)
		{
			this.m_equipmentMovementModifier += this.m_utilityItem.m_shared.m_movementModifier;
		}
	}

	// Token: 0x060001F5 RID: 501 RVA: 0x0000F1F7 File Offset: 0x0000D3F7
	public void OnSkillLevelup(Skills.SkillType skill, float level)
	{
		this.m_skillLevelupEffects.Create(this.m_head.position, this.m_head.rotation, this.m_head, 1f, -1);
	}

	// Token: 0x060001F6 RID: 502 RVA: 0x0000F228 File Offset: 0x0000D428
	protected override void OnJump()
	{
		this.ClearActionQueue();
		float num = this.m_jumpStaminaUsage - this.m_jumpStaminaUsage * this.m_equipmentMovementModifier;
		this.m_seman.ModifyJumpStaminaUsage(num, ref num);
		this.UseStamina(num);
	}

	// Token: 0x060001F7 RID: 503 RVA: 0x0000F268 File Offset: 0x0000D468
	protected override void OnSwimming(Vector3 targetVel, float dt)
	{
		base.OnSwimming(targetVel, dt);
		if (targetVel.magnitude > 0.1f)
		{
			float skillFactor = this.m_skills.GetSkillFactor(Skills.SkillType.Swim);
			float num = Mathf.Lerp(this.m_swimStaminaDrainMinSkill, this.m_swimStaminaDrainMaxSkill, skillFactor);
			this.UseStamina(dt * num);
			this.m_swimSkillImproveTimer += dt;
			if (this.m_swimSkillImproveTimer > 1f)
			{
				this.m_swimSkillImproveTimer = 0f;
				this.RaiseSkill(Skills.SkillType.Swim, 1f);
			}
		}
		if (!this.HaveStamina(0f))
		{
			this.m_drownDamageTimer += dt;
			if (this.m_drownDamageTimer > 1f)
			{
				this.m_drownDamageTimer = 0f;
				float damage = Mathf.Ceil(base.GetMaxHealth() / 20f);
				HitData hitData = new HitData();
				hitData.m_damage.m_damage = damage;
				hitData.m_point = base.GetCenterPoint();
				hitData.m_dir = Vector3.down;
				hitData.m_pushForce = 10f;
				base.Damage(hitData);
				Vector3 position = base.transform.position;
				position.y = base.GetLiquidLevel();
				this.m_drownEffects.Create(position, base.transform.rotation, null, 1f, -1);
			}
		}
	}

	// Token: 0x060001F8 RID: 504 RVA: 0x0000F3A8 File Offset: 0x0000D5A8
	protected override bool TakeInput()
	{
		bool result = (!Chat.instance || !Chat.instance.HasFocus()) && !global::Console.IsVisible() && !TextInput.IsVisible() && (!StoreGui.IsVisible() && !InventoryGui.IsVisible() && !Menu.IsVisible() && (!TextViewer.instance || !TextViewer.instance.IsVisible()) && !Minimap.IsOpen()) && !GameCamera.InFreeFly();
		if (this.IsDead() || this.InCutscene() || this.IsTeleporting())
		{
			result = false;
		}
		return result;
	}

	// Token: 0x060001F9 RID: 505 RVA: 0x0000F43C File Offset: 0x0000D63C
	public void UseHotbarItem(int index)
	{
		ItemDrop.ItemData itemAt = this.m_inventory.GetItemAt(index - 1, 0);
		if (itemAt != null)
		{
			base.UseItem(null, itemAt, false);
		}
	}

	// Token: 0x060001FA RID: 506 RVA: 0x0000F468 File Offset: 0x0000D668
	public bool RequiredCraftingStation(Recipe recipe, int qualityLevel, bool checkLevel)
	{
		CraftingStation requiredStation = recipe.GetRequiredStation(qualityLevel);
		if (requiredStation != null)
		{
			if (this.m_currentStation == null)
			{
				return false;
			}
			if (requiredStation.m_name != this.m_currentStation.m_name)
			{
				return false;
			}
			if (checkLevel)
			{
				int requiredStationLevel = recipe.GetRequiredStationLevel(qualityLevel);
				if (this.m_currentStation.GetLevel() < requiredStationLevel)
				{
					return false;
				}
			}
		}
		else if (this.m_currentStation != null && !this.m_currentStation.m_showBasicRecipies)
		{
			return false;
		}
		return true;
	}

	// Token: 0x060001FB RID: 507 RVA: 0x0000F4EC File Offset: 0x0000D6EC
	public bool HaveRequirements(Recipe recipe, bool discover, int qualityLevel)
	{
		if (discover)
		{
			if (recipe.m_craftingStation && !this.KnowStationLevel(recipe.m_craftingStation.m_name, recipe.m_minStationLevel))
			{
				return false;
			}
		}
		else if (!this.RequiredCraftingStation(recipe, qualityLevel, true))
		{
			return false;
		}
		return (recipe.m_item.m_itemData.m_shared.m_dlc.Length <= 0 || DLCMan.instance.IsDLCInstalled(recipe.m_item.m_itemData.m_shared.m_dlc)) && this.HaveRequirementItems(recipe, discover, qualityLevel);
	}

	// Token: 0x060001FC RID: 508 RVA: 0x0000F580 File Offset: 0x0000D780
	private bool HaveRequirementItems(Recipe piece, bool discover, int qualityLevel)
	{
		foreach (Piece.Requirement requirement in piece.m_resources)
		{
			if (requirement.m_resItem)
			{
				if (discover)
				{
					if (requirement.m_amount > 0)
					{
						if (piece.m_requireOnlyOneIngredient)
						{
							if (this.m_knownMaterial.Contains(requirement.m_resItem.m_itemData.m_shared.m_name))
							{
								return true;
							}
						}
						else if (!this.m_knownMaterial.Contains(requirement.m_resItem.m_itemData.m_shared.m_name))
						{
							return false;
						}
					}
				}
				else
				{
					int amount = requirement.GetAmount(qualityLevel);
					int num = this.m_inventory.CountItems(requirement.m_resItem.m_itemData.m_shared.m_name, -1);
					if (piece.m_requireOnlyOneIngredient)
					{
						if (num >= amount)
						{
							return true;
						}
					}
					else if (num < amount)
					{
						return false;
					}
				}
			}
		}
		return !piece.m_requireOnlyOneIngredient;
	}

	// Token: 0x060001FD RID: 509 RVA: 0x0000F668 File Offset: 0x0000D868
	public ItemDrop.ItemData GetFirstRequiredItem(Inventory inventory, Recipe recipe, int qualityLevel, out int amount, out int extraAmount)
	{
		foreach (Piece.Requirement requirement in recipe.m_resources)
		{
			if (requirement.m_resItem)
			{
				int amount2 = requirement.GetAmount(qualityLevel);
				for (int j = 0; j <= requirement.m_resItem.m_itemData.m_shared.m_maxQuality; j++)
				{
					if (this.m_inventory.CountItems(requirement.m_resItem.m_itemData.m_shared.m_name, j) >= amount2)
					{
						amount = amount2;
						extraAmount = requirement.m_extraAmountOnlyOneIngredient;
						return inventory.GetItem(requirement.m_resItem.m_itemData.m_shared.m_name, j, false);
					}
				}
			}
		}
		amount = 0;
		extraAmount = 0;
		return null;
	}

	// Token: 0x060001FE RID: 510 RVA: 0x0000F728 File Offset: 0x0000D928
	public bool HaveRequirements(Piece piece, Player.RequirementMode mode)
	{
		if (piece.m_craftingStation)
		{
			if (mode == Player.RequirementMode.IsKnown || mode == Player.RequirementMode.CanAlmostBuild)
			{
				if (!this.m_knownStations.ContainsKey(piece.m_craftingStation.m_name))
				{
					return false;
				}
			}
			else if (!CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, base.transform.position))
			{
				return false;
			}
		}
		if (piece.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(piece.m_dlc))
		{
			return false;
		}
		foreach (Piece.Requirement requirement in piece.m_resources)
		{
			if (requirement.m_resItem && requirement.m_amount > 0)
			{
				if (mode == Player.RequirementMode.IsKnown)
				{
					if (!this.m_knownMaterial.Contains(requirement.m_resItem.m_itemData.m_shared.m_name))
					{
						return false;
					}
				}
				else if (mode == Player.RequirementMode.CanAlmostBuild)
				{
					if (!this.m_inventory.HaveItem(requirement.m_resItem.m_itemData.m_shared.m_name))
					{
						return false;
					}
				}
				else if (mode == Player.RequirementMode.CanBuild && this.m_inventory.CountItems(requirement.m_resItem.m_itemData.m_shared.m_name, -1) < requirement.m_amount)
				{
					return false;
				}
			}
		}
		return true;
	}

	// Token: 0x060001FF RID: 511 RVA: 0x0000F864 File Offset: 0x0000DA64
	public void ConsumeResources(Piece.Requirement[] requirements, int qualityLevel, int itemQuality = -1)
	{
		foreach (Piece.Requirement requirement in requirements)
		{
			if (requirement.m_resItem)
			{
				int amount = requirement.GetAmount(qualityLevel);
				if (amount > 0)
				{
					this.m_inventory.RemoveItem(requirement.m_resItem.m_itemData.m_shared.m_name, amount, itemQuality);
				}
			}
		}
	}

	// Token: 0x06000200 RID: 512 RVA: 0x0000F8C0 File Offset: 0x0000DAC0
	private void UpdateHover()
	{
		if (this.InPlaceMode() || this.IsDead() || this.m_doodadController != null)
		{
			this.m_hovering = null;
			this.m_hoveringCreature = null;
			return;
		}
		this.FindHoverObject(out this.m_hovering, out this.m_hoveringCreature);
	}

	// Token: 0x06000201 RID: 513 RVA: 0x0000F8FC File Offset: 0x0000DAFC
	private bool CheckCanRemovePiece(Piece piece)
	{
		if (!this.m_noPlacementCost && piece.m_craftingStation != null && !CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, base.transform.position))
		{
			this.Message(MessageHud.MessageType.Center, "$msg_missingstation", 0, null);
			return false;
		}
		return true;
	}

	// Token: 0x06000202 RID: 514 RVA: 0x0000F954 File Offset: 0x0000DB54
	private bool RemovePiece()
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, out raycastHit, 50f, this.m_removeRayMask) && Vector3.Distance(raycastHit.point, this.m_eye.position) < this.m_maxPlaceDistance)
		{
			Piece piece = raycastHit.collider.GetComponentInParent<Piece>();
			if (piece == null && raycastHit.collider.GetComponent<Heightmap>())
			{
				piece = TerrainModifier.FindClosestModifierPieceInRange(raycastHit.point, 2.5f);
			}
			if (piece)
			{
				if (!piece.m_canBeRemoved)
				{
					return false;
				}
				if (Location.IsInsideNoBuildLocation(piece.transform.position))
				{
					this.Message(MessageHud.MessageType.Center, "$msg_nobuildzone", 0, null);
					return false;
				}
				if (!PrivateArea.CheckAccess(piece.transform.position, 0f, true, false))
				{
					this.Message(MessageHud.MessageType.Center, "$msg_privatezone", 0, null);
					return false;
				}
				if (!this.CheckCanRemovePiece(piece))
				{
					return false;
				}
				ZNetView component = piece.GetComponent<ZNetView>();
				if (component == null)
				{
					return false;
				}
				if (!piece.CanBeRemoved())
				{
					this.Message(MessageHud.MessageType.Center, "$msg_cantremovenow", 0, null);
					return false;
				}
				WearNTear component2 = piece.GetComponent<WearNTear>();
				if (component2)
				{
					component2.Remove();
				}
				else
				{
					ZLog.Log("Removing non WNT object with hammer " + piece.name);
					component.ClaimOwnership();
					piece.DropResources();
					piece.m_placeEffect.Create(piece.transform.position, piece.transform.rotation, piece.gameObject.transform, 1f, -1);
					this.m_removeEffects.Create(piece.transform.position, Quaternion.identity, null, 1f, -1);
					ZNetScene.instance.Destroy(piece.gameObject);
				}
				ItemDrop.ItemData rightItem = base.GetRightItem();
				if (rightItem != null)
				{
					this.FaceLookDirection();
					this.m_zanim.SetTrigger(rightItem.m_shared.m_attack.m_attackAnimation);
				}
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000203 RID: 515 RVA: 0x0000FB5A File Offset: 0x0000DD5A
	public void FaceLookDirection()
	{
		base.transform.rotation = base.GetLookYaw();
	}

	// Token: 0x06000204 RID: 516 RVA: 0x0000FB70 File Offset: 0x0000DD70
	private bool PlacePiece(Piece piece)
	{
		this.UpdatePlacementGhost(true);
		Vector3 position = this.m_placementGhost.transform.position;
		Quaternion rotation = this.m_placementGhost.transform.rotation;
		GameObject gameObject = piece.gameObject;
		switch (this.m_placementStatus)
		{
		case Player.PlacementStatus.Invalid:
			this.Message(MessageHud.MessageType.Center, "$msg_invalidplacement", 0, null);
			return false;
		case Player.PlacementStatus.BlockedbyPlayer:
			this.Message(MessageHud.MessageType.Center, "$msg_blocked", 0, null);
			return false;
		case Player.PlacementStatus.NoBuildZone:
			this.Message(MessageHud.MessageType.Center, "$msg_nobuildzone", 0, null);
			return false;
		case Player.PlacementStatus.PrivateZone:
			this.Message(MessageHud.MessageType.Center, "$msg_privatezone", 0, null);
			return false;
		case Player.PlacementStatus.MoreSpace:
			this.Message(MessageHud.MessageType.Center, "$msg_needspace", 0, null);
			return false;
		case Player.PlacementStatus.NoTeleportArea:
			this.Message(MessageHud.MessageType.Center, "$msg_noteleportarea", 0, null);
			return false;
		case Player.PlacementStatus.ExtensionMissingStation:
			this.Message(MessageHud.MessageType.Center, "$msg_extensionmissingstation", 0, null);
			return false;
		case Player.PlacementStatus.WrongBiome:
			this.Message(MessageHud.MessageType.Center, "$msg_wrongbiome", 0, null);
			return false;
		case Player.PlacementStatus.NeedCultivated:
			this.Message(MessageHud.MessageType.Center, "$msg_needcultivated", 0, null);
			return false;
		case Player.PlacementStatus.NeedDirt:
			this.Message(MessageHud.MessageType.Center, "$msg_needdirt", 0, null);
			return false;
		case Player.PlacementStatus.NotInDungeon:
			this.Message(MessageHud.MessageType.Center, "$msg_notindungeon", 0, null);
			return false;
		default:
		{
			TerrainModifier.SetTriggerOnPlaced(true);
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, position, rotation);
			TerrainModifier.SetTriggerOnPlaced(false);
			CraftingStation componentInChildren = gameObject2.GetComponentInChildren<CraftingStation>();
			if (componentInChildren)
			{
				this.AddKnownStation(componentInChildren);
			}
			Piece component = gameObject2.GetComponent<Piece>();
			if (component)
			{
				component.SetCreator(this.GetPlayerID());
			}
			PrivateArea component2 = gameObject2.GetComponent<PrivateArea>();
			if (component2)
			{
				component2.Setup(Game.instance.GetPlayerProfile().GetName());
			}
			WearNTear component3 = gameObject2.GetComponent<WearNTear>();
			if (component3)
			{
				component3.OnPlaced();
			}
			ItemDrop.ItemData rightItem = base.GetRightItem();
			if (rightItem != null)
			{
				this.FaceLookDirection();
				this.m_zanim.SetTrigger(rightItem.m_shared.m_attack.m_attackAnimation);
			}
			piece.m_placeEffect.Create(position, rotation, gameObject2.transform, 1f, -1);
			base.AddNoise(50f);
			Game.instance.GetPlayerProfile().m_playerStats.m_builds++;
			ZLog.Log("Placed " + gameObject.name);
			Gogan.LogEvent("Game", "PlacedPiece", gameObject.name, 0L);
			return true;
		}
		}
	}

	// Token: 0x06000205 RID: 517 RVA: 0x0000290F File Offset: 0x00000B0F
	public override bool IsPlayer()
	{
		return true;
	}

	// Token: 0x06000206 RID: 518 RVA: 0x0000FDC8 File Offset: 0x0000DFC8
	public void GetBuildSelection(out Piece go, out Vector2Int id, out int total, out Piece.PieceCategory category, out bool useCategory)
	{
		category = this.m_buildPieces.m_selectedCategory;
		useCategory = this.m_buildPieces.m_useCategories;
		if (this.m_buildPieces.GetAvailablePiecesInSelectedCategory() == 0)
		{
			go = null;
			id = Vector2Int.zero;
			total = 0;
			return;
		}
		GameObject selectedPrefab = this.m_buildPieces.GetSelectedPrefab();
		go = (selectedPrefab ? selectedPrefab.GetComponent<Piece>() : null);
		id = this.m_buildPieces.GetSelectedIndex();
		total = this.m_buildPieces.GetAvailablePiecesInSelectedCategory();
	}

	// Token: 0x06000207 RID: 519 RVA: 0x0000FE4D File Offset: 0x0000E04D
	public List<Piece> GetBuildPieces()
	{
		if (!(this.m_buildPieces != null))
		{
			return null;
		}
		return this.m_buildPieces.GetPiecesInSelectedCategory();
	}

	// Token: 0x06000208 RID: 520 RVA: 0x0000FE6A File Offset: 0x0000E06A
	public int GetAvailableBuildPiecesInCategory(Piece.PieceCategory cat)
	{
		if (!(this.m_buildPieces != null))
		{
			return 0;
		}
		return this.m_buildPieces.GetAvailablePiecesInCategory(cat);
	}

	// Token: 0x06000209 RID: 521 RVA: 0x0000FE88 File Offset: 0x0000E088
	private void RPC_OnDeath(long sender)
	{
		this.m_visual.SetActive(false);
	}

	// Token: 0x0600020A RID: 522 RVA: 0x0000FE98 File Offset: 0x0000E098
	private void CreateDeathEffects()
	{
		GameObject[] array = this.m_deathEffects.Create(base.transform.position, base.transform.rotation, base.transform, 1f, -1);
		for (int i = 0; i < array.Length; i++)
		{
			Ragdoll component = array[i].GetComponent<Ragdoll>();
			if (component)
			{
				Vector3 velocity = this.m_body.velocity;
				if (this.m_pushForce.magnitude * 0.5f > velocity.magnitude)
				{
					velocity = this.m_pushForce * 0.5f;
				}
				component.Setup(velocity, 0f, 0f, 0f, null);
				this.OnRagdollCreated(component);
				this.m_ragdoll = component;
			}
		}
	}

	// Token: 0x0600020B RID: 523 RVA: 0x0000FF50 File Offset: 0x0000E150
	public void UnequipDeathDropItems()
	{
		if (this.m_rightItem != null)
		{
			base.UnequipItem(this.m_rightItem, false);
		}
		if (this.m_leftItem != null)
		{
			base.UnequipItem(this.m_leftItem, false);
		}
		if (this.m_ammoItem != null)
		{
			base.UnequipItem(this.m_ammoItem, false);
		}
		if (this.m_utilityItem != null)
		{
			base.UnequipItem(this.m_utilityItem, false);
		}
	}

	// Token: 0x0600020C RID: 524 RVA: 0x0000FFB4 File Offset: 0x0000E1B4
	public void CreateTombStone()
	{
		if (this.m_inventory.NrOfItems() == 0)
		{
			return;
		}
		base.UnequipAllItems();
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_tombstone, base.GetCenterPoint(), base.transform.rotation);
		gameObject.GetComponent<Container>().GetInventory().MoveInventoryToGrave(this.m_inventory);
		TombStone component = gameObject.GetComponent<TombStone>();
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		component.Setup(playerProfile.GetName(), playerProfile.GetPlayerID());
	}

	// Token: 0x0600020D RID: 525 RVA: 0x00010028 File Offset: 0x0000E228
	private bool HardDeath()
	{
		return this.m_timeSinceDeath > this.m_hardDeathCooldown;
	}

	// Token: 0x0600020E RID: 526 RVA: 0x00010038 File Offset: 0x0000E238
	public void ClearHardDeath()
	{
		this.m_timeSinceDeath = this.m_hardDeathCooldown + 1f;
	}

	// Token: 0x0600020F RID: 527 RVA: 0x0001004C File Offset: 0x0000E24C
	protected override void OnDeath()
	{
		bool flag = this.HardDeath();
		this.m_nview.GetZDO().Set(ZDOVars.s_dead, true);
		this.m_nview.InvokeRPC(ZNetView.Everybody, "OnDeath", Array.Empty<object>());
		Game.instance.GetPlayerProfile().m_playerStats.m_deaths++;
		Game.instance.GetPlayerProfile().SetDeathPoint(base.transform.position);
		this.CreateDeathEffects();
		this.CreateTombStone();
		this.m_foods.Clear();
		if (flag)
		{
			this.m_skills.OnDeath();
		}
		this.m_seman.RemoveAllStatusEffects(false);
		Game.instance.RequestRespawn(10f);
		this.m_timeSinceDeath = 0f;
		if (!flag)
		{
			this.Message(MessageHud.MessageType.TopLeft, "$msg_softdeath", 0, null);
		}
		this.Message(MessageHud.MessageType.Center, "$msg_youdied", 0, null);
		this.ShowTutorial("death", false);
		Minimap.instance.AddPin(base.transform.position, Minimap.PinType.Death, string.Format("$hud_mapday {0}", EnvMan.instance.GetDay(ZNet.instance.GetTimeSeconds())), true, false, 0L);
		if (this.m_onDeath != null)
		{
			this.m_onDeath();
		}
		string eventLabel = "biome:" + this.GetCurrentBiome().ToString();
		Gogan.LogEvent("Game", "Death", eventLabel, 0L);
	}

	// Token: 0x06000210 RID: 528 RVA: 0x000101BC File Offset: 0x0000E3BC
	public void OnRespawn()
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_dead, false);
		base.SetHealth(base.GetMaxHealth());
	}

	// Token: 0x06000211 RID: 529 RVA: 0x000101E0 File Offset: 0x0000E3E0
	private void SetupPlacementGhost()
	{
		if (this.m_placementGhost)
		{
			UnityEngine.Object.Destroy(this.m_placementGhost);
			this.m_placementGhost = null;
		}
		if (this.m_buildPieces == null)
		{
			return;
		}
		GameObject selectedPrefab = this.m_buildPieces.GetSelectedPrefab();
		if (selectedPrefab == null)
		{
			return;
		}
		if (selectedPrefab.GetComponent<Piece>().m_repairPiece)
		{
			return;
		}
		bool enabled = false;
		TerrainModifier componentInChildren = selectedPrefab.GetComponentInChildren<TerrainModifier>();
		if (componentInChildren)
		{
			enabled = componentInChildren.enabled;
			componentInChildren.enabled = false;
		}
		TerrainOp.m_forceDisableTerrainOps = true;
		ZNetView.m_forceDisableInit = true;
		this.m_placementGhost = UnityEngine.Object.Instantiate<GameObject>(selectedPrefab);
		ZNetView.m_forceDisableInit = false;
		TerrainOp.m_forceDisableTerrainOps = false;
		this.m_placementGhost.name = selectedPrefab.name;
		if (componentInChildren)
		{
			componentInChildren.enabled = enabled;
		}
		Joint[] componentsInChildren = this.m_placementGhost.GetComponentsInChildren<Joint>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i]);
		}
		Rigidbody[] componentsInChildren2 = this.m_placementGhost.GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren2[i]);
		}
		ParticleSystemForceField[] componentsInChildren3 = this.m_placementGhost.GetComponentsInChildren<ParticleSystemForceField>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren3[i]);
		}
		Demister[] componentsInChildren4 = this.m_placementGhost.GetComponentsInChildren<Demister>();
		for (int i = 0; i < componentsInChildren4.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren4[i]);
		}
		foreach (Collider collider in this.m_placementGhost.GetComponentsInChildren<Collider>())
		{
			if ((1 << collider.gameObject.layer & this.m_placeRayMask) == 0)
			{
				ZLog.Log("Disabling " + collider.gameObject.name + "  " + LayerMask.LayerToName(collider.gameObject.layer));
				collider.enabled = false;
			}
		}
		Transform[] componentsInChildren6 = this.m_placementGhost.GetComponentsInChildren<Transform>();
		int layer = LayerMask.NameToLayer("ghost");
		Transform[] array = componentsInChildren6;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.layer = layer;
		}
		TerrainModifier[] componentsInChildren7 = this.m_placementGhost.GetComponentsInChildren<TerrainModifier>();
		for (int i = 0; i < componentsInChildren7.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren7[i]);
		}
		GuidePoint[] componentsInChildren8 = this.m_placementGhost.GetComponentsInChildren<GuidePoint>();
		for (int i = 0; i < componentsInChildren8.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren8[i]);
		}
		Light[] componentsInChildren9 = this.m_placementGhost.GetComponentsInChildren<Light>();
		for (int i = 0; i < componentsInChildren9.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren9[i]);
		}
		AudioSource[] componentsInChildren10 = this.m_placementGhost.GetComponentsInChildren<AudioSource>();
		for (int i = 0; i < componentsInChildren10.Length; i++)
		{
			componentsInChildren10[i].enabled = false;
		}
		ZSFX[] componentsInChildren11 = this.m_placementGhost.GetComponentsInChildren<ZSFX>();
		for (int i = 0; i < componentsInChildren11.Length; i++)
		{
			componentsInChildren11[i].enabled = false;
		}
		WispSpawner componentInChildren2 = this.m_placementGhost.GetComponentInChildren<WispSpawner>();
		if (componentInChildren2)
		{
			UnityEngine.Object.Destroy(componentInChildren2);
		}
		Windmill componentInChildren3 = this.m_placementGhost.GetComponentInChildren<Windmill>();
		if (componentInChildren3)
		{
			componentInChildren3.enabled = false;
		}
		ParticleSystem[] componentsInChildren12 = this.m_placementGhost.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren12.Length; i++)
		{
			componentsInChildren12[i].gameObject.SetActive(false);
		}
		Transform transform = this.m_placementGhost.transform.Find("_GhostOnly");
		if (transform)
		{
			transform.gameObject.SetActive(true);
		}
		this.m_placementGhost.transform.position = base.transform.position;
		this.m_placementGhost.transform.localScale = selectedPrefab.transform.localScale;
		this.CleanupGhostMaterials<MeshRenderer>(this.m_placementGhost);
		this.CleanupGhostMaterials<SkinnedMeshRenderer>(this.m_placementGhost);
	}

	// Token: 0x06000212 RID: 530 RVA: 0x000105BC File Offset: 0x0000E7BC
	private void CleanupGhostMaterials<T>(GameObject ghost) where T : Renderer
	{
		foreach (T t in this.m_placementGhost.GetComponentsInChildren<T>())
		{
			if (!(t.sharedMaterial == null))
			{
				Material[] sharedMaterials = t.sharedMaterials;
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					Material material = new Material(sharedMaterials[j]);
					material.SetFloat("_RippleDistance", 0f);
					material.SetFloat("_ValueNoise", 0f);
					material.SetFloat("_TriplanarLocalPos", 1f);
					sharedMaterials[j] = material;
				}
				t.sharedMaterials = sharedMaterials;
				t.shadowCastingMode = ShadowCastingMode.Off;
			}
		}
	}

	// Token: 0x06000213 RID: 531 RVA: 0x0001067E File Offset: 0x0000E87E
	private void SetPlacementGhostValid(bool valid)
	{
		this.m_placementGhost.GetComponent<Piece>().SetInvalidPlacementHeightlight(!valid);
	}

	// Token: 0x06000214 RID: 532 RVA: 0x00010694 File Offset: 0x0000E894
	protected override void SetPlaceMode(PieceTable buildPieces)
	{
		base.SetPlaceMode(buildPieces);
		this.m_buildPieces = buildPieces;
		this.UpdateAvailablePiecesList();
	}

	// Token: 0x06000215 RID: 533 RVA: 0x000106AA File Offset: 0x0000E8AA
	public void SetBuildCategory(int index)
	{
		if (this.m_buildPieces != null)
		{
			this.m_buildPieces.SetCategory(index);
			this.UpdateAvailablePiecesList();
		}
	}

	// Token: 0x06000216 RID: 534 RVA: 0x000106CC File Offset: 0x0000E8CC
	public override bool InPlaceMode()
	{
		return this.m_buildPieces != null;
	}

	// Token: 0x06000217 RID: 535 RVA: 0x000106DC File Offset: 0x0000E8DC
	private void Repair(ItemDrop.ItemData toolItem, Piece repairPiece)
	{
		if (!this.InPlaceMode())
		{
			return;
		}
		Piece hoveringPiece = this.GetHoveringPiece();
		if (hoveringPiece)
		{
			if (!this.CheckCanRemovePiece(hoveringPiece))
			{
				return;
			}
			if (!PrivateArea.CheckAccess(hoveringPiece.transform.position, 0f, true, false))
			{
				return;
			}
			bool flag = false;
			WearNTear component = hoveringPiece.GetComponent<WearNTear>();
			if (component && component.Repair())
			{
				flag = true;
			}
			if (flag)
			{
				this.FaceLookDirection();
				this.m_zanim.SetTrigger(toolItem.m_shared.m_attack.m_attackAnimation);
				hoveringPiece.m_placeEffect.Create(hoveringPiece.transform.position, hoveringPiece.transform.rotation, null, 1f, -1);
				this.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_repaired", new string[]
				{
					hoveringPiece.m_name
				}), 0, null);
				this.UseStamina(toolItem.m_shared.m_attack.m_attackStamina);
				this.UseEitr(toolItem.m_shared.m_attack.m_attackEitr);
				if (toolItem.m_shared.m_useDurability)
				{
					toolItem.m_durability -= toolItem.m_shared.m_useDurabilityDrain;
					return;
				}
			}
			else
			{
				this.Message(MessageHud.MessageType.TopLeft, hoveringPiece.m_name + " $msg_doesnotneedrepair", 0, null);
			}
		}
	}

	// Token: 0x06000218 RID: 536 RVA: 0x00010828 File Offset: 0x0000EA28
	private void UpdateWearNTearHover()
	{
		if (!this.InPlaceMode())
		{
			this.m_hoveringPiece = null;
			return;
		}
		this.m_hoveringPiece = null;
		RaycastHit raycastHit;
		if (Physics.Raycast(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, out raycastHit, 50f, this.m_removeRayMask) && Vector3.Distance(this.m_eye.position, raycastHit.point) < this.m_maxPlaceDistance)
		{
			Piece componentInParent = raycastHit.collider.GetComponentInParent<Piece>();
			this.m_hoveringPiece = componentInParent;
			if (componentInParent)
			{
				WearNTear component = componentInParent.GetComponent<WearNTear>();
				if (component)
				{
					component.Highlight();
				}
			}
		}
	}

	// Token: 0x06000219 RID: 537 RVA: 0x000108CE File Offset: 0x0000EACE
	public Piece GetHoveringPiece()
	{
		if (!this.InPlaceMode())
		{
			return null;
		}
		return this.m_hoveringPiece;
	}

	// Token: 0x0600021A RID: 538 RVA: 0x000108E0 File Offset: 0x0000EAE0
	private void UpdatePlacementGhost(bool flashGuardStone)
	{
		if (this.m_placementGhost == null)
		{
			if (this.m_placementMarkerInstance)
			{
				this.m_placementMarkerInstance.SetActive(false);
			}
			return;
		}
		bool flag = (ZInput.InputLayout == InputLayout.Alternative1 && ZInput.IsGamepadActive()) ? this.m_altPlace : (ZInput.GetButton("AltPlace") || ZInput.GetButton("JoyAltPlace"));
		Piece component = this.m_placementGhost.GetComponent<Piece>();
		bool water = component.m_waterPiece || component.m_noInWater;
		Vector3 vector;
		Vector3 up;
		Piece piece;
		Heightmap heightmap;
		Collider x;
		if (this.PieceRayTest(out vector, out up, out piece, out heightmap, out x, water))
		{
			this.m_placementStatus = Player.PlacementStatus.Valid;
			Quaternion rotation = Quaternion.Euler(0f, 22.5f * (float)this.m_placeRotation, 0f);
			if (this.m_placementMarkerInstance == null)
			{
				this.m_placementMarkerInstance = UnityEngine.Object.Instantiate<GameObject>(this.m_placeMarker, vector, Quaternion.identity);
			}
			this.m_placementMarkerInstance.SetActive(true);
			this.m_placementMarkerInstance.transform.position = vector;
			this.m_placementMarkerInstance.transform.rotation = Quaternion.LookRotation(up, rotation * Vector3.forward);
			if (component.m_groundOnly || component.m_groundPiece || component.m_cultivatedGroundOnly)
			{
				this.m_placementMarkerInstance.SetActive(false);
			}
			WearNTear wearNTear = (piece != null) ? piece.GetComponent<WearNTear>() : null;
			StationExtension component2 = component.GetComponent<StationExtension>();
			if (component2 != null)
			{
				CraftingStation craftingStation = component2.FindClosestStationInRange(vector);
				if (craftingStation)
				{
					component2.StartConnectionEffect(craftingStation, 1f);
				}
				else
				{
					component2.StopConnectionEffect();
					this.m_placementStatus = Player.PlacementStatus.ExtensionMissingStation;
				}
				if (component2.OtherExtensionInRange(component.m_spaceRequirement))
				{
					this.m_placementStatus = Player.PlacementStatus.MoreSpace;
				}
			}
			if (component.m_blockRadius > 0f && component.m_blockingPieces.Count > 0)
			{
				Collider[] array = Physics.OverlapSphere(vector, component.m_blockRadius, LayerMask.GetMask(new string[]
				{
					"piece"
				}));
				for (int i = 0; i < array.Length; i++)
				{
					Piece componentInParent = array[i].gameObject.GetComponentInParent<Piece>();
					if (componentInParent != null && componentInParent != component)
					{
						using (List<Piece>.Enumerator enumerator = component.m_blockingPieces.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								if (enumerator.Current.m_name == componentInParent.m_name)
								{
									this.m_placementStatus = Player.PlacementStatus.MoreSpace;
									break;
								}
							}
						}
					}
				}
			}
			if (component.m_mustConnectTo != null)
			{
				ZNetView exists = null;
				Collider[] array = Physics.OverlapSphere(component.transform.position, component.m_connectRadius);
				for (int i = 0; i < array.Length; i++)
				{
					ZNetView componentInParent2 = array[i].GetComponentInParent<ZNetView>();
					if (componentInParent2 != null && componentInParent2 != this.m_nview && componentInParent2.name.Contains(component.m_mustConnectTo.name))
					{
						if (component.m_mustBeAboveConnected)
						{
							RaycastHit raycastHit;
							Physics.Raycast(component.transform.position, Vector3.down, out raycastHit);
							if (raycastHit.transform.GetComponentInParent<ZNetView>() != componentInParent2)
							{
								goto IL_30D;
							}
						}
						exists = componentInParent2;
						break;
					}
					IL_30D:;
				}
				if (!exists)
				{
					this.m_placementStatus = Player.PlacementStatus.Invalid;
				}
			}
			if (wearNTear && !wearNTear.m_supports)
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_waterPiece && x == null && !flag)
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_noInWater && x != null)
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_groundPiece && heightmap == null)
			{
				this.m_placementGhost.SetActive(false);
				this.m_placementStatus = Player.PlacementStatus.Invalid;
				return;
			}
			if (component.m_groundOnly && heightmap == null)
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_cultivatedGroundOnly && (heightmap == null || !heightmap.IsCultivated(vector)))
			{
				this.m_placementStatus = Player.PlacementStatus.NeedCultivated;
			}
			if (component.m_vegetationGroundOnly && (heightmap == null || heightmap.GetVegetationMask(vector) < 0.25f))
			{
				this.m_placementStatus = Player.PlacementStatus.NeedDirt;
			}
			if (component.m_notOnWood && piece && wearNTear && (wearNTear.m_materialType == WearNTear.MaterialType.Wood || wearNTear.m_materialType == WearNTear.MaterialType.HardWood))
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_notOnTiltingSurface && up.y < 0.8f)
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_inCeilingOnly && up.y > -0.5f)
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_notOnFloor && up.y > 0.1f)
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_onlyInTeleportArea && !EffectArea.IsPointInsideArea(vector, EffectArea.Type.Teleport, 0f))
			{
				this.m_placementStatus = Player.PlacementStatus.NoTeleportArea;
			}
			if (!component.m_allowedInDungeons && base.InInterior() && !EnvMan.instance.CheckInteriorBuildingOverride())
			{
				this.m_placementStatus = Player.PlacementStatus.NotInDungeon;
			}
			if (heightmap)
			{
				up = Vector3.up;
			}
			this.m_placementGhost.SetActive(true);
			if (((component.m_groundPiece || component.m_clipGround) && heightmap) || component.m_clipEverything)
			{
				GameObject selectedPrefab = this.m_buildPieces.GetSelectedPrefab();
				TerrainModifier component3 = selectedPrefab.GetComponent<TerrainModifier>();
				TerrainOp component4 = selectedPrefab.GetComponent<TerrainOp>();
				if ((component3 || component4) && component.m_allowAltGroundPlacement && ((ZInput.InputLayout == InputLayout.Alternative1 && ZInput.IsGamepadActive()) ? (component.m_groundPiece && !this.m_altPlace) : (component.m_groundPiece && !ZInput.GetButton("AltPlace") && !ZInput.GetButton("JoyAltPlace"))))
				{
					float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
					vector.y = groundHeight;
				}
				this.m_placementGhost.transform.position = vector;
				this.m_placementGhost.transform.rotation = rotation;
			}
			else
			{
				Collider[] componentsInChildren = this.m_placementGhost.GetComponentsInChildren<Collider>();
				if (componentsInChildren.Length != 0)
				{
					this.m_placementGhost.transform.position = vector + up * 50f;
					this.m_placementGhost.transform.rotation = rotation;
					Vector3 b = Vector3.zero;
					float num = 999999f;
					foreach (Collider collider in componentsInChildren)
					{
						if (!collider.isTrigger && collider.enabled)
						{
							MeshCollider meshCollider = collider as MeshCollider;
							if (!(meshCollider != null) || meshCollider.convex)
							{
								Vector3 vector2 = collider.ClosestPoint(vector);
								float num2 = Vector3.Distance(vector2, vector);
								if (num2 < num)
								{
									b = vector2;
									num = num2;
								}
							}
						}
					}
					Vector3 b2 = this.m_placementGhost.transform.position - b;
					if (component.m_waterPiece)
					{
						b2.y = 3f;
					}
					this.m_placementGhost.transform.position = vector + b2;
					this.m_placementGhost.transform.rotation = rotation;
				}
			}
			if (!flag)
			{
				this.m_tempPieces.Clear();
				Transform transform;
				Transform transform2;
				if (this.FindClosestSnapPoints(this.m_placementGhost.transform, 0.5f, out transform, out transform2, this.m_tempPieces))
				{
					Vector3 position = transform2.parent.position;
					Vector3 vector3 = transform2.position - (transform.position - this.m_placementGhost.transform.position);
					if (!this.IsOverlappingOtherPiece(vector3, this.m_placementGhost.transform.rotation, this.m_placementGhost.name, this.m_tempPieces, component.m_allowRotatedOverlap))
					{
						this.m_placementGhost.transform.position = vector3;
					}
				}
			}
			if (Location.IsInsideNoBuildLocation(this.m_placementGhost.transform.position))
			{
				this.m_placementStatus = Player.PlacementStatus.NoBuildZone;
			}
			PrivateArea component5 = component.GetComponent<PrivateArea>();
			float radius = component5 ? component5.m_radius : 0f;
			bool wardCheck = component5 != null;
			if (!PrivateArea.CheckAccess(this.m_placementGhost.transform.position, radius, flashGuardStone, wardCheck))
			{
				this.m_placementStatus = Player.PlacementStatus.PrivateZone;
			}
			if (this.CheckPlacementGhostVSPlayers())
			{
				this.m_placementStatus = Player.PlacementStatus.BlockedbyPlayer;
			}
			if (component.m_onlyInBiome != Heightmap.Biome.None && (Heightmap.FindBiome(this.m_placementGhost.transform.position) & component.m_onlyInBiome) == Heightmap.Biome.None)
			{
				this.m_placementStatus = Player.PlacementStatus.WrongBiome;
			}
			if (component.m_noClipping && this.TestGhostClipping(this.m_placementGhost, 0.2f))
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
		}
		else
		{
			if (this.m_placementMarkerInstance)
			{
				this.m_placementMarkerInstance.SetActive(false);
			}
			this.m_placementGhost.SetActive(false);
			this.m_placementStatus = Player.PlacementStatus.Invalid;
		}
		this.SetPlacementGhostValid(this.m_placementStatus == Player.PlacementStatus.Valid);
	}

	// Token: 0x0600021B RID: 539 RVA: 0x000111B0 File Offset: 0x0000F3B0
	private bool IsOverlappingOtherPiece(Vector3 p, Quaternion rotation, string pieceName, List<Piece> pieces, bool allowRotatedOverlap)
	{
		foreach (Piece piece in this.m_tempPieces)
		{
			if (Vector3.Distance(p, piece.transform.position) < 0.05f && (!allowRotatedOverlap || Quaternion.Angle(piece.transform.rotation, rotation) <= 10f) && piece.gameObject.name.CustomStartsWith(pieceName))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600021C RID: 540 RVA: 0x0001124C File Offset: 0x0000F44C
	private bool FindClosestSnapPoints(Transform ghost, float maxSnapDistance, out Transform a, out Transform b, List<Piece> pieces)
	{
		this.m_tempSnapPoints1.Clear();
		ghost.GetComponent<Piece>().GetSnapPoints(this.m_tempSnapPoints1);
		this.m_tempSnapPoints2.Clear();
		this.m_tempPieces.Clear();
		Piece.GetSnapPoints(ghost.transform.position, 10f, this.m_tempSnapPoints2, this.m_tempPieces);
		float num = 9999999f;
		a = null;
		b = null;
		foreach (Transform transform in this.m_tempSnapPoints1)
		{
			Transform transform2;
			float num2;
			if (this.FindClosestSnappoint(transform.position, this.m_tempSnapPoints2, maxSnapDistance, out transform2, out num2) && num2 < num)
			{
				num = num2;
				a = transform;
				b = transform2;
			}
		}
		return a != null;
	}

	// Token: 0x0600021D RID: 541 RVA: 0x00011328 File Offset: 0x0000F528
	private bool FindClosestSnappoint(Vector3 p, List<Transform> snapPoints, float maxDistance, out Transform closest, out float distance)
	{
		closest = null;
		distance = 999999f;
		foreach (Transform transform in snapPoints)
		{
			float num = Vector3.Distance(transform.position, p);
			if (num <= maxDistance && num < distance)
			{
				closest = transform;
				distance = num;
			}
		}
		return closest != null;
	}

	// Token: 0x0600021E RID: 542 RVA: 0x000113A4 File Offset: 0x0000F5A4
	private bool TestGhostClipping(GameObject ghost, float maxPenetration)
	{
		Collider[] componentsInChildren = ghost.GetComponentsInChildren<Collider>();
		Collider[] array = Physics.OverlapSphere(ghost.transform.position, 10f, this.m_placeRayMask);
		foreach (Collider collider in componentsInChildren)
		{
			foreach (Collider collider2 in array)
			{
				Vector3 vector;
				float num;
				if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, collider2, collider2.transform.position, collider2.transform.rotation, out vector, out num) && num > maxPenetration)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x0600021F RID: 543 RVA: 0x00011448 File Offset: 0x0000F648
	private bool CheckPlacementGhostVSPlayers()
	{
		if (this.m_placementGhost == null)
		{
			return false;
		}
		List<Character> list = new List<Character>();
		Character.GetCharactersInRange(base.transform.position, 30f, list);
		foreach (Collider collider in this.m_placementGhost.GetComponentsInChildren<Collider>())
		{
			if (!collider.isTrigger && collider.enabled)
			{
				MeshCollider meshCollider = collider as MeshCollider;
				if (!(meshCollider != null) || meshCollider.convex)
				{
					foreach (Character character in list)
					{
						CapsuleCollider collider2 = character.GetCollider();
						Vector3 vector;
						float num;
						if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, collider2, collider2.transform.position, collider2.transform.rotation, out vector, out num))
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	// Token: 0x06000220 RID: 544 RVA: 0x0001155C File Offset: 0x0000F75C
	private bool PieceRayTest(out Vector3 point, out Vector3 normal, out Piece piece, out Heightmap heightmap, out Collider waterSurface, bool water)
	{
		int layerMask = this.m_placeRayMask;
		if (water)
		{
			layerMask = this.m_placeWaterRayMask;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, out raycastHit, 50f, layerMask) && raycastHit.collider && !raycastHit.collider.attachedRigidbody && Vector3.Distance(this.m_eye.position, raycastHit.point) < this.m_maxPlaceDistance)
		{
			point = raycastHit.point;
			normal = raycastHit.normal;
			piece = raycastHit.collider.GetComponentInParent<Piece>();
			heightmap = raycastHit.collider.GetComponent<Heightmap>();
			if (raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
			{
				waterSurface = raycastHit.collider;
			}
			else
			{
				waterSurface = null;
			}
			return true;
		}
		point = Vector3.zero;
		normal = Vector3.zero;
		piece = null;
		heightmap = null;
		waterSurface = null;
		return false;
	}

	// Token: 0x06000221 RID: 545 RVA: 0x0001167C File Offset: 0x0000F87C
	private void FindHoverObject(out GameObject hover, out Character hoverCreature)
	{
		hover = null;
		hoverCreature = null;
		RaycastHit[] array = Physics.RaycastAll(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, 50f, this.m_interactMask);
		Array.Sort<RaycastHit>(array, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
		RaycastHit[] array2 = array;
		int i = 0;
		while (i < array2.Length)
		{
			RaycastHit raycastHit = array2[i];
			if (!raycastHit.collider.attachedRigidbody || !(raycastHit.collider.attachedRigidbody.gameObject == base.gameObject))
			{
				if (hoverCreature == null)
				{
					Character character = raycastHit.collider.attachedRigidbody ? raycastHit.collider.attachedRigidbody.GetComponent<Character>() : raycastHit.collider.GetComponent<Character>();
					if (character != null && (!character.GetBaseAI() || !character.GetBaseAI().IsSleeping()) && !ParticleMist.IsMistBlocked(base.GetCenterPoint(), character.GetCenterPoint()))
					{
						hoverCreature = character;
					}
				}
				if (Vector3.Distance(this.m_eye.position, raycastHit.point) >= this.m_maxInteractDistance)
				{
					break;
				}
				if (raycastHit.collider.GetComponent<Hoverable>() != null)
				{
					hover = raycastHit.collider.gameObject;
					return;
				}
				if (raycastHit.collider.attachedRigidbody)
				{
					hover = raycastHit.collider.attachedRigidbody.gameObject;
					return;
				}
				hover = raycastHit.collider.gameObject;
				return;
			}
			else
			{
				i++;
			}
		}
	}

	// Token: 0x06000222 RID: 546 RVA: 0x0001181C File Offset: 0x0000FA1C
	private void Interact(GameObject go, bool hold, bool alt)
	{
		if (this.InAttack() || this.InDodge())
		{
			return;
		}
		if (hold && Time.time - this.m_lastHoverInteractTime < 0.2f)
		{
			return;
		}
		Interactable componentInParent = go.GetComponentInParent<Interactable>();
		if (componentInParent != null)
		{
			this.m_lastHoverInteractTime = Time.time;
			if (componentInParent.Interact(this, hold, alt))
			{
				base.DoInteractAnimation(go.transform.position);
			}
		}
	}

	// Token: 0x06000223 RID: 547 RVA: 0x00011884 File Offset: 0x0000FA84
	private void UpdateStations(float dt)
	{
		this.m_stationDiscoverTimer += dt;
		if (this.m_stationDiscoverTimer > 1f)
		{
			this.m_stationDiscoverTimer = 0f;
			CraftingStation.UpdateKnownStationsInRange(this);
		}
		if (!(this.m_currentStation != null))
		{
			if (this.m_inCraftingStation)
			{
				this.m_zanim.SetInt("crafting", 0);
				this.m_inCraftingStation = false;
				if (InventoryGui.IsVisible())
				{
					InventoryGui.instance.Hide();
				}
			}
			return;
		}
		if (!this.m_currentStation.InUseDistance(this))
		{
			InventoryGui.instance.Hide();
			this.SetCraftingStation(null);
			return;
		}
		if (!InventoryGui.IsVisible())
		{
			this.SetCraftingStation(null);
			return;
		}
		this.m_currentStation.PokeInUse();
		if (!this.AlwaysRotateCamera())
		{
			Vector3 normalized = (this.m_currentStation.transform.position - base.transform.position).normalized;
			normalized.y = 0f;
			normalized.Normalize();
			Quaternion to = Quaternion.LookRotation(normalized);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, this.m_turnSpeed * dt);
		}
		this.m_zanim.SetInt("crafting", this.m_currentStation.m_useAnimation);
		this.m_inCraftingStation = true;
	}

	// Token: 0x06000224 RID: 548 RVA: 0x000119CB File Offset: 0x0000FBCB
	public void SetCraftingStation(CraftingStation station)
	{
		if (this.m_currentStation == station)
		{
			return;
		}
		if (station)
		{
			this.AddKnownStation(station);
			station.PokeInUse();
			base.HideHandItems();
		}
		this.m_currentStation = station;
	}

	// Token: 0x06000225 RID: 549 RVA: 0x000119FE File Offset: 0x0000FBFE
	public CraftingStation GetCurrentCraftingStation()
	{
		return this.m_currentStation;
	}

	// Token: 0x06000226 RID: 550 RVA: 0x00011A08 File Offset: 0x0000FC08
	private void UpdateCover(float dt)
	{
		this.m_updateCoverTimer += dt;
		if (this.m_updateCoverTimer > 1f)
		{
			this.m_updateCoverTimer = 0f;
			Cover.GetCoverForPoint(base.GetCenterPoint(), out this.m_coverPercentage, out this.m_underRoof, 0.5f);
		}
	}

	// Token: 0x06000227 RID: 551 RVA: 0x00011A57 File Offset: 0x0000FC57
	public Character GetHoverCreature()
	{
		return this.m_hoveringCreature;
	}

	// Token: 0x06000228 RID: 552 RVA: 0x00011A5F File Offset: 0x0000FC5F
	public override GameObject GetHoverObject()
	{
		return this.m_hovering;
	}

	// Token: 0x06000229 RID: 553 RVA: 0x00011A67 File Offset: 0x0000FC67
	public override void OnNearFire(Vector3 point)
	{
		this.m_nearFireTimer = 0f;
	}

	// Token: 0x0600022A RID: 554 RVA: 0x00011A74 File Offset: 0x0000FC74
	public bool InShelter()
	{
		return this.m_coverPercentage >= 0.8f && this.m_underRoof;
	}

	// Token: 0x0600022B RID: 555 RVA: 0x00011A8B File Offset: 0x0000FC8B
	public float GetStamina()
	{
		return this.m_stamina;
	}

	// Token: 0x0600022C RID: 556 RVA: 0x00011A93 File Offset: 0x0000FC93
	public override float GetMaxStamina()
	{
		return this.m_maxStamina;
	}

	// Token: 0x0600022D RID: 557 RVA: 0x00011A9B File Offset: 0x0000FC9B
	public float GetEitr()
	{
		return this.m_eitr;
	}

	// Token: 0x0600022E RID: 558 RVA: 0x00011AA3 File Offset: 0x0000FCA3
	public override float GetMaxEitr()
	{
		return this.m_maxEitr;
	}

	// Token: 0x0600022F RID: 559 RVA: 0x00011AAB File Offset: 0x0000FCAB
	public override float GetEitrPercentage()
	{
		return this.m_eitr / this.m_maxEitr;
	}

	// Token: 0x06000230 RID: 560 RVA: 0x00011ABA File Offset: 0x0000FCBA
	public override float GetStaminaPercentage()
	{
		return this.m_stamina / this.m_maxStamina;
	}

	// Token: 0x06000231 RID: 561 RVA: 0x00011AC9 File Offset: 0x0000FCC9
	public void SetGodMode(bool godMode)
	{
		this.m_godMode = godMode;
	}

	// Token: 0x06000232 RID: 562 RVA: 0x00011AD2 File Offset: 0x0000FCD2
	public override bool InGodMode()
	{
		return this.m_godMode;
	}

	// Token: 0x06000233 RID: 563 RVA: 0x00011ADA File Offset: 0x0000FCDA
	public void SetGhostMode(bool ghostmode)
	{
		this.m_ghostMode = ghostmode;
	}

	// Token: 0x06000234 RID: 564 RVA: 0x00011AE3 File Offset: 0x0000FCE3
	public override bool InGhostMode()
	{
		return this.m_ghostMode;
	}

	// Token: 0x06000235 RID: 565 RVA: 0x00011AEC File Offset: 0x0000FCEC
	public override bool IsDebugFlying()
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return false;
		}
		if (this.m_nview.IsOwner())
		{
			return this.m_debugFly;
		}
		return this.m_nview.GetZDO().GetBool(ZDOVars.s_debugFly, false);
	}

	// Token: 0x06000236 RID: 566 RVA: 0x00011B40 File Offset: 0x0000FD40
	public override void AddEitr(float v)
	{
		this.m_eitr += v;
		if (this.m_eitr > this.m_maxEitr)
		{
			this.m_eitr = this.m_maxEitr;
		}
	}

	// Token: 0x06000237 RID: 567 RVA: 0x00011B6A File Offset: 0x0000FD6A
	public override void AddStamina(float v)
	{
		this.m_stamina += v;
		if (this.m_stamina > this.m_maxStamina)
		{
			this.m_stamina = this.m_maxStamina;
		}
	}

	// Token: 0x06000238 RID: 568 RVA: 0x00011B94 File Offset: 0x0000FD94
	public override void UseEitr(float v)
	{
		if (v == 0f)
		{
			return;
		}
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			this.RPC_UseEitr(0L, v);
			return;
		}
		this.m_nview.InvokeRPC("UseEitr", new object[]
		{
			v
		});
	}

	// Token: 0x06000239 RID: 569 RVA: 0x00011BEE File Offset: 0x0000FDEE
	private void RPC_UseEitr(long sender, float v)
	{
		if (v == 0f)
		{
			return;
		}
		this.m_eitr -= v;
		if (this.m_eitr < 0f)
		{
			this.m_eitr = 0f;
		}
		this.m_eitrRegenTimer = this.m_eitrRegenDelay;
	}

	// Token: 0x0600023A RID: 570 RVA: 0x00011C2C File Offset: 0x0000FE2C
	public override bool HaveEitr(float amount = 0f)
	{
		if (this.m_nview.IsValid() && !this.m_nview.IsOwner())
		{
			return this.m_nview.GetZDO().GetFloat(ZDOVars.s_eitr, this.m_maxEitr) > amount;
		}
		return this.m_eitr > amount;
	}

	// Token: 0x0600023B RID: 571 RVA: 0x00011C7C File Offset: 0x0000FE7C
	public override void UseStamina(float v)
	{
		if (v == 0f)
		{
			return;
		}
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			this.RPC_UseStamina(0L, v);
			return;
		}
		this.m_nview.InvokeRPC("UseStamina", new object[]
		{
			v
		});
	}

	// Token: 0x0600023C RID: 572 RVA: 0x00011CD6 File Offset: 0x0000FED6
	private void RPC_UseStamina(long sender, float v)
	{
		if (v == 0f)
		{
			return;
		}
		this.m_stamina -= v;
		if (this.m_stamina < 0f)
		{
			this.m_stamina = 0f;
		}
		this.m_staminaRegenTimer = this.m_staminaRegenDelay;
	}

	// Token: 0x0600023D RID: 573 RVA: 0x00011D14 File Offset: 0x0000FF14
	public override bool HaveStamina(float amount = 0f)
	{
		if (this.m_nview.IsValid() && !this.m_nview.IsOwner())
		{
			return this.m_nview.GetZDO().GetFloat(ZDOVars.s_stamina, this.m_maxStamina) > amount;
		}
		return this.m_stamina > amount;
	}

	// Token: 0x0600023E RID: 574 RVA: 0x00011D64 File Offset: 0x0000FF64
	public void Save(ZPackage pkg)
	{
		pkg.Write(26);
		pkg.Write(base.GetMaxHealth());
		pkg.Write(base.GetHealth());
		pkg.Write(this.GetMaxStamina());
		pkg.Write(this.m_firstSpawn);
		pkg.Write(this.m_timeSinceDeath);
		pkg.Write(this.m_guardianPower);
		pkg.Write(this.m_guardianPowerCooldown);
		this.m_inventory.Save(pkg);
		pkg.Write(this.m_knownRecipes.Count);
		foreach (string data in this.m_knownRecipes)
		{
			pkg.Write(data);
		}
		pkg.Write(this.m_knownStations.Count);
		foreach (KeyValuePair<string, int> keyValuePair in this.m_knownStations)
		{
			pkg.Write(keyValuePair.Key);
			pkg.Write(keyValuePair.Value);
		}
		pkg.Write(this.m_knownMaterial.Count);
		foreach (string data2 in this.m_knownMaterial)
		{
			pkg.Write(data2);
		}
		pkg.Write(this.m_shownTutorials.Count);
		foreach (string data3 in this.m_shownTutorials)
		{
			pkg.Write(data3);
		}
		pkg.Write(this.m_uniques.Count);
		foreach (string data4 in this.m_uniques)
		{
			pkg.Write(data4);
		}
		pkg.Write(this.m_trophies.Count);
		foreach (string data5 in this.m_trophies)
		{
			pkg.Write(data5);
		}
		pkg.Write(this.m_knownBiome.Count);
		foreach (Heightmap.Biome data6 in this.m_knownBiome)
		{
			pkg.Write((int)data6);
		}
		pkg.Write(this.m_knownTexts.Count);
		foreach (KeyValuePair<string, string> keyValuePair2 in this.m_knownTexts)
		{
			pkg.Write(keyValuePair2.Key);
			pkg.Write(keyValuePair2.Value);
		}
		pkg.Write(this.m_beardItem);
		pkg.Write(this.m_hairItem);
		pkg.Write(this.m_skinColor);
		pkg.Write(this.m_hairColor);
		pkg.Write(this.m_modelIndex);
		pkg.Write(this.m_foods.Count);
		foreach (Player.Food food in this.m_foods)
		{
			pkg.Write(food.m_name);
			pkg.Write(food.m_time);
		}
		this.m_skills.Save(pkg);
		pkg.Write(this.m_customData.Count);
		foreach (KeyValuePair<string, string> keyValuePair3 in this.m_customData)
		{
			pkg.Write(keyValuePair3.Key);
			pkg.Write(keyValuePair3.Value);
		}
		pkg.Write(this.GetStamina());
		pkg.Write(this.GetMaxEitr());
		pkg.Write(this.GetEitr());
	}

	// Token: 0x0600023F RID: 575 RVA: 0x000121E4 File Offset: 0x000103E4
	public void Load(ZPackage pkg)
	{
		this.m_isLoading = true;
		base.UnequipAllItems();
		int num = pkg.ReadInt();
		if (num >= 7)
		{
			this.SetMaxHealth(pkg.ReadSingle(), false);
		}
		float num2 = pkg.ReadSingle();
		float maxHealth = base.GetMaxHealth();
		if (num2 <= 0f || num2 > maxHealth || float.IsNaN(num2))
		{
			num2 = maxHealth;
		}
		base.SetHealth(num2);
		if (num >= 10)
		{
			float stamina = pkg.ReadSingle();
			this.SetMaxStamina(stamina, false);
			this.m_stamina = stamina;
		}
		if (num >= 8)
		{
			this.m_firstSpawn = pkg.ReadBool();
		}
		if (num >= 20)
		{
			this.m_timeSinceDeath = pkg.ReadSingle();
		}
		if (num >= 23)
		{
			string guardianPower = pkg.ReadString();
			this.SetGuardianPower(guardianPower);
		}
		if (num >= 24)
		{
			this.m_guardianPowerCooldown = pkg.ReadSingle();
		}
		if (num == 2)
		{
			pkg.ReadZDOID();
		}
		this.m_inventory.Load(pkg);
		int num3 = pkg.ReadInt();
		for (int i = 0; i < num3; i++)
		{
			string item = pkg.ReadString();
			this.m_knownRecipes.Add(item);
		}
		if (num < 15)
		{
			int num4 = pkg.ReadInt();
			for (int j = 0; j < num4; j++)
			{
				pkg.ReadString();
			}
		}
		else
		{
			int num5 = pkg.ReadInt();
			for (int k = 0; k < num5; k++)
			{
				string key = pkg.ReadString();
				int value = pkg.ReadInt();
				this.m_knownStations.Add(key, value);
			}
		}
		int num6 = pkg.ReadInt();
		for (int l = 0; l < num6; l++)
		{
			string item2 = pkg.ReadString();
			this.m_knownMaterial.Add(item2);
		}
		if (num < 19 || num >= 21)
		{
			int num7 = pkg.ReadInt();
			for (int m = 0; m < num7; m++)
			{
				string item3 = pkg.ReadString();
				this.m_shownTutorials.Add(item3);
			}
		}
		if (num >= 6)
		{
			int num8 = pkg.ReadInt();
			for (int n = 0; n < num8; n++)
			{
				string item4 = pkg.ReadString();
				this.m_uniques.Add(item4);
			}
		}
		if (num >= 9)
		{
			int num9 = pkg.ReadInt();
			for (int num10 = 0; num10 < num9; num10++)
			{
				string item5 = pkg.ReadString();
				this.m_trophies.Add(item5);
			}
		}
		if (num >= 18)
		{
			int num11 = pkg.ReadInt();
			for (int num12 = 0; num12 < num11; num12++)
			{
				Heightmap.Biome item6 = (Heightmap.Biome)pkg.ReadInt();
				this.m_knownBiome.Add(item6);
			}
		}
		if (num >= 22)
		{
			int num13 = pkg.ReadInt();
			for (int num14 = 0; num14 < num13; num14++)
			{
				string key2 = pkg.ReadString();
				string value2 = pkg.ReadString();
				this.m_knownTexts.Add(key2, value2);
			}
		}
		if (num >= 4)
		{
			string beard = pkg.ReadString();
			string hair = pkg.ReadString();
			base.SetBeard(beard);
			base.SetHair(hair);
		}
		if (num >= 5)
		{
			Vector3 skinColor = pkg.ReadVector3();
			Vector3 hairColor = pkg.ReadVector3();
			this.SetSkinColor(skinColor);
			this.SetHairColor(hairColor);
		}
		if (num >= 11)
		{
			int playerModel = pkg.ReadInt();
			this.SetPlayerModel(playerModel);
		}
		if (num >= 12)
		{
			this.m_foods.Clear();
			int num15 = pkg.ReadInt();
			for (int num16 = 0; num16 < num15; num16++)
			{
				if (num >= 14)
				{
					Player.Food food = new Player.Food();
					food.m_name = pkg.ReadString();
					if (num >= 25)
					{
						food.m_time = pkg.ReadSingle();
					}
					else
					{
						food.m_health = pkg.ReadSingle();
						if (num >= 16)
						{
							food.m_stamina = pkg.ReadSingle();
						}
					}
					GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(food.m_name);
					if (itemPrefab == null)
					{
						ZLog.LogWarning("Failed to find food item " + food.m_name);
					}
					else
					{
						food.m_item = itemPrefab.GetComponent<ItemDrop>().m_itemData;
						this.m_foods.Add(food);
					}
				}
				else
				{
					pkg.ReadString();
					pkg.ReadSingle();
					pkg.ReadSingle();
					pkg.ReadSingle();
					pkg.ReadSingle();
					pkg.ReadSingle();
					pkg.ReadSingle();
					if (num >= 13)
					{
						pkg.ReadSingle();
					}
				}
			}
		}
		if (num >= 17)
		{
			this.m_skills.Load(pkg);
		}
		if (num >= 26)
		{
			int num17 = pkg.ReadInt();
			for (int num18 = 0; num18 < num17; num18++)
			{
				string key3 = pkg.ReadString();
				string value3 = pkg.ReadString();
				this.m_customData[key3] = value3;
			}
			this.m_stamina = Mathf.Clamp(pkg.ReadSingle(), 0f, this.m_maxStamina);
			this.SetMaxEitr(pkg.ReadSingle(), false);
			this.m_eitr = Mathf.Clamp(pkg.ReadSingle(), 0f, this.m_maxEitr);
		}
		this.m_isLoading = false;
		this.UpdateAvailablePiecesList();
		this.EquipInventoryItems();
	}

	// Token: 0x06000240 RID: 576 RVA: 0x000126A4 File Offset: 0x000108A4
	private void EquipInventoryItems()
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory.GetEquippedItems())
		{
			if (!base.EquipItem(itemData, false))
			{
				itemData.m_equipped = false;
			}
		}
	}

	// Token: 0x06000241 RID: 577 RVA: 0x00012708 File Offset: 0x00010908
	public override bool CanMove()
	{
		return !this.m_teleporting && !this.InCutscene() && (!this.IsEncumbered() || this.HaveStamina(0f)) && base.CanMove();
	}

	// Token: 0x06000242 RID: 578 RVA: 0x0001273B File Offset: 0x0001093B
	public override bool IsEncumbered()
	{
		return this.m_inventory.GetTotalWeight() > this.GetMaxCarryWeight();
	}

	// Token: 0x06000243 RID: 579 RVA: 0x00012750 File Offset: 0x00010950
	public float GetMaxCarryWeight()
	{
		float maxCarryWeight = this.m_maxCarryWeight;
		this.m_seman.ModifyMaxCarryWeight(maxCarryWeight, ref maxCarryWeight);
		return maxCarryWeight;
	}

	// Token: 0x06000244 RID: 580 RVA: 0x00012773 File Offset: 0x00010973
	public override bool HaveUniqueKey(string name)
	{
		return this.m_uniques.Contains(name);
	}

	// Token: 0x06000245 RID: 581 RVA: 0x00012781 File Offset: 0x00010981
	protected override void AddUniqueKey(string name)
	{
		if (!this.m_uniques.Contains(name))
		{
			this.m_uniques.Add(name);
		}
	}

	// Token: 0x06000246 RID: 582 RVA: 0x0001279E File Offset: 0x0001099E
	public bool IsBiomeKnown(Heightmap.Biome biome)
	{
		return this.m_knownBiome.Contains(biome);
	}

	// Token: 0x06000247 RID: 583 RVA: 0x000127AC File Offset: 0x000109AC
	private void AddKnownBiome(Heightmap.Biome biome)
	{
		if (!this.m_knownBiome.Contains(biome))
		{
			this.m_knownBiome.Add(biome);
			if (biome != Heightmap.Biome.Meadows && biome != Heightmap.Biome.None)
			{
				string text = "$biome_" + biome.ToString().ToLower();
				MessageHud.instance.ShowBiomeFoundMsg(text, true);
			}
			if (biome == Heightmap.Biome.BlackForest && !ZoneSystem.instance.GetGlobalKey("defeated_eikthyr"))
			{
				this.ShowTutorial("blackforest", false);
			}
			Gogan.LogEvent("Game", "BiomeFound", biome.ToString(), 0L);
		}
	}

	// Token: 0x06000248 RID: 584 RVA: 0x00012843 File Offset: 0x00010A43
	public bool IsRecipeKnown(string name)
	{
		return this.m_knownRecipes.Contains(name);
	}

	// Token: 0x06000249 RID: 585 RVA: 0x00012854 File Offset: 0x00010A54
	private void AddKnownRecipe(Recipe recipe)
	{
		if (!this.m_knownRecipes.Contains(recipe.m_item.m_itemData.m_shared.m_name))
		{
			this.m_knownRecipes.Add(recipe.m_item.m_itemData.m_shared.m_name);
			MessageHud.instance.QueueUnlockMsg(recipe.m_item.m_itemData.GetIcon(), "$msg_newrecipe", recipe.m_item.m_itemData.m_shared.m_name);
			Gogan.LogEvent("Game", "RecipeFound", recipe.m_item.m_itemData.m_shared.m_name, 0L);
		}
	}

	// Token: 0x0600024A RID: 586 RVA: 0x00012900 File Offset: 0x00010B00
	private void AddKnownPiece(Piece piece)
	{
		if (this.m_knownRecipes.Contains(piece.m_name))
		{
			return;
		}
		this.m_knownRecipes.Add(piece.m_name);
		MessageHud.instance.QueueUnlockMsg(piece.m_icon, "$msg_newpiece", piece.m_name);
		Gogan.LogEvent("Game", "PieceFound", piece.m_name, 0L);
	}

	// Token: 0x0600024B RID: 587 RVA: 0x00012968 File Offset: 0x00010B68
	public void AddKnownStation(CraftingStation station)
	{
		int level = station.GetLevel();
		int num;
		if (this.m_knownStations.TryGetValue(station.m_name, out num))
		{
			if (num < level)
			{
				this.m_knownStations[station.m_name] = level;
				MessageHud.instance.QueueUnlockMsg(station.m_icon, "$msg_newstation_level", station.m_name + " $msg_level " + level.ToString());
				this.UpdateKnownRecipesList();
			}
			return;
		}
		this.m_knownStations.Add(station.m_name, level);
		MessageHud.instance.QueueUnlockMsg(station.m_icon, "$msg_newstation", station.m_name);
		Gogan.LogEvent("Game", "StationFound", station.m_name, 0L);
		this.UpdateKnownRecipesList();
	}

	// Token: 0x0600024C RID: 588 RVA: 0x00012A24 File Offset: 0x00010C24
	private bool KnowStationLevel(string name, int level)
	{
		int num;
		return this.m_knownStations.TryGetValue(name, out num) && num >= level;
	}

	// Token: 0x0600024D RID: 589 RVA: 0x00012A4C File Offset: 0x00010C4C
	public void AddKnownText(string label, string text)
	{
		if (label.Length == 0)
		{
			ZLog.LogWarning("Text " + text + " Is missing label");
			return;
		}
		if (!this.m_knownTexts.ContainsKey(label))
		{
			this.m_knownTexts.Add(label, text);
			this.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_newtext", new string[]
			{
				label
			}), 0, this.m_textIcon);
		}
	}

	// Token: 0x0600024E RID: 590 RVA: 0x00012AB9 File Offset: 0x00010CB9
	public List<KeyValuePair<string, string>> GetKnownTexts()
	{
		return this.m_knownTexts.ToList<KeyValuePair<string, string>>();
	}

	// Token: 0x0600024F RID: 591 RVA: 0x00012AC8 File Offset: 0x00010CC8
	public void AddKnownItem(ItemDrop.ItemData item)
	{
		if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophy)
		{
			this.AddTrophy(item);
		}
		if (!this.m_knownMaterial.Contains(item.m_shared.m_name))
		{
			this.m_knownMaterial.Add(item.m_shared.m_name);
			if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Material)
			{
				MessageHud.instance.QueueUnlockMsg(item.GetIcon(), "$msg_newmaterial", item.m_shared.m_name);
			}
			else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophy)
			{
				MessageHud.instance.QueueUnlockMsg(item.GetIcon(), "$msg_newtrophy", item.m_shared.m_name);
			}
			else
			{
				MessageHud.instance.QueueUnlockMsg(item.GetIcon(), "$msg_newitem", item.m_shared.m_name);
			}
			Gogan.LogEvent("Game", "ItemFound", item.m_shared.m_name, 0L);
			this.UpdateKnownRecipesList();
		}
	}

	// Token: 0x06000250 RID: 592 RVA: 0x00012BC0 File Offset: 0x00010DC0
	private void AddTrophy(ItemDrop.ItemData item)
	{
		if (item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Trophy)
		{
			return;
		}
		if (!this.m_trophies.Contains(item.m_dropPrefab.name))
		{
			this.m_trophies.Add(item.m_dropPrefab.name);
		}
	}

	// Token: 0x06000251 RID: 593 RVA: 0x00012C0C File Offset: 0x00010E0C
	public List<string> GetTrophies()
	{
		List<string> list = new List<string>();
		list.AddRange(this.m_trophies);
		return list;
	}

	// Token: 0x06000252 RID: 594 RVA: 0x00012C20 File Offset: 0x00010E20
	private void UpdateKnownRecipesList()
	{
		if (Game.instance == null)
		{
			return;
		}
		foreach (Recipe recipe in ObjectDB.instance.m_recipes)
		{
			if (recipe.m_enabled && !this.m_knownRecipes.Contains(recipe.m_item.m_itemData.m_shared.m_name) && this.HaveRequirements(recipe, true, 0))
			{
				this.AddKnownRecipe(recipe);
			}
		}
		this.m_tempOwnedPieceTables.Clear();
		this.m_inventory.GetAllPieceTables(this.m_tempOwnedPieceTables);
		bool flag = false;
		foreach (PieceTable pieceTable in this.m_tempOwnedPieceTables)
		{
			foreach (GameObject gameObject in pieceTable.m_pieces)
			{
				Piece component = gameObject.GetComponent<Piece>();
				if (component.m_enabled && !this.m_knownRecipes.Contains(component.m_name) && this.HaveRequirements(component, Player.RequirementMode.IsKnown))
				{
					this.AddKnownPiece(component);
					flag = true;
				}
			}
		}
		if (flag)
		{
			this.UpdateAvailablePiecesList();
		}
	}

	// Token: 0x06000253 RID: 595 RVA: 0x00012D90 File Offset: 0x00010F90
	private void UpdateAvailablePiecesList()
	{
		if (this.m_buildPieces != null)
		{
			this.m_buildPieces.UpdateAvailable(this.m_knownRecipes, this, false, this.m_noPlacementCost);
		}
		this.SetupPlacementGhost();
	}

	// Token: 0x06000254 RID: 596 RVA: 0x00012DC0 File Offset: 0x00010FC0
	public override void Message(MessageHud.MessageType type, string msg, int amount = 0, Sprite icon = null)
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			if (MessageHud.instance)
			{
				MessageHud.instance.ShowMessage(type, msg, amount, icon);
				return;
			}
		}
		else
		{
			this.m_nview.InvokeRPC("Message", new object[]
			{
				(int)type,
				msg,
				amount
			});
		}
	}

	// Token: 0x06000255 RID: 597 RVA: 0x00012E3E File Offset: 0x0001103E
	private void RPC_Message(long sender, int type, string msg, int amount)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (MessageHud.instance)
		{
			MessageHud.instance.ShowMessage((MessageHud.MessageType)type, msg, amount, null);
		}
	}

	// Token: 0x06000256 RID: 598 RVA: 0x00012E6C File Offset: 0x0001106C
	public static Player GetPlayer(long playerID)
	{
		foreach (Player player in Player.s_players)
		{
			if (player.GetPlayerID() == playerID)
			{
				return player;
			}
		}
		return null;
	}

	// Token: 0x06000257 RID: 599 RVA: 0x00012EC8 File Offset: 0x000110C8
	public static Player GetClosestPlayer(Vector3 point, float maxRange)
	{
		Player result = null;
		float num = 999999f;
		foreach (Player player in Player.s_players)
		{
			float num2 = Vector3.Distance(player.transform.position, point);
			if (num2 < num && num2 < maxRange)
			{
				num = num2;
				result = player;
			}
		}
		return result;
	}

	// Token: 0x06000258 RID: 600 RVA: 0x00012F40 File Offset: 0x00011140
	public static bool IsPlayerInRange(Vector3 point, float range, long playerID)
	{
		foreach (Player player in Player.s_players)
		{
			if (player.GetPlayerID() == playerID)
			{
				return Utils.DistanceXZ(player.transform.position, point) < range;
			}
		}
		return false;
	}

	// Token: 0x06000259 RID: 601 RVA: 0x00012FB0 File Offset: 0x000111B0
	public static void MessageAllInRange(Vector3 point, float range, MessageHud.MessageType type, string msg, Sprite icon = null)
	{
		foreach (Player player in Player.s_players)
		{
			if (Vector3.Distance(player.transform.position, point) < range)
			{
				player.Message(type, msg, 0, icon);
			}
		}
	}

	// Token: 0x0600025A RID: 602 RVA: 0x0001301C File Offset: 0x0001121C
	public static int GetPlayersInRangeXZ(Vector3 point, float range)
	{
		int num = 0;
		using (List<Player>.Enumerator enumerator = Player.s_players.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (Utils.DistanceXZ(enumerator.Current.transform.position, point) < range)
				{
					num++;
				}
			}
		}
		return num;
	}

	// Token: 0x0600025B RID: 603 RVA: 0x00013080 File Offset: 0x00011280
	private static void GetPlayersInRange(Vector3 point, float range, List<Player> players)
	{
		foreach (Player player in Player.s_players)
		{
			if (Vector3.Distance(player.transform.position, point) < range)
			{
				players.Add(player);
			}
		}
	}

	// Token: 0x0600025C RID: 604 RVA: 0x000130E8 File Offset: 0x000112E8
	public static bool IsPlayerInRange(Vector3 point, float range)
	{
		using (List<Player>.Enumerator enumerator = Player.s_players.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (Vector3.Distance(enumerator.Current.transform.position, point) < range)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x0600025D RID: 605 RVA: 0x0001314C File Offset: 0x0001134C
	public static bool IsPlayerInRange(Vector3 point, float range, float minNoise)
	{
		foreach (Player player in Player.s_players)
		{
			if (Vector3.Distance(player.transform.position, point) < range)
			{
				float noiseRange = player.GetNoiseRange();
				if (range <= noiseRange && noiseRange >= minNoise)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x0600025E RID: 606 RVA: 0x000131C4 File Offset: 0x000113C4
	public static Player GetPlayerNoiseRange(Vector3 point, float maxNoiseRange = 100f)
	{
		foreach (Player player in Player.s_players)
		{
			float num = Vector3.Distance(player.transform.position, point);
			float num2 = Mathf.Min(player.GetNoiseRange(), maxNoiseRange);
			if (num < num2)
			{
				return player;
			}
		}
		return null;
	}

	// Token: 0x0600025F RID: 607 RVA: 0x0001323C File Offset: 0x0001143C
	public static List<Player> GetAllPlayers()
	{
		return Player.s_players;
	}

	// Token: 0x06000260 RID: 608 RVA: 0x00013243 File Offset: 0x00011443
	public static Player GetRandomPlayer()
	{
		if (Player.s_players.Count == 0)
		{
			return null;
		}
		return Player.s_players[UnityEngine.Random.Range(0, Player.s_players.Count)];
	}

	// Token: 0x06000261 RID: 609 RVA: 0x00013270 File Offset: 0x00011470
	public void GetAvailableRecipes(ref List<Recipe> available)
	{
		available.Clear();
		foreach (Recipe recipe in ObjectDB.instance.m_recipes)
		{
			if (recipe.m_enabled && (recipe.m_item.m_itemData.m_shared.m_dlc.Length <= 0 || DLCMan.instance.IsDLCInstalled(recipe.m_item.m_itemData.m_shared.m_dlc)) && (this.m_knownRecipes.Contains(recipe.m_item.m_itemData.m_shared.m_name) || this.m_noPlacementCost) && (this.RequiredCraftingStation(recipe, 1, false) || this.m_noPlacementCost))
			{
				available.Add(recipe);
			}
		}
	}

	// Token: 0x06000262 RID: 610 RVA: 0x0001335C File Offset: 0x0001155C
	private void OnInventoryChanged()
	{
		if (this.m_isLoading)
		{
			return;
		}
		foreach (ItemDrop.ItemData itemData in this.m_inventory.GetAllItems())
		{
			this.AddKnownItem(itemData);
			if (itemData.m_shared.m_name == "$item_hammer")
			{
				this.ShowTutorial("hammer", false);
			}
			else if (itemData.m_shared.m_name == "$item_hoe")
			{
				this.ShowTutorial("hoe", false);
			}
			else if (itemData.m_shared.m_name == "$item_pickaxe_antler")
			{
				this.ShowTutorial("pickaxe", false);
			}
			else if (itemData.m_shared.m_name.CustomStartsWith("$item_shield"))
			{
				this.ShowTutorial("shield", false);
			}
			if (itemData.m_shared.m_name == "$item_trophy_eikthyr")
			{
				this.ShowTutorial("boss_trophy", false);
			}
			if (itemData.m_shared.m_name == "$item_wishbone")
			{
				this.ShowTutorial("wishbone", false);
			}
			else if (itemData.m_shared.m_name == "$item_copperore" || itemData.m_shared.m_name == "$item_tinore")
			{
				this.ShowTutorial("ore", false);
			}
			else if (itemData.m_shared.m_food > 0f || itemData.m_shared.m_foodStamina > 0f)
			{
				this.ShowTutorial("food", false);
			}
		}
		this.UpdateKnownRecipesList();
		this.UpdateAvailablePiecesList();
	}

	// Token: 0x06000263 RID: 611 RVA: 0x00013524 File Offset: 0x00011724
	public bool InDebugFlyMode()
	{
		return this.m_debugFly;
	}

	// Token: 0x06000264 RID: 612 RVA: 0x0001352C File Offset: 0x0001172C
	public void ShowTutorial(string name, bool force = false)
	{
		if (this.HaveSeenTutorial(name))
		{
			return;
		}
		Tutorial.instance.ShowText(name, force);
	}

	// Token: 0x06000265 RID: 613 RVA: 0x00013544 File Offset: 0x00011744
	public void SetSeenTutorial(string name)
	{
		if (name.Length == 0)
		{
			return;
		}
		if (this.m_shownTutorials.Contains(name))
		{
			return;
		}
		this.m_shownTutorials.Add(name);
	}

	// Token: 0x06000266 RID: 614 RVA: 0x0001356B File Offset: 0x0001176B
	public bool HaveSeenTutorial(string name)
	{
		return name.Length != 0 && this.m_shownTutorials.Contains(name);
	}

	// Token: 0x06000267 RID: 615 RVA: 0x00013583 File Offset: 0x00011783
	public static bool IsSeenTutorialsCleared()
	{
		return !Player.m_localPlayer || Player.m_localPlayer.m_shownTutorials.Count == 0;
	}

	// Token: 0x06000268 RID: 616 RVA: 0x000135A5 File Offset: 0x000117A5
	public static void ResetSeenTutorials()
	{
		if (Player.m_localPlayer)
		{
			Player.m_localPlayer.m_shownTutorials.Clear();
		}
	}

	// Token: 0x06000269 RID: 617 RVA: 0x000135C4 File Offset: 0x000117C4
	public void SetMouseLook(Vector2 mouseLook)
	{
		this.m_lookYaw *= Quaternion.Euler(0f, mouseLook.x, 0f);
		this.m_lookPitch = Mathf.Clamp(this.m_lookPitch - mouseLook.y, -89f, 89f);
		this.UpdateEyeRotation();
		this.m_lookDir = this.m_eye.forward;
		if (this.m_lookTransitionTime > 0f && mouseLook != Vector2.zero)
		{
			this.m_lookTransitionTime = 0f;
		}
	}

	// Token: 0x0600026A RID: 618 RVA: 0x00013655 File Offset: 0x00011855
	protected override void UpdateEyeRotation()
	{
		this.m_eye.rotation = this.m_lookYaw * Quaternion.Euler(this.m_lookPitch, 0f, 0f);
	}

	// Token: 0x0600026B RID: 619 RVA: 0x00013682 File Offset: 0x00011882
	public Ragdoll GetRagdoll()
	{
		return this.m_ragdoll;
	}

	// Token: 0x0600026C RID: 620 RVA: 0x0001368A File Offset: 0x0001188A
	public void OnDodgeMortal()
	{
		this.m_dodgeInvincible = false;
	}

	// Token: 0x0600026D RID: 621 RVA: 0x00013694 File Offset: 0x00011894
	private void UpdateDodge(float dt)
	{
		this.m_queuedDodgeTimer -= dt;
		if (this.m_queuedDodgeTimer > 0f && base.IsOnGround() && !this.IsDead() && !this.InAttack() && !this.IsEncumbered() && !this.InDodge() && !base.IsStaggering())
		{
			float num = this.m_dodgeStaminaUsage - this.m_dodgeStaminaUsage * this.m_equipmentMovementModifier;
			if (this.HaveStamina(num))
			{
				this.ClearActionQueue();
				this.m_queuedDodgeTimer = 0f;
				this.m_dodgeInvincible = true;
				base.transform.rotation = Quaternion.LookRotation(this.m_queuedDodgeDir);
				this.m_body.rotation = base.transform.rotation;
				this.m_zanim.SetTrigger("dodge");
				base.AddNoise(5f);
				this.UseStamina(num);
				this.m_dodgeEffects.Create(base.transform.position, Quaternion.identity, base.transform, 1f, -1);
			}
			else
			{
				Hud.instance.StaminaBarEmptyFlash();
			}
		}
		bool flag = this.m_animator.GetBool(Player.s_animatorTagDodge) || base.GetNextOrCurrentAnimHash() == Player.s_animatorTagDodge;
		bool value = flag && this.m_dodgeInvincible;
		this.m_nview.GetZDO().Set(ZDOVars.s_dodgeinv, value);
		this.m_inDodge = flag;
	}

	// Token: 0x0600026E RID: 622 RVA: 0x00013809 File Offset: 0x00011A09
	public override bool IsDodgeInvincible()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetBool(ZDOVars.s_dodgeinv, false);
	}

	// Token: 0x0600026F RID: 623 RVA: 0x00013830 File Offset: 0x00011A30
	public override bool InDodge()
	{
		return this.m_nview.IsValid() && this.m_nview.IsOwner() && this.m_inDodge;
	}

	// Token: 0x06000270 RID: 624 RVA: 0x00013854 File Offset: 0x00011A54
	public override bool IsDead()
	{
		ZDO zdo = this.m_nview.GetZDO();
		return zdo != null && zdo.GetBool(ZDOVars.s_dead, false);
	}

	// Token: 0x06000271 RID: 625 RVA: 0x0001387E File Offset: 0x00011A7E
	private void Dodge(Vector3 dodgeDir)
	{
		this.m_queuedDodgeTimer = 0.5f;
		this.m_queuedDodgeDir = dodgeDir;
	}

	// Token: 0x06000272 RID: 626 RVA: 0x00013894 File Offset: 0x00011A94
	protected override bool AlwaysRotateCamera()
	{
		ItemDrop.ItemData currentWeapon = base.GetCurrentWeapon();
		if ((currentWeapon != null && this.m_currentAttack != null && this.m_lastCombatTimer < 1f && this.m_currentAttack.m_attackType != Attack.AttackType.None && ZInput.IsMouseActive()) || this.IsDrawingBow() || this.m_blocking)
		{
			return true;
		}
		if (currentWeapon != null && currentWeapon.m_shared.m_alwaysRotate && this.m_moveDir.magnitude < 0.01f)
		{
			return true;
		}
		if (this.m_currentAttack != null && this.m_currentAttack.m_loopingAttack && this.InAttack())
		{
			return true;
		}
		if (this.InPlaceMode())
		{
			Vector3 from = base.GetLookYaw() * Vector3.forward;
			Vector3 forward = base.transform.forward;
			if (Vector3.Angle(from, forward) > 95f)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000273 RID: 627 RVA: 0x00013960 File Offset: 0x00011B60
	public override bool TeleportTo(Vector3 pos, Quaternion rot, bool distantTeleport)
	{
		if (!this.m_nview.IsOwner())
		{
			this.m_nview.InvokeRPC("RPC_TeleportTo", new object[]
			{
				pos,
				rot,
				distantTeleport
			});
			return false;
		}
		if (this.IsTeleporting())
		{
			return false;
		}
		if (this.m_teleportCooldown < 2f)
		{
			return false;
		}
		this.m_teleporting = true;
		this.m_distantTeleport = distantTeleport;
		this.m_teleportTimer = 0f;
		this.m_teleportCooldown = 0f;
		this.m_teleportFromPos = base.transform.position;
		this.m_teleportFromRot = base.transform.rotation;
		this.m_teleportTargetPos = pos;
		this.m_teleportTargetRot = rot;
		return true;
	}

	// Token: 0x06000274 RID: 628 RVA: 0x00013A1C File Offset: 0x00011C1C
	private void UpdateTeleport(float dt)
	{
		if (!this.m_teleporting)
		{
			this.m_teleportCooldown += dt;
			return;
		}
		this.m_teleportCooldown = 0f;
		this.m_teleportTimer += dt;
		if (this.m_teleportTimer > 2f)
		{
			Vector3 dir = this.m_teleportTargetRot * Vector3.forward;
			base.transform.position = this.m_teleportTargetPos;
			base.transform.rotation = this.m_teleportTargetRot;
			this.m_body.velocity = Vector3.zero;
			this.m_maxAirAltitude = base.transform.position.y;
			base.SetLookDir(dir, 0f);
			if ((this.m_teleportTimer > 8f || !this.m_distantTeleport) && ZNetScene.instance.IsAreaReady(this.m_teleportTargetPos))
			{
				float num = 0f;
				if (ZoneSystem.instance.FindFloor(this.m_teleportTargetPos, out num))
				{
					this.m_teleportTimer = 0f;
					this.m_teleporting = false;
					base.ResetCloth();
					return;
				}
				if (this.m_teleportTimer > 15f || !this.m_distantTeleport)
				{
					if (this.m_distantTeleport)
					{
						Vector3 position = base.transform.position;
						position.y = ZoneSystem.instance.GetSolidHeight(this.m_teleportTargetPos) + 0.5f;
						base.transform.position = position;
					}
					else
					{
						base.transform.rotation = this.m_teleportFromRot;
						base.transform.position = this.m_teleportFromPos;
						this.m_maxAirAltitude = base.transform.position.y;
						this.Message(MessageHud.MessageType.Center, "$msg_portal_blocked", 0, null);
					}
					this.m_teleportTimer = 0f;
					this.m_teleporting = false;
					base.ResetCloth();
				}
			}
		}
	}

	// Token: 0x06000275 RID: 629 RVA: 0x00013BE3 File Offset: 0x00011DE3
	public override bool IsTeleporting()
	{
		return this.m_teleporting;
	}

	// Token: 0x06000276 RID: 630 RVA: 0x00013BEB File Offset: 0x00011DEB
	public bool ShowTeleportAnimation()
	{
		return this.m_teleporting && this.m_distantTeleport;
	}

	// Token: 0x06000277 RID: 631 RVA: 0x00013BFD File Offset: 0x00011DFD
	public void SetPlayerModel(int index)
	{
		if (this.m_modelIndex == index)
		{
			return;
		}
		this.m_modelIndex = index;
		this.m_visEquipment.SetModel(index);
	}

	// Token: 0x06000278 RID: 632 RVA: 0x00013C1C File Offset: 0x00011E1C
	public int GetPlayerModel()
	{
		return this.m_modelIndex;
	}

	// Token: 0x06000279 RID: 633 RVA: 0x00013C24 File Offset: 0x00011E24
	public void SetSkinColor(Vector3 color)
	{
		if (color == this.m_skinColor)
		{
			return;
		}
		this.m_skinColor = color;
		this.m_visEquipment.SetSkinColor(this.m_skinColor);
	}

	// Token: 0x0600027A RID: 634 RVA: 0x00013C4D File Offset: 0x00011E4D
	public void SetHairColor(Vector3 color)
	{
		if (this.m_hairColor == color)
		{
			return;
		}
		this.m_hairColor = color;
		this.m_visEquipment.SetHairColor(this.m_hairColor);
	}

	// Token: 0x0600027B RID: 635 RVA: 0x00013C76 File Offset: 0x00011E76
	protected override void SetupVisEquipment(VisEquipment visEq, bool isRagdoll)
	{
		base.SetupVisEquipment(visEq, isRagdoll);
		visEq.SetModel(this.m_modelIndex);
		visEq.SetSkinColor(this.m_skinColor);
		visEq.SetHairColor(this.m_hairColor);
	}

	// Token: 0x0600027C RID: 636 RVA: 0x00013CA4 File Offset: 0x00011EA4
	public override bool CanConsumeItem(ItemDrop.ItemData item)
	{
		if (item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Consumable)
		{
			return false;
		}
		if (item.m_shared.m_food > 0f && !this.CanEat(item, true))
		{
			return false;
		}
		if (item.m_shared.m_consumeStatusEffect)
		{
			StatusEffect consumeStatusEffect = item.m_shared.m_consumeStatusEffect;
			if (this.m_seman.HaveStatusEffect(item.m_shared.m_consumeStatusEffect.name) || this.m_seman.HaveStatusEffectCategory(consumeStatusEffect.m_category))
			{
				this.Message(MessageHud.MessageType.Center, "$msg_cantconsume", 0, null);
				return false;
			}
		}
		return true;
	}

	// Token: 0x0600027D RID: 637 RVA: 0x00013D40 File Offset: 0x00011F40
	public override bool ConsumeItem(Inventory inventory, ItemDrop.ItemData item)
	{
		if (!this.CanConsumeItem(item))
		{
			return false;
		}
		if (item.m_shared.m_consumeStatusEffect)
		{
			StatusEffect consumeStatusEffect = item.m_shared.m_consumeStatusEffect;
			this.m_seman.AddStatusEffect(item.m_shared.m_consumeStatusEffect, true, 0, 0f);
		}
		if (item.m_shared.m_food > 0f)
		{
			this.EatFood(item);
		}
		inventory.RemoveOneItem(item);
		return true;
	}

	// Token: 0x0600027E RID: 638 RVA: 0x00013DB7 File Offset: 0x00011FB7
	public void SetIntro(bool intro)
	{
		if (this.m_intro == intro)
		{
			return;
		}
		this.m_intro = intro;
		this.m_zanim.SetBool("intro", intro);
	}

	// Token: 0x0600027F RID: 639 RVA: 0x00013DDB File Offset: 0x00011FDB
	public override bool InIntro()
	{
		return this.m_intro;
	}

	// Token: 0x06000280 RID: 640 RVA: 0x00013DE3 File Offset: 0x00011FE3
	public override bool InCutscene()
	{
		return base.GetCurrentAnimHash() == Player.s_animatorTagCutscene || this.InIntro() || this.m_sleeping || base.InCutscene();
	}

	// Token: 0x06000281 RID: 641 RVA: 0x00013E10 File Offset: 0x00012010
	public void SetMaxStamina(float stamina, bool flashBar)
	{
		if (flashBar && Hud.instance != null && stamina > this.m_maxStamina)
		{
			Hud.instance.StaminaBarUppgradeFlash();
		}
		this.m_maxStamina = stamina;
		this.m_stamina = Mathf.Clamp(this.m_stamina, 0f, this.m_maxStamina);
	}

	// Token: 0x06000282 RID: 642 RVA: 0x00013E64 File Offset: 0x00012064
	private void SetMaxEitr(float eitr, bool flashBar)
	{
		if (flashBar && Hud.instance != null && eitr > this.m_maxEitr)
		{
			Hud.instance.EitrBarUppgradeFlash();
		}
		this.m_maxEitr = eitr;
		this.m_eitr = Mathf.Clamp(this.m_eitr, 0f, this.m_maxEitr);
	}

	// Token: 0x06000283 RID: 643 RVA: 0x00013EB7 File Offset: 0x000120B7
	public void SetMaxHealth(float health, bool flashBar)
	{
		if (flashBar && Hud.instance != null && health > base.GetMaxHealth())
		{
			Hud.instance.FlashHealthBar();
		}
		base.SetMaxHealth(health);
	}

	// Token: 0x06000284 RID: 644 RVA: 0x00013EE3 File Offset: 0x000120E3
	public override bool IsPVPEnabled()
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (this.m_nview.IsOwner())
		{
			return this.m_pvp;
		}
		return this.m_nview.GetZDO().GetBool(ZDOVars.s_pvp, false);
	}

	// Token: 0x06000285 RID: 645 RVA: 0x00013F20 File Offset: 0x00012120
	public void SetPVP(bool enabled)
	{
		if (this.m_pvp == enabled)
		{
			return;
		}
		this.m_pvp = enabled;
		this.m_nview.GetZDO().Set(ZDOVars.s_pvp, this.m_pvp);
		if (this.m_pvp)
		{
			this.Message(MessageHud.MessageType.Center, "$msg_pvpon", 0, null);
			return;
		}
		this.Message(MessageHud.MessageType.Center, "$msg_pvpoff", 0, null);
	}

	// Token: 0x06000286 RID: 646 RVA: 0x00013F7E File Offset: 0x0001217E
	public bool CanSwitchPVP()
	{
		return this.m_lastCombatTimer > 10f;
	}

	// Token: 0x06000287 RID: 647 RVA: 0x00013F8D File Offset: 0x0001218D
	public bool NoCostCheat()
	{
		return this.m_noPlacementCost;
	}

	// Token: 0x06000288 RID: 648 RVA: 0x00013F98 File Offset: 0x00012198
	public bool StartEmote(string emote, bool oneshot = true)
	{
		if (!this.CanMove() || this.InAttack() || this.IsDrawingBow() || this.IsAttached() || this.IsAttachedToShip())
		{
			return false;
		}
		this.SetCrouch(false);
		int @int = this.m_nview.GetZDO().GetInt(ZDOVars.s_emoteID, 0);
		this.m_nview.GetZDO().Set(ZDOVars.s_emoteID, @int + 1, false);
		this.m_nview.GetZDO().Set(ZDOVars.s_emote, emote);
		this.m_nview.GetZDO().Set(ZDOVars.s_emoteOneshot, oneshot);
		return true;
	}

	// Token: 0x06000289 RID: 649 RVA: 0x00014034 File Offset: 0x00012234
	protected override void StopEmote()
	{
		if (this.m_nview.GetZDO().GetString(ZDOVars.s_emote, "") != "")
		{
			int @int = this.m_nview.GetZDO().GetInt(ZDOVars.s_emoteID, 0);
			this.m_nview.GetZDO().Set(ZDOVars.s_emoteID, @int + 1, false);
			this.m_nview.GetZDO().Set(ZDOVars.s_emote, "");
		}
	}

	// Token: 0x0600028A RID: 650 RVA: 0x000140B4 File Offset: 0x000122B4
	private void UpdateEmote()
	{
		if (this.m_nview.IsOwner() && this.InEmote() && this.m_moveDir != Vector3.zero)
		{
			this.StopEmote();
		}
		int @int = this.m_nview.GetZDO().GetInt(ZDOVars.s_emoteID, 0);
		if (@int != this.m_emoteID)
		{
			this.m_emoteID = @int;
			if (!string.IsNullOrEmpty(this.m_emoteState))
			{
				this.m_animator.SetBool("emote_" + this.m_emoteState, false);
			}
			this.m_emoteState = "";
			this.m_animator.SetTrigger("emote_stop");
			string @string = this.m_nview.GetZDO().GetString(ZDOVars.s_emote, "");
			if (!string.IsNullOrEmpty(@string))
			{
				bool @bool = this.m_nview.GetZDO().GetBool(ZDOVars.s_emoteOneshot, false);
				this.m_animator.ResetTrigger("emote_stop");
				if (@bool)
				{
					this.m_animator.SetTrigger("emote_" + @string);
					return;
				}
				this.m_emoteState = @string;
				this.m_animator.SetBool("emote_" + @string, true);
			}
		}
	}

	// Token: 0x0600028B RID: 651 RVA: 0x000141DC File Offset: 0x000123DC
	public override bool InEmote()
	{
		return !string.IsNullOrEmpty(this.m_emoteState) || base.GetCurrentAnimHash() == Player.s_animatorTagEmote;
	}

	// Token: 0x0600028C RID: 652 RVA: 0x000141FA File Offset: 0x000123FA
	public override bool IsCrouching()
	{
		return base.GetCurrentAnimHash() == Player.s_animatorTagCrouch;
	}

	// Token: 0x0600028D RID: 653 RVA: 0x0001420C File Offset: 0x0001240C
	private void UpdateCrouch(float dt)
	{
		if (this.m_crouchToggled)
		{
			if (!this.HaveStamina(0f) || base.IsSwimming() || this.InBed() || this.InPlaceMode() || this.m_run || this.IsBlocking() || base.IsFlying())
			{
				this.SetCrouch(false);
			}
			bool flag = this.InAttack() || this.IsDrawingBow();
			this.m_zanim.SetBool(Player.s_crouching, this.m_crouchToggled && !flag);
			return;
		}
		this.m_zanim.SetBool(Player.s_crouching, false);
	}

	// Token: 0x0600028E RID: 654 RVA: 0x000142A8 File Offset: 0x000124A8
	protected override void SetCrouch(bool crouch)
	{
		this.m_crouchToggled = crouch;
	}

	// Token: 0x0600028F RID: 655 RVA: 0x000142B1 File Offset: 0x000124B1
	public void SetGuardianPower(string name)
	{
		this.m_guardianPower = name;
		this.m_guardianSE = ObjectDB.instance.GetStatusEffect(this.m_guardianPower);
	}

	// Token: 0x06000290 RID: 656 RVA: 0x000142D0 File Offset: 0x000124D0
	public string GetGuardianPowerName()
	{
		return this.m_guardianPower;
	}

	// Token: 0x06000291 RID: 657 RVA: 0x000142D8 File Offset: 0x000124D8
	public void GetGuardianPowerHUD(out StatusEffect se, out float cooldown)
	{
		se = this.m_guardianSE;
		cooldown = this.m_guardianPowerCooldown;
	}

	// Token: 0x06000292 RID: 658 RVA: 0x000142EC File Offset: 0x000124EC
	public bool StartGuardianPower()
	{
		if (this.m_guardianSE == null)
		{
			return false;
		}
		if ((this.InAttack() && !this.HaveQueuedChain()) || this.InDodge() || !this.CanMove() || base.IsKnockedBack() || base.IsStaggering() || this.InMinorAction())
		{
			return false;
		}
		if (this.m_guardianPowerCooldown > 0f)
		{
			this.Message(MessageHud.MessageType.Center, "$hud_powernotready", 0, null);
			return false;
		}
		this.m_zanim.SetTrigger("gpower");
		return true;
	}

	// Token: 0x06000293 RID: 659 RVA: 0x00014374 File Offset: 0x00012574
	public bool ActivateGuardianPower()
	{
		if (this.m_guardianPowerCooldown > 0f)
		{
			return false;
		}
		if (this.m_guardianSE == null)
		{
			return false;
		}
		List<Player> list = new List<Player>();
		Player.GetPlayersInRange(base.transform.position, 10f, list);
		foreach (Player player in list)
		{
			player.GetSEMan().AddStatusEffect(this.m_guardianSE.NameHash(), true, 0, 0f);
		}
		this.m_guardianPowerCooldown = this.m_guardianSE.m_cooldown;
		return false;
	}

	// Token: 0x06000294 RID: 660 RVA: 0x00014424 File Offset: 0x00012624
	private void UpdateGuardianPower(float dt)
	{
		this.m_guardianPowerCooldown -= dt;
		if (this.m_guardianPowerCooldown < 0f)
		{
			this.m_guardianPowerCooldown = 0f;
		}
	}

	// Token: 0x06000295 RID: 661 RVA: 0x0001444C File Offset: 0x0001264C
	public override void AttachStart(Transform attachPoint, GameObject colliderRoot, bool hideWeapons, bool isBed, bool onShip, string attachAnimation, Vector3 detachOffset)
	{
		if (this.m_attached)
		{
			return;
		}
		this.m_attached = true;
		this.m_attachedToShip = onShip;
		this.m_attachPoint = attachPoint;
		this.m_detachOffset = detachOffset;
		this.m_attachAnimation = attachAnimation;
		this.m_zanim.SetBool(attachAnimation, true);
		this.m_nview.GetZDO().Set(ZDOVars.s_inBed, isBed);
		if (colliderRoot != null)
		{
			this.m_attachColliders = colliderRoot.GetComponentsInChildren<Collider>();
			ZLog.Log("Ignoring " + this.m_attachColliders.Length.ToString() + " colliders");
			foreach (Collider collider in this.m_attachColliders)
			{
				Physics.IgnoreCollision(this.m_collider, collider, true);
			}
		}
		if (hideWeapons)
		{
			base.HideHandItems();
		}
		this.UpdateAttach();
		base.ResetCloth();
	}

	// Token: 0x06000296 RID: 662 RVA: 0x00014524 File Offset: 0x00012724
	private void UpdateAttach()
	{
		if (this.m_attached)
		{
			if (this.m_attachPoint != null)
			{
				base.transform.position = this.m_attachPoint.position;
				base.transform.rotation = this.m_attachPoint.rotation;
				Rigidbody componentInParent = this.m_attachPoint.GetComponentInParent<Rigidbody>();
				this.m_body.useGravity = false;
				this.m_body.velocity = (componentInParent ? componentInParent.GetPointVelocity(base.transform.position) : Vector3.zero);
				this.m_body.angularVelocity = Vector3.zero;
				this.m_maxAirAltitude = base.transform.position.y;
				return;
			}
			this.AttachStop();
		}
	}

	// Token: 0x06000297 RID: 663 RVA: 0x000145E9 File Offset: 0x000127E9
	public override bool IsAttached()
	{
		return this.m_attached || base.IsAttached();
	}

	// Token: 0x06000298 RID: 664 RVA: 0x000145FB File Offset: 0x000127FB
	public override bool IsAttachedToShip()
	{
		return this.m_attached && this.m_attachedToShip;
	}

	// Token: 0x06000299 RID: 665 RVA: 0x0001460D File Offset: 0x0001280D
	public override bool IsRiding()
	{
		return this.m_doodadController != null && this.m_doodadController.IsValid() && this.m_doodadController is Sadle;
	}

	// Token: 0x0600029A RID: 666 RVA: 0x00014634 File Offset: 0x00012834
	public override bool InBed()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetBool(ZDOVars.s_inBed, false);
	}

	// Token: 0x0600029B RID: 667 RVA: 0x0001465C File Offset: 0x0001285C
	public override void AttachStop()
	{
		if (this.m_sleeping)
		{
			return;
		}
		if (this.m_attached)
		{
			if (this.m_attachPoint != null)
			{
				base.transform.position = this.m_attachPoint.TransformPoint(this.m_detachOffset);
			}
			if (this.m_attachColliders != null)
			{
				foreach (Collider collider in this.m_attachColliders)
				{
					if (collider)
					{
						Physics.IgnoreCollision(this.m_collider, collider, false);
					}
				}
				this.m_attachColliders = null;
			}
			this.m_body.useGravity = true;
			this.m_attached = false;
			this.m_attachPoint = null;
			this.m_zanim.SetBool(this.m_attachAnimation, false);
			this.m_nview.GetZDO().Set(ZDOVars.s_inBed, false);
			base.ResetCloth();
		}
	}

	// Token: 0x0600029C RID: 668 RVA: 0x0001472C File Offset: 0x0001292C
	public void StartDoodadControl(IDoodadController shipControl)
	{
		this.m_doodadController = shipControl;
		ZLog.Log("Doodad controlls set " + shipControl.GetControlledComponent().gameObject.name);
	}

	// Token: 0x0600029D RID: 669 RVA: 0x00014754 File Offset: 0x00012954
	public void StopDoodadControl()
	{
		if (this.m_doodadController != null)
		{
			if (this.m_doodadController.IsValid())
			{
				this.m_doodadController.OnUseStop(this);
			}
			ZLog.Log("Stop doodad controlls");
			this.m_doodadController = null;
		}
	}

	// Token: 0x0600029E RID: 670 RVA: 0x00014788 File Offset: 0x00012988
	private void SetDoodadControlls(ref Vector3 moveDir, ref Vector3 lookDir, ref bool run, ref bool autoRun, bool block)
	{
		if (this.m_doodadController.IsValid())
		{
			this.m_doodadController.ApplyControlls(moveDir, lookDir, run, autoRun, block);
		}
		moveDir = Vector3.zero;
		autoRun = false;
		run = false;
	}

	// Token: 0x0600029F RID: 671 RVA: 0x000147C7 File Offset: 0x000129C7
	public Ship GetControlledShip()
	{
		if (this.m_doodadController != null && this.m_doodadController.IsValid())
		{
			return this.m_doodadController.GetControlledComponent() as Ship;
		}
		return null;
	}

	// Token: 0x060002A0 RID: 672 RVA: 0x000147F0 File Offset: 0x000129F0
	public IDoodadController GetDoodadController()
	{
		return this.m_doodadController;
	}

	// Token: 0x060002A1 RID: 673 RVA: 0x000147F8 File Offset: 0x000129F8
	private void UpdateDoodadControls(float dt)
	{
		if (this.m_doodadController == null)
		{
			return;
		}
		if (!this.m_doodadController.IsValid())
		{
			this.StopDoodadControl();
			return;
		}
		Vector3 forward = this.m_doodadController.GetControlledComponent().transform.forward;
		forward.y = 0f;
		forward.Normalize();
		Quaternion to = Quaternion.LookRotation(forward);
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, 100f * dt);
		if (Vector3.Distance(this.m_doodadController.GetPosition(), base.transform.position) > this.m_maxInteractDistance)
		{
			this.StopDoodadControl();
		}
	}

	// Token: 0x060002A2 RID: 674 RVA: 0x0001489E File Offset: 0x00012A9E
	public bool IsSleeping()
	{
		return this.m_sleeping;
	}

	// Token: 0x060002A3 RID: 675 RVA: 0x000148A8 File Offset: 0x00012AA8
	public void SetSleeping(bool sleep)
	{
		if (this.m_sleeping == sleep)
		{
			return;
		}
		this.m_sleeping = sleep;
		if (!sleep)
		{
			this.Message(MessageHud.MessageType.Center, "$msg_goodmorning", 0, null);
			this.m_seman.AddStatusEffect(Player.s_statusEffectRested, true, 0, 0f);
			this.m_wakeupTime = DateTime.Now;
		}
	}

	// Token: 0x060002A4 RID: 676 RVA: 0x000148FC File Offset: 0x00012AFC
	public void SetControls(Vector3 movedir, bool attack, bool attackHold, bool secondaryAttack, bool secondaryAttackHold, bool block, bool blockHold, bool jump, bool crouch, bool run, bool autoRun, bool dodge = false)
	{
		if ((this.IsAttached() || this.InEmote()) && (movedir != Vector3.zero || attack || secondaryAttack || block || blockHold || jump || crouch) && this.GetDoodadController() == null)
		{
			attack = false;
			attackHold = false;
			secondaryAttack = false;
			secondaryAttackHold = false;
			this.StopEmote();
			this.AttachStop();
		}
		if (this.m_doodadController != null)
		{
			this.SetDoodadControlls(ref movedir, ref this.m_lookDir, ref run, ref autoRun, blockHold);
			if (jump || attack || secondaryAttack)
			{
				attack = false;
				attackHold = false;
				secondaryAttack = false;
				secondaryAttackHold = false;
				this.StopDoodadControl();
			}
		}
		if (run)
		{
			this.m_walk = false;
		}
		if (!this.m_autoRun)
		{
			Vector3 lookDir = this.m_lookDir;
			lookDir.y = 0f;
			lookDir.Normalize();
			this.m_moveDir = movedir.z * lookDir + movedir.x * Vector3.Cross(Vector3.up, lookDir);
		}
		if (!this.m_autoRun && autoRun && !this.InPlaceMode())
		{
			this.m_autoRun = true;
			this.SetCrouch(false);
			this.m_moveDir = this.m_lookDir;
			this.m_moveDir.y = 0f;
			this.m_moveDir.Normalize();
		}
		else if (this.m_autoRun)
		{
			if (attack || jump || crouch || movedir != Vector3.zero || this.InPlaceMode() || attackHold || secondaryAttackHold)
			{
				this.m_autoRun = false;
			}
			else if (autoRun || blockHold)
			{
				this.m_moveDir = this.m_lookDir;
				this.m_moveDir.y = 0f;
				this.m_moveDir.Normalize();
				blockHold = false;
				block = false;
			}
		}
		this.m_attack = attack;
		this.m_attackHold = attackHold;
		this.m_secondaryAttack = secondaryAttack;
		this.m_secondaryAttackHold = secondaryAttackHold;
		this.m_blocking = blockHold;
		this.m_run = run;
		if (crouch)
		{
			this.SetCrouch(!this.m_crouchToggled);
		}
		if (ZInput.InputLayout == InputLayout.Default || !ZInput.IsGamepadActive())
		{
			if (jump)
			{
				if (this.m_blocking)
				{
					Vector3 dodgeDir = this.m_moveDir;
					if (dodgeDir.magnitude < 0.1f)
					{
						dodgeDir = -this.m_lookDir;
						dodgeDir.y = 0f;
						dodgeDir.Normalize();
					}
					this.Dodge(dodgeDir);
					return;
				}
				if (this.IsCrouching() || this.m_crouchToggled)
				{
					Vector3 dodgeDir2 = this.m_moveDir;
					if (dodgeDir2.magnitude < 0.1f)
					{
						dodgeDir2 = this.m_lookDir;
						dodgeDir2.y = 0f;
						dodgeDir2.Normalize();
					}
					this.Dodge(dodgeDir2);
					return;
				}
				base.Jump(false);
				return;
			}
		}
		else if (ZInput.InputLayout == InputLayout.Alternative1)
		{
			if (dodge)
			{
				if (this.m_blocking)
				{
					Vector3 dodgeDir3 = this.m_moveDir;
					if (dodgeDir3.magnitude < 0.1f)
					{
						dodgeDir3 = -this.m_lookDir;
						dodgeDir3.y = 0f;
						dodgeDir3.Normalize();
					}
					this.Dodge(dodgeDir3);
				}
				else if (this.IsCrouching() || this.m_crouchToggled)
				{
					Vector3 dodgeDir4 = this.m_moveDir;
					if (dodgeDir4.magnitude < 0.1f)
					{
						dodgeDir4 = this.m_lookDir;
						dodgeDir4.y = 0f;
						dodgeDir4.Normalize();
					}
					this.Dodge(dodgeDir4);
				}
			}
			if (jump)
			{
				base.Jump(false);
			}
		}
	}

	// Token: 0x060002A5 RID: 677 RVA: 0x00014C37 File Offset: 0x00012E37
	private void UpdateTargeted(float dt)
	{
		this.m_timeSinceTargeted += dt;
		this.m_timeSinceSensed += dt;
	}

	// Token: 0x060002A6 RID: 678 RVA: 0x00014C58 File Offset: 0x00012E58
	public override void OnTargeted(bool sensed, bool alerted)
	{
		if (sensed)
		{
			if (this.m_timeSinceSensed > 0.5f)
			{
				this.m_timeSinceSensed = 0f;
				this.m_nview.InvokeRPC("OnTargeted", new object[]
				{
					sensed,
					alerted
				});
				return;
			}
		}
		else if (this.m_timeSinceTargeted > 0.5f)
		{
			this.m_timeSinceTargeted = 0f;
			this.m_nview.InvokeRPC("OnTargeted", new object[]
			{
				sensed,
				alerted
			});
		}
	}

	// Token: 0x060002A7 RID: 679 RVA: 0x00014CE9 File Offset: 0x00012EE9
	private void RPC_OnTargeted(long sender, bool sensed, bool alerted)
	{
		this.m_timeSinceTargeted = 0f;
		if (sensed)
		{
			this.m_timeSinceSensed = 0f;
		}
		if (alerted)
		{
			MusicMan.instance.ResetCombatTimer();
		}
	}

	// Token: 0x060002A8 RID: 680 RVA: 0x00014D11 File Offset: 0x00012F11
	protected override void OnDamaged(HitData hit)
	{
		base.OnDamaged(hit);
		if (hit.GetTotalDamage() > base.GetMaxHealth() / 10f)
		{
			Hud.instance.DamageFlash();
		}
	}

	// Token: 0x060002A9 RID: 681 RVA: 0x00014D38 File Offset: 0x00012F38
	public bool IsTargeted()
	{
		return this.m_timeSinceTargeted < 1f;
	}

	// Token: 0x060002AA RID: 682 RVA: 0x00014D47 File Offset: 0x00012F47
	public bool IsSensed()
	{
		return this.m_timeSinceSensed < 1f;
	}

	// Token: 0x060002AB RID: 683 RVA: 0x00014D58 File Offset: 0x00012F58
	protected override void ApplyArmorDamageMods(ref HitData.DamageModifiers mods)
	{
		if (this.m_chestItem != null)
		{
			mods.Apply(this.m_chestItem.m_shared.m_damageModifiers);
		}
		if (this.m_legItem != null)
		{
			mods.Apply(this.m_legItem.m_shared.m_damageModifiers);
		}
		if (this.m_helmetItem != null)
		{
			mods.Apply(this.m_helmetItem.m_shared.m_damageModifiers);
		}
		if (this.m_shoulderItem != null)
		{
			mods.Apply(this.m_shoulderItem.m_shared.m_damageModifiers);
		}
	}

	// Token: 0x060002AC RID: 684 RVA: 0x00014DE0 File Offset: 0x00012FE0
	public override float GetBodyArmor()
	{
		float num = 0f;
		if (this.m_chestItem != null)
		{
			num += this.m_chestItem.GetArmor();
		}
		if (this.m_legItem != null)
		{
			num += this.m_legItem.GetArmor();
		}
		if (this.m_helmetItem != null)
		{
			num += this.m_helmetItem.GetArmor();
		}
		if (this.m_shoulderItem != null)
		{
			num += this.m_shoulderItem.GetArmor();
		}
		return num;
	}

	// Token: 0x060002AD RID: 685 RVA: 0x00014E4C File Offset: 0x0001304C
	protected override void OnSneaking(float dt)
	{
		float t = Mathf.Pow(this.m_skills.GetSkillFactor(Skills.SkillType.Sneak), 0.5f);
		float num = Mathf.Lerp(1f, 0.25f, t);
		this.UseStamina(dt * this.m_sneakStaminaDrain * num);
		if (!this.HaveStamina(0f))
		{
			Hud.instance.StaminaBarEmptyFlash();
		}
		this.m_sneakSkillImproveTimer += dt;
		if (this.m_sneakSkillImproveTimer > 1f)
		{
			this.m_sneakSkillImproveTimer = 0f;
			if (BaseAI.InStealthRange(this))
			{
				this.RaiseSkill(Skills.SkillType.Sneak, 1f);
				return;
			}
			this.RaiseSkill(Skills.SkillType.Sneak, 0.1f);
		}
	}

	// Token: 0x060002AE RID: 686 RVA: 0x00014EF4 File Offset: 0x000130F4
	private void UpdateStealth(float dt)
	{
		this.m_stealthFactorUpdateTimer += dt;
		if (this.m_stealthFactorUpdateTimer > 0.5f)
		{
			this.m_stealthFactorUpdateTimer = 0f;
			this.m_stealthFactorTarget = 0f;
			if (this.IsCrouching())
			{
				float skillFactor = this.m_skills.GetSkillFactor(Skills.SkillType.Sneak);
				float lightFactor = StealthSystem.instance.GetLightFactor(base.GetCenterPoint());
				this.m_stealthFactorTarget = Mathf.Lerp(0.5f + lightFactor * 0.5f, 0.2f + lightFactor * 0.4f, skillFactor);
				this.m_stealthFactorTarget = Mathf.Clamp01(this.m_stealthFactorTarget);
				this.m_seman.ModifyStealth(this.m_stealthFactorTarget, ref this.m_stealthFactorTarget);
				this.m_stealthFactorTarget = Mathf.Clamp01(this.m_stealthFactorTarget);
			}
			else
			{
				this.m_stealthFactorTarget = 1f;
			}
		}
		this.m_stealthFactor = Mathf.MoveTowards(this.m_stealthFactor, this.m_stealthFactorTarget, dt / 4f);
		this.m_nview.GetZDO().Set(ZDOVars.s_stealth, this.m_stealthFactor);
	}

	// Token: 0x060002AF RID: 687 RVA: 0x00015004 File Offset: 0x00013204
	public override float GetStealthFactor()
	{
		if (!this.m_nview.IsValid())
		{
			return 0f;
		}
		if (this.m_nview.IsOwner())
		{
			return this.m_stealthFactor;
		}
		return this.m_nview.GetZDO().GetFloat(ZDOVars.s_stealth, 0f);
	}

	// Token: 0x060002B0 RID: 688 RVA: 0x00015054 File Offset: 0x00013254
	public override bool InAttack()
	{
		if (MonoUpdaters.UpdateCount == this.m_cachedFrame)
		{
			return this.m_cachedAttack;
		}
		this.m_cachedFrame = MonoUpdaters.UpdateCount;
		if (base.GetNextOrCurrentAnimHash() == Humanoid.s_animatorTagAttack)
		{
			this.m_cachedAttack = true;
			return true;
		}
		for (int i = 1; i < this.m_animator.layerCount; i++)
		{
			if ((this.m_animator.IsInTransition(i) ? this.m_animator.GetNextAnimatorStateInfo(i).tagHash : this.m_animator.GetCurrentAnimatorStateInfo(i).tagHash) == Humanoid.s_animatorTagAttack)
			{
				this.m_cachedAttack = true;
				return true;
			}
		}
		this.m_cachedAttack = false;
		return false;
	}

	// Token: 0x060002B1 RID: 689 RVA: 0x000150FC File Offset: 0x000132FC
	public override float GetEquipmentMovementModifier()
	{
		return this.m_equipmentMovementModifier;
	}

	// Token: 0x060002B2 RID: 690 RVA: 0x00015104 File Offset: 0x00013304
	protected override float GetJogSpeedFactor()
	{
		return 1f + this.m_equipmentMovementModifier;
	}

	// Token: 0x060002B3 RID: 691 RVA: 0x00015114 File Offset: 0x00013314
	protected override float GetRunSpeedFactor()
	{
		float skillFactor = this.m_skills.GetSkillFactor(Skills.SkillType.Run);
		return (1f + skillFactor * 0.25f) * (1f + this.m_equipmentMovementModifier * 1.5f);
	}

	// Token: 0x060002B4 RID: 692 RVA: 0x00015150 File Offset: 0x00013350
	public override bool InMinorAction()
	{
		int tagHash = this.m_animator.GetCurrentAnimatorStateInfo(1).tagHash;
		if (tagHash == Player.s_animatorTagMinorAction || tagHash == Player.s_animatorTagMinorActionFast)
		{
			return true;
		}
		if (this.m_animator.IsInTransition(1))
		{
			int tagHash2 = this.m_animator.GetNextAnimatorStateInfo(1).tagHash;
			return tagHash2 == Player.s_animatorTagMinorAction || tagHash2 == Player.s_animatorTagMinorActionFast;
		}
		return false;
	}

	// Token: 0x060002B5 RID: 693 RVA: 0x000151BC File Offset: 0x000133BC
	public override bool InMinorActionSlowdown()
	{
		return this.m_animator.GetCurrentAnimatorStateInfo(1).tagHash == Player.s_animatorTagMinorAction || (this.m_animator.IsInTransition(1) && this.m_animator.GetNextAnimatorStateInfo(1).tagHash == Player.s_animatorTagMinorAction);
	}

	// Token: 0x060002B6 RID: 694 RVA: 0x00015214 File Offset: 0x00013414
	public override bool GetRelativePosition(out ZDOID parent, out string attachJoint, out Vector3 relativePos, out Quaternion relativeRot, out Vector3 relativeVel)
	{
		if (this.m_attached && this.m_attachPoint)
		{
			ZNetView componentInParent = this.m_attachPoint.GetComponentInParent<ZNetView>();
			if (componentInParent && componentInParent.IsValid())
			{
				parent = componentInParent.GetZDO().m_uid;
				if (componentInParent.GetComponent<Character>() != null)
				{
					attachJoint = this.m_attachPoint.name;
					relativePos = Vector3.zero;
					relativeRot = Quaternion.identity;
				}
				else
				{
					attachJoint = "";
					relativePos = componentInParent.transform.InverseTransformPoint(base.transform.position);
					relativeRot = Quaternion.Inverse(componentInParent.transform.rotation) * base.transform.rotation;
				}
				relativeVel = Vector3.zero;
				return true;
			}
		}
		return base.GetRelativePosition(out parent, out attachJoint, out relativePos, out relativeRot, out relativeVel);
	}

	// Token: 0x060002B7 RID: 695 RVA: 0x00015308 File Offset: 0x00013508
	public override Skills GetSkills()
	{
		return this.m_skills;
	}

	// Token: 0x060002B8 RID: 696 RVA: 0x00015310 File Offset: 0x00013510
	public override float GetRandomSkillFactor(Skills.SkillType skill)
	{
		return this.m_skills.GetRandomSkillFactor(skill);
	}

	// Token: 0x060002B9 RID: 697 RVA: 0x0001531E File Offset: 0x0001351E
	public override float GetSkillFactor(Skills.SkillType skill)
	{
		return this.m_skills.GetSkillFactor(skill);
	}

	// Token: 0x060002BA RID: 698 RVA: 0x0001532C File Offset: 0x0001352C
	protected override void DoDamageCameraShake(HitData hit)
	{
		float totalStaggerDamage = hit.m_damage.GetTotalStaggerDamage();
		if (GameCamera.instance && totalStaggerDamage > 0f)
		{
			float num = Mathf.Clamp01(totalStaggerDamage / base.GetMaxHealth());
			GameCamera.instance.AddShake(base.transform.position, 50f, this.m_baseCameraShake * num, false);
		}
	}

	// Token: 0x060002BB RID: 699 RVA: 0x0001538C File Offset: 0x0001358C
	protected override void DamageArmorDurability(HitData hit)
	{
		List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>();
		if (this.m_chestItem != null)
		{
			list.Add(this.m_chestItem);
		}
		if (this.m_legItem != null)
		{
			list.Add(this.m_legItem);
		}
		if (this.m_helmetItem != null)
		{
			list.Add(this.m_helmetItem);
		}
		if (this.m_shoulderItem != null)
		{
			list.Add(this.m_shoulderItem);
		}
		if (list.Count == 0)
		{
			return;
		}
		float num = hit.GetTotalPhysicalDamage() + hit.GetTotalElementalDamage();
		if (num <= 0f)
		{
			return;
		}
		int index = UnityEngine.Random.Range(0, list.Count);
		ItemDrop.ItemData itemData = list[index];
		itemData.m_durability = Mathf.Max(0f, itemData.m_durability - num);
	}

	// Token: 0x060002BC RID: 700 RVA: 0x0001543C File Offset: 0x0001363C
	protected override bool ToggleEquipped(ItemDrop.ItemData item)
	{
		if (!item.IsEquipable())
		{
			return false;
		}
		if (this.InAttack())
		{
			return true;
		}
		if (item.m_shared.m_equipDuration <= 0f)
		{
			if (base.IsItemEquiped(item))
			{
				base.UnequipItem(item, true);
			}
			else
			{
				base.EquipItem(item, true);
			}
		}
		else if (base.IsItemEquiped(item))
		{
			this.QueueUnequipAction(item);
		}
		else
		{
			this.QueueEquipAction(item);
		}
		return true;
	}

	// Token: 0x060002BD RID: 701 RVA: 0x000154A8 File Offset: 0x000136A8
	public void GetActionProgress(out string name, out float progress)
	{
		if (this.m_actionQueue.Count > 0)
		{
			Player.MinorActionData minorActionData = this.m_actionQueue[0];
			if (minorActionData.m_duration > 0.5f)
			{
				float num = Mathf.Clamp01(minorActionData.m_time / minorActionData.m_duration);
				if (num > 0f)
				{
					name = minorActionData.m_progressText;
					progress = num;
					return;
				}
			}
		}
		name = null;
		progress = 0f;
	}

	// Token: 0x060002BE RID: 702 RVA: 0x00015510 File Offset: 0x00013710
	private void UpdateActionQueue(float dt)
	{
		if (this.m_actionQueuePause > 0f)
		{
			this.m_actionQueuePause -= dt;
			if (this.m_actionAnimation != null)
			{
				this.m_zanim.SetBool(this.m_actionAnimation, false);
				this.m_actionAnimation = null;
			}
			return;
		}
		if (this.InAttack())
		{
			if (this.m_actionAnimation != null)
			{
				this.m_zanim.SetBool(this.m_actionAnimation, false);
				this.m_actionAnimation = null;
			}
			return;
		}
		if (this.m_actionQueue.Count == 0)
		{
			if (this.m_actionAnimation != null)
			{
				this.m_zanim.SetBool(this.m_actionAnimation, false);
				this.m_actionAnimation = null;
			}
			return;
		}
		Player.MinorActionData minorActionData = this.m_actionQueue[0];
		if (this.m_actionAnimation != null && this.m_actionAnimation != minorActionData.m_animation)
		{
			this.m_zanim.SetBool(this.m_actionAnimation, false);
			this.m_actionAnimation = null;
		}
		this.m_zanim.SetBool(minorActionData.m_animation, true);
		this.m_actionAnimation = minorActionData.m_animation;
		if (minorActionData.m_time == 0f && minorActionData.m_startEffect != null)
		{
			minorActionData.m_startEffect.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		}
		if (minorActionData.m_staminaDrain > 0f)
		{
			this.UseStamina(minorActionData.m_staminaDrain * dt);
		}
		minorActionData.m_time += dt;
		if (minorActionData.m_time > minorActionData.m_duration)
		{
			this.m_actionQueue.RemoveAt(0);
			this.m_zanim.SetBool(this.m_actionAnimation, false);
			this.m_actionAnimation = null;
			if (!string.IsNullOrEmpty(minorActionData.m_doneAnimation))
			{
				this.m_zanim.SetTrigger(minorActionData.m_doneAnimation);
			}
			switch (minorActionData.m_type)
			{
			case Player.MinorActionData.ActionType.Equip:
				base.EquipItem(minorActionData.m_item, true);
				break;
			case Player.MinorActionData.ActionType.Unequip:
				base.UnequipItem(minorActionData.m_item, true);
				break;
			case Player.MinorActionData.ActionType.Reload:
				this.SetWeaponLoaded(minorActionData.m_item);
				break;
			}
			this.m_actionQueuePause = 0.3f;
		}
	}

	// Token: 0x060002BF RID: 703 RVA: 0x00015718 File Offset: 0x00013918
	private void QueueEquipAction(ItemDrop.ItemData item)
	{
		if (item == null)
		{
			return;
		}
		if (this.IsEquipActionQueued(item))
		{
			this.RemoveEquipAction(item);
			return;
		}
		this.CancelReloadAction();
		Player.MinorActionData minorActionData = new Player.MinorActionData();
		minorActionData.m_item = item;
		minorActionData.m_type = Player.MinorActionData.ActionType.Equip;
		minorActionData.m_duration = item.m_shared.m_equipDuration;
		minorActionData.m_progressText = "$hud_equipping " + item.m_shared.m_name;
		minorActionData.m_animation = "equipping";
		if (minorActionData.m_duration >= 1f)
		{
			minorActionData.m_startEffect = this.m_equipStartEffects;
		}
		this.m_actionQueue.Add(minorActionData);
	}

	// Token: 0x060002C0 RID: 704 RVA: 0x000157B0 File Offset: 0x000139B0
	private void QueueUnequipAction(ItemDrop.ItemData item)
	{
		if (item == null)
		{
			return;
		}
		if (this.IsEquipActionQueued(item))
		{
			this.RemoveEquipAction(item);
			return;
		}
		this.CancelReloadAction();
		Player.MinorActionData minorActionData = new Player.MinorActionData();
		minorActionData.m_item = item;
		minorActionData.m_type = Player.MinorActionData.ActionType.Unequip;
		minorActionData.m_duration = item.m_shared.m_equipDuration;
		minorActionData.m_progressText = "$hud_unequipping " + item.m_shared.m_name;
		minorActionData.m_animation = "equipping";
		this.m_actionQueue.Add(minorActionData);
	}

	// Token: 0x060002C1 RID: 705 RVA: 0x00015830 File Offset: 0x00013A30
	private void QueueReloadAction()
	{
		if (this.IsReloadActionQueued())
		{
			return;
		}
		ItemDrop.ItemData currentWeapon = base.GetCurrentWeapon();
		if (currentWeapon == null || !currentWeapon.m_shared.m_attack.m_requiresReload)
		{
			return;
		}
		Player.MinorActionData minorActionData = new Player.MinorActionData();
		minorActionData.m_item = currentWeapon;
		minorActionData.m_type = Player.MinorActionData.ActionType.Reload;
		minorActionData.m_duration = currentWeapon.GetWeaponLoadingTime();
		minorActionData.m_progressText = "$hud_reloading " + currentWeapon.m_shared.m_name;
		minorActionData.m_animation = currentWeapon.m_shared.m_attack.m_reloadAnimation;
		minorActionData.m_doneAnimation = currentWeapon.m_shared.m_attack.m_reloadAnimation + "_done";
		minorActionData.m_staminaDrain = currentWeapon.m_shared.m_attack.m_reloadStaminaDrain;
		this.m_actionQueue.Add(minorActionData);
	}

	// Token: 0x060002C2 RID: 706 RVA: 0x000158F6 File Offset: 0x00013AF6
	protected override void ClearActionQueue()
	{
		this.m_actionQueue.Clear();
	}

	// Token: 0x060002C3 RID: 707 RVA: 0x00015904 File Offset: 0x00013B04
	public override void RemoveEquipAction(ItemDrop.ItemData item)
	{
		if (item == null)
		{
			return;
		}
		foreach (Player.MinorActionData minorActionData in this.m_actionQueue)
		{
			if (minorActionData.m_item == item)
			{
				this.m_actionQueue.Remove(minorActionData);
				break;
			}
		}
	}

	// Token: 0x060002C4 RID: 708 RVA: 0x0001596C File Offset: 0x00013B6C
	public bool IsEquipActionQueued(ItemDrop.ItemData item)
	{
		if (item == null)
		{
			return false;
		}
		foreach (Player.MinorActionData minorActionData in this.m_actionQueue)
		{
			if ((minorActionData.m_type == Player.MinorActionData.ActionType.Equip || minorActionData.m_type == Player.MinorActionData.ActionType.Unequip) && minorActionData.m_item == item)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060002C5 RID: 709 RVA: 0x000159E0 File Offset: 0x00013BE0
	private bool IsReloadActionQueued()
	{
		using (List<Player.MinorActionData>.Enumerator enumerator = this.m_actionQueue.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_type == Player.MinorActionData.ActionType.Reload)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060002C6 RID: 710 RVA: 0x00015A3C File Offset: 0x00013C3C
	public void ResetCharacter()
	{
		this.m_guardianPowerCooldown = 0f;
		Player.ResetSeenTutorials();
		this.m_knownRecipes.Clear();
		this.m_knownStations.Clear();
		this.m_knownMaterial.Clear();
		this.m_uniques.Clear();
		this.m_trophies.Clear();
		this.m_skills.Clear();
		this.m_knownBiome.Clear();
		this.m_knownTexts.Clear();
	}

	// Token: 0x060002C7 RID: 711 RVA: 0x00015AB4 File Offset: 0x00013CB4
	public bool ToggleDebugFly()
	{
		this.m_debugFly = !this.m_debugFly;
		this.m_nview.GetZDO().Set(ZDOVars.s_debugFly, this.m_debugFly);
		this.Message(MessageHud.MessageType.TopLeft, "Debug fly:" + this.m_debugFly.ToString(), 0, null);
		return this.m_debugFly;
	}

	// Token: 0x060002C8 RID: 712 RVA: 0x00015B0F File Offset: 0x00013D0F
	public void SetNoPlacementCost(bool value)
	{
		if (value != this.m_noPlacementCost)
		{
			this.ToggleNoPlacementCost();
		}
	}

	// Token: 0x060002C9 RID: 713 RVA: 0x00015B21 File Offset: 0x00013D21
	public bool ToggleNoPlacementCost()
	{
		this.m_noPlacementCost = !this.m_noPlacementCost;
		this.Message(MessageHud.MessageType.TopLeft, "No placement cost:" + this.m_noPlacementCost.ToString(), 0, null);
		this.UpdateAvailablePiecesList();
		return this.m_noPlacementCost;
	}

	// Token: 0x060002CA RID: 714 RVA: 0x00015B5C File Offset: 0x00013D5C
	public bool IsKnownMaterial(string name)
	{
		return this.m_knownMaterial.Contains(name);
	}

	// Token: 0x17000006 RID: 6
	// (get) Token: 0x060002CB RID: 715 RVA: 0x00015B6A File Offset: 0x00013D6A
	public bool AlternativePlacementActive
	{
		get
		{
			return this.m_altPlace;
		}
	}

	// Token: 0x040001C0 RID: 448
	public static bool m_debugMode = false;

	// Token: 0x040001C1 RID: 449
	private const int m_playerDataSaveVersion = 26;

	// Token: 0x040001C2 RID: 450
	private float m_baseValueUpdateTimer;

	// Token: 0x040001C3 RID: 451
	private float m_rotatePieceTimer;

	// Token: 0x040001C4 RID: 452
	private bool m_altPlace;

	// Token: 0x040001C5 RID: 453
	public static Player m_localPlayer = null;

	// Token: 0x040001C6 RID: 454
	private static readonly List<Player> s_players = new List<Player>();

	// Token: 0x040001C7 RID: 455
	[Header("Player")]
	public float m_maxPlaceDistance = 5f;

	// Token: 0x040001C8 RID: 456
	public float m_maxInteractDistance = 5f;

	// Token: 0x040001C9 RID: 457
	public float m_staminaRegen = 5f;

	// Token: 0x040001CA RID: 458
	public float m_staminaRegenTimeMultiplier = 1f;

	// Token: 0x040001CB RID: 459
	public float m_staminaRegenDelay = 1f;

	// Token: 0x040001CC RID: 460
	public float m_runStaminaDrain = 10f;

	// Token: 0x040001CD RID: 461
	public float m_sneakStaminaDrain = 5f;

	// Token: 0x040001CE RID: 462
	public float m_swimStaminaDrainMinSkill = 5f;

	// Token: 0x040001CF RID: 463
	public float m_swimStaminaDrainMaxSkill = 2f;

	// Token: 0x040001D0 RID: 464
	public float m_dodgeStaminaUsage = 10f;

	// Token: 0x040001D1 RID: 465
	public float m_eiterRegen = 5f;

	// Token: 0x040001D2 RID: 466
	public float m_eitrRegenDelay = 1f;

	// Token: 0x040001D3 RID: 467
	public float m_autoPickupRange = 2f;

	// Token: 0x040001D4 RID: 468
	public float m_maxCarryWeight = 300f;

	// Token: 0x040001D5 RID: 469
	public float m_encumberedStaminaDrain = 10f;

	// Token: 0x040001D6 RID: 470
	public float m_hardDeathCooldown = 10f;

	// Token: 0x040001D7 RID: 471
	public float m_baseCameraShake = 4f;

	// Token: 0x040001D8 RID: 472
	public float m_placeDelay = 0.4f;

	// Token: 0x040001D9 RID: 473
	public float m_removeDelay = 0.25f;

	// Token: 0x040001DA RID: 474
	public EffectList m_drownEffects = new EffectList();

	// Token: 0x040001DB RID: 475
	public EffectList m_spawnEffects = new EffectList();

	// Token: 0x040001DC RID: 476
	public EffectList m_removeEffects = new EffectList();

	// Token: 0x040001DD RID: 477
	public EffectList m_dodgeEffects = new EffectList();

	// Token: 0x040001DE RID: 478
	public EffectList m_skillLevelupEffects = new EffectList();

	// Token: 0x040001DF RID: 479
	public EffectList m_equipStartEffects = new EffectList();

	// Token: 0x040001E0 RID: 480
	public GameObject m_placeMarker;

	// Token: 0x040001E1 RID: 481
	public GameObject m_tombstone;

	// Token: 0x040001E2 RID: 482
	public GameObject m_valkyrie;

	// Token: 0x040001E3 RID: 483
	public Sprite m_textIcon;

	// Token: 0x040001E4 RID: 484
	public DateTime m_wakeupTime;

	// Token: 0x040001E5 RID: 485
	public float m_baseHP = 25f;

	// Token: 0x040001E6 RID: 486
	public float m_baseStamina = 75f;

	// Token: 0x040001E7 RID: 487
	private Skills m_skills;

	// Token: 0x040001E8 RID: 488
	private PieceTable m_buildPieces;

	// Token: 0x040001E9 RID: 489
	private bool m_noPlacementCost;

	// Token: 0x040001EA RID: 490
	private const bool m_hideUnavailable = false;

	// Token: 0x040001EB RID: 491
	private bool m_enableAutoPickup = true;

	// Token: 0x040001EC RID: 492
	private readonly HashSet<string> m_knownRecipes = new HashSet<string>();

	// Token: 0x040001ED RID: 493
	private readonly Dictionary<string, int> m_knownStations = new Dictionary<string, int>();

	// Token: 0x040001EE RID: 494
	private readonly HashSet<string> m_knownMaterial = new HashSet<string>();

	// Token: 0x040001EF RID: 495
	private readonly HashSet<string> m_shownTutorials = new HashSet<string>();

	// Token: 0x040001F0 RID: 496
	private readonly HashSet<string> m_uniques = new HashSet<string>();

	// Token: 0x040001F1 RID: 497
	private readonly HashSet<string> m_trophies = new HashSet<string>();

	// Token: 0x040001F2 RID: 498
	private readonly HashSet<Heightmap.Biome> m_knownBiome = new HashSet<Heightmap.Biome>();

	// Token: 0x040001F3 RID: 499
	private readonly Dictionary<string, string> m_knownTexts = new Dictionary<string, string>();

	// Token: 0x040001F4 RID: 500
	private float m_stationDiscoverTimer;

	// Token: 0x040001F5 RID: 501
	private bool m_debugFly;

	// Token: 0x040001F6 RID: 502
	private bool m_godMode;

	// Token: 0x040001F7 RID: 503
	private bool m_ghostMode;

	// Token: 0x040001F8 RID: 504
	private float m_lookPitch;

	// Token: 0x040001F9 RID: 505
	private const int m_maxFoods = 3;

	// Token: 0x040001FA RID: 506
	private const float m_foodDrainPerSec = 0.1f;

	// Token: 0x040001FB RID: 507
	private float m_foodUpdateTimer;

	// Token: 0x040001FC RID: 508
	private float m_foodRegenTimer;

	// Token: 0x040001FD RID: 509
	private readonly List<Player.Food> m_foods = new List<Player.Food>();

	// Token: 0x040001FE RID: 510
	private float m_stamina = 100f;

	// Token: 0x040001FF RID: 511
	private float m_maxStamina = 100f;

	// Token: 0x04000200 RID: 512
	private float m_staminaRegenTimer;

	// Token: 0x04000201 RID: 513
	private float m_eitr;

	// Token: 0x04000202 RID: 514
	private float m_maxEitr;

	// Token: 0x04000203 RID: 515
	private float m_eitrRegenTimer;

	// Token: 0x04000204 RID: 516
	private string m_guardianPower = "";

	// Token: 0x04000205 RID: 517
	public float m_guardianPowerCooldown;

	// Token: 0x04000206 RID: 518
	private StatusEffect m_guardianSE;

	// Token: 0x04000207 RID: 519
	private float m_placePressedTime = -1000f;

	// Token: 0x04000208 RID: 520
	private float m_removePressedTime = -1000f;

	// Token: 0x04000209 RID: 521
	private float m_lastToolUseTime;

	// Token: 0x0400020A RID: 522
	private GameObject m_placementMarkerInstance;

	// Token: 0x0400020B RID: 523
	private GameObject m_placementGhost;

	// Token: 0x0400020C RID: 524
	private Player.PlacementStatus m_placementStatus = Player.PlacementStatus.Invalid;

	// Token: 0x0400020D RID: 525
	private int m_placeRotation;

	// Token: 0x0400020E RID: 526
	private int m_placeRayMask;

	// Token: 0x0400020F RID: 527
	private int m_placeGroundRayMask;

	// Token: 0x04000210 RID: 528
	private int m_placeWaterRayMask;

	// Token: 0x04000211 RID: 529
	private int m_removeRayMask;

	// Token: 0x04000212 RID: 530
	private int m_interactMask;

	// Token: 0x04000213 RID: 531
	private int m_autoPickupMask;

	// Token: 0x04000214 RID: 532
	private readonly List<Player.MinorActionData> m_actionQueue = new List<Player.MinorActionData>();

	// Token: 0x04000215 RID: 533
	private float m_actionQueuePause;

	// Token: 0x04000216 RID: 534
	private string m_actionAnimation;

	// Token: 0x04000217 RID: 535
	private GameObject m_hovering;

	// Token: 0x04000218 RID: 536
	private Character m_hoveringCreature;

	// Token: 0x04000219 RID: 537
	private float m_lastHoverInteractTime;

	// Token: 0x0400021A RID: 538
	private bool m_pvp;

	// Token: 0x0400021B RID: 539
	private float m_updateCoverTimer;

	// Token: 0x0400021C RID: 540
	private float m_coverPercentage;

	// Token: 0x0400021D RID: 541
	private bool m_underRoof = true;

	// Token: 0x0400021E RID: 542
	private float m_nearFireTimer;

	// Token: 0x0400021F RID: 543
	private bool m_isLoading;

	// Token: 0x04000220 RID: 544
	private ItemDrop.ItemData m_weaponLoaded;

	// Token: 0x04000221 RID: 545
	private float m_queuedAttackTimer;

	// Token: 0x04000222 RID: 546
	private float m_queuedSecondAttackTimer;

	// Token: 0x04000223 RID: 547
	private float m_queuedDodgeTimer;

	// Token: 0x04000224 RID: 548
	private Vector3 m_queuedDodgeDir = Vector3.zero;

	// Token: 0x04000225 RID: 549
	private bool m_inDodge;

	// Token: 0x04000226 RID: 550
	private bool m_dodgeInvincible;

	// Token: 0x04000227 RID: 551
	private CraftingStation m_currentStation;

	// Token: 0x04000228 RID: 552
	private bool m_inCraftingStation;

	// Token: 0x04000229 RID: 553
	private Ragdoll m_ragdoll;

	// Token: 0x0400022A RID: 554
	private Piece m_hoveringPiece;

	// Token: 0x0400022B RID: 555
	private string m_emoteState = "";

	// Token: 0x0400022C RID: 556
	private int m_emoteID;

	// Token: 0x0400022D RID: 557
	private bool m_intro;

	// Token: 0x0400022E RID: 558
	private bool m_firstSpawn = true;

	// Token: 0x0400022F RID: 559
	private bool m_crouchToggled;

	// Token: 0x04000230 RID: 560
	private bool m_autoRun;

	// Token: 0x04000231 RID: 561
	private bool m_safeInHome;

	// Token: 0x04000232 RID: 562
	private IDoodadController m_doodadController;

	// Token: 0x04000233 RID: 563
	private bool m_attached;

	// Token: 0x04000234 RID: 564
	private string m_attachAnimation = "";

	// Token: 0x04000235 RID: 565
	private bool m_sleeping;

	// Token: 0x04000236 RID: 566
	private bool m_attachedToShip;

	// Token: 0x04000237 RID: 567
	private Transform m_attachPoint;

	// Token: 0x04000238 RID: 568
	private Vector3 m_detachOffset = Vector3.zero;

	// Token: 0x04000239 RID: 569
	private Collider[] m_attachColliders;

	// Token: 0x0400023A RID: 570
	private int m_modelIndex;

	// Token: 0x0400023B RID: 571
	private Vector3 m_skinColor = Vector3.one;

	// Token: 0x0400023C RID: 572
	private Vector3 m_hairColor = Vector3.one;

	// Token: 0x0400023D RID: 573
	private bool m_teleporting;

	// Token: 0x0400023E RID: 574
	private bool m_distantTeleport;

	// Token: 0x0400023F RID: 575
	private float m_teleportTimer;

	// Token: 0x04000240 RID: 576
	private float m_teleportCooldown;

	// Token: 0x04000241 RID: 577
	private Vector3 m_teleportFromPos;

	// Token: 0x04000242 RID: 578
	private Quaternion m_teleportFromRot;

	// Token: 0x04000243 RID: 579
	private Vector3 m_teleportTargetPos;

	// Token: 0x04000244 RID: 580
	private Quaternion m_teleportTargetRot;

	// Token: 0x04000245 RID: 581
	private Heightmap.Biome m_currentBiome;

	// Token: 0x04000246 RID: 582
	private float m_biomeTimer;

	// Token: 0x04000247 RID: 583
	private int m_baseValue;

	// Token: 0x04000248 RID: 584
	private int m_comfortLevel;

	// Token: 0x04000249 RID: 585
	private float m_drownDamageTimer;

	// Token: 0x0400024A RID: 586
	private float m_timeSinceTargeted;

	// Token: 0x0400024B RID: 587
	private float m_timeSinceSensed;

	// Token: 0x0400024C RID: 588
	private float m_stealthFactorUpdateTimer;

	// Token: 0x0400024D RID: 589
	private float m_stealthFactor;

	// Token: 0x0400024E RID: 590
	private float m_stealthFactorTarget;

	// Token: 0x0400024F RID: 591
	private float m_wakeupTimer = -1f;

	// Token: 0x04000250 RID: 592
	private float m_timeSinceDeath = 999999f;

	// Token: 0x04000251 RID: 593
	private float m_runSkillImproveTimer;

	// Token: 0x04000252 RID: 594
	private float m_swimSkillImproveTimer;

	// Token: 0x04000253 RID: 595
	private float m_sneakSkillImproveTimer;

	// Token: 0x04000254 RID: 596
	private float m_equipmentMovementModifier;

	// Token: 0x04000255 RID: 597
	private readonly List<PieceTable> m_tempOwnedPieceTables = new List<PieceTable>();

	// Token: 0x04000256 RID: 598
	private readonly List<Transform> m_tempSnapPoints1 = new List<Transform>();

	// Token: 0x04000257 RID: 599
	private readonly List<Transform> m_tempSnapPoints2 = new List<Transform>();

	// Token: 0x04000258 RID: 600
	private readonly List<Piece> m_tempPieces = new List<Piece>();

	// Token: 0x04000259 RID: 601
	[HideInInspector]
	public Dictionary<string, string> m_customData = new Dictionary<string, string>();

	// Token: 0x0400025A RID: 602
	private static int s_attackMask = 0;

	// Token: 0x0400025B RID: 603
	private static readonly int s_crouching = ZSyncAnimation.GetHash("crouching");

	// Token: 0x0400025C RID: 604
	private static readonly int s_animatorTagDodge = ZSyncAnimation.GetHash("dodge");

	// Token: 0x0400025D RID: 605
	private static readonly int s_animatorTagCutscene = ZSyncAnimation.GetHash("cutscene");

	// Token: 0x0400025E RID: 606
	private static readonly int s_animatorTagCrouch = ZSyncAnimation.GetHash("crouch");

	// Token: 0x0400025F RID: 607
	private static readonly int s_animatorTagMinorAction = ZSyncAnimation.GetHash("minoraction");

	// Token: 0x04000260 RID: 608
	private static readonly int s_animatorTagMinorActionFast = ZSyncAnimation.GetHash("minoraction_fast");

	// Token: 0x04000261 RID: 609
	private static readonly int s_animatorTagEmote = ZSyncAnimation.GetHash("emote");

	// Token: 0x04000262 RID: 610
	private static readonly int s_statusEffectRested = "Rested".GetStableHashCode();

	// Token: 0x04000263 RID: 611
	private static readonly int s_statusEffectEncumbered = "Encumbered".GetStableHashCode();

	// Token: 0x04000264 RID: 612
	private static readonly int s_statusEffectSoftDeath = "SoftDeath".GetStableHashCode();

	// Token: 0x04000265 RID: 613
	private static readonly int s_statusEffectWet = "Wet".GetStableHashCode();

	// Token: 0x04000266 RID: 614
	private static readonly int s_statusEffectShelter = "Shelter".GetStableHashCode();

	// Token: 0x04000267 RID: 615
	private static readonly int s_statusEffectCampFire = "CampFire".GetStableHashCode();

	// Token: 0x04000268 RID: 616
	private static readonly int s_statusEffectResting = "Resting".GetStableHashCode();

	// Token: 0x04000269 RID: 617
	private static readonly int s_statusEffectCold = "Cold".GetStableHashCode();

	// Token: 0x0400026A RID: 618
	private static readonly int s_statusEffectFreezing = "Freezing".GetStableHashCode();

	// Token: 0x0400026B RID: 619
	private int m_cachedFrame;

	// Token: 0x0400026C RID: 620
	private bool m_cachedAttack;

	// Token: 0x02000021 RID: 33
	public enum RequirementMode
	{
		// Token: 0x0400026E RID: 622
		CanBuild,
		// Token: 0x0400026F RID: 623
		IsKnown,
		// Token: 0x04000270 RID: 624
		CanAlmostBuild
	}

	// Token: 0x02000022 RID: 34
	public class Food
	{
		// Token: 0x060002CE RID: 718 RVA: 0x00015F19 File Offset: 0x00014119
		public bool CanEatAgain()
		{
			return this.m_time < this.m_item.m_shared.m_foodBurnTime / 2f;
		}

		// Token: 0x04000271 RID: 625
		public string m_name = "";

		// Token: 0x04000272 RID: 626
		public ItemDrop.ItemData m_item;

		// Token: 0x04000273 RID: 627
		public float m_time;

		// Token: 0x04000274 RID: 628
		public float m_health;

		// Token: 0x04000275 RID: 629
		public float m_stamina;

		// Token: 0x04000276 RID: 630
		public float m_eitr;
	}

	// Token: 0x02000023 RID: 35
	public class MinorActionData
	{
		// Token: 0x04000277 RID: 631
		public Player.MinorActionData.ActionType m_type;

		// Token: 0x04000278 RID: 632
		public ItemDrop.ItemData m_item;

		// Token: 0x04000279 RID: 633
		public string m_progressText = "";

		// Token: 0x0400027A RID: 634
		public float m_time;

		// Token: 0x0400027B RID: 635
		public float m_duration;

		// Token: 0x0400027C RID: 636
		public string m_animation = "";

		// Token: 0x0400027D RID: 637
		public string m_doneAnimation = "";

		// Token: 0x0400027E RID: 638
		public float m_staminaDrain;

		// Token: 0x0400027F RID: 639
		public EffectList m_startEffect;

		// Token: 0x02000024 RID: 36
		public enum ActionType
		{
			// Token: 0x04000281 RID: 641
			Equip,
			// Token: 0x04000282 RID: 642
			Unequip,
			// Token: 0x04000283 RID: 643
			Reload
		}
	}

	// Token: 0x02000025 RID: 37
	private enum PlacementStatus
	{
		// Token: 0x04000285 RID: 645
		Valid,
		// Token: 0x04000286 RID: 646
		Invalid,
		// Token: 0x04000287 RID: 647
		BlockedbyPlayer,
		// Token: 0x04000288 RID: 648
		NoBuildZone,
		// Token: 0x04000289 RID: 649
		PrivateZone,
		// Token: 0x0400028A RID: 650
		MoreSpace,
		// Token: 0x0400028B RID: 651
		NoTeleportArea,
		// Token: 0x0400028C RID: 652
		ExtensionMissingStation,
		// Token: 0x0400028D RID: 653
		WrongBiome,
		// Token: 0x0400028E RID: 654
		NeedCultivated,
		// Token: 0x0400028F RID: 655
		NeedDirt,
		// Token: 0x04000290 RID: 656
		NotInDungeon
	}
}
