using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ChickensToNuggets.Scripts
{
	[HarmonyPatch(typeof(Thing))]
	public static class ThingPatch
    {
		[HarmonyPatch("AttackWith")]
		[HarmonyReversePatch]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Thing.DelayedActionInstance AttackWith(Thing __instance, Attack attack, bool doAction = true)
		{
			DynamicThing sourceItem = attack.SourceItem;
			bool flag = !sourceItem;
			Thing.DelayedActionInstance result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = sourceItem is AuthoringTool;
				if (flag2)
				{
					Thing.DelayedActionInstance delayedActionInstance = new Thing.DelayedActionInstance
					{
						Duration = 0.5f,
						ActionMessage = "Author"
					};
					bool isCopy = attack.IsCopy;
					if (isCopy)
					{
						delayedActionInstance = new Thing.DelayedActionInstance
						{
							Duration = 0.5f,
							ActionMessage = "Copy"
						};
						if (doAction)
						{
							int spawnItemIndex = InventoryManager.DynamicThingPrefabs.IndexOf(__instance.name);
							InventoryManager.Instance.SetSpawnItemIndex(spawnItemIndex);
						}
					}
					bool isDestroy = attack.IsDestroy;
					if (isDestroy)
					{
						Human human = __instance as Human;
						bool flag3 = human && human.OrganBrain;
						if (flag3)
						{
							return null;
						}
						delayedActionInstance = new Thing.DelayedActionInstance
						{
							Duration = 0.5f,
							ActionMessage = "Delete"
						};
						if (doAction)
						{
							__instance.Delete(attack.SourceItem);
						}
					}
					__instance.AddReferenceIDToContextualMessage(ref delayedActionInstance);
					result = delayedActionInstance;
				}
				else
				{
					SprayCan sprayCan = sourceItem as SprayCan;
					bool flag4 = __instance.PaintableMaterial != null && sprayCan != null && !__instance.HasColorState;
					if (flag4)
					{
						bool flag5 = (__instance.CustomColor.Normal != null && __instance.CustomColor.Normal != sprayCan.PaintMaterial) || (__instance.CustomColor.Normal == null && __instance.PaintableMaterial != sprayCan.PaintMaterial);
						if (flag5)
						{
							Thing.DelayedActionInstance delayedActionInstance2 = new Thing.DelayedActionInstance
							{
								Duration = 0.5f,
								ActionMessage = ActionStrings.Paint
							};
							ColorSwatch colorSwatch = GameManager.GetColorSwatch(sprayCan.PaintMaterial);
							bool flag6 = sprayCan.Quantity < 0.04f;
							if (flag6)
							{
								delayedActionInstance2.IsDisabled = true;
								delayedActionInstance2.StateMessage = "Not enough paint left to spraypaint";
								result = delayedActionInstance2;
							}
							else
							{
								delayedActionInstance2.ExtendedMessage = string.Format("The {0} will be painted {1}", __instance.ToTooltip(), colorSwatch.ToTooltip());
								bool flag7 = !doAction;
								if (flag7)
								{
									result = delayedActionInstance2;
								}
								else
								{
									bool flag8 = __instance.isServer && !sprayCan.OnUseItem(sprayCan.UseAmount, __instance);
									if (flag8)
									{
										result = delayedActionInstance2;
									}
									else
									{
										bool isServer = GameManager.IsServer;
										if (isServer)
										{
											OnServer.SetCustomColor(__instance, colorSwatch.Index);
										}
										result = delayedActionInstance2;
									}
								}
							}
						}
						else
						{
							result = null;
						}
					}
					else
					{
						FireExtinguisher fireExtinguisher = attack.SourceItem as FireExtinguisher;
						bool flag9 = fireExtinguisher;
						if (flag9)
						{
							float extinguishDuration = -1f;
							try
                            {
								extinguishDuration = (float)typeof(Thing).GetField("ExtinguishDuration", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
							}
							catch (Exception ex)
                            {
								Debug.LogError(ModReference.Name + ": Unable to get ExtinguishDuration of Thing via reflection: " + ex.Message);
							}
							
							Thing.DelayedActionInstance delayedActionInstance3 = new Thing.DelayedActionInstance
							{
								Duration = extinguishDuration,
								ActionMessage = "Extinguish Item"
							};
							bool flag10 = !doAction;
							if (flag10)
							{
								result = delayedActionInstance3;
							}
							else
							{
								__instance.NetworkIsBurning = false;
								fireExtinguisher.ExtinguishAtmos(__instance.Position);
								result = null;
							}
						}
						else
						{
							Tablet tablet = attack.SourceItem as Tablet;
							bool flag11 = tablet && tablet.Cartridge && !string.IsNullOrEmpty(tablet.Cartridge.ScanActionText);
							if (flag11)
							{
								Thing.DelayedActionInstance result2 = new Thing.DelayedActionInstance
								{
									Duration = tablet.ActionTime,
									ActionMessage = tablet.Cartridge.ScanActionText
								};
								bool flag12 = !doAction;
								if (flag12)
								{
									return result2;
								}
								tablet.Scan(__instance);
							}
							result = null;
						}
					}
				}
			}
			return result;
		}

	}
}
