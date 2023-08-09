using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200010B RID: 267
public class Tutorial : MonoBehaviour
{
	// Token: 0x17000062 RID: 98
	// (get) Token: 0x06000AB9 RID: 2745 RVA: 0x0005119C File Offset: 0x0004F39C
	public static Tutorial instance
	{
		get
		{
			return Tutorial.m_instance;
		}
	}

	// Token: 0x06000ABA RID: 2746 RVA: 0x000511A3 File Offset: 0x0004F3A3
	private void Awake()
	{
		Tutorial.m_instance = this;
		this.m_windowRoot.gameObject.SetActive(false);
	}

	// Token: 0x06000ABB RID: 2747 RVA: 0x000511BC File Offset: 0x0004F3BC
	private void Update()
	{
		if (ZoneSystem.instance && Player.m_localPlayer && DateTime.Now > this.m_lastGlobalKeyCheck + TimeSpan.FromSeconds((double)this.m_GlobalKeyCheckRateSec))
		{
			this.m_lastGlobalKeyCheck = DateTime.Now;
			foreach (Tutorial.TutorialText tutorialText in this.m_texts)
			{
				if (!string.IsNullOrEmpty(tutorialText.m_globalKeyTrigger) && ZoneSystem.instance.GetGlobalKey(tutorialText.m_globalKeyTrigger))
				{
					Player.m_localPlayer.ShowTutorial(tutorialText.m_globalKeyTrigger, false);
				}
			}
		}
	}

	// Token: 0x06000ABC RID: 2748 RVA: 0x00051284 File Offset: 0x0004F484
	public void ShowText(string name, bool force)
	{
		Tutorial.TutorialText tutorialText = this.m_texts.Find((Tutorial.TutorialText x) => x.m_name == name);
		if (tutorialText != null)
		{
			this.SpawnRaven(tutorialText.m_name, tutorialText.m_topic, tutorialText.m_text, tutorialText.m_label, tutorialText.m_isMunin);
			return;
		}
		Debug.Log("Missing tutorial text for: " + name);
	}

	// Token: 0x06000ABD RID: 2749 RVA: 0x000512F3 File Offset: 0x0004F4F3
	private void SpawnRaven(string key, string topic, string text, string label, bool munin)
	{
		if (!Raven.IsInstantiated())
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_ravenPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
		}
		Raven.AddTempText(key, topic, text, label, munin);
	}

	// Token: 0x04000CFD RID: 3325
	public List<Tutorial.TutorialText> m_texts = new List<Tutorial.TutorialText>();

	// Token: 0x04000CFE RID: 3326
	public int m_GlobalKeyCheckRateSec = 10;

	// Token: 0x04000CFF RID: 3327
	public RectTransform m_windowRoot;

	// Token: 0x04000D00 RID: 3328
	public Text m_topic;

	// Token: 0x04000D01 RID: 3329
	public Text m_text;

	// Token: 0x04000D02 RID: 3330
	public GameObject m_ravenPrefab;

	// Token: 0x04000D03 RID: 3331
	private static Tutorial m_instance;

	// Token: 0x04000D04 RID: 3332
	private Queue<string> m_tutQueue = new Queue<string>();

	// Token: 0x04000D05 RID: 3333
	private DateTime m_lastGlobalKeyCheck;

	// Token: 0x0200010C RID: 268
	[Serializable]
	public class TutorialText
	{
		// Token: 0x04000D06 RID: 3334
		public string m_name;

		// Token: 0x04000D07 RID: 3335
		public string m_globalKeyTrigger;

		// Token: 0x04000D08 RID: 3336
		public string m_topic = "";

		// Token: 0x04000D09 RID: 3337
		public string m_label = "";

		// Token: 0x04000D0A RID: 3338
		public bool m_isMunin;

		// Token: 0x04000D0B RID: 3339
		[TextArea]
		public string m_text = "";
	}
}
