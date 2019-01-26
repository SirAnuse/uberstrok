using Photon.SocketServer;
using System;
using System.Linq;
using UberStrok.Core.Common;
using System.Collections.Generic;
using UberStrok.Core;
using UberStrok.Core.Views;
using MoreLinq;

namespace UberStrok.Realtime.Server.Game
{
    public class GamePeer : BasePeer
    {
        private readonly GamePeerEvents _events;
        private readonly StateMachine<PeerState.Id> _state;

        public GamePeer(InitRequest initRequest) : base(initRequest)
        {
            _events = new GamePeerEvents(this);

            _state = new StateMachine<PeerState.Id>();
            _state.Register(PeerState.Id.None, null);
            _state.Register(PeerState.Id.Overview, new OverviewPeerState(this));
            _state.Register(PeerState.Id.WaitingForPlayers, new WaitingForPlayersPeerState(this));
            _state.Register(PeerState.Id.Countdown, new CountdownPeerState(this));
            _state.Register(PeerState.Id.Playing, new PlayingPeerState(this));
            _state.Register(PeerState.Id.Killed, new KilledPeerState(this));

            KnownActors = new List<int>(16);
            /* Could make GamePeerOperationHandler a singleton but what ever. */
            AddOperationHandler(new GamePeerOperationHandler());
        }

        // Save stats to their corresponding json file.
        public void SaveStats(EndOfMatchDataView data)
        {
            PlayerStatisticsView stats = Web.GetMember().UberstrikeMemberView.PlayerStatisticsView;
            stats.TimeSpentInGame += data.TimeInGameMinutes;
            foreach (var i in data.PlayerXpEarned)
                stats.Xp += i.Value;
            #region OH GOD TAKE IT AWAY
            /* MACHINEGUN STATS */
            stats.WeaponStatistics.MachineGunTotalDamageDone += data.PlayerStatsTotal.MachineGunDamageDone;
            stats.WeaponStatistics.MachineGunTotalSplats += data.PlayerStatsTotal.MachineGunKills;
            stats.WeaponStatistics.MachineGunTotalShotsFired += data.PlayerStatsTotal.MachineGunShotsFired;
            stats.WeaponStatistics.MachineGunTotalShotsHit += data.PlayerStatsTotal.MachineGunShotsHit;
            /* SHOTGUN STATS */
            stats.WeaponStatistics.ShotgunTotalDamageDone += data.PlayerStatsTotal.ShotgunDamageDone;
            stats.WeaponStatistics.ShotgunTotalSplats += data.PlayerStatsTotal.ShotgunSplats;
            stats.WeaponStatistics.ShotgunTotalShotsFired += data.PlayerStatsTotal.ShotgunShotsFired;
            stats.WeaponStatistics.ShotgunTotalShotsHit += data.PlayerStatsTotal.ShotgunShotsHit;
            /* SPLATTERGUN STATS */
            stats.WeaponStatistics.SplattergunTotalDamageDone += data.PlayerStatsTotal.SplattergunDamageDone;
            stats.WeaponStatistics.SplattergunTotalSplats += data.PlayerStatsTotal.SplattergunKills;
            stats.WeaponStatistics.SplattergunTotalShotsFired += data.PlayerStatsTotal.SplattergunShotsFired;
            stats.WeaponStatistics.SplattergunTotalShotsHit += data.PlayerStatsTotal.SplattergunShotsHit;
            /* SNIPERRIFLE STATS */
            stats.WeaponStatistics.SniperTotalDamageDone += data.PlayerStatsTotal.SniperDamageDone;
            stats.WeaponStatistics.SniperTotalSplats += data.PlayerStatsTotal.SniperKills;
            stats.WeaponStatistics.SniperTotalShotsFired += data.PlayerStatsTotal.SniperShotsFired;
            stats.WeaponStatistics.SniperTotalShotsHit += data.PlayerStatsTotal.SniperShotsHit;
            /* MELEE STATS */
            stats.WeaponStatistics.MeleeTotalDamageDone += data.PlayerStatsTotal.MeleeDamageDone;
            stats.WeaponStatistics.MeleeTotalSplats += data.PlayerStatsTotal.MeleeKills;
            stats.WeaponStatistics.MeleeTotalShotsFired += data.PlayerStatsTotal.MeleeShotsFired;
            stats.WeaponStatistics.MeleeTotalShotsHit += data.PlayerStatsTotal.MeleeShotsHit;
            /* CANNON STATS */
            stats.WeaponStatistics.CannonTotalDamageDone += data.PlayerStatsTotal.CannonDamageDone;
            stats.WeaponStatistics.CannonTotalSplats += data.PlayerStatsTotal.CannonKills;
            stats.WeaponStatistics.CannonTotalShotsFired += data.PlayerStatsTotal.CannonShotsFired;
            stats.WeaponStatistics.CannonTotalShotsHit += data.PlayerStatsTotal.CannonShotsHit;
            /* LAUNCHER STATS */
            stats.WeaponStatistics.LauncherTotalDamageDone += data.PlayerStatsTotal.LauncherDamageDone;
            stats.WeaponStatistics.LauncherTotalSplats += data.PlayerStatsTotal.LauncherKills;
            stats.WeaponStatistics.LauncherTotalShotsFired += data.PlayerStatsTotal.LauncherShotsFired;
            stats.WeaponStatistics.LauncherTotalShotsHit += data.PlayerStatsTotal.LauncherShotsHit;

            var best = data.PlayerStatsBestPerLife;
            // Yes, I could've used something like the following:
            // stats.PersonalRecord.MostArmorPickedUp = stats.PersonalRecord.MostArmorPickedUp < best.ArmorPickedUp ? stats.PersonalRecord.MostArmorPickedUp : best.ArmorPickedUp;
            // But the question is, should I have?
            if (stats.PersonalRecord.MostArmorPickedUp < best.ArmorPickedUp)
                stats.PersonalRecord.MostArmorPickedUp = best.ArmorPickedUp;
            if (stats.PersonalRecord.MostHealthPickedUp < best.HealthPickedUp)
                stats.PersonalRecord.MostHealthPickedUp = best.HealthPickedUp;
            if (stats.PersonalRecord.MostMachinegunSplats < best.MachineGunKills)
                stats.PersonalRecord.MostMachinegunSplats = best.MachineGunKills;
            if (stats.PersonalRecord.MostShotgunSplats < best.ShotgunSplats)
                stats.PersonalRecord.MostShotgunSplats = best.ShotgunSplats;
            if (stats.PersonalRecord.MostSplattergunSplats < best.SplattergunKills)
                stats.PersonalRecord.MostSplattergunSplats = best.SplattergunKills;
            if (stats.PersonalRecord.MostSniperSplats < best.SniperKills)
                stats.PersonalRecord.MostSniperSplats = best.SniperKills;
            if (stats.PersonalRecord.MostMeleeSplats < best.MeleeKills)
                stats.PersonalRecord.MostMeleeSplats = best.MeleeKills;
            if (stats.PersonalRecord.MostCannonSplats < best.CannonKills)
                stats.PersonalRecord.MostCannonSplats = best.CannonKills;
            if (stats.PersonalRecord.MostLauncherSplats < best.LauncherKills)
                stats.PersonalRecord.MostLauncherSplats = best.LauncherKills;
            if (stats.PersonalRecord.MostDamageDealt < best.GetDamageDealt())
                stats.PersonalRecord.MostDamageDealt = best.GetDamageDealt();
            if (stats.PersonalRecord.MostDamageReceived < best.DamageReceived)
                stats.PersonalRecord.MostDamageReceived = best.DamageReceived;
            if (stats.PersonalRecord.MostHeadshots < best.Headshots)
                stats.PersonalRecord.MostHeadshots = best.Headshots;
            if (stats.PersonalRecord.MostNutshots < best.Nutshots)
                stats.PersonalRecord.MostNutshots = best.Nutshots;
            if (stats.PersonalRecord.MostSplats < best.GetKills())
                stats.PersonalRecord.MostSplats = best.GetKills();
            if (stats.PersonalRecord.MostXPEarned < best.Xp)
                stats.PersonalRecord.MostXPEarned = best.Xp;
            if (stats.PersonalRecord.MostConsecutiveSnipes < best.ConsecutiveSnipes)
                stats.PersonalRecord.MostConsecutiveSnipes = best.ConsecutiveSnipes;
            #endregion

            Web.SetStats(stats);
        }

        public void IncrementShotsFired(UberStrikeItemClass itemClass, int weaponId, int shots)
        {
            if (WeaponStats.ContainsKey(weaponId))
                WeaponStats[weaponId].ShotsFired += shots;
            else
            {
                WeaponStats.Add(weaponId, new WeaponStats());
                WeaponStats[weaponId].ShotsFired += shots;
                WeaponStats[weaponId].ItemClass = itemClass;
            }
            switch (itemClass)
            {
                case UberStrikeItemClass.WeaponShotgun:
                    TotalStats.ShotgunShotsFired += shots;
                    CurrentLifeStats.ShotgunShotsFired += shots;
                    break;
                case UberStrikeItemClass.WeaponSniperRifle:
                    TotalStats.SniperShotsFired += shots;
                    CurrentLifeStats.SniperShotsFired += shots;
                    break;
                case UberStrikeItemClass.WeaponSplattergun:
                    TotalStats.SplattergunShotsFired += shots;
                    CurrentLifeStats.SplattergunShotsFired += shots;
                    break;
                case UberStrikeItemClass.WeaponMelee:
                    TotalStats.MeleeShotsFired += shots;
                    CurrentLifeStats.MeleeShotsFired += shots;
                    break;
                case UberStrikeItemClass.WeaponMachinegun:
                    TotalStats.MachineGunShotsFired += shots;
                    CurrentLifeStats.MachineGunShotsFired += shots;
                    break;
                case UberStrikeItemClass.WeaponLauncher:
                    TotalStats.LauncherShotsFired += shots;
                    CurrentLifeStats.LauncherShotsFired += shots;
                    break;
                case UberStrikeItemClass.WeaponCannon:
                    TotalStats.CannonShotsFired += shots;
                    CurrentLifeStats.CannonShotsFired += shots;
                    break;
            }
        }

        public void IncrementDamageDone(UberStrikeItemClass itemClass, int weaponId, int dmg)
        {
            if (WeaponStats.ContainsKey(weaponId))
                WeaponStats[weaponId].DamageDone += dmg;
            else
            {
                WeaponStats.Add(weaponId, new WeaponStats());
                WeaponStats[weaponId].DamageDone += dmg;
                WeaponStats[weaponId].ItemClass = itemClass;
            }
            switch (itemClass)
            {
                case UberStrikeItemClass.WeaponShotgun:
                    TotalStats.ShotgunDamageDone += dmg;
                    CurrentLifeStats.ShotgunDamageDone += dmg;
                    break;
                case UberStrikeItemClass.WeaponSniperRifle:
                    TotalStats.SniperDamageDone += dmg;
                    CurrentLifeStats.SniperDamageDone += dmg;
                    break;
                case UberStrikeItemClass.WeaponSplattergun:
                    TotalStats.SplattergunDamageDone += dmg;
                    CurrentLifeStats.SplattergunDamageDone += dmg;
                    break;
                case UberStrikeItemClass.WeaponMelee:
                    TotalStats.MeleeDamageDone += dmg;
                    CurrentLifeStats.MeleeDamageDone += dmg;
                    break;
                case UberStrikeItemClass.WeaponMachinegun:
                    TotalStats.MachineGunDamageDone += dmg;
                    CurrentLifeStats.MachineGunDamageDone += dmg;
                    break;
                case UberStrikeItemClass.WeaponLauncher:
                    TotalStats.LauncherDamageDone += dmg;
                    CurrentLifeStats.LauncherDamageDone += dmg;
                    break;
                case UberStrikeItemClass.WeaponCannon:
                    TotalStats.CannonDamageDone += dmg;
                    CurrentLifeStats.CannonDamageDone += dmg;
                    break;
            }
        }

        public void IncrementShotsHit(UberStrikeItemClass itemClass, int weaponId)
        {
            if (WeaponStats.ContainsKey(weaponId))
                WeaponStats[weaponId].ShotsHit++;
            else
            {
                WeaponStats.Add(weaponId, new WeaponStats());
                WeaponStats[weaponId].ShotsHit++;
                WeaponStats[weaponId].ItemClass = itemClass;
            }

            switch (itemClass)
            {
                case UberStrikeItemClass.WeaponShotgun:
                    TotalStats.ShotgunShotsHit++;
                    CurrentLifeStats.ShotgunShotsHit++;
                    break;
                case UberStrikeItemClass.WeaponSniperRifle:
                    TotalStats.SniperShotsHit++;
                    CurrentLifeStats.SniperShotsHit++;
                    break;
                case UberStrikeItemClass.WeaponSplattergun:
                    TotalStats.SplattergunShotsHit++;
                    CurrentLifeStats.SplattergunShotsHit++;
                    break;
                case UberStrikeItemClass.WeaponMelee:
                    TotalStats.MeleeShotsHit++;
                    CurrentLifeStats.MeleeShotsHit++;
                    break;
                case UberStrikeItemClass.WeaponMachinegun:
                    TotalStats.MachineGunShotsHit++;
                    CurrentLifeStats.MachineGunShotsHit++;
                    break;
                case UberStrikeItemClass.WeaponLauncher:
                    TotalStats.LauncherShotsHit++;
                    CurrentLifeStats.LauncherShotsHit++;
                    break;
                case UberStrikeItemClass.WeaponCannon:
                    TotalStats.CannonShotsHit++;
                    CurrentLifeStats.CannonShotsHit++;
                    break;
            }
        }

        public void IncrementPowerUp(PickupItemType itemType, int amount)
        {
            switch (itemType)
            {
                case PickupItemType.Armor:
                    TotalStats.ArmorPickedUp += amount;
                    CurrentLifeStats.ArmorPickedUp += amount;
                    break;
                case PickupItemType.Health:
                    TotalStats.HealthPickedUp += amount;
                    CurrentLifeStats.HealthPickedUp += amount;
                    break;
            }
        }

        public void IncrementKills(UberStrikeItemClass itemClass)
        {

            switch (itemClass)
            {
                case UberStrikeItemClass.WeaponShotgun:
                    TotalStats.ShotgunSplats++;
                    CurrentLifeStats.ShotgunSplats++;
                    break;
                case UberStrikeItemClass.WeaponSniperRifle:
                    TotalStats.SniperKills++;
                    CurrentLifeStats.SniperKills++;
                    break;
                case UberStrikeItemClass.WeaponSplattergun:
                    TotalStats.SplattergunKills++;
                    CurrentLifeStats.SplattergunKills++;
                    break;
                case UberStrikeItemClass.WeaponMelee:
                    TotalStats.MeleeKills++;
                    CurrentLifeStats.MeleeKills++;
                    break;
                case UberStrikeItemClass.WeaponMachinegun:
                    TotalStats.MachineGunKills++;
                    CurrentLifeStats.MachineGunKills++;
                    break;
                case UberStrikeItemClass.WeaponLauncher:
                    TotalStats.LauncherKills++;
                    CurrentLifeStats.LauncherKills++;
                    break;
                case UberStrikeItemClass.WeaponCannon:
                    TotalStats.CannonKills++;
                    CurrentLifeStats.CannonKills++;
                    break;
            }
        }

        public EndOfMatchDataView GetStats(bool hasWon, string matchGuid, List<StatsSummaryView> MVPs)
        {
            EndOfMatchDataView ret = new EndOfMatchDataView();
            ret.HasWonMatch = hasWon;
            ret.MatchGuid = matchGuid;
            ret.MostEffecientWeaponId = GetMostEfficientWeaponId();
            ret.PlayerStatsTotal = TotalStats;
            ret.PlayerStatsBestPerLife = GetBestPerLifeStats();
            ret.PlayerXpEarned = CalculateXp();
            ret.MostValuablePlayers = MVPs;
            ret.TimeInGameMinutes = Room.View.TimeLimit;
            return ret;
        }

        public Dictionary<byte, ushort> CalculateXp()
        {
            // TODO: Calculate XP earned.
            Dictionary<byte, ushort> ret = new Dictionary<byte, ushort>
            {
                { 1, 10 },
                { 2, 20 },
                { 3, 30 },
                { 4, 40 },
                { 5, 50 },
                { 6, 60 }
            };
            return ret;
        }

        public StatsCollectionView GetBestPerLifeStats()
        {
            StatsCollectionView ret = new StatsCollectionView();
            StatsPerLife.Add(CurrentLifeStats);
            ret.ArmorPickedUp = StatsPerLife.Max(x => x.ArmorPickedUp);
            ret.HealthPickedUp = StatsPerLife.Max(x => x.HealthPickedUp);
            ret.CannonKills = StatsPerLife.Max(x => x.CannonKills);
            ret.CannonDamageDone = StatsPerLife.Max(x => x.CannonDamageDone);
            ret.CannonShotsFired = StatsPerLife.Max(x => x.CannonShotsFired);
            ret.CannonShotsHit = StatsPerLife.Max(x => x.CannonShotsHit);
            ret.LauncherKills = StatsPerLife.Max(x => x.LauncherKills);
            ret.LauncherDamageDone = StatsPerLife.Max(x => x.LauncherDamageDone);
            ret.LauncherShotsFired = StatsPerLife.Max(x => x.LauncherShotsFired);
            ret.LauncherShotsHit = StatsPerLife.Max(x => x.LauncherShotsHit);
            ret.SniperKills = StatsPerLife.Max(x => x.SniperKills);
            ret.SniperDamageDone = StatsPerLife.Max(x => x.SniperDamageDone);
            ret.SniperShotsFired = StatsPerLife.Max(x => x.SniperShotsFired);
            ret.SniperShotsHit = StatsPerLife.Max(x => x.SniperShotsHit);
            ret.MachineGunKills = StatsPerLife.Max(x => x.MachineGunKills);
            ret.MachineGunDamageDone = StatsPerLife.Max(x => x.MachineGunDamageDone);
            ret.MachineGunShotsFired = StatsPerLife.Max(x => x.MachineGunShotsFired);
            ret.MachineGunShotsHit = StatsPerLife.Max(x => x.MachineGunShotsHit);
            ret.SplattergunKills = StatsPerLife.Max(x => x.SplattergunKills);
            ret.SplattergunDamageDone = StatsPerLife.Max(x => x.SplattergunDamageDone);
            ret.SplattergunShotsFired = StatsPerLife.Max(x => x.SplattergunShotsFired);
            ret.SplattergunShotsHit = StatsPerLife.Max(x => x.SplattergunShotsHit);
            ret.ShotgunSplats = StatsPerLife.Max(x => x.ShotgunSplats);
            ret.ShotgunDamageDone = StatsPerLife.Max(x => x.ShotgunDamageDone);
            ret.ShotgunShotsFired = StatsPerLife.Max(x => x.ShotgunShotsFired);
            ret.ShotgunShotsHit = StatsPerLife.Max(x => x.ShotgunShotsHit);
            ret.MeleeKills = StatsPerLife.Max(x => x.MeleeKills);
            ret.MeleeDamageDone = StatsPerLife.Max(x => x.MeleeDamageDone);
            ret.MeleeShotsFired = StatsPerLife.Max(x => x.MeleeShotsFired);
            ret.MeleeShotsHit = StatsPerLife.Max(x => x.MeleeShotsHit);
            ret.Headshots = StatsPerLife.Max(x => x.Headshots);
            ret.Nutshots = StatsPerLife.Max(x => x.Nutshots);
            ret.Points = StatsPerLife.Max(x => x.Points);
            // idk how they somehow gonna die twice or something
            // it could be .Max but makes more sense to .Min so eh
            ret.Deaths = StatsPerLife.Min(x => x.Deaths);
            ret.Suicides = StatsPerLife.Min(x => x.Suicides);
            return ret;
        }

        public int GetMostEfficientWeaponId()
        {
            // Get the weapon with highest efficiency score.
            // Efficiency score: Accuracy * Damage
            if (WeaponStats.Count > 0)
                return WeaponStats.MaxBy(x => (x.Value.ShotsHit * x.Value.ShotsFired) * x.Value.DamageDone).Key;
            else
                // Splatbat weapon ID. Placeholder.
                return 1;
        }

        public TimeSpan ShootStart;
        public TimeSpan ShootEnd;
        public bool IsShooting;
        public int ShootWeapon;

        public TimeSpan lastKillTime;
        public int killCounter;

        public StatsCollectionView TotalStats { get; set; }
        public StatsCollectionView CurrentLifeStats { get; set; }
        public List<StatsCollectionView> StatsPerLife { get; set; }
        // weapon id, stats
        public Dictionary<int, WeaponStats> WeaponStats { get; set; }

        public string AuthToken { get; set; }
        public ushort Ping { get; set; }
        public GameActor Actor { get; set; }
        public CrossServer Web { get; set; }

        /* TODO: Not really sure if we need this. But might want to turn it into a HashSet. */
        public List<int> KnownActors { get; set; }
        public BaseGameRoom Room { get; set; }
        public LoadoutView Loadout { get; set; }
        public UberstrikeUserView Member { get; set; }

        public GamePeerEvents Events => _events;
        public StateMachine<PeerState.Id> State => _state;
    }
}
