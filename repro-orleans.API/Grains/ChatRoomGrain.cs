
using Orleans.Utilities;

namespace API;

public class ChatRoomGrain(ILogger<ChatRoomGrain> logger) : Grain, IChatRoomGrain
{
    private readonly ObserverManager<IChatRoomObserver> chatObservers = new (
        expiration: TimeSpan.FromMinutes(10),
        logger
    );

    List<(string user, string message)> messages = [];
    HashSet<string> users = [];

    public Task MessageAsync(string user, string message)
    {
        messages.Add((user, message));
        chatObservers.Notify(observer => observer.ReceiveMessageAsync(user, message));
        return Task.CompletedTask;
    }

    public async Task SubscribeAsync(string user, IChatRoomObserver observer)
    {
        // Send entire history to new observer
        foreach (var (otherUser, message) in messages)
        {
            await observer.ReceiveMessageAsync(otherUser, message);
        }

        chatObservers.Subscribe(observer, observer);
        users.Add(user);
    }

    public Task UnsubscribeAsync(string user, IChatRoomObserver observer)
    {
        chatObservers.Unsubscribe(observer);
        users.Remove(user);
        return Task.CompletedTask;
    }
}