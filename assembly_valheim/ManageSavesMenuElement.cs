using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x020000CE RID: 206
public class ManageSavesMenuElement : MonoBehaviour
{
	// Token: 0x17000039 RID: 57
	// (get) Token: 0x0600089B RID: 2203 RVA: 0x00043355 File Offset: 0x00041555
	public RectTransform rectTransform
	{
		get
		{
			return base.transform as RectTransform;
		}
	}

	// Token: 0x1700003A RID: 58
	// (get) Token: 0x0600089C RID: 2204 RVA: 0x00043362 File Offset: 0x00041562
	private RectTransform arrowRectTransform
	{
		get
		{
			return this.arrow.transform as RectTransform;
		}
	}

	// Token: 0x14000001 RID: 1
	// (add) Token: 0x0600089D RID: 2205 RVA: 0x00043374 File Offset: 0x00041574
	// (remove) Token: 0x0600089E RID: 2206 RVA: 0x000433AC File Offset: 0x000415AC
	public event ManageSavesMenuElement.HeightChangedHandler HeightChanged;

	// Token: 0x14000002 RID: 2
	// (add) Token: 0x0600089F RID: 2207 RVA: 0x000433E4 File Offset: 0x000415E4
	// (remove) Token: 0x060008A0 RID: 2208 RVA: 0x0004341C File Offset: 0x0004161C
	public event ManageSavesMenuElement.ElementClickedHandler ElementClicked;

	// Token: 0x14000003 RID: 3
	// (add) Token: 0x060008A1 RID: 2209 RVA: 0x00043454 File Offset: 0x00041654
	// (remove) Token: 0x060008A2 RID: 2210 RVA: 0x0004348C File Offset: 0x0004168C
	public event ManageSavesMenuElement.ElementExpandedChangedHandler ElementExpandedChanged;

	// Token: 0x1700003B RID: 59
	// (get) Token: 0x060008A3 RID: 2211 RVA: 0x000434C1 File Offset: 0x000416C1
	// (set) Token: 0x060008A4 RID: 2212 RVA: 0x000434C9 File Offset: 0x000416C9
	public bool IsExpanded { get; private set; }

	// Token: 0x1700003C RID: 60
	// (get) Token: 0x060008A5 RID: 2213 RVA: 0x000434D2 File Offset: 0x000416D2
	public int BackupCount
	{
		get
		{
			return this.backupElements.Count;
		}
	}

	// Token: 0x1700003D RID: 61
	// (get) Token: 0x060008A6 RID: 2214 RVA: 0x000434DF File Offset: 0x000416DF
	// (set) Token: 0x060008A7 RID: 2215 RVA: 0x000434E7 File Offset: 0x000416E7
	public SaveWithBackups Save { get; private set; }

	// Token: 0x060008A8 RID: 2216 RVA: 0x000434F0 File Offset: 0x000416F0
	public void SetUp(SaveWithBackups save)
	{
		this.UpdatePrimaryElement();
		for (int i = 0; i < this.Save.BackupFiles.Length; i++)
		{
			ManageSavesMenuElement.BackupElement item = this.CreateBackupElement(this.Save.BackupFiles[i], i);
			this.backupElements.Add(item);
		}
		this.UpdateElementPositions();
	}

	// Token: 0x060008A9 RID: 2217 RVA: 0x00043543 File Offset: 0x00041743
	public IEnumerator SetUpEnumerator(SaveWithBackups save)
	{
		this.Save = save;
		this.UpdatePrimaryElement();
		yield return null;
		int num;
		for (int i = 0; i < this.Save.BackupFiles.Length; i = num + 1)
		{
			ManageSavesMenuElement.BackupElement item = this.CreateBackupElement(this.Save.BackupFiles[i], i);
			this.backupElements.Add(item);
			yield return null;
			num = i;
		}
		IEnumerator updateElementPositions = this.UpdateElementPositionsEnumerator();
		while (updateElementPositions.MoveNext())
		{
			yield return null;
		}
		yield break;
	}

	// Token: 0x060008AA RID: 2218 RVA: 0x0004355C File Offset: 0x0004175C
	public void UpdateElement(SaveWithBackups save)
	{
		this.Save = save;
		this.UpdatePrimaryElement();
		List<ManageSavesMenuElement.BackupElement> list = new List<ManageSavesMenuElement.BackupElement>();
		Dictionary<string, Dictionary<FileHelpers.FileSource, ManageSavesMenuElement.BackupElement>> dictionary = new Dictionary<string, Dictionary<FileHelpers.FileSource, ManageSavesMenuElement.BackupElement>>();
		for (int i = 0; i < this.backupElements.Count; i++)
		{
			if (!dictionary.ContainsKey(this.backupElements[i].File.FileName))
			{
				dictionary.Add(this.backupElements[i].File.FileName, new Dictionary<FileHelpers.FileSource, ManageSavesMenuElement.BackupElement>());
			}
			dictionary[this.backupElements[i].File.FileName].Add(this.backupElements[i].File.m_source, this.backupElements[i]);
		}
		for (int j = 0; j < this.Save.BackupFiles.Length; j++)
		{
			SaveFile saveFile = this.Save.BackupFiles[j];
			if (dictionary.ContainsKey(saveFile.FileName) && dictionary[saveFile.FileName].ContainsKey(saveFile.m_source))
			{
				int currentIndex = j;
				dictionary[saveFile.FileName][saveFile.m_source].UpdateElement(saveFile, delegate
				{
					this.OnBackupElementClicked(currentIndex);
				});
				list.Add(dictionary[saveFile.FileName][saveFile.m_source]);
				dictionary[saveFile.FileName].Remove(saveFile.m_source);
				if (dictionary.Count <= 0)
				{
					dictionary.Remove(saveFile.FileName);
				}
			}
			else
			{
				ManageSavesMenuElement.BackupElement item = this.CreateBackupElement(saveFile, j);
				list.Add(item);
			}
		}
		foreach (KeyValuePair<string, Dictionary<FileHelpers.FileSource, ManageSavesMenuElement.BackupElement>> keyValuePair in dictionary)
		{
			foreach (KeyValuePair<FileHelpers.FileSource, ManageSavesMenuElement.BackupElement> keyValuePair2 in keyValuePair.Value)
			{
				UnityEngine.Object.Destroy(keyValuePair2.Value.GuiInstance);
			}
		}
		this.backupElements = list;
		float num = this.UpdateElementPositions();
		this.rectTransform.sizeDelta = new Vector2(this.rectTransform.sizeDelta.x, this.IsExpanded ? num : this.elementHeight);
	}

	// Token: 0x060008AB RID: 2219 RVA: 0x000437F8 File Offset: 0x000419F8
	public IEnumerator UpdateElementEnumerator(SaveWithBackups save)
	{
		this.Save = save;
		this.UpdatePrimaryElement();
		List<ManageSavesMenuElement.BackupElement> newBackupElementsList = new List<ManageSavesMenuElement.BackupElement>();
		Dictionary<string, Dictionary<FileHelpers.FileSource, ManageSavesMenuElement.BackupElement>> backupNameToElementMap = new Dictionary<string, Dictionary<FileHelpers.FileSource, ManageSavesMenuElement.BackupElement>>();
		int num;
		for (int i = 0; i < this.backupElements.Count; i = num + 1)
		{
			if (!backupNameToElementMap.ContainsKey(this.backupElements[i].File.FileName))
			{
				backupNameToElementMap.Add(this.backupElements[i].File.FileName, new Dictionary<FileHelpers.FileSource, ManageSavesMenuElement.BackupElement>());
			}
			backupNameToElementMap[this.backupElements[i].File.FileName].Add(this.backupElements[i].File.m_source, this.backupElements[i]);
			yield return null;
			num = i;
		}
		for (int i = 0; i < this.Save.BackupFiles.Length; i = num + 1)
		{
			SaveFile saveFile = this.Save.BackupFiles[i];
			if (backupNameToElementMap.ContainsKey(saveFile.FileName) && backupNameToElementMap[saveFile.FileName].ContainsKey(saveFile.m_source))
			{
				int currentIndex = i;
				backupNameToElementMap[saveFile.FileName][saveFile.m_source].UpdateElement(saveFile, delegate
				{
					this.OnBackupElementClicked(currentIndex);
				});
				newBackupElementsList.Add(backupNameToElementMap[saveFile.FileName][saveFile.m_source]);
				backupNameToElementMap[saveFile.FileName].Remove(saveFile.m_source);
				if (backupNameToElementMap.Count <= 0)
				{
					backupNameToElementMap.Remove(saveFile.FileName);
				}
			}
			else
			{
				ManageSavesMenuElement.BackupElement item = this.CreateBackupElement(saveFile, i);
				newBackupElementsList.Add(item);
			}
			yield return null;
			num = i;
		}
		foreach (KeyValuePair<string, Dictionary<FileHelpers.FileSource, ManageSavesMenuElement.BackupElement>> keyValuePair in backupNameToElementMap)
		{
			foreach (KeyValuePair<FileHelpers.FileSource, ManageSavesMenuElement.BackupElement> keyValuePair2 in keyValuePair.Value)
			{
				UnityEngine.Object.Destroy(keyValuePair2.Value.GuiInstance);
				yield return null;
			}
			Dictionary<FileHelpers.FileSource, ManageSavesMenuElement.BackupElement>.Enumerator enumerator2 = default(Dictionary<FileHelpers.FileSource, ManageSavesMenuElement.BackupElement>.Enumerator);
		}
		Dictionary<string, Dictionary<FileHelpers.FileSource, ManageSavesMenuElement.BackupElement>>.Enumerator enumerator = default(Dictionary<string, Dictionary<FileHelpers.FileSource, ManageSavesMenuElement.BackupElement>>.Enumerator);
		this.backupElements = newBackupElementsList;
		float num2 = this.UpdateElementPositions();
		this.rectTransform.sizeDelta = new Vector2(this.rectTransform.sizeDelta.x, this.IsExpanded ? num2 : this.elementHeight);
		yield break;
		yield break;
	}

	// Token: 0x060008AC RID: 2220 RVA: 0x00043810 File Offset: 0x00041A10
	private ManageSavesMenuElement.BackupElement CreateBackupElement(SaveFile backup, int index)
	{
		return new ManageSavesMenuElement.BackupElement(UnityEngine.Object.Instantiate<GameObject>(this.backupElement.gameObject, this.rectTransform), backup, delegate()
		{
			this.OnBackupElementClicked(index);
		});
	}

	// Token: 0x060008AD RID: 2221 RVA: 0x0004385C File Offset: 0x00041A5C
	private float UpdateElementPositions()
	{
		float num = this.elementHeight;
		for (int i = 0; i < this.backupElements.Count; i++)
		{
			this.backupElements[i].rectTransform.anchoredPosition = new Vector2(this.backupElements[i].rectTransform.anchoredPosition.x, -num);
			num += this.backupElements[i].rectTransform.sizeDelta.y;
		}
		return num;
	}

	// Token: 0x060008AE RID: 2222 RVA: 0x000438DD File Offset: 0x00041ADD
	private IEnumerator UpdateElementPositionsEnumerator()
	{
		float pos = this.elementHeight;
		int num;
		for (int i = 0; i < this.backupElements.Count; i = num + 1)
		{
			this.backupElements[i].rectTransform.anchoredPosition = new Vector2(this.backupElements[i].rectTransform.anchoredPosition.x, -pos);
			pos += this.backupElements[i].rectTransform.sizeDelta.y;
			yield return null;
			num = i;
		}
		yield break;
	}

	// Token: 0x060008AF RID: 2223 RVA: 0x000438EC File Offset: 0x00041AEC
	private void UpdatePrimaryElement()
	{
		this.arrow.gameObject.SetActive(this.Save.BackupFiles.Length != 0);
		string text = this.Save.m_name;
		if (!this.Save.IsDeleted)
		{
			text = this.Save.PrimaryFile.FileName;
			if (SaveSystem.IsCorrupt(this.Save.PrimaryFile))
			{
				text += " [CORRUPT]";
			}
			if (SaveSystem.IsWorldWithMissingMetaFile(this.Save.PrimaryFile))
			{
				text += " [MISSING META]";
			}
		}
		this.nameText.text = text;
		this.sizeText.text = FileHelpers.BytesAsNumberString(this.Save.IsDeleted ? 0UL : this.Save.PrimaryFile.Size, 1U) + "/" + FileHelpers.BytesAsNumberString(this.Save.SizeWithBackups, 1U);
		this.backupCountText.text = Localization.instance.Localize("$menu_backupcount", new string[]
		{
			this.Save.BackupFiles.Length.ToString()
		});
		this.dateText.text = (this.Save.IsDeleted ? Localization.instance.Localize("$menu_deleted") : this.Save.PrimaryFile.LastModified.ToShortDateString());
		Transform transform = this.sourceParent.Find("source_cloud");
		if (transform != null)
		{
			transform.gameObject.SetActive(!this.Save.IsDeleted && this.Save.PrimaryFile.m_source == FileHelpers.FileSource.Cloud);
		}
		Transform transform2 = this.sourceParent.Find("source_local");
		if (transform2 != null)
		{
			transform2.gameObject.SetActive(!this.Save.IsDeleted && this.Save.PrimaryFile.m_source == FileHelpers.FileSource.Local);
		}
		Transform transform3 = this.sourceParent.Find("source_legacy");
		if (transform3 != null)
		{
			transform3.gameObject.SetActive(!this.Save.IsDeleted && this.Save.PrimaryFile.m_source == FileHelpers.FileSource.Legacy);
		}
		if (this.IsExpanded && this.Save.BackupFiles.Length == 0)
		{
			this.SetExpanded(false, false);
		}
	}

	// Token: 0x060008B0 RID: 2224 RVA: 0x00043B38 File Offset: 0x00041D38
	private void OnDestroy()
	{
		foreach (ManageSavesMenuElement.BackupElement backupElement in this.backupElements)
		{
			UnityEngine.Object.Destroy(backupElement.GuiInstance);
		}
		this.backupElements.Clear();
	}

	// Token: 0x060008B1 RID: 2225 RVA: 0x00043B98 File Offset: 0x00041D98
	private void Start()
	{
		this.elementHeight = this.rectTransform.sizeDelta.y;
	}

	// Token: 0x060008B2 RID: 2226 RVA: 0x00043BB0 File Offset: 0x00041DB0
	private void OnEnable()
	{
		this.primaryElement.onClick.AddListener(new UnityAction(this.OnElementClicked));
		this.arrow.onClick.AddListener(new UnityAction(this.OnArrowClicked));
	}

	// Token: 0x060008B3 RID: 2227 RVA: 0x00043BEA File Offset: 0x00041DEA
	private void OnDisable()
	{
		this.primaryElement.onClick.RemoveListener(new UnityAction(this.OnElementClicked));
		this.arrow.onClick.RemoveListener(new UnityAction(this.OnArrowClicked));
	}

	// Token: 0x060008B4 RID: 2228 RVA: 0x00043C24 File Offset: 0x00041E24
	private void OnElementClicked()
	{
		ManageSavesMenuElement.ElementClickedHandler elementClicked = this.ElementClicked;
		if (elementClicked == null)
		{
			return;
		}
		elementClicked(this, -1);
	}

	// Token: 0x060008B5 RID: 2229 RVA: 0x00043C38 File Offset: 0x00041E38
	private void OnBackupElementClicked(int index)
	{
		ManageSavesMenuElement.ElementClickedHandler elementClicked = this.ElementClicked;
		if (elementClicked == null)
		{
			return;
		}
		elementClicked(this, index);
	}

	// Token: 0x060008B6 RID: 2230 RVA: 0x00043C4C File Offset: 0x00041E4C
	private void OnArrowClicked()
	{
		this.SetExpanded(!this.IsExpanded, true);
	}

	// Token: 0x060008B7 RID: 2231 RVA: 0x00043C60 File Offset: 0x00041E60
	public void SetExpanded(bool value, bool animated = true)
	{
		if (this.IsExpanded == value)
		{
			return;
		}
		this.IsExpanded = value;
		ManageSavesMenuElement.ElementExpandedChangedHandler elementExpandedChanged = this.ElementExpandedChanged;
		if (elementExpandedChanged != null)
		{
			elementExpandedChanged(this, this.IsExpanded);
		}
		if (this.arrowAnimationCoroutine != null)
		{
			base.StopCoroutine(this.arrowAnimationCoroutine);
		}
		if (this.listAnimationCoroutine != null)
		{
			base.StopCoroutine(this.listAnimationCoroutine);
		}
		if (animated)
		{
			this.arrowAnimationCoroutine = base.StartCoroutine(this.AnimateArrow());
			this.listAnimationCoroutine = base.StartCoroutine(this.AnimateList());
			return;
		}
		float z = (float)(this.IsExpanded ? 0 : 90);
		this.arrowRectTransform.rotation = Quaternion.Euler(0f, 0f, z);
		float y = this.IsExpanded ? (this.elementHeight * (float)(this.backupElements.Count + 1)) : this.elementHeight;
		this.rectTransform.sizeDelta = new Vector2(this.rectTransform.sizeDelta.x, y);
		ManageSavesMenuElement.HeightChangedHandler heightChanged = this.HeightChanged;
		if (heightChanged == null)
		{
			return;
		}
		heightChanged();
	}

	// Token: 0x060008B8 RID: 2232 RVA: 0x00043D68 File Offset: 0x00041F68
	public void Select(ref int backupIndex)
	{
		if (backupIndex < 0 || this.BackupCount <= 0)
		{
			this.selectedBackground.gameObject.SetActive(true);
			backupIndex = -1;
			return;
		}
		backupIndex = Mathf.Clamp(backupIndex, 0, this.BackupCount - 1);
		this.backupElements[backupIndex].rectTransform.Find("selected").gameObject.SetActive(true);
	}

	// Token: 0x060008B9 RID: 2233 RVA: 0x00043DD4 File Offset: 0x00041FD4
	public void Deselect(int backupIndex = -1)
	{
		if (backupIndex < 0)
		{
			this.selectedBackground.gameObject.SetActive(false);
			return;
		}
		if (backupIndex > this.backupElements.Count - 1)
		{
			ZLog.LogWarning(string.Concat(new string[]
			{
				"Failed to deselect backup: Index ",
				backupIndex.ToString(),
				" was outside of the valid range -1-",
				(this.backupElements.Count - 1).ToString(),
				". Ignoring."
			}));
			return;
		}
		this.backupElements[backupIndex].rectTransform.Find("selected").gameObject.SetActive(false);
	}

	// Token: 0x060008BA RID: 2234 RVA: 0x00043E78 File Offset: 0x00042078
	public RectTransform GetTransform(int backupIndex = -1)
	{
		if (backupIndex < 0)
		{
			return this.primaryElement.transform as RectTransform;
		}
		return this.backupElements[backupIndex].rectTransform;
	}

	// Token: 0x060008BB RID: 2235 RVA: 0x00043EA0 File Offset: 0x000420A0
	private IEnumerator AnimateArrow()
	{
		float currentRotation = this.arrowRectTransform.rotation.eulerAngles.z;
		float targetRotation = (float)(this.IsExpanded ? 0 : 90);
		float sign = Mathf.Sign(targetRotation - currentRotation);
		for (;;)
		{
			currentRotation += sign * 90f * 10f * Time.deltaTime;
			if (currentRotation * sign > targetRotation * sign)
			{
				currentRotation = targetRotation;
			}
			this.arrowRectTransform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
			if (currentRotation == targetRotation)
			{
				break;
			}
			yield return null;
		}
		this.arrowAnimationCoroutine = null;
		yield break;
	}

	// Token: 0x060008BC RID: 2236 RVA: 0x00043EAF File Offset: 0x000420AF
	private IEnumerator AnimateList()
	{
		float currentSize = this.rectTransform.sizeDelta.y;
		float targetSize = this.IsExpanded ? (this.elementHeight * (float)(this.backupElements.Count + 1)) : this.elementHeight;
		float sign = Mathf.Sign(targetSize - currentSize);
		float velocity = 0f;
		for (;;)
		{
			currentSize = Mathf.SmoothDamp(currentSize, targetSize, ref velocity, 0.06f);
			if (currentSize * sign + 0.1f > targetSize * sign)
			{
				currentSize = targetSize;
			}
			this.rectTransform.sizeDelta = new Vector2(this.rectTransform.sizeDelta.x, currentSize);
			ManageSavesMenuElement.HeightChangedHandler heightChanged = this.HeightChanged;
			if (heightChanged != null)
			{
				heightChanged();
			}
			if (currentSize == targetSize)
			{
				break;
			}
			yield return null;
		}
		this.listAnimationCoroutine = null;
		yield break;
	}

	// Token: 0x04000AA2 RID: 2722
	[SerializeField]
	private Button primaryElement;

	// Token: 0x04000AA3 RID: 2723
	[SerializeField]
	private Button backupElement;

	// Token: 0x04000AA4 RID: 2724
	[SerializeField]
	private GameObject selectedBackground;

	// Token: 0x04000AA5 RID: 2725
	[SerializeField]
	private Button arrow;

	// Token: 0x04000AA6 RID: 2726
	[SerializeField]
	private Text nameText;

	// Token: 0x04000AA7 RID: 2727
	[SerializeField]
	private Text sizeText;

	// Token: 0x04000AA8 RID: 2728
	[SerializeField]
	private Text backupCountText;

	// Token: 0x04000AA9 RID: 2729
	[SerializeField]
	private Text dateText;

	// Token: 0x04000AAA RID: 2730
	[SerializeField]
	private RectTransform sourceParent;

	// Token: 0x04000AAE RID: 2734
	private float elementHeight = 32f;

	// Token: 0x04000AAF RID: 2735
	private List<ManageSavesMenuElement.BackupElement> backupElements = new List<ManageSavesMenuElement.BackupElement>();

	// Token: 0x04000AB2 RID: 2738
	private Coroutine arrowAnimationCoroutine;

	// Token: 0x04000AB3 RID: 2739
	private Coroutine listAnimationCoroutine;

	// Token: 0x020000CF RID: 207
	// (Invoke) Token: 0x060008BF RID: 2239
	public delegate void BackupElementClickedHandler();

	// Token: 0x020000D0 RID: 208
	private class BackupElement
	{
		// Token: 0x060008C2 RID: 2242 RVA: 0x00043EDC File Offset: 0x000420DC
		public BackupElement(GameObject guiInstance, SaveFile backup, ManageSavesMenuElement.BackupElementClickedHandler clickedCallback)
		{
			this.GuiInstance = guiInstance;
			this.GuiInstance.SetActive(true);
			this.Button = this.GuiInstance.GetComponent<Button>();
			this.UpdateElement(backup, clickedCallback);
		}

		// Token: 0x060008C3 RID: 2243 RVA: 0x00043F10 File Offset: 0x00042110
		public void UpdateElement(SaveFile backup, ManageSavesMenuElement.BackupElementClickedHandler clickedCallback)
		{
			this.File = backup;
			this.Button.onClick.RemoveAllListeners();
			this.Button.onClick.AddListener(delegate
			{
				ManageSavesMenuElement.BackupElementClickedHandler clickedCallback2 = clickedCallback;
				if (clickedCallback2 == null)
				{
					return;
				}
				clickedCallback2();
			});
			string text = backup.FileName;
			if (SaveSystem.IsCorrupt(backup))
			{
				text += " [CORRUPT]";
			}
			if (SaveSystem.IsWorldWithMissingMetaFile(backup))
			{
				text += " [MISSING META FILE]";
			}
			this.rectTransform.Find("name").GetComponent<Text>().text = text;
			this.rectTransform.Find("size").GetComponent<Text>().text = FileHelpers.BytesAsNumberString(backup.Size, 1U);
			this.rectTransform.Find("date").GetComponent<Text>().text = backup.LastModified.ToShortDateString();
			Transform transform = this.rectTransform.Find("source");
			Transform transform2 = transform.Find("source_cloud");
			if (transform2 != null)
			{
				transform2.gameObject.SetActive(backup.m_source == FileHelpers.FileSource.Cloud);
			}
			Transform transform3 = transform.Find("source_local");
			if (transform3 != null)
			{
				transform3.gameObject.SetActive(backup.m_source == FileHelpers.FileSource.Local);
			}
			Transform transform4 = transform.Find("source_legacy");
			if (transform4 == null)
			{
				return;
			}
			transform4.gameObject.SetActive(backup.m_source == FileHelpers.FileSource.Legacy);
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060008C4 RID: 2244 RVA: 0x0004406F File Offset: 0x0004226F
		// (set) Token: 0x060008C5 RID: 2245 RVA: 0x00044077 File Offset: 0x00042277
		public SaveFile File { get; private set; }

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060008C6 RID: 2246 RVA: 0x00044080 File Offset: 0x00042280
		// (set) Token: 0x060008C7 RID: 2247 RVA: 0x00044088 File Offset: 0x00042288
		public GameObject GuiInstance { get; private set; }

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060008C8 RID: 2248 RVA: 0x00044091 File Offset: 0x00042291
		// (set) Token: 0x060008C9 RID: 2249 RVA: 0x00044099 File Offset: 0x00042299
		public Button Button { get; private set; }

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060008CA RID: 2250 RVA: 0x000440A2 File Offset: 0x000422A2
		public RectTransform rectTransform
		{
			get
			{
				return this.GuiInstance.transform as RectTransform;
			}
		}
	}

	// Token: 0x020000D2 RID: 210
	// (Invoke) Token: 0x060008CE RID: 2254
	public delegate void HeightChangedHandler();

	// Token: 0x020000D3 RID: 211
	// (Invoke) Token: 0x060008D2 RID: 2258
	public delegate void ElementClickedHandler(ManageSavesMenuElement element, int backupElementIndex);

	// Token: 0x020000D4 RID: 212
	// (Invoke) Token: 0x060008D6 RID: 2262
	public delegate void ElementExpandedChangedHandler(ManageSavesMenuElement element, bool isExpanded);
}
