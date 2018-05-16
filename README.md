# **DEPRECATED: Use [RedBear.Auth.ServiceClient](https://github.com/RedBearSys/RedBear.Auth.ServiceClient) instead!**

# RedBear.Auth.Client
OAuth2 Client for use with our Auth server.

## Usage

### Overview

This library makes it easy to obtain an access token to Red Bear APIs for a service application.

Under the hood, the `OAuth2Client` class will create what is known as a [JWT](https://jwt.io/) and sign this with the private key (`.cer` file) that you've been given by Red Bear. Our Auth server will receive the JWT, will validate the signature using the private key's corresponding public key, and will then issue an access token.

The access token must then be included in the `Authorization` header as follows:

```http
Authorization: Bearer {access_token}
```

### Implementation Approach

The `OAuth2Client` class implements an `IOAuth2Client` interface. 

The implementation class takes an `IHttpClient` and an `IFileReader` in its constructor. The implementations of each are thin wrappers around out-of-the-box .NET Framework code (`System.Net.Http.HttpClient` and static `File` methods respectively) but the interface approach allows each to be mocked in a unit test scenario.

Any class that uses the `OAuth2Client` class will require the following `using` statements:

```csharp
using RedBear.Auth.Client;
using System;
using System.Threading.Tasks;
```

The following demonstrates use of the class:

```csharp
var oauthParams = new OAuth2Params
{
	Audience = "https://auth.rbdebug.redbearapp.io",
	AuthServerUri = new Uri("https://auth.rbdebug.redbearapp.io/auth/token"),
	CertificateFilePath = "MyCertificate.cer",
	ClientId = "MyApp",
	ClientSecret = "secret",
	Scopes = new[] { "https://rbdebug.redbearapp.io/UI" }
};

AccessToken token;

using (var fileReader = new FileReader())
using (var httpClient = new System.Net.Http.HttpClient())
{
	var client = new OAuth2Client(new RedBear.Auth.Client.HttpClient(httpClient), fileReader);
	token = await client.GetAccessTokenAsync(oauthParams);
}

Console.WriteLine($"Token: {token.Token}");
Console.WriteLine($"Expires: {token.Expires}");
Console.ReadLine();
```

Ensure you use the details provided by Red Bear in the `OAuth2Params` class.

It goes without saying that access tokens expire! The calling code will need to manage the expiry scenario: i.e. establish that the current UTC date and time is *after* `token.Expires` and therefore request a new access token. 

### Good Practice

It's good practice not to wait right until the expiry date and time to request a new access token. We advise requesting a new access token a couple of minutes before the current token's expiry. This will ensure that you don't become a victim of any clock synchronisation issues between servers.

Remember that if your service application is multi-threaded, several threads might need a new access token at once. Your service application should **only ever have a single access token at once**, so you may need to synchronise threads around the process of obtaining a new access token (e.g. have a singleton access token service class with a `lock` region inside).
