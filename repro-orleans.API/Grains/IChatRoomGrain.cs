namespace API;

public interface IChatRoomGrain : IGrainWithStringKey
{
    Task MessageAsync(string user, string message);

    Task SubscribeAsync(string user, IChatRoomObserver observer);

    Task UnsubscribeAsync(string user, IChatRoomObserver observer);
}

public interface IChatRoomObserver : IGrainObserver
{
    Task ReceiveMessageAsync(string user, string message);
}