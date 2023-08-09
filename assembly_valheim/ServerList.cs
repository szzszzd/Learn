using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000F1 RID: 241
public class ServerList : MonoBehaviour
{
	// Token: 0x17000056 RID: 86
	// (get) Token: 0x060009C0 RID: 2496 RVA: 0x0004A1AD File Offset: 0x000483AD
	public bool currentServerListIsLocal
	{
		get
		{
			return this.currentServerList == ServerListType.recent || this.currentServerList == ServerListType.favorite;
		}
	}

	// Token: 0x17000057 RID: 87
	// (get) Token: 0x060009C1 RID: 2497 RVA: 0x0004A1C3 File Offset: 0x000483C3
	private List<ServerStatus> CurrentServerListFiltered
	{
		get
		{
			if (this.filteredListOutdated)
			{
				this.FilterList();
				this.filteredListOutdated = false;
			}
			return this.m_filteredList;
		}
	}

	// Token: 0x060009C2 RID: 2498 RVA: 0x0004A1E0 File Offset: 0x000483E0
	private static string GetServerListFolder(FileHelpers.FileSource fileSource)
	{
		if (fileSource != FileHelpers.FileSource.Local)
		{
			return "/serverlist/";
		}
		return "/serverlist_local/";
	}

	// Token: 0x060009C3 RID: 2499 RVA: 0x0004A1F1 File Offset: 0x000483F1
	private static string GetServerListFolderPath(FileHelpers.FileSource fileSource)
	{
		return Utils.GetSaveDataPath(fileSource) + ServerList.GetServerListFolder(fileSource);
	}

	// Token: 0x060009C4 RID: 2500 RVA: 0x0004A204 File Offset: 0x00048404
	private static string GetFavoriteListFile(FileHelpers.FileSource fileSource)
	{
		return ServerList.GetServerListFolderPath(fileSource) + "favorite";
	}

	// Token: 0x060009C5 RID: 2501 RVA: 0x0004A216 File Offset: 0x00048416
	private static string GetRecentListFile(FileHelpers.FileSource fileSource)
	{
		return ServerList.GetServerListFolderPath(fileSource) + "recent";
	}

	// Token: 0x060009C6 RID: 2502 RVA: 0x0004A228 File Offset: 0x00048428
	private void Awake()
	{
		this.InitializeIfNot();
	}

	// Token: 0x060009C7 RID: 2503 RVA: 0x0004A230 File Offset: 0x00048430
	private void OnEnable()
	{
		if (ServerList.instance != null && ServerList.instance != this)
		{
			ZLog.LogError("More than one instance of ServerList!");
			return;
		}
		ServerList.instance = this;
		this.OnServerListTab();
	}

	// Token: 0x060009C8 RID: 2504 RVA: 0x0004A263 File Offset: 0x00048463
	private void OnDestroy()
	{
		if (ServerList.instance != this)
		{
			ZLog.LogError("ServerList instance was not this!");
			return;
		}
		ServerList.instance = null;
		this.FlushLocalServerLists();
	}

	// Token: 0x060009C9 RID: 2505 RVA: 0x0004A28C File Offset: 0x0004848C
	private void Update()
	{
		if (this.m_addServerPanel.activeInHierarchy)
		{
			this.m_addServerConfirmButton.interactable = (this.m_addServerTextInput.text.Length > 0 && !this.isAwaitingServerAdd);
			this.m_addServerCancelButton.interactable = !this.isAwaitingServerAdd;
		}
		ServerListType serverListType = this.currentServerList;
		if (serverListType - ServerListType.favorite > 1)
		{
			if (serverListType - ServerListType.friends <= 1 && Time.timeAsDouble >= this.serverListLastUpdatedTime + 0.5)
			{
				this.UpdateMatchmakingServerList();
				this.UpdateServerCount();
			}
		}
		else if (Time.timeAsDouble >= this.serverListLastUpdatedTime + 0.5)
		{
			this.UpdateLocalServerListStatus();
			this.UpdateServerCount();
		}
		if (!base.GetComponent<UIGamePad>().IsBlocked())
		{
			this.UpdateGamepad();
			this.UpdateKeyboard();
		}
		this.m_serverRefreshButton.interactable = (Time.time - this.m_lastServerListRequesTime > 1f);
		if (this.buttonsOutdated)
		{
			this.buttonsOutdated = false;
			this.UpdateButtons();
		}
	}

	// Token: 0x060009CA RID: 2506 RVA: 0x0004A38C File Offset: 0x0004858C
	private void InitializeIfNot()
	{
		if (this.initialized)
		{
			return;
		}
		this.initialized = true;
		this.m_favoriteButton.onClick.AddListener(delegate
		{
			this.OnFavoriteServerButton();
		});
		this.m_removeButton.onClick.AddListener(delegate
		{
			this.OnRemoveServerButton();
		});
		this.m_upButton.onClick.AddListener(delegate
		{
			this.OnMoveServerUpButton();
		});
		this.m_downButton.onClick.AddListener(delegate
		{
			this.OnMoveServerDownButton();
		});
		this.m_filterInputField.onValueChanged.AddListener(delegate(string _)
		{
			this.OnServerFilterChanged(true);
		});
		this.m_addServerButton.gameObject.SetActive(true);
		if (PlayerPrefs.HasKey("LastIPJoined"))
		{
			PlayerPrefs.DeleteKey("LastIPJoined");
		}
		this.m_serverListBaseSize = this.m_serverListRoot.rect.height;
		this.OnServerListTab();
	}

	// Token: 0x060009CB RID: 2507 RVA: 0x0004A47C File Offset: 0x0004867C
	public static uint[] FairSplit(uint[] entryCounts, uint maxEntries)
	{
		uint num = 0U;
		uint num2 = 0U;
		for (int i = 0; i < entryCounts.Length; i++)
		{
			num += entryCounts[i];
			if (entryCounts[i] > 0U)
			{
				num2 += 1U;
			}
		}
		if (num <= maxEntries)
		{
			return entryCounts;
		}
		uint[] array = new uint[entryCounts.Length];
		while (num2 > 0U)
		{
			uint num3 = maxEntries / num2;
			if (num3 <= 0U)
			{
				uint num4 = 0U;
				int num5 = 0;
				while ((long)num5 < (long)((ulong)maxEntries))
				{
					if (entryCounts[(int)num4] > 0U)
					{
						array[(int)num4] += 1U;
					}
					else
					{
						num5--;
					}
					num4 += 1U;
					num5++;
				}
				maxEntries = 0U;
				break;
			}
			for (int j = 0; j < entryCounts.Length; j++)
			{
				if (entryCounts[j] > 0U)
				{
					if (entryCounts[j] > num3)
					{
						array[j] += num3;
						maxEntries -= num3;
						entryCounts[j] -= num3;
					}
					else
					{
						array[j] += entryCounts[j];
						maxEntries -= entryCounts[j];
						entryCounts[j] = 0U;
						num2 -= 1U;
					}
				}
			}
		}
		return array;
	}

	// Token: 0x060009CC RID: 2508 RVA: 0x0004A578 File Offset: 0x00048778
	public void FilterList()
	{
		if (this.currentServerListIsLocal)
		{
			List<ServerStatus> list;
			if (this.currentServerList == ServerListType.favorite)
			{
				list = this.m_favoriteServerList;
			}
			else
			{
				if (this.currentServerList != ServerListType.recent)
				{
					ZLog.LogError("Can't filter invalid server list!");
					return;
				}
				list = this.m_recentServerList;
			}
			this.m_filteredList = new List<ServerStatus>();
			for (int i = 0; i < list.Count; i++)
			{
				if (this.m_filterInputField.text.Length <= 0 || list[i].m_joinData.m_serverName.ToLowerInvariant().Contains(this.m_filterInputField.text.ToLowerInvariant()))
				{
					this.m_filteredList.Add(list[i]);
				}
			}
			return;
		}
		List<ServerStatus> list2 = new List<ServerStatus>();
		if (this.currentServerList == ServerListType.community)
		{
			for (int j = 0; j < this.m_crossplayMatchmakingServerList.Count; j++)
			{
				if (this.m_filterInputField.text.Length <= 0 || this.m_crossplayMatchmakingServerList[j].m_joinData.m_serverName.ToLowerInvariant().Contains(this.m_filterInputField.text.ToLowerInvariant()))
				{
					list2.Add(this.m_crossplayMatchmakingServerList[j]);
				}
			}
		}
		uint[] array = ServerList.FairSplit(new uint[]
		{
			(uint)list2.Count,
			(uint)this.m_steamMatchmakingServerList.Count
		}, 200U);
		this.m_filteredList = new List<ServerStatus>();
		if (array[0] > 0U)
		{
			this.m_filteredList.AddRange(list2.GetRange(0, (int)array[0]));
		}
		if (array[1] > 0U)
		{
			int num = 0;
			while (num < this.m_steamMatchmakingServerList.Count && (long)this.m_filteredList.Count < 200L)
			{
				if (this.m_steamMatchmakingServerList[num].IsCrossplay)
				{
					bool flag = false;
					for (int k = 0; k < this.m_filteredList.Count; k++)
					{
						if (this.m_steamMatchmakingServerList[num].m_joinData == this.m_filteredList[k].m_joinData)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						this.m_filteredList.Add(this.m_steamMatchmakingServerList[num]);
					}
				}
				else
				{
					this.m_filteredList.Add(this.m_steamMatchmakingServerList[num]);
				}
				num++;
			}
		}
		this.m_filteredList.Sort((ServerStatus a, ServerStatus b) => a.m_joinData.m_serverName.CompareTo(b.m_joinData.m_serverName));
	}

	// Token: 0x060009CD RID: 2509 RVA: 0x0004A7FC File Offset: 0x000489FC
	private void UpdateButtons()
	{
		int selectedServer = this.GetSelectedServer();
		bool flag = selectedServer >= 0;
		bool flag2 = false;
		if (flag)
		{
			for (int i = 0; i < this.m_favoriteServerList.Count; i++)
			{
				if (this.m_favoriteServerList[i].m_joinData == this.CurrentServerListFiltered[selectedServer].m_joinData)
				{
					flag2 = true;
					break;
				}
			}
		}
		switch (this.currentServerList)
		{
		case ServerListType.favorite:
			this.m_upButton.interactable = (flag && selectedServer != 0);
			this.m_downButton.interactable = (flag && selectedServer != this.CurrentServerListFiltered.Count - 1);
			this.m_removeButton.interactable = flag;
			this.m_favoriteButton.interactable = (flag && (this.m_removeButton == null || !this.m_removeButton.gameObject.activeSelf));
			break;
		case ServerListType.recent:
			this.m_favoriteButton.interactable = (flag && !flag2);
			this.m_removeButton.interactable = flag;
			break;
		case ServerListType.friends:
		case ServerListType.community:
			this.m_favoriteButton.interactable = (flag && !flag2);
			break;
		}
		this.m_joinGameButton.interactable = flag;
	}

	// Token: 0x060009CE RID: 2510 RVA: 0x0004A94C File Offset: 0x00048B4C
	public void OnFavoriteServersTab()
	{
		this.InitializeIfNot();
		if (this.currentServerList == ServerListType.favorite)
		{
			return;
		}
		this.currentServerList = ServerListType.favorite;
		this.m_filterInputField.text = "";
		this.OnServerFilterChanged(false);
		if (this.m_doneInitialServerListRequest)
		{
			PlayerPrefs.SetInt("serverListTab", this.m_serverListTabHandler.GetActiveTab());
		}
		this.ResetListManipulationButtons();
		this.m_removeButton.gameObject.SetActive(true);
		this.UpdateLocalServerListStatus();
		this.UpdateLocalServerListSelection();
	}

	// Token: 0x060009CF RID: 2511 RVA: 0x0004A9C8 File Offset: 0x00048BC8
	public void OnRecentServersTab()
	{
		this.InitializeIfNot();
		if (this.currentServerList == ServerListType.recent)
		{
			return;
		}
		this.currentServerList = ServerListType.recent;
		this.m_filterInputField.text = "";
		this.OnServerFilterChanged(false);
		if (this.m_doneInitialServerListRequest)
		{
			PlayerPrefs.SetInt("serverListTab", this.m_serverListTabHandler.GetActiveTab());
		}
		this.ResetListManipulationButtons();
		this.m_favoriteButton.gameObject.SetActive(true);
		this.UpdateLocalServerListStatus();
		this.UpdateLocalServerListSelection();
	}

	// Token: 0x060009D0 RID: 2512 RVA: 0x0004AA44 File Offset: 0x00048C44
	public void OnFriendsServersTab()
	{
		this.InitializeIfNot();
		if (this.currentServerList == ServerListType.friends)
		{
			return;
		}
		this.currentServerList = ServerListType.friends;
		if (this.m_doneInitialServerListRequest)
		{
			PlayerPrefs.SetInt("serverListTab", this.m_serverListTabHandler.GetActiveTab());
		}
		this.ResetListManipulationButtons();
		this.m_favoriteButton.gameObject.SetActive(true);
		this.m_filterInputField.text = "";
		this.OnServerFilterChanged(false);
		this.UpdateMatchmakingServerList();
		this.UpdateServerListGui(true);
		this.UpdateServerCount();
	}

	// Token: 0x060009D1 RID: 2513 RVA: 0x0004AAC8 File Offset: 0x00048CC8
	public void OnCommunityServersTab()
	{
		this.InitializeIfNot();
		if (this.currentServerList == ServerListType.community)
		{
			return;
		}
		this.currentServerList = ServerListType.community;
		if (this.m_doneInitialServerListRequest)
		{
			PlayerPrefs.SetInt("serverListTab", this.m_serverListTabHandler.GetActiveTab());
		}
		this.ResetListManipulationButtons();
		this.m_favoriteButton.gameObject.SetActive(true);
		this.m_filterInputField.text = "";
		this.OnServerFilterChanged(false);
		this.UpdateMatchmakingServerList();
		this.UpdateServerListGui(true);
		this.UpdateServerCount();
	}

	// Token: 0x060009D2 RID: 2514 RVA: 0x0004AB4C File Offset: 0x00048D4C
	public void OnFavoriteServerButton()
	{
		if ((this.m_removeButton == null || !this.m_removeButton.gameObject.activeSelf) && this.currentServerList == ServerListType.favorite)
		{
			this.OnRemoveServerButton();
			return;
		}
		int selectedServer = this.GetSelectedServer();
		ServerStatus item = this.CurrentServerListFiltered[selectedServer];
		this.m_favoriteServerList.Add(item);
		this.SetButtonsOutdated();
	}

	// Token: 0x060009D3 RID: 2515 RVA: 0x0004ABB0 File Offset: 0x00048DB0
	public void OnRemoveServerButton()
	{
		int selectedServer = this.GetSelectedServer();
		UnifiedPopup.Push(new YesNoPopup("$menu_removeserver", CensorShittyWords.FilterUGC(this.CurrentServerListFiltered[selectedServer].m_joinData.m_serverName, UGCType.ServerName), delegate()
		{
			this.OnRemoveServerConfirm();
		}, delegate()
		{
			UnifiedPopup.Pop();
		}, true));
	}

	// Token: 0x060009D4 RID: 2516 RVA: 0x0004AC1C File Offset: 0x00048E1C
	public void OnMoveServerUpButton()
	{
		List<ServerStatus> favoriteServerList = this.m_favoriteServerList;
		int selectedServer = this.GetSelectedServer();
		ServerStatus value = favoriteServerList[selectedServer - 1];
		favoriteServerList[selectedServer - 1] = favoriteServerList[selectedServer];
		favoriteServerList[selectedServer] = value;
		this.filteredListOutdated = true;
		this.UpdateServerListGui(true);
	}

	// Token: 0x060009D5 RID: 2517 RVA: 0x0004AC68 File Offset: 0x00048E68
	public void OnMoveServerDownButton()
	{
		List<ServerStatus> favoriteServerList = this.m_favoriteServerList;
		int selectedServer = this.GetSelectedServer();
		ServerStatus value = favoriteServerList[selectedServer + 1];
		favoriteServerList[selectedServer + 1] = favoriteServerList[selectedServer];
		favoriteServerList[selectedServer] = value;
		this.filteredListOutdated = true;
		this.UpdateServerListGui(true);
	}

	// Token: 0x060009D6 RID: 2518 RVA: 0x0004ACB4 File Offset: 0x00048EB4
	private void OnRemoveServerConfirm()
	{
		if (this.currentServerList == ServerListType.favorite)
		{
			List<ServerStatus> favoriteServerList = this.m_favoriteServerList;
			int selectedServer = this.GetSelectedServer();
			ServerStatus item = this.CurrentServerListFiltered[selectedServer];
			int index = favoriteServerList.IndexOf(item);
			favoriteServerList.RemoveAt(index);
			this.filteredListOutdated = true;
			if (this.CurrentServerListFiltered.Count <= 0 && this.m_filterInputField.text != "")
			{
				this.m_filterInputField.text = "";
				this.OnServerFilterChanged(false);
				this.m_startup.SetServerToJoin(null);
			}
			else
			{
				this.UpdateLocalServerListSelection();
				this.SetSelectedServer(selectedServer, true);
			}
			UnifiedPopup.Pop();
			return;
		}
		ZLog.LogError("Can't remove server from invalid list!");
	}

	// Token: 0x060009D7 RID: 2519 RVA: 0x0004AD68 File Offset: 0x00048F68
	private void ResetListManipulationButtons()
	{
		this.m_favoriteButton.gameObject.SetActive(false);
		this.m_removeButton.gameObject.SetActive(false);
		this.m_favoriteButton.interactable = false;
		this.m_upButton.interactable = false;
		this.m_downButton.interactable = false;
		this.m_removeButton.interactable = false;
	}

	// Token: 0x060009D8 RID: 2520 RVA: 0x0004ADC7 File Offset: 0x00048FC7
	private void SetButtonsOutdated()
	{
		this.buttonsOutdated = true;
	}

	// Token: 0x060009D9 RID: 2521 RVA: 0x0004ADD0 File Offset: 0x00048FD0
	private void UpdateServerListGui(bool centerSelection)
	{
		new List<ServerStatus>();
		List<ServerList.ServerListElement> list = new List<ServerList.ServerListElement>();
		Dictionary<ServerJoinData, ServerList.ServerListElement> dictionary = new Dictionary<ServerJoinData, ServerList.ServerListElement>();
		for (int i = 0; i < this.m_serverListElements.Count; i++)
		{
			ServerList.ServerListElement serverListElement;
			if (dictionary.TryGetValue(this.m_serverListElements[i].m_serverStatus.m_joinData, out serverListElement))
			{
				ZLog.LogWarning("Join data " + this.m_serverListElements[i].m_serverStatus.m_joinData.ToString() + " already has a server list element, even though duplicates are not allowed! Discarding this element.\nWhile this warning itself is fine, it might be an indication of a bug that may cause navigation issues in the server list.");
				UnityEngine.Object.Destroy(this.m_serverListElements[i].m_element);
			}
			else
			{
				dictionary.Add(this.m_serverListElements[i].m_serverStatus.m_joinData, this.m_serverListElements[i]);
			}
		}
		float num = 0f;
		for (int j = 0; j < this.CurrentServerListFiltered.Count; j++)
		{
			ServerList.ServerListElement serverListElement2;
			if (dictionary.ContainsKey(this.CurrentServerListFiltered[j].m_joinData))
			{
				serverListElement2 = dictionary[this.CurrentServerListFiltered[j].m_joinData];
				list.Add(serverListElement2);
				dictionary.Remove(this.CurrentServerListFiltered[j].m_joinData);
			}
			else
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_serverListElementSteamCrossplay, this.m_serverListRoot);
				gameObject.SetActive(true);
				serverListElement2 = new ServerList.ServerListElement(gameObject, this.CurrentServerListFiltered[j]);
				ServerStatus selectedStatus = this.CurrentServerListFiltered[j];
				serverListElement2.m_button.onClick.AddListener(delegate
				{
					this.OnSelectedServer(selectedStatus);
				});
				list.Add(serverListElement2);
			}
			serverListElement2.m_rectTransform.anchoredPosition = new Vector2(0f, -num);
			num += serverListElement2.m_rectTransform.sizeDelta.y;
			ServerStatus serverStatus = this.CurrentServerListFiltered[j];
			serverListElement2.m_serverName.text = CensorShittyWords.FilterUGC(serverStatus.m_joinData.m_serverName, UGCType.ServerName);
			serverListElement2.m_tooltip.m_text = serverStatus.m_joinData.ToString();
			if (serverStatus.m_joinData is ServerJoinDataSteamUser)
			{
				UITooltip tooltip = serverListElement2.m_tooltip;
				tooltip.m_text += " (Steam)";
			}
			if (serverStatus.m_joinData is ServerJoinDataPlayFabUser)
			{
				serverListElement2.m_tooltip.m_text = "(PlayFab)";
			}
			if (serverStatus.m_joinData is ServerJoinDataDedicated)
			{
				UITooltip tooltip2 = serverListElement2.m_tooltip;
				tooltip2.m_text += " (Dedicated)";
			}
			if (serverStatus.IsJoinable || serverStatus.PlatformRestriction == PrivilegeManager.Platform.Unknown)
			{
				serverListElement2.m_version.text = serverStatus.m_gameVersion;
				if (serverStatus.OnlineStatus == OnlineStatus.Online)
				{
					serverListElement2.m_players.text = serverStatus.m_playerCount.ToString() + " / " + this.m_serverPlayerLimit.ToString();
				}
				else
				{
					serverListElement2.m_players.text = "";
				}
				switch (serverStatus.PingStatus)
				{
				case ServerPingStatus.NotStarted:
					serverListElement2.m_status.sprite = this.connectUnknown;
					break;
				case ServerPingStatus.AwaitingResponse:
					serverListElement2.m_status.sprite = this.connectTrying;
					break;
				case ServerPingStatus.Success:
					serverListElement2.m_status.sprite = this.connectSuccess;
					break;
				case ServerPingStatus.TimedOut:
				case ServerPingStatus.CouldNotReach:
				case ServerPingStatus.Unpingable:
					goto IL_356;
				default:
					goto IL_356;
				}
				IL_368:
				if (serverListElement2.m_crossplay != null)
				{
					if (serverStatus.IsCrossplay)
					{
						serverListElement2.m_crossplay.gameObject.SetActive(true);
					}
					else
					{
						serverListElement2.m_crossplay.gameObject.SetActive(false);
					}
				}
				serverListElement2.m_private.gameObject.SetActive(serverStatus.m_isPasswordProtected);
				goto IL_427;
				IL_356:
				serverListElement2.m_status.sprite = this.connectFailed;
				goto IL_368;
			}
			serverListElement2.m_version.text = "";
			serverListElement2.m_players.text = "";
			serverListElement2.m_status.sprite = this.connectFailed;
			if (serverListElement2.m_crossplay != null)
			{
				serverListElement2.m_crossplay.gameObject.SetActive(false);
			}
			serverListElement2.m_private.gameObject.SetActive(false);
			IL_427:
			bool flag = this.m_startup.HasServerToJoin() && this.m_startup.GetServerToJoin().Equals(serverStatus.m_joinData);
			if (flag)
			{
				this.m_startup.SetServerToJoin(serverStatus);
			}
			serverListElement2.m_selected.gameObject.SetActive(flag);
			if (centerSelection && flag)
			{
				this.m_serverListEnsureVisible.CenterOnItem(serverListElement2.m_selected);
			}
		}
		foreach (KeyValuePair<ServerJoinData, ServerList.ServerListElement> keyValuePair in dictionary)
		{
			UnityEngine.Object.Destroy(keyValuePair.Value.m_element);
		}
		this.m_serverListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(num, this.m_serverListBaseSize));
		this.m_serverListElements = list;
		this.SetButtonsOutdated();
	}

	// Token: 0x060009DA RID: 2522 RVA: 0x0004B2F4 File Offset: 0x000494F4
	private void UpdateServerCount()
	{
		int num = 0;
		if (this.currentServerListIsLocal)
		{
			num += this.CurrentServerListFiltered.Count;
		}
		else
		{
			num += ZSteamMatchmaking.instance.GetTotalNrOfServers();
			num += this.m_crossplayMatchmakingServerList.Count;
		}
		int num2 = 0;
		for (int i = 0; i < this.CurrentServerListFiltered.Count; i++)
		{
			if (this.CurrentServerListFiltered[i].PingStatus != ServerPingStatus.NotStarted && this.CurrentServerListFiltered[i].PingStatus != ServerPingStatus.AwaitingResponse)
			{
				num2++;
			}
		}
		this.m_serverCount.text = num2.ToString() + " / " + num.ToString();
	}

	// Token: 0x060009DB RID: 2523 RVA: 0x0004B39C File Offset: 0x0004959C
	private void OnSelectedServer(ServerStatus selected)
	{
		this.m_startup.SetServerToJoin(selected);
		this.UpdateServerListGui(false);
	}

	// Token: 0x060009DC RID: 2524 RVA: 0x0004B3B4 File Offset: 0x000495B4
	private void SetSelectedServer(int index, bool centerSelection)
	{
		if (this.CurrentServerListFiltered.Count == 0)
		{
			if (this.m_startup.HasServerToJoin())
			{
				ZLog.Log("Serverlist is empty, clearing selection");
			}
			this.ClearSelectedServer();
			return;
		}
		index = Mathf.Clamp(index, 0, this.CurrentServerListFiltered.Count - 1);
		this.m_startup.SetServerToJoin(this.CurrentServerListFiltered[index]);
		this.UpdateServerListGui(centerSelection);
	}

	// Token: 0x060009DD RID: 2525 RVA: 0x0004B420 File Offset: 0x00049620
	private int GetSelectedServer()
	{
		if (!this.m_startup.HasServerToJoin())
		{
			return -1;
		}
		for (int i = 0; i < this.CurrentServerListFiltered.Count; i++)
		{
			if (this.m_startup.GetServerToJoin() == this.CurrentServerListFiltered[i].m_joinData)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x060009DE RID: 2526 RVA: 0x0004B478 File Offset: 0x00049678
	private void ClearSelectedServer()
	{
		this.m_startup.SetServerToJoin(null);
		this.SetButtonsOutdated();
	}

	// Token: 0x060009DF RID: 2527 RVA: 0x0004B48C File Offset: 0x0004968C
	private int FindSelectedServer(GameObject button)
	{
		for (int i = 0; i < this.m_serverListElements.Count; i++)
		{
			if (this.m_serverListElements[i].m_element == button)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x060009E0 RID: 2528 RVA: 0x0004B4CC File Offset: 0x000496CC
	private void UpdateLocalServerListStatus()
	{
		this.serverListLastUpdatedTime = Time.timeAsDouble;
		List<ServerStatus> list;
		if (this.currentServerList == ServerListType.favorite)
		{
			list = this.m_favoriteServerList;
		}
		else
		{
			if (this.currentServerList != ServerListType.recent)
			{
				ZLog.LogError("Can't update status of invalid server list!");
				return;
			}
			list = this.m_recentServerList;
		}
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].PingStatus != ServerPingStatus.Success && list[i].PingStatus != ServerPingStatus.CouldNotReach)
			{
				if (list[i].PingStatus == ServerPingStatus.NotStarted)
				{
					list[i].Ping();
					flag = true;
				}
				if (list[i].PingStatus == ServerPingStatus.AwaitingResponse && list[i].TryGetResult())
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			this.UpdateServerListGui(false);
			this.UpdateServerCount();
		}
	}

	// Token: 0x060009E1 RID: 2529 RVA: 0x0004B590 File Offset: 0x00049790
	private void UpdateMatchmakingServerList()
	{
		this.serverListLastUpdatedTime = Time.timeAsDouble;
		if (this.m_serverListRevision == ZSteamMatchmaking.instance.GetServerListRevision())
		{
			return;
		}
		this.m_serverListRevision = ZSteamMatchmaking.instance.GetServerListRevision();
		this.m_steamMatchmakingServerList.Clear();
		ZSteamMatchmaking.instance.GetServers(this.m_steamMatchmakingServerList);
		if (!this.currentServerListIsLocal && this.m_whenToSearchPlayFab >= 0f && this.m_whenToSearchPlayFab <= Time.time)
		{
			this.m_whenToSearchPlayFab = -1f;
			this.RequestPlayFabServerList();
		}
		bool flag = false;
		this.filteredListOutdated = true;
		for (int i = 0; i < this.CurrentServerListFiltered.Count; i++)
		{
			if (this.CurrentServerListFiltered[i].m_joinData == this.m_startup.GetServerToJoin())
			{
				flag = true;
				break;
			}
		}
		if (this.m_startup.HasServerToJoin() && !flag)
		{
			ZLog.Log("Serverlist does not contain selected server, clearing");
			if (this.CurrentServerListFiltered.Count > 0)
			{
				this.SetSelectedServer(0, true);
			}
			else
			{
				this.ClearSelectedServer();
			}
		}
		this.UpdateServerListGui(false);
		this.UpdateServerCount();
	}

	// Token: 0x060009E2 RID: 2530 RVA: 0x0004B6A4 File Offset: 0x000498A4
	private void UpdateLocalServerListSelection()
	{
		if (this.GetSelectedServer() < 0)
		{
			this.ClearSelectedServer();
			this.UpdateServerListGui(true);
		}
	}

	// Token: 0x060009E3 RID: 2531 RVA: 0x0004B6BC File Offset: 0x000498BC
	public void OnServerListTab()
	{
		if (PlayerPrefs.HasKey("publicfilter"))
		{
			PlayerPrefs.DeleteKey("publicfilter");
		}
		int @int = PlayerPrefs.GetInt("serverListTab", 0);
		this.m_serverListTabHandler.SetActiveTab(@int);
		if (!this.m_doneInitialServerListRequest)
		{
			this.m_doneInitialServerListRequest = true;
			this.RequestServerList();
		}
		this.UpdateServerListGui(true);
		this.m_filterInputField.ActivateInputField();
	}

	// Token: 0x060009E4 RID: 2532 RVA: 0x0004B71E File Offset: 0x0004991E
	public void OnRefreshButton()
	{
		this.RequestServerList();
		this.UpdateServerListGui(true);
		this.UpdateServerCount();
	}

	// Token: 0x060009E5 RID: 2533 RVA: 0x0004B733 File Offset: 0x00049933
	public static void Refresh()
	{
		if (ServerList.instance == null)
		{
			return;
		}
		ServerList.instance.OnRefreshButton();
	}

	// Token: 0x060009E6 RID: 2534 RVA: 0x0004B74D File Offset: 0x0004994D
	public static void UpdateServerListGuiStatic()
	{
		if (ServerList.instance == null)
		{
			return;
		}
		ServerList.instance.UpdateServerListGui(false);
	}

	// Token: 0x060009E7 RID: 2535 RVA: 0x0004B768 File Offset: 0x00049968
	private void RequestPlayFabServerListIfUnchangedIn(float time)
	{
		if (time < 0f)
		{
			this.m_whenToSearchPlayFab = -1f;
			this.RequestPlayFabServerList();
			return;
		}
		this.m_whenToSearchPlayFab = Time.time + time;
	}

	// Token: 0x060009E8 RID: 2536 RVA: 0x0004B794 File Offset: 0x00049994
	private void RequestPlayFabServerList()
	{
		if (!PlayFabManager.IsLoggedIn)
		{
			this.m_playFabServerSearchQueued = true;
			if (PlayFabManager.instance != null)
			{
				PlayFabManager.instance.LoginFinished += delegate(LoginType loginType)
				{
					this.RequestPlayFabServerList();
				};
				return;
			}
		}
		else
		{
			if (this.m_playFabServerSearchOngoing)
			{
				this.m_playFabServerSearchQueued = true;
				return;
			}
			this.m_playFabServerSearchQueued = false;
			this.m_playFabServerSearchOngoing = true;
			ZPlayFabMatchmaking.ListServers(this.m_filterInputField.text, new ZPlayFabMatchmakingSuccessCallback(this.PlayFabServerFound), new ZPlayFabMatchmakingFailedCallback(this.PlayFabServerSearchDone), this.currentServerList == ServerListType.friends);
			ZLog.DevLog("PlayFab server search started!");
		}
	}

	// Token: 0x060009E9 RID: 2537 RVA: 0x0004B82C File Offset: 0x00049A2C
	public void PlayFabServerFound(PlayFabMatchmakingServerData serverData)
	{
		MonoBehaviour.print("Found PlayFab server with name: " + serverData.serverName);
		if (this.PlayFabDisplayEntry(serverData))
		{
			PlayFabMatchmakingServerData playFabMatchmakingServerData;
			if (this.m_playFabTemporarySearchServerList.TryGetValue(serverData, out playFabMatchmakingServerData))
			{
				if (serverData.tickCreated > playFabMatchmakingServerData.tickCreated)
				{
					this.m_playFabTemporarySearchServerList.Remove(serverData);
					this.m_playFabTemporarySearchServerList.Add(serverData, serverData);
					return;
				}
			}
			else
			{
				this.m_playFabTemporarySearchServerList.Add(serverData, serverData);
			}
		}
	}

	// Token: 0x060009EA RID: 2538 RVA: 0x0004B89D File Offset: 0x00049A9D
	private bool PlayFabDisplayEntry(PlayFabMatchmakingServerData serverData)
	{
		return serverData != null && this.currentServerList == ServerListType.community;
	}

	// Token: 0x060009EB RID: 2539 RVA: 0x0004B8B0 File Offset: 0x00049AB0
	public void PlayFabServerSearchDone(ZPLayFabMatchmakingFailReason failedReason)
	{
		ZLog.DevLog("PlayFab server search done!");
		if (this.m_playFabServerSearchQueued)
		{
			this.m_playFabServerSearchQueued = false;
			this.m_playFabServerSearchOngoing = true;
			this.m_playFabTemporarySearchServerList.Clear();
			ZPlayFabMatchmaking.ListServers(this.m_filterInputField.text, new ZPlayFabMatchmakingSuccessCallback(this.PlayFabServerFound), new ZPlayFabMatchmakingFailedCallback(this.PlayFabServerSearchDone), this.currentServerList == ServerListType.friends);
			ZLog.DevLog("PlayFab server search started!");
			return;
		}
		this.m_playFabServerSearchOngoing = false;
		this.m_crossplayMatchmakingServerList.Clear();
		foreach (KeyValuePair<PlayFabMatchmakingServerData, PlayFabMatchmakingServerData> keyValuePair in this.m_playFabTemporarySearchServerList)
		{
			ServerStatus serverStatus;
			if (keyValuePair.Value.isDedicatedServer && !string.IsNullOrEmpty(keyValuePair.Value.serverIp))
			{
				ServerJoinDataDedicated serverJoinDataDedicated = new ServerJoinDataDedicated(keyValuePair.Value.serverIp);
				if (serverJoinDataDedicated.IsValid())
				{
					serverStatus = new ServerStatus(serverJoinDataDedicated);
				}
				else
				{
					ZLog.Log("Dedicated server with invalid IP address - fallback to PlayFab ID");
					serverStatus = new ServerStatus(new ServerJoinDataPlayFabUser(keyValuePair.Value.remotePlayerId));
				}
			}
			else
			{
				serverStatus = new ServerStatus(new ServerJoinDataPlayFabUser(keyValuePair.Value.remotePlayerId));
			}
			GameVersion lhs;
			if (GameVersion.TryParseGameVersion(keyValuePair.Value.gameVersion, out lhs))
			{
				PrivilegeManager.Platform platformRestriction;
				if (lhs >= global::Version.FirstVersionWithPlatformRestriction)
				{
					platformRestriction = PrivilegeManager.ParsePlatform(keyValuePair.Value.platformRestriction);
				}
				else
				{
					platformRestriction = PrivilegeManager.Platform.None;
				}
				serverStatus.UpdateStatus(OnlineStatus.Online, keyValuePair.Value.serverName, keyValuePair.Value.numPlayers, keyValuePair.Value.gameVersion, keyValuePair.Value.networkVersion, keyValuePair.Value.havePassword, platformRestriction, true);
				this.m_crossplayMatchmakingServerList.Add(serverStatus);
			}
			else
			{
				ZLog.LogWarning("Failed to parse version string! Skipping server entry with name \"" + serverStatus.m_joinData.m_serverName + "\".");
			}
		}
		this.m_playFabTemporarySearchServerList.Clear();
		this.filteredListOutdated = true;
	}

	// Token: 0x060009EC RID: 2540 RVA: 0x0004BACC File Offset: 0x00049CCC
	public void RequestServerList()
	{
		ZLog.DevLog("Request serverlist");
		if (!this.m_serverRefreshButton.interactable)
		{
			ZLog.DevLog("Server queue already running");
			return;
		}
		this.m_serverRefreshButton.interactable = false;
		this.m_lastServerListRequesTime = Time.time;
		this.m_steamMatchmakingServerList.Clear();
		ZSteamMatchmaking.instance.RequestServerlist();
		this.RequestPlayFabServerListIfUnchangedIn(0f);
		this.ReloadLocalServerLists();
		this.filteredListOutdated = true;
		if (this.currentServerListIsLocal)
		{
			this.UpdateLocalServerListStatus();
		}
	}

	// Token: 0x060009ED RID: 2541 RVA: 0x0004BB50 File Offset: 0x00049D50
	private void ReloadLocalServerLists()
	{
		if (!this.m_localServerListsLoaded)
		{
			this.LoadServerListFromDisk(ServerListType.favorite, ref this.m_favoriteServerList);
			this.LoadServerListFromDisk(ServerListType.recent, ref this.m_recentServerList);
			this.m_localServerListsLoaded = true;
			return;
		}
		foreach (ServerStatus serverStatus in this.m_allLoadedServerData.Values)
		{
			serverStatus.Reset();
		}
	}

	// Token: 0x060009EE RID: 2542 RVA: 0x0004BBD4 File Offset: 0x00049DD4
	public void FlushLocalServerLists()
	{
		if (!this.m_localServerListsLoaded)
		{
			return;
		}
		ServerList.SaveServerListToDisk(ServerListType.favorite, this.m_favoriteServerList);
		ServerList.SaveServerListToDisk(ServerListType.recent, this.m_recentServerList);
		this.m_favoriteServerList.Clear();
		this.m_recentServerList.Clear();
		this.m_allLoadedServerData.Clear();
		this.m_localServerListsLoaded = false;
		this.filteredListOutdated = true;
	}

	// Token: 0x060009EF RID: 2543 RVA: 0x0004BC34 File Offset: 0x00049E34
	public void OnServerFilterChanged(bool isTyping = false)
	{
		ZSteamMatchmaking.instance.SetNameFilter(this.m_filterInputField.text);
		ZSteamMatchmaking.instance.SetFriendFilter(this.currentServerList == ServerListType.friends);
		if (!this.currentServerListIsLocal)
		{
			this.RequestPlayFabServerListIfUnchangedIn(isTyping ? 0.5f : 0f);
		}
		this.filteredListOutdated = true;
		if (this.currentServerListIsLocal)
		{
			this.UpdateServerListGui(false);
			this.UpdateServerCount();
		}
	}

	// Token: 0x060009F0 RID: 2544 RVA: 0x0004BCA4 File Offset: 0x00049EA4
	private void UpdateGamepad()
	{
		if (!ZInput.IsGamepadActive())
		{
			return;
		}
		if (ZInput.GetButtonDown("JoyLStickDown") || ZInput.GetButtonDown("JoyDPadDown"))
		{
			this.SetSelectedServer(this.GetSelectedServer() + 1, true);
		}
		if (ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyDPadUp"))
		{
			this.SetSelectedServer(this.GetSelectedServer() - 1, true);
		}
	}

	// Token: 0x060009F1 RID: 2545 RVA: 0x0004BD08 File Offset: 0x00049F08
	private void UpdateKeyboard()
	{
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			this.SetSelectedServer(this.GetSelectedServer() - 1, true);
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			this.SetSelectedServer(this.GetSelectedServer() + 1, true);
		}
		int num = 0;
		num += (Input.GetKeyDown(KeyCode.W) ? -1 : 0);
		num += (Input.GetKeyDown(KeyCode.S) ? 1 : 0);
		int selectedServer = this.GetSelectedServer();
		if (num != 0 && !this.m_filterInputField.isFocused && this.m_favoriteServerList.Count == this.m_filteredList.Count && this.currentServerList == ServerListType.favorite && selectedServer >= 0 && selectedServer + num >= 0 && selectedServer + num < this.m_favoriteServerList.Count)
		{
			if (num > 0)
			{
				this.OnMoveServerDownButton();
				return;
			}
			this.OnMoveServerUpButton();
		}
	}

	// Token: 0x060009F2 RID: 2546 RVA: 0x0004BDD0 File Offset: 0x00049FD0
	public static void AddToRecentServersList(ServerJoinData data)
	{
		if (ServerList.instance != null)
		{
			ServerList.instance.AddToRecentServersListCached(data);
			return;
		}
		if (data == null)
		{
			ZLog.LogError("Couldn't add server to server list, server data was null");
			return;
		}
		List<ServerJoinData> list = new List<ServerJoinData>();
		if (!ServerList.LoadServerListFromDisk(ServerListType.recent, ref list))
		{
			ZLog.Log("Server list doesn't exist yet");
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == data)
			{
				list.RemoveAt(i);
				i--;
			}
		}
		list.Insert(0, data);
		int num = (ServerList.maxRecentServers > 0) ? Mathf.Max(list.Count - ServerList.maxRecentServers, 0) : 0;
		for (int j = 0; j < num; j++)
		{
			list.RemoveAt(list.Count - 1);
		}
		ServerList.SaveStatusCode saveStatusCode = ServerList.SaveServerListToDisk(ServerListType.recent, list);
		if (saveStatusCode == ServerList.SaveStatusCode.Succeess)
		{
			ZLog.Log("Added server with name " + data.m_serverName + " to server list");
			return;
		}
		switch (saveStatusCode)
		{
		case ServerList.SaveStatusCode.UnsupportedServerListType:
			ZLog.LogError("Couln't add server with name " + data.m_serverName + " to server list, tried to save an unsupported server list type");
			return;
		case ServerList.SaveStatusCode.UnknownServerBackend:
			ZLog.LogError("Couln't add server with name " + data.m_serverName + " to server list, tried to save a server entry with an unknown server backend");
			return;
		case ServerList.SaveStatusCode.CloudQuotaExceeded:
			ZLog.LogWarning("Couln't add server with name " + data.m_serverName + " to server list, cloud quota exceeded.");
			return;
		default:
			ZLog.LogError("Couln't add server with name " + data.m_serverName + " to server list, unknown issue when saving to disk");
			return;
		}
	}

	// Token: 0x060009F3 RID: 2547 RVA: 0x0004BF40 File Offset: 0x0004A140
	private void AddToRecentServersListCached(ServerJoinData data)
	{
		if (data == null)
		{
			ZLog.LogError("Couldn't add server to server list, server data was null");
			return;
		}
		ServerStatus serverStatus = null;
		for (int i = 0; i < this.m_recentServerList.Count; i++)
		{
			if (this.m_recentServerList[i].m_joinData == data)
			{
				serverStatus = this.m_recentServerList[i];
				this.m_recentServerList.RemoveAt(i);
				i--;
			}
		}
		if (serverStatus == null)
		{
			ServerStatus item;
			if (this.m_allLoadedServerData.TryGetValue(data, out item))
			{
				this.m_recentServerList.Insert(0, item);
			}
			else
			{
				ServerStatus serverStatus2 = new ServerStatus(data);
				this.m_allLoadedServerData.Add(data, serverStatus2);
				this.m_recentServerList.Insert(0, serverStatus2);
			}
		}
		else
		{
			this.m_recentServerList.Insert(0, serverStatus);
		}
		int num = (ServerList.maxRecentServers > 0) ? Mathf.Max(this.m_recentServerList.Count - ServerList.maxRecentServers, 0) : 0;
		for (int j = 0; j < num; j++)
		{
			this.m_recentServerList.RemoveAt(this.m_recentServerList.Count - 1);
		}
		ZLog.Log("Added server with name " + data.m_serverName + " to server list");
	}

	// Token: 0x060009F4 RID: 2548 RVA: 0x0004C06C File Offset: 0x0004A26C
	public bool LoadServerListFromDisk(ServerListType listType, ref List<ServerStatus> list)
	{
		List<ServerJoinData> list2 = new List<ServerJoinData>();
		if (!ServerList.LoadServerListFromDisk(listType, ref list2))
		{
			return false;
		}
		list.Clear();
		for (int i = 0; i < list2.Count; i++)
		{
			ServerStatus item;
			if (this.m_allLoadedServerData.TryGetValue(list2[i], out item))
			{
				list.Add(item);
			}
			else
			{
				ServerStatus serverStatus = new ServerStatus(list2[i]);
				this.m_allLoadedServerData.Add(list2[i], serverStatus);
				list.Add(serverStatus);
			}
		}
		return true;
	}

	// Token: 0x060009F5 RID: 2549 RVA: 0x0004C0EC File Offset: 0x0004A2EC
	private static List<ServerList.StorageLocation> GetServerListFileLocations(ServerListType listType)
	{
		List<ServerList.StorageLocation> list = new List<ServerList.StorageLocation>();
		switch (listType)
		{
		case ServerListType.favorite:
			list.Add(new ServerList.StorageLocation(ServerList.GetFavoriteListFile(FileHelpers.FileSource.Local), FileHelpers.FileSource.Local));
			if (FileHelpers.m_cloudEnabled)
			{
				list.Add(new ServerList.StorageLocation(ServerList.GetFavoriteListFile(FileHelpers.FileSource.Cloud), FileHelpers.FileSource.Cloud));
				return list;
			}
			return list;
		case ServerListType.recent:
			list.Add(new ServerList.StorageLocation(ServerList.GetRecentListFile(FileHelpers.FileSource.Local), FileHelpers.FileSource.Local));
			if (FileHelpers.m_cloudEnabled)
			{
				list.Add(new ServerList.StorageLocation(ServerList.GetRecentListFile(FileHelpers.FileSource.Cloud), FileHelpers.FileSource.Cloud));
				return list;
			}
			return list;
		}
		return null;
	}

	// Token: 0x060009F6 RID: 2550 RVA: 0x0004C178 File Offset: 0x0004A378
	private static bool LoadUniqueServerListEntriesIntoList(ServerList.StorageLocation location, ref List<ServerJoinData> joinData)
	{
		HashSet<ServerJoinData> hashSet = new HashSet<ServerJoinData>();
		for (int i = 0; i < joinData.Count; i++)
		{
			hashSet.Add(joinData[i]);
		}
		FileReader fileReader;
		try
		{
			fileReader = new FileReader(location.path, location.source, FileHelpers.FileHelperType.Binary);
		}
		catch (Exception ex)
		{
			ZLog.Log(string.Concat(new string[]
			{
				"Failed to load: ",
				location.path,
				" (",
				ex.Message,
				")"
			}));
			return false;
		}
		byte[] data;
		try
		{
			BinaryReader binary = fileReader.m_binary;
			int count = binary.ReadInt32();
			data = binary.ReadBytes(count);
		}
		catch (Exception ex2)
		{
			ZLog.LogError(string.Format("error loading player.dat. Source: {0}, Path: {1}, Error: {2}", location.source, location.path, ex2.Message));
			fileReader.Dispose();
			return false;
		}
		fileReader.Dispose();
		ZPackage zpackage = new ZPackage(data);
		uint num = zpackage.ReadUInt();
		if (num == 0U || num == 1U)
		{
			int num2 = zpackage.ReadInt();
			int j = 0;
			while (j < num2)
			{
				string text = zpackage.ReadString();
				string serverName = zpackage.ReadString();
				if (text != null)
				{
					ServerJoinData serverJoinData;
					if (!(text == "Steam user"))
					{
						if (!(text == "PlayFab user"))
						{
							if (!(text == "Dedicated"))
							{
								goto IL_197;
							}
							serverJoinData = ((num == 0U) ? new ServerJoinDataDedicated(zpackage.ReadUInt(), (ushort)zpackage.ReadUInt()) : new ServerJoinDataDedicated(zpackage.ReadString(), (ushort)zpackage.ReadUInt()));
						}
						else
						{
							serverJoinData = new ServerJoinDataPlayFabUser(zpackage.ReadString());
						}
					}
					else
					{
						serverJoinData = new ServerJoinDataSteamUser(zpackage.ReadULong());
					}
					if (serverJoinData != null)
					{
						serverJoinData.m_serverName = serverName;
						if (!hashSet.Contains(serverJoinData))
						{
							joinData.Add(serverJoinData);
						}
					}
					j++;
					continue;
				}
				IL_197:
				ZLog.LogError("Unsupported backend! This should be an impossible code path if the server list was saved and loaded properly.");
				return false;
			}
			return true;
		}
		ZLog.LogError("Couldn't read list of version " + num.ToString());
		return false;
	}

	// Token: 0x060009F7 RID: 2551 RVA: 0x0004C398 File Offset: 0x0004A598
	public static bool LoadServerListFromDisk(ServerListType listType, ref List<ServerJoinData> destination)
	{
		List<ServerList.StorageLocation> serverListFileLocations = ServerList.GetServerListFileLocations(listType);
		if (serverListFileLocations == null)
		{
			ZLog.LogError("Can't load a server list of unsupported type");
			return false;
		}
		for (int i = 0; i < serverListFileLocations.Count; i++)
		{
			if (!FileHelpers.Exists(serverListFileLocations[i].path, serverListFileLocations[i].source))
			{
				serverListFileLocations.RemoveAt(i);
				i--;
			}
		}
		if (serverListFileLocations.Count <= 0)
		{
			ZLog.Log("No list saved! Aborting load operation");
			return false;
		}
		SortedList<DateTime, List<ServerList.StorageLocation>> sortedList = new SortedList<DateTime, List<ServerList.StorageLocation>>();
		for (int j = 0; j < serverListFileLocations.Count; j++)
		{
			DateTime lastWriteTime = FileHelpers.GetLastWriteTime(serverListFileLocations[j].path, serverListFileLocations[j].source);
			if (sortedList.ContainsKey(lastWriteTime))
			{
				sortedList[lastWriteTime].Add(serverListFileLocations[j]);
			}
			else
			{
				sortedList.Add(lastWriteTime, new List<ServerList.StorageLocation>
				{
					serverListFileLocations[j]
				});
			}
		}
		List<ServerJoinData> list = new List<ServerJoinData>();
		for (int k = sortedList.Count - 1; k >= 0; k--)
		{
			for (int l = 0; l < sortedList.Values[k].Count; l++)
			{
				if (!ServerList.LoadUniqueServerListEntriesIntoList(sortedList.Values[k][l], ref list))
				{
					ZLog.Log("Failed to load list entries! Aborting load operation.");
					return false;
				}
			}
		}
		destination = list;
		return true;
	}

	// Token: 0x060009F8 RID: 2552 RVA: 0x0004C4F4 File Offset: 0x0004A6F4
	public static ServerList.SaveStatusCode SaveServerListToDisk(ServerListType listType, List<ServerStatus> list)
	{
		List<ServerJoinData> list2 = new List<ServerJoinData>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			list2.Add(list[i].m_joinData);
		}
		return ServerList.SaveServerListToDisk(listType, list2);
	}

	// Token: 0x060009F9 RID: 2553 RVA: 0x0004C538 File Offset: 0x0004A738
	private static ServerList.SaveStatusCode SaveServerListEntries(ServerList.StorageLocation location, List<ServerJoinData> list)
	{
		string oldFile = location.path + ".old";
		string text = location.path + ".new";
		ZPackage zpackage = new ZPackage();
		zpackage.Write(1U);
		zpackage.Write(list.Count);
		int i = 0;
		while (i < list.Count)
		{
			ServerJoinData serverJoinData = list[i];
			zpackage.Write(serverJoinData.GetDataName());
			zpackage.Write(serverJoinData.m_serverName);
			string dataName = serverJoinData.GetDataName();
			if (dataName != null)
			{
				if (!(dataName == "Steam user"))
				{
					if (!(dataName == "PlayFab user"))
					{
						if (!(dataName == "Dedicated"))
						{
							goto IL_FB;
						}
						zpackage.Write((serverJoinData as ServerJoinDataDedicated).m_host);
						zpackage.Write((uint)(serverJoinData as ServerJoinDataDedicated).m_port);
					}
					else
					{
						zpackage.Write((serverJoinData as ServerJoinDataPlayFabUser).m_remotePlayerId.ToString());
					}
				}
				else
				{
					zpackage.Write((ulong)(serverJoinData as ServerJoinDataSteamUser).m_joinUserID);
				}
				i++;
				continue;
			}
			IL_FB:
			ZLog.LogError("Unsupported backend! Aborting save operation.");
			return ServerList.SaveStatusCode.UnknownServerBackend;
		}
		if (FileHelpers.m_cloudEnabled && location.source == FileHelpers.FileSource.Cloud)
		{
			ulong num = 0UL;
			if (FileHelpers.FileExistsCloud(location.path))
			{
				num += FileHelpers.GetFileSize(location.path, location.source);
			}
			num = Math.Max((ulong)(4L + (long)zpackage.Size()), num);
			num *= 2UL;
			if (FileHelpers.OperationExceedsCloudCapacity(num))
			{
				ZLog.LogWarning("Saving server list to cloud would exceed the cloud storage quota. Therefore the operation has been aborted!");
				return ServerList.SaveStatusCode.CloudQuotaExceeded;
			}
		}
		byte[] array = zpackage.GetArray();
		FileWriter fileWriter = new FileWriter(text, FileHelpers.FileHelperType.Binary, location.source);
		fileWriter.m_binary.Write(array.Length);
		fileWriter.m_binary.Write(array);
		fileWriter.Finish();
		FileHelpers.ReplaceOldFile(location.path, text, oldFile, location.source);
		return ServerList.SaveStatusCode.Succeess;
	}

	// Token: 0x060009FA RID: 2554 RVA: 0x0004C70C File Offset: 0x0004A90C
	public static ServerList.SaveStatusCode SaveServerListToDisk(ServerListType listType, List<ServerJoinData> list)
	{
		List<ServerList.StorageLocation> serverListFileLocations = ServerList.GetServerListFileLocations(listType);
		if (serverListFileLocations == null)
		{
			ZLog.LogError("Can't save a server list of unsupported type");
			return ServerList.SaveStatusCode.UnsupportedServerListType;
		}
		bool flag = false;
		bool flag2 = false;
		int i = 0;
		while (i < serverListFileLocations.Count)
		{
			switch (ServerList.SaveServerListEntries(serverListFileLocations[i], list))
			{
			case ServerList.SaveStatusCode.Succeess:
				flag = true;
				break;
			case ServerList.SaveStatusCode.UnsupportedServerListType:
				goto IL_4E;
			case ServerList.SaveStatusCode.UnknownServerBackend:
				break;
			case ServerList.SaveStatusCode.CloudQuotaExceeded:
				flag2 = true;
				break;
			default:
				goto IL_4E;
			}
			IL_58:
			i++;
			continue;
			IL_4E:
			ZLog.LogError("Unknown error when saving server list");
			goto IL_58;
		}
		if (flag)
		{
			return ServerList.SaveStatusCode.Succeess;
		}
		if (flag2)
		{
			return ServerList.SaveStatusCode.CloudQuotaExceeded;
		}
		return ServerList.SaveStatusCode.FailedUnknownReason;
	}

	// Token: 0x060009FB RID: 2555 RVA: 0x0004C789 File Offset: 0x0004A989
	public void OnAddServerOpen()
	{
		this.m_addServerPanel.SetActive(true);
		this.m_addServerTextInput.ActivateInputField();
	}

	// Token: 0x060009FC RID: 2556 RVA: 0x0004C7A2 File Offset: 0x0004A9A2
	public void OnAddServerClose()
	{
		this.m_addServerPanel.SetActive(false);
	}

	// Token: 0x060009FD RID: 2557 RVA: 0x0004C7B0 File Offset: 0x0004A9B0
	public void OnAddServer()
	{
		this.m_addServerPanel.SetActive(true);
		string text = this.m_addServerTextInput.text;
		string[] array = text.Split(new char[]
		{
			':'
		});
		if (array.Length == 0)
		{
			return;
		}
		if (array.Length == 1)
		{
			string text2 = array[0];
			if (ZPlayFabMatchmaking.IsJoinCode(text2))
			{
				if (PlayFabManager.IsLoggedIn)
				{
					this.OnManualAddToFavoritesStart();
					ZPlayFabMatchmaking.ResolveJoinCode(text2, new ZPlayFabMatchmakingSuccessCallback(this.OnPlayFabJoinCodeSuccess), new ZPlayFabMatchmakingFailedCallback(this.OnJoinCodeFailed));
					return;
				}
				this.OnJoinCodeFailed(ZPLayFabMatchmakingFailReason.NotLoggedIn);
				return;
			}
		}
		if (array.Length == 1 || array.Length == 2)
		{
			ServerJoinDataDedicated newServerListEntryDedicated = new ServerJoinDataDedicated(text);
			this.OnManualAddToFavoritesStart();
			newServerListEntryDedicated.IsValidAsync(delegate(bool result)
			{
				if (result)
				{
					this.OnManualAddToFavoritesSuccess(newServerListEntryDedicated);
					return;
				}
				if (newServerListEntryDedicated.AddressVariant == ServerJoinDataDedicated.AddressType.URL)
				{
					UnifiedPopup.Push(new WarningPopup("$menu_addserverfailed", "$menu_addserverfaileddnslookup", delegate()
					{
						UnifiedPopup.Pop();
					}, true));
				}
				else
				{
					UnifiedPopup.Push(new WarningPopup("$menu_addserverfailed", "$menu_addserverfailedincorrectformatting", delegate()
					{
						UnifiedPopup.Pop();
					}, true));
				}
				this.isAwaitingServerAdd = false;
			});
			return;
		}
		UnifiedPopup.Push(new WarningPopup("$menu_addserverfailed", "$menu_addserverfailedincorrectformatting", delegate()
		{
			UnifiedPopup.Pop();
		}, true));
	}

	// Token: 0x060009FE RID: 2558 RVA: 0x0004C8A9 File Offset: 0x0004AAA9
	private void OnManualAddToFavoritesStart()
	{
		this.isAwaitingServerAdd = true;
	}

	// Token: 0x060009FF RID: 2559 RVA: 0x0004C8B4 File Offset: 0x0004AAB4
	private void OnManualAddToFavoritesSuccess(ServerJoinData newServerListEntry)
	{
		ServerStatus serverStatus = null;
		for (int i = 0; i < this.m_favoriteServerList.Count; i++)
		{
			if (this.m_favoriteServerList[i].m_joinData == newServerListEntry)
			{
				serverStatus = this.m_favoriteServerList[i];
				break;
			}
		}
		if (serverStatus == null)
		{
			serverStatus = new ServerStatus(newServerListEntry);
			this.m_favoriteServerList.Add(serverStatus);
			this.filteredListOutdated = true;
		}
		this.m_serverListTabHandler.SetActiveTab(0);
		this.m_startup.SetServerToJoin(serverStatus);
		this.SetSelectedServer(this.GetSelectedServer(), true);
		this.OnAddServerClose();
		this.m_addServerTextInput.text = "";
		this.isAwaitingServerAdd = false;
	}

	// Token: 0x06000A00 RID: 2560 RVA: 0x0004C960 File Offset: 0x0004AB60
	private void OnPlayFabJoinCodeSuccess(PlayFabMatchmakingServerData serverData)
	{
		if (serverData == null)
		{
			UnifiedPopup.Push(new WarningPopup("$menu_addserverfailed", "$error_incompatibleversion", delegate()
			{
				UnifiedPopup.Pop();
			}, true));
			this.isAwaitingServerAdd = false;
			return;
		}
		if (serverData.platformRestriction != "None" && serverData.platformRestriction != PrivilegeManager.GetCurrentPlatform().ToString())
		{
			UnifiedPopup.Push(new WarningPopup("$menu_addserverfailed", "$error_platformexcluded", delegate()
			{
				UnifiedPopup.Pop();
			}, true));
			this.isAwaitingServerAdd = false;
			return;
		}
		if (!PrivilegeManager.CanCrossplay && serverData.platformRestriction != PrivilegeManager.GetCurrentPlatform().ToString())
		{
			UnifiedPopup.Push(new WarningPopup("$menu_addserverfailed", "$xbox_error_crossplayprivilege", delegate()
			{
				UnifiedPopup.Pop();
			}, true));
			this.isAwaitingServerAdd = false;
			return;
		}
		ZPlayFabMatchmaking.JoinCode = serverData.joinCode;
		this.OnManualAddToFavoritesSuccess(new ServerJoinDataPlayFabUser(serverData.remotePlayerId)
		{
			m_serverName = serverData.serverName
		});
	}

	// Token: 0x06000A01 RID: 2561 RVA: 0x0004CAA8 File Offset: 0x0004ACA8
	private void OnJoinCodeFailed(ZPLayFabMatchmakingFailReason failReason)
	{
		ZLog.Log("Failed to resolve join code for the following reason: " + failReason.ToString());
		this.isAwaitingServerAdd = false;
		UnifiedPopup.Push(new WarningPopup("$menu_addserverfailed", "$menu_addserverfailedresolvejoincode", delegate()
		{
			UnifiedPopup.Pop();
		}, true));
	}

	// Token: 0x04000BC9 RID: 3017
	private static ServerList instance = null;

	// Token: 0x04000BCA RID: 3018
	private ServerListType currentServerList;

	// Token: 0x04000BCB RID: 3019
	[SerializeField]
	private Button m_favoriteButton;

	// Token: 0x04000BCC RID: 3020
	[SerializeField]
	private Button m_removeButton;

	// Token: 0x04000BCD RID: 3021
	[SerializeField]
	private Button m_upButton;

	// Token: 0x04000BCE RID: 3022
	[SerializeField]
	private Button m_downButton;

	// Token: 0x04000BCF RID: 3023
	[SerializeField]
	private FejdStartup m_startup;

	// Token: 0x04000BD0 RID: 3024
	[SerializeField]
	private Sprite connectUnknown;

	// Token: 0x04000BD1 RID: 3025
	[SerializeField]
	private Sprite connectTrying;

	// Token: 0x04000BD2 RID: 3026
	[SerializeField]
	private Sprite connectSuccess;

	// Token: 0x04000BD3 RID: 3027
	[SerializeField]
	private Sprite connectFailed;

	// Token: 0x04000BD4 RID: 3028
	[Header("Join")]
	public float m_serverListElementStep = 32f;

	// Token: 0x04000BD5 RID: 3029
	public RectTransform m_serverListRoot;

	// Token: 0x04000BD6 RID: 3030
	public GameObject m_serverListElementSteamCrossplay;

	// Token: 0x04000BD7 RID: 3031
	public GameObject m_serverListElement;

	// Token: 0x04000BD8 RID: 3032
	public ScrollRectEnsureVisible m_serverListEnsureVisible;

	// Token: 0x04000BD9 RID: 3033
	public Button m_serverRefreshButton;

	// Token: 0x04000BDA RID: 3034
	public TextMeshProUGUI m_serverCount;

	// Token: 0x04000BDB RID: 3035
	public int m_serverPlayerLimit = 10;

	// Token: 0x04000BDC RID: 3036
	public InputField m_filterInputField;

	// Token: 0x04000BDD RID: 3037
	public Button m_addServerButton;

	// Token: 0x04000BDE RID: 3038
	public GameObject m_addServerPanel;

	// Token: 0x04000BDF RID: 3039
	public Button m_addServerConfirmButton;

	// Token: 0x04000BE0 RID: 3040
	public Button m_addServerCancelButton;

	// Token: 0x04000BE1 RID: 3041
	public InputField m_addServerTextInput;

	// Token: 0x04000BE2 RID: 3042
	public TabHandler m_serverListTabHandler;

	// Token: 0x04000BE3 RID: 3043
	private bool isAwaitingServerAdd;

	// Token: 0x04000BE4 RID: 3044
	public Button m_joinGameButton;

	// Token: 0x04000BE5 RID: 3045
	private float m_serverListBaseSize;

	// Token: 0x04000BE6 RID: 3046
	private int m_serverListRevision = -1;

	// Token: 0x04000BE7 RID: 3047
	private float m_lastServerListRequesTime = -999f;

	// Token: 0x04000BE8 RID: 3048
	private bool m_doneInitialServerListRequest;

	// Token: 0x04000BE9 RID: 3049
	private bool buttonsOutdated = true;

	// Token: 0x04000BEA RID: 3050
	private bool initialized;

	// Token: 0x04000BEB RID: 3051
	private static int maxRecentServers = 11;

	// Token: 0x04000BEC RID: 3052
	private List<ServerStatus> m_steamMatchmakingServerList = new List<ServerStatus>();

	// Token: 0x04000BED RID: 3053
	private readonly List<ServerStatus> m_crossplayMatchmakingServerList = new List<ServerStatus>();

	// Token: 0x04000BEE RID: 3054
	private bool m_localServerListsLoaded;

	// Token: 0x04000BEF RID: 3055
	private Dictionary<ServerJoinData, ServerStatus> m_allLoadedServerData = new Dictionary<ServerJoinData, ServerStatus>();

	// Token: 0x04000BF0 RID: 3056
	private List<ServerStatus> m_recentServerList = new List<ServerStatus>();

	// Token: 0x04000BF1 RID: 3057
	private List<ServerStatus> m_favoriteServerList = new List<ServerStatus>();

	// Token: 0x04000BF2 RID: 3058
	private bool filteredListOutdated;

	// Token: 0x04000BF3 RID: 3059
	private List<ServerStatus> m_filteredList = new List<ServerStatus>();

	// Token: 0x04000BF4 RID: 3060
	private List<ServerList.ServerListElement> m_serverListElements = new List<ServerList.ServerListElement>();

	// Token: 0x04000BF5 RID: 3061
	private double serverListLastUpdatedTime;

	// Token: 0x04000BF6 RID: 3062
	private bool m_playFabServerSearchOngoing;

	// Token: 0x04000BF7 RID: 3063
	private bool m_playFabServerSearchQueued;

	// Token: 0x04000BF8 RID: 3064
	private readonly Dictionary<PlayFabMatchmakingServerData, PlayFabMatchmakingServerData> m_playFabTemporarySearchServerList = new Dictionary<PlayFabMatchmakingServerData, PlayFabMatchmakingServerData>();

	// Token: 0x04000BF9 RID: 3065
	private float m_whenToSearchPlayFab = -1f;

	// Token: 0x04000BFA RID: 3066
	private const uint serverListVersion = 1U;

	// Token: 0x020000F2 RID: 242
	private class ServerListElement
	{
		// Token: 0x06000A0B RID: 2571 RVA: 0x0004CBF8 File Offset: 0x0004ADF8
		public ServerListElement(GameObject element, ServerStatus serverStatus)
		{
			this.m_element = element;
			this.m_serverStatus = serverStatus;
			this.m_button = this.m_element.GetComponent<Button>();
			this.m_rectTransform = (this.m_element.transform as RectTransform);
			this.m_serverName = this.m_element.GetComponentInChildren<Text>();
			this.m_tooltip = this.m_element.GetComponentInChildren<UITooltip>();
			this.m_version = this.m_element.transform.Find("version").GetComponent<Text>();
			this.m_players = this.m_element.transform.Find("players").GetComponent<Text>();
			this.m_status = this.m_element.transform.Find("status").GetComponent<Image>();
			this.m_crossplay = this.m_element.transform.Find("crossplay");
			this.m_private = this.m_element.transform.Find("Private");
			this.m_selected = (this.m_element.transform.Find("selected") as RectTransform);
		}

		// Token: 0x04000BFB RID: 3067
		public GameObject m_element;

		// Token: 0x04000BFC RID: 3068
		public ServerStatus m_serverStatus;

		// Token: 0x04000BFD RID: 3069
		public Button m_button;

		// Token: 0x04000BFE RID: 3070
		public RectTransform m_rectTransform;

		// Token: 0x04000BFF RID: 3071
		public Text m_serverName;

		// Token: 0x04000C00 RID: 3072
		public UITooltip m_tooltip;

		// Token: 0x04000C01 RID: 3073
		public Text m_version;

		// Token: 0x04000C02 RID: 3074
		public Text m_players;

		// Token: 0x04000C03 RID: 3075
		public Image m_status;

		// Token: 0x04000C04 RID: 3076
		public Transform m_crossplay;

		// Token: 0x04000C05 RID: 3077
		public Transform m_private;

		// Token: 0x04000C06 RID: 3078
		public RectTransform m_selected;
	}

	// Token: 0x020000F3 RID: 243
	private struct StorageLocation
	{
		// Token: 0x06000A0C RID: 2572 RVA: 0x0004CD18 File Offset: 0x0004AF18
		public StorageLocation(string path, FileHelpers.FileSource source)
		{
			this.path = path;
			this.source = source;
		}

		// Token: 0x04000C07 RID: 3079
		public string path;

		// Token: 0x04000C08 RID: 3080
		public FileHelpers.FileSource source;
	}

	// Token: 0x020000F4 RID: 244
	public enum SaveStatusCode
	{
		// Token: 0x04000C0A RID: 3082
		Succeess,
		// Token: 0x04000C0B RID: 3083
		UnsupportedServerListType,
		// Token: 0x04000C0C RID: 3084
		UnknownServerBackend,
		// Token: 0x04000C0D RID: 3085
		CloudQuotaExceeded,
		// Token: 0x04000C0E RID: 3086
		FailedUnknownReason
	}
}
