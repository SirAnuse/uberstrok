﻿using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UberStrok.Core.Common;
using UberStrok.Core.Views;
using UberStrok.WebServices.Db;

namespace UberStrok.WebServices
{
    public class UserManager
    {
        private readonly static ILog s_log = LogManager.GetLogger(typeof(UserManager).Name);

        private int _nextCmid;
        private readonly UserDb _db;
        private readonly Dictionary<string, MemberView> _sessions; // AuthToken -> MemberView

        private readonly WebServiceContext _ctx;

        public UserManager(WebServiceContext ctx)
        {
            _ctx = ctx;
            _db = new UserDb();

            _sessions = new Dictionary<string, MemberView>();
            _nextCmid = _db.GetNextCmid();
            if (_nextCmid == -1)
            {
                _nextCmid = 0;
                _db.SetNextCmid(_nextCmid);
            }
        }

        public UserDb Db => _db;

        public MemberView NewMember()
        {
            var cmid = Interlocked.Increment(ref _nextCmid);
            var publicProfile = new PublicProfileView(
                cmid,
                "Player",
                MemberAccessLevel.Default,
                false,
                DateTime.UtcNow,
                EmailAddressStatus.Unverified,
                "-1"
            );

            var memberWallet = new MemberWalletView(
                cmid,
                _ctx.Configuration.Wallet.StartingCredits,
                _ctx.Configuration.Wallet.StartingPoints,
                DateTime.MaxValue,
                DateTime.MaxValue
            );

            var memberInventories = new List<ItemInventoryView>
            {
                new ItemInventoryView(1, null, -1, cmid),
                new ItemInventoryView(12, null, -1, cmid)
            };

            //TODO: Create helper function for conversion of this stuff.
            var memberItems = new List<int>();
            for (int i = 0; i < memberInventories.Count; i++)
                memberItems.Add(memberInventories[i].ItemId);

            var memberLoadout = new LoadoutView
            {
                Cmid = cmid,
                MeleeWeapon = 1,
                Weapon1 = 12
            };

            var member = new MemberView(
                publicProfile,
                memberWallet,
                memberItems
            );

            // Just set everything to zero
            // They don't have stats so it's the truth
            var playerStats = new PlayerStatisticsView(
                cmid, 0, 0, 0, 0, 0, 0,
                personalRecord: new PlayerPersonalRecordStatisticsView(),
                weaponStatistics: new PlayerWeaponStatisticsView()
                );

            // Save the member.
            Db.Profiles.Save(publicProfile);
            Db.Wallets.Save(memberWallet);
            Db.Inventories.Save(cmid, memberInventories);
            Db.Loadouts.Save(memberLoadout);
            Db.Stats.Save(playerStats);

            Db.SetNextCmid(_nextCmid);

            return member;
        }

        public MemberView GetMember(string authToken)
        {
            if (authToken == null)
                throw new ArgumentNullException(nameof(authToken));

            var member = default(MemberView);
            lock (_sessions)
            {
                if (!_sessions.TryGetValue(authToken, out member))
                    return null;
            }
            return member;
        }

        public MemberView GetMember(int cmid)
        {
            if (cmid <= 0)
                throw new ArgumentException("CMID must be greater than 0.");

            lock (_sessions)
            {
                foreach (var value in _sessions.Values)
                {
                    if (value.PublicProfile.Cmid == cmid)
                        return value;
                }
            }
            return null;
        }

        public string LogInUser(MemberView member)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            // Encode ServiceBase URL into the AuthToken so the realtime servers can figure out
            // where the user came from.
            var data = _ctx.ServiceBase + "#####" + DateTime.UtcNow.ToFileTime();
            var bytes = Encoding.UTF8.GetBytes(data);
            var authToken = Convert.ToBase64String(bytes);

            member.PublicProfile.LastLoginDate = DateTime.UtcNow;

            lock (_sessions)
            {
                foreach (var value in _sessions.Values)
                {
                    if (value.PublicProfile.Cmid == member.PublicProfile.Cmid)
                        throw new Exception("A player with the same CMID is already logged in.");
                }

                _sessions.Add(authToken, member);
            }

            // Save only profile since we only modified the profile.
            Db.Profiles.Save(member.PublicProfile);
            return authToken;
        }

        public void LogOutUser(MemberView member)
        {
            // Space
        }
    }
}
