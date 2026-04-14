using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartToggle.BusinessLogic;
using SmartToggle.Models;

namespace SmartToggle.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeatureFlagController : ControllerBase
    {
        private readonly IFeatureFlagBusinessLogic _featureFlagService;

        public FeatureFlagController(IFeatureFlagBusinessLogic featureFlagService)
        {
            _featureFlagService = featureFlagService;
        }

        /// <summary>
        /// Get all feature flags
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FeatureFlag<bool>>>> GetAllFeatureFlags()
        {
            try
            {
                var flags = await _featureFlagService.GetAllFeatureFlagsAsync();
                return Ok(flags);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Get feature flag by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<FeatureFlag<bool>>> GetFeatureFlagById(string id, [FromQuery] string serviceId)
        {
            try
            {
                var flag = await _featureFlagService.GetFeatureFlagByIdAsync(id, serviceId);
                if (flag == null)
                    return NotFound(new { message = "Feature flag not found" });

                return Ok(flag);
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

        /// <summary>
        /// Get feature flags by company ID
        /// </summary>
        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<IEnumerable<FeatureFlag<bool>>>> GetFeatureFlagsByCompanyId(string companyId)
        {
            try
            {
                var flags = await _featureFlagService.GetFeatureFlagsByCompanyIdAsync(companyId);
                if (flags == null || !flags.Any())
                    return NotFound(new { message = "No feature flags found for this company" });

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

        /// <summary>
        /// Get feature flags by service ID
        /// </summary>
        [HttpGet("service/{serviceId}")]
        public async Task<ActionResult<IEnumerable<FeatureFlag<bool>>>> GetFeatureFlagsByServiceId(string serviceId)
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

        /// <summary>
        /// Get feature flag by flag ID
        /// </summary>
        [HttpGet("flag/{flagId}")]
        public async Task<ActionResult<FeatureFlag<bool>>> GetFeatureFlagByFlagId(string flagId, [FromQuery] string serviceId)
        {
            try
            {
                var flag = await _featureFlagService.GetFeatureFlagByIdAsync(flagId, serviceId);
                if (flag == null)
                    return NotFound(new { message = "Feature flag not found" });

                return Ok(flag);
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

        /// <summary>
        /// Create a new feature flag
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FeatureFlag<bool>>> CreateFeatureFlag([FromBody] FeatureFlag<bool> featureFlag)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdFlag = await _featureFlagService.CreateFeatureFlagAsync(featureFlag);
                return CreatedAtAction(nameof(GetFeatureFlagById), new { id = createdFlag.Id }, createdFlag);
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

        /// <summary>
        /// Update an existing feature flag
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeatureFlag(string id, [FromBody] FeatureFlag<bool> featureFlag)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedFlag = await _featureFlagService.UpdateFeatureFlagAsync(id, featureFlag);
                if (updatedFlag == null)
                    return NotFound(new { message = "Feature flag not found" });

                return Ok(updatedFlag);
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

        /// <summary>
        /// Delete a feature flag
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeatureFlag(string id, [FromQuery] string serviceId)
        {
            try
            {
                var result = await _featureFlagService.DeleteFeatureFlagAsync(id, serviceId);
                if (!result)
                    return NotFound(new { message = "Feature flag not found" });

                return NoContent();
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
