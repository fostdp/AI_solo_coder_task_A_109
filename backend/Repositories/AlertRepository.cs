using Microsoft.EntityFrameworkCore;
using SculptureMonitor.Data;
using SculptureMonitor.Models;

namespace SculptureMonitor.Repositories
{
    public class AlertRepository : IAlertRepository
    {
        private readonly AppDbContext _context;

        public AlertRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Alert>> GetAllAsync()
        {
            return await _context.Alerts
                .Include(a => a.Sculpture)
                .Include(a => a.Sensor)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Alert> GetByIdAsync(string id)
        {
            return await _context.Alerts
                .Include(a => a.Sculpture)
                .Include(a => a.Sensor)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Alert>> GetBySculptureIdAsync(int sculptureId, int limit = 50)
        {
            return await _context.Alerts
                .Where(a => a.SculptureId == sculptureId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Alert>> GetBySeverityAsync(string severity, int limit = 50)
        {
            return await _context.Alerts
                .Where(a => a.Severity == severity)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Alert>> GetByStatusAsync(string status, int limit = 50)
        {
            return await _context.Alerts
                .Where(a => a.Status == status)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Alert>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            return await _context.Alerts
                .Where(a => a.CreatedAt >= startTime && a.CreatedAt <= endTime)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Alert> CreateAsync(Alert alert)
        {
            _context.Alerts.Add(alert);
            await _context.SaveChangesAsync();
            return alert;
        }

        public async Task<Alert> UpdateAsync(Alert alert)
        {
            _context.Entry(alert).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return alert;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null) return false;
            
            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Alerts.CountAsync();
        }

        public async Task<int> GetCountByStatusAsync(string status)
        {
            return await _context.Alerts.CountAsync(a => a.Status == status);
        }

        public async Task<int> GetCountBySeverityAsync(string severity)
        {
            return await _context.Alerts.CountAsync(a => a.Severity == severity);
        }

        public async Task<bool> ExistsRecentAlertAsync(int sculptureId, string alertType, int minutes = 60)
        {
            var cutoff = DateTime.Now.AddMinutes(-minutes);
            return await _context.Alerts.AnyAsync(a => 
                a.SculptureId == sculptureId && 
                a.AlertType == alertType && 
                a.CreatedAt >= cutoff);
        }
    }
}
