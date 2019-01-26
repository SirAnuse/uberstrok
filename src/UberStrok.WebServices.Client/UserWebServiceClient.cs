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
