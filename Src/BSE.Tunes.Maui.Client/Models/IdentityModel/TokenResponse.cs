/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * https://github.com/IdentityModel/Thinktecture.IdentityModel/blob/1a70b161dba814070d238e1fa7080f529ca040b1/source/Client.Shared/TokenResponse.cs
 */

using System.Net;
using System.Text.Json.Nodes;

namespace BSE.Tunes.Maui.Client.Models.IdentityModel
{
    public class TokenResponse
    {
        public string Raw
        {
            get; protected set;
        }
        public JsonObject Json
        {
            get; protected set;
        }

        private bool _isHttpError;
        private HttpStatusCode _httpErrorstatusCode;
        private string _httpErrorReason;

        public TokenResponse(string raw)
        {
            Raw = raw;

            Json = JsonNode.Parse(raw)?.AsObject();
        }

        public TokenResponse(HttpStatusCode statusCode, string reason)
        {
            _isHttpError = true;
            _httpErrorstatusCode = statusCode;
            _httpErrorReason = reason;
        }

        public bool IsHttpError
        {
            get
            {
                return _isHttpError;
            }
        }

        public HttpStatusCode HttpErrorStatusCode
        {
            get
            {
                return _httpErrorstatusCode;
            }
        }

        public string HttpErrorReason
        {
            get
            {
                return _httpErrorReason;
            }
        }

        public string AccessToken
        {
            get
            {
                return GetStringOrNull(OAuth2Constants.AccessToken);
            }
        }

        public string IdentityToken
        {
            get
            {
                return GetStringOrNull(OAuth2Constants.IdentityToken);
            }
        }

        public string Error
        {
            get
            {
                return GetStringOrNull(OAuth2Constants.Error);
            }
        }

        public bool IsError
        {
            get
            {
                return (IsHttpError ||
                        !string.IsNullOrWhiteSpace(GetStringOrNull(OAuth2Constants.Error)));
            }
        }

        public long ExpiresIn
        {
            get
            {
                return GetLongOrNull(OAuth2Constants.ExpiresIn);
            }
        }

        public string TokenType
        {
            get
            {
                return GetStringOrNull(OAuth2Constants.TokenType);
            }
        }

        public string RefreshToken
        {
            get
            {
                return GetStringOrNull(OAuth2Constants.RefreshToken);
            }
        }

        protected virtual string GetStringOrNull(string name)
        {
            if (Json != null && Json.TryGetPropertyValue(name, out JsonNode value))
            {
                return value?.ToString();
            }

            return null;
        }

        protected virtual long GetLongOrNull(string name)
        {
            if (Json != null && Json.TryGetPropertyValue(name, out JsonNode value))
            {
                if (long.TryParse(value?.ToString(), out long longValue))
                {
                    return longValue;
                }
            }

            return 0;
        }
    }
}
