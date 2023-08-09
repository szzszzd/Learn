using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x0200009A RID: 154
public abstract class Terminal : MonoBehaviour
{
	// Token: 0x06000692 RID: 1682 RVA: 0x00032100 File Offset: 0x00030300
	private static void InitTerminal()
	{
		if (Terminal.m_terminalInitialized)
		{
			return;
		}
		Terminal.m_terminalInitialized = true;
		new Terminal.ConsoleCommand("help", "Shows a list of console commands (optional: help 2 4 shows the second quarter)", delegate(Terminal.ConsoleEventArgs args)
		{
			if (ZNet.instance && ZNet.instance.IsServer())
			{
				Player.m_localPlayer;
			}
			args.Context.IsCheatsEnabled();
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, Terminal.ConsoleCommand> keyValuePair in Terminal.commands)
			{
				if (!keyValuePair.Value.IsSecret && keyValuePair.Value.IsValid(args.Context, false))
				{
					list.Add(keyValuePair.Value.Command + " - " + keyValuePair.Value.Description);
				}
			}
			list.Sort();
			if (args.Context != null)
			{
				int num = args.TryParameterInt(2, 5);
				int num2;
				if (args.TryParameterInt(1, out num2))
				{
					int num3 = list.Count / num;
					for (int j = num3 * (num2 - 1); j < Mathf.Min(list.Count, num3 * (num2 - 1) + num3); j++)
					{
						args.Context.AddString(list[j]);
					}
					return;
				}
				foreach (string text in list)
				{
					args.Context.AddString(text);
				}
			}
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("devcommands", "enables cheats", delegate(Terminal.ConsoleEventArgs args)
		{
			Terminal.m_cheat = !Terminal.m_cheat;
			Terminal context = args.Context;
			if (context != null)
			{
				context.AddString("Dev commands: " + Terminal.m_cheat.ToString());
			}
			Terminal context2 = args.Context;
			if (context2 != null)
			{
				context2.AddString("WARNING: using any dev commands is not recommended and is done at your own risk.");
			}
			Gogan.LogEvent("Cheat", "CheatsEnabled", Terminal.m_cheat.ToString(), 0L);
			args.Context.updateCommandList();
		}, false, false, false, true, false, null);
		new Terminal.ConsoleCommand("hidebetatext", "", delegate(Terminal.ConsoleEventArgs args)
		{
			if (Hud.instance)
			{
				Hud.instance.ToggleBetaTextVisible();
			}
		}, false, false, false, true, false, null);
		new Terminal.ConsoleCommand("ping", "ping server", delegate(Terminal.ConsoleEventArgs args)
		{
			if (Game.instance)
			{
				Game.instance.Ping();
			}
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("dpsdebug", "toggle dps debug print", delegate(Terminal.ConsoleEventArgs args)
		{
			Character.SetDPSDebug(!Character.IsDPSDebugEnabled());
			Terminal context = args.Context;
			if (context == null)
			{
				return;
			}
			context.AddString("DPS debug " + Character.IsDPSDebugEnabled().ToString());
		}, true, false, false, false, false, null);
		new Terminal.ConsoleCommand("lodbias", "set distance lod bias", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length == 1)
			{
				args.Context.AddString("Lod bias:" + QualitySettings.lodBias.ToString());
				return;
			}
			float lodBias;
			if (args.TryParameterFloat(1, out lodBias))
			{
				args.Context.AddString("Setting lod bias:" + lodBias.ToString());
				QualitySettings.lodBias = lodBias;
			}
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("info", "print system info", delegate(Terminal.ConsoleEventArgs args)
		{
			args.Context.AddString("Render threading mode:" + SystemInfo.renderingThreadingMode.ToString());
			long totalMemory = GC.GetTotalMemory(false);
			args.Context.AddString("Total allocated mem: " + (totalMemory / 1048576L).ToString("0") + "mb");
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("gc", "shows garbage collector information", delegate(Terminal.ConsoleEventArgs args)
		{
			long totalMemory = GC.GetTotalMemory(false);
			GC.Collect();
			long totalMemory2 = GC.GetTotalMemory(true);
			long num = totalMemory2 - totalMemory;
			args.Context.AddString(string.Concat(new string[]
			{
				"GC collect, Delta: ",
				(num / 1048576L).ToString("0"),
				"mb   Total left:",
				(totalMemory2 / 1048576L).ToString("0"),
				"mb"
			}));
		}, true, false, false, false, false, null);
		new Terminal.ConsoleCommand("cr", "unloads unused assets", delegate(Terminal.ConsoleEventArgs args)
		{
			args.Context.AddString("Unloading unused assets");
			Game.instance.CollectResources(true);
		}, true, false, false, false, false, null);
		new Terminal.ConsoleCommand("fov", "changes camera field of view", delegate(Terminal.ConsoleEventArgs args)
		{
			Camera mainCamera = Utils.GetMainCamera();
			if (mainCamera)
			{
				if (args.Length == 1)
				{
					args.Context.AddString("Fov:" + mainCamera.fieldOfView.ToString());
					return;
				}
				float num;
				if (args.TryParameterFloat(1, out num) && num > 5f)
				{
					args.Context.AddString("Setting fov to " + num.ToString());
					Camera[] componentsInChildren = mainCamera.GetComponentsInChildren<Camera>();
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						componentsInChildren[j].fieldOfView = num;
					}
				}
			}
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("kick", "[name/ip/userID] - kick user", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			string user = args[1];
			ZNet.instance.Kick(user);
			return true;
		}, false, true, false, false, false, null);
		new Terminal.ConsoleCommand("ban", "[name/ip/userID] - ban user", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			string user = args[1];
			ZNet.instance.Ban(user);
			return true;
		}, false, true, false, false, false, null);
		new Terminal.ConsoleCommand("unban", "[ip/userID] - unban user", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			string user = args[1];
			ZNet.instance.Unban(user);
			return true;
		}, false, true, false, false, false, null);
		new Terminal.ConsoleCommand("banned", "list banned users", delegate(Terminal.ConsoleEventArgs args)
		{
			ZNet.instance.PrintBanned();
		}, false, true, false, false, false, null);
		new Terminal.ConsoleCommand("save", "force saving of world and resets world save interval", delegate(Terminal.ConsoleEventArgs args)
		{
			ZNet.instance.ConsoleSave();
		}, false, true, false, false, false, null);
		new Terminal.ConsoleCommand("optterrain", "optimize old terrain modifications", delegate(Terminal.ConsoleEventArgs args)
		{
			TerrainComp.UpgradeTerrain();
		}, false, true, false, false, false, null);
		new Terminal.ConsoleCommand("genloc", "regenerate all locations.", delegate(Terminal.ConsoleEventArgs args)
		{
			ZoneSystem.instance.GenerateLocations();
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("players", "[nr] - force diffuculty scale ( 0 = reset)", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			int forcePlayerDifficulty;
			if (args.TryParameterInt(1, out forcePlayerDifficulty))
			{
				Game.instance.SetForcePlayerDifficulty(forcePlayerDifficulty);
				args.Context.AddString("Setting players to " + forcePlayerDifficulty.ToString());
			}
			return true;
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("exclusivefullscreen", "changes window mode to exclusive fullscreen, or back to borderless", delegate(Terminal.ConsoleEventArgs args)
		{
			if (Screen.fullScreenMode != FullScreenMode.ExclusiveFullScreen)
			{
				Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
				return;
			}
			Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("setkey", "[name]", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length >= 2)
			{
				ZoneSystem.instance.SetGlobalKey(args[1]);
				args.Context.AddString("Setting global key " + args[1]);
				return;
			}
			args.Context.AddString("Syntax: setkey [key]");
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("removekey", "[name]", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length >= 2)
			{
				ZoneSystem.instance.RemoveGlobalKey(args[1]);
				args.Context.AddString("Removing global key " + args[1]);
				return;
			}
			args.Context.AddString("Syntax: setkey [key]");
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("resetkeys", "[name]", delegate(Terminal.ConsoleEventArgs args)
		{
			ZoneSystem.instance.ResetGlobalKeys();
			args.Context.AddString("Global keys cleared");
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("listkeys", "", delegate(Terminal.ConsoleEventArgs args)
		{
			List<string> globalKeys = ZoneSystem.instance.GetGlobalKeys();
			args.Context.AddString("Keys " + globalKeys.Count.ToString());
			foreach (string text in globalKeys)
			{
				args.Context.AddString(text);
			}
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("debugmode", "fly mode", delegate(Terminal.ConsoleEventArgs args)
		{
			Player.m_debugMode = !Player.m_debugMode;
			args.Context.AddString("Debugmode " + Player.m_debugMode.ToString());
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("fly", "fly mode", delegate(Terminal.ConsoleEventArgs args)
		{
			Player.m_localPlayer.ToggleDebugFly();
			int debugFlySpeed;
			if (args.TryParameterInt(1, out debugFlySpeed))
			{
				Character.m_debugFlySpeed = debugFlySpeed;
			}
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("nocost", "no build cost", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.HasArgumentAnywhere("on", 0, true))
			{
				Player.m_localPlayer.SetNoPlacementCost(true);
				return;
			}
			if (args.HasArgumentAnywhere("off", 0, true))
			{
				Player.m_localPlayer.SetNoPlacementCost(false);
				return;
			}
			Player.m_localPlayer.ToggleNoPlacementCost();
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("raiseskill", "[skill] [amount]", delegate(Terminal.ConsoleEventArgs args)
		{
			int num;
			if (args.TryParameterInt(2, out num))
			{
				Player.m_localPlayer.GetSkills().CheatRaiseSkill(args[1], (float)num, true);
				return;
			}
			args.Context.AddString("Syntax: raiseskill [skill] [amount]");
		}, true, false, true, false, false, delegate()
		{
			List<string> list = Enum.GetNames(typeof(Skills.SkillType)).ToList<string>();
			list.Remove(Skills.SkillType.All.ToString());
			list.Remove(Skills.SkillType.None.ToString());
			return list;
		});
		new Terminal.ConsoleCommand("resetskill", "[skill]", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length > 1)
			{
				string name = args[1];
				Player.m_localPlayer.GetSkills().CheatResetSkill(name);
				return;
			}
			args.Context.AddString("Syntax: resetskill [skill]");
		}, true, false, true, false, false, delegate()
		{
			List<string> list = Enum.GetNames(typeof(Skills.SkillType)).ToList<string>();
			list.Remove(Skills.SkillType.All.ToString());
			list.Remove(Skills.SkillType.None.ToString());
			return list;
		});
		new Terminal.ConsoleCommand("sleep", "skips to next morning", delegate(Terminal.ConsoleEventArgs args)
		{
			EnvMan.instance.SkipToMorning();
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("skiptime", "[gameseconds] skips head in seconds", delegate(Terminal.ConsoleEventArgs args)
		{
			double num = ZNet.instance.GetTimeSeconds();
			float num2 = args.TryParameterFloat(1, 240f);
			num += (double)num2;
			ZNet.instance.SetNetTime(num);
			args.Context.AddString("Skipping " + num2.ToString("0") + "s , Day:" + EnvMan.instance.GetDay(num).ToString());
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("time", "shows current time", delegate(Terminal.ConsoleEventArgs args)
		{
			double timeSeconds = ZNet.instance.GetTimeSeconds();
			bool flag = EnvMan.instance.CanSleep();
			args.Context.AddString(string.Format("{0} sec, Day: {1} ({2}), {3}, Session start: {4}", new object[]
			{
				timeSeconds.ToString("0.00"),
				EnvMan.instance.GetDay(timeSeconds),
				EnvMan.instance.GetDayFraction().ToString("0.00"),
				flag ? "Can sleep" : "Can NOT sleep",
				ZoneSystem.instance.TimeSinceStart()
			}));
		}, true, false, false, false, false, null);
		new Terminal.ConsoleCommand("maxfps", "[FPS] sets fps limit", delegate(Terminal.ConsoleEventArgs args)
		{
			int num;
			if (args.TryParameterInt(1, out num))
			{
				Settings.FPSLimit = num;
				PlatformPrefs.SetInt("FPSLimit", num);
				return true;
			}
			return false;
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("resetcharacter", "reset character data", delegate(Terminal.ConsoleEventArgs args)
		{
			Terminal context = args.Context;
			if (context != null)
			{
				context.AddString("Reseting character");
			}
			Player.m_localPlayer.ResetCharacter();
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("tutorialreset", "reset tutorial data", delegate(Terminal.ConsoleEventArgs args)
		{
			Terminal context = args.Context;
			if (context != null)
			{
				context.AddString("Reseting tutorials");
			}
			Player.ResetSeenTutorials();
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("timescale", "[target] [fadetime, default: 1, max: 3] sets timescale", delegate(Terminal.ConsoleEventArgs args)
		{
			float b;
			if (args.TryParameterFloat(1, out b))
			{
				Game.FadeTimeScale(Mathf.Min(3f, b), args.TryParameterFloat(2, 0f));
				return true;
			}
			return false;
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("randomevent", "start a random event", delegate(Terminal.ConsoleEventArgs args)
		{
			RandEventSystem.instance.StartRandomEvent();
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("event", "[name] - start event", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			string text = args[1];
			if (!RandEventSystem.instance.HaveEvent(text))
			{
				args.Context.AddString("Random event not found:" + text);
				return true;
			}
			RandEventSystem.instance.SetRandomEventByName(text, Player.m_localPlayer.transform.position);
			return true;
		}, true, false, true, false, false, delegate()
		{
			List<string> list = new List<string>();
			foreach (RandomEvent randomEvent in RandEventSystem.instance.m_events)
			{
				list.Add(randomEvent.m_name);
			}
			return list;
		});
		new Terminal.ConsoleCommand("stopevent", "stop current event", delegate(Terminal.ConsoleEventArgs args)
		{
			RandEventSystem.instance.ResetRandomEvent();
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("removedrops", "remove all item-drops in area", delegate(Terminal.ConsoleEventArgs args)
		{
			int num = 0;
			foreach (ItemDrop itemDrop in UnityEngine.Object.FindObjectsOfType<ItemDrop>())
			{
				Fish component = itemDrop.gameObject.GetComponent<Fish>();
				if (!component || component.IsOutOfWater())
				{
					ZNetView component2 = itemDrop.GetComponent<ZNetView>();
					if (component2 && component2.IsValid() && component2.IsOwner())
					{
						component2.Destroy();
						num++;
					}
				}
			}
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Removed item drops: " + num.ToString(), 0, null);
		}, true, false, false, false, false, null);
		new Terminal.ConsoleCommand("removefish", "remove all fish", delegate(Terminal.ConsoleEventArgs args)
		{
			int num = 0;
			Fish[] array = UnityEngine.Object.FindObjectsOfType<Fish>();
			for (int j = 0; j < array.Length; j++)
			{
				ZNetView component = array[j].GetComponent<ZNetView>();
				if (component && component.IsValid() && component.IsOwner())
				{
					component.Destroy();
					num++;
				}
			}
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Removed fish: " + num.ToString(), 0, null);
		}, true, false, false, false, false, null);
		new Terminal.ConsoleCommand("printcreatures", "shows counts and levels of active creatures", delegate(Terminal.ConsoleEventArgs args)
		{
			Terminal.<>c__DisplayClass7_0 CS$<>8__locals2;
			CS$<>8__locals2.args = args;
			CS$<>8__locals2.counts = new Dictionary<string, Dictionary<int, int>>();
			Terminal.<InitTerminal>g__GetInfo|7_108(Character.GetAllCharacters(), ref CS$<>8__locals2);
			Terminal.<InitTerminal>g__GetInfo|7_108(UnityEngine.Object.FindObjectsOfType<RandomFlyingBird>(), ref CS$<>8__locals2);
			Terminal.<InitTerminal>g__GetInfo|7_108(UnityEngine.Object.FindObjectsOfType<Fish>(), ref CS$<>8__locals2);
			foreach (KeyValuePair<string, Dictionary<int, int>> keyValuePair in CS$<>8__locals2.counts)
			{
				string text = Localization.instance.Localize(keyValuePair.Key) + ": ";
				foreach (KeyValuePair<int, int> keyValuePair2 in keyValuePair.Value)
				{
					text += string.Format("Level {0}: {1}, ", keyValuePair2.Key, keyValuePair2.Value);
				}
				CS$<>8__locals2.args.Context.AddString(text);
			}
		}, true, false, false, false, false, null);
		new Terminal.ConsoleCommand("printnetobj", "[radius = 5] lists number of network objects by name surrounding the player", delegate(Terminal.ConsoleEventArgs args)
		{
			float num = args.TryParameterFloat(1, 5f);
			ZNetView[] array = UnityEngine.Object.FindObjectsOfType<ZNetView>();
			Terminal.<>c__DisplayClass7_1 CS$<>8__locals2;
			CS$<>8__locals2.counts = new Dictionary<string, int>();
			CS$<>8__locals2.total = 0;
			foreach (ZNetView znetView in array)
			{
				Transform transform = (znetView.transform.parent != null) ? znetView.transform.parent : znetView.transform;
				if (num <= 0f || Vector3.Distance(transform.position, Player.m_localPlayer.transform.position) <= num)
				{
					string name = transform.name;
					int num2 = name.IndexOf('(');
					if (num2 > 0)
					{
						Terminal.<InitTerminal>g__add|7_110(name.Substring(0, num2), ref CS$<>8__locals2);
					}
					else
					{
						Terminal.<InitTerminal>g__add|7_110("Other", ref CS$<>8__locals2);
					}
				}
			}
			args.Context.AddString(string.Format("Total network objects found: {0}", CS$<>8__locals2.total));
			foreach (KeyValuePair<string, int> keyValuePair in CS$<>8__locals2.counts)
			{
				args.Context.AddString(string.Format("   {0}: {1}", keyValuePair.Key, keyValuePair.Value));
			}
		}, true, false, false, false, false, null);
		new Terminal.ConsoleCommand("removebirds", "remove all birds", delegate(Terminal.ConsoleEventArgs args)
		{
			int num = 0;
			RandomFlyingBird[] array = UnityEngine.Object.FindObjectsOfType<RandomFlyingBird>();
			for (int j = 0; j < array.Length; j++)
			{
				ZNetView component = array[j].GetComponent<ZNetView>();
				if (component && component.IsValid() && component.IsOwner())
				{
					component.Destroy();
					num++;
				}
			}
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Removed birds: " + num.ToString(), 0, null);
		}, true, false, false, false, false, null);
		new Terminal.ConsoleCommand("printlocations", "shows counts of loaded locations", delegate(Terminal.ConsoleEventArgs args)
		{
			new Dictionary<string, Dictionary<int, int>>();
			foreach (Location location in UnityEngine.Object.FindObjectsOfType<Location>())
			{
				args.Context.AddString(string.Format("   {0}, Dist: {1}, Offset: {2}", location.name, Vector3.Distance(Player.m_localPlayer.transform.position, location.transform.position).ToString("0.0"), location.transform.position - Player.m_localPlayer.transform.position));
			}
		}, true, false, false, false, false, null);
		new Terminal.ConsoleCommand("find", "[text] [pingmax] searches loaded objects and location list matching name and pings them on the map. pingmax defaults to 1, if more will place pins on map instead", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			new Dictionary<string, Dictionary<int, int>>();
			GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
			string text = args[1].ToLower();
			List<Tuple<object, Vector3>> list = new List<Tuple<object, Vector3>>();
			foreach (GameObject gameObject in array)
			{
				if (gameObject.name.ToLower().Contains(text))
				{
					list.Add(new Tuple<object, Vector3>(gameObject, gameObject.transform.position));
				}
			}
			foreach (ZoneSystem.LocationInstance locationInstance in ZoneSystem.instance.GetLocationList())
			{
				if (locationInstance.m_location.m_prefabName.ToLower().Contains(text))
				{
					list.Add(new Tuple<object, Vector3>(locationInstance, locationInstance.m_position));
				}
			}
			List<ZDO> list2 = new List<ZDO>();
			int num = 0;
			while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(text, list2, ref num))
			{
			}
			foreach (ZDO zdo in list2)
			{
				list.Add(new Tuple<object, Vector3>(zdo, zdo.GetPosition()));
			}
			list.Sort((Tuple<object, Vector3> a, Tuple<object, Vector3> b) => Vector3.Distance(a.Item2, Player.m_localPlayer.transform.position).CompareTo(Vector3.Distance(b.Item2, Player.m_localPlayer.transform.position)));
			foreach (Tuple<object, Vector3> tuple in list)
			{
				Terminal context = args.Context;
				string format = "   {0}, Dist: {1}, Pos: {2}";
				GameObject gameObject2 = tuple.Item1 as GameObject;
				object arg;
				if (gameObject2 == null)
				{
					object item = tuple.Item1;
					if (item is ZoneSystem.LocationInstance)
					{
						ZoneSystem.LocationInstance locationInstance2 = (ZoneSystem.LocationInstance)item;
						arg = locationInstance2.m_location.m_location.gameObject.name.ToString();
					}
					else
					{
						arg = "unknown";
					}
				}
				else
				{
					arg = gameObject2.name.ToString();
				}
				context.AddString(string.Format(format, arg, Vector3.Distance(Player.m_localPlayer.transform.position, tuple.Item2).ToString("0.0"), tuple.Item2));
			}
			foreach (Minimap.PinData pin in args.Context.m_findPins)
			{
				Minimap.instance.RemovePin(pin);
			}
			args.Context.m_findPins.Clear();
			int num2 = Math.Min(list.Count, args.TryParameterInt(2, 1));
			if (num2 == 1)
			{
				Chat.instance.SendPing(list[0].Item2);
			}
			else
			{
				for (int k = 0; k < num2; k++)
				{
					List<Minimap.PinData> findPins = args.Context.m_findPins;
					Minimap instance = Minimap.instance;
					Vector3 item2 = list[k].Item2;
					Minimap.PinType type = (list[k].Item1 is ZDO) ? Minimap.PinType.Icon2 : ((list[k].Item1 is ZoneSystem.LocationInstance) ? Minimap.PinType.Icon1 : Minimap.PinType.Icon3);
					ZDO zdo2 = list[k].Item1 as ZDO;
					findPins.Add(instance.AddPin(item2, type, (zdo2 != null) ? zdo2.GetString(ZDOVars.s_tag, "") : "", false, true, Player.m_localPlayer.GetPlayerID()));
				}
			}
			args.Context.AddString(string.Format("Found {0} objects containing '{1}'", list.Count, text));
			return true;
		}, true, false, false, false, false, delegate()
		{
			if (!ZNetScene.instance)
			{
				return null;
			}
			List<string> list = new List<string>(ZNetScene.instance.GetPrefabNames());
			foreach (ZoneSystem.ZoneLocation zoneLocation in ZoneSystem.instance.m_locations)
			{
				list.Add(zoneLocation.m_prefabName);
			}
			return list;
		});
		new Terminal.ConsoleCommand("freefly", "freefly photo mode", delegate(Terminal.ConsoleEventArgs args)
		{
			args.Context.AddString("Toggling free fly camera");
			GameCamera.instance.ToggleFreeFly();
		}, true, false, false, false, false, null);
		new Terminal.ConsoleCommand("ffsmooth", "freefly smoothness", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length <= 1)
			{
				args.Context.AddString(GameCamera.instance.GetFreeFlySmoothness().ToString());
				return true;
			}
			float freeFlySmoothness;
			if (args.TryParameterFloat(1, out freeFlySmoothness))
			{
				args.Context.AddString("Setting free fly camera smoothing:" + freeFlySmoothness.ToString());
				GameCamera.instance.SetFreeFlySmoothness(freeFlySmoothness);
				return true;
			}
			return false;
		}, true, false, false, false, false, null);
		new Terminal.ConsoleCommand("location", "[SAVE*] spawn location (CAUTION: saving permanently disabled, *unless you specify SAVE)", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			string name = args[1];
			Vector3 pos = Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 10f;
			ZoneSystem.instance.TestSpawnLocation(name, pos, args.Length < 3 || args[2] != "SAVE");
			return true;
		}, true, false, true, false, false, delegate()
		{
			List<string> list = new List<string>();
			foreach (ZoneSystem.ZoneLocation zoneLocation in ZoneSystem.instance.m_locations)
			{
				if (zoneLocation.m_prefab != null)
				{
					list.Add(zoneLocation.m_prefabName);
				}
			}
			return list;
		});
		new Terminal.ConsoleCommand("nextseed", "forces the next dungeon to a seed (CAUTION: saving permanently disabled)", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return true;
			}
			int forceSeed;
			if (args.TryParameterInt(1, out forceSeed))
			{
				DungeonGenerator.m_forceSeed = forceSeed;
				ZoneSystem.instance.m_didZoneTest = true;
				MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Location seed set, world saving DISABLED until restart", 0, null);
			}
			return true;
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("spawn", "[amount] [level] [p/e/i] - spawn something. (End word with a star (*) to create each object containing that word.) Add a 'p' after to try to pick up the spawned items, adding 'e' will try to use/equip, 'i' will only spawn and pickup if you don't have one in your inventory.", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length <= 1 || !ZNetScene.instance)
			{
				return false;
			}
			string text = args[1];
			Terminal.<>c__DisplayClass7_2 CS$<>8__locals2;
			CS$<>8__locals2.count = args.TryParameterInt(2, 1);
			CS$<>8__locals2.level = args.TryParameterInt(3, 1);
			CS$<>8__locals2.pickup = args.HasArgumentAnywhere("p", 2, true);
			CS$<>8__locals2.use = args.HasArgumentAnywhere("e", 2, true);
			CS$<>8__locals2.onlyIfMissing = args.HasArgumentAnywhere("i", 2, true);
			DateTime now = DateTime.Now;
			if (text.Length >= 2 && text[text.Length - 1] == '*')
			{
				text = text.Substring(0, text.Length - 1).ToLower();
				using (List<string>.Enumerator enumerator = ZNetScene.instance.GetPrefabNames().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string text2 = enumerator.Current;
						string text3 = text2.ToLower();
						if (text3.Contains(text) && (text.Contains("fx") || !text3.Contains("fx")))
						{
							Terminal.<InitTerminal>g__spawn|7_112(text2, ref CS$<>8__locals2);
						}
					}
					goto IL_12E;
				}
			}
			Terminal.<InitTerminal>g__spawn|7_112(text, ref CS$<>8__locals2);
			IL_12E:
			ZLog.Log("Spawn time :" + (DateTime.Now - now).TotalMilliseconds.ToString() + " ms");
			Gogan.LogEvent("Cheat", "Spawn", text, (long)CS$<>8__locals2.count);
			return true;
		}, true, false, true, false, false, delegate()
		{
			if (!ZNetScene.instance)
			{
				return new List<string>();
			}
			return ZNetScene.instance.GetPrefabNames();
		});
		new Terminal.ConsoleCommand("catch", "[fishname] [level] simulates catching a fish", delegate(Terminal.ConsoleEventArgs args)
		{
			string text = args[1];
			int num = args.TryParameterInt(2, 1);
			num = Mathf.Min(num, 4);
			GameObject prefab = ZNetScene.instance.GetPrefab(text);
			if (!prefab)
			{
				return "No prefab named: " + text;
			}
			Fish componentInChildren = prefab.GetComponentInChildren<Fish>();
			if (!componentInChildren)
			{
				return "No fish prefab named: " + text;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, Player.m_localPlayer.transform.position, Quaternion.identity);
			componentInChildren = gameObject.GetComponentInChildren<Fish>();
			ItemDrop component = gameObject.GetComponent<ItemDrop>();
			if (component)
			{
				component.SetQuality(num);
			}
			string msg = FishingFloat.Catch(componentInChildren, Player.m_localPlayer);
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, msg, 0, null);
			return true;
		}, true, false, false, false, false, () => new List<string>
		{
			"Fish1",
			"Fish2",
			"Fish3",
			"Fish4_cave",
			"Fish5",
			"Fish6",
			"Fish7",
			"Fish8",
			"Fish9",
			"Fish10",
			"Fish11",
			"Fish12"
		});
		new Terminal.ConsoleCommand("itemset", "[name] [keep] - spawn a premade named set, add 'keep' to not drop current items", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length >= 2)
			{
				ItemSets.instance.TryGetSet(args.Args[1], args.Length < 3 || args[2].ToLower() != "keep");
				return true;
			}
			return false;
		}, true, false, true, false, false, () => ItemSets.instance.GetSetNames());
		new Terminal.ConsoleCommand("pos", "print current player position", delegate(Terminal.ConsoleEventArgs args)
		{
			Player localPlayer = Player.m_localPlayer;
			if (localPlayer)
			{
				Terminal context = args.Context;
				if (context == null)
				{
					return;
				}
				context.AddString("Player position (X,Y,Z):" + localPlayer.transform.position.ToString("F0"));
			}
		}, true, false, false, false, false, null);
		new Terminal.ConsoleCommand("recall", "[*name] recalls players to you, optionally that match given name", delegate(Terminal.ConsoleEventArgs args)
		{
			foreach (ZNetPeer znetPeer in ZNet.instance.GetPeers())
			{
				if (znetPeer.m_playerName != Player.m_localPlayer.GetPlayerName() && (args.Length < 2 || znetPeer.m_playerName.ToLower().Contains(args[1].ToLower())))
				{
					Chat.instance.TeleportPlayer(znetPeer.m_uid, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation, true);
				}
			}
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("goto", "[x,z] - teleport", delegate(Terminal.ConsoleEventArgs args)
		{
			int num;
			int num2;
			if (args.Length < 3 || !args.TryParameterInt(1, out num) || !args.TryParameterInt(2, out num2))
			{
				return false;
			}
			Player localPlayer = Player.m_localPlayer;
			if (localPlayer)
			{
				Vector3 vector = new Vector3((float)num, localPlayer.transform.position.y, (float)num2);
				float max = localPlayer.IsDebugFlying() ? 400f : ZoneSystem.instance.m_waterLevel;
				vector.y = Mathf.Clamp(vector.y, ZoneSystem.instance.m_waterLevel, max);
				localPlayer.TeleportTo(vector, localPlayer.transform.rotation, true);
			}
			Gogan.LogEvent("Cheat", "Goto", "", 0L);
			return true;
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("exploremap", "explore entire map", delegate(Terminal.ConsoleEventArgs args)
		{
			Minimap.instance.ExploreAll();
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("resetmap", "reset map exploration", delegate(Terminal.ConsoleEventArgs args)
		{
			Minimap.instance.Reset();
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("resetsharedmap", "removes any shared map data from cartography table", delegate(Terminal.ConsoleEventArgs args)
		{
			Minimap.instance.ResetSharedMapData();
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("restartparty", "restart playfab party network", delegate(Terminal.ConsoleEventArgs args)
		{
			if (ZNet.m_onlineBackend == OnlineBackendType.PlayFab)
			{
				if (ZNet.instance.IsServer())
				{
					ZPlayFabMatchmaking.ResetParty();
					return;
				}
				ZPlayFabSocket.ScheduleResetParty();
			}
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("puke", "empties your stomach of food", delegate(Terminal.ConsoleEventArgs args)
		{
			if (Player.m_localPlayer)
			{
				Player.m_localPlayer.ClearFood();
			}
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("tame", "tame all nearby tameable creatures", delegate(Terminal.ConsoleEventArgs args)
		{
			Tameable.TameAllInArea(Player.m_localPlayer.transform.position, 20f);
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("aggravate", "aggravated all nearby neutrals", delegate(Terminal.ConsoleEventArgs args)
		{
			BaseAI.AggravateAllInArea(Player.m_localPlayer.transform.position, 20f, BaseAI.AggravatedReason.Damage);
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("killall", "kill nearby creatures", delegate(Terminal.ConsoleEventArgs args)
		{
			List<Character> allCharacters = Character.GetAllCharacters();
			int num = 0;
			foreach (Character character in allCharacters)
			{
				if (!character.IsPlayer())
				{
					HitData hitData = new HitData();
					hitData.m_damage.m_damage = 1E+10f;
					character.Damage(hitData);
					num++;
				}
			}
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Killing all the monsters:" + num.ToString(), 0, null);
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("killenemies", "kill nearby enemies", delegate(Terminal.ConsoleEventArgs args)
		{
			List<Character> allCharacters = Character.GetAllCharacters();
			int num = 0;
			foreach (Character character in allCharacters)
			{
				if (!character.IsPlayer() && !character.IsTamed())
				{
					HitData hitData = new HitData();
					hitData.m_damage.m_damage = 1E+10f;
					character.Damage(hitData);
					num++;
				}
			}
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Killing all the monsters:" + num.ToString(), 0, null);
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("killtame", "kill nearby tame creatures.", delegate(Terminal.ConsoleEventArgs args)
		{
			List<Character> allCharacters = Character.GetAllCharacters();
			int num = 0;
			foreach (Character character in allCharacters)
			{
				if (!character.IsPlayer() && character.IsTamed())
				{
					HitData hitData = new HitData();
					hitData.m_damage.m_damage = 1E+10f;
					character.Damage(hitData);
					num++;
				}
			}
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Killing all tame creatures:" + num.ToString(), 0, null);
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("heal", "heal to full health & stamina", delegate(Terminal.ConsoleEventArgs args)
		{
			Player.m_localPlayer.Heal(Player.m_localPlayer.GetMaxHealth(), true);
			Player.m_localPlayer.AddStamina(Player.m_localPlayer.GetMaxStamina());
			Player.m_localPlayer.AddEitr(Player.m_localPlayer.GetMaxEitr());
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("god", "invincible mode", delegate(Terminal.ConsoleEventArgs args)
		{
			Player.m_localPlayer.SetGodMode(args.HasArgumentAnywhere("on", 0, true) || (!args.HasArgumentAnywhere("off", 0, true) && !Player.m_localPlayer.InGodMode()));
			args.Context.AddString("God mode:" + Player.m_localPlayer.InGodMode().ToString());
			Gogan.LogEvent("Cheat", "God", Player.m_localPlayer.InGodMode().ToString(), 0L);
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("ghost", "", delegate(Terminal.ConsoleEventArgs args)
		{
			Player.m_localPlayer.SetGhostMode(args.HasArgumentAnywhere("on", 0, true) || (!args.HasArgumentAnywhere("off", 0, true) && !Player.m_localPlayer.InGhostMode()));
			args.Context.AddString("Ghost mode:" + Player.m_localPlayer.InGhostMode().ToString());
			Gogan.LogEvent("Cheat", "Ghost", Player.m_localPlayer.InGhostMode().ToString(), 0L);
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("beard", "change beard", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			if (Player.m_localPlayer)
			{
				Player.m_localPlayer.SetBeard(args[1]);
			}
			return true;
		}, true, false, true, false, false, delegate()
		{
			List<string> list = new List<string>();
			foreach (ItemDrop itemDrop in ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Beard"))
			{
				list.Add(itemDrop.name);
			}
			return list;
		});
		new Terminal.ConsoleCommand("hair", "change hair", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			if (Player.m_localPlayer)
			{
				Player.m_localPlayer.SetHair(args[1]);
			}
			return true;
		}, true, false, true, false, false, delegate()
		{
			List<string> list = new List<string>();
			foreach (ItemDrop itemDrop in ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Hair"))
			{
				list.Add(itemDrop.name);
			}
			return list;
		});
		new Terminal.ConsoleCommand("model", "change player model", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			int playerModel;
			if (Player.m_localPlayer && args.TryParameterInt(1, out playerModel))
			{
				Player.m_localPlayer.SetPlayerModel(playerModel);
			}
			return true;
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("tod", "-1 OR [0-1]", delegate(Terminal.ConsoleEventArgs args)
		{
			float num;
			if (EnvMan.instance == null || args.Length < 2 || !args.TryParameterFloat(1, out num))
			{
				return false;
			}
			args.Context.AddString("Setting time of day:" + num.ToString());
			if (num < 0f)
			{
				EnvMan.instance.m_debugTimeOfDay = false;
			}
			else
			{
				EnvMan.instance.m_debugTimeOfDay = true;
				EnvMan.instance.m_debugTime = Mathf.Clamp01(num);
			}
			return true;
		}, true, false, true, false, true, null);
		new Terminal.ConsoleCommand("env", "[env] override environment", delegate(Terminal.ConsoleEventArgs args)
		{
			if (EnvMan.instance == null || args.Length < 2)
			{
				return false;
			}
			string text = string.Join(" ", args.Args, 1, args.Args.Length - 1);
			args.Context.AddString("Setting debug enviornment:" + text);
			EnvMan.instance.m_debugEnv = text;
			return true;
		}, true, false, true, false, true, delegate()
		{
			List<string> list = new List<string>();
			foreach (EnvSetup envSetup in EnvMan.instance.m_environments)
			{
				list.Add(envSetup.m_name);
			}
			return list;
		});
		new Terminal.ConsoleCommand("resetenv", "disables environment override", delegate(Terminal.ConsoleEventArgs args)
		{
			if (EnvMan.instance == null)
			{
				return false;
			}
			args.Context.AddString("Resetting debug environment");
			EnvMan.instance.m_debugEnv = "";
			return true;
		}, true, false, true, false, true, null);
		new Terminal.ConsoleCommand("wind", "[angle] [intensity]", delegate(Terminal.ConsoleEventArgs args)
		{
			float angle;
			float intensity;
			if (args.TryParameterFloat(1, out angle) && args.TryParameterFloat(2, out intensity))
			{
				EnvMan.instance.SetDebugWind(angle, intensity);
				return true;
			}
			return false;
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("resetwind", "", delegate(Terminal.ConsoleEventArgs args)
		{
			EnvMan.instance.ResetDebugWind();
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("clear", "clear the console window", delegate(Terminal.ConsoleEventArgs args)
		{
			args.Context.m_chatBuffer.Clear();
			args.Context.UpdateChat();
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("clearstatus", "clear any status modifiers", delegate(Terminal.ConsoleEventArgs args)
		{
			Player.m_localPlayer.ClearHardDeath();
			Player.m_localPlayer.GetSEMan().RemoveAllStatusEffects(false);
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("addstatus", "[name] adds a status effect (ex: Rested, Burning, SoftDeath, Wet, etc)", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			Player.m_localPlayer.GetSEMan().AddStatusEffect(args[1].GetStableHashCode(), true, 0, 0f);
			return true;
		}, true, false, true, false, false, delegate()
		{
			List<StatusEffect> statusEffects = ObjectDB.instance.m_StatusEffects;
			List<string> list = new List<string>();
			foreach (StatusEffect statusEffect in statusEffects)
			{
				list.Add(statusEffect.name);
			}
			return list;
		});
		new Terminal.ConsoleCommand("setpower", "[name] sets your current guardian power and resets cooldown (ex: GP_Eikthyr, GP_TheElder, etc)", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			Player.m_localPlayer.SetGuardianPower(args[1]);
			Player.m_localPlayer.m_guardianPowerCooldown = 0f;
			return true;
		}, true, false, true, false, false, delegate()
		{
			List<StatusEffect> statusEffects = ObjectDB.instance.m_StatusEffects;
			List<string> list = new List<string>();
			foreach (StatusEffect statusEffect in statusEffects)
			{
				list.Add(statusEffect.name);
			}
			return list;
		});
		new Terminal.ConsoleCommand("bind", "[keycode] [command and parameters] bind a key to a console command. note: may cause conflicts with game controls", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			KeyCode keyCode;
			if (!Enum.TryParse<KeyCode>(args[1], true, out keyCode))
			{
				args.Context.AddString("'" + args[1] + "' is not a valid UnityEngine.KeyCode.");
			}
			else
			{
				string item = string.Join(" ", args.Args, 1, args.Length - 1);
				Terminal.m_bindList.Add(item);
				Terminal.updateBinds();
			}
			return true;
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("unbind", "[keycode] clears all binds connected to keycode", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				return false;
			}
			for (int j = Terminal.m_bindList.Count - 1; j >= 0; j--)
			{
				if (Terminal.m_bindList[j].Split(new char[]
				{
					' '
				})[0].ToLower() == args[1].ToLower())
				{
					Terminal.m_bindList.RemoveAt(j);
				}
			}
			Terminal.updateBinds();
			return true;
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("printbinds", "prints current binds", delegate(Terminal.ConsoleEventArgs args)
		{
			foreach (string text in Terminal.m_bindList)
			{
				args.Context.AddString(text);
			}
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("resetbinds", "resets all custom binds to default dev commands", delegate(Terminal.ConsoleEventArgs args)
		{
			for (int j = Terminal.m_bindList.Count - 1; j >= 0; j--)
			{
				Terminal.m_bindList.Remove(Terminal.m_bindList[j]);
			}
			Terminal.updateBinds();
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("test", "[key] [value] set test string, with optional value. set empty existing key to remove", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.Length < 2)
			{
				Terminal.m_showTests = !Terminal.m_showTests;
				return true;
			}
			string text = (args.Length >= 3) ? args[2] : "";
			if (Terminal.m_testList.ContainsKey(args[1]) && text.Length == 0)
			{
				Terminal.m_testList.Remove(args[1]);
				Terminal context = args.Context;
				if (context != null)
				{
					context.AddString("'" + args[1] + "' removed");
				}
			}
			else
			{
				Terminal.m_testList[args[1]] = text;
				Terminal context2 = args.Context;
				if (context2 != null)
				{
					context2.AddString(string.Concat(new string[]
					{
						"'",
						args[1],
						"' added with value '",
						text,
						"'"
					}));
				}
			}
			return true;
		}, true, false, false, true, false, null);
		new Terminal.ConsoleCommand("forcedelete", "[radius] [*name] force remove all objects within given radius. If name is entered, only deletes items with matching names. Caution! Use at your own risk. Make backups! Radius default: 5, max: 50.", delegate(Terminal.ConsoleEventArgs args)
		{
			if (Player.m_localPlayer == null)
			{
				return false;
			}
			float num = Math.Min(50f, args.TryParameterFloat(1, 5f));
			foreach (GameObject gameObject in UnityEngine.Object.FindObjectsOfType(typeof(GameObject)))
			{
				if (Vector3.Distance(gameObject.transform.position, Player.m_localPlayer.transform.position) < num)
				{
					string path = gameObject.gameObject.transform.GetPath();
					if (!(gameObject.GetComponentInParent<Game>() != null) && !(gameObject.GetComponentInParent<Player>() != null) && !(gameObject.GetComponentInParent<Valkyrie>() != null) && !(gameObject.GetComponentInParent<LocationProxy>() != null) && !(gameObject.GetComponentInParent<Room>() != null) && !(gameObject.GetComponentInParent<Vegvisir>() != null) && !(gameObject.GetComponentInParent<DungeonGenerator>() != null) && !(gameObject.GetComponentInParent<TombStone>() != null) && !path.Contains("StartTemple") && !path.Contains("BossStone") && (args.Length <= 2 || gameObject.name.ToLower().Contains(args[2].ToLower())))
					{
						Destructible component = gameObject.GetComponent<Destructible>();
						ZNetView component2 = gameObject.GetComponent<ZNetView>();
						if (component != null)
						{
							component.DestroyNow();
						}
						else if (component2 != null && ZNetScene.instance)
						{
							ZNetScene.instance.Destroy(gameObject);
						}
					}
				}
			}
			return true;
		}, true, false, true, false, false, null);
		new Terminal.ConsoleCommand("printseeds", "print seeds of loaded dungeons", delegate(Terminal.ConsoleEventArgs args)
		{
			if (Player.m_localPlayer == null)
			{
				return false;
			}
			Math.Min(20f, args.TryParameterFloat(1, 5f));
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(DungeonGenerator));
			args.Context.AddString(((ZNet.instance && ZNet.instance.IsServer()) ? "Server" : "Client") + " version " + global::Version.GetVersionString(false));
			foreach (DungeonGenerator dungeonGenerator in array)
			{
				args.Context.AddString(string.Format("  {0}: Seed: {1}/{2}, Hash: {3}, Distance: {4}", new object[]
				{
					dungeonGenerator.name,
					dungeonGenerator.m_generatedSeed,
					dungeonGenerator.GetSeed(),
					dungeonGenerator.m_generatedHash,
					Utils.DistanceXZ(Player.m_localPlayer.transform.position, dungeonGenerator.transform.position).ToString("0.0")
				}));
			}
			return true;
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("nomap", "disables map for this character. If used as host, will disable for all joining players from now on.", delegate(Terminal.ConsoleEventArgs args)
		{
			if (Player.m_localPlayer != null)
			{
				string key = "mapenabled_" + Player.m_localPlayer.GetPlayerName();
				bool flag = PlayerPrefs.GetFloat(key, 1f) == 1f;
				PlayerPrefs.SetFloat(key, (float)(flag ? 0 : 1));
				Minimap.instance.SetMapMode(Minimap.MapMode.None);
				Terminal context = args.Context;
				if (context != null)
				{
					context.AddString("Map " + (flag ? "disabled" : "enabled"));
				}
				if (ZNet.instance && ZNet.instance.IsServer())
				{
					if (flag)
					{
						ZoneSystem.instance.SetGlobalKey("nomap");
						return;
					}
					ZoneSystem.instance.RemoveGlobalKey("nomap");
				}
			}
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("noportals", "disables portals for server.", delegate(Terminal.ConsoleEventArgs args)
		{
			if (Player.m_localPlayer != null)
			{
				bool globalKey = ZoneSystem.instance.GetGlobalKey("noportals");
				if (globalKey)
				{
					ZoneSystem.instance.RemoveGlobalKey("noportals");
				}
				else
				{
					ZoneSystem.instance.SetGlobalKey("noportals");
				}
				Terminal context = args.Context;
				if (context == null)
				{
					return;
				}
				context.AddString("Portals " + (globalKey ? "enabled" : "disabled"));
			}
		}, false, false, true, false, false, null);
		new Terminal.ConsoleCommand("resetspawn", "resets spawn location", delegate(Terminal.ConsoleEventArgs args)
		{
			if (!Game.instance)
			{
				return false;
			}
			PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
			if (playerProfile != null)
			{
				playerProfile.ClearCustomSpawnPoint();
			}
			Terminal context = args.Context;
			if (context != null)
			{
				context.AddString("Reseting spawn point");
			}
			return true;
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("die", "kill yourself", delegate(Terminal.ConsoleEventArgs args)
		{
			if (!Player.m_localPlayer)
			{
				return false;
			}
			HitData hitData = new HitData();
			hitData.m_damage.m_damage = 99999f;
			Player.m_localPlayer.Damage(hitData);
			return true;
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("say", "chat message", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.FullLine.Length < 5 || Chat.instance == null)
			{
				return false;
			}
			Chat.instance.SendText(Talker.Type.Normal, args.FullLine.Substring(4));
			return true;
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("s", "shout message", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.FullLine.Length < 3 || Chat.instance == null)
			{
				return false;
			}
			Chat.instance.SendText(Talker.Type.Shout, args.FullLine.Substring(2));
			return true;
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("w", "[playername] whispers a private message to a player", delegate(Terminal.ConsoleEventArgs args)
		{
			if (args.FullLine.Length < 3 || Chat.instance == null)
			{
				return false;
			}
			Chat.instance.SendText(Talker.Type.Whisper, args.FullLine.Substring(2));
			return true;
		}, false, false, false, false, false, null);
		new Terminal.ConsoleCommand("resetplayerprefs", "Resets any saved settings and variables (not the save game)", delegate(Terminal.ConsoleEventArgs args)
		{
			PlayerPrefs.DeleteAll();
			Terminal context = args.Context;
			if (context == null)
			{
				return;
			}
			context.AddString("Reset saved player preferences");
		}, false, false, false, true, true, null);
		for (int i = 0; i < 20; i++)
		{
			Emotes emote = (Emotes)i;
			new Terminal.ConsoleCommand(emote.ToString().ToLower(), string.Format("emote: {0}", emote), delegate(Terminal.ConsoleEventArgs args)
			{
				Emote.DoEmote(emote);
			}, false, false, false, false, false, null);
		}
	}

	// Token: 0x06000693 RID: 1683 RVA: 0x000336AC File Offset: 0x000318AC
	protected static void updateBinds()
	{
		Terminal.m_binds.Clear();
		foreach (string text in Terminal.m_bindList)
		{
			string[] array = text.Split(new char[]
			{
				' '
			});
			string item = string.Join(" ", array, 1, array.Length - 1);
			KeyCode key;
			if (Enum.TryParse<KeyCode>(array[0], true, out key))
			{
				List<string> list;
				if (Terminal.m_binds.TryGetValue(key, out list))
				{
					list.Add(item);
				}
				else
				{
					Terminal.m_binds[key] = new List<string>
					{
						item
					};
				}
			}
		}
		PlayerPrefs.SetString("ConsoleBindings", string.Join("\n", Terminal.m_bindList));
	}

	// Token: 0x06000694 RID: 1684 RVA: 0x0003377C File Offset: 0x0003197C
	private void updateCommandList()
	{
		this.m_commandList.Clear();
		foreach (KeyValuePair<string, Terminal.ConsoleCommand> keyValuePair in Terminal.commands)
		{
			if (keyValuePair.Value.IsValid(this, false) && (this.m_autoCompleteSecrets || !keyValuePair.Value.IsSecret))
			{
				this.m_commandList.Add(keyValuePair.Key);
			}
		}
	}

	// Token: 0x06000695 RID: 1685 RVA: 0x0003380C File Offset: 0x00031A0C
	public bool IsCheatsEnabled()
	{
		return Terminal.m_cheat && ZNet.instance && ZNet.instance.IsServer();
	}

	// Token: 0x06000696 RID: 1686 RVA: 0x00033830 File Offset: 0x00031A30
	public void TryRunCommand(string text, bool silentFail = false, bool skipAllowedCheck = false)
	{
		string[] array = text.Split(new char[]
		{
			' '
		});
		Terminal.ConsoleCommand consoleCommand;
		if (Terminal.commands.TryGetValue(array[0].ToLower(), out consoleCommand))
		{
			if (consoleCommand.IsValid(this, skipAllowedCheck))
			{
				consoleCommand.RunAction(new Terminal.ConsoleEventArgs(text, this));
				return;
			}
			if (!silentFail)
			{
				this.AddString("'" + text.Split(new char[]
				{
					' '
				})[0] + "' is not valid in the current context.");
				return;
			}
		}
		else if (!silentFail)
		{
			this.AddString("'" + array[0] + "' is not a recognized command. Type 'help' to see a list of valid commands.");
		}
	}

	// Token: 0x06000697 RID: 1687 RVA: 0x000338C4 File Offset: 0x00031AC4
	public virtual void Awake()
	{
		Terminal.InitTerminal();
		this.m_gamepadTextInput = new TextInputHandler(new TextInputEvent(this.onGamePadTextInput));
	}

	// Token: 0x06000698 RID: 1688 RVA: 0x000338E3 File Offset: 0x00031AE3
	public virtual void Update()
	{
		if (this.m_focused)
		{
			this.UpdateInput();
		}
	}

	// Token: 0x06000699 RID: 1689 RVA: 0x000338F4 File Offset: 0x00031AF4
	private void UpdateInput()
	{
		if (ZInput.GetButtonDown("ChatUp") || ZInput.GetButtonDown("JoyDPadUp"))
		{
			if (this.m_historyPosition > 0)
			{
				this.m_historyPosition--;
			}
			this.m_input.text = ((this.m_history.Count > 0) ? this.m_history[this.m_historyPosition] : "");
			this.m_input.caretPosition = this.m_input.text.Length;
		}
		if (ZInput.GetButtonDown("ChatDown") || ZInput.GetButtonDown("JoyDPadDown"))
		{
			if (this.m_historyPosition < this.m_history.Count)
			{
				this.m_historyPosition++;
			}
			this.m_input.text = ((this.m_historyPosition < this.m_history.Count) ? this.m_history[this.m_historyPosition] : "");
			this.m_input.caretPosition = this.m_input.text.Length;
		}
		if ((ZInput.GetButtonDown("ScrollChatUp") || ZInput.GetButtonDown("JoyScrollChatUp")) && this.m_scrollHeight < this.m_chatBuffer.Count - 5)
		{
			this.m_scrollHeight++;
			this.UpdateChat();
		}
		if ((ZInput.GetButtonDown("ScrollChatDown") || ZInput.GetButtonDown("JoyScrollChatDown")) && this.m_scrollHeight > 0)
		{
			this.m_scrollHeight--;
			this.UpdateChat();
		}
		if (this.m_input.caretPosition != this.m_tabCaretPositionEnd)
		{
			this.m_tabCaretPosition = -1;
		}
		if (this.m_lastSearchLength != this.m_input.text.Length)
		{
			this.m_lastSearchLength = this.m_input.text.Length;
			if (this.m_commandList.Count == 0)
			{
				this.updateCommandList();
			}
			string[] array = this.m_input.text.Split(new char[]
			{
				' '
			});
			if (array.Length == 1)
			{
				this.updateSearch(array[0], this.m_commandList, true);
			}
			else
			{
				string key = (this.m_tabPrefix == '\0') ? array[0] : ((array[0].Length == 0) ? "" : array[0].Substring(1));
				Terminal.ConsoleCommand consoleCommand;
				if (Terminal.commands.TryGetValue(key, out consoleCommand))
				{
					this.updateSearch(array[1], consoleCommand.GetTabOptions(), false);
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.Tab) || ZInput.GetButtonDown("JoyDPadRight"))
		{
			if (this.m_commandList.Count == 0)
			{
				this.updateCommandList();
			}
			string[] array2 = this.m_input.text.Split(new char[]
			{
				' '
			});
			if (array2.Length == 1)
			{
				this.tabCycle(array2[0], this.m_commandList, true);
			}
			else
			{
				string key2 = (this.m_tabPrefix == '\0') ? array2[0] : array2[0].Substring(1);
				Terminal.ConsoleCommand consoleCommand2;
				if (Terminal.commands.TryGetValue(key2, out consoleCommand2))
				{
					this.tabCycle(array2[1], consoleCommand2.GetTabOptions(), false);
				}
			}
		}
		this.m_input.gameObject.SetActive(true);
		this.m_input.ActivateInputField();
		if (Input.GetKeyDown(KeyCode.Return) || ZInput.GetButtonDown("JoyButtonA"))
		{
			this.SendInput();
			EventSystem.current.SetSelectedGameObject(null);
			this.m_input.gameObject.SetActive(false);
		}
	}

	// Token: 0x0600069A RID: 1690 RVA: 0x00033C40 File Offset: 0x00031E40
	protected void SendInput()
	{
		if (string.IsNullOrEmpty(this.m_input.text))
		{
			return;
		}
		this.InputText();
		if (this.m_history.Count == 0 || this.m_history[this.m_history.Count - 1] != this.m_input.text)
		{
			this.m_history.Add(this.m_input.text);
		}
		this.m_historyPosition = this.m_history.Count;
		this.m_input.text = "";
		this.m_scrollHeight = 0;
		this.UpdateChat();
	}

	// Token: 0x0600069B RID: 1691 RVA: 0x00033CE4 File Offset: 0x00031EE4
	protected virtual void InputText()
	{
		string text = this.m_input.text;
		this.AddString(text);
		this.TryRunCommand(text, false, false);
	}

	// Token: 0x0600069C RID: 1692 RVA: 0x0000290F File Offset: 0x00000B0F
	protected virtual bool isAllowedCommand(Terminal.ConsoleCommand cmd)
	{
		return true;
	}

	// Token: 0x0600069D RID: 1693 RVA: 0x00033D10 File Offset: 0x00031F10
	public void AddString(string user, string text, Talker.Type type, bool timestamp = false)
	{
		Color color = Color.white;
		if (type != Talker.Type.Whisper)
		{
			if (type == Talker.Type.Shout)
			{
				color = Color.yellow;
				text = text.ToUpper();
			}
			else
			{
				color = Color.white;
			}
		}
		else
		{
			color = new Color(1f, 1f, 1f, 0.75f);
			text = text.ToLowerInvariant();
		}
		string text2 = timestamp ? ("[" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "] ") : "";
		text2 = string.Concat(new string[]
		{
			text2,
			"<color=orange>",
			user,
			"</color>: <color=#",
			ColorUtility.ToHtmlStringRGBA(color),
			">",
			text,
			"</color>"
		});
		this.AddString(text2);
	}

	// Token: 0x0600069E RID: 1694 RVA: 0x00033DDC File Offset: 0x00031FDC
	public void AddString(string text)
	{
		while (this.m_maxVisibleBufferLength > 1)
		{
			try
			{
				this.m_chatBuffer.Add(text);
				while (this.m_chatBuffer.Count > 300)
				{
					this.m_chatBuffer.RemoveAt(0);
				}
				this.UpdateChat();
				break;
			}
			catch (Exception)
			{
				this.m_maxVisibleBufferLength--;
			}
		}
	}

	// Token: 0x0600069F RID: 1695 RVA: 0x00033E4C File Offset: 0x0003204C
	private void UpdateChat()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = Mathf.Min(this.m_chatBuffer.Count, Mathf.Max(5, this.m_chatBuffer.Count - this.m_scrollHeight));
		for (int i = Mathf.Max(0, num - this.m_maxVisibleBufferLength); i < num; i++)
		{
			stringBuilder.Append(this.m_chatBuffer[i]);
			stringBuilder.Append("\n");
		}
		this.m_output.text = stringBuilder.ToString();
	}

	// Token: 0x060006A0 RID: 1696 RVA: 0x00033ED4 File Offset: 0x000320D4
	public static float GetTestValue(string key, float defaultIfMissing = 0f)
	{
		string s;
		float result;
		if (Terminal.m_testList.TryGetValue(key, out s) && float.TryParse(s, out result))
		{
			return result;
		}
		return defaultIfMissing;
	}

	// Token: 0x060006A1 RID: 1697 RVA: 0x00033F00 File Offset: 0x00032100
	private void tabCycle(string word, List<string> options, bool usePrefix)
	{
		if (options == null || options.Count == 0)
		{
			return;
		}
		usePrefix = (usePrefix && this.m_tabPrefix > '\0');
		if (usePrefix)
		{
			if (word.Length < 1 || word[0] != this.m_tabPrefix)
			{
				return;
			}
			word = word.Substring(1);
		}
		if (this.m_tabCaretPosition == -1)
		{
			this.m_tabOptions.Clear();
			this.m_tabCaretPosition = this.m_input.caretPosition;
			word = word.ToLower();
			this.m_tabLength = word.Length;
			if (this.m_tabLength == 0)
			{
				this.m_tabOptions.AddRange(options);
			}
			else
			{
				foreach (string text in options)
				{
					if (text.Length > this.m_tabLength && this.safeSubstring(text, 0, this.m_tabLength).ToLower() == word)
					{
						this.m_tabOptions.Add(text);
					}
				}
			}
			this.m_tabOptions.Sort();
			this.m_tabIndex = -1;
		}
		if (this.m_tabOptions.Count == 0)
		{
			this.m_tabOptions.AddRange(this.m_lastSearch);
		}
		if (this.m_tabOptions.Count == 0)
		{
			return;
		}
		int num = this.m_tabIndex + 1;
		this.m_tabIndex = num;
		if (num >= this.m_tabOptions.Count)
		{
			this.m_tabIndex = 0;
		}
		if (this.m_tabCaretPosition - this.m_tabLength >= 0)
		{
			this.m_input.text = this.safeSubstring(this.m_input.text, 0, this.m_tabCaretPosition - this.m_tabLength) + this.m_tabOptions[this.m_tabIndex];
		}
		this.m_tabCaretPositionEnd = (this.m_input.caretPosition = this.m_input.text.Length);
	}

	// Token: 0x060006A2 RID: 1698 RVA: 0x000340E8 File Offset: 0x000322E8
	private void updateSearch(string word, List<string> options, bool usePrefix)
	{
		if (this.m_search == null)
		{
			return;
		}
		this.m_search.text = "";
		if (options == null || options.Count == 0)
		{
			return;
		}
		usePrefix = (usePrefix && this.m_tabPrefix > '\0');
		if (usePrefix)
		{
			if (word.Length < 1 || word[0] != this.m_tabPrefix)
			{
				return;
			}
			word = word.Substring(1);
		}
		this.m_lastSearch.Clear();
		foreach (string text in options)
		{
			string text2 = text.ToLower();
			if (text2.Contains(word.ToLower()) && (word.Contains("fx") || !text2.Contains("fx")))
			{
				this.m_lastSearch.Add(text);
			}
		}
		int num = 10;
		for (int i = 0; i < Math.Min(this.m_lastSearch.Count, num); i++)
		{
			string text3 = this.m_lastSearch[i];
			int num2 = text3.ToLower().IndexOf(word.ToLower());
			Text search = this.m_search;
			search.text += this.safeSubstring(text3, 0, num2);
			Text search2 = this.m_search;
			search2.text = search2.text + "<color=white>" + this.safeSubstring(text3, num2, word.Length) + "</color>";
			Text search3 = this.m_search;
			search3.text = search3.text + this.safeSubstring(text3, num2 + word.Length, -1) + " ";
		}
		if (this.m_lastSearch.Count > num)
		{
			Text search4 = this.m_search;
			search4.text += string.Format("... {0} more.", this.m_lastSearch.Count - num);
		}
	}

	// Token: 0x060006A3 RID: 1699 RVA: 0x000342E0 File Offset: 0x000324E0
	private string safeSubstring(string text, int start, int length = -1)
	{
		if (text.Length == 0)
		{
			return text;
		}
		if (start < 0)
		{
			start = 0;
		}
		if (start + length >= text.Length)
		{
			length = text.Length - start;
		}
		if (length >= 0)
		{
			return text.Substring(start, length);
		}
		return text.Substring(start);
	}

	// Token: 0x060006A4 RID: 1700 RVA: 0x0003431C File Offset: 0x0003251C
	public static float TryTestFloat(string key, float defaultValue = 1f)
	{
		string s;
		float result;
		if (Terminal.m_testList.TryGetValue(key, out s) && float.TryParse(s, out result))
		{
			return result;
		}
		return defaultValue;
	}

	// Token: 0x060006A5 RID: 1701 RVA: 0x00034348 File Offset: 0x00032548
	public static int TryTestInt(string key, int defaultValue = 1)
	{
		string s;
		int result;
		if (Terminal.m_testList.TryGetValue(key, out s) && int.TryParse(s, out result))
		{
			return result;
		}
		return defaultValue;
	}

	// Token: 0x060006A6 RID: 1702 RVA: 0x00034374 File Offset: 0x00032574
	public static string TryTest(string key, string defaultValue = "")
	{
		string result;
		if (Terminal.m_testList.TryGetValue(key, out result))
		{
			return result;
		}
		return defaultValue;
	}

	// Token: 0x060006A7 RID: 1703 RVA: 0x00034393 File Offset: 0x00032593
	public static void Log(object obj)
	{
		if (Terminal.m_showTests)
		{
			ZLog.Log(obj);
			if (global::Console.instance)
			{
				global::Console.instance.AddString("Log", obj.ToString(), Talker.Type.Whisper, true);
			}
		}
	}

	// Token: 0x060006A8 RID: 1704 RVA: 0x000343C5 File Offset: 0x000325C5
	public static void LogWarning(object obj)
	{
		if (Terminal.m_showTests)
		{
			ZLog.LogWarning(obj);
			if (global::Console.instance)
			{
				global::Console.instance.AddString("Warning", obj.ToString(), Talker.Type.Whisper, true);
			}
		}
	}

	// Token: 0x060006A9 RID: 1705 RVA: 0x000343F7 File Offset: 0x000325F7
	public static void LogError(object obj)
	{
		if (Terminal.m_showTests)
		{
			ZLog.LogError(obj);
			if (global::Console.instance)
			{
				global::Console.instance.AddString("Warning", obj.ToString(), Talker.Type.Whisper, true);
			}
		}
	}

	// Token: 0x060006AA RID: 1706 RVA: 0x00034429 File Offset: 0x00032629
	protected bool TryShowGamepadTextInput()
	{
		return this.m_gamepadTextInput.TryOpenTextInput(63, Localization.instance.Localize("$chat_entermessage"), "");
	}

	// Token: 0x060006AB RID: 1707 RVA: 0x0003444C File Offset: 0x0003264C
	protected virtual void onGamePadTextInput(TextInputEventArgs args)
	{
		this.m_input.text = args.m_text;
		this.m_input.caretPosition = this.m_input.text.Length;
	}

	// Token: 0x17000026 RID: 38
	// (get) Token: 0x060006AC RID: 1708
	protected abstract Terminal m_terminalInstance { get; }

	// Token: 0x060006AF RID: 1711 RVA: 0x0003451C File Offset: 0x0003271C
	[CompilerGenerated]
	internal static void <InitTerminal>g__GetInfo|7_108(IEnumerable collection, ref Terminal.<>c__DisplayClass7_0 A_1)
	{
		foreach (object obj in collection)
		{
			Character character = obj as Character;
			if (character != null)
			{
				Terminal.<InitTerminal>g__count|7_109(character.m_name, character.GetLevel(), 1, ref A_1);
			}
			else if (obj is RandomFlyingBird)
			{
				Terminal.<InitTerminal>g__count|7_109("Bird", 1, 1, ref A_1);
			}
			else
			{
				Fish fish = obj as Fish;
				if (fish != null)
				{
					ItemDrop component = fish.GetComponent<ItemDrop>();
					if (component != null)
					{
						Terminal.<InitTerminal>g__count|7_109(component.m_itemData.m_shared.m_name, component.m_itemData.m_quality, component.m_itemData.m_stack, ref A_1);
					}
				}
			}
		}
		foreach (object obj2 in collection)
		{
			MonoBehaviour monoBehaviour = obj2 as MonoBehaviour;
			if (monoBehaviour != null)
			{
				A_1.args.Context.AddString(string.Format("   {0}, Dist: {1}, Offset: {2}", monoBehaviour.name, Vector3.Distance(Player.m_localPlayer.transform.position, monoBehaviour.transform.position).ToString("0.0"), monoBehaviour.transform.position - Player.m_localPlayer.transform.position));
			}
		}
	}

	// Token: 0x060006B0 RID: 1712 RVA: 0x000346A8 File Offset: 0x000328A8
	[CompilerGenerated]
	internal static void <InitTerminal>g__count|7_109(string key, int level, int increment, ref Terminal.<>c__DisplayClass7_0 A_3)
	{
		Dictionary<int, int> dictionary;
		if (!A_3.counts.TryGetValue(key, out dictionary))
		{
			dictionary = (A_3.counts[key] = new Dictionary<int, int>());
		}
		int num;
		if (dictionary.TryGetValue(level, out num))
		{
			dictionary[level] = num + increment;
			return;
		}
		dictionary[level] = increment;
	}

	// Token: 0x060006B1 RID: 1713 RVA: 0x000346F8 File Offset: 0x000328F8
	[CompilerGenerated]
	internal static void <InitTerminal>g__add|7_110(string key, ref Terminal.<>c__DisplayClass7_1 A_1)
	{
		int total = A_1.total;
		A_1.total = total + 1;
		int num;
		if (A_1.counts.TryGetValue(key, out num))
		{
			A_1.counts[key] = num + 1;
			return;
		}
		A_1.counts[key] = 1;
	}

	// Token: 0x060006B2 RID: 1714 RVA: 0x00034744 File Offset: 0x00032944
	[CompilerGenerated]
	internal static void <InitTerminal>g__spawn|7_112(string name, ref Terminal.<>c__DisplayClass7_2 A_1)
	{
		GameObject prefab = ZNetScene.instance.GetPrefab(name);
		if (!prefab)
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Missing object " + name, 0, null);
			return;
		}
		for (int i = 0; i < A_1.count; i++)
		{
			Vector3 b = UnityEngine.Random.insideUnitSphere * ((A_1.count == 1) ? 0f : 0.5f);
			Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Spawning object " + name, 0, null);
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 2f + Vector3.up + b, Quaternion.identity);
			ItemDrop component = gameObject.GetComponent<ItemDrop>();
			if (A_1.level > 1)
			{
				if (component)
				{
					A_1.level = Mathf.Min(A_1.level, 4);
				}
				else
				{
					A_1.level = Mathf.Min(A_1.level, 9);
				}
				Character component2 = gameObject.GetComponent<Character>();
				if (component2 != null)
				{
					component2.SetLevel(A_1.level);
				}
				if (A_1.level > 4)
				{
					A_1.level = 4;
				}
				if (component)
				{
					component.SetQuality(A_1.level);
				}
			}
			if (A_1.pickup | A_1.use | A_1.onlyIfMissing)
			{
				if (A_1.onlyIfMissing && component && Player.m_localPlayer.GetInventory().HaveItem(component.m_itemData.m_shared.m_name))
				{
					ZNetView component3 = gameObject.GetComponent<ZNetView>();
					if (component3 != null)
					{
						component3.Destroy();
						goto IL_1D1;
					}
				}
				if ((Player.m_localPlayer.Pickup(gameObject, false, false) & A_1.use) && component)
				{
					Player.m_localPlayer.UseItem(Player.m_localPlayer.GetInventory(), component.m_itemData, false);
				}
			}
			IL_1D1:;
		}
	}

	// Token: 0x0400081A RID: 2074
	private static bool m_terminalInitialized;

	// Token: 0x0400081B RID: 2075
	protected static List<string> m_bindList;

	// Token: 0x0400081C RID: 2076
	public static Dictionary<string, string> m_testList = new Dictionary<string, string>();

	// Token: 0x0400081D RID: 2077
	protected static Dictionary<KeyCode, List<string>> m_binds = new Dictionary<KeyCode, List<string>>();

	// Token: 0x0400081E RID: 2078
	private static bool m_cheat = false;

	// Token: 0x0400081F RID: 2079
	public static bool m_showTests;

	// Token: 0x04000820 RID: 2080
	protected float m_lastDebugUpdate;

	// Token: 0x04000821 RID: 2081
	protected static Dictionary<string, Terminal.ConsoleCommand> commands = new Dictionary<string, Terminal.ConsoleCommand>();

	// Token: 0x04000822 RID: 2082
	public static ConcurrentQueue<string> m_threadSafeMessages = new ConcurrentQueue<string>();

	// Token: 0x04000823 RID: 2083
	public static ConcurrentQueue<string> m_threadSafeConsoleLog = new ConcurrentQueue<string>();

	// Token: 0x04000824 RID: 2084
	protected char m_tabPrefix;

	// Token: 0x04000825 RID: 2085
	protected bool m_autoCompleteSecrets;

	// Token: 0x04000826 RID: 2086
	private List<string> m_history = new List<string>();

	// Token: 0x04000827 RID: 2087
	private List<string> m_tabOptions = new List<string>();

	// Token: 0x04000828 RID: 2088
	private int m_historyPosition;

	// Token: 0x04000829 RID: 2089
	private int m_tabCaretPosition = -1;

	// Token: 0x0400082A RID: 2090
	private int m_tabCaretPositionEnd;

	// Token: 0x0400082B RID: 2091
	private int m_tabLength;

	// Token: 0x0400082C RID: 2092
	private int m_tabIndex;

	// Token: 0x0400082D RID: 2093
	private List<string> m_commandList = new List<string>();

	// Token: 0x0400082E RID: 2094
	private List<Minimap.PinData> m_findPins = new List<Minimap.PinData>();

	// Token: 0x0400082F RID: 2095
	protected TextInputHandler m_gamepadTextInput;

	// Token: 0x04000830 RID: 2096
	protected bool m_focused;

	// Token: 0x04000831 RID: 2097
	public RectTransform m_chatWindow;

	// Token: 0x04000832 RID: 2098
	public TextMeshProUGUI m_output;

	// Token: 0x04000833 RID: 2099
	public InputField m_input;

	// Token: 0x04000834 RID: 2100
	public Text m_search;

	// Token: 0x04000835 RID: 2101
	private int m_lastSearchLength;

	// Token: 0x04000836 RID: 2102
	private List<string> m_lastSearch = new List<string>();

	// Token: 0x04000837 RID: 2103
	protected List<string> m_chatBuffer = new List<string>();

	// Token: 0x04000838 RID: 2104
	protected const int m_maxBufferLength = 300;

	// Token: 0x04000839 RID: 2105
	public int m_maxVisibleBufferLength = 30;

	// Token: 0x0400083A RID: 2106
	private const int m_maxScrollHeight = 5;

	// Token: 0x0400083B RID: 2107
	private int m_scrollHeight;

	// Token: 0x0200009B RID: 155
	public class ConsoleEventArgs
	{
		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060006B3 RID: 1715 RVA: 0x00034932 File Offset: 0x00032B32
		public int Length
		{
			get
			{
				return this.Args.Length;
			}
		}

		// Token: 0x17000028 RID: 40
		public string this[int i]
		{
			get
			{
				return this.Args[i];
			}
		}

		// Token: 0x060006B5 RID: 1717 RVA: 0x00034946 File Offset: 0x00032B46
		public ConsoleEventArgs(string line, Terminal context)
		{
			this.Context = context;
			this.FullLine = line;
			this.Args = line.Split(new char[]
			{
				' '
			});
		}

		// Token: 0x060006B6 RID: 1718 RVA: 0x00034974 File Offset: 0x00032B74
		public int TryParameterInt(int parameterIndex, int defaultValue = 1)
		{
			int result;
			if (this.TryParameterInt(parameterIndex, out result))
			{
				return result;
			}
			return defaultValue;
		}

		// Token: 0x060006B7 RID: 1719 RVA: 0x0003498F File Offset: 0x00032B8F
		public bool TryParameterInt(int parameterIndex, out int value)
		{
			if (this.Args.Length <= parameterIndex || !int.TryParse(this.Args[parameterIndex], out value))
			{
				value = 0;
				return false;
			}
			return true;
		}

		// Token: 0x060006B8 RID: 1720 RVA: 0x000349B4 File Offset: 0x00032BB4
		public float TryParameterFloat(int parameterIndex, float defaultValue = 1f)
		{
			float result;
			if (this.TryParameterFloat(parameterIndex, out result))
			{
				return result;
			}
			return defaultValue;
		}

		// Token: 0x060006B9 RID: 1721 RVA: 0x000349CF File Offset: 0x00032BCF
		public bool TryParameterFloat(int parameterIndex, out float value)
		{
			if (this.Args.Length <= parameterIndex || !float.TryParse(this.Args[parameterIndex].Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
			{
				value = 0f;
				return false;
			}
			return true;
		}

		// Token: 0x060006BA RID: 1722 RVA: 0x00034A10 File Offset: 0x00032C10
		public bool HasArgumentAnywhere(string value, int firstIndexToCheck = 0, bool toLower = true)
		{
			for (int i = firstIndexToCheck; i < this.Args.Length; i++)
			{
				if ((toLower && this.Args[i].ToLower() == value) || (!toLower && this.Args[i] == value))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0400083C RID: 2108
		public string[] Args;

		// Token: 0x0400083D RID: 2109
		public string FullLine;

		// Token: 0x0400083E RID: 2110
		public Terminal Context;
	}

	// Token: 0x0200009C RID: 156
	public class ConsoleCommand
	{
		// Token: 0x060006BB RID: 1723 RVA: 0x00034A60 File Offset: 0x00032C60
		public ConsoleCommand(string command, string description, Terminal.ConsoleEventFailable action, bool isCheat = false, bool isNetwork = false, bool onlyServer = false, bool isSecret = false, bool allowInDevBuild = false, Terminal.ConsoleOptionsFetcher optionsFetcher = null)
		{
			Terminal.commands[command.ToLower()] = this;
			this.Command = command;
			this.Description = description;
			this.actionFailable = action;
			this.IsCheat = isCheat;
			this.OnlyServer = onlyServer;
			this.IsSecret = isSecret;
			this.IsNetwork = isNetwork;
			this.AllowInDevBuild = allowInDevBuild;
			this.m_tabOptionsFetcher = optionsFetcher;
		}

		// Token: 0x060006BC RID: 1724 RVA: 0x00034ACC File Offset: 0x00032CCC
		public ConsoleCommand(string command, string description, Terminal.ConsoleEvent action, bool isCheat = false, bool isNetwork = false, bool onlyServer = false, bool isSecret = false, bool allowInDevBuild = false, Terminal.ConsoleOptionsFetcher optionsFetcher = null)
		{
			Terminal.commands[command.ToLower()] = this;
			this.Command = command;
			this.Description = description;
			this.action = action;
			this.IsCheat = isCheat;
			this.OnlyServer = onlyServer;
			this.IsSecret = isSecret;
			this.IsNetwork = isNetwork;
			this.AllowInDevBuild = allowInDevBuild;
			this.m_tabOptionsFetcher = optionsFetcher;
		}

		// Token: 0x060006BD RID: 1725 RVA: 0x00034B35 File Offset: 0x00032D35
		public List<string> GetTabOptions()
		{
			if (this.m_tabOptions == null && this.m_tabOptionsFetcher != null)
			{
				this.m_tabOptions = this.m_tabOptionsFetcher();
			}
			return this.m_tabOptions;
		}

		// Token: 0x060006BE RID: 1726 RVA: 0x00034B60 File Offset: 0x00032D60
		public void RunAction(Terminal.ConsoleEventArgs args)
		{
			if (args.Length >= 2)
			{
				List<string> tabOptions = this.GetTabOptions();
				if (tabOptions != null)
				{
					foreach (string text in tabOptions)
					{
						if (args[1].ToLower() == text.ToLower())
						{
							args.Args[1] = text;
							break;
						}
					}
				}
			}
			if (this.action != null)
			{
				this.action(args);
				return;
			}
			object obj = this.actionFailable(args);
			if (obj is bool && !(bool)obj)
			{
				args.Context.AddString(string.Concat(new string[]
				{
					"<color=#8B0000>Error executing command. Check parameters and context.</color>\n   <color=grey>",
					this.Command,
					" - ",
					this.Description,
					"</color>"
				}));
			}
			string text2 = obj as string;
			if (text2 != null)
			{
				args.Context.AddString(string.Concat(new string[]
				{
					"<color=#8B0000>Error executing command: ",
					text2,
					"</color>\n   <color=grey>",
					this.Command,
					" - ",
					this.Description,
					"</color>"
				}));
			}
		}

		// Token: 0x060006BF RID: 1727 RVA: 0x00034CB0 File Offset: 0x00032EB0
		public bool IsValid(Terminal context, bool skipAllowedCheck = false)
		{
			return (!this.IsCheat || context.IsCheatsEnabled()) && (context.isAllowedCommand(this) || skipAllowedCheck) && (!this.IsNetwork || ZNet.instance) && (!this.OnlyServer || (ZNet.instance && ZNet.instance.IsServer() && Player.m_localPlayer));
		}

		// Token: 0x0400083F RID: 2111
		public string Command;

		// Token: 0x04000840 RID: 2112
		public string Description;

		// Token: 0x04000841 RID: 2113
		public bool IsCheat;

		// Token: 0x04000842 RID: 2114
		public bool IsNetwork;

		// Token: 0x04000843 RID: 2115
		public bool OnlyServer;

		// Token: 0x04000844 RID: 2116
		public bool IsSecret;

		// Token: 0x04000845 RID: 2117
		public bool AllowInDevBuild;

		// Token: 0x04000846 RID: 2118
		private Terminal.ConsoleEventFailable actionFailable;

		// Token: 0x04000847 RID: 2119
		private Terminal.ConsoleEvent action;

		// Token: 0x04000848 RID: 2120
		private Terminal.ConsoleOptionsFetcher m_tabOptionsFetcher;

		// Token: 0x04000849 RID: 2121
		private List<string> m_tabOptions;
	}

	// Token: 0x0200009D RID: 157
	// (Invoke) Token: 0x060006C1 RID: 1729
	public delegate object ConsoleEventFailable(Terminal.ConsoleEventArgs args);

	// Token: 0x0200009E RID: 158
	// (Invoke) Token: 0x060006C5 RID: 1733
	public delegate void ConsoleEvent(Terminal.ConsoleEventArgs args);

	// Token: 0x0200009F RID: 159
	// (Invoke) Token: 0x060006C9 RID: 1737
	public delegate List<string> ConsoleOptionsFetcher();
}
