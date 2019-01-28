﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberStrok.Core.Views;
using UberStrok.WebServices.Client;
using log4net;

namespace UberStrok.Realtime.Server.Game
{
    public class CrossServer
    {
        private byte[] authBytes;
        private string data;
        private string webServer;
        private string authToken;

        private readonly static ILog s_log = LogManager.GetLogger(nameof(GamePeerOperationHandler));

        public CrossServer(string auth)
        {
            authBytes = Convert.FromBase64String(auth);
            data = Encoding.UTF8.GetString(authBytes);
            webServer = data.Substring(0, data.IndexOf("#####"));
            authToken = auth;
        }

        public void SetStats(PlayerStatisticsView view)
        {
            s_log.Debug($"Sending player stats {authToken} to the web server {webServer}");

            // Retrieve user data from the web server.
            var client = new UserWebServiceClient(webServer);
            client.SetPlayerStats(authToken, view);
        }

        public void SetWallet(MemberWalletView view)
        {
            s_log.Debug($"Sending member wallet {authToken} to the web server {webServer}");

            // Retrieve user data from the web server.
            var client = new UserWebServiceClient(webServer);
            client.SetWallet(authToken, view);
        }

        public void Ban(DateTime expiry)
        {
            s_log.Debug($"Sending ban request {authToken} to the web server {webServer}");

            // Retrieve user data from the web server.
            var client = new UserWebServiceClient(webServer);
            client.Ban(authToken, expiry);
        }

        public void Unban()
        {
            s_log.Debug($"Sending ban request {authToken} to the web server {webServer}");

            // Retrieve user data from the web server.
            var client = new UserWebServiceClient(webServer);
            client.Unban(authToken);
        }

        public bool IsBanned()
        {
            s_log.Debug($"Sending ban inquiry {authToken} to the web server {webServer}");

            // Retrieve user data from the web server.
            var client = new UserWebServiceClient(webServer);
            return client.IsBanned(authToken);
        }

        public DateTime GetBanExpiry()
        {
            s_log.Debug($"Sending ban expiry inquiry {authToken} to the web server {webServer}");

            // Retrieve user data from the web server.
            var client = new UserWebServiceClient(webServer);
            return client.GetBanExpiry(authToken);
        }

        public ApplicationConfigurationView GetAppConfig()
        {
            s_log.Debug($"Retrieving user data {authToken} from the web server {webServer}");

            // Retrieve user data from the web server.
            var client = new UserWebServiceClient(webServer);
            var appConfig = client.GetAppConfig();
            return appConfig;
        }

        public UberstrikeUserView GetMember()
        {
            s_log.Debug($"Retrieving user data {authToken} from the web server {webServer}");

            // Retrieve user data from the web server.
            var client = new UserWebServiceClient(webServer);
            var member = client.GetMember(authToken);
            return member;
        }

        public LoadoutView GetLoadout()
        {
            s_log.Debug($"Retrieving loadout data {authToken} from the web server {webServer}");

            // Retrieve loadout data from the web server.
            var client = new UserWebServiceClient(webServer);
            var loadout = client.GetLoadout(authToken);
            return loadout;
        }
    }
}