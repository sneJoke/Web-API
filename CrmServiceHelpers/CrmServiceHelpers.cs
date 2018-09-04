using System;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;

namespace CrmServiceHelpers
{
    public class ServerConnection
    {
        public class Configuration
        {
            public String ServerAddress;
            public String OrganizationName;
            public Uri DiscoveryUri;
            public Uri OrganizationUri;
            public Uri HomeRealmUri = null;
            public ClientCredentials DeviceCredentials = null;
            public ClientCredentials Credentials = null;
            public AuthenticationProviderType EndpointType;
            public String UserPrincipalName;
            internal IServiceManagement<IOrganizationService> OrganizationServiceManagement;
            internal SecurityTokenResponse OrganizationTokenResponse;            
            internal Int16 AuthFailureCount = 0;

            public override int GetHashCode()
            {
                int returnHashCode = this.ServerAddress.GetHashCode() 
                    ^ this.OrganizationName.GetHashCode() 
                    ^ this.EndpointType.GetHashCode();
                if (null != this.Credentials)
                {
                    if (this.EndpointType == AuthenticationProviderType.ActiveDirectory)
                        returnHashCode = returnHashCode
                            ^ this.Credentials.Windows.ClientCredential.UserName.GetHashCode()
                            ^ this.Credentials.Windows.ClientCredential.Domain.GetHashCode();
                    else if (this.EndpointType == AuthenticationProviderType.LiveId)
                        returnHashCode = returnHashCode
                            ^ this.Credentials.UserName.UserName.GetHashCode()
                            ^ this.DeviceCredentials.UserName.UserName.GetHashCode()
                            ^ this.DeviceCredentials.UserName.Password.GetHashCode();
                    else
                        returnHashCode = returnHashCode
                            ^ this.Credentials.UserName.UserName.GetHashCode();
                }
                return returnHashCode;
            }

        }

        public static OrganizationServiceProxy GetOrganizationProxy(ServerConnection.Configuration serverConfiguration)
        {
            return GetProxy<IOrganizationService, OrganizationServiceProxy>(serverConfiguration);
        }

        public virtual Configuration GetServerConfiguration(string ServerAddress, 
                                                            string OrganizationName, 
                                                            Uri DiscoveryUri, 
                                                            Uri OrganizationUri, 
                                                            string EndpointType,
                                                            string UserName,
                                                            string Domain,
                                                            String Password)
        {
            Configuration newConfig = new Configuration();
            newConfig.ServerAddress = ServerAddress;
            newConfig.OrganizationName = OrganizationName;
            newConfig.DiscoveryUri = DiscoveryUri;
            newConfig.OrganizationUri = OrganizationUri;
            newConfig.EndpointType = RetrieveAuthenticationType(EndpointType);
            newConfig.Credentials = ParseInCredentials(UserName, Domain, newConfig.EndpointType, Password);
            return newConfig;
        }

        public static ClientCredentials GetUserLogonCredentials(ServerConnection.Configuration config)
        {
            ClientCredentials credentials = new ClientCredentials();
            String userName;
            SecureString password;
            String domain;
            
            domain = config.Credentials.Windows.ClientCredential.Domain;
            userName = config.Credentials.Windows.ClientCredential.UserName;
            password = config.Credentials.Windows.ClientCredential.SecurePassword;

            credentials.Windows.ClientCredential = new System.Net.NetworkCredential(userName, password, domain);
            return credentials;
        }

        public static TProxy GetProxy<TService, TProxy>(ServerConnection.Configuration currentConfig)
            where TService : class
            where TProxy : ServiceProxy<TService>
        {
            Boolean isOrgServiceRequest = typeof(TService).Equals(typeof(IOrganizationService));
            Uri serviceUri = isOrgServiceRequest ?
                currentConfig.OrganizationUri : currentConfig.DiscoveryUri;
            IServiceManagement<TService> serviceManagement =
                (isOrgServiceRequest && null != currentConfig.OrganizationServiceManagement) ?
                (IServiceManagement<TService>)currentConfig.OrganizationServiceManagement :
                ServiceConfigurationFactory.CreateManagement<TService>(
                serviceUri);
            if (isOrgServiceRequest)
            {
                if (currentConfig.OrganizationTokenResponse == null)
                {
                    currentConfig.OrganizationServiceManagement =
                        (IServiceManagement<IOrganizationService>)serviceManagement;
                }
            }
            else
            {
                currentConfig.EndpointType = serviceManagement.AuthenticationType;
                currentConfig.Credentials = GetUserLogonCredentials(currentConfig);
            }
            AuthenticationCredentials authCredentials = new AuthenticationCredentials();
            if (!String.IsNullOrWhiteSpace(currentConfig.UserPrincipalName))
            {
                authCredentials.UserPrincipalName = currentConfig.UserPrincipalName;
            }
            else
            {
                authCredentials.ClientCredentials = currentConfig.Credentials;
            }
            Type classType;

            AuthenticationCredentials tokenCredentials = serviceManagement.Authenticate(authCredentials);
            if (isOrgServiceRequest)
            {
                currentConfig.OrganizationTokenResponse = tokenCredentials.SecurityTokenResponse;
                classType = typeof(ManagedTokenOrganizationServiceProxy);
            }
            else
            {
                classType = typeof(ManagedTokenDiscoveryServiceProxy);
            }
            return (TProxy)classType
                .GetConstructor(new Type[]
                   {
                       typeof(IServiceManagement<TService>),
                       typeof(ClientCredentials)
                   })
               .Invoke(new object[]
                   {
                       serviceManagement,
                       authCredentials.ClientCredentials
                   });
        }

        private AuthenticationProviderType RetrieveAuthenticationType(String authType)
        {
            switch (authType)
            {
                case "ActiveDirectory":
                    return AuthenticationProviderType.ActiveDirectory;
                case "LiveId":
                    return AuthenticationProviderType.LiveId;
                case "Federation":
                    return AuthenticationProviderType.Federation;
                case "OnlineFederation":
                    return AuthenticationProviderType.OnlineFederation;
                default:
                    throw new ArgumentException(String.Format("{0} is not a valid authentication type", authType));
            }
        }

        private ClientCredentials ParseInCredentials
            (string strUserName, string strDomain, AuthenticationProviderType endpointType, String strPassw)
        {
            ClientCredentials result = new ClientCredentials();
            if (strUserName != null && strPassw != null)
            {
                switch (endpointType)
                {
                    case AuthenticationProviderType.ActiveDirectory:
                        result.Windows.ClientCredential = new System.Net.NetworkCredential()
                        {
                            UserName = strUserName,
                            Domain = strDomain,
                            Password = strPassw
                        };
                        break;
                    case AuthenticationProviderType.LiveId:
                    case AuthenticationProviderType.Federation:
                    case AuthenticationProviderType.OnlineFederation:
                            result.UserName.UserName = strUserName;
                            result.UserName.Password = strPassw;
                        break;
                    default:
                        break;
                }
            }
            else
                return null;

            return result;
        }
    }

    internal sealed class Credential
    {
        private SecureString _userName;
        private SecureString _password;
        internal Credential(CREDENTIAL_STRUCT cred)
        {
            _userName = ConvertToSecureString(cred.userName);
            int size = (int)cred.credentialBlobSize;
            if (size != 0)
            {
                byte[] bpassword = new byte[size];
                Marshal.Copy(cred.credentialBlob, bpassword, 0, size);
                _password = ConvertToSecureString(Encoding.Unicode.GetString(bpassword));
            }
            else
            {
                _password = ConvertToSecureString(String.Empty);
            }
        }

        public Credential(string userName, string password)
        {
            if (String.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException("userName");
            if (String.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("password");
            _userName = ConvertToSecureString(userName);
            _password = ConvertToSecureString(password);
        }

        public string UserName
        {
            get { return ConvertToUnsecureString(_userName); }
        }

        public string Password
        {
            get { return ConvertToUnsecureString(_password); }
        }

        private string ConvertToUnsecureString(SecureString secret)
        {
            if (secret == null)
                return string.Empty;
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secret);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        private SecureString ConvertToSecureString(string secret)
        {
            if (string.IsNullOrEmpty(secret))
                return null;
            SecureString securePassword = new SecureString();
            char[] passwordChars = secret.ToCharArray();
            foreach (char pwdChar in passwordChars)
            {
                securePassword.AppendChar(pwdChar);
            }
            securePassword.MakeReadOnly();
            return securePassword;
        }
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CREDENTIAL_STRUCT
        {
            public UInt32 flags;
            public UInt32 type;
            public string targetName;
            public string comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME lastWritten;
            public UInt32 credentialBlobSize;
            public IntPtr credentialBlob;
            public UInt32 persist;
            public UInt32 attributeCount;
            public IntPtr credAttribute;
            public string targetAlias;
            public string userName;
        }    
    }
       
    internal sealed class ManagedTokenDiscoveryServiceProxy : DiscoveryServiceProxy
    {
        private AutoRefreshSecurityToken<DiscoveryServiceProxy, IDiscoveryService> _proxyManager;

        public ManagedTokenDiscoveryServiceProxy(Uri serviceUri, ClientCredentials userCredentials)
            : base(serviceUri, null, userCredentials, null)
        {
            this._proxyManager = new AutoRefreshSecurityToken<DiscoveryServiceProxy, IDiscoveryService>(this);
        }

        public ManagedTokenDiscoveryServiceProxy(IServiceManagement<IDiscoveryService> serviceManagement, 
            SecurityTokenResponse securityTokenRes)
            : base(serviceManagement, securityTokenRes)
        {
            this._proxyManager = new AutoRefreshSecurityToken<DiscoveryServiceProxy, IDiscoveryService>(this);
        }

        public ManagedTokenDiscoveryServiceProxy(IServiceManagement<IDiscoveryService> serviceManagement,
           ClientCredentials userCredentials)
            : base(serviceManagement, userCredentials)
        {
            this._proxyManager = new AutoRefreshSecurityToken<DiscoveryServiceProxy, IDiscoveryService>(this);
        }

        protected override void AuthenticateCore()
        {
            this._proxyManager.PrepareCredentials();
            base.AuthenticateCore();
        }

        protected override void ValidateAuthentication()
        {
            this._proxyManager.RenewTokenIfRequired();
            base.ValidateAuthentication();
        }
    }

    internal sealed class ManagedTokenOrganizationServiceProxy : OrganizationServiceProxy
    {
        private AutoRefreshSecurityToken<OrganizationServiceProxy, IOrganizationService> _proxyManager;

        public ManagedTokenOrganizationServiceProxy(Uri serviceUri, ClientCredentials userCredentials)
            : base(serviceUri, null, userCredentials, null)
        {
            this._proxyManager = new AutoRefreshSecurityToken<OrganizationServiceProxy, IOrganizationService>(this);
        }

        public ManagedTokenOrganizationServiceProxy(IServiceManagement<IOrganizationService> serviceManagement, 
            SecurityTokenResponse securityTokenRes)
            : base(serviceManagement, securityTokenRes)
        {
            this._proxyManager = new AutoRefreshSecurityToken<OrganizationServiceProxy, IOrganizationService>(this);
        }

        public ManagedTokenOrganizationServiceProxy(IServiceManagement<IOrganizationService> serviceManagement,
            ClientCredentials userCredentials)
            : base(serviceManagement, userCredentials)
        {
            this._proxyManager = new AutoRefreshSecurityToken<OrganizationServiceProxy, IOrganizationService>(this);
        }

        protected override void AuthenticateCore()
        {
            this._proxyManager.PrepareCredentials();
            base.AuthenticateCore();
        }

        protected override void ValidateAuthentication()
        {
            this._proxyManager.RenewTokenIfRequired();
            base.ValidateAuthentication();
        }
    }

    public sealed class AutoRefreshSecurityToken<TProxy, TService>
        where TProxy : ServiceProxy<TService>
        where TService : class
    {
        private TProxy _proxy;

        public AutoRefreshSecurityToken(TProxy proxy)
        {
            if (null == proxy)
            {
                throw new ArgumentNullException("proxy");
            }
            this._proxy = proxy;
        }

        public void PrepareCredentials()
        {
            if (null == this._proxy.ClientCredentials)
            {
                return;
            }
            switch (this._proxy.ServiceConfiguration.AuthenticationType)
            {
                case AuthenticationProviderType.ActiveDirectory:
                    this._proxy.ClientCredentials.UserName.UserName = null;
                    this._proxy.ClientCredentials.UserName.Password = null;
                    break;
                case AuthenticationProviderType.Federation:
                case AuthenticationProviderType.LiveId:
                    this._proxy.ClientCredentials.Windows.ClientCredential = null;
                    break;
                default:
                    return;
            }
        }

        public void RenewTokenIfRequired()
        {
            if (null != this._proxy.SecurityTokenResponse &&
                DateTime.UtcNow.AddMinutes(15) >= this._proxy.SecurityTokenResponse.Response.Lifetime.Expires)
                {
                try
                {
                    this._proxy.Authenticate();
                }
                catch (CommunicationException)
                {
                    if (null == this._proxy.SecurityTokenResponse ||
                        DateTime.UtcNow >= this._proxy.SecurityTokenResponse.Response.Lifetime.Expires)
                    {
                        throw;
                    }
                }
            }
        }
    }
}