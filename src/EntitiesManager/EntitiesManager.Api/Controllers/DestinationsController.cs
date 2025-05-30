using EntitiesManager.Core.Entities;
using EntitiesManager.Core.Exceptions;
using EntitiesManager.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EntitiesManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DestinationsController : ControllerBase
{
    private readonly IDestinationEntityRepository _repository;
    private readonly ILogger<DestinationsController> _logger;

    public DestinationsController(
        IDestinationEntityRepository repository,
        ILogger<DestinationsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DestinationEntity>>> GetAll()
    {
        try
        {
            var entities = await _repository.GetAllAsync();
            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all destination entities");
            return StatusCode(500, "An error occurred while retrieving destination entities");
        }
    }

    [HttpGet("paged")]
    public async Task<ActionResult<object>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var entities = await _repository.GetPagedAsync(page, pageSize);
            var totalCount = await _repository.CountAsync();

            return Ok(new
            {
                Data = entities,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged destination entities");
            return StatusCode(500, "An error occurred while retrieving destination entities");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DestinationEntity>> GetById(Guid id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return NotFound($"Destination with ID {id} not found");

            return Ok(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving destination entity by ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the destination entity");
        }
    }

    [HttpGet("by-key/{name}/{version}")]
    public async Task<ActionResult<DestinationEntity>> GetByCompositeKey(string name, string version)
    {
        try
        {
            var compositeKey = $"{name}_{version}";
            var entity = await _repository.GetByCompositeKeyAsync(compositeKey);

            if (entity == null)
                return NotFound($"Destination with name '{name}' and version '{version}' not found");

            return Ok(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving destination entity by composite key {Name}_{Version}", name, version);
            return StatusCode(500, "An error occurred while retrieving the destination entity");
        }
    }

    [HttpPost]
    public async Task<ActionResult<DestinationEntity>> Create([FromBody] DestinationEntity entity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            entity.CreatedBy = User.Identity?.Name ?? "System";
            entity.Id = Guid.Empty; // Ensure MongoDB generates the ID

            var created = await _repository.CreateAsync(entity);

            if (created.Id == Guid.Empty)
            {
                _logger.LogError("MongoDB failed to generate ID for new DestinationEntity");
                return StatusCode(500, "Failed to generate entity ID");
            }

            // Event publishing is now handled by the repository

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (DuplicateKeyException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating destination entity");
            return StatusCode(500, "An error occurred while creating the destination");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DestinationEntity>> Update(Guid id, [FromBody] DestinationEntity entity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != entity.Id)
            return BadRequest("ID in URL does not match ID in request body");

        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Destination with ID {id} not found");

            // Preserve audit fields
            entity.CreatedAt = existing.CreatedAt;
            entity.CreatedBy = existing.CreatedBy;
            entity.UpdatedBy = User.Identity?.Name ?? "System";

            var updated = await _repository.UpdateAsync(entity);

            // Event publishing is now handled by the repository

            return Ok(updated);
        }
        catch (DuplicateKeyException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (EntityNotFoundException)
        {
            return NotFound($"Destination with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating destination entity with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the destination");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
                return NotFound($"Destination with ID {id} not found");

            // Event publishing is now handled by the repository

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting destination entity with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the destination");
        }
    }
}
