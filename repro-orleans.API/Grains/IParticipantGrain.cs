namespace API;

public interface IParticipantGrain : IGrainWithStringKey, IChatRoomObserver
{
    Task JoinAsync(string room);

    Task LeaveAsync(string room);

    Task SayAsync(string message);

    Task<List<(string user, string message)>> GetMessagesAsync();
}