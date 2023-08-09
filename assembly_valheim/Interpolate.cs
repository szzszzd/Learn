using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200007C RID: 124
public class Interpolate
{
	// Token: 0x060005A3 RID: 1443 RVA: 0x0002C27B File Offset: 0x0002A47B
	private static Vector3 Identity(Vector3 v)
	{
		return v;
	}

	// Token: 0x060005A4 RID: 1444 RVA: 0x0002C27E File Offset: 0x0002A47E
	private static Vector3 TransformDotPosition(Transform t)
	{
		return t.position;
	}

	// Token: 0x060005A5 RID: 1445 RVA: 0x0002C286 File Offset: 0x0002A486
	private static IEnumerable<float> NewTimer(float duration)
	{
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			yield return elapsedTime;
			elapsedTime += Time.deltaTime;
			if (elapsedTime >= duration)
			{
				yield return elapsedTime;
			}
		}
		yield break;
	}

	// Token: 0x060005A6 RID: 1446 RVA: 0x0002C296 File Offset: 0x0002A496
	private static IEnumerable<float> NewCounter(int start, int end, int step)
	{
		for (int i = start; i <= end; i += step)
		{
			yield return (float)i;
		}
		yield break;
	}

	// Token: 0x060005A7 RID: 1447 RVA: 0x0002C2B4 File Offset: 0x0002A4B4
	public static IEnumerator NewEase(Interpolate.Function ease, Vector3 start, Vector3 end, float duration)
	{
		IEnumerable<float> driver = Interpolate.NewTimer(duration);
		return Interpolate.NewEase(ease, start, end, duration, driver);
	}

	// Token: 0x060005A8 RID: 1448 RVA: 0x0002C2D4 File Offset: 0x0002A4D4
	public static IEnumerator NewEase(Interpolate.Function ease, Vector3 start, Vector3 end, int slices)
	{
		IEnumerable<float> driver = Interpolate.NewCounter(0, slices + 1, 1);
		return Interpolate.NewEase(ease, start, end, (float)(slices + 1), driver);
	}

	// Token: 0x060005A9 RID: 1449 RVA: 0x0002C2F9 File Offset: 0x0002A4F9
	private static IEnumerator NewEase(Interpolate.Function ease, Vector3 start, Vector3 end, float total, IEnumerable<float> driver)
	{
		Vector3 distance = end - start;
		foreach (float elapsedTime in driver)
		{
			yield return Interpolate.Ease(ease, start, distance, elapsedTime, total);
		}
		IEnumerator<float> enumerator = null;
		yield break;
		yield break;
	}

	// Token: 0x060005AA RID: 1450 RVA: 0x0002C328 File Offset: 0x0002A528
	private static Vector3 Ease(Interpolate.Function ease, Vector3 start, Vector3 distance, float elapsedTime, float duration)
	{
		start.x = ease(start.x, distance.x, elapsedTime, duration);
		start.y = ease(start.y, distance.y, elapsedTime, duration);
		start.z = ease(start.z, distance.z, elapsedTime, duration);
		return start;
	}

	// Token: 0x060005AB RID: 1451 RVA: 0x0002C38C File Offset: 0x0002A58C
	public static Interpolate.Function Ease(Interpolate.EaseType type)
	{
		Interpolate.Function result = null;
		switch (type)
		{
		case Interpolate.EaseType.Linear:
			result = new Interpolate.Function(Interpolate.Linear);
			break;
		case Interpolate.EaseType.EaseInQuad:
			result = new Interpolate.Function(Interpolate.EaseInQuad);
			break;
		case Interpolate.EaseType.EaseOutQuad:
			result = new Interpolate.Function(Interpolate.EaseOutQuad);
			break;
		case Interpolate.EaseType.EaseInOutQuad:
			result = new Interpolate.Function(Interpolate.EaseInOutQuad);
			break;
		case Interpolate.EaseType.EaseInCubic:
			result = new Interpolate.Function(Interpolate.EaseInCubic);
			break;
		case Interpolate.EaseType.EaseOutCubic:
			result = new Interpolate.Function(Interpolate.EaseOutCubic);
			break;
		case Interpolate.EaseType.EaseInOutCubic:
			result = new Interpolate.Function(Interpolate.EaseInOutCubic);
			break;
		case Interpolate.EaseType.EaseInQuart:
			result = new Interpolate.Function(Interpolate.EaseInQuart);
			break;
		case Interpolate.EaseType.EaseOutQuart:
			result = new Interpolate.Function(Interpolate.EaseOutQuart);
			break;
		case Interpolate.EaseType.EaseInOutQuart:
			result = new Interpolate.Function(Interpolate.EaseInOutQuart);
			break;
		case Interpolate.EaseType.EaseInQuint:
			result = new Interpolate.Function(Interpolate.EaseInQuint);
			break;
		case Interpolate.EaseType.EaseOutQuint:
			result = new Interpolate.Function(Interpolate.EaseOutQuint);
			break;
		case Interpolate.EaseType.EaseInOutQuint:
			result = new Interpolate.Function(Interpolate.EaseInOutQuint);
			break;
		case Interpolate.EaseType.EaseInSine:
			result = new Interpolate.Function(Interpolate.EaseInSine);
			break;
		case Interpolate.EaseType.EaseOutSine:
			result = new Interpolate.Function(Interpolate.EaseOutSine);
			break;
		case Interpolate.EaseType.EaseInOutSine:
			result = new Interpolate.Function(Interpolate.EaseInOutSine);
			break;
		case Interpolate.EaseType.EaseInExpo:
			result = new Interpolate.Function(Interpolate.EaseInExpo);
			break;
		case Interpolate.EaseType.EaseOutExpo:
			result = new Interpolate.Function(Interpolate.EaseOutExpo);
			break;
		case Interpolate.EaseType.EaseInOutExpo:
			result = new Interpolate.Function(Interpolate.EaseInOutExpo);
			break;
		case Interpolate.EaseType.EaseInCirc:
			result = new Interpolate.Function(Interpolate.EaseInCirc);
			break;
		case Interpolate.EaseType.EaseOutCirc:
			result = new Interpolate.Function(Interpolate.EaseOutCirc);
			break;
		case Interpolate.EaseType.EaseInOutCirc:
			result = new Interpolate.Function(Interpolate.EaseInOutCirc);
			break;
		}
		return result;
	}

	// Token: 0x060005AC RID: 1452 RVA: 0x0002C570 File Offset: 0x0002A770
	public static IEnumerable<Vector3> NewBezier(Interpolate.Function ease, Transform[] nodes, float duration)
	{
		IEnumerable<float> steps = Interpolate.NewTimer(duration);
		return Interpolate.NewBezier<Transform>(ease, nodes, new Interpolate.ToVector3<Transform>(Interpolate.TransformDotPosition), duration, steps);
	}

	// Token: 0x060005AD RID: 1453 RVA: 0x0002C59C File Offset: 0x0002A79C
	public static IEnumerable<Vector3> NewBezier(Interpolate.Function ease, Transform[] nodes, int slices)
	{
		IEnumerable<float> steps = Interpolate.NewCounter(0, slices + 1, 1);
		return Interpolate.NewBezier<Transform>(ease, nodes, new Interpolate.ToVector3<Transform>(Interpolate.TransformDotPosition), (float)(slices + 1), steps);
	}

	// Token: 0x060005AE RID: 1454 RVA: 0x0002C5CC File Offset: 0x0002A7CC
	public static IEnumerable<Vector3> NewBezier(Interpolate.Function ease, Vector3[] points, float duration)
	{
		IEnumerable<float> steps = Interpolate.NewTimer(duration);
		return Interpolate.NewBezier<Vector3>(ease, points, new Interpolate.ToVector3<Vector3>(Interpolate.Identity), duration, steps);
	}

	// Token: 0x060005AF RID: 1455 RVA: 0x0002C5F8 File Offset: 0x0002A7F8
	public static IEnumerable<Vector3> NewBezier(Interpolate.Function ease, Vector3[] points, int slices)
	{
		IEnumerable<float> steps = Interpolate.NewCounter(0, slices + 1, 1);
		return Interpolate.NewBezier<Vector3>(ease, points, new Interpolate.ToVector3<Vector3>(Interpolate.Identity), (float)(slices + 1), steps);
	}

	// Token: 0x060005B0 RID: 1456 RVA: 0x0002C628 File Offset: 0x0002A828
	private static IEnumerable<Vector3> NewBezier<T>(Interpolate.Function ease, IList nodes, Interpolate.ToVector3<T> toVector3, float maxStep, IEnumerable<float> steps)
	{
		if (nodes.Count >= 2)
		{
			Vector3[] points = new Vector3[nodes.Count];
			foreach (float elapsedTime in steps)
			{
				for (int i = 0; i < nodes.Count; i++)
				{
					points[i] = toVector3((T)((object)nodes[i]));
				}
				yield return Interpolate.Bezier(ease, points, elapsedTime, maxStep);
			}
			IEnumerator<float> enumerator = null;
			points = null;
		}
		yield break;
		yield break;
	}

	// Token: 0x060005B1 RID: 1457 RVA: 0x0002C658 File Offset: 0x0002A858
	private static Vector3 Bezier(Interpolate.Function ease, Vector3[] points, float elapsedTime, float duration)
	{
		for (int i = points.Length - 1; i > 0; i--)
		{
			for (int j = 0; j < i; j++)
			{
				points[j].x = ease(points[j].x, points[j + 1].x - points[j].x, elapsedTime, duration);
				points[j].y = ease(points[j].y, points[j + 1].y - points[j].y, elapsedTime, duration);
				points[j].z = ease(points[j].z, points[j + 1].z - points[j].z, elapsedTime, duration);
			}
		}
		return points[0];
	}

	// Token: 0x060005B2 RID: 1458 RVA: 0x0002C745 File Offset: 0x0002A945
	public static IEnumerable<Vector3> NewCatmullRom(Transform[] nodes, int slices, bool loop)
	{
		return Interpolate.NewCatmullRom<Transform>(nodes, new Interpolate.ToVector3<Transform>(Interpolate.TransformDotPosition), slices, loop);
	}

	// Token: 0x060005B3 RID: 1459 RVA: 0x0002C75B File Offset: 0x0002A95B
	public static IEnumerable<Vector3> NewCatmullRom(Vector3[] points, int slices, bool loop)
	{
		return Interpolate.NewCatmullRom<Vector3>(points, new Interpolate.ToVector3<Vector3>(Interpolate.Identity), slices, loop);
	}

	// Token: 0x060005B4 RID: 1460 RVA: 0x0002C771 File Offset: 0x0002A971
	private static IEnumerable<Vector3> NewCatmullRom<T>(IList nodes, Interpolate.ToVector3<T> toVector3, int slices, bool loop)
	{
		if (nodes.Count >= 2)
		{
			yield return toVector3((T)((object)nodes[0]));
			int last = nodes.Count - 1;
			int current = 0;
			while (loop || current < last)
			{
				if (loop && current > last)
				{
					current = 0;
				}
				int previous = (current == 0) ? (loop ? last : current) : (current - 1);
				int start = current;
				int end = (current == last) ? (loop ? 0 : current) : (current + 1);
				int next = (end == last) ? (loop ? 0 : end) : (end + 1);
				int stepCount = slices + 1;
				int num;
				for (int step = 1; step <= stepCount; step = num + 1)
				{
					yield return Interpolate.CatmullRom(toVector3((T)((object)nodes[previous])), toVector3((T)((object)nodes[start])), toVector3((T)((object)nodes[end])), toVector3((T)((object)nodes[next])), (float)step, (float)stepCount);
					num = step;
				}
				num = current;
				current = num + 1;
			}
		}
		yield break;
	}

	// Token: 0x060005B5 RID: 1461 RVA: 0x0002C798 File Offset: 0x0002A998
	private static Vector3 CatmullRom(Vector3 previous, Vector3 start, Vector3 end, Vector3 next, float elapsedTime, float duration)
	{
		float num = elapsedTime / duration;
		float num2 = num * num;
		float num3 = num2 * num;
		return previous * (-0.5f * num3 + num2 - 0.5f * num) + start * (1.5f * num3 + -2.5f * num2 + 1f) + end * (-1.5f * num3 + 2f * num2 + 0.5f * num) + next * (0.5f * num3 - 0.5f * num2);
	}

	// Token: 0x060005B6 RID: 1462 RVA: 0x0002C826 File Offset: 0x0002AA26
	private static float Linear(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return distance * (elapsedTime / duration) + start;
	}

	// Token: 0x060005B7 RID: 1463 RVA: 0x0002C836 File Offset: 0x0002AA36
	private static float EaseInQuad(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return distance * elapsedTime * elapsedTime + start;
	}

	// Token: 0x060005B8 RID: 1464 RVA: 0x0002C84F File Offset: 0x0002AA4F
	private static float EaseOutQuad(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return -distance * elapsedTime * (elapsedTime - 2f) + start;
	}

	// Token: 0x060005B9 RID: 1465 RVA: 0x0002C870 File Offset: 0x0002AA70
	private static float EaseInOutQuad(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * elapsedTime * elapsedTime + start;
		}
		elapsedTime -= 1f;
		return -distance / 2f * (elapsedTime * (elapsedTime - 2f) - 1f) + start;
	}

	// Token: 0x060005BA RID: 1466 RVA: 0x0002C8CC File Offset: 0x0002AACC
	private static float EaseInCubic(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return distance * elapsedTime * elapsedTime * elapsedTime + start;
	}

	// Token: 0x060005BB RID: 1467 RVA: 0x0002C8E7 File Offset: 0x0002AAE7
	private static float EaseOutCubic(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		elapsedTime -= 1f;
		return distance * (elapsedTime * elapsedTime * elapsedTime + 1f) + start;
	}

	// Token: 0x060005BC RID: 1468 RVA: 0x0002C914 File Offset: 0x0002AB14
	private static float EaseInOutCubic(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * elapsedTime * elapsedTime * elapsedTime + start;
		}
		elapsedTime -= 2f;
		return distance / 2f * (elapsedTime * elapsedTime * elapsedTime + 2f) + start;
	}

	// Token: 0x060005BD RID: 1469 RVA: 0x0002C96D File Offset: 0x0002AB6D
	private static float EaseInQuart(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
	}

	// Token: 0x060005BE RID: 1470 RVA: 0x0002C98A File Offset: 0x0002AB8A
	private static float EaseOutQuart(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		elapsedTime -= 1f;
		return -distance * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 1f) + start;
	}

	// Token: 0x060005BF RID: 1471 RVA: 0x0002C9B8 File Offset: 0x0002ABB8
	private static float EaseInOutQuart(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
		}
		elapsedTime -= 2f;
		return -distance / 2f * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 2f) + start;
	}

	// Token: 0x060005C0 RID: 1472 RVA: 0x0002CA16 File Offset: 0x0002AC16
	private static float EaseInQuint(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
	}

	// Token: 0x060005C1 RID: 1473 RVA: 0x0002CA35 File Offset: 0x0002AC35
	private static float EaseOutQuint(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		elapsedTime -= 1f;
		return distance * (elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + 1f) + start;
	}

	// Token: 0x060005C2 RID: 1474 RVA: 0x0002CA64 File Offset: 0x0002AC64
	private static float EaseInOutQuint(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
		}
		elapsedTime -= 2f;
		return distance / 2f * (elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + 2f) + start;
	}

	// Token: 0x060005C3 RID: 1475 RVA: 0x0002CAC5 File Offset: 0x0002ACC5
	private static float EaseInSine(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return -distance * Mathf.Cos(elapsedTime / duration * 1.5707964f) + distance + start;
	}

	// Token: 0x060005C4 RID: 1476 RVA: 0x0002CAE3 File Offset: 0x0002ACE3
	private static float EaseOutSine(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return distance * Mathf.Sin(elapsedTime / duration * 1.5707964f) + start;
	}

	// Token: 0x060005C5 RID: 1477 RVA: 0x0002CAFE File Offset: 0x0002ACFE
	private static float EaseInOutSine(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return -distance / 2f * (Mathf.Cos(3.1415927f * elapsedTime / duration) - 1f) + start;
	}

	// Token: 0x060005C6 RID: 1478 RVA: 0x0002CB26 File Offset: 0x0002AD26
	private static float EaseInExpo(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return distance * Mathf.Pow(2f, 10f * (elapsedTime / duration - 1f)) + start;
	}

	// Token: 0x060005C7 RID: 1479 RVA: 0x0002CB4C File Offset: 0x0002AD4C
	private static float EaseOutExpo(float start, float distance, float elapsedTime, float duration)
	{
		if (elapsedTime > duration)
		{
			elapsedTime = duration;
		}
		return distance * (-Mathf.Pow(2f, -10f * elapsedTime / duration) + 1f) + start;
	}

	// Token: 0x060005C8 RID: 1480 RVA: 0x0002CB74 File Offset: 0x0002AD74
	private static float EaseInOutExpo(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return distance / 2f * Mathf.Pow(2f, 10f * (elapsedTime - 1f)) + start;
		}
		elapsedTime -= 1f;
		return distance / 2f * (-Mathf.Pow(2f, -10f * elapsedTime) + 2f) + start;
	}

	// Token: 0x060005C9 RID: 1481 RVA: 0x0002CBEC File Offset: 0x0002ADEC
	private static float EaseInCirc(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		return -distance * (Mathf.Sqrt(1f - elapsedTime * elapsedTime) - 1f) + start;
	}

	// Token: 0x060005CA RID: 1482 RVA: 0x0002CC17 File Offset: 0x0002AE17
	private static float EaseOutCirc(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 1f : (elapsedTime / duration));
		elapsedTime -= 1f;
		return distance * Mathf.Sqrt(1f - elapsedTime * elapsedTime) + start;
	}

	// Token: 0x060005CB RID: 1483 RVA: 0x0002CC44 File Offset: 0x0002AE44
	private static float EaseInOutCirc(float start, float distance, float elapsedTime, float duration)
	{
		elapsedTime = ((elapsedTime > duration) ? 2f : (elapsedTime / (duration / 2f)));
		if (elapsedTime < 1f)
		{
			return -distance / 2f * (Mathf.Sqrt(1f - elapsedTime * elapsedTime) - 1f) + start;
		}
		elapsedTime -= 2f;
		return distance / 2f * (Mathf.Sqrt(1f - elapsedTime * elapsedTime) + 1f) + start;
	}

	// Token: 0x0200007D RID: 125
	public enum EaseType
	{
		// Token: 0x040006D2 RID: 1746
		Linear,
		// Token: 0x040006D3 RID: 1747
		EaseInQuad,
		// Token: 0x040006D4 RID: 1748
		EaseOutQuad,
		// Token: 0x040006D5 RID: 1749
		EaseInOutQuad,
		// Token: 0x040006D6 RID: 1750
		EaseInCubic,
		// Token: 0x040006D7 RID: 1751
		EaseOutCubic,
		// Token: 0x040006D8 RID: 1752
		EaseInOutCubic,
		// Token: 0x040006D9 RID: 1753
		EaseInQuart,
		// Token: 0x040006DA RID: 1754
		EaseOutQuart,
		// Token: 0x040006DB RID: 1755
		EaseInOutQuart,
		// Token: 0x040006DC RID: 1756
		EaseInQuint,
		// Token: 0x040006DD RID: 1757
		EaseOutQuint,
		// Token: 0x040006DE RID: 1758
		EaseInOutQuint,
		// Token: 0x040006DF RID: 1759
		EaseInSine,
		// Token: 0x040006E0 RID: 1760
		EaseOutSine,
		// Token: 0x040006E1 RID: 1761
		EaseInOutSine,
		// Token: 0x040006E2 RID: 1762
		EaseInExpo,
		// Token: 0x040006E3 RID: 1763
		EaseOutExpo,
		// Token: 0x040006E4 RID: 1764
		EaseInOutExpo,
		// Token: 0x040006E5 RID: 1765
		EaseInCirc,
		// Token: 0x040006E6 RID: 1766
		EaseOutCirc,
		// Token: 0x040006E7 RID: 1767
		EaseInOutCirc
	}

	// Token: 0x0200007E RID: 126
	// (Invoke) Token: 0x060005CE RID: 1486
	public delegate Vector3 ToVector3<T>(T v);

	// Token: 0x0200007F RID: 127
	// (Invoke) Token: 0x060005D2 RID: 1490
	public delegate float Function(float a, float b, float c, float d);
}
