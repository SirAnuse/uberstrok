using System;
using System.IO;
using UberStrok.Core.Serialization;
using UberStrok.Core.Serialization.Views;
using UberStrok.Core.Views;
using UberStrok.WebServices.Contracts;

namespace UberStrok.WebServices.Client
{
    public class UserWebServiceClient : BaseWebServiceClient<IUserWebServiceContract>
    {
        public UserWebServiceClient(string endPoint) : base(endPoint, "UserWebService")
        {
            // Space
        }

        public void SetPlayerStats(string authToken, PlayerStatisticsView statsView)
        {
            using (var bytes = new MemoryStream())
            {
                StringProxy.Serialize(bytes, authToken);
                PlayerStatisticsViewProxy.Serialize(bytes, statsView);

                var data = Channel.SetStats(bytes.ToArray());
            }
        }

        public void SetWallet(string authToken, MemberWalletView walletView)
        {
            using (var bytes = new MemoryStream())
            {
                StringProxy.Serialize(bytes, authToken);
                MemberWalletViewProxy.Serialize(bytes, walletView);

                var data = Channel.SetWallet(bytes.ToArray());
            }
        }

        public void Ban(string authToken, DateTime expiry)
        {
            using (var bytes = new MemoryStream())
            {
                StringProxy.Serialize(bytes, authToken);
                DateTimeProxy.Serialize(bytes, expiry);

                var data = Channel.Ban(bytes.ToArray());
            }
        }

        public void Unban(string authToken)
        {
            using (var bytes = new MemoryStream())
            {
                StringProxy.Serialize(bytes, authToken);

                var data = Channel.Unban(bytes.ToArray());
            }
        }

        public bool IsBanned(string authToken)
        {
            using (var bytes = new MemoryStream())
            {
                StringProxy.Serialize(bytes, authToken);
                var data = Channel.IsBanned(bytes.ToArray());
                using (var inBytes = new MemoryStream(data))
                    return BooleanProxy.Deserialize(inBytes);
            }
        }

        public DateTime GetBanExpiry(string authToken)
        {
            using (var bytes = new MemoryStream())
            {
                StringProxy.Serialize(bytes, authToken);
                var data = Channel.GetBanExpiry(bytes.ToArray());
                using (var inBytes = new MemoryStream(data))
                    return DateTimeProxy.Deserialize(inBytes);
            }
        }

        public ApplicationConfigurationView GetAppConfig()
        {
            using (var bytes = new MemoryStream())
            {
                var data = Channel.GetAppConfig();
                using (var inBytes = new MemoryStream(data))
                    return ApplicationConfigurationViewProxy.Deserialize(inBytes);
            }
        }

        public UberstrikeUserView GetMember(string authToken)
        {
            using (var bytes = new MemoryStream())
            {
                StringProxy.Serialize(bytes, authToken);

                var data = Channel.GetMember(bytes.ToArray());
                using (var inBytes = new MemoryStream(data))
                    return UberstrikeUserViewProxy.Deserialize(inBytes);
            }
        }

        public LoadoutView GetLoadout(string authToken)
        {
            using (var bytes = new MemoryStream())
            {
                StringProxy.Serialize(bytes, authToken);

                var data = Channel.GetLoadout(bytes.ToArray());
                using (var inBytes = new MemoryStream(data))
                    return LoadoutViewProxy.Deserialize(inBytes);
            }
        }
    }
}
