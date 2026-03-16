namespace AutomatedTaskSystem.Services.Leave.BackgroundService
{
    public interface IRemoveOldAnnualLeaveService
    {
        Task RunRemoveOldAnnualLeavesAsync();
    }
}