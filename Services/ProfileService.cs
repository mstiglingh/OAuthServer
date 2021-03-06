﻿using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdSrvr4Demo.Context;
using IdSrvr4Demo.Models;
using IdSrvr4Demo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdSrvr4Demo.Services
{
  public class ProfileService : IProfileService
  {
    private readonly IUserRepository _userRepository;

    public ProfileService(IUserRepository userRepository)
    {
      _userRepository = userRepository;
    }

    Task IProfileService.GetProfileDataAsync(ProfileDataRequestContext context)
    {
      // depending on the scope accessing the user data.
      if (!string.IsNullOrEmpty(context.Subject.Identity.Name))
       {
        //get user from db (in my case this is by key)
        var user = _userRepository.GetUserByKey(10);

        if (user != null)
        {
          var claims = GetUserClaims(user);

          //set issued claims to return
          context.IssuedClaims = claims.ToList();
        }
      }
      else
      {
        //get subject from context (this was set ResourceOwnerPasswordValidator.ValidateAsync),
        //where and subject was set to my user id.
        var userId = context.Subject.Claims.FirstOrDefault(x => x.Type == x.Type);

        if (!string.IsNullOrEmpty(userId?.Value) && long.Parse(userId.Value) > 0)
        {
          //get user from db (find user by user id)
          var user = _userRepository.GetUserByKey(int.Parse(userId.Value));

          // issue the claims for the user
          if (user != null)
          {
            var claims = GetUserClaims(user);
            context.IssuedClaims = claims.Where(x => context.RequestedClaimTypes.Contains(x.Type)).ToList();
          }
        }
      }

      return Task.FromResult(context);
    }

    public static List<Claim> GetUserClaims(User user)
    {
      //IEnumerable<string> roles = new List<string>() { "2", "5", "21" };
      var claims = new Claim[]
      {
            new Claim(JwtClaimTypes.Subject, user.UsersId.ToString() ?? ""),
            new Claim("unique_name", user.Username.ToString() ?? ""),
            new Claim(JwtClaimTypes.Name, user.Name),         
            new Claim(JwtClaimTypes.Profile, user.Profile  ?? ""),          
          
      };

      return claims.ToList();
    }

    Task IProfileService.IsActiveAsync(IsActiveContext context)
    {
        return Task.FromResult(context);
  
    }
  }
}
