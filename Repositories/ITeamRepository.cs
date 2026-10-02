using WebApiPrac4.Models;

namespace WebApiPrac4.Repositories;

public interface ITeamRepository
{
    Task<IEnumerable<Team>> GetAllAsync();
    Task<Team?> GetAsync(int id);
    Task<IEnumerable<Team>> GetByCityAsync(string city);
    Task CreateAsync(Team team);
    Task UpdateAsync(Team team);
    Task DeleteAsync(int id);
}
