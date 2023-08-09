using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000054 RID: 84
public class NpcTalk : MonoBehaviour
{
	// Token: 0x060004A3 RID: 1187 RVA: 0x00026410 File Offset: 0x00024610
	private void Start()
	{
		this.m_character = base.GetComponentInChildren<Character>();
		this.m_monsterAI = base.GetComponent<MonsterAI>();
		this.m_animator = base.GetComponentInChildren<Animator>();
		this.m_nview = base.GetComponent<ZNetView>();
		MonsterAI monsterAI = this.m_monsterAI;
		monsterAI.m_onBecameAggravated = (Action<BaseAI.AggravatedReason>)Delegate.Combine(monsterAI.m_onBecameAggravated, new Action<BaseAI.AggravatedReason>(this.OnBecameAggravated));
		base.InvokeRepeating("RandomTalk", UnityEngine.Random.Range(this.m_randomTalkInterval / 5f, this.m_randomTalkInterval), this.m_randomTalkInterval);
	}

	// Token: 0x060004A4 RID: 1188 RVA: 0x0002649C File Offset: 0x0002469C
	private void Update()
	{
		if (this.m_monsterAI.GetTargetCreature() != null || this.m_monsterAI.GetStaticTarget() != null)
		{
			return;
		}
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.UpdateTarget();
		if (this.m_targetPlayer)
		{
			if (this.m_nview.IsOwner() && this.m_character.GetVelocity().magnitude < 0.5f)
			{
				Vector3 normalized = (this.m_targetPlayer.GetEyePoint() - this.m_character.GetEyePoint()).normalized;
				this.m_character.SetLookDir(normalized, 0f);
			}
			if (this.m_seeTarget)
			{
				float num = Vector3.Distance(this.m_targetPlayer.transform.position, base.transform.position);
				if (!this.m_didGreet && num < this.m_greetRange)
				{
					this.m_didGreet = true;
					this.QueueSay(this.m_randomGreets, "Greet", this.m_randomGreetFX);
				}
				if (this.m_didGreet && !this.m_didGoodbye && num > this.m_byeRange)
				{
					this.m_didGoodbye = true;
					this.QueueSay(this.m_randomGoodbye, "Greet", this.m_randomGoodbyeFX);
				}
			}
		}
		this.UpdateSayQueue();
	}

	// Token: 0x060004A5 RID: 1189 RVA: 0x000265E8 File Offset: 0x000247E8
	private void UpdateTarget()
	{
		if (Time.time - this.m_lastTargetUpdate > 1f)
		{
			this.m_lastTargetUpdate = Time.time;
			this.m_targetPlayer = null;
			Player closestPlayer = Player.GetClosestPlayer(base.transform.position, this.m_maxRange);
			if (closestPlayer == null)
			{
				return;
			}
			if (this.m_monsterAI.IsEnemy(closestPlayer))
			{
				return;
			}
			this.m_seeTarget = this.m_monsterAI.CanSeeTarget(closestPlayer);
			this.m_hearTarget = this.m_monsterAI.CanHearTarget(closestPlayer);
			if (!this.m_seeTarget && !this.m_hearTarget)
			{
				return;
			}
			this.m_targetPlayer = closestPlayer;
		}
	}

	// Token: 0x060004A6 RID: 1190 RVA: 0x00026686 File Offset: 0x00024886
	private void OnBecameAggravated(BaseAI.AggravatedReason reason)
	{
		this.QueueSay(this.m_aggravated, "Aggravated", null);
	}

	// Token: 0x060004A7 RID: 1191 RVA: 0x0002669C File Offset: 0x0002489C
	public void OnPrivateAreaAttacked(Character attacker)
	{
		if (attacker.IsPlayer() && this.m_monsterAI.IsAggravatable() && !this.m_monsterAI.IsAggravated() && Vector3.Distance(base.transform.position, attacker.transform.position) < this.m_maxRange)
		{
			this.QueueSay(this.m_privateAreaAlarm, "Angry", null);
		}
	}

	// Token: 0x060004A8 RID: 1192 RVA: 0x00026700 File Offset: 0x00024900
	private void RandomTalk()
	{
		if (Time.time - NpcTalk.m_lastTalkTime < this.m_minTalkInterval)
		{
			return;
		}
		if (UnityEngine.Random.Range(0f, 1f) > this.m_randomTalkChance)
		{
			return;
		}
		this.UpdateTarget();
		if (this.m_targetPlayer && this.m_seeTarget)
		{
			List<string> texts = this.InFactionBase() ? this.m_randomTalkInFactionBase : this.m_randomTalk;
			this.QueueSay(texts, "Talk", this.m_randomTalkFX);
		}
	}

	// Token: 0x060004A9 RID: 1193 RVA: 0x00026780 File Offset: 0x00024980
	private void QueueSay(List<string> texts, string trigger, EffectList effect)
	{
		if (texts.Count == 0)
		{
			return;
		}
		if (this.m_queuedTexts.Count >= 3)
		{
			return;
		}
		NpcTalk.QueuedSay queuedSay = new NpcTalk.QueuedSay();
		queuedSay.text = texts[UnityEngine.Random.Range(0, texts.Count)];
		queuedSay.trigger = trigger;
		queuedSay.m_effect = effect;
		this.m_queuedTexts.Enqueue(queuedSay);
	}

	// Token: 0x060004AA RID: 1194 RVA: 0x000267E0 File Offset: 0x000249E0
	private void UpdateSayQueue()
	{
		if (this.m_queuedTexts.Count == 0)
		{
			return;
		}
		if (Time.time - NpcTalk.m_lastTalkTime < this.m_minTalkInterval)
		{
			return;
		}
		NpcTalk.QueuedSay queuedSay = this.m_queuedTexts.Dequeue();
		this.Say(queuedSay.text, queuedSay.trigger);
		if (queuedSay.m_effect != null)
		{
			queuedSay.m_effect.Create(base.transform.position, Quaternion.identity, null, 1f, -1);
		}
	}

	// Token: 0x060004AB RID: 1195 RVA: 0x00026858 File Offset: 0x00024A58
	private void Say(string text, string trigger)
	{
		NpcTalk.m_lastTalkTime = Time.time;
		Chat.instance.SetNpcText(base.gameObject, Vector3.up * this.m_offset, 20f, this.m_hideDialogDelay, "", text, false);
		if (trigger.Length > 0)
		{
			this.m_animator.SetTrigger(trigger);
		}
	}

	// Token: 0x060004AC RID: 1196 RVA: 0x000268B6 File Offset: 0x00024AB6
	private bool InFactionBase()
	{
		return PrivateArea.InsideFactionArea(base.transform.position, this.m_character.GetFaction());
	}

	// Token: 0x0400056A RID: 1386
	private float m_lastTargetUpdate;

	// Token: 0x0400056B RID: 1387
	public string m_name = "Haldor";

	// Token: 0x0400056C RID: 1388
	public float m_maxRange = 15f;

	// Token: 0x0400056D RID: 1389
	public float m_greetRange = 10f;

	// Token: 0x0400056E RID: 1390
	public float m_byeRange = 15f;

	// Token: 0x0400056F RID: 1391
	public float m_offset = 2f;

	// Token: 0x04000570 RID: 1392
	public float m_minTalkInterval = 1.5f;

	// Token: 0x04000571 RID: 1393
	private const int m_maxQueuedTexts = 3;

	// Token: 0x04000572 RID: 1394
	public float m_hideDialogDelay = 5f;

	// Token: 0x04000573 RID: 1395
	public float m_randomTalkInterval = 10f;

	// Token: 0x04000574 RID: 1396
	public float m_randomTalkChance = 1f;

	// Token: 0x04000575 RID: 1397
	public List<string> m_randomTalk = new List<string>();

	// Token: 0x04000576 RID: 1398
	public List<string> m_randomTalkInFactionBase = new List<string>();

	// Token: 0x04000577 RID: 1399
	public List<string> m_randomGreets = new List<string>();

	// Token: 0x04000578 RID: 1400
	public List<string> m_randomGoodbye = new List<string>();

	// Token: 0x04000579 RID: 1401
	public List<string> m_privateAreaAlarm = new List<string>();

	// Token: 0x0400057A RID: 1402
	public List<string> m_aggravated = new List<string>();

	// Token: 0x0400057B RID: 1403
	public EffectList m_randomTalkFX = new EffectList();

	// Token: 0x0400057C RID: 1404
	public EffectList m_randomGreetFX = new EffectList();

	// Token: 0x0400057D RID: 1405
	public EffectList m_randomGoodbyeFX = new EffectList();

	// Token: 0x0400057E RID: 1406
	private bool m_didGreet;

	// Token: 0x0400057F RID: 1407
	private bool m_didGoodbye;

	// Token: 0x04000580 RID: 1408
	private MonsterAI m_monsterAI;

	// Token: 0x04000581 RID: 1409
	private Animator m_animator;

	// Token: 0x04000582 RID: 1410
	private Character m_character;

	// Token: 0x04000583 RID: 1411
	private ZNetView m_nview;

	// Token: 0x04000584 RID: 1412
	private Player m_targetPlayer;

	// Token: 0x04000585 RID: 1413
	private bool m_seeTarget;

	// Token: 0x04000586 RID: 1414
	private bool m_hearTarget;

	// Token: 0x04000587 RID: 1415
	private Queue<NpcTalk.QueuedSay> m_queuedTexts = new Queue<NpcTalk.QueuedSay>();

	// Token: 0x04000588 RID: 1416
	private static float m_lastTalkTime;

	// Token: 0x02000055 RID: 85
	private class QueuedSay
	{
		// Token: 0x04000589 RID: 1417
		public string text;

		// Token: 0x0400058A RID: 1418
		public string trigger;

		// Token: 0x0400058B RID: 1419
		public EffectList m_effect;
	}
}
