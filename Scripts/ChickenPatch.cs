using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Vehicles;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ChickensToNuggets.Scripts
{
	[HarmonyPatch(typeof(Chicken))]
	public static class ChickenPatch
	{
		[HarmonyPatch("AttackWith")]
		[HarmonyPrefix]
		public static bool AttackWithPrefix(Chicken __instance, ref Thing.DelayedActionInstance __result, Attack attack, bool doAction = true)
		{
			AngleGrinder grinder = attack.SourceItem as AngleGrinder;
			bool isChickenButcher = grinder != null;
			if (isChickenButcher)
			{
				Thing.DelayedActionInstance delayedActionInstance = new Thing.DelayedActionInstance
				{
					Duration = 2f,
					ActionMessage = "Butcher"
				};
				bool flag3 = !doAction;
				if (flag3)
				{
					__result = delayedActionInstance;
					return false; // skip original method
				}
				bool isServer = GameManager.IsServer;
				if (isServer)
				{
					BakedPotato chickenNugget = Thing.AllPrefabs.Find(x => x.PrefabName == "ItemPotatoBaked") as BakedPotato;
					DynamicThing dynamicThing = Thing.Create<DynamicThing>(chickenNugget, __instance.Position, __instance.Rotation);
					NetworkManagerOverride.HandleSpawn(dynamicThing.gameObject);
					OnServer.MoveToWorld(dynamicThing);
					InventoryManager.Parent.CallCmdRenameThing(dynamicThing.netId, "Chicken Nugget");
					NetworkServer.Destroy(__instance.gameObject);

					__result = DynamicThingPatch.AttackWith(__instance, attack, doAction);
					return false; // skip original method
				}
				__result = DynamicThingPatch.AttackWith(__instance, attack, doAction);
				return false; // skip original method
			}

			DynamicThing sourceItem = attack.SourceItem;
			bool flag = !sourceItem;
			Thing.DelayedActionInstance result;
			if (flag)
			{
				result = null;
			}
			else
			{
				Thing.DelayedActionInstance delayedActionInstance = new Thing.DelayedActionInstance
				{
					Duration = 0f,
					ActionMessage = ActionStrings.Rename
				};
				Labeller labeller = sourceItem as Labeller;
				bool flag2 = labeller;
				if (flag2)
				{
					bool flag3 = !labeller.OnOff;
					if (flag3)
					{
						result = delayedActionInstance.Fail(HelpTextDevice.DeviceNotOn);
					}
					else
					{
						bool flag4 = !labeller.IsOperable;
						if (flag4)
						{
							result = delayedActionInstance.Fail(HelpTextDevice.DeviceNoPower);
						}
						else
						{
							bool flag5 = !doAction;
							if (flag5)
							{
								result = delayedActionInstance;
							}
							else
							{
								labeller.Rename(__instance);
								result = delayedActionInstance;
							}
						}
					}
				}
				else
				{
					result = DynamicThingPatch.AttackWith(__instance, attack, doAction);
				}
			}
			__result = result;

			return false; // skip original method
		}

	}
}
