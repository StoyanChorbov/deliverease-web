using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO.Delivery;
using Repository.Context;

namespace Repository;

public class DeliveryRepository(DelivereaseDbContext context)
{
    private readonly DbSet<Delivery> _deliveries = context.Set<Delivery>();

    public async Task AddAsync(Delivery delivery)
    {
        await _deliveries.AddAsync(delivery);
        await context.SaveChangesAsync();
    }

    public async Task<Delivery> GetAsync(Guid id)
    {
        var delivery = await _deliveries
            .AsNoTrackingWithIdentityResolution()
            .Include(d => d.StartingLocation)
            .Include(d => d.EndingLocation)
            .Include(d => d.Sender)
            .Include(d => d.Deliverer)
            .Include(d => d.Recipients)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (delivery == null)
            throw new ArgumentException("Delivery not found");

        return delivery;
    }

    public async Task<List<Delivery>> GetAllAsync()
    {
        return await _deliveries
            .AsNoTrackingWithIdentityResolution()
            .Include(d => d.StartingLocation)
            .Include(d => d.EndingLocation)
            .Include(d => d.Sender)
            .Include(d => d.Deliverer)
            .Include(d => d.Recipients)
            .ToListAsync();
    }

    public async Task<List<Delivery>> GetAllByStartingAndEndingLocation(string startingLocationRegion,
        string endingLocationRegion, string username)
    {
        var deliveries = await _deliveries
            .Include(d => d.StartingLocation)
            .Include(d => d.EndingLocation)
            .Include(d => d.Sender)
            .Include(d => d.Recipients)
            .Where(d =>
                d.DelivererId == null &&
                d.Sender.UserName != username && d.Recipients.All(r => r.UserName != username) &&
                d.StartingLocationRegion == startingLocationRegion &&
                d.EndingLocationRegion == endingLocationRegion &&
                d.DeliveryDate == null
            )
            .Take(15)
            .ToListAsync();

        return deliveries;
    }

    public async Task<List<Delivery>> GetAllToDeliver(string username) =>
        await _deliveries
            .Include(d => d.StartingLocation)
            .Include(d => d.EndingLocation)
            .Where(d => d.Deliverer != null && d.Deliverer.UserName == username && d.DeliveryDate == null)
            .ToListAsync();

    public async Task<List<Delivery>> GetAllToReceive(string username) =>
        await _deliveries
            .Include(d => d.StartingLocation)
            .Include(d => d.EndingLocation)
            .Where(d => d.Recipients.Any(r => r.UserName == username) && d.DeliveryDate == null)
            .ToListAsync();

    public async Task UpdateAsync(Delivery delivery)
    {
        var currentDelivery = await _deliveries.FindAsync(delivery.Id);

        if (currentDelivery == null)
            throw new ArgumentException("Delivery not found");

        _deliveries.Update(delivery);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var delivery = await _deliveries.FindAsync(id);

        if (delivery == null)
            throw new ArgumentException("Delivery not found");

        _deliveries.Remove(delivery);
        await context.SaveChangesAsync();
    }

    public async Task<List<Delivery>> GetPastDeliveriesAsync(string username)
    {
        var deliveries = await _deliveries
            .AsNoTrackingWithIdentityResolution()
            .Include(d => d.StartingLocation)
            .Include(d => d.EndingLocation)
            .Include(d => d.Sender)
            .Include(d => d.Deliverer)
            .Include(d => d.Recipients)
            .Where(d => d.Sender.UserName == username)
            .ToListAsync();

        return deliveries;
    }

    public async Task SetDeliveryDelivererAsync(string deliveryId, User user)
    {
        var delivery = await _deliveries
            .Include(d => d.Deliverer)
            .FirstOrDefaultAsync(d => d.Id.ToString() == deliveryId);

        if (delivery == null)
            throw new ArgumentException("Delivery not found");

        delivery.Deliverer = user;
        await context.SaveChangesAsync();
    }

    public async Task MarkDeliveryAsCompleted(Guid deliveryId)
    {
        var delivery = await _deliveries
            .Include(d => d.Deliverer)
            .FirstOrDefaultAsync(d => d.Id == deliveryId);

        if (delivery == null)
            throw new ArgumentException("Delivery not found");

        delivery.DeliveryDate = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }
}