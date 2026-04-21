using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartToggle.BusinessLogic;
using SmartToggle.Models;

namespace SmartToggle.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/public")]
    public class PublicController : ControllerBase
    {
        private readonly IFeatureFlagBusinessLogic _featureFlagService;

        public PublicController(IFeatureFlagBusinessLogic featureFlagService)
        {
            _featureFlagService = featureFlagService;
        }

        /// <summary>
        /// Get feature flags for a service — public, no auth required
        /// </summary>
        [HttpGet("featureflags/{serviceId}")]
        public async Task<ActionResult<IEnumerable<FeatureFlag<bool>>>> GetFlagsByServiceId(string serviceId)
        {
            try
            {
                var flags = await _featureFlagService.GetFeatureFlagsByServiceIdAsync(serviceId);
                if (flags == null || !flags.Any())
                    return NotFound(new { message = "No feature flags found for this service" });

                return Ok(flags);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
