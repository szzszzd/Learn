using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000E0 RID: 224
public class MessageHud : MonoBehaviour
{
	// Token: 0x06000927 RID: 2343 RVA: 0x0004566A File Offset: 0x0004386A
	private void Awake()
	{
		MessageHud.m_instance = this;
	}

	// Token: 0x06000928 RID: 2344 RVA: 0x00045672 File Offset: 0x00043872
	private void OnDestroy()
	{
		MessageHud.m_instance = null;
	}

	// Token: 0x17000051 RID: 81
	// (get) Token: 0x06000929 RID: 2345 RVA: 0x0004567A File Offset: 0x0004387A
	public static MessageHud instance
	{
		get
		{
			return MessageHud.m_instance;
		}
	}

	// Token: 0x0600092A RID: 2346 RVA: 0x00045684 File Offset: 0x00043884
	private void Start()
	{
		this.m_messageText.canvasRenderer.SetAlpha(0f);
		this.m_messageIcon.canvasRenderer.SetAlpha(0f);
		this.m_messageCenterText.canvasRenderer.SetAlpha(0f);
		for (int i = 0; i < this.m_maxUnlockMessages; i++)
		{
			this.m_unlockMessages.Add(null);
		}
		ZRoutedRpc.instance.Register<int, string>("ShowMessage", new Action<long, int, string>(this.RPC_ShowMessage));
	}

	// Token: 0x0600092B RID: 2347 RVA: 0x00045708 File Offset: 0x00043908
	private void Update()
	{
		if (Hud.IsUserHidden())
		{
			this.HideAll();
			return;
		}
		this.UpdateUnlockMsg(Time.deltaTime);
		this.UpdateMessage(Time.deltaTime);
		this.UpdateBiomeFound(Time.deltaTime);
	}

	// Token: 0x0600092C RID: 2348 RVA: 0x0004573C File Offset: 0x0004393C
	private void HideAll()
	{
		for (int i = 0; i < this.m_maxUnlockMessages; i++)
		{
			if (this.m_unlockMessages[i] != null)
			{
				UnityEngine.Object.Destroy(this.m_unlockMessages[i]);
				this.m_unlockMessages[i] = null;
			}
		}
		this.m_messageText.canvasRenderer.SetAlpha(0f);
		this.m_messageIcon.canvasRenderer.SetAlpha(0f);
		this.m_messageCenterText.canvasRenderer.SetAlpha(0f);
		if (this.m_biomeMsgInstance)
		{
			UnityEngine.Object.Destroy(this.m_biomeMsgInstance);
			this.m_biomeMsgInstance = null;
		}
	}

	// Token: 0x0600092D RID: 2349 RVA: 0x000457EA File Offset: 0x000439EA
	public void MessageAll(MessageHud.MessageType type, string text)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ShowMessage", new object[]
		{
			(int)type,
			text
		});
	}

	// Token: 0x0600092E RID: 2350 RVA: 0x00045813 File Offset: 0x00043A13
	private void RPC_ShowMessage(long sender, int type, string text)
	{
		this.ShowMessage((MessageHud.MessageType)type, text, 0, null);
	}

	// Token: 0x0600092F RID: 2351 RVA: 0x00045820 File Offset: 0x00043A20
	public void ShowMessage(MessageHud.MessageType type, string text, int amount = 0, Sprite icon = null)
	{
		if (Hud.IsUserHidden())
		{
			return;
		}
		text = Localization.instance.Localize(text);
		if (type == MessageHud.MessageType.TopLeft)
		{
			MessageHud.MsgData msgData = new MessageHud.MsgData();
			msgData.m_icon = icon;
			msgData.m_text = text;
			msgData.m_amount = amount;
			this.m_msgQeue.Enqueue(msgData);
			this.AddLog(text);
			return;
		}
		if (type != MessageHud.MessageType.Center)
		{
			return;
		}
		this.m_messageCenterText.text = text;
		this.m_messageCenterText.canvasRenderer.SetAlpha(1f);
		this.m_messageCenterText.CrossFadeAlpha(0f, 4f, true);
	}

	// Token: 0x06000930 RID: 2352 RVA: 0x000458B4 File Offset: 0x00043AB4
	private void UpdateMessage(float dt)
	{
		this.m_msgQueueTimer += dt;
		if (this.m_msgQeue.Count > 0)
		{
			MessageHud.MsgData msgData = this.m_msgQeue.Peek();
			bool flag = this.m_msgQueueTimer < 4f && msgData.m_text == this.currentMsg.m_text && msgData.m_icon == this.currentMsg.m_icon;
			if (this.m_msgQueueTimer >= 1f || flag)
			{
				MessageHud.MsgData msgData2 = this.m_msgQeue.Dequeue();
				this.m_messageText.text = msgData2.m_text;
				if (flag)
				{
					msgData2.m_amount += this.currentMsg.m_amount;
				}
				if (msgData2.m_amount > 1)
				{
					Text messageText = this.m_messageText;
					messageText.text = messageText.text + " x" + msgData2.m_amount.ToString();
				}
				this.m_messageText.canvasRenderer.SetAlpha(1f);
				this.m_messageText.CrossFadeAlpha(0f, 4f, true);
				if (msgData2.m_icon != null)
				{
					this.m_messageIcon.sprite = msgData2.m_icon;
					this.m_messageIcon.canvasRenderer.SetAlpha(1f);
					this.m_messageIcon.CrossFadeAlpha(0f, 4f, true);
				}
				else
				{
					this.m_messageIcon.canvasRenderer.SetAlpha(0f);
				}
				this.currentMsg = msgData2;
				this.m_msgQueueTimer = 0f;
			}
		}
	}

	// Token: 0x06000931 RID: 2353 RVA: 0x00045A48 File Offset: 0x00043C48
	private void UpdateBiomeFound(float dt)
	{
		if (this.m_biomeMsgInstance != null && this.m_biomeMsgInstance.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("done"))
		{
			UnityEngine.Object.Destroy(this.m_biomeMsgInstance);
			this.m_biomeMsgInstance = null;
		}
		if (this.m_biomeFoundQueue.Count > 0 && this.m_biomeMsgInstance == null && this.m_msgQeue.Count == 0 && this.m_msgQueueTimer > 2f)
		{
			MessageHud.BiomeMessage biomeMessage = this.m_biomeFoundQueue.Dequeue();
			this.m_biomeMsgInstance = UnityEngine.Object.Instantiate<GameObject>(this.m_biomeFoundPrefab, base.transform);
			Text component = Utils.FindChild(this.m_biomeMsgInstance.transform, "Title").GetComponent<Text>();
			string text = Localization.instance.Localize(biomeMessage.m_text);
			component.text = text;
			if (biomeMessage.m_playStinger && this.m_biomeFoundStinger)
			{
				UnityEngine.Object.Instantiate<GameObject>(this.m_biomeFoundStinger);
			}
		}
	}

	// Token: 0x06000932 RID: 2354 RVA: 0x00045B4C File Offset: 0x00043D4C
	public void ShowBiomeFoundMsg(string text, bool playStinger)
	{
		MessageHud.BiomeMessage biomeMessage = new MessageHud.BiomeMessage();
		biomeMessage.m_text = text;
		biomeMessage.m_playStinger = playStinger;
		this.m_biomeFoundQueue.Enqueue(biomeMessage);
	}

	// Token: 0x06000933 RID: 2355 RVA: 0x00045B7C File Offset: 0x00043D7C
	public void QueueUnlockMsg(Sprite icon, string topic, string description)
	{
		MessageHud.UnlockMsg unlockMsg = new MessageHud.UnlockMsg();
		unlockMsg.m_icon = icon;
		unlockMsg.m_topic = Localization.instance.Localize(topic);
		unlockMsg.m_description = Localization.instance.Localize(description);
		this.m_unlockMsgQueue.Enqueue(unlockMsg);
		this.AddLog(topic + ":" + description);
		ZLog.Log("Queue unlock msg:" + topic + ":" + description);
	}

	// Token: 0x06000934 RID: 2356 RVA: 0x00045BEC File Offset: 0x00043DEC
	private int GetFreeUnlockMsgSlot()
	{
		for (int i = 0; i < this.m_unlockMessages.Count; i++)
		{
			if (this.m_unlockMessages[i] == null)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x06000935 RID: 2357 RVA: 0x00045C28 File Offset: 0x00043E28
	private void UpdateUnlockMsg(float dt)
	{
		for (int i = 0; i < this.m_unlockMessages.Count; i++)
		{
			GameObject gameObject = this.m_unlockMessages[i];
			if (!(gameObject == null) && gameObject.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("done"))
			{
				UnityEngine.Object.Destroy(gameObject);
				this.m_unlockMessages[i] = null;
				break;
			}
		}
		if (this.m_unlockMsgQueue.Count > 0)
		{
			int freeUnlockMsgSlot = this.GetFreeUnlockMsgSlot();
			if (freeUnlockMsgSlot != -1)
			{
				Transform transform = base.transform;
				GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.m_unlockMsgPrefab, transform);
				this.m_unlockMessages[freeUnlockMsgSlot] = gameObject2;
				RectTransform rectTransform = gameObject2.transform as RectTransform;
				Vector3 v = rectTransform.anchoredPosition;
				v.y -= (float)(this.m_maxUnlockMsgSpace * freeUnlockMsgSlot);
				rectTransform.anchoredPosition = v;
				MessageHud.UnlockMsg unlockMsg = this.m_unlockMsgQueue.Dequeue();
				Image component = rectTransform.Find("UnlockMessage/icon_bkg/UnlockIcon").GetComponent<Image>();
				Text component2 = rectTransform.Find("UnlockMessage/UnlockTitle").GetComponent<Text>();
				Text component3 = rectTransform.Find("UnlockMessage/UnlockDescription").GetComponent<Text>();
				component.sprite = unlockMsg.m_icon;
				component2.text = unlockMsg.m_topic;
				component3.text = unlockMsg.m_description;
			}
		}
	}

	// Token: 0x06000936 RID: 2358 RVA: 0x00045D77 File Offset: 0x00043F77
	private void AddLog(string logText)
	{
		this.m_messageLog.Add(logText);
		while (this.m_messageLog.Count > this.m_maxLogMessages)
		{
			this.m_messageLog.RemoveAt(0);
		}
	}

	// Token: 0x06000937 RID: 2359 RVA: 0x00045DA6 File Offset: 0x00043FA6
	public List<string> GetLog()
	{
		return this.m_messageLog;
	}

	// Token: 0x04000AFC RID: 2812
	private MessageHud.MsgData currentMsg = new MessageHud.MsgData();

	// Token: 0x04000AFD RID: 2813
	private static MessageHud m_instance;

	// Token: 0x04000AFE RID: 2814
	public Text m_messageText;

	// Token: 0x04000AFF RID: 2815
	public Image m_messageIcon;

	// Token: 0x04000B00 RID: 2816
	public Text m_messageCenterText;

	// Token: 0x04000B01 RID: 2817
	public GameObject m_unlockMsgPrefab;

	// Token: 0x04000B02 RID: 2818
	public int m_maxUnlockMsgSpace = 110;

	// Token: 0x04000B03 RID: 2819
	public int m_maxUnlockMessages = 4;

	// Token: 0x04000B04 RID: 2820
	public int m_maxLogMessages = 50;

	// Token: 0x04000B05 RID: 2821
	public GameObject m_biomeFoundPrefab;

	// Token: 0x04000B06 RID: 2822
	public GameObject m_biomeFoundStinger;

	// Token: 0x04000B07 RID: 2823
	private Queue<MessageHud.BiomeMessage> m_biomeFoundQueue = new Queue<MessageHud.BiomeMessage>();

	// Token: 0x04000B08 RID: 2824
	private List<string> m_messageLog = new List<string>();

	// Token: 0x04000B09 RID: 2825
	private List<GameObject> m_unlockMessages = new List<GameObject>();

	// Token: 0x04000B0A RID: 2826
	private Queue<MessageHud.UnlockMsg> m_unlockMsgQueue = new Queue<MessageHud.UnlockMsg>();

	// Token: 0x04000B0B RID: 2827
	private Queue<MessageHud.MsgData> m_msgQeue = new Queue<MessageHud.MsgData>();

	// Token: 0x04000B0C RID: 2828
	private float m_msgQueueTimer = -1f;

	// Token: 0x04000B0D RID: 2829
	private GameObject m_biomeMsgInstance;

	// Token: 0x020000E1 RID: 225
	public enum MessageType
	{
		// Token: 0x04000B0F RID: 2831
		TopLeft = 1,
		// Token: 0x04000B10 RID: 2832
		Center
	}

	// Token: 0x020000E2 RID: 226
	private class UnlockMsg
	{
		// Token: 0x04000B11 RID: 2833
		public Sprite m_icon;

		// Token: 0x04000B12 RID: 2834
		public string m_topic;

		// Token: 0x04000B13 RID: 2835
		public string m_description;
	}

	// Token: 0x020000E3 RID: 227
	private class MsgData
	{
		// Token: 0x04000B14 RID: 2836
		public Sprite m_icon;

		// Token: 0x04000B15 RID: 2837
		public string m_text;

		// Token: 0x04000B16 RID: 2838
		public int m_amount;
	}

	// Token: 0x020000E4 RID: 228
	private class BiomeMessage
	{
		// Token: 0x04000B17 RID: 2839
		public string m_text;

		// Token: 0x04000B18 RID: 2840
		public bool m_playStinger;
	}
}
