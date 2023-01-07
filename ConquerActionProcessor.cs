// using MagnumOpus.Squiggly.Models;
// using MagnumOpus.ECS;
// using MagnumOpus.Enums;
// using HerstLib.IO;

// namespace MagnumOpus.Squiggly
// {
//     public static class ConquerActionProcessor
//     {
//         // public static void ExecuteAction(CqMonster target, PixelEntity attacker)
//         // {
//         //     using var db = new SquigglyContext();
//         //     var cqaction = db.cq_action.Find((long)target.CQAction);
//         //     Process(target, attacker, cqaction, db);
//         // }
//         public static void ExecuteAction(CqNpc target, PixelEntity attacker, long task = 0)
//         {
//             using var db = new SquigglyContext();
//             var ntt = PixelWorld.GetEntityByNetId(target.UniqueId);
//             if (task == 0)
//             {
//                 var cqtask = db.cq_task.Find(target.Task0);
//                 var cqaction = db.cq_action.Find(cqtask.id_next);
//                 ProcessNpc(ntt, attacker, cqaction, db);
//             }
//             else
//             {
//                 var cqtask = db.cq_task.Find(task);
//                 var cqaction = db.cq_action.Find(cqtask.id_next);
//                 ProcessNpc(ntt, attacker, cqaction, db);
//             }
//         }

//         private static void ProcessNpc(PixelEntity npc, PixelEntity player, cq_action act, SquigglyContext db)
//         {
//             if (act == null || act.id == 0)
//                 return;
                            
//             var type = (Cq_ActionId)act.type;
//             //FConsole.WriteLine($"Mob Action -> Type: {type}:{(int) type} Data: {cqaction.data} Param: {cqaction.param.Trim()}");

//             switch (type)
//             {
//                 default:
//                     {
//                         FConsole.WriteLine($"Unknown Cq_ActionId -> {act}");
//                         break;
//                     }
//                 case Cq_ActionId.ACTION_MENUTEXT:
//                     {
//                             player.NetSync(LegacyPackets.NpcSay(act.param.Trim().Replace("~", " ")));
//                             if (act.id_next == 0)
//                                 player.NetSync(LegacyPackets.NpcFinish());
//                             else
//                                 ProcessNpc(npc, player, db.cq_action.Find(act.id_next), db);
//                         break;
//                     }
//                 case Cq_ActionId.ACTION_MENULINK:
//                     {
//                         if (player is PixelEntity player)
//                         {
//                             var option = act.param.Trim().Split(' ')[0];
//                             var sControl = act.param.Split(' ', StringSplitOptions.RemoveEmptyEntries);
//                             var control = int.Parse(sControl[1]);

//                             player.NpcTasks.Add((byte)player.NpcTasks.Count, (int)control);

//                             player.NetSync(LegacyPackets.NpcLink(option.Replace("~", " "), (byte)(player.NpcTasks.Count - 1)));
//                             if (act.id_next == 0)
//                                 player.NetSync(LegacyPackets.NpcFinish());
//                             else
//                                 ProcessNpc(npc, player, db.cq_action.Find(act.id_next), db);
//                         }

//                         break;
//                     }
//                 case Cq_ActionId.ACTION_MENUPIC:
//                     {
//                         if (player is PixelEntity player)
//                         {
//                             var faceId = byte.Parse(act.param.Trim().Split(' ')[2]);
//                             player.NetSync(LegacyPackets.NpcFace(faceId));
//                             if (act.id_next == 0)
//                                 player.NetSync(LegacyPackets.NpcFinish());
//                             else
//                                 ProcessNpc(npc, player, db.cq_action.Find(act.id_next), db);
//                         }
//                         break;
//                     }
//                 case Cq_ActionId.ACTION_MENUCREATE:
//                     {
//                         if (player is PixelEntity player)
//                         {
//                             player.NetSync(LegacyPackets.NpcFinish());
//                         }
//                         break;
//                     }
//                 case Cq_ActionId.ACTION_USER_SEX:
//                     {
//                         if (act.id_nextfail == 0 && act.id_next == 0)
//                             return;

//                         //If male next_id else nextid_fail

//                         break;
//                     }
//                 case Cq_ActionId.ACTION_ITEM_CHECK:
//                     {
//                         ACTION_ITEM_CHECK(npc, player, act, db);
//                         break;
//                     }
//                 case Cq_ActionId.ACTION_MST_DROPITEM:
//                     {
//                         ACTION_MST_DROPITEM(npc, player, act, db);
//                         break;
//                     }
//                 case Cq_ActionId.ACTION_RAND:
//                     {
//                         ACTION_RAND(npc, player, act, db);
//                         break;
//                     }
//                 case Cq_ActionId.ACTION_USER_ATTR:
//                     {
//                         ACTION_USER_ATTR(npc, player, act, db);
//                         break;
//                     }
//                 case Cq_ActionId.ACTION_RANDACTION:
//                     {
//                         ACTION_RANDACTION(npc, player, act, db);
//                         break;
//                     }
//                 case Cq_ActionId.ACTION_USER_TALK:
//                     {
//                         player.GetMessage("SYSTEM", player.Name.Trim(), act.param.Trim().Replace("~", ""), (MsgTextType)act.data);
//                         break;
//                     }
//                 case Cq_ActionId.ACTION_USER_HAIR:
//                     {
//                         ACTION_USER_HAIR(npc, player, act, db);
//                         break;
//                     }
//                 case Cq_ActionId.ACTION_USER_MEDIAPLAY:
//                     {
//                         ACTION_USER_MEDIAPLAY(npc, player, act, db);
//                         break;
//                     }
//                 case Cq_ActionId.ACTION_USER_EFFECT:
//                     {
//                         ACTION_USER_EFFECT(npc, player, act, db);
//                         break;
//                     }
//                 case Cq_ActionId.ACTION_USER_CHGMAP:
//                     {
//                         ACTION_USER_CHGMAP(npc, player, act, db);
//                         break;
//                     }
//                 case Cq_ActionId.ACTION_ITEM_LEAVESPACE:
//                     ACTION_ITEM_LEAVESPACE(npc, player, act, db);
//                     break;
//                 case Cq_ActionId.ACTION_ITEM_ADD:
//                     ACTION_ITEM_ADD(npc, player, act, db);
//                     break;
//                 case Cq_ActionId.ACTION_ITEM_MULTICHK:
//                     ACTION_ITEM_MULTICHK(npc, player, act, db);
//                     break;
//                 case Cq_ActionId.ACTION_ITEM_MULTIDEL:
//                     ACTION_ITEM_MULTIDEL(npc, player, act, db);
//                     break;
//                 case Cq_ActionId.ACTION_CHKTIME:
//                     ACTION_CHKTIME(npc, player, act, db);
//                     break;
//                 case Cq_ActionId.ACTION_USER_MAGIC:
//                     ACTION_USER_MAGIC(npc, player, act, db);
//                     break;
//             }
//         }

//         private static void ACTION_USER_MAGIC(PixelEntity target, PixelEntity invoker, cq_action cqaction, SquigglyContext db)
//         {
//             var param = cqaction.param.Trim().Split(' ');
//             var skillId = ushort.Parse(param[1]);
//             switch (param[0])
//             {
//                 case "learn":
//                     {
//                         invoker.AddSkill(new Skill(skillId, 0, 0));
//                         break;
//                     }
//             }
//             ProcessNpc(target, invoker, db.cq_action.Find(cqaction.id_next), db);
//         }

//         private static void ACTION_CHKTIME(PixelEntity target, PixelEntity invoker, cq_action cqaction, SquigglyContext db)
//         {
//             if (cqaction.data == 3)
//             {
//                 var param = cqaction.param.Trim().Split(' ');
//                 var startDayOfWeek = int.Parse(param[0]);
//                 var startTime = param[1];
//                 var endDayOfWeek = int.Parse(param[2]);
//                 var endTime = param[3];

//                 if (((int)DateTime.Now.DayOfWeek) >= startDayOfWeek && ((int)DateTime.Now.DayOfWeek) <= endDayOfWeek)
//                 {
//                     var startDT = DateTime.ParseExact(startTime, "H:mm", null, System.Globalization.DateTimeStyles.None);
//                     var endDT = DateTime.ParseExact(endTime, "H:mm", null, System.Globalization.DateTimeStyles.None);

//                     if (DateTime.Now.Hour >= startDT.Hour && DateTime.Now.Hour <= endDT.Hour)
//                     {
//                         ProcessNpc(target, invoker, db.cq_action.Find(cqaction.id_next), db);
//                         return;
//                     }

//                 }
//             }
//             ProcessNpc(target, invoker, db.cq_action.Find(cqaction.id_nextfail), db);
//         }

//         private static void ACTION_ITEM_MULTIDEL(PixelEntity target, PixelEntity invoker, cq_action cqaction, SquigglyContext db)
//         {
//             var items = cqaction.param.Trim().Split(' ');

//             var startId = int.Parse(items[0]);
//             var endId = int.Parse(items[1]);
//             var amount = int.Parse(items[2]);

//             var range = Enumerable.Range(startId, (endId - startId) + 1);


//             var amountFound = (from id in range from inventoryItem in invoker.Inventory.Items where inventoryItem.Value.ItemId == id select id).Count();
//             var removed = 0;
//             foreach (var inventoryItem in invoker.Inventory.Items)
//             {
//                 if (removed == amount)
//                     break;
//                 if (range.Contains(inventoryItem.Value.ItemId))
//                 {
//                     invoker.Inventory.RemoveItem(inventoryItem.Value);
//                     removed++;
//                 }
//             }


//             ProcessNpc(target, invoker, amountFound >= amount
//                 ? db.cq_action.Find(cqaction.id_next)
//                 : db.cq_action.Find(cqaction.id_nextfail), db);
//         }
//         private static void ACTION_ITEM_MULTICHK(PixelEntity target, PixelEntity invoker, cq_action cqaction, SquigglyContext db)
//         {
//             var items = cqaction.param.Trim().Split(' ');

//             var startId = int.Parse(items[0]);
//             var endId = int.Parse(items[1]);
//             var amount = int.Parse(items[2]);

//             var range = Enumerable.Range(startId, (endId - startId) + 1);


//             var amountFound = (from id in range from inventoryItem in invoker.Inventory.Items where inventoryItem.Value.ItemId == id select id).Count();

//             ProcessNpc(target, invoker, amountFound >= amount
//                 ? db.cq_action.Find(cqaction.id_next)
//                 : db.cq_action.Find(cqaction.id_nextfail), db);
//         }

//         private static void ACTION_ITEM_ADD(PixelEntity target, PixelEntity invoker, cq_action cqaction, SquigglyContext db)
//         {
//             invoker.Inventory.AddItem(Item.Factory.Create(721189));
//             ProcessNpc(target, invoker, db.cq_action.Find(cqaction.id_next), db);
//         }

//         private static void ACTION_ITEM_LEAVESPACE(PixelEntity target, PixelEntity invoker, cq_action cqaction, SquigglyContext db)
//         {
//             if (invoker.Inventory.Count + cqaction.data < 40)
//             {
//                 ProcessNpc(target, invoker, db.cq_action.Find(cqaction.id_next), db);
//             }
//             else
//             {
//                 ProcessNpc(target, invoker, db.cq_action.Find(cqaction.id_nextfail), db);
//             }
//         }

//         private static void ACTION_USER_CHGMAP(PixelEntity target, PixelEntity invoker, cq_action cqaction, SquigglyContext db)
//         {
//             var nextIds = cqaction.param.Trim().Split(' ');
//             var map = ushort.Parse(nextIds[0]);
//             var x = ushort.Parse(nextIds[1]);
//             var y = ushort.Parse(nextIds[2]);
//             invoker.Teleport(x, y, map);
//             ProcessNpc(target, invoker, db.cq_action.Find(cqaction.id_next), db);
//         }

//         private static void ACTION_USER_EFFECT(PixelEntity target, PixelEntity invoker, cq_action cqaction, SquigglyContext db)
//         {
//             var nextIds = cqaction.param.Trim().Split(' ');
//             invoker.Send(LegacyPackets.Effect(invoker, nextIds[1]));
//             ProcessNpc(target, invoker, db.cq_action.Find(cqaction.id_next), db);
//         }

//         private static void ACTION_USER_MEDIAPLAY(PixelEntity target, PixelEntity invoker, cq_action cqaction, SquigglyContext db)
//         {
//             var nextIds = cqaction.param.Trim().Split(' ');
//             //Send Sound Packet here
//             ProcessNpc(target, invoker, db.cq_action.Find(cqaction.id_next), db);
//         }

//         private static void ACTION_USER_HAIR(PixelEntity target, PixelEntity invoker, cq_action cqaction, SquigglyContext db)
//         {
//             var nextIds = cqaction.param.Trim().Split(' ');
//             invoker.Hair = uint.Parse(nextIds[1]);
//             ProcessNpc(target, invoker, db.cq_action.Find(cqaction.id_next), db);
//         }

//         private static void ACTION_ITEM_CHECK(PixelEntity target, PixelEntity invoker, cq_action cqaction, SquigglyContext db)
//         {
//             var itemId = cqaction.data;

//             if (invoker.Inventory.HasItem(itemId))
//             {
//                 ProcessNpc(target, invoker, db.cq_action.Find(cqaction.id_next),
//                     db);
//             }
//             else
//             {
//                 ProcessNpc(target, invoker, db.cq_action.Find(cqaction.id_nextfail), db);
//             }
//         }

//         private static void ACTION_RANDACTION(PixelEntity target, PixelEntity attacker, cq_action cqaction, SquigglyContext db)
//         {
//             var nextIds = cqaction.param.Trim().Split(' ');

//             var nextIndex = Random.Shared.Next(nextIds.Length);

//             var nextId = long.Parse(nextIds[nextIndex]);

//             cqaction = db.cq_action.Find(nextId);
//             //FConsole.WriteLine($"Mob Action -> Data: {cqaction.data} Param: {cqaction.param.Trim()}",ConsoleColor.Green);

//             var dropId = cqaction.param.Trim().Split(' ')[1];
//             var item = Item.Factory.Create(int.Parse(dropId));
//             FloorItemSystem.Drop(attacker, target, item);
//         }

//         private static void ACTION_USER_ATTR(PixelEntity target, PixelEntity attacker, cq_action cqaction, SquigglyContext db)
//         {
//             var condition = cqaction.param.Trim();
//             var what = condition.Split(' ')[0];
//             switch (what)
//             {
//                 case "level":
//                     {
//                         var op = condition.Split(' ')[1];
//                         var val = condition.Split(' ')[2];

//                         switch (op)
//                         {
//                             case "<=":
//                                 {
//                                     if (attacker.Level <= byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case ">=":
//                                 {
//                                     if (attacker.Level >= byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case "<":
//                                 {
//                                     if (attacker.Level < byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case ">":
//                                 {
//                                     if (attacker.Level > byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case "==":
//                                 {
//                                     if (attacker.Level == byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             default:
//                                 Console.WriteLine("Unknown Operator for " + what + " -> " + op);
//                                 break;
//                         }

//                         break;
//                     }
//                 case "life":
//                     {
//                         var op = condition.Split(' ')[1];
//                         var val = condition.Split(' ')[2];

//                         switch (op)
//                         {
//                             case "<=":
//                                 {
//                                     if (attacker.CurrentHp <= int.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next),
//                                             db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case ">=":
//                                 {
//                                     if (attacker.CurrentHp >= int.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case "<":
//                                 {
//                                     if (attacker.CurrentHp < int.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case ">":
//                                 {
//                                     if (attacker.CurrentHp > int.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case "==":
//                                 {
//                                     if (attacker.CurrentHp == int.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             default:
//                                 Console.WriteLine("Unknown Operator for " + what + " -> " + op);
//                                 break;
//                         }

//                         break;
//                     }
//                 case "metempsychosis":
//                     {
//                         var op = condition.Split(' ')[1];
//                         var val = condition.Split(' ')[2];

//                         switch (op)
//                         {
//                             case "<=":
//                                 {
//                                     if (attacker.Reborn <= byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case ">=":
//                                 {
//                                     if (attacker.Reborn >= byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case "<":
//                                 {
//                                     if (attacker.Reborn < byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case ">":
//                                 {
//                                     if (attacker.Reborn > byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case "==":
//                                 {
//                                     if (attacker.Reborn == byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next),
//                                            db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int) type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker,
//                                             db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             default:
//                                 Console.WriteLine("Unknown Operator for " + what + " -> " + op);
//                                 break;
//                         }

//                         break;
//                     }
//                 case "profession":
//                     {
//                         var op = condition.Split(' ')[1];
//                         var val = condition.Split(' ')[2];

//                         switch (op)
//                         {
//                             case "<=":
//                                 {
//                                     if (attacker.Class <= byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case "<":
//                                 {
//                                     if (attacker.Class < byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case ">=":
//                                 {
//                                     if (attacker.Class >= byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case ">":
//                                 {
//                                     if (attacker.Class > byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case "==":
//                                 {
//                                     if (attacker.Class == byte.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             default:
//                                 Console.WriteLine("Unknown Operator for " + what + " -> " + op);
//                                 break;
//                         }

//                         break;
//                     }
//                 case "money":
//                     {
//                         var op = condition.Split(' ')[1];
//                         var val = condition.Split(' ')[2];

//                         switch (op)
//                         {
//                             case "+=":
//                                 {
//                                     attacker.Money += int.Parse(val);
//                                     ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     break;
//                                 }
//                             case "<=":
//                                 {
//                                     if (attacker.Money <= int.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case "<":
//                                 {
//                                     if (attacker.Money < int.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case ">=":
//                                 {
//                                     if (attacker.Money >= int.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case ">":
//                                 {
//                                     if (attacker.Money > int.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             case "==":
//                                 {
//                                     if (attacker.Money == int.Parse(val))
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Green);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                                     }
//                                     else
//                                     {
//                                         //FConsole.WriteLine($"{type}:{(int)type} -> {condition}", ConsoleColor.Red);
//                                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                                     }

//                                     break;
//                                 }
//                             default:
//                                 Console.WriteLine("Unknown Operator for " + what + " -> " + op);
//                                 break;
//                         }

//                         break;
//                     }
//                 default:
//                     {
//                         Console.WriteLine("Unknown ACTION_USER_ATTR -> " + what);
//                         break;
//                     }
//             }
//         }

//         private static void ACTION_RAND(PixelEntity target, PixelEntity attacker, cq_action cqaction, SquigglyContext db)
//         {
//             var amount = float.Parse(cqaction.param.Trim().Split(' ')[0]);
//             var afterKills = float.Parse(cqaction.param.Trim().Split(' ')[1]);
//             if (YiCore.Success(afterKills / amount))
//             {
//                 //FConsole.WriteLine($"{type}:{(int) type} -> Chance: {afterKills/amount}", ConsoleColor.Green);
//                 ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//             }
//             else
//             {
//                 //FConsole.WriteLine($"{type}:{(int) type} -> Chance: {afterKills / amount}", ConsoleColor.Red);
//                 ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//             }
//         }

//         private static void ACTION_MST_DROPITEM(PixelEntity target, PixelEntity attacker, cq_action cqaction, SquigglyContext db)
//         {
//             var condition = cqaction.param.Trim();
//             var what = condition.Split(' ')[0];
//             switch (what)
//             {
//                 case "dropmoney":
//                     {
//                         var maxAmount = int.Parse(condition.Split(' ')[1]);
//                         var chance = int.Parse(condition.Split(' ')[2]) / 100;


//                         if (YiCore.Success(chance))
//                         {
//                             //FConsole.WriteLine($"{type}:{(int) type} -> {maxAmount} {chance}", ConsoleColor.Green);
//                             ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                             FloorItemSystem.DropMoney(attacker, target, maxAmount);
//                         }
//                         else
//                         {
//                             //FConsole.WriteLine($"{type}:{(int) type} -> {maxAmount} {chance}", ConsoleColor.Red);
//                             ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_nextfail), db);
//                         }

//                         break;
//                     }
//                 case "dropitem":
//                     {
//                         var id = int.Parse(condition.Split(' ')[1]);
//                         //FConsole.WriteLine($"{type}:{(int) type} -> {id}", ConsoleColor.Green);
//                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                         FloorItemSystem.Drop(attacker, target, Item.Factory.Create(id));
//                         ProcessNpc(target, attacker, db.cq_action.Find(cqaction.id_next), db);
//                         break;
//                     }
//             }
//         }
//     }
// }