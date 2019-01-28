using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UberStrok.Core.Common;
using UberStrok.Core.Views;
using System.Linq;
using System.Drawing;
using Color = UberStrok.Core.Common.Color;

namespace UberStrok.Realtime.Server.Game
{
    public abstract partial class BaseGameRoom : BaseGameRoomOperationHandler, IRoom<GamePeer>
    {
        // ANTI-CHEAT CONFIG
        // LeniencyMS is how lenient the anti-cheat is when it comes to shots fired.
        // 50 is recommended. It allows shots to be 50ms out-of-range of their intended firerate.
        // Setting this value too high will not allow it to detect reasonably fast firerate cheats,
        // but settings it too low will have an increased rate of accidental auto-bans.
        int leniencyMs = 50;
        // Making this value false will disable the anti-cheat. This is highly recommended if you're playing with people you trust.
        // The anti-cheat system is not perfect, and I'm currently unsure of the effects high ping will have on this system.
        bool enableAntiCheat = true;

        public override void OnDisconnect(GamePeer peer, DisconnectReason reasonCode, string reasonDetail)
        {
            Leave(peer);
        }

        protected override void OnJoinGame(GamePeer peer, TeamID team)
        {
            /* 
                When the client joins a game it resets its match state to 'none'.               

                Update the actor's team + other data and register the peer in the player list.
                Update the number of connected players while we're at it.
             */

            peer.Actor.Team = team;
            peer.Actor.Info.Health = 100;
            peer.Actor.Info.Ping = (ushort)(peer.RoundTripTime / 2);
            peer.Actor.Info.PlayerState = PlayerStates.Ready;
            // Convert the skin color hex code to an actual colour.
            peer.Actor.Info.SkinColor = Color.Convert(peer.Loadout.SkinColor);
            peer.Loadout = peer.Web.GetLoadout();
            s_log.Info("Joining game in progress.");

            lock (_peers)
            {
                // Make sure that we're not doubling up.
                // This is for when a client is rejoining.
                if (_players.FirstOrDefault(x => x.Actor.Cmid == peer.Actor.Cmid) == null)
                {
                    _players.Add(peer);
                    _view.ConnectedPlayers = Players.Count;
                }
            }

            OnPlayerJoined(new PlayerJoinedEventArgs
            {
                Player = peer,
                Team = team
            });

            s_log.Info($"Joining team -> CMID:{peer.Actor.Cmid}:{team}:{peer.Actor.Number}");
        }

        protected override void OnChatMessage(GamePeer peer, string message, byte context)
        {
            Actions.ChatMessage(peer, message, (ChatContext)context);
        }

        protected override void OnPowerUpPicked(GamePeer peer, int pickupId, byte type, byte value)
        {
            PowerUps.PickUp(peer, pickupId, (PickupItemType)type, value);
        }

        protected override void OnPowerUpRespawnTimes(GamePeer peer, List<ushort> respawnTimes)
        {
            /* We care only about the first operation sent. */
            if (!_powerUpManager.IsLoaded)
                _powerUpManager.Load(respawnTimes);
        }

        protected override void OnSpawnPositions(GamePeer peer, TeamID team, List<Vector3> positions, List<byte> rotations)
        {
            Debug.Assert(positions.Count == rotations.Count, "Number of spawn positions given and number of rotations given is not equal.");

            /* We care only about the first operation sent for that team ID. */
            if (!_spawnManager.IsLoaded(team))
                _spawnManager.Load(team, positions, rotations);
        }

        protected override void OnRespawnRequest(GamePeer peer)
        {
            OnPlayerRespawned(new PlayerRespawnedEventArgs
            {
                Player = peer
            });
        }

        protected override void OnExplosionDamage(GamePeer peer, int target, byte slot, byte distance, Vector3 force)
        {
            var weaponId = peer.Actor.Info.Weapons[slot];

            foreach (var player in Players)
            {
                if (player.Actor.Cmid != target)
                    continue;

                var weapon = default(UberStrikeItemWeaponView);
                if (ShopManager.WeaponItems.TryGetValue(weaponId, out weapon))
                {
                    float damage = weapon.DamagePerProjectile;
                    float radius = weapon.SplashRadius / 100f;
                    float damageExplosion = damage * (radius - distance) / radius;

                    s_log.Debug($"Calculated: {damageExplosion} damage explosive {damage}, {radius}, {distance}, {force}");

                    peer.IncrementDamageDone(weapon.ItemClass, weaponId, (int)damageExplosion);
                    peer.IncrementShotsHit(weapon.ItemClass, weaponId);

                    /* Calculate the direction of the hit. */
                    var shortDamage = (short)damageExplosion;

                    var victimPos = player.Actor.Movement.Position;
                    var attackerPos = peer.Actor.Movement.Position;

                    var direction = attackerPos - victimPos;
                    var back = new Vector3(0, 0, -1);

                    var angle = Vector3.Angle(direction, back);
                    if (direction.x < 0)
                        angle = 360 - angle;

                    var byteAngle = Conversions.Angle2Byte(angle);

                    /* TODO: Find out the damage effect type (slow down -> needler) & stuffs. */

                    var weightedArmorAbsorption = _armorAbsorb;
                    // Armor weight. Max armor absorption is 72% (two armor pieces of 20 weight)
                    if (View.GameFlags.DefenseBonus)
                    {
                        foreach (var armor in player.Actor.Info.Gear)
                        {
                            // don't attempt to calculate empty slot
                            if (armor == 0)
                                continue;

                            var gear = default(UberStrikeItemGearView);
                            if (ShopManager.GearItems.TryGetValue(armor, out gear))
                            {
                                if (gear.ArmorWeight > 0)
                                    weightedArmorAbsorption *= 1 + (gear.ArmorWeight / 100f);
                            }
                            else
                                s_log.Debug($"Could not find gear with ID {armor}.");
                        }
                    }

                    /* Don't mess with rocket jumps. */
                    if (player.Actor.Cmid != peer.Actor.Cmid)
                    {
                        // take off armor™
                        if (player.Actor.Info.ArmorPoints > 0)
                        {
                            // player's armor before they were damaged, we store this to calculate the diff
                            int originalArmor = player.Actor.Info.ArmorPoints;
                            // make sure their armor cant go below 0, also allows partial absorption
                            player.Actor.Info.ArmorPoints = (byte)Math.Max(0, player.Actor.Info.ArmorPoints - shortDamage);
                            // the value the diff is multiplied by is the armor absorption ratio
                            // maybe put this value into the config?
                            double diff = (originalArmor - player.Actor.Info.ArmorPoints) * weightedArmorAbsorption;
                            // subtract the absorbed damage from the damage value
                            shortDamage -= (short)diff;
                        }
                        // lol idk what this does but i think its necessary for enemy damage
                        player.Actor.Damages.Add(byteAngle, shortDamage, BodyPart.Body, 0, 0);
                    }
                    else
                    {
                        // take off armor based on the halved value
                        shortDamage /= 2;
                        if (player.Actor.Info.ArmorPoints > 0)
                        {
                            int originalArmor = player.Actor.Info.ArmorPoints;
                            player.Actor.Info.ArmorPoints = (byte)Math.Max(0, player.Actor.Info.ArmorPoints - shortDamage);
                            double diff = (originalArmor - player.Actor.Info.ArmorPoints) * weightedArmorAbsorption;
                            shortDamage -= (short)diff;
                        }
                    }

                    player.Actor.Info.Health -= shortDamage;
                    player.CurrentLifeStats.DamageReceived += (int)damage;
                    player.TotalStats.DamageReceived += (int)damage;

                    /* Check if the player is dead. */
                    if (player.Actor.Info.Health <= 0)
                    {
                        player.Actor.Info.PlayerState |= PlayerStates.Dead;
                        player.Actor.Info.Deaths++;
                        if (peer.Actor.Cmid != player.Actor.Cmid)
                        {
                            peer.Actor.Info.Kills++;
                            peer.IncrementKills(weapon.ItemClass, BodyPart.Body);
                        }
                        else
                            peer.Actor.Info.Kills--;

                        player.State.Set(PeerState.Id.Killed);
                        OnPlayerKilled(new PlayerKilledEventArgs
                        {
                            AttackerCmid = peer.Actor.Cmid,
                            VictimCmid = player.Actor.Cmid,
                            ItemClass = weapon.ItemClass,
                            Damage = (ushort)shortDamage,
                            Part = BodyPart.Body,
                            Direction = -direction
                        });
                    }
                    else
                    {
                        player.Events.Game.SendPlayerHit(force);
                    }
                }
                else
                {
                    s_log.Debug($"Unable to find weapon with ID {weaponId}");
                }
                return;
            }
        }

        protected override void OnDirectDeath(GamePeer peer)
        {
            OnPlayerKilled(new PlayerKilledEventArgs
            {
                AttackerCmid = peer.Actor.Cmid,
                VictimCmid = peer.Actor.Cmid,
                ItemClass = UberStrikeItemClass.WeaponMachinegun,
                Part = BodyPart.Body,
                Direction = new Vector3()
            });
        }

        protected override void OnDirectDamage(GamePeer peer, ushort damage)
        {
            var actualDamage = (short)damage;
            s_log.Debug($"Damage: {damage}");
            /* THEY SHEATING */
            if (damage < 0)
                return;

            peer.Actor.Info.Health -= actualDamage;

            /* Check if the player is dead. */
            if (peer.Actor.Info.Health <= 0)
            {
                peer.Actor.Info.PlayerState |= PlayerStates.Dead;
                peer.Actor.Info.Deaths++;

                peer.State.Set(PeerState.Id.Killed);
                OnPlayerKilled(new PlayerKilledEventArgs
                {
                    AttackerCmid = peer.Actor.Cmid,
                    VictimCmid = peer.Actor.Cmid,
                    ItemClass = UberStrikeItemClass.WeaponMachinegun,
                    Damage = (ushort)actualDamage,
                    Part = BodyPart.Body,
                    Direction = new Vector3()
                });
            }
        }

        protected override void OnDirectHitDamage(GamePeer peer, int target, byte bodyPart, byte bullets, byte slot)
        {
            // Use the client's slot ID.
            // Allows for more accurate damage.
            // With the previous system, if you hit someone with a melee weapon and swapped fast enough, it would deal the damage of the swapped weapon.
            // E.g. hit someone with Stalwart Steel and swap to AWP, instant 135 damage, at the firerate of Stalwart Steel.
            // Not exactly balanced, is it?
            //var weaponId = peer.Actor.Info.CurrentWeaponID;

            foreach (var player in Players)
            {
                if (player.Actor.Cmid != target)
                    continue;

                var weapon = default(UberStrikeItemWeaponView);
                if (ShopManager.WeaponItems.TryGetValue(peer.Actor.Info.Weapons[slot], out weapon))
                {
                    /* TODO: Clamp value. */
                    var damage = (weapon.DamagePerProjectile * bullets);

                    // Anti-Cheat thing
                    if (enableAntiCheat)
                    {
                        if (!peer.WeaponStats.ContainsKey(peer.Actor.Info.Weapons[slot]))
                            peer.WeaponStats.Add(weapon.ID, new WeaponStats
                            {
                                DamageDone = damage,
                                ItemClass = weapon.ItemClass
                            });

                        var timeFromLastShot = DateTime.Now.TimeOfDay - peer.WeaponStats[weapon.ID].LastShot;
                        // Check if the time from the last shot was over the rate of fire. We can give or take 20ms, because even the hackiest of hacks don't go that fast.
                        if (timeFromLastShot.TotalMilliseconds < weapon.RateOfFire - 50 && timeFromLastShot.TotalMilliseconds > 50)
                        {
                            s_log.Debug($"{weapon.Name} was last fired {timeFromLastShot.TotalMilliseconds}ms ago, but can only be fired once every {weapon.RateOfFire}ms!");
                            // ban the heckin heck outta them for 1 day
                            peer.Web.Ban(DateTime.Now.AddDays(1));
                            peer.Disconnect();
                            peer.Dispose();
                        }

                        peer.WeaponStats[weapon.ID].LastShot = DateTime.Now.TimeOfDay;
                    }
                    /* Calculate the critical hit damage. */
                    var part = (BodyPart)bodyPart;
                    var bonus = weapon.CriticalStrikeBonus;
                    if (bonus > 0)
                    {
                        if (part == BodyPart.Head || part == BodyPart.Nuts)
                            damage = (int)Math.Round(damage + (damage * (bonus / 100f)));
                    }

                    peer.IncrementDamageDone(weapon.ItemClass, weapon.ID, damage);
                    peer.IncrementShotsHit(weapon.ItemClass, weapon.ID);

                    /* Calculate the direction of the hit. */
                    var shortDamage = (short)damage;

                    var victimPos = player.Actor.Movement.Position;
                    var attackerPos = peer.Actor.Movement.Position;

                    var direction = attackerPos - victimPos;
                    var back = new Vector3(0, 0, -1);

                    var angle = Vector3.Angle(direction, back);
                    if (direction.x < 0)
                        angle = 360 - angle;

                    var byteAngle = Conversions.Angle2Byte(angle);

                    /* TODO: Find out the damage effect type (slow down -> needler) & stuffs. */

                    /* 
                     * Armor absorption percentage
                     * Change 'armorAbsorbPercent' to modify effective health given by armor.
                     * E.g. currently, 100 armor is equal to 66 extra health (if you are on at least 33% of your armor in health)
                     */
                    var weightedArmorAbsorption = _armorAbsorb;
                    // Armor weight. Max armor absorption is 72% (two armor pieces of 20 weight)
                    if (View.GameFlags.DefenseBonus)
                    {
                        foreach (var armor in player.Actor.Info.Gear)
                        {
                            // don't attempt to calculate empty slot
                            if (armor == 0)
                                continue;

                            var gear = default(UberStrikeItemGearView);
                            if (ShopManager.GearItems.TryGetValue(armor, out gear))
                            {
                                if (gear.ArmorWeight > 0)
                                    weightedArmorAbsorption *= 1 + (gear.ArmorWeight / 100f);
                            }
                            else
                                s_log.Debug($"Could not find gear with ID {armor}.");
                        }
                    }

                    if (player.Actor.Info.ArmorPoints > 0)
                    {
                        // player's armor before they were damaged, we store this to calculate the diff
                        int originalArmor = player.Actor.Info.ArmorPoints;
                        // make sure their armor cant go below 0, also allows partial absorption
                        player.Actor.Info.ArmorPoints = (byte)Math.Max(0, player.Actor.Info.ArmorPoints - shortDamage);
                        // the value the diff is multiplied by is the armor absorption ratio
                        // maybe put this value into the config? -------------------------v
                        double diff = (originalArmor - player.Actor.Info.ArmorPoints) * weightedArmorAbsorption;
                        // subtract the absorbed damage from the damage value
                        shortDamage -= (short)diff;
                    }

                    player.Actor.Damages.Add(byteAngle, shortDamage, part, 0, 0);
                    player.Actor.Info.Health -= shortDamage;
                    player.CurrentLifeStats.DamageReceived += damage;
                    player.TotalStats.DamageReceived += damage;

                    /* Check if the player is dead. */
                    if (player.Actor.Info.Health <= 0)
                    {
                        player.Actor.Info.PlayerState |= PlayerStates.Dead;
                        player.Actor.Info.Deaths++;
                        peer.Actor.Info.Kills++;
                        
                        peer.IncrementKills(weapon.ItemClass, part);

                        player.State.Set(PeerState.Id.Killed);
                        OnPlayerKilled(new PlayerKilledEventArgs
                        {
                            AttackerCmid = peer.Actor.Cmid,
                            VictimCmid = player.Actor.Cmid,
                            ItemClass = weapon.ItemClass,
                            Damage = (ushort)shortDamage,
                            Part = part,
                            Direction = -(direction.Normalized * weapon.DamageKnockback)
                        });
                    }
                }
                else
                {
                    s_log.Debug($"Unable to find weapon with ID {weapon.ID}");
                }

                return;
            }
        }

        protected override void OnSwitchTeam(GamePeer peer)
        {
            // Tally teams
            int redTeam = 0;
            int blueTeam = 0;
            foreach (var player in Players)
            {
                if (player.Actor.Team == TeamID.RED)
                    redTeam++;
                else if (player.Actor.Team == TeamID.BLUE)
                    blueTeam++;
            }

            TeamID targetTeam = TeamID.NONE;
            if (peer.Actor.Team == TeamID.BLUE)
                targetTeam = TeamID.RED;
            else if (peer.Actor.Team == TeamID.RED)
                targetTeam = TeamID.BLUE;

            if (redTeam == blueTeam)
                return;
            else if (targetTeam == TeamID.RED && redTeam > blueTeam)
                return;
            else if (targetTeam == TeamID.BLUE && blueTeam > redTeam)
                return;

            peer.Actor.Team = targetTeam;
            peer.State.Set(PeerState.Id.Killed);
            OnPlayerKilled(new PlayerKilledEventArgs
            {
                AttackerCmid = peer.Actor.Cmid,
                VictimCmid = peer.Actor.Cmid,
                ItemClass = UberStrikeItemClass.WeaponMachinegun,
                Part = BodyPart.Body,
            });

            foreach (var player in Players)
                player.Events.Game.SendPlayerChangedTeam(peer.Actor.Cmid, targetTeam);
        }

        protected override void OnEmitQuickItem(GamePeer peer, Vector3 origin, Vector3 direction, int itemId, byte playerNumber, int projectileID)
        {
            var userCmid = peer.Actor.Cmid;
            var quickItem = default(UberStrikeItemQuickView);
            if (ShopManager.QuickItems.TryGetValue(itemId, out quickItem))
            {
                foreach (var otherPeer in Peers)
                {
                    if (otherPeer.Actor.Cmid != userCmid)
                        otherPeer.Events.Game.SendEmitQuickItem(origin, direction, itemId, playerNumber, projectileID);
                }
            }
            else
                s_log.Debug($"Could not find quick item with ID {itemId}.");
            
        }

        protected override void OnEmitProjectile(GamePeer peer, Vector3 origin, Vector3 direction, byte slot, int projectileId, bool explode)
        {
            var shooterCmid = peer.Actor.Cmid;

            // TODO: Add anti-cheat here.
            // Should be easy, as we can actually tell how much and when they're firing.

            var weaponId = peer.Actor.Info.CurrentWeaponID;
            var weapon = default(UberStrikeItemWeaponView);

            if (ShopManager.WeaponItems.TryGetValue(weaponId, out weapon))
                peer.IncrementShotsFired(weapon.ItemClass, weaponId, 1);
            else
                s_log.Debug($"Unable to find weapon with ID {weaponId}");

            foreach (var otherPeer in Peers)
            {
                if (otherPeer.Actor.Cmid != shooterCmid)
                    otherPeer.Events.Game.SendEmitProjectile(shooterCmid, origin, direction, slot, projectileId, explode);
            }
        }

        protected override void OnRemoveProjectile(GamePeer peer, int projectileId, bool explode)
        {
            foreach (var otherPeer in Peers)
                otherPeer.Events.Game.SendRemoveProjectile(projectileId, explode);
        }

        protected override void OnJump(GamePeer peer, Vector3 position)
        {
            foreach (var otherPeer in Peers)
            {
                if (otherPeer.Actor.Cmid != peer.Actor.Cmid)
                    otherPeer.Events.Game.SendPlayerJumped(peer.Actor.Cmid, peer.Actor.Movement.Position);
            }
        }

        protected override void OnUpdatePositionAndRotation(GamePeer peer, Vector3 position, Vector3 velocity, byte horizontalRotation, byte verticalRotation, byte moveState)
        {
            peer.Actor.Movement.Position = position;
            peer.Actor.Movement.Velocity = velocity;
            peer.Actor.Movement.HorizontalRotation = horizontalRotation;
            peer.Actor.Movement.VerticalRotation = verticalRotation;
            peer.Actor.Movement.MovementState = moveState;
        }

        protected override void OnSwitchWeapon(GamePeer peer, byte slot)
        {
            /* Just incase. */
            peer.Actor.Info.ShootingTick = 0;
            peer.Actor.Info.CurrentWeaponSlot = slot;
        }

        protected override void OnSingleBulletFire(GamePeer peer)
        {
            var weapon = default(UberStrikeItemWeaponView);
            var weaponId = peer.Actor.Info.CurrentWeaponID;
            if (ShopManager.WeaponItems.TryGetValue(weaponId, out weapon))
                peer.IncrementShotsFired(weapon.ItemClass, weaponId, 1);
            /* 
                Set player in shooting state for 200ms.
                To allow client to respond to the change and play the animation.
             */
            var duration = Loop.ToTicks(TimeSpan.FromMilliseconds(200));

            peer.Actor.Info.ShootingTick += duration;
            peer.Actor.Info.PlayerState |= PlayerStates.Shooting;
        }

        protected override void OnIsInSniperMode(GamePeer peer, bool on)
        {
            var state = peer.Actor.Info.PlayerState;
            if (on)
                state |= PlayerStates.Sniping;
            else
                state &= ~PlayerStates.Sniping;

            peer.Actor.Info.PlayerState = state;
        }

        protected override void OnIsFiring(GamePeer peer, bool on)
        {
            var state = peer.Actor.Info.PlayerState;
            if (on)
            {
                peer.ShootStart = DateTime.Now.TimeOfDay;
                peer.ShootWeapon = peer.Actor.Info.CurrentWeaponID;
                state |= PlayerStates.Shooting;
            }
            else
            {
                peer.ShootEnd = DateTime.Now.TimeOfDay;

                var weapon = default(UberStrikeItemWeaponView);
                var weaponId = peer.ShootWeapon;
                if (ShopManager.WeaponItems.TryGetValue(weaponId, out weapon))
                {
                    if (!peer.WeaponStats.ContainsKey(weaponId))
                        peer.WeaponStats.Add(weapon.ID, new WeaponStats
                        {
                            DamageDone = 0,
                            ItemClass = weapon.ItemClass
                        });

                    TimeSpan span = peer.ShootEnd - peer.ShootStart;

                    var shots = 0;
                    var timeFromLastClick = DateTime.Now.TimeOfDay - peer.WeaponStats[weaponId].LastClick;
                    // Prevent click spam, but high leniency.
                    if (timeFromLastClick.TotalMilliseconds > weapon.RateOfFire - 100)
                    {
                        // Always round the amount of shots up.
                        shots = (int)Math.Ceiling(span.TotalMilliseconds / weapon.RateOfFire);
                        //s_log.Debug($"Estimated shots fired from {weapon.Name}: {shots}.");
                    }

                    peer.WeaponStats[weaponId].LastClick = DateTime.Now.TimeOfDay;

                    peer.IncrementShotsFired(weapon.ItemClass, weaponId, shots);
                    //s_log.Debug($"Shots ended for weapon {weapon.Name}. Shooting for {span.TotalMilliseconds}ms. Estimated shots fired: {shots}.");
                }
                // This is the part where you would put 'else' and 'unable to find weapon with {ID}.
                // Due to the nature of swapping weapons and whatnot, this happens a lot, and produces large amounts of unnecessary logging.

                // Reset values after usage
                peer.ShootEnd = new TimeSpan();
                peer.ShootStart = new TimeSpan();
                peer.ShootWeapon = 0;

                state &= ~PlayerStates.Shooting;
            }

            peer.Actor.Info.PlayerState = state;
        }

        protected override void OnIsPaused(GamePeer peer, bool on)
        {
            var state = peer.Actor.Info.PlayerState;
            if (on)
                state |= PlayerStates.Paused;
            else
                state &= ~PlayerStates.Paused;

            peer.Actor.Info.PlayerState = state;
        }
    }
}
