using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x020001DE RID: 478
public class RandEventSystem : MonoBehaviour
{
	// Token: 0x170000C9 RID: 201
	// (get) Token: 0x0600138B RID: 5003 RVA: 0x00080FB6 File Offset: 0x0007F1B6
	public static RandEventSystem instance
	{
		get
		{
			return RandEventSystem.m_instance;
		}
	}

	// Token: 0x0600138C RID: 5004 RVA: 0x00080FBD File Offset: 0x0007F1BD
	private void Awake()
	{
		RandEventSystem.m_instance = this;
	}

	// Token: 0x0600138D RID: 5005 RVA: 0x00080FC5 File Offset: 0x0007F1C5
	private void OnDestroy()
	{
		RandEventSystem.m_instance = null;
	}

	// Token: 0x0600138E RID: 5006 RVA: 0x00080FCD File Offset: 0x0007F1CD
	private void Start()
	{
		ZRoutedRpc.instance.Register<string, float, Vector3>("SetEvent", new Action<long, string, float, Vector3>(this.RPC_SetEvent));
	}

	// Token: 0x0600138F RID: 5007 RVA: 0x00080FEC File Offset: 0x0007F1EC
	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		this.UpdateForcedEvents(fixedDeltaTime);
		this.UpdateRandomEvent(fixedDeltaTime);
		if (this.m_forcedEvent != null)
		{
			this.m_forcedEvent.Update(ZNet.instance.IsServer(), this.m_forcedEvent == this.m_activeEvent, true, fixedDeltaTime);
		}
		if (this.m_randomEvent != null && ZNet.instance.IsServer())
		{
			bool playerInArea = this.IsAnyPlayerInEventArea(this.m_randomEvent);
			if (this.m_randomEvent.Update(true, this.m_randomEvent == this.m_activeEvent, playerInArea, fixedDeltaTime))
			{
				this.SetRandomEvent(null, Vector3.zero);
			}
		}
		if (this.m_forcedEvent != null)
		{
			this.SetActiveEvent(this.m_forcedEvent, false);
			return;
		}
		if (this.m_randomEvent == null || !Player.m_localPlayer)
		{
			this.SetActiveEvent(null, false);
			return;
		}
		if (this.IsInsideRandomEventArea(this.m_randomEvent, Player.m_localPlayer.transform.position))
		{
			this.SetActiveEvent(this.m_randomEvent, false);
			return;
		}
		this.SetActiveEvent(null, false);
	}

	// Token: 0x06001390 RID: 5008 RVA: 0x000810EC File Offset: 0x0007F2EC
	private bool IsInsideRandomEventArea(RandomEvent re, Vector3 position)
	{
		return position.y <= 3000f && Utils.DistanceXZ(position, re.m_pos) < this.m_randomEventRange;
	}

	// Token: 0x06001391 RID: 5009 RVA: 0x00081114 File Offset: 0x0007F314
	private void UpdateRandomEvent(float dt)
	{
		if (ZNet.instance.IsServer())
		{
			this.m_eventTimer += dt;
			if (this.m_eventTimer > this.m_eventIntervalMin * 60f)
			{
				this.m_eventTimer = 0f;
				if (UnityEngine.Random.Range(0f, 100f) <= this.m_eventChance)
				{
					this.StartRandomEvent();
				}
			}
			this.m_sendTimer += dt;
			if (this.m_sendTimer > 2f)
			{
				this.m_sendTimer = 0f;
				this.SendCurrentRandomEvent();
			}
		}
	}

	// Token: 0x06001392 RID: 5010 RVA: 0x000811A4 File Offset: 0x0007F3A4
	private void UpdateForcedEvents(float dt)
	{
		this.m_forcedEventUpdateTimer += dt;
		if (this.m_forcedEventUpdateTimer > 2f)
		{
			this.m_forcedEventUpdateTimer = 0f;
			string forcedEvent = this.GetForcedEvent();
			this.SetForcedEvent(forcedEvent);
		}
	}

	// Token: 0x06001393 RID: 5011 RVA: 0x000811E8 File Offset: 0x0007F3E8
	private void SetForcedEvent(string name)
	{
		if (this.m_forcedEvent != null && name != null && this.m_forcedEvent.m_name == name)
		{
			return;
		}
		if (this.m_forcedEvent != null)
		{
			if (this.m_forcedEvent == this.m_activeEvent)
			{
				this.SetActiveEvent(null, true);
			}
			this.m_forcedEvent.OnStop();
			this.m_forcedEvent = null;
		}
		RandomEvent @event = this.GetEvent(name);
		if (@event != null)
		{
			this.m_forcedEvent = @event.Clone();
			this.m_forcedEvent.OnStart();
		}
	}

	// Token: 0x06001394 RID: 5012 RVA: 0x00081268 File Offset: 0x0007F468
	private string GetForcedEvent()
	{
		if (EnemyHud.instance != null)
		{
			Character activeBoss = EnemyHud.instance.GetActiveBoss();
			if (activeBoss != null && activeBoss.m_bossEvent.Length > 0)
			{
				return activeBoss.m_bossEvent;
			}
			string @event = EventZone.GetEvent();
			if (@event != null)
			{
				return @event;
			}
		}
		return null;
	}

	// Token: 0x06001395 RID: 5013 RVA: 0x000812B8 File Offset: 0x0007F4B8
	private void SendCurrentRandomEvent()
	{
		if (this.m_randomEvent != null)
		{
			ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SetEvent", new object[]
			{
				this.m_randomEvent.m_name,
				this.m_randomEvent.m_time,
				this.m_randomEvent.m_pos
			});
			return;
		}
		ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SetEvent", new object[]
		{
			"",
			0f,
			Vector3.zero
		});
	}

	// Token: 0x06001396 RID: 5014 RVA: 0x00081358 File Offset: 0x0007F558
	private void RPC_SetEvent(long sender, string eventName, float time, Vector3 pos)
	{
		if (ZNet.instance.IsServer())
		{
			return;
		}
		if (this.m_randomEvent == null || this.m_randomEvent.m_name != eventName)
		{
			this.SetRandomEventByName(eventName, pos);
		}
		if (this.m_randomEvent != null)
		{
			this.m_randomEvent.m_time = time;
			this.m_randomEvent.m_pos = pos;
		}
	}

	// Token: 0x06001397 RID: 5015 RVA: 0x000813B8 File Offset: 0x0007F5B8
	public void StartRandomEvent()
	{
		if (!ZNet.instance.IsServer())
		{
			return;
		}
		List<KeyValuePair<RandomEvent, Vector3>> possibleRandomEvents = this.GetPossibleRandomEvents();
		ZLog.Log("Possible events:" + possibleRandomEvents.Count.ToString());
		if (possibleRandomEvents.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<RandomEvent, Vector3> keyValuePair in possibleRandomEvents)
		{
			ZLog.DevLog("Event " + keyValuePair.Key.m_name);
		}
		KeyValuePair<RandomEvent, Vector3> keyValuePair2 = possibleRandomEvents[UnityEngine.Random.Range(0, possibleRandomEvents.Count)];
		this.SetRandomEvent(keyValuePair2.Key, keyValuePair2.Value);
	}

	// Token: 0x06001398 RID: 5016 RVA: 0x0008147C File Offset: 0x0007F67C
	private RandomEvent GetEvent(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		foreach (RandomEvent randomEvent in this.m_events)
		{
			if (randomEvent.m_name == name && randomEvent.m_enabled)
			{
				return randomEvent;
			}
		}
		return null;
	}

	// Token: 0x06001399 RID: 5017 RVA: 0x000814F0 File Offset: 0x0007F6F0
	public void SetRandomEventByName(string name, Vector3 pos)
	{
		RandomEvent @event = this.GetEvent(name);
		this.SetRandomEvent(@event, pos);
	}

	// Token: 0x0600139A RID: 5018 RVA: 0x0008150D File Offset: 0x0007F70D
	public void ResetRandomEvent()
	{
		this.SetRandomEvent(null, Vector3.zero);
	}

	// Token: 0x0600139B RID: 5019 RVA: 0x0008151B File Offset: 0x0007F71B
	public bool HaveEvent(string name)
	{
		return this.GetEvent(name) != null;
	}

	// Token: 0x0600139C RID: 5020 RVA: 0x00081528 File Offset: 0x0007F728
	private void SetRandomEvent(RandomEvent ev, Vector3 pos)
	{
		if (this.m_randomEvent != null)
		{
			if (this.m_randomEvent == this.m_activeEvent)
			{
				this.SetActiveEvent(null, true);
			}
			this.m_randomEvent.OnStop();
			this.m_randomEvent = null;
		}
		if (ev != null)
		{
			this.m_randomEvent = ev.Clone();
			this.m_randomEvent.m_pos = pos;
			this.m_randomEvent.OnStart();
			ZLog.Log("Random event set:" + ev.m_name);
			if (Player.m_localPlayer)
			{
				Player.m_localPlayer.ShowTutorial("randomevent", false);
			}
		}
		if (ZNet.instance.IsServer())
		{
			this.SendCurrentRandomEvent();
		}
	}

	// Token: 0x0600139D RID: 5021 RVA: 0x000815D0 File Offset: 0x0007F7D0
	private bool IsAnyPlayerInEventArea(RandomEvent re)
	{
		foreach (ZDO zdo in ZNet.instance.GetAllCharacterZDOS())
		{
			if (this.IsInsideRandomEventArea(re, zdo.GetPosition()))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600139E RID: 5022 RVA: 0x00081638 File Offset: 0x0007F838
	private List<KeyValuePair<RandomEvent, Vector3>> GetPossibleRandomEvents()
	{
		List<KeyValuePair<RandomEvent, Vector3>> list = new List<KeyValuePair<RandomEvent, Vector3>>();
		List<ZDO> allCharacterZDOS = ZNet.instance.GetAllCharacterZDOS();
		foreach (RandomEvent randomEvent in this.m_events)
		{
			if (randomEvent.m_enabled && randomEvent.m_random && this.HaveGlobalKeys(randomEvent))
			{
				List<Vector3> validEventPoints = this.GetValidEventPoints(randomEvent, allCharacterZDOS);
				if (validEventPoints.Count != 0)
				{
					Vector3 value = validEventPoints[UnityEngine.Random.Range(0, validEventPoints.Count)];
					list.Add(new KeyValuePair<RandomEvent, Vector3>(randomEvent, value));
				}
			}
		}
		return list;
	}

	// Token: 0x0600139F RID: 5023 RVA: 0x000816E8 File Offset: 0x0007F8E8
	private List<Vector3> GetValidEventPoints(RandomEvent ev, List<ZDO> characters)
	{
		List<Vector3> list = new List<Vector3>();
		foreach (ZDO zdo in characters)
		{
			if (this.InValidBiome(ev, zdo) && this.CheckBase(ev, zdo) && zdo.GetPosition().y <= 3000f)
			{
				list.Add(zdo.GetPosition());
			}
		}
		return list;
	}

	// Token: 0x060013A0 RID: 5024 RVA: 0x00081768 File Offset: 0x0007F968
	private bool InValidBiome(RandomEvent ev, ZDO zdo)
	{
		if (ev.m_biome == Heightmap.Biome.None)
		{
			return true;
		}
		Vector3 position = zdo.GetPosition();
		return (WorldGenerator.instance.GetBiome(position) & ev.m_biome) != Heightmap.Biome.None;
	}

	// Token: 0x060013A1 RID: 5025 RVA: 0x0008179D File Offset: 0x0007F99D
	private bool CheckBase(RandomEvent ev, ZDO zdo)
	{
		return !ev.m_nearBaseOnly || zdo.GetInt(ZDOVars.s_baseValue, 0) >= 3;
	}

	// Token: 0x060013A2 RID: 5026 RVA: 0x000817BC File Offset: 0x0007F9BC
	private bool HaveGlobalKeys(RandomEvent ev)
	{
		foreach (string name in ev.m_requiredGlobalKeys)
		{
			if (!ZoneSystem.instance.GetGlobalKey(name))
			{
				return false;
			}
		}
		foreach (string name2 in ev.m_notRequiredGlobalKeys)
		{
			if (ZoneSystem.instance.GetGlobalKey(name2))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x060013A3 RID: 5027 RVA: 0x00081868 File Offset: 0x0007FA68
	public List<SpawnSystem.SpawnData> GetCurrentSpawners()
	{
		if (this.m_activeEvent != null)
		{
			return this.m_activeEvent.m_spawn;
		}
		return null;
	}

	// Token: 0x060013A4 RID: 5028 RVA: 0x0008187F File Offset: 0x0007FA7F
	public string GetEnvOverride()
	{
		if (this.m_activeEvent != null && !string.IsNullOrEmpty(this.m_activeEvent.m_forceEnvironment) && this.m_activeEvent.InEventBiome())
		{
			return this.m_activeEvent.m_forceEnvironment;
		}
		return null;
	}

	// Token: 0x060013A5 RID: 5029 RVA: 0x000818B5 File Offset: 0x0007FAB5
	public string GetMusicOverride()
	{
		if (this.m_activeEvent != null && !string.IsNullOrEmpty(this.m_activeEvent.m_forceMusic))
		{
			return this.m_activeEvent.m_forceMusic;
		}
		return null;
	}

	// Token: 0x060013A6 RID: 5030 RVA: 0x000818E0 File Offset: 0x0007FAE0
	private void SetActiveEvent(RandomEvent ev, bool end = false)
	{
		if (ev != null && this.m_activeEvent != null && ev.m_name == this.m_activeEvent.m_name)
		{
			return;
		}
		if (this.m_activeEvent != null)
		{
			this.m_activeEvent.OnDeactivate(end);
			this.m_activeEvent = null;
		}
		if (ev != null)
		{
			this.m_activeEvent = ev;
			if (this.m_activeEvent != null)
			{
				this.m_activeEvent.OnActivate();
			}
		}
	}

	// Token: 0x060013A7 RID: 5031 RVA: 0x00081949 File Offset: 0x0007FB49
	public static bool InEvent()
	{
		return !(RandEventSystem.m_instance == null) && RandEventSystem.m_instance.m_activeEvent != null;
	}

	// Token: 0x060013A8 RID: 5032 RVA: 0x00081967 File Offset: 0x0007FB67
	public static bool HaveActiveEvent()
	{
		return !(RandEventSystem.m_instance == null) && (RandEventSystem.m_instance.m_activeEvent != null || RandEventSystem.m_instance.m_randomEvent != null || RandEventSystem.m_instance.m_activeEvent != null);
	}

	// Token: 0x060013A9 RID: 5033 RVA: 0x000819A1 File Offset: 0x0007FBA1
	public RandomEvent GetCurrentRandomEvent()
	{
		return this.m_randomEvent;
	}

	// Token: 0x060013AA RID: 5034 RVA: 0x000819A9 File Offset: 0x0007FBA9
	public RandomEvent GetActiveEvent()
	{
		return this.m_activeEvent;
	}

	// Token: 0x060013AB RID: 5035 RVA: 0x000819B4 File Offset: 0x0007FBB4
	public void PrepareSave()
	{
		this.m_tempSaveEventTimer = this.m_eventTimer;
		if (this.m_randomEvent != null)
		{
			this.m_tempSaveRandomEvent = this.m_randomEvent.m_name;
			this.m_tempSaveRandomEventTime = this.m_randomEvent.m_time;
			this.m_tempSaveRandomEventPos = this.m_randomEvent.m_pos;
			return;
		}
		this.m_tempSaveRandomEvent = "";
		this.m_tempSaveRandomEventTime = 0f;
		this.m_tempSaveRandomEventPos = Vector3.zero;
	}

	// Token: 0x060013AC RID: 5036 RVA: 0x00081A2C File Offset: 0x0007FC2C
	public void SaveAsync(BinaryWriter writer)
	{
		writer.Write(this.m_tempSaveEventTimer);
		writer.Write(this.m_tempSaveRandomEvent);
		writer.Write(this.m_tempSaveRandomEventTime);
		writer.Write(this.m_tempSaveRandomEventPos.x);
		writer.Write(this.m_tempSaveRandomEventPos.y);
		writer.Write(this.m_tempSaveRandomEventPos.z);
	}

	// Token: 0x060013AD RID: 5037 RVA: 0x00081A90 File Offset: 0x0007FC90
	public void Load(BinaryReader reader, int version)
	{
		this.m_eventTimer = reader.ReadSingle();
		if (version >= 25)
		{
			string text = reader.ReadString();
			float time = reader.ReadSingle();
			Vector3 pos;
			pos.x = reader.ReadSingle();
			pos.y = reader.ReadSingle();
			pos.z = reader.ReadSingle();
			if (!string.IsNullOrEmpty(text))
			{
				this.SetRandomEventByName(text, pos);
				if (this.m_randomEvent != null)
				{
					this.m_randomEvent.m_time = time;
					this.m_randomEvent.m_pos = pos;
				}
			}
		}
	}

	// Token: 0x04001477 RID: 5239
	private static RandEventSystem m_instance;

	// Token: 0x04001478 RID: 5240
	public float m_eventIntervalMin = 1f;

	// Token: 0x04001479 RID: 5241
	public float m_eventChance = 25f;

	// Token: 0x0400147A RID: 5242
	public float m_randomEventRange = 200f;

	// Token: 0x0400147B RID: 5243
	public List<RandomEvent> m_events = new List<RandomEvent>();

	// Token: 0x0400147C RID: 5244
	private float m_eventTimer;

	// Token: 0x0400147D RID: 5245
	private float m_sendTimer;

	// Token: 0x0400147E RID: 5246
	private RandomEvent m_randomEvent;

	// Token: 0x0400147F RID: 5247
	private float m_forcedEventUpdateTimer;

	// Token: 0x04001480 RID: 5248
	private RandomEvent m_forcedEvent;

	// Token: 0x04001481 RID: 5249
	private RandomEvent m_activeEvent;

	// Token: 0x04001482 RID: 5250
	private float m_tempSaveEventTimer;

	// Token: 0x04001483 RID: 5251
	private string m_tempSaveRandomEvent;

	// Token: 0x04001484 RID: 5252
	private float m_tempSaveRandomEventTime;

	// Token: 0x04001485 RID: 5253
	private Vector3 m_tempSaveRandomEventPos;
}
