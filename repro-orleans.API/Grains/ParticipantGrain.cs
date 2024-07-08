
namespace API;

public class ParticipanGrain : Grain, IParticipantGrain
{
    private IChatRoomGrain? chatRoom = null;

    private List<(string user, string message)> messages = [];

    public Task<List<(string user, string message)>> GetMessagesAsync() => Task.FromResult(messages);

    public async Task JoinAsync(string room)
    {
        messages.Clear();

        if (chatRoom is not null)
            await chatRoom.UnsubscribeAsync(this.GetPrimaryKeyString(), this.AsReference<IChatRoomObserver>());

        chatRoom = GrainFactory.GetGrain<IChatRoomGrain>(room);

        await chatRoom.SubscribeAsync(this.GetPrimaryKeyString(), this.AsReference<IChatRoomObserver>());
    }

    public Task LeaveAsync(string room)
    {
        return chatRoom?.UnsubscribeAsync(this.GetPrimaryKeyString(), this.AsReference<IChatRoomObserver>()) ??
            Task.CompletedTask;
    }

    public async Task ReceiveMessageAsync(string user, string message)
    {
        // This method is called, but never appears in traces

        messages.Add((user, message));

        // Request ip
        using var httpClient = new HttpClient();
        var ip = await httpClient.GetStringAsync("https://icanhazip.com");
    }

    public Task SayAsync(string message)
    {
        return chatRoom?.MessageAsync(this.GetPrimaryKeyString(), message) ?? Task.CompletedTask;
    }
}