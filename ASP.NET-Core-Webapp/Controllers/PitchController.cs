﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ASP.NET_Core_Webapp.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP.NET_Core_Webapp.Controllers
{
    [Route("api")]
    [ApiController]
    public class PitchController : ControllerBase
    {
        List<Pitch> pitches = new List<Pitch>();

        public PitchController() {
            pitches.Add(new Pitch() { BadgeName = "C pro", OldLevel = 2, PitchedLevel = 3, PitchMessage = "Hello World! My English is bloody gorgeous.", Holders = { "balazs.jozsef", "benedek.vamosi", "balazs.barna" } });
            pitches.Add(new Pitch() { BadgeName = "C++ pro", OldLevel = 2, PitchedLevel = 3, PitchMessage = "Hello World! My English is bloody gorgeous.", Holders = { "balazs.jozsef", "benedek.vamosi", "balazs.barna" } });
            pitches.Add(new Pitch() { BadgeName = "Java pro", OldLevel = 2, PitchedLevel = 3, PitchMessage = "Hello World! My English is bloody gorgeous.", Holders = { "balazs.jozsef", "benedek.vamosi", "balazs.barna" } });
            pitches.Add(new Pitch() { BadgeName = "C# pro", OldLevel = 2, PitchedLevel = 3, PitchMessage = "Hello World! My English is bloody gorgeous.", Holders = { "balazs.jozsef", "benedek.vamosi", "balazs.barna" } });
        }

        [HttpPost("pitches")]
        public IActionResult CreateNewPitch(Pitch newPitch)
        {        
            if (!pitches.Exists(e => e.BadgeName.Equals(newPitch.BadgeName)) && !newPitch.Equals(null)) {
                pitches.Add(newPitch);               
                return Created("",new { message = "Success" });
            }else {
                return Unauthorized( new { error = "Unauthorizied" });
            }                     
        }
    }
}