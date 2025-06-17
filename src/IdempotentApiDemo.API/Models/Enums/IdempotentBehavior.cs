namespace IdempotentApiDemo.API.Models.Enums;

public enum IdempotentBehavior
{
    ReturnFromCache,
    ThrowErrorIfExists
}