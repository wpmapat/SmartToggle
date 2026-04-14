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
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyBusinessLogic _companyService;

        public CompanyController(ICompanyBusinessLogic companyService)
        {
            _companyService = companyService;
        }

        /// <summary>
        /// Get all companies
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetAllCompanies()
        {
            try
            {
                var companies = await _companyService.GetAllCompaniesAsync();
                return Ok(companies);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Get company by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetCompanyById(string id)
        {
            try
            {
                var company = await _companyService.GetCompanyByIdAsync(id);
                if (company == null)
                    return NotFound(new { message = "Company not found" });

                return Ok(company);
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
        /// Create a new company
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Company>> CreateCompany([FromBody] Company company)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdCompany = await _companyService.CreateCompanyAsync(company);
                return CreatedAtAction(nameof(GetCompanyById), new { id = createdCompany.Id }, createdCompany);
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
        /// Update an existing company
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(string id, [FromBody] Company company)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedCompany = await _companyService.UpdateCompanyAsync(id, company);
                if (updatedCompany == null)
                    return NotFound(new { message = "Company not found" });

                return Ok(updatedCompany);
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
        /// Delete a company
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(string id)
        {
            try
            {
                var result = await _companyService.DeleteCompanyAsync(id);
                if (!result)
                    return NotFound(new { message = "Company not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
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
        /// Search companies by name
        /// </summary>
        [HttpGet("search/{name}")]
        public async Task<ActionResult<IEnumerable<Company>>> SearchCompaniesByName(string name)
        {
            try
            {
                var companies = await _companyService.GetCompaniesByNameAsync(name);
                if (companies == null || !companies.Any())
                    return NotFound(new { message = "No companies found with that name" });

                return Ok(companies);
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
