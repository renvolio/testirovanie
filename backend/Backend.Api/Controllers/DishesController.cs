using Backend.Api.Dtos.Dishes;
using Backend.Api.Services.Dishes;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DishesController : ControllerBase
{
    private readonly IDishService _dishes;

    public DishesController(IDishService dishes)
    {
        _dishes = dishes;
    }

    /// <summary>Список блюд с фильтрами по категории и флагам и поиском по названию (2.5).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DishDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DishDto>>> GetMany(
        [FromQuery] DishListQuery query,
        CancellationToken cancellationToken)
    {
        var items = await _dishes.QueryAsync(query, cancellationToken);
        return Ok(items);
    }

    /// <summary>Просмотр блюда с полным составом и атрибутами (2.6).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DishDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DishDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _dishes.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DishDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DishDto>> Create(
        [FromBody] CreateDishDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await _dishes.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DishDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DishDto>> Update(
        Guid id,
        [FromBody] UpdateDishDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _dishes.UpdateAsync(id, dto, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _dishes.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
