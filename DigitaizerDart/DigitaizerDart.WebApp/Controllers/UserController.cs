using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitaizerDart.WebApp.Models;
using DigitaizerDart.WebApp.ModelViews;
using DigitaizerDart.WebApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitaizerDart.WebApp.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {
        private readonly DataBaseContext dbContext;
        private readonly IIdFactory idFactory;

        public UserController(DataBaseContext dbContext, IIdFactory idFactory)
        {
            this.dbContext = dbContext;
            this.idFactory = idFactory;
        }

        [HttpPost("registr")]
        public async Task<IActionResult> AddUser([FromBody] RegistrationForm form)
        {
            

            User user = new User()
            {
                Name = form.Name,
                Versions = new List<Models.Version>(),
                LoginId = idFactory.IdGenerator()
            };

            await dbContext.AddAsync(user);
            await dbContext.SaveChangesAsync();
            return Json(user.LoginId);
        }

        [HttpPost("auth")]
        public IActionResult Auth([FromBody] AuthForm form)
        {

            
            var user = dbContext.Users.FirstOrDefault(x => x.LoginId == form.Login);

            if (user != null)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}