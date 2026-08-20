using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniTradeWebApi.Models;
using OmniTradeWebApi.Repositories;
using System.Security.Claims;

namespace OmniTradeWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorsController : ControllerBase
    {
        private readonly IVendorRepository _vendorRepository;

        public VendorsController(IVendorRepository vendorRepository)
        {
            _vendorRepository = vendorRepository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Vendor>> GetVendor(int id)
        {
            var vendor = await _vendorRepository.GetVendorByIdAsync(id);

            if (vendor == null)
            {
                return NotFound();
            }

            return Ok(vendor);
        }

        [HttpPost("register")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> RegisterVendor(Vendor vendor)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var existingVendor = await _vendorRepository
                .GetVendorByUserIdAsync(userId);

            if (existingVendor != null)
            {
                return BadRequest("Vendor profile already exists for this user.");
            }

            var newVendor = new Vendor
            {
                UserId = userId,
                StoreName = vendor.StoreName,
                Description = vendor.Description,
                ContactEmail = vendor.ContactEmail,
                IsApproved = false
            };

            await _vendorRepository.RegisterVendorProfileAsync(newVendor);

            return StatusCode(201, new
            {
                message = "Vendor profile registered successfully.",
                vendorId = newVendor.Id
            });
        }

        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ApproveVendor(int id, [FromBody] bool isApproved)
        {
            var vendor = await _vendorRepository.GetVendorByIdAsync(id);

            if (vendor == null)
            {
                return NotFound("Vendor not found.");
            }

            await _vendorRepository.UpdateVendorApprovalAsync(id, isApproved);

            return Ok(new
            {
                message = isApproved
                    ? "Vendor approved successfully."
                    : "Vendor suspended successfully."
            });
        }

        [HttpGet("dashboard")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> GetVendorDashboard()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var vendor = await _vendorRepository.GetVendorByUserIdAsync(userId);

            if (vendor == null)
            {
                return NotFound("Vendor profile not found.");
            }

            var products = await _productRepository.GetAllProductsAsync();

            var dashboard = new
            {
                vendorId = vendor.Id,
                storeName = vendor.StoreName,
                totalProducts = products.Count(p => p.VendorId == vendor.Id),
                approvedProducts = products.Count(p => p.VendorId == vendor.Id && p.IsActive),
                pendingProducts = products.Count(p => p.VendorId == vendor.Id && !p.IsActive),
                totalStock = products.Where(p => p.VendorId == vendor.Id).Sum(p => p.StockQuantity),
                isApproved = vendor.IsApproved.Value
            };

            return Ok(dashboard);
        }

        [HttpPut("profile")]
        [Authorize(Roles = "Vendor")]
        public async Task<ActionResult> UpdateVendorProfile(Vendor vendor)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var existingVendor = await _vendorRepository.GetVendorByUserIdAsync(userId);

            if (existingVendor == null)
            {
                return NotFound("Vendor profile not found.");
            }

            existingVendor.StoreName = vendor.StoreName;
            existingVendor.Description = vendor.Description;
            existingVendor.ContactEmail = vendor.ContactEmail;

            await _vendorRepository.UpdateVendorProfileAsync(existingVendor);

            return Ok(new
            {
                message = "Vendor profile updated successfully.",
                vendorId = existingVendor.Id
            });
        }
    }
}