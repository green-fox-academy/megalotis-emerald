using ASP.NET_Core_Webapp.Data;
using ASP.NET_Core_Webapp.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using ASP.NET_Core_Webapp.Services;
using Microsoft.EntityFrameworkCore;

namespace ASP.NET_Core_Webapp.Controllers
{
    public class BadgeController : Controller
    {
        private readonly IAuthService authService;
        private readonly IBadgeService badgeService;
        private readonly ApplicationContext applicationContext;

        public BadgeController(IAuthService authService, IBadgeService badgeService, ApplicationContext applicationContext)
        {
            this.authService = authService;
            this.badgeService = badgeService;
            this.applicationContext = applicationContext;
        }

        [Authorize("Bearer")]
        [HttpGet("mybadges")]
        public IActionResult MyBadges()
        {
            string openId = authService.GetOpenIdFromJwtToken(Request);
            //InitializeSampleDb(openId);
            return Ok(badgeService.GetMyBadges(openId));
        }

        [HttpPost("badges")]
        public IActionResult RecieveBadge([FromBody] BadgeDTO badgeDTO)
        {
            badgeService.CreateBadge(badgeDTO);
            return Created("/badges", new { message = "Success" });
        }

        public void InitializeSampleDb(string openId)
        {
            User user = applicationContext.Users.FirstOrDefault(u => u.OpenId == openId);
            UserLevel userlevel1 = new UserLevel() { BadgeLevel = new BadgeLevel() { Level = 2, Description = "I am an upper-intermediate speaker", Badge = new Badge("English speaker") }, User = user };
            UserLevel userlevel2 = new UserLevel() { BadgeLevel = new BadgeLevel() { Level = 3, Description = "I can write some working code", Badge = new Badge("Java developer") }, User = user };
            UserLevel userlevel3 = new UserLevel() { BadgeLevel = new BadgeLevel() { Level = 1, Description = "I easily freak out", Badge = new Badge("Stress management") }, User = user };
            user.UserLevels = new List<UserLevel>();
            user.UserLevels.Add(userlevel1);
            user.UserLevels.Add(userlevel2);
            user.UserLevels.Add(userlevel3);
            applicationContext.Users.Update(user);
            applicationContext.SaveChanges();
        }
    }
}
