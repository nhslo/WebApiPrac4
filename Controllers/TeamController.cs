using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApiPrac4.DTOs;
using WebApiPrac4.Models;
using WebApiPrac4.Repositories;
using WebApiPrac4.Results;

namespace WebApiPrac4.Controllers;

[ApiController]
[Route("api/team")]
[Produces("application/json")]
public class TeamController(
    ITeamRepository repository,
    IMapper mapper,
    ILogger<TeamController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<IEnumerable<TeamDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReturnResult<IEnumerable<TeamDto>>>> GetAll()
    {
        logger.LogInformation("Request received: list all teams");
        var teams = await repository.GetAllAsync();
        return Ok(ReturnResult<IEnumerable<TeamDto>>.Success(mapper.Map<IEnumerable<TeamDto>>(teams)));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReturnResult<TeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReturnResult<TeamDto>>> Get(int id)
    {
        var team = await repository.GetAsync(id);
        if (team is null)
        {
            logger.LogWarning("Team with ID {TeamId} was not found", id);
            return NotFound(ReturnResult<object>.Failure($"Команда с ID {id} не найдена."));
        }

        logger.LogInformation("Team with ID {TeamId} retrieved", id);
        return Ok(ReturnResult<TeamDto>.Success(mapper.Map<TeamDto>(team)));
    }

    [HttpGet("city/{city}")]
    [ProducesResponseType(typeof(ReturnResult<IEnumerable<TeamDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReturnResult<IEnumerable<TeamDto>>>> GetByCity(string city)
    {
        logger.LogInformation("Request received: find teams in city {City}", city);
        var teams = await repository.GetByCityAsync(city.Trim());
        return Ok(ReturnResult<IEnumerable<TeamDto>>.Success(mapper.Map<IEnumerable<TeamDto>>(teams)));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReturnResult<TeamDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReturnResult<TeamDto>>> Create(TeamInputDto dto)
    {
        var team = mapper.Map<Team>(dto);
        await repository.CreateAsync(team);
        logger.LogInformation("Team with ID {TeamId} created", team.Id);
        var result = ReturnResult<TeamDto>.Success(mapper.Map<TeamDto>(team));
        return CreatedAtAction(nameof(Get), new { id = team.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReturnResult<TeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReturnResult<TeamDto>>> Update(int id, TeamInputDto dto)
    {
        var team = await repository.GetAsync(id);
        if (team is null)
        {
            logger.LogWarning("Team with ID {TeamId} was not found for update", id);
            return NotFound(ReturnResult<object>.Failure($"Команда с ID {id} не найдена."));
        }

        mapper.Map(dto, team);
        await repository.UpdateAsync(team);
        logger.LogInformation("Team with ID {TeamId} updated", id);
        return Ok(ReturnResult<TeamDto>.Success(mapper.Map<TeamDto>(team)));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReturnResult<object>>> Delete(int id)
    {
        if (await repository.GetAsync(id) is null)
        {
            logger.LogWarning("Team with ID {TeamId} was not found for deletion", id);
            return NotFound(ReturnResult<object>.Failure($"Команда с ID {id} не найдена."));
        }

        await repository.DeleteAsync(id);
        logger.LogInformation("Team with ID {TeamId} deleted", id);
        return Ok(ReturnResult<object>.Success(new { id }));
    }
}
