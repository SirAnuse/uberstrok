using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using UberStrok.Core.Common;
using UberStrok.Core.Serialization;
using UberStrok.Core.Serialization.Views;
using UberStrok.Core.Views;
using UberStrok.WebServices.Contracts;

namespace UberStrok.WebServices.Core
{
    public abstract class BaseUserWebService : BaseWebService, IUserWebServiceContract
    {
        private readonly static ILog Log = LogManager.GetLogger(typeof(BaseUserWebService).Name);

        protected BaseUserWebService(WebServiceContext ctx) : base(ctx)
        {
            // Space
        }

        public abstract bool OnIsDuplicateMemberName(string username);
        public abstract MemberOperationResult OnSetLoaduout(string authToken, LoadoutView loadoutView);
        public abstract MemberOperationResult OnSetStats(string authToken, PlayerStatisticsView statsView);
        public abstract MemberOperationResult OnSetWallet(string authToken, MemberWalletView userView);
        public abstract MemberOperationResult OnBan(string authToken, DateTime expiry);
        public abstract MemberOperationResult OnUnban(string authToken);
        public abstract bool OnIsBanned(string authToken);
        public abstract DateTime OnGetBanExpiry(string authToken);
        public abstract UberstrikeUserView OnGetMember(string authToken);
        public abstract ApplicationConfigurationView OnGetAppConfig();
        public abstract LoadoutView OnGetLoadout(string authToken);
        public abstract PlayerStatisticsView OnGetPlayerStats(string authToken);
        public abstract List<ItemInventoryView> OnGetInventory(string authToken);

        byte[] IUserWebServiceContract.ChangeMemberName(byte[] data)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle ChangeMemberName request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.GenerateNonDuplicateMemberNames(byte[] data)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle GenerateNonDuplicateMemberNames request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.GetCurrentDeposits(byte[] data)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle GetCurrentDeposits request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.GetInventory(byte[] data)
        {
            try
            {
                using (var bytes = new MemoryStream(data))
                {
                    var authToken = StringProxy.Deserialize(bytes);

                    var view = OnGetInventory(authToken);
                    using (var outBytes = new MemoryStream())
                    {
                        ListProxy<ItemInventoryView>.Serialize(outBytes, view, ItemInventoryViewProxy.Serialize);
                        return outBytes.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle GetInventory request:");
                Log.Error(ex);
                return null;
            }
        }


        byte[] IUserWebServiceContract.GetItemTransactions(byte[] data)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle GetItemTransactions request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.GetLoadout(byte[] data)
        {
            try
            {
                using (var bytes = new MemoryStream(data))
                {
                    var authToken = StringProxy.Deserialize(bytes);

                    LoadoutView view = OnGetLoadout(authToken);
                    using (var outBytes = new MemoryStream())
                    {
                        LoadoutViewProxy.Serialize(outBytes, view);
                        return outBytes.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle GetLoadout request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.GetAppConfig()
        {
            try
            {
                using (var outBytes = new MemoryStream())
                {
                    var view = OnGetAppConfig();
                    ApplicationConfigurationViewProxy.Serialize(outBytes, view);
                    return outBytes.ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle GetAppConfig request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.GetMember(byte[] data)
        {
            try
            {
                using (var bytes = new MemoryStream(data))
                {
                    var authToken = StringProxy.Deserialize(bytes);

                    var view = OnGetMember(authToken);
                    using (var outBytes = new MemoryStream())
                    {
                        UberstrikeUserViewProxy.Serialize(outBytes, view);
                        return outBytes.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle GetMember request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.GetMemberListSessionData(byte[] data)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle GetMemberListSessionData request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.GetMemberSessionData(byte[] data)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle GetMemberSessionData request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.GetMemberWallet(byte[] data)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle GetMemberWallet request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.GetPointsDeposits(byte[] data)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle GetPointsDeposits request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.IsDuplicateMemberName(byte[] data)
        {
            try
            {
                using (var bytes = new MemoryStream(data))
                {
                    var username = StringProxy.Deserialize(bytes);

                    var result = OnIsDuplicateMemberName(username);
                    using (var outBytes = new MemoryStream())
                    {
                        BooleanProxy.Serialize(outBytes, result);
                        return outBytes.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle IsDuplicateMemberName request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.SetWallet(byte[] data)
        {
            try
            {
                using (var bytes = new MemoryStream(data))
                {
                    var authToken = StringProxy.Deserialize(bytes);
                    var walletView = MemberWalletViewProxy.Deserialize(bytes);

                    var result = OnSetWallet(authToken, walletView);
                    using (var outBytes = new MemoryStream())
                    {
                        EnumProxy<MemberOperationResult>.Serialize(outBytes, result);
                        return outBytes.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle SetWallet request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.IsBanned(byte[] data)
        {
            try
            {
                using (var bytes = new MemoryStream(data))
                {
                    var authToken = StringProxy.Deserialize(bytes);

                    var isBanned = OnIsBanned(authToken);
                    using (var outBytes = new MemoryStream())
                    {
                        BooleanProxy.Serialize(outBytes, isBanned);
                        return outBytes.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle IsBanned request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.GetBanExpiry(byte[] data)
        {
            try
            {
                using (var bytes = new MemoryStream(data))
                {
                    var authToken = StringProxy.Deserialize(bytes);

                    var banExpiry = OnGetBanExpiry(authToken);
                    using (var outBytes = new MemoryStream())
                    {
                        DateTimeProxy.Serialize(outBytes, banExpiry);
                        return outBytes.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle SetStats request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.Ban(byte[] data)
        {
            try
            {
                using (var bytes = new MemoryStream(data))
                {
                    var authToken = StringProxy.Deserialize(bytes);
                    var expiry = DateTimeProxy.Deserialize(bytes);

                    var result = OnBan(authToken, expiry);
                    using (var outBytes = new MemoryStream())
                    {
                        EnumProxy<MemberOperationResult>.Serialize(outBytes, result);
                        return outBytes.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle Ban request:");
                Log.Error(ex);
                return null;
            }
        }

        // Probably could've passed a boolean into the ban function instead.
        byte[] IUserWebServiceContract.Unban(byte[] data)
        {
            try
            {
                using (var bytes = new MemoryStream(data))
                {
                    var authToken = StringProxy.Deserialize(bytes);

                    var result = OnUnban(authToken);
                    using (var outBytes = new MemoryStream())
                    {
                        EnumProxy<MemberOperationResult>.Serialize(outBytes, result);
                        return outBytes.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle Ban request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.SetStats(byte[] data)
        {
            try
            {
                using (var bytes = new MemoryStream(data))
                {
                    var authToken = StringProxy.Deserialize(bytes);
                    var statsView = PlayerStatisticsViewProxy.Deserialize(bytes);

                    var result = OnSetStats(authToken, statsView);
                    using (var outBytes = new MemoryStream())
                    {
                        EnumProxy<MemberOperationResult>.Serialize(outBytes, result);
                        return outBytes.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle SetStats request:");
                Log.Error(ex);
                return null;
            }
        }

        byte[] IUserWebServiceContract.SetLoadout(byte[] data)
        {
            try
            {
                Log.Info("Handling SetLoadout request.");
                using (var bytes = new MemoryStream(data))
                {
                    var authToken = StringProxy.Deserialize(bytes);
                    var loadoutView = LoadoutViewProxy.Deserialize(bytes);

                    var result = OnSetLoaduout(authToken, loadoutView);
                    using (var outBytes = new MemoryStream())
                    {
                        EnumProxy<MemberOperationResult>.Serialize(outBytes, result);
                        return outBytes.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to handle SetLoadout request:");
                Log.Error(ex);
                return null;
            }
        }
    }
}
