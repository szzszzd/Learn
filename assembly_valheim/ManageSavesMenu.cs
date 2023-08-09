using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x020000BC RID: 188
public class ManageSavesMenu : MonoBehaviour
{
	// Token: 0x06000823 RID: 2083 RVA: 0x00040E6C File Offset: 0x0003F06C
	private void Update()
	{
		bool flag = false;
		if (!this.blockerInfo.IsBlocked())
		{
			bool flag2 = true;
			if (Input.GetKeyDown(KeyCode.LeftArrow) && this.IsSelectedExpanded())
			{
				this.CollapseSelected();
				flag = true;
				flag2 = false;
			}
			if (Input.GetKeyDown(KeyCode.RightArrow) && !this.IsSelectedExpanded())
			{
				this.ExpandSelected();
				flag = true;
			}
			if (flag2)
			{
				if (Input.GetKeyDown(KeyCode.DownArrow))
				{
					this.SelectRelative(1);
					flag = true;
				}
				if (Input.GetKeyDown(KeyCode.UpArrow))
				{
					this.SelectRelative(-1);
					flag = true;
				}
			}
			if (ZInput.IsGamepadActive())
			{
				if (ZInput.GetButtonDown("JoyLStickDown") || ZInput.GetButtonDown("JoyDPadDown"))
				{
					this.SelectRelative(1);
					flag = true;
				}
				if (ZInput.GetButtonDown("JoyLStickUp") || ZInput.GetButtonDown("JoyDPadUp"))
				{
					this.SelectRelative(-1);
					flag = true;
				}
			}
		}
		if (flag)
		{
			this.UpdateButtons();
			this.CenterSelected();
			return;
		}
		this.UpdateButtonsInteractable();
	}

	// Token: 0x06000824 RID: 2084 RVA: 0x00040F53 File Offset: 0x0003F153
	private void LateUpdate()
	{
		if (this.elementHeightChanged)
		{
			this.elementHeightChanged = false;
			this.UpdateElementPositions();
		}
	}

	// Token: 0x06000825 RID: 2085 RVA: 0x00040F6C File Offset: 0x0003F16C
	private void UpdateButtons()
	{
		this.moveButton.gameObject.SetActive(FileHelpers.m_cloudEnabled && !FileHelpers.m_cloudOnly);
		if (this.selectedSaveIndex < 0)
		{
			this.actionButton.GetComponentInChildren<Text>().text = Localization.instance.Localize("$menu_expand");
		}
		else
		{
			if (this.selectedBackupIndex < 0)
			{
				if (this.listElements[this.selectedSaveIndex].BackupCount > 0)
				{
					this.actionButton.GetComponentInChildren<Text>().text = Localization.instance.Localize(this.listElements[this.selectedSaveIndex].IsExpanded ? "$menu_collapse" : "$menu_expand");
				}
			}
			else
			{
				this.actionButton.GetComponentInChildren<Text>().text = Localization.instance.Localize("$menu_restorebackup");
			}
			if (this.selectedBackupIndex < 0)
			{
				if (!this.currentList[this.selectedSaveIndex].IsDeleted)
				{
					this.moveButton.GetComponentInChildren<Text>().text = Localization.instance.Localize((this.currentList[this.selectedSaveIndex].PrimaryFile.m_source != FileHelpers.FileSource.Cloud) ? "$menu_movetocloud" : "$menu_movetolocal");
				}
			}
			else
			{
				this.moveButton.GetComponentInChildren<Text>().text = Localization.instance.Localize((this.currentList[this.selectedSaveIndex].BackupFiles[this.selectedBackupIndex].m_source != FileHelpers.FileSource.Cloud) ? "$menu_movetocloud" : "$menu_movetolocal");
			}
		}
		this.UpdateButtonsInteractable();
	}

	// Token: 0x06000826 RID: 2086 RVA: 0x000410F8 File Offset: 0x0003F2F8
	private void UpdateButtonsInteractable()
	{
		bool flag = (DateTime.Now - this.mostRecentBackupCreatedTime).TotalSeconds >= 1.0;
		bool flag2 = this.selectedSaveIndex >= 0 && this.selectedSaveIndex < this.listElements.Count;
		bool flag3 = flag2 && this.selectedBackupIndex >= 0;
		bool flag4 = flag2 && this.listElements[this.selectedSaveIndex].BackupCount > 0 && this.selectedBackupIndex < 0;
		this.actionButton.interactable = (flag4 || (flag3 && flag));
		bool flag5 = flag2 && (this.selectedBackupIndex >= 0 || !this.currentList[this.selectedSaveIndex].IsDeleted);
		this.removeButton.interactable = flag5;
		this.moveButton.interactable = (flag5 && flag);
	}

	// Token: 0x06000827 RID: 2087 RVA: 0x000411DD File Offset: 0x0003F3DD
	private void OnSaveElementHeighChanged()
	{
		this.elementHeightChanged = true;
	}

	// Token: 0x06000828 RID: 2088 RVA: 0x000411E8 File Offset: 0x0003F3E8
	private void UpdateCloudUsageAsync(ManageSavesMenu.UpdateCloudUsageFinishedCallback callback = null)
	{
		if (FileHelpers.m_cloudEnabled)
		{
			this.PushPleaseWait();
			BackgroundWorker backgroundWorker = new BackgroundWorker();
			ulong usedBytes = 0UL;
			ulong capacityBytes = 0UL;
			backgroundWorker.DoWork += delegate(object sender, DoWorkEventArgs args)
			{
				usedBytes = FileHelpers.GetTotalCloudUsage();
				capacityBytes = FileHelpers.GetTotalCloudCapacity();
			};
			backgroundWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs args)
			{
				this.storageUsed.gameObject.SetActive(true);
				this.storageBar.parent.gameObject.SetActive(true);
				this.storageUsed.text = Localization.instance.Localize("$menu_cloudstorageused", new string[]
				{
					FileHelpers.BytesAsNumberString(usedBytes, 1U),
					FileHelpers.BytesAsNumberString(capacityBytes, 1U)
				});
				this.storageBar.localScale = new Vector3(usedBytes / capacityBytes, this.storageBar.localScale.y, this.storageBar.localScale.z);
				this.PopPleaseWait();
				ManageSavesMenu.UpdateCloudUsageFinishedCallback callback3 = callback;
				if (callback3 == null)
				{
					return;
				}
				callback3();
			};
			backgroundWorker.RunWorkerAsync();
			return;
		}
		this.storageUsed.gameObject.SetActive(false);
		this.storageBar.parent.gameObject.SetActive(false);
		ManageSavesMenu.UpdateCloudUsageFinishedCallback callback2 = callback;
		if (callback2 == null)
		{
			return;
		}
		callback2();
	}

	// Token: 0x06000829 RID: 2089 RVA: 0x0004128C File Offset: 0x0003F48C
	private void OnBackButton()
	{
		this.Close();
	}

	// Token: 0x0600082A RID: 2090 RVA: 0x00041294 File Offset: 0x0003F494
	private void OnRemoveButton()
	{
		if (this.selectedSaveIndex < 0)
		{
			return;
		}
		bool isBackup = this.selectedBackupIndex >= 0;
		string text;
		if (isBackup)
		{
			text = "$menu_removebackup";
		}
		else
		{
			int activeTab = this.tabHandler.GetActiveTab();
			if (activeTab != 0)
			{
				if (activeTab != 1)
				{
					text = "Remove?";
				}
				else
				{
					text = "$menu_removecharacter";
				}
			}
			else
			{
				text = "$menu_removeworld";
			}
		}
		SaveFile toDelete = isBackup ? this.currentList[this.selectedSaveIndex].BackupFiles[this.selectedBackupIndex] : this.currentList[this.selectedSaveIndex].PrimaryFile;
		UnifiedPopup.Push(new YesNoPopup(Localization.instance.Localize(text), isBackup ? toDelete.FileName : this.currentList[this.selectedSaveIndex].m_name, delegate()
		{
			UnifiedPopup.Pop();
			this.DeleteSaveFile(toDelete, isBackup);
		}, delegate()
		{
			UnifiedPopup.Pop();
		}, false));
	}

	// Token: 0x0600082B RID: 2091 RVA: 0x000413A8 File Offset: 0x0003F5A8
	private void OnMoveButton()
	{
		if (this.selectedSaveIndex < 0)
		{
			return;
		}
		bool flag = this.selectedBackupIndex >= 0;
		SaveFile saveFile = flag ? this.currentList[this.selectedSaveIndex].BackupFiles[this.selectedBackupIndex] : this.currentList[this.selectedSaveIndex].PrimaryFile;
		FileHelpers.FileSource fileSource = (saveFile.m_source != FileHelpers.FileSource.Cloud) ? FileHelpers.FileSource.Cloud : FileHelpers.FileSource.Local;
		SaveFile saveFile2 = null;
		for (int i = 0; i < this.currentList[this.selectedSaveIndex].BackupFiles.Length; i++)
		{
			if (i != this.selectedBackupIndex && this.currentList[this.selectedSaveIndex].BackupFiles[i].m_source == fileSource && this.currentList[this.selectedSaveIndex].BackupFiles[i].FileName == saveFile.FileName)
			{
				saveFile2 = this.currentList[this.selectedSaveIndex].BackupFiles[i];
				break;
			}
		}
		if (saveFile2 == null && flag && !this.currentList[this.selectedSaveIndex].IsDeleted && this.currentList[this.selectedSaveIndex].PrimaryFile.m_source == fileSource && this.currentList[this.selectedSaveIndex].PrimaryFile.FileName == saveFile.FileName)
		{
			saveFile2 = this.currentList[this.selectedSaveIndex].PrimaryFile;
		}
		if (saveFile2 != null)
		{
			UnifiedPopup.Push(new WarningPopup(Localization.instance.Localize("$menu_cantmovesave"), Localization.instance.Localize("$menu_duplicatefileprompttext", new string[]
			{
				saveFile.FileName
			}), delegate()
			{
				UnifiedPopup.Pop();
			}, false));
			return;
		}
		if (SaveSystem.IsCorrupt(saveFile))
		{
			UnifiedPopup.Push(new WarningPopup("$menu_cantmovesave", "$menu_savefilecorrupt", delegate()
			{
				UnifiedPopup.Pop();
			}, true));
			return;
		}
		this.MoveSource(saveFile, flag, fileSource);
	}

	// Token: 0x0600082C RID: 2092 RVA: 0x000415A8 File Offset: 0x0003F7A8
	private void OnPrimaryActionButton()
	{
		if (this.selectedSaveIndex < 0)
		{
			return;
		}
		if (this.selectedBackupIndex >= 0)
		{
			this.RestoreBackup();
			return;
		}
		if (this.listElements[this.selectedSaveIndex].BackupCount > 0)
		{
			this.listElements[this.selectedSaveIndex].SetExpanded(!this.listElements[this.selectedSaveIndex].IsExpanded, true);
			this.UpdateButtons();
		}
	}

	// Token: 0x0600082D RID: 2093 RVA: 0x00041624 File Offset: 0x0003F824
	private void RestoreBackup()
	{
		SaveWithBackups saveWithBackups = this.currentList[this.selectedSaveIndex];
		SaveFile backup = this.currentList[this.selectedSaveIndex].BackupFiles[this.selectedBackupIndex];
		UnifiedPopup.Push(new YesNoPopup(Localization.instance.Localize("$menu_backuprestorepromptheader"), saveWithBackups.IsDeleted ? Localization.instance.Localize("$menu_backuprestorepromptrecover", new string[]
		{
			saveWithBackups.m_name,
			backup.FileName
		}) : Localization.instance.Localize("$menu_backuprestorepromptreplace", new string[]
		{
			saveWithBackups.m_name,
			backup.FileName
		}), delegate()
		{
			UnifiedPopup.Pop();
			base.<RestoreBackup>g__RestoreBackupAsync|2();
		}, delegate()
		{
			UnifiedPopup.Pop();
		}, false));
	}

	// Token: 0x0600082E RID: 2094 RVA: 0x00041714 File Offset: 0x0003F914
	private void UpdateGuiAfterFileModification(bool alwaysSelectSave = false)
	{
		string saveName = (this.selectedSaveIndex >= 0) ? this.listElements[this.selectedSaveIndex].Save.m_name : "";
		string backupName = (this.selectedSaveIndex >= 0 && this.selectedBackupIndex >= 0 && this.selectedBackupIndex < this.listElements[this.selectedSaveIndex].Save.BackupFiles.Length) ? this.listElements[this.selectedSaveIndex].Save.BackupFiles[this.selectedBackupIndex].FileName : "";
		int saveIndex = this.selectedSaveIndex;
		int backupIndex = this.selectedBackupIndex;
		this.DeselectCurrent();
		this.UpdateCloudUsageAsync(null);
		this.ReloadSavesAsync(delegate(bool success)
		{
			if (success)
			{
				base.<UpdateGuiAfterFileModification>g__UpdateGuiAsync|1();
				return;
			}
			this.ShowReloadError();
		});
	}

	// Token: 0x0600082F RID: 2095 RVA: 0x00041808 File Offset: 0x0003FA08
	public void OnWorldTab()
	{
		if (this.pleaseWaitCount > 0)
		{
			return;
		}
		this.ChangeList(SaveDataType.World);
	}

	// Token: 0x06000830 RID: 2096 RVA: 0x0004181B File Offset: 0x0003FA1B
	public void OnCharacterTab()
	{
		if (this.pleaseWaitCount > 0)
		{
			return;
		}
		this.ChangeList(SaveDataType.Character);
	}

	// Token: 0x06000831 RID: 2097 RVA: 0x0004182E File Offset: 0x0003FA2E
	private void ChangeList(SaveDataType dataType)
	{
		this.DeselectCurrent();
		this.currentList = SaveSystem.GetSavesByType(dataType);
		this.currentListType = dataType;
		this.UpdateSavesListGuiAsync(delegate
		{
			bool flag = false;
			if (!string.IsNullOrEmpty(this.m_queuedNameToSelect))
			{
				for (int i = 0; i < this.currentList.Length; i++)
				{
					if (!this.currentList[i].IsDeleted && this.currentList[i].PrimaryFile.FileName == this.m_queuedNameToSelect)
					{
						this.SelectByIndex(i, -1);
						flag = true;
						break;
					}
				}
				this.m_queuedNameToSelect = null;
			}
			if (!flag || this.listElements.Count <= 0)
			{
				this.SelectByIndex(0, -1);
			}
			if (this.selectedSaveIndex >= 0)
			{
				this.CenterSelected();
			}
			this.UpdateButtons();
		});
	}

	// Token: 0x06000832 RID: 2098 RVA: 0x0004185C File Offset: 0x0003FA5C
	private void DeleteSaveFile(SaveFile file, bool isBackup)
	{
		this.PushPleaseWait();
		bool success = false;
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		backgroundWorker.DoWork += delegate(object sender, DoWorkEventArgs args)
		{
			success = SaveSystem.Delete(file);
		};
		backgroundWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs args)
		{
			this.PopPleaseWait();
			if (!success)
			{
				ManageSavesMenu.<DeleteSaveFile>g__DeleteSaveFailed|37_2();
				ZLog.LogError("Failed to delete save " + file.FileName);
			}
			this.mostRecentBackupCreatedTime = DateTime.Now;
			ManageSavesMenu.SavesModifiedCallback savesModifiedCallback = this.savesModifiedCallback;
			if (savesModifiedCallback != null)
			{
				savesModifiedCallback(this.GetCurrentListType());
			}
			this.UpdateGuiAfterFileModification(false);
		};
		backgroundWorker.RunWorkerAsync();
	}

	// Token: 0x06000833 RID: 2099 RVA: 0x000418B8 File Offset: 0x0003FAB8
	private void MoveSource(SaveFile file, bool isBackup, FileHelpers.FileSource destinationSource)
	{
		this.PushPleaseWait();
		bool cloudQuotaExceeded = false;
		bool success = false;
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		backgroundWorker.DoWork += delegate(object sender, DoWorkEventArgs args)
		{
			success = SaveSystem.MoveSource(file, isBackup, destinationSource, out cloudQuotaExceeded);
		};
		backgroundWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs args)
		{
			this.PopPleaseWait();
			if (cloudQuotaExceeded)
			{
				this.ShowCloudQuotaWarning();
			}
			else if (!success)
			{
				ManageSavesMenu.<MoveSource>g__MoveSourceFailed|38_2();
			}
			this.mostRecentBackupCreatedTime = DateTime.Now;
			ManageSavesMenu.SavesModifiedCallback savesModifiedCallback = this.savesModifiedCallback;
			if (savesModifiedCallback != null)
			{
				savesModifiedCallback(this.GetCurrentListType());
			}
			this.UpdateGuiAfterFileModification(false);
		};
		backgroundWorker.RunWorkerAsync();
	}

	// Token: 0x06000834 RID: 2100 RVA: 0x00041929 File Offset: 0x0003FB29
	private SaveDataType GetCurrentListType()
	{
		return this.currentListType;
	}

	// Token: 0x06000835 RID: 2101 RVA: 0x00041934 File Offset: 0x0003FB34
	private void ReloadSavesAsync(ManageSavesMenu.ReloadSavesFinishedCallback callback)
	{
		this.PushPleaseWait();
		Exception reloadException = null;
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		backgroundWorker.DoWork += delegate(object sender, DoWorkEventArgs args)
		{
			try
			{
				SaveSystem.ForceRefreshCache();
			}
			catch (Exception reloadException)
			{
				reloadException = reloadException;
			}
		};
		backgroundWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs args)
		{
			this.currentList = SaveSystem.GetSavesByType(this.currentListType);
			this.PopPleaseWait();
			if (reloadException != null)
			{
				ZLog.LogError(reloadException.ToString());
			}
			ManageSavesMenu.ReloadSavesFinishedCallback callback2 = callback;
			if (callback2 == null)
			{
				return;
			}
			callback2(reloadException == null);
		};
		backgroundWorker.RunWorkerAsync();
	}

	// Token: 0x06000836 RID: 2102 RVA: 0x00041990 File Offset: 0x0003FB90
	private void UpdateElementPositions()
	{
		float num = 0f;
		for (int i = 0; i < this.listElements.Count; i++)
		{
			this.listElements[i].rectTransform.anchoredPosition = new Vector2(this.listElements[i].rectTransform.anchoredPosition.x, -num);
			num += this.listElements[i].rectTransform.sizeDelta.y;
		}
		this.listRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num);
	}

	// Token: 0x06000837 RID: 2103 RVA: 0x00041A1C File Offset: 0x0003FC1C
	private IEnumerator UpdateElementPositionsEnumerator()
	{
		float pos = 0f;
		int num;
		for (int i = 0; i < this.listElements.Count; i = num + 1)
		{
			this.listElements[i].rectTransform.anchoredPosition = new Vector2(this.listElements[i].rectTransform.anchoredPosition.x, -pos);
			pos += this.listElements[i].rectTransform.sizeDelta.y;
			yield return null;
			num = i;
		}
		this.listRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pos);
		yield break;
	}

	// Token: 0x06000838 RID: 2104 RVA: 0x00041A2C File Offset: 0x0003FC2C
	private ManageSavesMenuElement CreateElement()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.saveElement, this.listRoot);
		ManageSavesMenuElement component = gameObject.GetComponent<ManageSavesMenuElement>();
		gameObject.SetActive(true);
		component.HeightChanged += this.OnSaveElementHeighChanged;
		component.ElementClicked += this.OnElementClicked;
		component.ElementExpandedChanged += this.OnElementExpandedChanged;
		return component;
	}

	// Token: 0x06000839 RID: 2105 RVA: 0x00041A90 File Offset: 0x0003FC90
	private void UpdateSavesListGui()
	{
		List<ManageSavesMenuElement> list = new List<ManageSavesMenuElement>();
		Dictionary<string, ManageSavesMenuElement> dictionary = new Dictionary<string, ManageSavesMenuElement>();
		for (int i = 0; i < this.listElements.Count; i++)
		{
			dictionary.Add(this.listElements[i].Save.m_name, this.listElements[i]);
		}
		for (int j = 0; j < this.currentList.Length; j++)
		{
			if (dictionary.ContainsKey(this.currentList[j].m_name))
			{
				dictionary[this.currentList[j].m_name].UpdateElement(this.currentList[j]);
				list.Add(dictionary[this.currentList[j].m_name]);
				dictionary.Remove(this.currentList[j].m_name);
			}
			else
			{
				ManageSavesMenuElement manageSavesMenuElement = this.CreateElement();
				manageSavesMenuElement.SetUp(manageSavesMenuElement.Save);
				list.Add(manageSavesMenuElement);
			}
		}
		foreach (KeyValuePair<string, ManageSavesMenuElement> keyValuePair in dictionary)
		{
			UnityEngine.Object.Destroy(keyValuePair.Value.gameObject);
		}
		this.listElements = list;
		this.UpdateElementPositions();
	}

	// Token: 0x0600083A RID: 2106 RVA: 0x00041BDC File Offset: 0x0003FDDC
	private IEnumerator UpdateSaveListGuiAsyncCoroutine(ManageSavesMenu.UpdateGuiListFinishedCallback callback)
	{
		this.PushPleaseWait();
		float timeBudget = 0.25f / (float)Application.targetFrameRate;
		DateTime now = DateTime.Now;
		int num;
		for (int i = this.listElements.Count - 1; i >= 0; i = num - 1)
		{
			this.listElements[i].rectTransform.anchoredPosition = new Vector2(this.listElements[i].rectTransform.anchoredPosition.x, 1000000f);
			if ((DateTime.Now - now).TotalSeconds >= (double)timeBudget)
			{
				yield return null;
				now = DateTime.Now;
			}
			num = i;
		}
		if ((DateTime.Now - now).TotalSeconds >= (double)timeBudget)
		{
			yield return null;
			now = DateTime.Now;
		}
		List<ManageSavesMenuElement> newSaveElementsList = new List<ManageSavesMenuElement>();
		Dictionary<string, ManageSavesMenuElement> saveNameToElementMap = new Dictionary<string, ManageSavesMenuElement>();
		for (int i = 0; i < this.listElements.Count; i = num + 1)
		{
			saveNameToElementMap.Add(this.listElements[i].Save.m_name, this.listElements[i]);
			if ((DateTime.Now - now).TotalSeconds >= (double)timeBudget)
			{
				yield return null;
				now = DateTime.Now;
			}
			num = i;
		}
		for (int i = 0; i < this.currentList.Length; i = num + 1)
		{
			if (saveNameToElementMap.ContainsKey(this.currentList[i].m_name))
			{
				IEnumerator updateElementEnumerator = saveNameToElementMap[this.currentList[i].m_name].UpdateElementEnumerator(this.currentList[i]);
				while (updateElementEnumerator.MoveNext())
				{
					if ((DateTime.Now - now).TotalSeconds >= (double)timeBudget)
					{
						yield return null;
						now = DateTime.Now;
					}
				}
				newSaveElementsList.Add(saveNameToElementMap[this.currentList[i].m_name]);
				saveNameToElementMap.Remove(this.currentList[i].m_name);
				updateElementEnumerator = null;
			}
			else
			{
				ManageSavesMenuElement manageSavesMenuElement = this.CreateElement();
				newSaveElementsList.Add(manageSavesMenuElement);
				newSaveElementsList[newSaveElementsList.Count - 1].rectTransform.anchoredPosition = new Vector2(newSaveElementsList[newSaveElementsList.Count - 1].rectTransform.anchoredPosition.x, 1000000f);
				IEnumerator updateElementEnumerator = manageSavesMenuElement.SetUpEnumerator(this.currentList[i]);
				while (updateElementEnumerator.MoveNext())
				{
					if ((DateTime.Now - now).TotalSeconds >= (double)timeBudget)
					{
						yield return null;
						now = DateTime.Now;
					}
				}
				updateElementEnumerator = null;
			}
			if ((DateTime.Now - now).TotalSeconds >= (double)timeBudget)
			{
				yield return null;
				now = DateTime.Now;
			}
			num = i;
		}
		foreach (KeyValuePair<string, ManageSavesMenuElement> keyValuePair in saveNameToElementMap)
		{
			UnityEngine.Object.Destroy(keyValuePair.Value.gameObject);
			if ((DateTime.Now - now).TotalSeconds > (double)timeBudget)
			{
				yield return null;
				now = DateTime.Now;
			}
		}
		Dictionary<string, ManageSavesMenuElement>.Enumerator enumerator = default(Dictionary<string, ManageSavesMenuElement>.Enumerator);
		this.listElements = newSaveElementsList;
		if ((DateTime.Now - now).TotalSeconds >= (double)timeBudget)
		{
			yield return null;
			now = DateTime.Now;
		}
		IEnumerator updateElementPositionsEnumerator = this.UpdateElementPositionsEnumerator();
		while (updateElementPositionsEnumerator.MoveNext())
		{
			if ((DateTime.Now - now).TotalSeconds >= (double)timeBudget)
			{
				yield return null;
				now = DateTime.Now;
			}
		}
		this.PopPleaseWait();
		if (callback != null)
		{
			callback();
		}
		yield break;
		yield break;
	}

	// Token: 0x0600083B RID: 2107 RVA: 0x00041BF2 File Offset: 0x0003FDF2
	private void UpdateSavesListGuiAsync(ManageSavesMenu.UpdateGuiListFinishedCallback callback)
	{
		base.StartCoroutine(this.UpdateSaveListGuiAsyncCoroutine(callback));
	}

	// Token: 0x0600083C RID: 2108 RVA: 0x00041C04 File Offset: 0x0003FE04
	private void DestroyGui()
	{
		for (int i = 0; i < this.listElements.Count; i++)
		{
			UnityEngine.Object.Destroy(this.listElements[i].gameObject);
		}
		this.listElements.Clear();
	}

	// Token: 0x0600083D RID: 2109 RVA: 0x00041C48 File Offset: 0x0003FE48
	public void Open(SaveDataType dataType, string selectedSaveName, ManageSavesMenu.ClosedCallback closedCallback, ManageSavesMenu.SavesModifiedCallback savesModifiedCallback)
	{
		this.QueueSelectByName(selectedSaveName);
		this.Open(dataType, closedCallback, savesModifiedCallback);
	}

	// Token: 0x0600083E RID: 2110 RVA: 0x00041C5C File Offset: 0x0003FE5C
	public void Open(SaveDataType dataType, ManageSavesMenu.ClosedCallback closedCallback, ManageSavesMenu.SavesModifiedCallback savesModifiedCallback)
	{
		this.closedCallback = closedCallback;
		this.savesModifiedCallback = savesModifiedCallback;
		if (base.gameObject.activeSelf && this.tabHandler.GetActiveTab() == this.GetTabIndexFromSaveDataType(dataType))
		{
			return;
		}
		this.backButton.onClick.AddListener(new UnityAction(this.OnBackButton));
		this.removeButton.onClick.AddListener(new UnityAction(this.OnRemoveButton));
		this.moveButton.onClick.AddListener(new UnityAction(this.OnMoveButton));
		this.actionButton.onClick.AddListener(new UnityAction(this.OnPrimaryActionButton));
		this.storageUsed.gameObject.SetActive(false);
		this.storageBar.parent.gameObject.SetActive(false);
		base.gameObject.SetActive(true);
		this.UpdateCloudUsageAsync(null);
		this.ReloadSavesAsync(delegate(bool success)
		{
			if (!success)
			{
				this.ShowReloadError();
			}
			this.tabHandler.SetActiveTabWithoutInvokingOnClick(this.GetTabIndexFromSaveDataType(dataType));
			this.ChangeList(dataType);
		});
	}

	// Token: 0x0600083F RID: 2111 RVA: 0x00041D6E File Offset: 0x0003FF6E
	private void QueueSelectByName(string name)
	{
		this.m_queuedNameToSelect = name;
	}

	// Token: 0x06000840 RID: 2112 RVA: 0x00041D77 File Offset: 0x0003FF77
	private int GetTabIndexFromSaveDataType(SaveDataType dataType)
	{
		if (dataType == SaveDataType.World)
		{
			return 0;
		}
		if (dataType != SaveDataType.Character)
		{
			throw new ArgumentException(string.Format("{0} does not have a tab!", dataType));
		}
		return 1;
	}

	// Token: 0x06000841 RID: 2113 RVA: 0x00041D9C File Offset: 0x0003FF9C
	public void Close()
	{
		this.DestroyGui();
		this.backButton.onClick.RemoveListener(new UnityAction(this.OnBackButton));
		this.removeButton.onClick.RemoveListener(new UnityAction(this.OnRemoveButton));
		this.moveButton.onClick.RemoveListener(new UnityAction(this.OnMoveButton));
		this.actionButton.onClick.RemoveListener(new UnityAction(this.OnPrimaryActionButton));
		base.gameObject.SetActive(false);
		ManageSavesMenu.ClosedCallback closedCallback = this.closedCallback;
		if (closedCallback == null)
		{
			return;
		}
		closedCallback();
	}

	// Token: 0x06000842 RID: 2114 RVA: 0x00041E3B File Offset: 0x0004003B
	public bool IsVisible()
	{
		return base.gameObject.activeInHierarchy;
	}

	// Token: 0x06000843 RID: 2115 RVA: 0x00041E48 File Offset: 0x00040048
	private void SelectByIndex(int saveIndex, int backupIndex = -1)
	{
		this.DeselectCurrent();
		this.selectedSaveIndex = saveIndex;
		this.selectedBackupIndex = backupIndex;
		if (this.listElements.Count <= 0)
		{
			this.selectedSaveIndex = -1;
			this.selectedBackupIndex = -1;
			return;
		}
		this.selectedSaveIndex = Mathf.Clamp(this.selectedSaveIndex, 0, this.listElements.Count - 1);
		this.listElements[this.selectedSaveIndex].Select(ref this.selectedBackupIndex);
	}

	// Token: 0x06000844 RID: 2116 RVA: 0x00041EC4 File Offset: 0x000400C4
	private void SelectRelative(int offset)
	{
		int num = this.selectedSaveIndex;
		int num2 = this.selectedBackupIndex;
		this.DeselectCurrent();
		if (this.listElements.Count <= 0)
		{
			this.selectedSaveIndex = -1;
			this.selectedBackupIndex = -1;
			return;
		}
		if (num < 0)
		{
			num = 0;
			num2 = -1;
		}
		else if (num > this.listElements.Count - 1)
		{
			num = this.listElements.Count - 1;
			num2 = (this.listElements[num].IsExpanded ? this.listElements[num].BackupCount : -1);
		}
		int num4;
		for (int num3 = offset; num3 != 0; num3 -= num4)
		{
			num4 = Math.Sign(num3);
			if (this.listElements[num].IsExpanded)
			{
				if (num2 + num4 < -1 || num2 + num4 > this.listElements[num].BackupCount - 1)
				{
					if (num + num4 >= 0 && num + num4 <= this.listElements.Count - 1)
					{
						num += num4;
						num2 = ((num4 < 0 && this.listElements[num].IsExpanded) ? (this.listElements[num].BackupCount - 1) : -1);
					}
				}
				else
				{
					num2 += num4;
				}
			}
			else if (num2 >= 0)
			{
				if (num + num4 >= 0 && num + num4 <= this.listElements.Count - 1 && num4 > 0)
				{
					num += num4;
				}
				num2 = -1;
			}
			else if (num + num4 >= 0 && num + num4 <= this.listElements.Count - 1)
			{
				num += num4;
				num2 = ((num4 < 0 && this.listElements[num].IsExpanded) ? (this.listElements[num].BackupCount - 1) : -1);
			}
		}
		this.SelectByIndex(num, num2);
	}

	// Token: 0x06000845 RID: 2117 RVA: 0x00042070 File Offset: 0x00040270
	private void DeselectCurrent()
	{
		if (this.selectedSaveIndex >= 0 && this.selectedSaveIndex <= this.listElements.Count - 1)
		{
			this.listElements[this.selectedSaveIndex].Deselect(this.selectedBackupIndex);
		}
		this.selectedSaveIndex = -1;
		this.selectedBackupIndex = -1;
	}

	// Token: 0x06000846 RID: 2118 RVA: 0x000420C8 File Offset: 0x000402C8
	private bool IsSelectedExpanded()
	{
		if (this.selectedSaveIndex < 0 || this.selectedSaveIndex > this.listElements.Count - 1)
		{
			ZLog.LogError(string.Concat(new string[]
			{
				"Failed to expand save: Index ",
				this.selectedSaveIndex.ToString(),
				" was outside of the valid range 0-",
				(this.listElements.Count - 1).ToString(),
				"."
			}));
			return false;
		}
		return this.listElements[this.selectedSaveIndex].IsExpanded;
	}

	// Token: 0x06000847 RID: 2119 RVA: 0x0004215C File Offset: 0x0004035C
	private void ExpandSelected()
	{
		if (this.selectedSaveIndex < 0 || this.selectedSaveIndex > this.listElements.Count - 1)
		{
			ZLog.LogWarning(string.Concat(new string[]
			{
				"Failed to expand save: Index ",
				this.selectedSaveIndex.ToString(),
				" was outside of the valid range 0-",
				(this.listElements.Count - 1).ToString(),
				". Ignoring."
			}));
			return;
		}
		this.listElements[this.selectedSaveIndex].SetExpanded(true, true);
	}

	// Token: 0x06000848 RID: 2120 RVA: 0x000421F0 File Offset: 0x000403F0
	private void CollapseSelected()
	{
		if (this.selectedSaveIndex < 0 || this.selectedSaveIndex > this.listElements.Count - 1)
		{
			ZLog.LogWarning(string.Concat(new string[]
			{
				"Failed to collapse save: Index ",
				this.selectedSaveIndex.ToString(),
				" was outside of the valid range 0-",
				(this.listElements.Count - 1).ToString(),
				". Ignoring."
			}));
			return;
		}
		this.listElements[this.selectedSaveIndex].SetExpanded(false, true);
	}

	// Token: 0x06000849 RID: 2121 RVA: 0x00042284 File Offset: 0x00040484
	private void CenterSelected()
	{
		if (this.selectedSaveIndex < 0 || this.selectedSaveIndex > this.listElements.Count - 1)
		{
			ZLog.LogWarning(string.Concat(new string[]
			{
				"Failed to center save: Index ",
				this.selectedSaveIndex.ToString(),
				" was outside of the valid range 0-",
				(this.listElements.Count - 1).ToString(),
				". Ignoring."
			}));
			return;
		}
		this.scrollRectEnsureVisible.CenterOnItem(this.listElements[this.selectedSaveIndex].GetTransform(this.selectedBackupIndex));
	}

	// Token: 0x0600084A RID: 2122 RVA: 0x00042328 File Offset: 0x00040528
	private void OnElementClicked(ManageSavesMenuElement element, int backupElementIndex)
	{
		int num = this.selectedSaveIndex;
		int num2 = this.selectedBackupIndex;
		int saveIndex = this.listElements.IndexOf(element);
		this.DeselectCurrent();
		this.SelectByIndex(saveIndex, backupElementIndex);
		if (this.selectedSaveIndex == num && this.selectedBackupIndex == num2 && Time.time < this.timeClicked + 0.5f)
		{
			this.OnPrimaryActionButton();
			this.timeClicked = Time.time - 0.5f;
		}
		else
		{
			this.timeClicked = Time.time;
		}
		this.UpdateButtons();
	}

	// Token: 0x0600084B RID: 2123 RVA: 0x000423B0 File Offset: 0x000405B0
	private void OnElementExpandedChanged(ManageSavesMenuElement element, bool isExpanded)
	{
		int num = this.listElements.IndexOf(element);
		if (this.selectedSaveIndex == num)
		{
			if (!isExpanded && this.selectedBackupIndex >= 0)
			{
				this.DeselectCurrent();
				this.SelectByIndex(num, -1);
			}
			this.UpdateButtons();
		}
	}

	// Token: 0x0600084C RID: 2124 RVA: 0x000423F3 File Offset: 0x000405F3
	public void ShowCloudQuotaWarning()
	{
		UnifiedPopup.Push(new WarningPopup("$menu_cloudstoragefull", "$menu_cloudstoragefulloperationfailed", delegate()
		{
			UnifiedPopup.Pop();
		}, true));
	}

	// Token: 0x0600084D RID: 2125 RVA: 0x00042429 File Offset: 0x00040629
	public void ShowReloadError()
	{
		UnifiedPopup.Push(new WarningPopup("$menu_reloadfailed", "$menu_checklogfile", delegate()
		{
			UnifiedPopup.Pop();
		}, true));
	}

	// Token: 0x0600084E RID: 2126 RVA: 0x0004245F File Offset: 0x0004065F
	private void PushPleaseWait()
	{
		if (this.pleaseWaitCount == 0)
		{
			this.pleaseWait.SetActive(true);
		}
		this.pleaseWaitCount++;
	}

	// Token: 0x0600084F RID: 2127 RVA: 0x00042483 File Offset: 0x00040683
	private void PopPleaseWait()
	{
		this.pleaseWaitCount--;
		if (this.pleaseWaitCount == 0)
		{
			this.pleaseWait.SetActive(false);
		}
	}

	// Token: 0x06000851 RID: 2129 RVA: 0x000424D3 File Offset: 0x000406D3
	[CompilerGenerated]
	internal static void <RestoreBackup>g__RestoreBackupFailed|32_3()
	{
		UnifiedPopup.Push(new WarningPopup("$menu_backuprestorefailedheader", "$menu_tryagainorrestart", delegate()
		{
			UnifiedPopup.Pop();
		}, true));
	}

	// Token: 0x06000853 RID: 2131 RVA: 0x000425AA File Offset: 0x000407AA
	[CompilerGenerated]
	internal static void <DeleteSaveFile>g__DeleteSaveFailed|37_2()
	{
		UnifiedPopup.Push(new WarningPopup("$menu_deletefailedheader", "$menu_tryagainorrestart", delegate()
		{
			UnifiedPopup.Pop();
		}, true));
	}

	// Token: 0x06000854 RID: 2132 RVA: 0x000425E0 File Offset: 0x000407E0
	[CompilerGenerated]
	internal static void <MoveSource>g__MoveSourceFailed|38_2()
	{
		UnifiedPopup.Push(new WarningPopup("$menu_movefailedheader", "$menu_tryagainorrestart", delegate()
		{
			UnifiedPopup.Pop();
		}, true));
	}

	// Token: 0x04000A50 RID: 2640
	[SerializeField]
	private Button backButton;

	// Token: 0x04000A51 RID: 2641
	[SerializeField]
	private Button removeButton;

	// Token: 0x04000A52 RID: 2642
	[SerializeField]
	private Button moveButton;

	// Token: 0x04000A53 RID: 2643
	[SerializeField]
	private Button actionButton;

	// Token: 0x04000A54 RID: 2644
	[SerializeField]
	private GameObject saveElement;

	// Token: 0x04000A55 RID: 2645
	[SerializeField]
	private Text storageUsed;

	// Token: 0x04000A56 RID: 2646
	[SerializeField]
	private TabHandler tabHandler;

	// Token: 0x04000A57 RID: 2647
	[SerializeField]
	private RectTransform storageBar;

	// Token: 0x04000A58 RID: 2648
	[SerializeField]
	private RectTransform listRoot;

	// Token: 0x04000A59 RID: 2649
	[SerializeField]
	private ScrollRectEnsureVisible scrollRectEnsureVisible;

	// Token: 0x04000A5A RID: 2650
	[SerializeField]
	private UIGamePad blockerInfo;

	// Token: 0x04000A5B RID: 2651
	[SerializeField]
	private GameObject pleaseWait;

	// Token: 0x04000A5C RID: 2652
	private SaveWithBackups[] currentList;

	// Token: 0x04000A5D RID: 2653
	private SaveDataType currentListType;

	// Token: 0x04000A5E RID: 2654
	private DateTime mostRecentBackupCreatedTime = DateTime.MinValue;

	// Token: 0x04000A5F RID: 2655
	private List<ManageSavesMenuElement> listElements = new List<ManageSavesMenuElement>();

	// Token: 0x04000A60 RID: 2656
	private bool elementHeightChanged;

	// Token: 0x04000A61 RID: 2657
	private ManageSavesMenu.ClosedCallback closedCallback;

	// Token: 0x04000A62 RID: 2658
	private ManageSavesMenu.SavesModifiedCallback savesModifiedCallback;

	// Token: 0x04000A63 RID: 2659
	private string m_queuedNameToSelect;

	// Token: 0x04000A64 RID: 2660
	private int selectedSaveIndex = -1;

	// Token: 0x04000A65 RID: 2661
	private int selectedBackupIndex = -1;

	// Token: 0x04000A66 RID: 2662
	private float timeClicked;

	// Token: 0x04000A67 RID: 2663
	private const float doubleClickTime = 0.5f;

	// Token: 0x04000A68 RID: 2664
	private int pleaseWaitCount;

	// Token: 0x020000BD RID: 189
	// (Invoke) Token: 0x06000856 RID: 2134
	public delegate void ClosedCallback();

	// Token: 0x020000BE RID: 190
	// (Invoke) Token: 0x0600085A RID: 2138
	public delegate void SavesModifiedCallback(SaveDataType list);

	// Token: 0x020000BF RID: 191
	// (Invoke) Token: 0x0600085E RID: 2142
	private delegate void UpdateCloudUsageFinishedCallback();

	// Token: 0x020000C0 RID: 192
	// (Invoke) Token: 0x06000862 RID: 2146
	private delegate void ReloadSavesFinishedCallback(bool success);

	// Token: 0x020000C1 RID: 193
	// (Invoke) Token: 0x06000866 RID: 2150
	private delegate void UpdateGuiListFinishedCallback();
}
