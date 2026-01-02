/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * https://github.com/IdentityModel/Thinktecture.IdentityModel/blob/1a70b161dba814070d238e1fa7080f529ca040b1/source/Client.Shared/OAuth2Constants.cs
 */
namespace BSE.Tunes.Maui.Client.Models.IdentityModel
{
    public static class OAuth2Constants
    {
        public const string AccessToken = "access_token";
        public const string UserName = "username";
        public const string Password = "password";
        public const string ClientId = "client_id";
        public const string ClientSecret = "client_secret";
        public const string ExpiresIn = "expires_in";
        public const string Error = "error";
        public const string IdentityToken = "id_token";
        public const string RefreshToken = "refresh_token";
        public const string UserId = "user_id";
        public const string TokenType = "token_type";
        public const string GrantType = "grant_type";

        public static class GrantTypes
        {
            public const string Password = "password";
            public const string AuthorizationCode = "authorization_code";
            public const string ClientCredentials = "client_credentials";
            public const string RefreshToken = "refresh_token";
            public const string JwtBearer = "urn:ietf:params:oauth:grant-type:jwt-bearer";
            public const string Saml2Bearer = "urn:ietf:params:oauth:grant-type:saml2-bearer";
        }
    }
}
