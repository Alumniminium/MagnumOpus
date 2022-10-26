using System.Globalization;
using HerstLib.IO;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Helpers;
using MagnumOpus.Networking.Packets;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Networking
{
    public static unsafe class GamePacketHandler
    {
        internal static void Process(in PixelEntity ntt, in Memory<byte> packet)
        {
            var id = (PacketType)BitConverter.ToUInt16(packet.Span[2..]);
            ref readonly var net = ref ntt.Get<NetworkComponent>();

            switch (id)
            {
                case PacketType.MsgLogin:
                    {
                        var msg = (MsgConnectGame)packet;
                        var language = msg.GetLanguage();
                        FConsole.WriteLine($"[GAME] Client Version: {msg.ClientVersion}, Language: {language}");
                        var ntc = new NameTagComponent(ntt.Id, "test");
                        ntt.Add(ref ntc);

                        var ok = MsgText.Create("SYSTEM", "ALLUSERS", "ANSWER_OK", MsgTextType.LoginInformation);
                        ntt.NetSync(in ok);

                        var info = MsgCharacter.Create(ntt);
                        ntt.NetSync(in info);
                        break;
                    }
                case PacketType.MsgRole:
                    {
                        // var msg = (MsgRole)packet;
                        var ok = MsgText.Create("SYSTEM", "ALLUSERS", "ANSWER_OK", MsgTextType.LoginInformation);
                        ntt.NetSync(in ok);
                        break;
                    }
                    case PacketType.MsgItem:
                    {
                        var msg = (MsgItem)packet;

                        switch (msg.Type)
                        {
                            case MsgItemType.BuyItemAddItem:
                                break;
                            case MsgItemType.SellItem:
                                break;
                            case MsgItemType.RemoveInventory:
                                break;
                            case MsgItemType.EquipItem:
                                break;
                            case MsgItemType.SetEquipPosition:
                                break;
                            case MsgItemType.UnEquipItem:
                                break;
                            case MsgItemType.ShowWarehouseMoney:
                                break;
                            case MsgItemType.DepositWarehouseMoney:
                                break;
                            case MsgItemType.WithdrawWarehouseMoney:
                                break;
                            case MsgItemType.DropGold:
                                break;
                            case MsgItemType.RepairItem:
                                break;
                            case MsgItemType.UpdateDurability:
                                break;
                            case MsgItemType.RemoveEquipment:
                                break;
                            case MsgItemType.UpgradeDragonball:
                                break;
                            case MsgItemType.UpgradeMeteor:
                                break;
                            case MsgItemType.ShowVendingList:
                                break;
                            case MsgItemType.AddVendingItem:
                                break;
                            case MsgItemType.RemoveVendingItem:
                                break;
                            case MsgItemType.BuyVendingItem:
                                break;
                            case MsgItemType.UpdateArrowCount:
                                break;
                            case MsgItemType.ParticleEffect:
                                break;
                            case MsgItemType.Ping:
                                var reply = MsgItem.Create(ntt.Id, msg.Value, msg.Param, msg.Timestamp, MsgItemType.Ping);
                                var tick = MsgTick.Create(in ntt);
                                ntt.NetSync(in reply);
                                ntt.NetSync(in tick);
                                break;
                            case MsgItemType.Enchant:
                                break;
                            case MsgItemType.BoothAddCp:
                                break;
                        }
                        break;
                    }
                case PacketType.MsgAction:
                    {
                        var msg = (MsgAction)packet;

                        switch (msg.Type)
                        {
                            case MsgActionType.SendLocation:
                                {
                                    ref var pos = ref ntt.Get<PositionComponent>();
                                    //var reply = MsgAction.Create(0, ntt.Id, (int)pos.Position.Z, (ushort)pos.Position.X, (ushort)pos.Position.Y, MsgActionType.SendLocation);
                                    var reply = MsgAction.Create(0, ntt.Id, 1002, 438, 377, 0, msg.Type);
                                    ntt.NetSync(in reply);
                                    PixelWorld.Players.Add(ntt);
                                    break;
                                }
                            case MsgActionType.Jump:
                                {
                                    var jmp = new JumpComponent(ntt.Id, msg.X, msg.Y);
                                    ntt.Add(ref jmp);
                                    // ref var pos = ref ntt.Get<PositionComponent>();
                                    // pos.Position.X = msg.X;
                                    // pos.Position.Y = msg.Y;
                                    // pos.ChangedTick = PixelWorld.Tick;

                                    // var reply = MsgAction.Create(0, ntt.Id, (ushort)pos.Position.Z, msg.X, msg.Y, 0, msg.Type);
                                    // ntt.NetSync(in reply);
                                    break;
                                }
                            case MsgActionType.SendItems:
                            case MsgActionType.SendAssociates:
                            case MsgActionType.SendProficiencies:
                            case MsgActionType.SendSpells:
                            {
                                var reply = MsgAction.Create(0, ntt.Id, 0, 0, 0, 0, msg.Type);
                                ntt.NetSync(in reply);
                                break;
                            }
                            case MsgActionType.ChangeFacing:
                            {
                                var dir = new DirectionComponent(ntt.Id, msg.Direction);
                                ntt.Add(ref dir);
                                break;
                            }
                            case MsgActionType.ChangeAction:
                            {
                                var emo = new EmoteComponent(ntt.Id, (Emote)msg.Param);
                                ntt.Add(ref emo);
                                break;
                            }
                            case MsgActionType.ChangeMap:
                                break;
                            case MsgActionType.Teleport:
                                break;
                            case MsgActionType.LevelUp:
                                break;
                            case MsgActionType.XpClear:
                                break;
                            case MsgActionType.Revive:
                                break;
                            case MsgActionType.DelRole:
                                break;
                            case MsgActionType.SetKillMode:
                                break;
                            case MsgActionType.ConfirmGuild:
                                break;
                            case MsgActionType.Mine:
                                break;
                            case MsgActionType.TeamMemberPos:
                                break;
                            case MsgActionType.QueryEntity:
                                break;
                            case MsgActionType.AbortMagic:
                                break;
                            case MsgActionType.MapARGB:
                                break;
                            case MsgActionType.MapStatus:
                                break;
                            case MsgActionType.QueryTeamMember:
                                break;
                            case MsgActionType.Kickback:
                                break;
                            case MsgActionType.DropMagic:
                                break;
                            case MsgActionType.DropSkill:
                                break;
                            case MsgActionType.CreateBooth:
                                break;
                            case MsgActionType.SuspendBooth:
                                break;
                            case MsgActionType.ResumeBooth:
                                break;
                            case MsgActionType.LeaveBooth:
                                break;
                            case MsgActionType.PostCommand:
                                break;
                            case MsgActionType.QueryEquipment:
                                break;
                            case MsgActionType.AbortTransform:
                                break;
                            case MsgActionType.EndFly:
                                break;
                            case MsgActionType.GetMoney:
                                break;
                            case MsgActionType.QueryEnemy:
                                break;
                            case MsgActionType.OpenDialog:
                                break;
                            case MsgActionType.GuardJump:
                                break;
                            case MsgActionType.SpawnEffect:
                                break;
                            case MsgActionType.RemoveEntity:
                                break;
                            case MsgActionType.TeleportReply:
                                break;
                            case MsgActionType.DeathConfirmation:
                                break;
                            case MsgActionType.QueryAssociateInfo:
                                break;
                            case MsgActionType.ChangeFace:
                                break;
                            case MsgActionType.ItemsDetained:
                                break;
                            case MsgActionType.NinjaStep:
                                break;
                            case MsgActionType.HideInterface:
                                break;
                            case MsgActionType.OpenUpgrade:
                                break;
                            case MsgActionType.AwayFromKeyboard:
                                break;
                            case MsgActionType.PathFinding:
                                break;
                            case MsgActionType.DragonBallDropped:
                                break;
                            case MsgActionType.TableState:
                                break;
                            case MsgActionType.TablePot:
                                break;
                            case MsgActionType.TablePlayerCount:
                                break;
                            case MsgActionType.QueryFriendEquip:
                                break;
                            case MsgActionType.QueryStatInfo:
                                break;
                            default:
                                {
                                    FConsole.WriteLine($"[GAME] Unhandled MsgActionType: {(int)msg.Type}/{msg.Type}");
                                    var reply = MsgAction.Create(msg.Timestamp, ntt.Id, msg.Param, msg.X, msg.Y, msg.Direction, msg.Type);
                                    ntt.NetSync(in reply);
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        FConsole.WriteLine($"[GAME] Unknown packet ID: {(int)id}/{id}");
                        FConsole.WriteLine(packet.Dump());
                        break;
                    }
            }
        }
    }
}