namespace Chinook.Domain.Converters;

public interface IConvertModel<out TTarget>
{
    TTarget Convert();
}