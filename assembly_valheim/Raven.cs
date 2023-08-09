using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000284 RID: 644
public class Raven : MonoBehaviour, Hoverable, Interactable, IDestructible
{
	// Token: 0x0600188B RID: 6283 RVA: 0x000A3BCE File Offset: 0x000A1DCE
	public static bool IsInstantiated()
	{
		return Raven.m_instance != null;
	}

	// Token: 0x0600188C RID: 6284 RVA: 0x000A3BDC File Offset: 0x000A1DDC
	private void Awake()
	{
		base.transform.position = new Vector3(0f, 100000f, 0f);
		Raven.m_instance = this;
		this.m_animator = this.m_visual.GetComponentInChildren<Animator>();
		this.m_collider = base.GetComponent<Collider>();
		base.InvokeRepeating("IdleEffect", UnityEngine.Random.Range(this.m_idleEffectIntervalMin, this.m_idleEffectIntervalMax), UnityEngine.Random.Range(this.m_idleEffectIntervalMin, this.m_idleEffectIntervalMax));
		base.InvokeRepeating("CheckSpawn", 1f, 1f);
	}

	// Token: 0x0600188D RID: 6285 RVA: 0x000A3C6D File Offset: 0x000A1E6D
	private void OnDestroy()
	{
		if (Raven.m_instance == this)
		{
			Raven.m_instance = null;
		}
	}

	// Token: 0x0600188E RID: 6286 RVA: 0x000A3C82 File Offset: 0x000A1E82
	public string GetHoverText()
	{
		if (this.IsSpawned())
		{
			return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $raven_interact");
		}
		return "";
	}

	// Token: 0x0600188F RID: 6287 RVA: 0x000A3CAC File Offset: 0x000A1EAC
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x06001890 RID: 6288 RVA: 0x000A3CBE File Offset: 0x000A1EBE
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		if (this.m_hasTalked && Chat.instance.IsDialogVisible(base.gameObject))
		{
			Chat.instance.ClearNpcText(base.gameObject);
		}
		else
		{
			this.Talk();
		}
		return false;
	}

	// Token: 0x06001891 RID: 6289 RVA: 0x000A3CF8 File Offset: 0x000A1EF8
	private void Talk()
	{
		if (!Player.m_localPlayer)
		{
			return;
		}
		if (this.m_currentText == null)
		{
			return;
		}
		if (this.m_currentText.m_key.Length > 0)
		{
			Player.m_localPlayer.SetSeenTutorial(this.m_currentText.m_key);
			Gogan.LogEvent("Game", "Raven", this.m_currentText.m_key, 0L);
		}
		else
		{
			Gogan.LogEvent("Game", "Raven", this.m_currentText.m_topic, 0L);
		}
		this.m_hasTalked = true;
		if (this.m_currentText.m_label.Length > 0)
		{
			Player.m_localPlayer.AddKnownText(this.m_currentText.m_label, this.m_currentText.m_text);
		}
		this.Say(this.m_currentText.m_topic, this.m_currentText.m_text, false, true, true);
	}

	// Token: 0x06001892 RID: 6290 RVA: 0x000A3DD8 File Offset: 0x000A1FD8
	private void Say(string topic, string text, bool showName, bool longTimeout, bool large)
	{
		if (topic.Length > 0)
		{
			text = "<color=orange>" + topic + "</color>\n" + text;
		}
		Chat.instance.SetNpcText(base.gameObject, Vector3.up * this.m_textOffset, this.m_textCullDistance, longTimeout ? this.m_longDialogVisibleTime : this.m_dialogVisibleTime, showName ? this.m_name : "", text, large);
		this.m_animator.SetTrigger("talk");
	}

	// Token: 0x06001893 RID: 6291 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06001894 RID: 6292 RVA: 0x000A3E5C File Offset: 0x000A205C
	private void IdleEffect()
	{
		if (!this.IsSpawned())
		{
			return;
		}
		this.m_idleEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		base.CancelInvoke("IdleEffect");
		base.InvokeRepeating("IdleEffect", UnityEngine.Random.Range(this.m_idleEffectIntervalMin, this.m_idleEffectIntervalMax), UnityEngine.Random.Range(this.m_idleEffectIntervalMin, this.m_idleEffectIntervalMax));
	}

	// Token: 0x06001895 RID: 6293 RVA: 0x000A3ED3 File Offset: 0x000A20D3
	private bool CanHide()
	{
		return Player.m_localPlayer == null || !Chat.instance.IsDialogVisible(base.gameObject);
	}

	// Token: 0x06001896 RID: 6294 RVA: 0x000A3EFC File Offset: 0x000A20FC
	private void Update()
	{
		this.m_timeSinceTeleport += Time.deltaTime;
		if (!this.IsAway() && !this.IsFlying() && Player.m_localPlayer)
		{
			Vector3 vector = Player.m_localPlayer.transform.position - base.transform.position;
			vector.y = 0f;
			vector.Normalize();
			float f = Vector3.SignedAngle(base.transform.forward, vector, Vector3.up);
			if (Mathf.Abs(f) > this.m_minRotationAngle)
			{
				this.m_animator.SetFloat("anglevel", this.m_rotateSpeed * Mathf.Sign(f), 0.4f, Time.deltaTime);
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(vector), Time.deltaTime * this.m_rotateSpeed);
			}
			else
			{
				this.m_animator.SetFloat("anglevel", 0f, 0.4f, Time.deltaTime);
			}
		}
		if (this.IsSpawned())
		{
			if (Player.m_localPlayer != null && !Chat.instance.IsDialogVisible(base.gameObject) && Vector3.Distance(Player.m_localPlayer.transform.position, base.transform.position) < this.m_autoTalkDistance)
			{
				this.m_randomTextTimer += Time.deltaTime;
				float num = this.m_hasTalked ? this.m_randomTextInterval : this.m_randomTextIntervalImportant;
				if (this.m_randomTextTimer >= num)
				{
					this.m_randomTextTimer = 0f;
					if (this.m_hasTalked)
					{
						this.Say("", this.m_randomTexts[UnityEngine.Random.Range(0, this.m_randomTexts.Count)], false, false, false);
					}
					else
					{
						this.Say("", this.m_randomTextsImportant[UnityEngine.Random.Range(0, this.m_randomTextsImportant.Count)], false, false, false);
					}
				}
			}
			if ((Player.m_localPlayer == null || Vector3.Distance(Player.m_localPlayer.transform.position, base.transform.position) > this.m_despawnDistance || this.EnemyNearby(base.transform.position) || RandEventSystem.InEvent() || this.m_currentText == null || this.m_groundObject == null || this.m_hasTalked) && this.CanHide())
			{
				bool forceTeleport = this.GetBestText() != null || this.m_groundObject == null;
				this.FlyAway(forceTeleport);
				this.RestartSpawnCheck(3f);
			}
			this.m_exclamation.SetActive(!this.m_hasTalked);
			return;
		}
		this.m_exclamation.SetActive(false);
	}

	// Token: 0x06001897 RID: 6295 RVA: 0x000A41C4 File Offset: 0x000A23C4
	private bool FindSpawnPoint(out Vector3 point, out GameObject landOn)
	{
		Vector3 position = Player.m_localPlayer.transform.position;
		Vector3 forward = Utils.GetMainCamera().transform.forward;
		forward.y = 0f;
		forward.Normalize();
		point = new Vector3(0f, -999f, 0f);
		landOn = null;
		bool result = false;
		for (int i = 0; i < 20; i++)
		{
			Vector3 a = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(-30, 30), 0f) * forward;
			Vector3 vector = position + a * UnityEngine.Random.Range(this.m_spawnDistance - 5f, this.m_spawnDistance);
			float num;
			Vector3 vector2;
			GameObject gameObject;
			if (ZoneSystem.instance.GetSolidHeight(vector, out num, out vector2, out gameObject) && num > ZoneSystem.instance.m_waterLevel && num > point.y && num < 2000f && vector2.y > 0.5f && Mathf.Abs(num - position.y) < 2f)
			{
				vector.y = num;
				point = vector;
				landOn = gameObject;
				result = true;
			}
		}
		return result;
	}

	// Token: 0x06001898 RID: 6296 RVA: 0x000A42ED File Offset: 0x000A24ED
	private bool EnemyNearby(Vector3 point)
	{
		return LootSpawner.IsMonsterInRange(point, this.m_enemyCheckDistance);
	}

	// Token: 0x06001899 RID: 6297 RVA: 0x000A42FC File Offset: 0x000A24FC
	private bool InState(string name)
	{
		return this.m_animator.isInitialized && (this.m_animator.GetCurrentAnimatorStateInfo(0).IsTag(name) || this.m_animator.GetNextAnimatorStateInfo(0).IsTag(name));
	}

	// Token: 0x0600189A RID: 6298 RVA: 0x000A434C File Offset: 0x000A254C
	private Raven.RavenText GetBestText()
	{
		Raven.RavenText ravenText = this.GetTempText();
		Raven.RavenText closestStaticText = this.GetClosestStaticText(this.m_spawnDistance);
		if (closestStaticText != null && (ravenText == null || closestStaticText.m_priority >= ravenText.m_priority))
		{
			ravenText = closestStaticText;
		}
		return ravenText;
	}

	// Token: 0x0600189B RID: 6299 RVA: 0x000A4384 File Offset: 0x000A2584
	private Raven.RavenText GetTempText()
	{
		foreach (Raven.RavenText ravenText in Raven.m_tempTexts)
		{
			if (ravenText.m_munin == this.m_isMunin)
			{
				return ravenText;
			}
		}
		return null;
	}

	// Token: 0x0600189C RID: 6300 RVA: 0x000A43E4 File Offset: 0x000A25E4
	private Raven.RavenText GetClosestStaticText(float maxDistance)
	{
		if (Player.m_localPlayer == null)
		{
			return null;
		}
		Raven.RavenText ravenText = null;
		float num = 9999f;
		bool flag = false;
		Vector3 position = Player.m_localPlayer.transform.position;
		foreach (Raven.RavenText ravenText2 in Raven.m_staticTexts)
		{
			if (ravenText2.m_munin == this.m_isMunin && ravenText2.m_guidePoint)
			{
				float num2 = Vector3.Distance(position, ravenText2.m_guidePoint.transform.position);
				if (num2 < maxDistance)
				{
					bool flag2 = ravenText2.m_key.Length > 0 && Player.m_localPlayer.HaveSeenTutorial(ravenText2.m_key);
					if (ravenText2.m_alwaysSpawn || !flag2)
					{
						if (ravenText == null)
						{
							ravenText = ravenText2;
							num = num2;
							flag = flag2;
						}
						else if (flag2 == flag)
						{
							if (ravenText2.m_priority == ravenText.m_priority || flag2)
							{
								if (num2 < num)
								{
									ravenText = ravenText2;
									num = num2;
									flag = flag2;
								}
							}
							else if (ravenText2.m_priority > ravenText.m_priority)
							{
								ravenText = ravenText2;
								num = num2;
								flag = flag2;
							}
						}
						else if (!flag2 && flag)
						{
							ravenText = ravenText2;
							num = num2;
							flag = flag2;
						}
					}
				}
			}
		}
		return ravenText;
	}

	// Token: 0x0600189D RID: 6301 RVA: 0x000A453C File Offset: 0x000A273C
	private void RemoveSeendTempTexts()
	{
		for (int i = 0; i < Raven.m_tempTexts.Count; i++)
		{
			if (Player.m_localPlayer.HaveSeenTutorial(Raven.m_tempTexts[i].m_key))
			{
				Raven.m_tempTexts.RemoveAt(i);
				return;
			}
		}
	}

	// Token: 0x0600189E RID: 6302 RVA: 0x000A4588 File Offset: 0x000A2788
	private void FlyAway(bool forceTeleport = false)
	{
		Chat.instance.ClearNpcText(base.gameObject);
		if (forceTeleport || this.IsUnderRoof())
		{
			this.m_animator.SetTrigger("poff");
			this.m_timeSinceTeleport = 0f;
			return;
		}
		this.m_animator.SetTrigger("flyaway");
	}

	// Token: 0x0600189F RID: 6303 RVA: 0x000A45DC File Offset: 0x000A27DC
	private void CheckSpawn()
	{
		if (Player.m_localPlayer == null)
		{
			return;
		}
		this.RemoveSeendTempTexts();
		Raven.RavenText bestText = this.GetBestText();
		if (this.IsSpawned() && this.CanHide() && bestText != null && bestText != this.m_currentText)
		{
			this.FlyAway(true);
			this.m_currentText = null;
		}
		if (this.IsAway() && bestText != null)
		{
			if (this.EnemyNearby(base.transform.position))
			{
				return;
			}
			if (RandEventSystem.InEvent())
			{
				return;
			}
			bool forceTeleport = this.m_timeSinceTeleport < 6f;
			this.Spawn(bestText, forceTeleport);
		}
	}

	// Token: 0x060018A0 RID: 6304 RVA: 0x00004264 File Offset: 0x00002464
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Character;
	}

	// Token: 0x060018A1 RID: 6305 RVA: 0x000A466B File Offset: 0x000A286B
	public void Damage(HitData hit)
	{
		if (!this.IsSpawned())
		{
			return;
		}
		this.FlyAway(true);
		this.RestartSpawnCheck(4f);
	}

	// Token: 0x060018A2 RID: 6306 RVA: 0x000A4688 File Offset: 0x000A2888
	private void RestartSpawnCheck(float delay)
	{
		base.CancelInvoke("CheckSpawn");
		base.InvokeRepeating("CheckSpawn", delay, 1f);
	}

	// Token: 0x060018A3 RID: 6307 RVA: 0x000A46A6 File Offset: 0x000A28A6
	private bool IsSpawned()
	{
		return this.InState("visible");
	}

	// Token: 0x060018A4 RID: 6308 RVA: 0x000A46B3 File Offset: 0x000A28B3
	public bool IsAway()
	{
		return this.InState("away");
	}

	// Token: 0x060018A5 RID: 6309 RVA: 0x000A46C0 File Offset: 0x000A28C0
	public bool IsFlying()
	{
		return this.InState("flying");
	}

	// Token: 0x060018A6 RID: 6310 RVA: 0x000A46D0 File Offset: 0x000A28D0
	private void Spawn(Raven.RavenText text, bool forceTeleport)
	{
		if (Utils.GetMainCamera() == null || !Raven.m_tutorialsEnabled)
		{
			return;
		}
		if (text.m_static)
		{
			this.m_groundObject = text.m_guidePoint.gameObject;
			base.transform.position = text.m_guidePoint.transform.position;
		}
		else
		{
			Vector3 position;
			GameObject groundObject;
			if (!this.FindSpawnPoint(out position, out groundObject))
			{
				return;
			}
			base.transform.position = position;
			this.m_groundObject = groundObject;
		}
		this.m_currentText = text;
		this.m_hasTalked = false;
		this.m_randomTextTimer = 99999f;
		if (this.m_currentText.m_key.Length > 0 && Player.m_localPlayer.HaveSeenTutorial(this.m_currentText.m_key))
		{
			this.m_hasTalked = true;
		}
		Vector3 forward = Player.m_localPlayer.transform.position - base.transform.position;
		forward.y = 0f;
		forward.Normalize();
		base.transform.rotation = Quaternion.LookRotation(forward);
		if (forceTeleport)
		{
			this.m_animator.SetTrigger("teleportin");
			return;
		}
		if (!text.m_static)
		{
			this.m_animator.SetTrigger("flyin");
			return;
		}
		if (this.IsUnderRoof())
		{
			this.m_animator.SetTrigger("teleportin");
			return;
		}
		this.m_animator.SetTrigger("flyin");
	}

	// Token: 0x060018A7 RID: 6311 RVA: 0x000A4830 File Offset: 0x000A2A30
	private bool IsUnderRoof()
	{
		return Physics.Raycast(base.transform.position + Vector3.up * 0.2f, Vector3.up, 20f, LayerMask.GetMask(new string[]
		{
			"Default",
			"static_solid",
			"piece"
		}));
	}

	// Token: 0x060018A8 RID: 6312 RVA: 0x000A488E File Offset: 0x000A2A8E
	public static void RegisterStaticText(Raven.RavenText text)
	{
		Raven.m_staticTexts.Add(text);
	}

	// Token: 0x060018A9 RID: 6313 RVA: 0x000A489B File Offset: 0x000A2A9B
	public static void UnregisterStaticText(Raven.RavenText text)
	{
		Raven.m_staticTexts.Remove(text);
	}

	// Token: 0x060018AA RID: 6314 RVA: 0x000A48AC File Offset: 0x000A2AAC
	public static void AddTempText(string key, string topic, string text, string label, bool munin)
	{
		if (key.Length > 0)
		{
			using (List<Raven.RavenText>.Enumerator enumerator = Raven.m_tempTexts.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.m_key == key)
					{
						return;
					}
				}
			}
		}
		Raven.RavenText ravenText = new Raven.RavenText();
		ravenText.m_key = key;
		ravenText.m_topic = topic;
		ravenText.m_label = label;
		ravenText.m_text = text;
		ravenText.m_static = false;
		ravenText.m_munin = munin;
		Raven.m_tempTexts.Add(ravenText);
	}

	// Token: 0x04001A6D RID: 6765
	public GameObject m_visual;

	// Token: 0x04001A6E RID: 6766
	public GameObject m_exclamation;

	// Token: 0x04001A6F RID: 6767
	public string m_name = "Name";

	// Token: 0x04001A70 RID: 6768
	public bool m_isMunin;

	// Token: 0x04001A71 RID: 6769
	public bool m_autoTalk = true;

	// Token: 0x04001A72 RID: 6770
	public float m_idleEffectIntervalMin = 10f;

	// Token: 0x04001A73 RID: 6771
	public float m_idleEffectIntervalMax = 20f;

	// Token: 0x04001A74 RID: 6772
	public float m_spawnDistance = 15f;

	// Token: 0x04001A75 RID: 6773
	public float m_despawnDistance = 20f;

	// Token: 0x04001A76 RID: 6774
	public float m_autoTalkDistance = 3f;

	// Token: 0x04001A77 RID: 6775
	public float m_enemyCheckDistance = 10f;

	// Token: 0x04001A78 RID: 6776
	public float m_rotateSpeed = 10f;

	// Token: 0x04001A79 RID: 6777
	public float m_minRotationAngle = 15f;

	// Token: 0x04001A7A RID: 6778
	public float m_dialogVisibleTime = 10f;

	// Token: 0x04001A7B RID: 6779
	public float m_longDialogVisibleTime = 10f;

	// Token: 0x04001A7C RID: 6780
	public float m_dontFlyDistance = 3f;

	// Token: 0x04001A7D RID: 6781
	public float m_textOffset = 1.5f;

	// Token: 0x04001A7E RID: 6782
	public float m_textCullDistance = 20f;

	// Token: 0x04001A7F RID: 6783
	public float m_randomTextInterval = 30f;

	// Token: 0x04001A80 RID: 6784
	public float m_randomTextIntervalImportant = 10f;

	// Token: 0x04001A81 RID: 6785
	public List<string> m_randomTextsImportant = new List<string>();

	// Token: 0x04001A82 RID: 6786
	public List<string> m_randomTexts = new List<string>();

	// Token: 0x04001A83 RID: 6787
	public EffectList m_idleEffect = new EffectList();

	// Token: 0x04001A84 RID: 6788
	public EffectList m_despawnEffect = new EffectList();

	// Token: 0x04001A85 RID: 6789
	private Raven.RavenText m_currentText;

	// Token: 0x04001A86 RID: 6790
	private GameObject m_groundObject;

	// Token: 0x04001A87 RID: 6791
	private Animator m_animator;

	// Token: 0x04001A88 RID: 6792
	private Collider m_collider;

	// Token: 0x04001A89 RID: 6793
	private bool m_hasTalked;

	// Token: 0x04001A8A RID: 6794
	private float m_randomTextTimer = 9999f;

	// Token: 0x04001A8B RID: 6795
	private float m_timeSinceTeleport = 9999f;

	// Token: 0x04001A8C RID: 6796
	private static List<Raven.RavenText> m_tempTexts = new List<Raven.RavenText>();

	// Token: 0x04001A8D RID: 6797
	private static List<Raven.RavenText> m_staticTexts = new List<Raven.RavenText>();

	// Token: 0x04001A8E RID: 6798
	private static Raven m_instance = null;

	// Token: 0x04001A8F RID: 6799
	public static bool m_tutorialsEnabled = true;

	// Token: 0x02000285 RID: 645
	[Serializable]
	public class RavenText
	{
		// Token: 0x04001A90 RID: 6800
		public bool m_alwaysSpawn = true;

		// Token: 0x04001A91 RID: 6801
		public bool m_munin;

		// Token: 0x04001A92 RID: 6802
		public int m_priority;

		// Token: 0x04001A93 RID: 6803
		public string m_key = "";

		// Token: 0x04001A94 RID: 6804
		public string m_topic = "";

		// Token: 0x04001A95 RID: 6805
		public string m_label = "";

		// Token: 0x04001A96 RID: 6806
		[TextArea]
		public string m_text = "";

		// Token: 0x04001A97 RID: 6807
		[NonSerialized]
		public bool m_static;

		// Token: 0x04001A98 RID: 6808
		[NonSerialized]
		public GuidePoint m_guidePoint;
	}
}
