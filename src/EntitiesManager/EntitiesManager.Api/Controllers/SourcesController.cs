using EntitiesManager.Core.Entities;
using EntitiesManager.Core.Exceptions;
using EntitiesManager.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EntitiesManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SourcesController : ControllerBase
{
    private readonly ISourceEntityRepository _repository;
    private readonly ILogger<SourcesController> _logger;

    public SourcesController(
        ISourceEntityRepository repository,
        ILogger<SourcesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SourceEntity>>> GetAll()
    {
        try
        {
            var entities = await _repository.GetAllAsync();
            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all source entities");
            return StatusCode(500, "An error occurred while retrieving source entities");
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
            _logger.LogError(ex, "Error retrieving paged source entities");
            return StatusCode(500, "An error occurred while retrieving source entities");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SourceEntity>> GetById(Guid id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return NotFound($"Source with ID {id} not found");

            return Ok(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving source entity by ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the source entity");
        }
    }

    [HttpGet("by-key/{address}/{version}")]
    public async Task<ActionResult<SourceEntity>> GetByCompositeKey(string address, string version)
    {
        try
        {
            var compositeKey = $"{address}_{version}";
            var entity = await _repository.GetByCompositeKeyAsync(compositeKey);

            if (entity == null)
                return NotFound($"Source with address '{address}' and version '{version}' not found");

            return Ok(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving source entity by composite key {Address}_{Version}", address, version);
            return StatusCode(500, "An error occurred while retrieving the source entity");
        }
    }

    [HttpPost]
    public async Task<ActionResult<SourceEntity>> Create([FromBody] SourceEntity entity)
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
                _logger.LogError("MongoDB failed to generate ID for new SourceEntity");
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
            _logger.LogError(ex, "Error creating source entity");
            return StatusCode(500, "An error occurred while creating the source");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SourceEntity>> Update(Guid id, [FromBody] SourceEntity entity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != entity.Id)
            return BadRequest("ID in URL does not match ID in request body");

        try
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Source with ID {id} not found");

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
            return NotFound($"Source with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating source entity with ID {Id}", id);
            return StatusCode(500, "An error occurred while updating the source");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
                return NotFound($"Source with ID {id} not found");

            // Event publishing is now handled by the repository

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting source entity with ID {Id}", id);
            return StatusCode(500, "An error occurred while deleting the source");
        }
    }
}
