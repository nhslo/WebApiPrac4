using Microsoft.EntityFrameworkCore;
using WebApiPrac4.Data;
using WebApiPrac4.Models;

namespace WebApiPrac4.Repositories;

public class TeamRepository(AppDbContext context) : ITeamRepository
{
    public async Task<IEnumerable<Team>> GetAllAsync() =>
        await context.Teams.AsNoTracking().OrderBy(team => team.Id).ToListAsync();

    public Task<Team?> GetAsync(int id) =>
        context.Teams.AsNoTracking().FirstOrDefaultAsync(team => team.Id == id);

    public async Task<IEnumerable<Team>> GetByCityAsync(string city) =>
        await context.Teams.AsNoTracking()
            .Where(team => EF.Functions.Like(team.City, city))
            .OrderBy(team => team.Id)
            .ToListAsync();

    public async Task CreateAsync(Team team)
    {
        context.Teams.Add(team);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Team team)
    {
        context.Teams.Update(team);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var team = await context.Teams.FindAsync(id);
        if (team is null) return;
        context.Teams.Remove(team);
        await context.SaveChangesAsync();
    }
}
