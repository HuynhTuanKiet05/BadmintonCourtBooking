namespace BadmintonCourtBooking.Data.Entities;

public class UserSnapshotEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public string RoleLabel { get; set; } = string.Empty;
}
