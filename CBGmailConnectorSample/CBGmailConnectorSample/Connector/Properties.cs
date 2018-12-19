using System;
using MG.CB.Connector.Authentication.OAuth;
using MG.CB.Connector.Properties;

namespace CBGmailConnectorSample.Connector
{
    public class Properties : ConnectorProperties<Properties>, IOAuth2Configuration
    {
        /// <summary>
        ///     Gets the PrivateKey used to decrypt secret plugin properties. The Algorithm used to do that is RSA.
        /// </summary>
        /// <returns>Return the PrivateKey used to decrypt secret plugin properties.</returns>
        protected override string GetKey()
        {
            return "<RSAKeyValue><Modulus>xNxSWbjc05m1/CZewq4OWqBPD3l1EnO5dTd/wkZIuQie/tGXcu+0fxIEFsRhm9Z27a2NZ64WWW7D57KHqr35Hp+VV71mGYGzlegAZW02a3/8vPWv/FIFIIKfrv8VCw870Pf8D7UzejHtGpIvUhNcN7tw/0XqGpbb7B3UkX3mIIU=</Modulus><Exponent>AQAB</Exponent><P>yvFiHofXDbMANaBkAZfSj2x9+kOjF/u5HeG+Y3O8hhc8yRU9KU1dPNzsYQ66m+mYSV4XcG0IGLVoIRdzQHl7yw==</P><Q>+FPczDhhaXkGSY/Vm8A5i9dXwUIh4jNo4ijng5N0V4mXFnoOrfx+n5yjw83/VKIcOHFZOAoNhtGOUXRwgXzq7w==</Q><DP>jlkr+sBLjxdccUEUhK2KiwGNh8qDjqIJYbVjRvz6Yo/QGjekk+DpInTP9PBQ4mXCZMvz4u8He9VaucNqGvJbaw==</DP><DQ>ordY45xCADmkLAmKn276hi5Ju0GZMD4diKvi3618O2vVy42ZFtpvIikiicfuecdrlHR5UKYNrPydM7SHj+GJkw==</DQ><InverseQ>sKQjkqho04tU4kPTKeN5JW1lJFhfMGCxJBOZeQ5Ol/4BzNvCfM5UdkKlyWKOyclrioj7iDSq6yD21c6dTMbSGw==</InverseQ><D>sHb7DthqgvenrVbL3OFflpdbJ3jtm9PGrC9Kw+By8gKrW6qPgwUinEGPDsWM5b/Sre3D8uuXtyVWUQy1FjXi558J4aZ58YeZytDewlc8c/B62zNFgoSnG9T8OcGpMS0ETNonLVBwSXE5+B1VS3Dxzp/LlWbIIux+6RVYOfxcwQU=</D></RSAKeyValue>";
        }
        
        [ConnectorProperty(
            Key = "ClientID",
            Name = "Client ID",
            Description = "The client identifier issued to the client during the application registration process.",
            Flags = (ConnectorPropertyFlags)((int)ConnectorPropertyFlags.AuthMask & ((int)AuthenticationType.OAuth2 << 4)),
            Namespace = "IOAuth2Configuration",
            IsEncrypted = false)]
        public string ClientId { get; set; }

        [ConnectorProperty(
            Key = "ClientSecret",
            Name = "Client Secret",
            Description = "The client secret issued to the client during the application registration process.",
            Flags = (ConnectorPropertyFlags)((int)ConnectorPropertyFlags.AuthMask &((int)AuthenticationType.OAuth2 << 4)),
            Namespace = "IOAuth2Configuration",
            IsEncrypted = true)]
        public string ClientSecret { get; set; }
        
        public string Scope
        {
            get => "https://mail.google.com/ " +
                   "https://www.googleapis.com/auth/gmail.modify " +
                   "https://www.googleapis.com/auth/gmail.metadata " +
                   "https://www.googleapis.com/auth/gmail.readonly " +
                   "https://www.googleapis.com/auth/gmail.compose " +
                   "https://www.googleapis.com/auth/gmail.insert " +
                   "https://www.googleapis.com/auth/gmail.labels " +
                   "https://www.googleapis.com/auth/gmail.send " +
                   "https://www.googleapis.com/auth/gmail.settings.basic " +
                   "https://www.googleapis.com/auth/gmail.settings.sharing";
            set => throw new NotSupportedException("Scope cannot be set.");
        }

        [ConnectorProperty(
            Key = "RedirectURL",
            Name = "Redirect URL",
            Description =
                "The Redirect URL used to redirect the client to after authentication using third-party service.",
            Flags = (ConnectorPropertyFlags)((int)ConnectorPropertyFlags.AuthMask & ((int)AuthenticationType.OAuth2 << 4)),
            IsEncrypted = false)]
        public string RedirectUrl { get; set; }

        public string AuthUrl
        {
            get => "https://accounts.google.com/o/oauth2/auth";
            set => throw new NotSupportedException("Auth URL cannot be set.");
        }

        public string AccessTokenUrl
        {
            get => "https://accounts.google.com/o/oauth2/token";
            set => throw new NotSupportedException("Access Token URL cannot be set.");
        }

        [ConnectorProperty(
           Key = "AccessToken",
           Name = "Access Token",
           Description =
               "Access token secret received from service. Can be used for further calls to the data provider.",
           Flags = ConnectorPropertyFlags.Hidden | ConnectorPropertyFlags.ConnectorCanSet,
           IsEncrypted = true)]
        public string AccessToken
        {
            get => Shadows.AccessToken;
            set => Shadows.AccessToken = value;
        }

        [ConnectorProperty(
            Key = "RefreshToken",
            Name = "RefreshToken",
            Description = "Refresh token returned by provider. Can be used for further calls to the data provider.",
            Flags = ConnectorPropertyFlags.Hidden | ConnectorPropertyFlags.ConnectorCanSet,
            IsEncrypted = true)]
        public string RefreshToken
        {
            get => Shadows.RefreshToken;
            set => Shadows.RefreshToken = value;
        }

        [ConnectorProperty(
            Key = "ExpiresAt",
            Name = "ExpiresAt",
            Description = "The token's expiration date.",
            Flags = ConnectorPropertyFlags.Hidden | ConnectorPropertyFlags.ConnectorCanSet,
            IsEncrypted = true)]
        public DateTime ExpiresAt
        {
            get => Shadows.ExpiresAt;
            set => Shadows.ExpiresAt = value;
        }

        [ConnectorProperty(
            Key = "TokenType",
            Name = "TokenType",
            Description = "The token type returned from service.",
            Flags = ConnectorPropertyFlags.Hidden | ConnectorPropertyFlags.ConnectorCanSet,
            IsEncrypted = false)]
        public string TokenType
        {
            get => Shadows.TokenType;
            set => Shadows.TokenType = value;
        }
    }
}