namespace AutomatedTaskSystem.Services.Leave.BackgroundService
{
    public interface ILeaveResetService
    {
        System.Threading.Tasks.Task RunAnnualResetAsync();
    }
}
