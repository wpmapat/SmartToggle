using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartToggle.Models;
using SmartToggle.BusinessLogic;

namespace SmartToggle.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceBusinessLogic _serviceService;

        public ServiceController(IServiceBusinessLogic serviceService)
        {
            _serviceService = serviceService;
        }

        /// <summary>
        /// Get all services
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetAllServices()
        {
            try
            {
                var services = await _serviceService.GetAllServicesAsync();
                return Ok(services);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get service by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetServiceById(string id)
        {
            try
            {
                var service = await _serviceService.GetServiceByIdAsync(id);
                if (service == null)
                    return NotFound(new { message = "Service not found" });

                return Ok(service);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get services by company ID
        /// </summary>
        [HttpGet("company/{companyId}")]
        public async Task<ActionResult<IEnumerable<Service>>> GetServicesByCompanyId(string companyId)
        {
            try
            {
                var services = await _serviceService.GetServicesByCompanyIdAsync(companyId);
                if (services == null || !services.Any())
                    return NotFound(new { message = "No services found for this company" });

                return Ok(services);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new service
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Service>> CreateService([FromBody] Service service)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdService = await _serviceService.CreateServiceAsync(service);
                return CreatedAtAction(nameof(GetServiceById), new { id = createdService.Id }, createdService);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing service
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(string id, [FromBody] Service service)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedService = await _serviceService.UpdateServiceAsync(id, service);
                if (updatedService == null)
                    return NotFound(new { message = "Service not found" });

                return Ok(updatedService);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a service
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(string id)
        {
            try
            {
                var result = await _serviceService.DeleteServiceAsync(id);
                if (!result)
                    return NotFound(new { message = "Service not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}