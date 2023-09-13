namespace Cube.QuickSocket;

public record class MiddlewareFeature
{
    public DecoderMiddlewareDelegate DecoderDelegate { get; private set; }
    public EncoderMiddlewareDelegate EncoderDelegate { get; private set; }
    public KeyValuePair<Type, EncoderMiddlewareDelegate>[] SpecificEncoderDelegates { get; private set; }
    public IList<IMiddleware> Middlewares { get; private set; }

    public MiddlewareFeature(
        IList<IMiddleware> middlewares,
        DecoderMiddlewareDelegate decoderDelegate,
        EncoderMiddlewareDelegate encoderDelegate,
        KeyValuePair<Type, EncoderMiddlewareDelegate>[] specificEncoderDelegates)
    {
        Middlewares = middlewares;
        DecoderDelegate = decoderDelegate;
        EncoderDelegate = encoderDelegate;
        SpecificEncoderDelegates = specificEncoderDelegates;
    }
}