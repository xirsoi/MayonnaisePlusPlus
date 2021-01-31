using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Events;
using StardewValley.Network;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace MayonnaisePlusPlus
{
	public class ObjectOverrides
	{
		public static int CalculateQualityLevel(Farmer who, int sourceQuality = 0, bool bonus = false) {
			int quality = sourceQuality;
			double threshold = Loader.CONFIG.QualityThreshold;

			if (who.professions.Contains(4)) threshold += 0.2;
			if (bonus) threshold += 0.1;

			var random = new Random((int) Game1.stats.DaysPlayed);
			if (random.NextDouble() > threshold) quality /= 2;

			return quality;
		}

		public static bool PerformObjectDropInAction(ref SObject __instance, ref Item dropInItem, ref bool probe, ref Farmer who, ref bool __result) {
			ModEntry.MOD_MONITOR.Log("Entering PerformDropInAction...", StardewModdingAPI.LogLevel.Trace);
			__result = false;
			if (__instance.isTemporarilyInvisible || !(dropInItem is SObject))
				return false;
			SObject object1 = dropInItem as SObject;
			if (__instance.IsSprinkler() && __instance.heldObject.Value == null && (Utility.IsNormalObjectAtParentSheetIndex(dropInItem, 915) || Utility.IsNormalObjectAtParentSheetIndex(dropInItem, 913)))
				return true;
			if (object1 != null && object1.ParentSheetIndex == 872 && SObject.autoLoadChest == null)
				return true;
			if (dropInItem is Wallpaper
				|| __instance.heldObject.Value != null && !__instance.name.Equals("Recycling Machine") && !__instance.name.Equals("Crystalarium")
				|| object1 != null && object1.bigCraftable)
				return false;
			if (__instance.bigCraftable && !probe && object1 != null && __instance.heldObject.Value == null)
				__instance.scale.X = 5f;
			if (probe && __instance.MinutesUntilReady > 0)
				return false;
			if (__instance.name.Equals("Mayonnaise Machine")) {
				switch (object1.ParentSheetIndex) {
					case 107: // dinosaur egg!
										// only accept fertile eggs if the infertile eggs option is off
						if (Loader.CONFIG.InfertileEggs) {
							return false;
						}
						__instance.heldObject.Value = new SObject(Vector2.Zero, 807, null, false, true, false, false) {
							Quality = CalculateQualityLevel(who, object1.Quality)
						};
						if (!probe) {
							__instance.MinutesUntilReady = 180;
							who.currentLocation.playSound("Ship");
						}
						__result = true;
						break;
					case 174:
					case 182:
						__instance.heldObject.Value = new SObject(Vector2.Zero, 306, null, false, true, false, false) {
							Quality = CalculateQualityLevel(who, object1.Quality, true)
						};
						if (!probe) {
							__instance.MinutesUntilReady = 180;
							who.currentLocation.playSound("Ship");
						}
						__result = true;
						break;
					case 176:
					case 180:
						__instance.heldObject.Value = new SObject(Vector2.Zero, 306, null, false, true, false, false) {
							Quality = CalculateQualityLevel(who, Math.Min(object1.Quality, 2), object1.Quality == 4)
						};
						if (!probe) {
							__instance.MinutesUntilReady = 180;
							who.currentLocation.playSound("Ship");
						}
						__result = true;
						break;
					case 305:
						__instance.heldObject.Value = new SObject(Vector2.Zero, 308, null, false, true, false, false) {
							Quality = CalculateQualityLevel(who, object1.Quality)
						};
						if (!probe) {
							__instance.MinutesUntilReady = 180;
							who.currentLocation.playSound("Ship");
						}
						__result = true;
						break;
					case 442:
						__instance.heldObject.Value = new SObject(Vector2.Zero, 307, null, false, true, false, false) {
							Quality = CalculateQualityLevel(who, object1.Quality)
						};
						if (!probe) {
							__instance.MinutesUntilReady = 180;
							who.currentLocation.playSound("Ship", NetAudio.SoundContext.Default);
						}
						__result = true;
						break;
					default:
						if (object1.ParentSheetIndex == Loader.DATA["Blue Chicken Egg"]) {
							__instance.heldObject.Value = new SObject(Vector2.Zero, Loader.DATA["Blue Mayonnaise"], null, false, true, false, false) {
								Quality = CalculateQualityLevel(who, object1.Quality)
							};
							if (!probe) {
								__instance.MinutesUntilReady = 180;
								who.currentLocation.playSound("Ship", NetAudio.SoundContext.Default);
							}
							__result = true;
						} else if (object1.ParentSheetIndex == Loader.DATA["Dino Egg"]) {
							__instance.heldObject.Value = new SObject(Vector2.Zero, 807, null, false, true, false, false) {
								Quality = CalculateQualityLevel(who, object1.Quality)
							};
							if (!probe) {
								__instance.MinutesUntilReady = 180;
								who.currentLocation.playSound("Ship", NetAudio.SoundContext.Default);
							}
							__result = true;
						}
						break;
				}
			} else if (__instance.name.Equals("Incubator")) {
				if (__instance.heldObject.Value == null && (object1.Category == -5 || Utility.IsNormalObjectAtParentSheetIndex(object1, 107)) && object1.ParentSheetIndex != Loader.DATA["Dino Egg"]) {
					__instance.heldObject.Value = new SObject(object1.ParentSheetIndex, 1, false, -1, 0);
					if (!probe) {
						who.currentLocation.playSound("coin");
						__instance.MinutesUntilReady = 9000 * object1.ParentSheetIndex == 107 ? 2 : 1;
						if (who.professions.Contains(2))
							__instance.MinutesUntilReady /= 2;
						if (object1.ParentSheetIndex == 180 || object1.ParentSheetIndex == 182 || object1.ParentSheetIndex == 305)
							__instance.ParentSheetIndex += 2;
						else
							++__instance.ParentSheetIndex;
					}
					__result = true;
				}
			}

			return !__result;
		}

		public static bool FarmAnimalDayUpdate(ref FarmAnimal __instance, GameLocation environtment) {
			ModEntry.MOD_MONITOR.Log("Entering Day Update...", StardewModdingAPI.LogLevel.Trace);
			if (__instance.daysOwned.Value < 0)
				__instance.daysOwned.Value = __instance.age.Value;
			__instance.StopAllActions();
			__instance.health.Value = 3;
			bool flag1 = false;
			if (__instance.home != null && !(__instance.home.indoors.Value as AnimalHouse).animals.ContainsKey(__instance.myID) && environtment is Farm) {
				if (!__instance.home.animalDoorOpen) {
					__instance.moodMessage.Value = 6;
					flag1 = true;
					__instance.happiness.Value /= 2;
				} else {
					(environtment as Farm).animals.Remove(__instance.myID);
					(__instance.home.indoors.Value as AnimalHouse).animals.Add(__instance.myID, __instance);
					if (Game1.timeOfDay > 1800 && __instance.controller == null)
						__instance.happiness.Value /= 2;
					environtment = __instance.home.indoors;
					__instance.setRandomPosition(environtment);
					return false;
				}
			}
			++__instance.daysSinceLastLay.Value;
			if (!__instance.wasPet.Value && !__instance.wasAutoPet.Value) {
				__instance.friendshipTowardFarmer.Value = Math.Max(0, __instance.friendshipTowardFarmer - (10 - __instance.friendshipTowardFarmer / 200));
				__instance.happiness.Value = (byte)Math.Max(0, __instance.happiness - __instance.happinessDrain * 5);
			}
			__instance.wasPet.Value = false;
			__instance.wasAutoPet.Value = false;
			++__instance.daysOwned.Value;
			if (__instance.fullness < 200 && environtment is AnimalHouse) {
				for (int index = environtment.objects.Count() - 1; index >= 0; --index) {
					OverlaidDictionary.PairsCollection pairs = environtment.objects.Pairs;
					if (pairs.ElementAt(index).Value.Name.Equals("Hay")) {
						OverlaidDictionary objects = environtment.objects;
						pairs = environtment.objects.Pairs;
						Vector2 key = pairs.ElementAt(index).Key;
						objects.Remove(key);
						__instance.fullness.Value = byte.MaxValue;
						break;
					}
				}
			}
			var random = new Random((int) (long) __instance.myID / 2 + (int) Game1.stats.DaysPlayed);
			if (__instance.fullness > 200 || random.NextDouble() < (__instance.fullness - 30) / 170.0) {
				++__instance.age.Value;
				if (__instance.age == __instance.ageWhenMature) {
					__instance.Sprite.LoadTexture("Animals\\" + __instance.type.Value);
					if (__instance.type.Value.Contains("Sheep"))
						__instance.currentProduce.Value = __instance.defaultProduceIndex;
					__instance.daysSinceLastLay.Value = 99;
				}
				__instance.happiness.Value = (byte)Math.Min(byte.MaxValue, __instance.happiness + __instance.happinessDrain * 2);
			}
			if (__instance.fullness.Value < 200) {
				__instance.happiness.Value = (byte)Math.Max(0, __instance.happiness - 100);
				__instance.friendshipTowardFarmer.Value = Math.Max(0, __instance.friendshipTowardFarmer - 20);
			}
			bool flag2 =   __instance.daysSinceLastLay >=   __instance.daysToLay - (!__instance.type.Value.Equals("Sheep") || !Game1.getFarmer( __instance.ownerID).professions.Contains(3) ? 0 : 1) && random.NextDouble() <   __instance.fullness / 200.0 && random.NextDouble() <   __instance.happiness / 70.0;
			int pse;
			if (!flag2 || __instance.age < __instance.ageWhenMature) {
				pse = -1;
			} else {
				pse = __instance.defaultProduceIndex;
				if (random.NextDouble() < __instance.happiness / 150.0) {
					float num1 =  __instance.happiness >  200 ?   __instance.happiness * 1.5f : ( __instance.happiness <=  100 ?    __instance.happiness - 100 : 0.0f);
					if (__instance.type.Value.Equals("Duck") && random.NextDouble() < (__instance.friendshipTowardFarmer + num1) / 4750.0 + Game1.player.team.AverageDailyLuck() + Game1.player.team.AverageLuckLevel() * 0.01) {
						pse = __instance.deluxeProduceIndex;
					} else if (__instance.type.Value.Equals("Rabbit") && random.NextDouble() < (__instance.friendshipTowardFarmer + num1) / 5000.0 + Game1.player.team.AverageDailyLuck() + Game1.player.team.AverageLuckLevel() * 0.02) {
						pse = __instance.deluxeProduceIndex;
					} else if (__instance.type.Value.Equals("Blue Chicken") && random.NextDouble() < (__instance.friendshipTowardFarmer + num1) / 4750.0 + Game1.player.team.AverageDailyLuck() + Game1.player.team.AverageLuckLevel() * 0.01) {
						pse = Loader.DATA["Blue Chicken Egg"];
					}

					__instance.daysSinceLastLay.Value = 0;
					switch (pse) {
						case 176:
						case 180:
							++Game1.stats.ChickenEggsLayed;
							break;
						case 440:
							++Game1.stats.RabbitWoolProduced;
							break;
						case 442:
							++Game1.stats.DuckEggsLayed;
							break;
					}
					if (random.NextDouble() < (__instance.friendshipTowardFarmer + num1) / 1200.0 && !__instance.type.Value.Equals("Duck") && !__instance.type.Value.Equals("Rabbit") && !__instance.type.Value.Equals("Blue Chicken") && __instance.deluxeProduceIndex != -1 && __instance.friendshipTowardFarmer >= 200)
						pse = __instance.deluxeProduceIndex;
					double num2 =    __instance.friendshipTowardFarmer / 1000.0 - (1.0 -    __instance.happiness / 225.0);
					if (!__instance.isCoopDweller() && Game1.getFarmer(__instance.ownerID).professions.Contains(3) || __instance.isCoopDweller() && Game1.getFarmer(__instance.ownerID).professions.Contains(2))
						num2 += 0.33;
					if (num2 >= 0.95 && random.NextDouble() < num2 / 2.0)
						__instance.produceQuality.Value = 4;
					else if (random.NextDouble() < num2 / 2.0)
						__instance.produceQuality.Value = 2;
					else if (random.NextDouble() < num2)
						__instance.produceQuality.Value = 1;
					else
						__instance.produceQuality.Value = 0;
				}
			}
			if (__instance.harvestType == 1 & flag2) {
				__instance.currentProduce.Value = pse;
				pse = -1;
			}
			if (pse != -1 && __instance.home != null) {
				bool flag3 = true;
				foreach (SObject @object in __instance.home.indoors.Value.objects.Values) {
					if (@object.bigCraftable && @object.ParentSheetIndex == 165 && @object.heldObject.Value != null) {
						if ((@object.heldObject.Value as Chest).addItem(new SObject(Vector2.Zero, pse, null, false, true, false, false) {
							Quality = __instance.produceQuality
						}) == null) {
							@object.showNextIndex.Value = true;
							flag3 = false;
							break;
						}
					}
				}
				if (flag3 && !__instance.home.indoors.Value.Objects.ContainsKey(__instance.getTileLocation()))
					__instance.home.indoors.Value.Objects.Add(__instance.getTileLocation(), new SObject(Vector2.Zero, pse, null, false, true, false, true) {
						Quality = __instance.produceQuality
					});
			}
			if (!flag1) {
				if (__instance.fullness < 30)
					__instance.moodMessage.Value = 4;
				else if (__instance.happiness < 30)
					__instance.moodMessage.Value = 3;
				else if (__instance.happiness < 200)
					__instance.moodMessage.Value = 2;
				else
					__instance.moodMessage.Value = 1;
			}
			if (Game1.timeOfDay < 1700)
				__instance.fullness.Value = (byte)Math.Max(0, __instance.fullness.Value - __instance.fullnessDrain * (1700 - Game1.timeOfDay) / 100);
			__instance.fullness.Value = 0;
			if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
				__instance.fullness.Value = 250;
			__instance.reload(__instance.home);

			return false;
		}

		public bool AnimalHouseAddNewHatchedAnimal(ref AnimalHouse __instance, string name) {
			ModEntry.MOD_MONITOR.Log("Entering AddNewHatchedAnimal...", StardewModdingAPI.LogLevel.Trace);
			bool flag = false;
			foreach (SObject @object in __instance.objects.Values) {
				if (@object.bigCraftable && @object.Name.Contains("Incubator") && (@object.heldObject.Value != null && @object.minutesUntilReady <= 0) && !__instance.isFull()) {
					flag = true;
					string type = "??";
					if (@object.heldObject.Value == null) {
						type = "White Chicken";
					} else {
						switch (@object.heldObject.Value.ParentSheetIndex) {
							case 107:
								type = "Dinosaur";
								break;
							case 174:
							case 176:
								type = "White Chicken";
								break;
							case 180:
							case 182:
								type = "Brown Chicken";
								break;
							case 289:
								type = "Ostrich";
								break;
							case 305:
								type = "Void Chicken";
								break;
							case 442:
								type = "Duck";
								break;
							case 928:
								type = "Golden Chicken";
								break;
							default:
								if (@object.heldObject.Value.ParentSheetIndex == Loader.DATA["Blue Chicken Egg"]) {
									type = "Blue Chicken";
								}
								break;
						}
					}
					FarmAnimal farmAnimal = new FarmAnimal(type, Loader.HELPER.Multiplayer.GetNewID(),  Game1.player.uniqueMultiplayerID);
					ModEntry.MOD_MONITOR.Log("Animal Created...", StardewModdingAPI.LogLevel.Trace);
					while ((Game1.player.eventsSeen.Contains(3900074) || !type.Equals("Blue Chicken")) && !farmAnimal.type.Value.Equals(type)) {
						farmAnimal = new FarmAnimal(type, farmAnimal.myID.Value, Game1.player.UniqueMultiplayerID);
					}
					ModEntry.MOD_MONITOR.Log("Animal Type Assured...", StardewModdingAPI.LogLevel.Trace);
					farmAnimal.Name = name;
					farmAnimal.displayName = name;
					Building building = __instance.getBuilding();
					farmAnimal.home = building;
					farmAnimal.homeLocation.Value = new Vector2(building.tileX, building.tileY);
					farmAnimal.setRandomPosition(farmAnimal.home.indoors);
					(building.indoors.Value as AnimalHouse).animals.Add(farmAnimal.myID, farmAnimal);
					(building.indoors.Value as AnimalHouse).animalsThatLiveHere.Add(farmAnimal.myID);
					ModEntry.MOD_MONITOR.Log("Animal Homed...", StardewModdingAPI.LogLevel.Trace);
					@object.heldObject.Value = null;
					ModEntry.MOD_MONITOR.Log("Incubator Cleared...", StardewModdingAPI.LogLevel.Trace);
					@object.ParentSheetIndex = 101;
					if (type == "Ostrich") {
						@object.ParentSheetIndex = 254;
						break;
					}
					break;
				}
			}
			if (!flag && Game1.farmEvent != null && Game1.farmEvent is QuestionEvent) {
				ModEntry.MOD_MONITOR.Log("In Question Event Block...", StardewModdingAPI.LogLevel.Trace);
				var qe = Game1.farmEvent as QuestionEvent;
				var farmAnimal = new FarmAnimal(qe.animal.type.Value, Loader.HELPER.Multiplayer.GetNewID(), Game1.player.uniqueMultiplayerID);
				farmAnimal.Name = name;
				farmAnimal.displayName = name;
				farmAnimal.parentId.Value = qe.animal.myID;
				ModEntry.MOD_MONITOR.Log("Animal Named...", StardewModdingAPI.LogLevel.Trace);
				Building building = __instance.getBuilding();
				farmAnimal.home = building;
				farmAnimal.homeLocation.Value = new Vector2(building.tileX, building.tileY);
				qe.forceProceed = true;
				farmAnimal.setRandomPosition(farmAnimal.home.indoors);
				(building.indoors.Value as AnimalHouse).animals.Add(farmAnimal.myID, farmAnimal);
				(building.indoors.Value as AnimalHouse).animalsThatLiveHere.Add(farmAnimal.myID);
				ModEntry.MOD_MONITOR.Log("Animal Homed(2)...", StardewModdingAPI.LogLevel.Trace);
			}
			Game1.exitActiveMenu();
			return false;
		}
	}
}