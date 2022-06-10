﻿module DB.Tests.ProfileTests

open System.Collections.Generic
open System.Net
open System.Net.Http
open DB.Models
open Microsoft.AspNetCore.Mvc.Testing
open System.Text.Json
open Xunit
open DB.Data.Repository
open DB
open DB.Models.EnumTypes
open GetResponse
open Microsoft.AspNetCore.Identity

type responseNotFound = { error: string }
type profile = {userId:string; username:string; birthday:string; email:string; country:Country}
type responseProfile = List<profile>
    
[<Theory>]
[<InlineData("5f34130c-2ed9-4c83-a600-e474e8f48bac")>]
[<InlineData("ca2aa01b-a215-4611-838a-f11b9552103e")>]
[<InlineData("5f34130c-2ed9-4c83-a600-e474e8f43bac")>]
let ``Search Profile returns Profile``(userId: string) =
    let path = $"/api/profile/getProfile?userId={userId}"
    let response = getResponseAsync path
    Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
    let responseJson = response.Result.Content.ReadAsStringAsync().Result
    let responseData = JsonSerializer.Deserialize<DB.Models.Profile> responseJson
    Assert.NotNull(responseData)
    
    
[<Theory>]
[<InlineData("NotExistingName")>]
[<InlineData("qwerty123456")>]
[<InlineData("")>]
let ``Search Profile return Not Found``(userId: string) =
    let path = $"/api/profile/getProfile?input={userId}"
    let response = getResponseAsync path
    Assert.Equal(HttpStatusCode.BadRequest, response.Result.StatusCode)
    
    
[<Fact>]
let ``Change Profile returns Profile``() =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient();
    let values = [|
        KeyValuePair<string, string>("userId", "5f34130c-2ed9-4c83-a600-e474e8f44bac");
        KeyValuePair<string, string>("username", "user04@gmail.com");
        KeyValuePair<string, string>("birthday", "2000.01.01");
        KeyValuePair<string, string>("email", "user04@gmail.com");
        KeyValuePair<string, string>("country", "Greece");
    |]
    let content = new FormUrlEncodedContent(values)
    let response = client.PutAsync($"/api/profile/changeProfile", content)
    Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
    
    
[<Fact>]
let ``Change Profile returns NotFound``() =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient();
    let values = [|
        KeyValuePair<string, string>("userId", "noProfile");
        KeyValuePair<string, string>("username", "user01@gmail.com");
        KeyValuePair<string, string>("birthday", "2000.01.01");
        KeyValuePair<string, string>("email", "user01@gmail.com");
        KeyValuePair<string, string>("country", "Greece");
    |]
    let content = new FormUrlEncodedContent(values)
    
    let response = client.PutAsync($"/api/profile/changeProfile", content)
    Assert.Equal(HttpStatusCode.NotFound, response.Result.StatusCode)
    
    
[<Fact>]
let ``Change Password returns Success``() =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient();
    let values = [|
        KeyValuePair<string, string>("userId", "5f34130c-2ed9-4c83-a600-e474e8f44bac");
        KeyValuePair<string, string>("oldPassword", "qWe!123");
        KeyValuePair<string, string>("newPassword", "newqWe!123");
    |]
    let content = new FormUrlEncodedContent(values)
    
    let response = client.PostAsync($"/api/profile/changePassword", content)
    Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
    

[<Fact>]
let ``Change Password returns Password Wrong``() =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient();
    let values = [|
        KeyValuePair<string, string>("userId", "5f34130c-2ed9-4c83-a600-e474e8f44bac");
        KeyValuePair<string, string>("oldPassword", "wrondPassword");
        KeyValuePair<string, string>("newPassword", "123");
    |]
    let content = new FormUrlEncodedContent(values)
    let response = client.PostAsync($"/api/profile/changePassword", content)
    Assert.Equal(HttpStatusCode.BadRequest, response.Result.StatusCode)
    
[<Fact>]
let ``Change Password returns NotFound``() =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient();
    let values = [|
        KeyValuePair<string, string>("userId", "123");
        KeyValuePair<string, string>("oldPassword", "qWe!123");
        KeyValuePair<string, string>("newPassword", "newqWe!123");
    |]
    let content = new FormUrlEncodedContent(values)
    
    let response = client.PostAsync($"/api/profile/changePassword", content)
    Assert.Equal(HttpStatusCode.NotFound, response.Result.StatusCode)
    
    
[<Fact>]
let ``Change Premium returns Success``() =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient()
    let values = [|
        KeyValuePair<string, string>("userId", "5f34130c-2ed9-4c83-a600-e474e8f48bac");
        KeyValuePair<string, string>("premiumId", "1");
    |]
    let content = new FormUrlEncodedContent(values)
    let response = client.PostAsync($"/api/profile/changePremium", content)
    Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
    
    
[<Fact>]
let ``Change Premium returns Already such a Premium``() =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient()
    let values = [|
        KeyValuePair<string, string>("userId", "120877ed-84b9-4ed5-9b87-d78965fc4fe0");
        KeyValuePair<string, string>("premiumType", "Individual");
    |]
    
    let content = new FormUrlEncodedContent(values)
    let response = client.PostAsync($"/api/profile/changePremium", content)
    Assert.Equal(HttpStatusCode.BadRequest, response.Result.StatusCode)
    
    
[<Fact>]
let ``Change Premium returns NotFound``() =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient()
    let values = [|
        KeyValuePair<string, string>("userId", "noUser");
        KeyValuePair<string, string>("premiumType", "Duo");
    |]
    let content = new FormUrlEncodedContent(values)
    let response = client.PostAsync($"/api/profile/changePremium", content)
    Assert.Equal(HttpStatusCode.NotFound, response.Result.StatusCode)
  
    
[<Fact>]
let ``Get Premiums returns Premiums``() =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient()
    let response = client.GetAsync($"/api/profile/premiums")
    Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
    
[<Fact>]
let ``Get user Premium returns Premium``() =
    let _factory = new WebApplicationFactory<Startup>()
    let client = _factory.CreateClient()
    let response = client.GetAsync($"/api/profile/user_premium/120877ed-84b9-4ed5-9b87-d78965fc4fe0")
    Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)