namespace AutomatedTaskSystem.Interfaces
{
	public interface IEncryptionService
	{
		string EncryptString(string content);

		string DecryptString(string encrypted);
	}
}