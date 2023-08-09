using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Token: 0x0200027A RID: 634
public class PrivateArea : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x0600183D RID: 6205 RVA: 0x000A1ACC File Offset: 0x0009FCCC
	private void Awake()
	{
		if (this.m_areaMarker)
		{
			this.m_areaMarker.m_radius = this.m_radius;
		}
		this.m_nview = base.GetComponent<ZNetView>();
		if (!this.m_nview.IsValid())
		{
			return;
		}
		WearNTear component = base.GetComponent<WearNTear>();
		component.m_onDamaged = (Action)Delegate.Combine(component.m_onDamaged, new Action(this.OnDamaged));
		this.m_piece = base.GetComponent<Piece>();
		if (this.m_areaMarker)
		{
			this.m_areaMarker.gameObject.SetActive(false);
		}
		if (this.m_inRangeEffect)
		{
			this.m_inRangeEffect.SetActive(false);
		}
		PrivateArea.m_allAreas.Add(this);
		base.InvokeRepeating("UpdateStatus", 0f, 1f);
		this.m_nview.Register<long>("ToggleEnabled", new Action<long, long>(this.RPC_ToggleEnabled));
		this.m_nview.Register<long, string>("TogglePermitted", new Action<long, long, string>(this.RPC_TogglePermitted));
		this.m_nview.Register("FlashShield", new Action<long>(this.RPC_FlashShield));
		if (this.m_enabledByDefault && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(ZDOVars.s_enabled, true);
		}
	}

	// Token: 0x0600183E RID: 6206 RVA: 0x000A1C1A File Offset: 0x0009FE1A
	private void OnDestroy()
	{
		PrivateArea.m_allAreas.Remove(this);
	}

	// Token: 0x0600183F RID: 6207 RVA: 0x000A1C28 File Offset: 0x0009FE28
	private void UpdateStatus()
	{
		bool flag = this.IsEnabled();
		this.m_enabledEffect.SetActive(flag);
		this.m_flashAvailable = true;
		foreach (Material material in this.m_model.materials)
		{
			if (flag)
			{
				material.EnableKeyword("_EMISSION");
			}
			else
			{
				material.DisableKeyword("_EMISSION");
			}
		}
	}

	// Token: 0x06001840 RID: 6208 RVA: 0x000A1C88 File Offset: 0x0009FE88
	public string GetHoverText()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		if (Player.m_localPlayer == null)
		{
			return "";
		}
		if (this.m_ownerFaction != Character.Faction.Players)
		{
			return Localization.instance.Localize(this.m_name);
		}
		this.ShowAreaMarker();
		StringBuilder stringBuilder = new StringBuilder(256);
		if (this.m_piece.IsCreator())
		{
			if (this.IsEnabled())
			{
				stringBuilder.Append(this.m_name + " ( $piece_guardstone_active )");
				stringBuilder.Append("\n$piece_guardstone_owner:" + this.GetCreatorName());
				stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_deactivate");
			}
			else
			{
				stringBuilder.Append(this.m_name + " ($piece_guardstone_inactive )");
				stringBuilder.Append("\n$piece_guardstone_owner:" + this.GetCreatorName());
				stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_activate");
			}
		}
		else if (this.IsEnabled())
		{
			stringBuilder.Append(this.m_name + " ( $piece_guardstone_active )");
			stringBuilder.Append("\n$piece_guardstone_owner:" + this.GetCreatorName());
		}
		else
		{
			stringBuilder.Append(this.m_name + " ( $piece_guardstone_inactive )");
			stringBuilder.Append("\n$piece_guardstone_owner:" + this.GetCreatorName());
			if (this.IsPermitted(Player.m_localPlayer.GetPlayerID()))
			{
				stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_remove");
			}
			else
			{
				stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_add");
			}
		}
		this.AddUserList(stringBuilder);
		return Localization.instance.Localize(stringBuilder.ToString());
	}

	// Token: 0x06001841 RID: 6209 RVA: 0x000A1E24 File Offset: 0x000A0024
	private void AddUserList(StringBuilder text)
	{
		List<KeyValuePair<long, string>> permittedPlayers = this.GetPermittedPlayers();
		text.Append("\n$piece_guardstone_additional: ");
		for (int i = 0; i < permittedPlayers.Count; i++)
		{
			text.Append(permittedPlayers[i].Value);
			if (i != permittedPlayers.Count - 1)
			{
				text.Append(", ");
			}
		}
	}

	// Token: 0x06001842 RID: 6210 RVA: 0x000A1E84 File Offset: 0x000A0084
	private void RemovePermitted(long playerID)
	{
		List<KeyValuePair<long, string>> permittedPlayers = this.GetPermittedPlayers();
		if (permittedPlayers.RemoveAll((KeyValuePair<long, string> x) => x.Key == playerID) > 0)
		{
			this.SetPermittedPlayers(permittedPlayers);
			this.m_removedPermittedEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		}
	}

	// Token: 0x06001843 RID: 6211 RVA: 0x000A1EEC File Offset: 0x000A00EC
	private bool IsPermitted(long playerID)
	{
		foreach (KeyValuePair<long, string> keyValuePair in this.GetPermittedPlayers())
		{
			if (keyValuePair.Key == playerID)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001844 RID: 6212 RVA: 0x000A1F4C File Offset: 0x000A014C
	private void AddPermitted(long playerID, string playerName)
	{
		List<KeyValuePair<long, string>> permittedPlayers = this.GetPermittedPlayers();
		foreach (KeyValuePair<long, string> keyValuePair in permittedPlayers)
		{
			if (keyValuePair.Key == playerID)
			{
				return;
			}
		}
		permittedPlayers.Add(new KeyValuePair<long, string>(playerID, playerName));
		this.SetPermittedPlayers(permittedPlayers);
		this.m_addPermittedEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
	}

	// Token: 0x06001845 RID: 6213 RVA: 0x000A1FE4 File Offset: 0x000A01E4
	private void SetPermittedPlayers(List<KeyValuePair<long, string>> users)
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_permitted, users.Count, false);
		for (int i = 0; i < users.Count; i++)
		{
			KeyValuePair<long, string> keyValuePair = users[i];
			this.m_nview.GetZDO().Set("pu_id" + i.ToString(), keyValuePair.Key);
			this.m_nview.GetZDO().Set("pu_name" + i.ToString(), keyValuePair.Value);
		}
	}

	// Token: 0x06001846 RID: 6214 RVA: 0x000A2078 File Offset: 0x000A0278
	private List<KeyValuePair<long, string>> GetPermittedPlayers()
	{
		List<KeyValuePair<long, string>> list = new List<KeyValuePair<long, string>>();
		int @int = this.m_nview.GetZDO().GetInt(ZDOVars.s_permitted, 0);
		for (int i = 0; i < @int; i++)
		{
			long @long = this.m_nview.GetZDO().GetLong("pu_id" + i.ToString(), 0L);
			string @string = this.m_nview.GetZDO().GetString("pu_name" + i.ToString(), "");
			if (@long != 0L)
			{
				list.Add(new KeyValuePair<long, string>(@long, @string));
			}
		}
		return list;
	}

	// Token: 0x06001847 RID: 6215 RVA: 0x000A210C File Offset: 0x000A030C
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06001848 RID: 6216 RVA: 0x000A2114 File Offset: 0x000A0314
	public bool Interact(Humanoid human, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (this.m_ownerFaction != Character.Faction.Players)
		{
			return false;
		}
		Player player = human as Player;
		if (this.m_piece.IsCreator())
		{
			this.m_nview.InvokeRPC("ToggleEnabled", new object[]
			{
				player.GetPlayerID()
			});
			return true;
		}
		if (this.IsEnabled())
		{
			return false;
		}
		this.m_nview.InvokeRPC("TogglePermitted", new object[]
		{
			player.GetPlayerID(),
			player.GetPlayerName()
		});
		return true;
	}

	// Token: 0x06001849 RID: 6217 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x0600184A RID: 6218 RVA: 0x000A21A2 File Offset: 0x000A03A2
	private void RPC_TogglePermitted(long uid, long playerID, string name)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.IsEnabled())
		{
			return;
		}
		if (this.IsPermitted(playerID))
		{
			this.RemovePermitted(playerID);
			return;
		}
		this.AddPermitted(playerID, name);
	}

	// Token: 0x0600184B RID: 6219 RVA: 0x000A21D4 File Offset: 0x000A03D4
	private void RPC_ToggleEnabled(long uid, long playerID)
	{
		ZLog.Log("Toggle enabled from " + playerID.ToString() + "  creator is " + this.m_piece.GetCreator().ToString());
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_piece.GetCreator() != playerID)
		{
			return;
		}
		this.SetEnabled(!this.IsEnabled());
	}

	// Token: 0x0600184C RID: 6220 RVA: 0x000A223B File Offset: 0x000A043B
	private bool IsEnabled()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetBool(ZDOVars.s_enabled, false);
	}

	// Token: 0x0600184D RID: 6221 RVA: 0x000A2264 File Offset: 0x000A0464
	private void SetEnabled(bool enabled)
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_enabled, enabled);
		this.UpdateStatus();
		if (enabled)
		{
			this.m_activateEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
			return;
		}
		this.m_deactivateEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
	}

	// Token: 0x0600184E RID: 6222 RVA: 0x000A22E3 File Offset: 0x000A04E3
	public void Setup(string name)
	{
		this.m_nview.GetZDO().Set(ZDOVars.s_creatorName, name);
	}

	// Token: 0x0600184F RID: 6223 RVA: 0x000A22FC File Offset: 0x000A04FC
	public void PokeAllAreasInRange()
	{
		foreach (PrivateArea privateArea in PrivateArea.m_allAreas)
		{
			if (!(privateArea == this) && this.IsInside(privateArea.transform.position, 0f))
			{
				privateArea.StartInRangeEffect();
			}
		}
	}

	// Token: 0x06001850 RID: 6224 RVA: 0x000A2370 File Offset: 0x000A0570
	private void StartInRangeEffect()
	{
		this.m_inRangeEffect.SetActive(true);
		base.CancelInvoke("StopInRangeEffect");
		base.Invoke("StopInRangeEffect", 0.2f);
	}

	// Token: 0x06001851 RID: 6225 RVA: 0x000A2399 File Offset: 0x000A0599
	private void StopInRangeEffect()
	{
		this.m_inRangeEffect.SetActive(false);
	}

	// Token: 0x06001852 RID: 6226 RVA: 0x000A23A8 File Offset: 0x000A05A8
	public void PokeConnectionEffects()
	{
		List<PrivateArea> connectedAreas = this.GetConnectedAreas(false);
		this.StartConnectionEffects();
		foreach (PrivateArea privateArea in connectedAreas)
		{
			privateArea.StartConnectionEffects();
		}
	}

	// Token: 0x06001853 RID: 6227 RVA: 0x000A2400 File Offset: 0x000A0600
	private void StartConnectionEffects()
	{
		List<PrivateArea> list = new List<PrivateArea>();
		foreach (PrivateArea privateArea in PrivateArea.m_allAreas)
		{
			if (!(privateArea == this) && this.IsInside(privateArea.transform.position, 0f))
			{
				list.Add(privateArea);
			}
		}
		Vector3 vector = base.transform.position + Vector3.up * 1.4f;
		if (this.m_connectionInstances.Count != list.Count)
		{
			this.StopConnectionEffects();
			for (int i = 0; i < list.Count; i++)
			{
				GameObject item = UnityEngine.Object.Instantiate<GameObject>(this.m_connectEffect, vector, Quaternion.identity, base.transform);
				this.m_connectionInstances.Add(item);
			}
		}
		if (this.m_connectionInstances.Count == 0)
		{
			return;
		}
		for (int j = 0; j < list.Count; j++)
		{
			Vector3 vector2 = list[j].transform.position + Vector3.up * 1.4f - vector;
			Quaternion rotation = Quaternion.LookRotation(vector2.normalized);
			GameObject gameObject = this.m_connectionInstances[j];
			gameObject.transform.position = vector;
			gameObject.transform.rotation = rotation;
			gameObject.transform.localScale = new Vector3(1f, 1f, vector2.magnitude);
		}
		base.CancelInvoke("StopConnectionEffects");
		base.Invoke("StopConnectionEffects", 0.3f);
	}

	// Token: 0x06001854 RID: 6228 RVA: 0x000A25B4 File Offset: 0x000A07B4
	private void StopConnectionEffects()
	{
		foreach (GameObject obj in this.m_connectionInstances)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_connectionInstances.Clear();
	}

	// Token: 0x06001855 RID: 6229 RVA: 0x000A2610 File Offset: 0x000A0810
	private string GetCreatorName()
	{
		return this.m_nview.GetZDO().GetString(ZDOVars.s_creatorName, "");
	}

	// Token: 0x06001856 RID: 6230 RVA: 0x000A262C File Offset: 0x000A082C
	public static bool OnObjectDamaged(Vector3 point, Character attacker, bool destroyed)
	{
		foreach (PrivateArea privateArea in PrivateArea.m_allAreas)
		{
			if (privateArea.IsEnabled() && privateArea.IsInside(point, 0f))
			{
				privateArea.OnObjectDamaged(attacker, destroyed);
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001857 RID: 6231 RVA: 0x000A269C File Offset: 0x000A089C
	public static bool CheckAccess(Vector3 point, float radius = 0f, bool flash = true, bool wardCheck = false)
	{
		List<PrivateArea> list = new List<PrivateArea>();
		bool flag = true;
		if (wardCheck)
		{
			flag = true;
			using (List<PrivateArea>.Enumerator enumerator = PrivateArea.m_allAreas.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PrivateArea privateArea = enumerator.Current;
					if (privateArea.IsEnabled() && privateArea.IsInside(point, radius) && !privateArea.HaveLocalAccess())
					{
						flag = false;
						list.Add(privateArea);
					}
				}
				goto IL_B8;
			}
		}
		flag = false;
		foreach (PrivateArea privateArea2 in PrivateArea.m_allAreas)
		{
			if (privateArea2.IsEnabled() && privateArea2.IsInside(point, radius))
			{
				if (privateArea2.HaveLocalAccess())
				{
					flag = true;
				}
				else
				{
					list.Add(privateArea2);
				}
			}
		}
		IL_B8:
		if (!flag && list.Count > 0)
		{
			if (flash)
			{
				foreach (PrivateArea privateArea3 in list)
				{
					privateArea3.FlashShield(false);
				}
			}
			return false;
		}
		return true;
	}

	// Token: 0x06001858 RID: 6232 RVA: 0x000A27CC File Offset: 0x000A09CC
	private bool HaveLocalAccess()
	{
		return this.m_piece.IsCreator() || this.IsPermitted(Player.m_localPlayer.GetPlayerID());
	}

	// Token: 0x06001859 RID: 6233 RVA: 0x000A27F2 File Offset: 0x000A09F2
	private List<PrivateArea> GetConnectedAreas(bool forceUpdate = false)
	{
		if (Time.time - this.m_connectionUpdateTime > this.m_updateConnectionsInterval || forceUpdate)
		{
			this.GetAllConnectedAreas(this.m_connectedAreas);
			this.m_connectionUpdateTime = Time.time;
		}
		return this.m_connectedAreas;
	}

	// Token: 0x0600185A RID: 6234 RVA: 0x000A282C File Offset: 0x000A0A2C
	private void GetAllConnectedAreas(List<PrivateArea> areas)
	{
		Queue<PrivateArea> queue = new Queue<PrivateArea>();
		queue.Enqueue(this);
		foreach (PrivateArea privateArea in PrivateArea.m_allAreas)
		{
			privateArea.m_tempChecked = false;
		}
		this.m_tempChecked = true;
		while (queue.Count > 0)
		{
			PrivateArea privateArea2 = queue.Dequeue();
			foreach (PrivateArea privateArea3 in PrivateArea.m_allAreas)
			{
				if (!privateArea3.m_tempChecked && privateArea3.IsEnabled() && privateArea3.IsInside(privateArea2.transform.position, 0f))
				{
					privateArea3.m_tempChecked = true;
					queue.Enqueue(privateArea3);
					areas.Add(privateArea3);
				}
			}
		}
	}

	// Token: 0x0600185B RID: 6235 RVA: 0x000A291C File Offset: 0x000A0B1C
	private void OnObjectDamaged(Character attacker, bool destroyed)
	{
		this.FlashShield(false);
		if (this.m_ownerFaction != Character.Faction.Players)
		{
			List<Character> list = new List<Character>();
			Character.GetCharactersInRange(base.transform.position, this.m_radius * 2f, list);
			foreach (Character character in list)
			{
				if (character.GetFaction() == this.m_ownerFaction)
				{
					MonsterAI component = character.GetComponent<MonsterAI>();
					if (component)
					{
						component.OnPrivateAreaAttacked(attacker, destroyed);
					}
					NpcTalk component2 = character.GetComponent<NpcTalk>();
					if (component2)
					{
						component2.OnPrivateAreaAttacked(attacker);
					}
				}
			}
		}
	}

	// Token: 0x0600185C RID: 6236 RVA: 0x000A29D8 File Offset: 0x000A0BD8
	private void FlashShield(bool flashConnected)
	{
		if (!this.m_flashAvailable)
		{
			return;
		}
		this.m_flashAvailable = false;
		this.m_nview.InvokeRPC(ZNetView.Everybody, "FlashShield", Array.Empty<object>());
		if (flashConnected)
		{
			foreach (PrivateArea privateArea in this.GetConnectedAreas(false))
			{
				if (privateArea.m_nview.IsValid())
				{
					privateArea.m_nview.InvokeRPC(ZNetView.Everybody, "FlashShield", Array.Empty<object>());
				}
			}
		}
	}

	// Token: 0x0600185D RID: 6237 RVA: 0x000A2A7C File Offset: 0x000A0C7C
	private void RPC_FlashShield(long uid)
	{
		this.m_flashEffect.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
	}

	// Token: 0x0600185E RID: 6238 RVA: 0x000A2AA4 File Offset: 0x000A0CA4
	public static bool InsideFactionArea(Vector3 point, Character.Faction faction)
	{
		foreach (PrivateArea privateArea in PrivateArea.m_allAreas)
		{
			if (privateArea.m_ownerFaction == faction && privateArea.IsInside(point, 0f))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600185F RID: 6239 RVA: 0x000A2B10 File Offset: 0x000A0D10
	private bool IsInside(Vector3 point, float radius)
	{
		return Utils.DistanceXZ(base.transform.position, point) < this.m_radius + radius;
	}

	// Token: 0x06001860 RID: 6240 RVA: 0x000A2B2D File Offset: 0x000A0D2D
	public void ShowAreaMarker()
	{
		if (this.m_areaMarker)
		{
			this.m_areaMarker.gameObject.SetActive(true);
			base.CancelInvoke("HideMarker");
			base.Invoke("HideMarker", 0.5f);
		}
	}

	// Token: 0x06001861 RID: 6241 RVA: 0x000A2B68 File Offset: 0x000A0D68
	private void HideMarker()
	{
		this.m_areaMarker.gameObject.SetActive(false);
	}

	// Token: 0x06001862 RID: 6242 RVA: 0x000A2B7B File Offset: 0x000A0D7B
	private void OnDamaged()
	{
		if (this.IsEnabled())
		{
			this.FlashShield(false);
		}
	}

	// Token: 0x06001863 RID: 6243 RVA: 0x000023E2 File Offset: 0x000005E2
	private void OnDrawGizmosSelected()
	{
	}

	// Token: 0x04001A0D RID: 6669
	public string m_name = "Guard stone";

	// Token: 0x04001A0E RID: 6670
	public float m_radius = 10f;

	// Token: 0x04001A0F RID: 6671
	public float m_updateConnectionsInterval = 5f;

	// Token: 0x04001A10 RID: 6672
	public bool m_enabledByDefault;

	// Token: 0x04001A11 RID: 6673
	public Character.Faction m_ownerFaction;

	// Token: 0x04001A12 RID: 6674
	public GameObject m_enabledEffect;

	// Token: 0x04001A13 RID: 6675
	public CircleProjector m_areaMarker;

	// Token: 0x04001A14 RID: 6676
	public EffectList m_flashEffect = new EffectList();

	// Token: 0x04001A15 RID: 6677
	public EffectList m_activateEffect = new EffectList();

	// Token: 0x04001A16 RID: 6678
	public EffectList m_deactivateEffect = new EffectList();

	// Token: 0x04001A17 RID: 6679
	public EffectList m_addPermittedEffect = new EffectList();

	// Token: 0x04001A18 RID: 6680
	public EffectList m_removedPermittedEffect = new EffectList();

	// Token: 0x04001A19 RID: 6681
	public GameObject m_connectEffect;

	// Token: 0x04001A1A RID: 6682
	public GameObject m_inRangeEffect;

	// Token: 0x04001A1B RID: 6683
	public MeshRenderer m_model;

	// Token: 0x04001A1C RID: 6684
	private ZNetView m_nview;

	// Token: 0x04001A1D RID: 6685
	private Piece m_piece;

	// Token: 0x04001A1E RID: 6686
	private bool m_flashAvailable = true;

	// Token: 0x04001A1F RID: 6687
	private bool m_tempChecked;

	// Token: 0x04001A20 RID: 6688
	private List<GameObject> m_connectionInstances = new List<GameObject>();

	// Token: 0x04001A21 RID: 6689
	private float m_connectionUpdateTime = -1000f;

	// Token: 0x04001A22 RID: 6690
	private List<PrivateArea> m_connectedAreas = new List<PrivateArea>();

	// Token: 0x04001A23 RID: 6691
	private static List<PrivateArea> m_allAreas = new List<PrivateArea>();
}
