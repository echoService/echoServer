using System.Net.Sockets;

namespace Server;

public class Dispatcher
{
    private readonly IMessageHandler _chatMessageHandler;
    private readonly IMessageHandler _createMessageHandler;
    private readonly IMessageHandler _joinMessageHandler;
    private readonly IMessageHandler _inquiryMessageHandler;
    delegate object MyDelegateResponse(MemoryStream stream);
    delegate void MyDelegateRequest(object request, Socket socket);

    public Dispatcher(IMessageHandler chatMessageHandler, IMessageHandler createMessageHandler, IMessageHandler joinMessageHandler, IMessageHandler inquiryMessageHandler)
    {
        _chatMessageHandler = chatMessageHandler;
        _createMessageHandler = createMessageHandler;
        _joinMessageHandler = joinMessageHandler;
        _inquiryMessageHandler = inquiryMessageHandler;
    }
    
    public void DispatchResponse(object request, Command command, Socket socket)
    {
        switch (command)
        {
            case Command.Chat:
                MyDelegateRequest chatDelegate = _chatMessageHandler.HandleResponse;
                chatDelegate(request, socket);
                break;
            
            case Command.Create:
                MyDelegateRequest createDelegate = _createMessageHandler.HandleResponse;
                createDelegate(request, socket);
                break;
            case Command.Join:
                MyDelegateRequest joinDelegate = _joinMessageHandler.HandleResponse;
                joinDelegate(request, socket);
                break;
            case Command.Inquiry:
                MyDelegateRequest inquiryDelegate = _inquiryMessageHandler.HandleResponse;
                inquiryDelegate(request, socket);
                break;
        }
    }
    
    public object DispatchRequest(Command command, MemoryStream stream)
    {
        switch (command)
        {
            case Command.Chat:
                MyDelegateResponse chatDelegate = _chatMessageHandler.HandleRequest;
                return chatDelegate(stream);
            
            case Command.Create:
                MyDelegateResponse createDelegate = _createMessageHandler.HandleRequest;
                return createDelegate(stream);
            
            
            case Command.Join:
                MyDelegateResponse joinDelegate = _joinMessageHandler.HandleRequest;
                return joinDelegate(stream);
            
            case Command.Inquiry:
                MyDelegateResponse inquiryDelegate = _inquiryMessageHandler.HandleRequest;
                return inquiryDelegate(stream);
        }

        return null;
    }
}