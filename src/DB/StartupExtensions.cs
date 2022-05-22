﻿using DB.Data;
using DB.Models;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using static Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults;

namespace DB;

public static class StartupExtensions
{
    public static IServiceCollection AddAuthenticationAndJwt(this IServiceCollection sc)
    {
        sc.AddAuthentication(configureOptions =>
            {
                configureOptions.DefaultAuthenticateScheme = AuthenticationScheme;
                configureOptions.DefaultChallengeScheme = AuthenticationScheme;
            })
            .AddJwtBearer(options => { options.ClaimsIssuer = AuthenticationScheme; });
        return sc;
    }
    
    public static OpenIddictBuilder AddOpenIddictServer(this IServiceCollection services, 
        IWebHostEnvironment environment)
    {
        return services
            .AddOpenIddict()
            .AddCore(options =>
            {
                options
                    .UseEntityFrameworkCore()
                    .UseDbContext<SpotifyContext>();
            })
            .AddServer(options =>
            {
                
                
                options
                    .AcceptAnonymousClients()
                    .AllowPasswordFlow()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow();

                options
                    .SetTokenEndpointUris("/api/auth/signup", "/api/auth/login", "/api/auth/client_credentials");
                
                options
                    .AddEphemeralEncryptionKey()
                    .AddEphemeralSigningKey();
                options.RegisterScopes("api", OpenIddictConstants.Scopes.OfflineAccess);
                
                
                
                var cfg = options.UseAspNetCore();
                if (environment.IsDevelopment() || environment.IsStaging())
                {
                    cfg.DisableTransportSecurityRequirement();   
                }
                
                cfg.EnableTokenEndpointPassthrough();
                
                options
                    .AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();
                options.DisableAccessTokenEncryption();
            }).AddValidation(options =>
            {
                options.UseAspNetCore();
                options.UseLocalServer();
            });
    }
    
    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services
            .AddIdentity<UserInfo, IdentityRole>()
            .AddEntityFrameworkStores<SpotifyContext>()
            .AddDefaultTokenProviders();
        
        // Configure Identity to use the same JWT claims as OpenIddict instead
        // of the legacy WS-Federation claims it uses by default (ClaimTypes),
        // which saves you from doing the mapping in your authorization controller.
        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
            options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
        });
        
        return services;
    }
}