using ChickensToNuggets.Scripts;
using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace ChickensToNuggets.Scripts
{
	[HarmonyPatch(typeof(DynamicThing))]
	public static class DynamicThingPatch
    {
		[HarmonyPatch("AttackWith")]
		[HarmonyReversePatch]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Thing.DelayedActionInstance AttackWith(DynamicThing __instance, Attack attack, bool doAction = true)
		{
			DynamicThing sourceItem = attack.SourceItem;
			bool flag = !sourceItem || !__instance.CanDeconstruct();
			Thing.DelayedActionInstance result;
			if (flag)
			{
				result = ThingPatch.AttackWith(__instance, attack, doAction);
			}
			else
			{
				Thing.DelayedActionInstance delayedActionInstance = new Thing.DelayedActionInstance
				{
					Duration = __instance.ExitTool.ExitTime,
					ActionMessage = ActionStrings.Deconstruct
				};
				Tool tool = sourceItem as Tool;
				bool flag2 = tool && __instance.ExitTool.ToolExit && __instance.ExitTool.IsToolExit(tool);
				if (flag2)
				{
					bool flag3 = tool.ReplacementTool != null;
					if (flag3)
					{
						delayedActionInstance.Duration *= tool.ReplacementTool.ReplacementSpeed;
					}
					bool flag4 = tool && !tool.IsOperable;
					if (flag4)
					{
						delayedActionInstance.IsDisabled = true;
						PowerTool powerTool = tool as PowerTool;
						bool flag5 = powerTool && !powerTool.Battery;
						if (flag5)
						{
							return delayedActionInstance.Fail(string.Format("{0} does not have anything in its {1} slot.", tool.ToTooltip(), powerTool.BatterySlot.ToTooltip()));
						}
						bool flag6 = powerTool && powerTool.Battery && powerTool.Battery.IsEmpty;
						if (flag6)
						{
							return delayedActionInstance.Fail(string.Format("{0} does not have enough {1} charge.", tool.ToTooltip(), powerTool.Battery.ToTooltip()));
						}
						delayedActionInstance.StateMessage = tool.ToTooltip() + " cannot complete this task.";
						bool flag7 = !doAction;
						if (flag7)
						{
							return delayedActionInstance;
						}
						return null;
					}
					else
					{
						bool flag8 = !doAction;
						if (flag8)
						{
							return delayedActionInstance;
						}
						bool isServer = GameManager.IsServer;
						if (isServer)
						{
							ConstructionEventInstance eventInstance = new ConstructionEventInstance
							{
								Parent = __instance,
								Position = attack.Position,
								Rotation = __instance.ThingTransform.rotation,
								SteamId = __instance.OwnerSteamId,
								OtherHandSlot = attack.OtherHand,
								MothershipRigidbody = null
							};
							foreach (Slot slot in __instance.Slots)
							{
								bool flag9 = slot.Occupant;
								if (flag9)
								{
									slot.PlayerMoveToWorld();
								}
							}
							__instance.ExitTool.Deconstruct(eventInstance);
							NetworkServer.Destroy(__instance.gameObject);
							return delayedActionInstance;
						}
						bool flag10 = tool && !tool.OnUseItem((float)__instance.ExitTool.ExitQuantity, __instance);
						if (flag10)
						{
							return null;
						}
					}
				}
				result = ThingPatch.AttackWith(__instance, attack, doAction);
			}
			return result;
		}

	}
}
