﻿module DB.Tests.AuthorizationTests

open System.Collections.Generic
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Text.Json
open Microsoft.AspNetCore.Mvc.Testing
open Xunit
open DB
open DB.Tests.ResponseToken
open DB.Tests.AuthorizeUser

type ResponseError = {error: string; error_description: string; error_uri: string}

[<Fact>]
let ``Correct Sign Up returns JWT`` () =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient();
    let values = [|
        KeyValuePair<string, string>("grant_type", "password");
        KeyValuePair<string, string>("username", "Admin333@gmail.com");
        KeyValuePair<string, string>("password", "AsdQwe-123");
        KeyValuePair<string, string>("Name", "User");
        KeyValuePair<string, string>("BirthYear", "2000");
        KeyValuePair<string, string>("BirthMonth", "2");
        KeyValuePair<string, string>("BirthDay", "12");
        KeyValuePair<string, string>("Country", "Russia");
        KeyValuePair<string, string>("ProfileImg", "src1");
    |]
    let content = new FormUrlEncodedContent(values)
    content.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")
    let response = client.PostAsync($"api/auth/signup", content)
    Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
    let responseJson = response.Result.Content.ReadAsStringAsync().Result
    let responseData = JsonSerializer.Deserialize<ResponseToken> responseJson
    Assert.NotNull(responseData.access_token)
    
[<Fact>]
let ``Sign Up with wrong data return BadRequest`` () =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient();
    let values = [|
        KeyValuePair<string, string>("grant_type", "password");
        KeyValuePair<string, string>("username", "user01@gmail.com");
        KeyValuePair<string, string>("password", "qWe!123");
        KeyValuePair<string, string>("Name", "User");
        KeyValuePair<string, string>("BirthYear", "2000");
        KeyValuePair<string, string>("BirthMonth", "2");
        KeyValuePair<string, string>("BirthDay", "12");
        KeyValuePair<string, string>("Country", "Russia");
        KeyValuePair<string, string>("ProfileImg", "src1");
    |]
    let content = new FormUrlEncodedContent(values)
    content.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")
    let response = client.PostAsync($"api/auth/signup", content)
    Assert.Equal(HttpStatusCode.BadRequest, response.Result.StatusCode)
    let responseJson = response.Result.Content.ReadAsStringAsync().Result
    let responseData = JsonSerializer.Deserialize<ResponseError> responseJson
    Assert.Equal("Unable to create new user", responseData.error_description)
    
[<Fact>] 
let ``Correct Login returns JWT`` () =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient();
    let values = [|
        KeyValuePair<string, string>("grant_type", "password");
        KeyValuePair<string, string>("username", "user01@gmail.com");
        KeyValuePair<string, string>("password", "qWe!123");
    |]
    let content = new FormUrlEncodedContent(values)
    content.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")
    let response = client.PostAsync($"api/auth/login", content)
    Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
    let responseJson = response.Result.Content.ReadAsStringAsync().Result
    let responseData = JsonSerializer.Deserialize<ResponseToken> responseJson
    Assert.NotNull(responseData.access_token)

[<Fact>]
let ``Log In with wrong user name return BadRequest`` () =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient();
    let values = [|
        KeyValuePair<string, string>("grant_type", "password");
        KeyValuePair<string, string>("username", "user11@gmail.com");
        KeyValuePair<string, string>("password", "qWe!123");
    |]
    let content = new FormUrlEncodedContent(values)
    content.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")
    let response = client.PostAsync($"api/auth/login", content)
    Assert.Equal(HttpStatusCode.BadRequest, response.Result.StatusCode)
    let responseJson = response.Result.Content.ReadAsStringAsync().Result
    let responseData = JsonSerializer.Deserialize<ResponseError> responseJson
    Assert.Equal("The username/password couple is invalid.", responseData.error_description)
    
[<Fact>]
let ``Log In with wrong password return BadRequest`` () =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient();
    let values = [|
        KeyValuePair<string, string>("grant_type", "password");
        KeyValuePair<string, string>("username", "user01@gmail.com");
        KeyValuePair<string, string>("password", "qWe!123A");
    |]
    let content = new FormUrlEncodedContent(values)
    content.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")
    let response = client.PostAsync($"api/auth/login", content)
    Assert.Equal(HttpStatusCode.BadRequest, response.Result.StatusCode)
    let responseJson = response.Result.Content.ReadAsStringAsync().Result
    let responseData = JsonSerializer.Deserialize<ResponseError> responseJson
    Assert.Equal("The username/password couple is invalid.", responseData.error_description)

[<Fact>]
let ``Refresh Token returns new access and refresh token`` () =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient()
    let authResponse = AuthorizeUser
    let access_token = authResponse.access_token
    client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", access_token)
    let refresh_token = authResponse.refresh_token
    let values = [|
        KeyValuePair<string, string>("grant_type", "refresh_token");
        KeyValuePair<string, string>("refresh_token", refresh_token);
    |]
    let content = new FormUrlEncodedContent(values)
    content.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")
    let response = client.PostAsync($"api/auth/refresh_token", content)
    Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
    let responseJson = response.Result.Content.ReadAsStringAsync().Result
    let responseData = JsonSerializer.Deserialize<ResponseToken> responseJson
    Assert.NotNull(responseData.access_token)
    Assert.NotNull(responseData.refresh_token)
   
[<Fact>]
let ``Validate Token returns user's claims`` () =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient()
    let authResponse = AuthorizeUser
    let access_token = authResponse.access_token
    client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", access_token)
    let response = client.GetAsync($"api/auth/validate_token")
    Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
   